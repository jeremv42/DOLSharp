using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS.Quests
{
	public class EndGoal : DataQuestJsonGoal
	{
		public override eQuestGoalType Type => eQuestGoalType.Unknown;
		public override int ProgressTotal => 1;

		public GameNPC Target { get; }

		public override bool CanInteractWith(PlayerQuest questData, PlayerGoalState state, GameObject target)
			=> state?.IsActive == true && target.Name == Target.Name && target.CurrentRegion == Target.CurrentRegion;

		public EndGoal(DataQuestJson quest, int goalId, dynamic db) : base(quest, goalId, (object)db)
		{
			Target = WorldMgr.GetNPCsByNameFromRegion((string)db.TargetName ?? "", (ushort)(db.TargetRegion ?? 0), eRealm.None)
				.FirstOrDefault(quest.Npc);
		}

		public override Dictionary<string, object> GetDatabaseJsonObject()
		{
			var dict = base.GetDatabaseJsonObject();
			dict.Add("TargetName", Target.Name);
			dict.Add("TargetRegion", Target.CurrentRegionID);
			return dict;
		}

		public override void NotifyActive(PlayerQuest quest, PlayerGoalState goal, DOLEvent e, object sender, EventArgs args)
		{
			var player = quest.Owner;
			
			// interact with the final NPC
			if (e == GameObjectEvent.InteractWith && args is InteractWithEventArgs interact && interact.Target.Name == Target.Name && interact.Target.CurrentRegion == Target.CurrentRegion)
				player.Out.SendQuestRewardWindow(interact.Target as GameNPC, player, quest);

			// receive the quest window response
			if (e == GamePlayerEvent.QuestRewardChosen && args is QuestRewardChosenEventArgs rewardArgs && rewardArgs.QuestID == quest.QuestId)
			{
				if (quest.Quest.NbChooseOptionalItems != rewardArgs.CountChosen && quest.Quest.OptionalRewardItemTemplates.Count >= quest.Quest.NbChooseOptionalItems)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "RewardQuest.Notify"), eChatType.CT_System, eChatLoc.CL_ChatWindow);
					return;
				}

				var items = quest.Quest.OptionalRewardItemTemplates.Where((item, idx) => rewardArgs.ItemsChosen.Contains(idx + 1)).ToList();
				quest.Quest.FinishQuest(quest, items);
			}
		}

		public override PlayerGoalState ForceStartGoal(PlayerQuest questData)
		{
			var res = base.ForceStartGoal(questData);
			if (res.IsActive && GameMath.IsWithinRadius(questData.Owner, Target, WorldMgr.OBJ_UPDATE_DISTANCE))
				questData.Owner.Out.SendNPCsQuestEffect(Target, Target.GetQuestIndicator(questData.Owner));
			return res;
		}
	}
}
