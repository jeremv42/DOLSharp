using DOL.Database;
using DOL.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DOL.GS.Behaviour;

namespace DOL.GS.Quests
{
	public abstract class DataQuestJsonGoal
	{
		public readonly ushort QuestId;
		public DataQuestJson Quest => DataQuestJsonMgr.GetQuest(QuestId);
		public readonly int GoalId;

		public string Description { get; set; }
		public abstract eQuestGoalType Type { get; }
		public abstract int ProgressTotal { get; }
		public virtual QuestZonePoint PointA => QuestZonePoint.None;
		public virtual QuestZonePoint PointB => QuestZonePoint.None;
		public virtual ItemTemplate QuestItem => null;
		public virtual bool Visible => true;
		public ItemTemplate GiveItemTemplate { get; set; }

		public string MessageStarted { get; set; }
		public string MessageAborted { get; set; }
		public string MessageDone { get; set; }
		public string MessageCompleted { get; set; }

		public List<int> StartGoalsDone { get; set; } = new();
		public List<int> EndWhenGoalsDone { get; set; } = new();

		public DataQuestJsonGoal(DataQuestJson quest, int goalId, dynamic db)
		{
			QuestId = quest.Id;
			GoalId = goalId;
			Description = db.Description;
			MessageStarted = db.MessageStarted ?? "";
			MessageAborted = db.MessageAborted ?? "";
			MessageDone = db.MessageDone ?? "";
			MessageCompleted = db.MessageCompleted ?? "";
			string item = db.GiveItem ?? "";
			if (!string.IsNullOrWhiteSpace(item))
				GiveItemTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(item);
			if (db.StartGoalsDone != null)
				foreach (var id in db.StartGoalsDone)
					StartGoalsDone.Add((int)id);
			if (db.EndWhenGoalsDone != null)
				foreach (var id in db.EndWhenGoalsDone)
					EndWhenGoalsDone.Add((int)id);
		}

		public bool IsActive(PlayerQuest questData) => questData.GoalStates.Any(gs => gs.GoalId == GoalId && gs.IsActive);
		public bool IsDone(PlayerQuest questData) => questData.GoalStates.Any(gs => gs.GoalId == GoalId && gs.IsDone);
		public bool IsFinished(PlayerQuest questData) => questData.GoalStates.Any(gs => gs.GoalId == GoalId && gs.IsFinished);

		public void NotifyActive(PlayerQuest quest, DOLEvent e, object sender, EventArgs args)
		{
			var goalData = quest.GoalStates.Find(gs => gs.GoalId == GoalId);
			NotifyActive(quest, goalData, e, sender, args);
		}
		public abstract void NotifyActive(PlayerQuest quest, PlayerGoalState goal, DOLEvent e, object sender, EventArgs args);

		// this one is always called, useful if you want to start a goal with some hidden task
		public void Notify(PlayerQuest questData, DOLEvent e, object sender, EventArgs args)
		{
			var goalData = questData.GoalStates.Find(gs => gs.GoalId == GoalId);
			Notify(questData, goalData, e, sender, args);
		}
		// this one is always called, useful if you want to start a goal with some hidden task
		public virtual void Notify(PlayerQuest questData, PlayerGoalState goalData, DOLEvent e, object sender, EventArgs args) {}

		public virtual bool CanStart(PlayerQuest questData)
		{
			if (IsActive(questData) || IsFinished(questData))
				return false;
			return StartGoalsDone.All(gId => questData.GoalStates.Any(gs => gs.GoalId == gId && gs.IsDone));
		}

		public virtual bool CanComplete(PlayerQuest questData)
		{
			var gs = questData.GoalStates.Find(s => s.GoalId == GoalId);
			return gs?.State == eQuestGoalStatus.DoneAndActive && EndWhenGoalsDone.All(id => questData.GoalStates.Any(s => s.GoalId == id && s.IsDone));
		}

		public virtual bool CanInteractWith(PlayerQuest questData, PlayerGoalState state, GameObject target) => false;

		public PlayerGoalState StartGoal(PlayerQuest questData)
		{
			if (CanStart(questData))
				return ForceStartGoal(questData);
			return null;
		}

		public virtual PlayerGoalState ForceStartGoal(PlayerQuest questData)
		{
			var goalData = new PlayerGoalState
			{
				GoalId = GoalId,
				State = eQuestGoalStatus.Active,
			};
			questData.GoalStates.Add(goalData);
			var player = questData.Owner;
			if (Visible)
			{
				player.Out.SendQuestUpdate(questData);
				ChatUtil.SendScreenCenter(player, $"{Description} - {goalData.Progress}/{ProgressTotal}");
			}
			if (!string.IsNullOrWhiteSpace(MessageStarted))
				ChatUtil.SendImportant(player, $"[Quest {Quest.Name}] " + BehaviourUtils.GetPersonalizedMessage(MessageStarted, player));
			return goalData;
		}

		public virtual void AdvanceGoal(PlayerQuest questData, PlayerGoalState goalData)
		{
			goalData.Progress += 1;
			if (goalData.Progress >= ProgressTotal)
			{
				EndGoal(questData, goalData);
				return;
			}
			questData.SaveIntoDatabase();
			if (Visible)
			{
				questData.Owner.Out.SendQuestUpdate(questData);
				ChatUtil.SendScreenCenter(questData.Owner, $"{Description} - {goalData.Progress}/{ProgressTotal}");
			}
		}

