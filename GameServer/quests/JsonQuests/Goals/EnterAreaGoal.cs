using DOL.Events;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DOL.GS.Quests
{
	public class EnterAreaGoal : DataQuestJsonGoal
	{
		private readonly Area.Circle m_area;
		private readonly ushort m_areaRegion;

		public override eQuestGoalType Type => eQuestGoalType.Unknown;
		public override int ProgressTotal => 1;
		public override QuestZonePoint PointA { get; }

		public EnterAreaGoal(DataQuestJson quest, int goalId, dynamic db) : base(quest, goalId, (object)db)
		{
			m_area = new Area.Circle($"{quest.Name} EnterAreaGoal {goalId}", new Vector3((float)db.AreaCenter.X, (float)db.AreaCenter.Y, (float)db.AreaCenter.Z), (int)db.AreaRadius);
			m_area.DisplayMessage = !false;
			m_areaRegion = db.AreaRegion;

			var reg = WorldMgr.GetRegion(m_areaRegion);
			reg.AddArea(m_area);
			PointA = new QuestZonePoint(reg.GetZone(m_area.Position), m_area.Position);
		}

		public override Dictionary<string, object> GetDatabaseJsonObject()
		{
			var dict = base.GetDatabaseJsonObject();
			dict.Add("AreaCenter", m_area.Position);
			dict.Add("AreaRadius", m_area.Radius);
			dict.Add("AreaRegion", m_areaRegion);
			return dict;
		}

		private void OnPlayerEnterArea(PlayerQuest quest, PlayerGoalState goal)
		{
			AdvanceGoal(quest, goal);
		}
		private void OnPlayerLeaveArea(PlayerQuest quest, PlayerGoalState goal)
		{
			goal.Progress = 0;
			goal.State = eQuestGoalStatus.Active;
			quest.SaveIntoDatabase();
			quest.Owner.Out.SendQuestUpdate(quest);
		}

		public override void NotifyActive(PlayerQuest quest, PlayerGoalState goal, DOLEvent e, object sender, EventArgs args)
		{
			if ((sender as AbstractArea)?.ID != m_area.ID || args is not AreaEventArgs arguments || arguments.GameObject != quest.Owner)
				return;
			if (e == AreaEvent.PlayerEnter)
				OnPlayerEnterArea(quest, goal);
			if (e == AreaEvent.PlayerLeave)
				OnPlayerLeaveArea(quest, goal);
		}

		public override void Unload()
		{
			WorldMgr.GetRegion(m_areaRegion)?.RemoveArea(m_area);
			base.Unload();
		}
	}
}
