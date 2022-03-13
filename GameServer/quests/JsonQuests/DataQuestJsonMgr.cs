using DOL.Database;
using DOL.Events;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DOL.GS.Quests;

public static class DataQuestJsonMgr
{
	private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	public static Dictionary<ushort, DataQuestJson> Quests = new();

	[ScriptLoadedEvent]
	public static void OnScriptsCompiled(DOLEvent e, object sender, EventArgs args)
	{
		GameServer.Database.RegisterDataObject(typeof(DBDataQuestJson));
		log.Info("QuestLoader: initialized.");
	}

	[GameServerStartedEvent]
	public static void OnGameServerStarted(DOLEvent e, object sender, EventArgs args)
	{
		ReloadQuests();
		GameEventMgr.AddHandlerUnique(GameObjectEvent.Interact, OnInteract);
		GameEventMgr.AddHandlerUnique(GamePlayerEvent.AcceptQuest, OnAcceptQuest);
	}

	public static DataQuestJson GetQuest(ushort id)
	{
		return Quests.TryGetValue(id, out var quest) ? quest : null;
	}

	public static List<string> ReloadQuests()
	{
		var old = Quests;
		foreach (var quest in old.Values)
			quest.Unload();

		var errors = new List<string>();
		var quests = new Dictionary<ushort, DataQuestJson>();
		foreach (var db in GameServer.Database.SelectAllObjects<DBDataQuestJson>())
		{
			try
			{
				var loaded = new DataQuestJson(db);
				quests.Add(loaded.Id, loaded);
			}
			catch (Exception ex)
			{
				errors.Add($"Error with quest \"{db.Name}\" (ID: {db.Id}): {ex.Message}");
				log.Error($"QuestLoader: error when loading quest {db.Id}", ex);
			}
		}

		// just exchange the reference
		Quests = quests;

		log.Info($"QuestLoader: {old.Count} quests unloaded, {Quests.Count} quests loaded");
		errors.Add($"QuestLoader: {old.Count} quests unloaded, {Quests.Count} quests loaded");
		return errors;
	}

	public static void OnInteract(DOLEvent _, object sender, EventArgs args)
	{
		var arguments = args as InteractEventArgs;
		if (arguments == null || arguments.Source == null || !(sender is GameNPC))
			return;

		var player = arguments.Source;
		var possibleQuests = Quests.Values.Where(q => q.Npc == sender).ToList();
		if (possibleQuests.Count == 0)
			return;

		lock (player.QuestList)
			if (player.QuestList.OfType<PlayerQuest>().Any(q => possibleQuests.Any(pq => pq.Id == q.QuestId)))
				return; // Quest in progress
		foreach (var quest in possibleQuests)
		{
			if (quest.CheckQuestQualification(player))
			{
				player.Out.SendQuestOfferWindow(quest.Npc, player, PlayerQuest.CreateQuestPreview(quest, player));
				return;
			}
		}
	}

	public static void OnAcceptQuest(DOLEvent _, object sender, EventArgs args)
	{
		var arguments = args as QuestEventArgs;
		if (arguments == null || arguments.Source == null)
			return;
		var player = arguments.Player;
		var quest = Quests.Values.FirstOrDefault(q => q.Id == arguments.QuestID);
		if (quest == null || arguments.Source != quest.Npc || !quest.CheckQuestQualification(player))
			return;
		var npc = quest.Npc;

		var dbQuest = new DBQuest
		{
			Character_ID = player.InternalID,
			Name = typeof(PlayerQuest).FullName,
			Step = 1,
			CustomPropertiesString = JsonConvert.SerializeObject(new PlayerQuest.JsonState { QuestId = quest.Id, Goals = null }),
		};
		var dq = new PlayerQuest(player, dbQuest);
		player.Out.SendSoundEffect(7, 0, 0, 0, 0, 0);
		ChatUtil.SendScreenCenter(player, $"Quest \"{quest.Name}\" accepted!");
		if (player.AddQuest(dq))
		{
			dq.SaveIntoDatabase();
			player.Out.SendNPCsQuestEffect(npc, npc.GetQuestIndicator(player));
			player.Out.SendQuestListUpdate();
		}
	}

	public static (PlayerQuest quest, PlayerGoalState goal) FindQuestAndGoalFromPlayer(GamePlayer player, ushort questId, int goalId)
	{
		var quest = player.QuestList.Find(q => q.QuestId == questId);
		var goal = quest?.GoalStates.Find(gs => gs.GoalId == goalId);
		return (quest, goal);
	}
}