		public void AbortGoal(PlayerQuest questData)
		{
			var goalState = questData.GoalStates.Find(gs => gs.GoalId == GoalId);
			if (goalState == null)
			{
				goalState = new PlayerGoalState
				{
					GoalId = GoalId,
					Progress = 0,
					State = eQuestGoalStatus.Aborted,
				};
				questData.GoalStates.Add(goalState);
			}
			else if (!goalState.IsFinished)
				goalState.State = eQuestGoalStatus.Aborted;

			var player = questData.Owner;
			if (goalState.State == eQuestGoalStatus.Aborted && !string.IsNullOrWhiteSpace(MessageAborted))
				ChatUtil.SendImportant(player, $"[Quest {Quest.Name}] " + BehaviourUtils.GetPersonalizedMessage(MessageAborted, player));
		}

		public void EndGoal(PlayerQuest questData, PlayerGoalState goalData)
		{
			EndGoal(questData, goalData, null);
			questData.SaveIntoDatabase();
			questData.Owner.Out.SendQuestUpdate(questData);
		}

		/// <summary>Recursive call</summary>
		private void EndGoal(PlayerQuest questData, PlayerGoalState goalData, List<DataQuestJsonGoal> except)
		{
			goalData.Progress = ProgressTotal;
			goalData.State = eQuestGoalStatus.DoneAndActive;

			var player = questData.Owner;
			if (Visible)
				ChatUtil.SendScreenCenter(player, $"{Description} - {goalData.Progress}/{ProgressTotal}");
			if (!string.IsNullOrWhiteSpace(MessageDone))
				ChatUtil.SendImportant(player, $"[Quest {Quest.Name}] " + BehaviourUtils.GetPersonalizedMessage(MessageDone, player));
			EndOtherGoals(questData, except ?? new List<DataQuestJsonGoal>());

			CompleteGoal(questData, goalData);
		}

		private void EndOtherGoals(PlayerQuest questData, List<DataQuestJsonGoal> except)
		{
			except.Add(this);
			foreach (var goal in Quest.Goals.Values)
				if (!except.Contains(goal) && goal.CanComplete(questData))
					goal.EndGoal(questData, questData.GoalStates.Find(s => s.GoalId == goal.GoalId), except);
		}

		private void CompleteGoal(PlayerQuest questData, PlayerGoalState goalData)
		{
			// try starting new goals
			foreach (var goal in Quest.Goals.Values)
				goal.StartGoal(questData);

			if (!CanComplete(questData))
				return;

			var player = questData.Owner;
			if (GiveItemTemplate != null)
				GiveItem(player, GiveItemTemplate);
			goalData.State = eQuestGoalStatus.Completed;
			if (!string.IsNullOrWhiteSpace(MessageCompleted))
				ChatUtil.SendImportant(player, $"[Quest {Quest.Name}] " + BehaviourUtils.GetPersonalizedMessage(MessageCompleted, player));
		}

		public virtual IQuestGoal ToQuestGoal(PlayerQuest questData, PlayerGoalState goalData)
			=> new GenericDataQuestGoal(this, goalData?.Progress ?? 0, goalData?.State ?? eQuestGoalStatus.NotStarted);

		/// <summary>
		/// Returns the object to be saved as JSON given back as third argument in the constructor for loading
		/// </summary>
		/// <returns>A serialisable object</returns>
		public virtual Dictionary<string, object> GetDatabaseJsonObject()
		{
			return new Dictionary<string, object>
			{
				{ "Description", Description },
				{ "GiveItem", GiveItemTemplate?.Id_nb },
				{ "MessageStarted", MessageStarted },
				{ "MessageAborted", MessageAborted },
				{ "MessageDone", MessageDone },
				{ "MessageCompleted", MessageCompleted },
				{ "StartGoalsDone", StartGoalsDone.Count > 0 ? StartGoalsDone : null },
				{ "EndWhenGoalsDone", EndWhenGoalsDone.Count > 0 ? EndWhenGoalsDone : null },
			};
		}

		public virtual void Unload()
		{
			// nothing to do but we need to remove some handler sometimes
		}

		protected static void GiveItem(GamePlayer player, ItemTemplate itemTemplate)
		{
			var item = GameInventoryItem.Create(itemTemplate);
			if (!player.ReceiveItem(null, item))
			{
				player.CreateItemOnTheGround(item);
				ChatUtil.SendImportant(player, $"Your backpack is full, {itemTemplate.Name} is dropped on the ground.");
			}
		}

		public class GenericDataQuestGoal : IQuestGoal
		{
			public string Description => Goal.Description;
			public eQuestGoalType Type => Goal.Type;
			public int Progress { get; set; }
			public int ProgressTotal => Goal.ProgressTotal;
			public QuestZonePoint PointA => Goal.PointA;
			public QuestZonePoint PointB => Goal.PointB;
			public eQuestGoalStatus Status { get; set; }
			public ItemTemplate QuestItem => Goal.QuestItem;

			public readonly DataQuestJsonGoal Goal;

			public GenericDataQuestGoal(DataQuestJsonGoal goal, int progress, eQuestGoalStatus status)
			{
				Progress = progress;
				Status = status;
				Goal = goal;
			}
		}
	}
}
