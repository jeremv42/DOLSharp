using System;
using System.Collections.Generic;
using System.Linq;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS;
using DOL.GS.Quests;
using DOL.GS.Scripts;

public class AmteMob : GameNPC, IAmteNPC
{
	private readonly Dictionary<string, DBBrainsParam> _nameXcp = new Dictionary<string, DBBrainsParam>();

	private readonly AmteCustomParam _linkParam;

	public AmteMob()
	{
		SetOwnBrain(new AmteMobBrain(Brain));
		_linkParam = new AmteCustomParam(
			"link",
			() => ((AmteMobBrain) Brain).AggroLink.ToString(),
			v => ((AmteMobBrain) Brain).AggroLink = int.Parse(v),
			"-1");
	}

	public AmteMob(INpcTemplate npc)
		: base(npc)
	{
		SetOwnBrain(new AmteMobBrain(Brain));
		_linkParam = new AmteCustomParam(
			"link",
			() => ((AmteMobBrain) Brain).AggroLink.ToString(),
			v => ((AmteMobBrain) Brain).AggroLink = int.Parse(v),
			"-1");
	}

	public override void LoadFromDatabase(DataObject obj)
	{
		base.LoadFromDatabase(obj);

		DBBrainsParam[] data;
		if (!DBBrainsParam.MobXDBBrains.TryGetValue(obj.ObjectId, out data))
			data = Array.Empty<DBBrainsParam>();
		for (var cp = GetCustomParam(); cp != null; cp = cp.next)
		{
			var cp1 = cp;
			var param = data.FirstOrDefault(o => o.Param == cp1.name);
			if (param == null)
				continue;
			if (_nameXcp.ContainsKey(cp.name))
			{
				GameServer.Database.DeleteObject(param);
				continue;
			}
			try
			{
				cp.Value = param.Value;
			}
			catch (Exception)
			{
			}
			_nameXcp.Add(cp.name, param);
		}
	}

	public override void SaveIntoDatabase()
	{
		base.SaveIntoDatabase();

		DBBrainsParam param;
		for (var cp = GetCustomParam(); cp != null; cp = cp.next)
			if (_nameXcp.TryGetValue(cp.name, out param))
			{
				param.Value = cp.Value;
				GameServer.Database.SaveObject(param);
				
			}
			else if (cp.defaultValue != cp.Value)
			{
				param = new DBBrainsParam
				{
					MobID = InternalID,
					Param = cp.name,
					Value = cp.Value,
				};
				_nameXcp.Add(cp.name, param);
				GameServer.Database.AddObject(param);
			}
	}

	public override void DeleteFromDatabase()
	{
		base.DeleteFromDatabase();
		_nameXcp.Values.Foreach(o => GameServer.Database.DeleteObject(o));
	}

	public virtual AmteCustomParam GetCustomParam()
	{
		return _linkParam;
	}

	public virtual IList<string> DelveInfo()
	{
		var list = new List<string>();
		for (var cp = GetCustomParam(); cp != null; cp = cp.next)
			list.Add(" - " + cp.name + ": " + cp.Value);
		return list;
	}

	public override eQuestIndicator GetQuestIndicator(GamePlayer player)
	{
		var res = base.GetQuestIndicator(player);
		if (res != eQuestIndicator.None)
			return res;

		foreach (var q in QuestIdListToGive.OfType<PlayerQuest>())
		{
			var quest = player.QuestList.OfType<PlayerQuest>().FirstOrDefault(pq => pq.QuestId == q.QuestId);
			if (quest == null)
				continue;
			if (quest.VisibleGoals.OfType<DataQuestJsonGoal.GenericDataQuestGoal>().Any(g => g.Goal is EndGoal end && end.Target == this))
				return eQuestIndicator.Finish;
		}

		return eQuestIndicator.None;
	}
}
