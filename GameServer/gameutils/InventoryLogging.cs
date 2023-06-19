using System;
using System.Collections.Generic;
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Type of trade
	/// </summary>
	public enum eInventoryActionType
	{
		/// <summary>
		/// Trade between 2 players
		/// </summary>
		Trade,
		/// <summary>
		/// A player pick up a loot
		/// </summary>
		Loot,
		/// <summary>
		/// Gain of a quest or quest's items
		/// </summary>
		Quest,
		/// <summary>
		/// Buy/sell an item
		/// </summary>
		Merchant,
		/// <summary>
		/// Crafting an item
		/// </summary>
		Craft,
		/// <summary>
		/// Any other action
		/// </summary>
		Other,
	}

	public static class InventoryLogging
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly Dictionary<eInventoryActionType, string> ActionXformat =
			new Dictionary<eInventoryActionType, string>
                {
                    {eInventoryActionType.Trade, "[TRADE] {0} > {1}: {2}"},
                    {eInventoryActionType.Loot, "[LOOT] {0} > {1}: {2}"},
                    {eInventoryActionType.Quest, "[QUEST] {0} > {1}: {2}"},
                    {eInventoryActionType.Merchant, "[MERCHANT] {0} > {1}: {2}"},
                    {eInventoryActionType.Craft, "[CRAFT] {0} > {1}: {2}"},
                    {eInventoryActionType.Other, "[OTHER] {0} > {1}: {2}"}
                };

		public static Func<GameObject, string> GetGameObjectString = obj =>
			obj == null ? null : ($"({obj.Name};{obj.GetType()};{obj.Position.X:F0};{obj.Position.Y:F0};{obj.Position.Z:F0};{obj.CurrentRegionID})");

		public static Func<ItemTemplate, int, string> GetItemTemplateString = (item, count) =>
			item == null ? null : ("(" + count + ";" + item.Name + ";" + item.Id_nb + ")");
		public static Func<InventoryItem, int, string> GetInventoryItemString = (item, count) =>
			item == null ? null : ("(" + count + ";" + item.Name + ";" + item.ITemplate_Id + ";" + item.UTemplate_Id + ")");

		public static Func<long, string> GetMoneyString = amount =>
			"(MONEY;" + amount + ")";

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(GameObject source, GameObject destination, eInventoryActionType type, ItemTemplate item, int count = 1)
		{
			LogInventoryAction(source?.InternalID, GetGameObjectString(source), destination?.InternalID, GetGameObjectString(destination), type, item, count);
		}
		public static void LogInventoryAction(GameObject source, GameObject destination, eInventoryActionType type, InventoryItem item, int count = 1)
		{
			LogInventoryAction(source?.InternalID, GetGameObjectString(source), destination?.InternalID, GetGameObjectString(destination), type, item, count);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(string sourceId, string source, GameObject destination, eInventoryActionType type, ItemTemplate item, int count = 1)
		{
			LogInventoryAction(sourceId, source, destination?.InternalID, GetGameObjectString(destination), type, item, count);
		}
		public static void LogInventoryAction(string sourceId, string source, GameObject destination, eInventoryActionType type, InventoryItem item, int count = 1)
		{
			LogInventoryAction(sourceId, source, destination?.InternalID, GetGameObjectString(destination), type, item, count);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(GameObject source, string destinationId, string destination, eInventoryActionType type, ItemTemplate item, int count = 1)
		{
			LogInventoryAction(source?.InternalID, GetGameObjectString(source), destinationId, destination, type, item, count);
		}
		public static void LogInventoryAction(GameObject source, string destinationId, string destination, eInventoryActionType type, InventoryItem item, int count = 1)
		{
			LogInventoryAction(source?.InternalID, GetGameObjectString(source), destinationId, destination, type, item, count);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(string sourceId, string source, string destinationId, string destination, eInventoryActionType type, ItemTemplate item, int count = 1)
		{
			// Check if you can log this action
			if (!_IsLoggingEnabled(type))
				return;

			string format;
			if (!ActionXformat.TryGetValue(type, out format))
				return; // Error, this format does not exists ?!

			try
			{
				GameServer.Instance.LogInventoryAction(string.Format(format, source ?? "(null)", destination ?? "(null)", GetItemTemplateString(item, count)));
				GameServer.Database.AddObject(new InventoryLog
				{
					Source = source,
					SourceId = sourceId,
					Destination = destination,
					DestinationId = destinationId,
					ItemTemplate = item.Id_nb,
					ItemUnique = null,
					Money = 0,
					ItemCount = count,
				});
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Log inventory error", e);
			}
		}
		public static void LogInventoryAction(string sourceId, string source, string destinationId, string destination, eInventoryActionType type, InventoryItem item, int count = 1)
		{
			// Check if you can log this action
			if (!_IsLoggingEnabled(type))
				return;

			string format;
			if (!ActionXformat.TryGetValue(type, out format))
				return; // Error, this format does not exists ?!

			try
			{
				GameServer.Instance.LogInventoryAction(string.Format(format, source ?? "(null)", destination ?? "(null)", GetInventoryItemString(item, count)));
				GameServer.Database.AddObject(new InventoryLog
				{
					Source = source,
					SourceId = sourceId,
					Destination = destination,
					DestinationId = destinationId,
					ItemTemplate = item.ITemplate_Id,
					ItemUnique = item.UTemplate_Id,
					Money = 0,
					ItemCount = count,
				});
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Log inventory error", e);
			}
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(GameObject source, GameObject destination, eInventoryActionType type, long money)
		{
			LogInventoryAction(source?.InternalID, GetGameObjectString(source), destination?.InternalID, GetGameObjectString(destination), type, money);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(string sourceId, string source, GameObject destination, eInventoryActionType type, long money)
		{
			LogInventoryAction(sourceId, source, destination?.InternalID, GetGameObjectString(destination), type, money);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(GameObject source, string destinationId, string destination, eInventoryActionType type, long money)
		{
			LogInventoryAction(source?.InternalID, GetGameObjectString(source), destinationId, destination, type, money);
		}

		/// <summary>
		/// Log an action of player's inventory (loot, buy, trade, etc...)
		/// </summary>
		/// <param name="source">Source of the item</param>
		/// <param name="destination">Destination of the item</param>
		/// <param name="type">Type of action (trade, loot, quest, ...)</param>
		/// <param name="item">The item or money account traded</param>
		public static void LogInventoryAction(string sourceId, string source, string destinationId, string destination, eInventoryActionType type, long money)
		{
			// Check if you can log this action
			if (!_IsLoggingEnabled(type))
				return;

			string format;
			if (!ActionXformat.TryGetValue(type, out format))
				return; // Error, this format does not exists ?!

			try
			{
				GameServer.Instance.LogInventoryAction(string.Format(format, source ?? "(null)", destination ?? "(null)", GetMoneyString(money)));
				GameServer.Database.AddObject(new InventoryLog
				{
					Source = source,
					SourceId = sourceId,
					Destination = destination,
					DestinationId = destinationId,
					ItemTemplate = null,
					ItemUnique = null,
					Money = money,
					ItemCount = 0,
				});
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Log inventory error", e);
			}
		}

		private static bool _IsLoggingEnabled(eInventoryActionType type)
		{
			if (!ServerProperties.Properties.LOG_INVENTORY)
				return false;

			switch (type)
			{
				case eInventoryActionType.Trade: return ServerProperties.Properties.LOG_INVENTORY_TRADE;
				case eInventoryActionType.Loot: return ServerProperties.Properties.LOG_INVENTORY_LOOT;
				case eInventoryActionType.Craft: return ServerProperties.Properties.LOG_INVENTORY_CRAFT;
				case eInventoryActionType.Merchant: return ServerProperties.Properties.LOG_INVENTORY_MERCHANT;
				case eInventoryActionType.Quest: return ServerProperties.Properties.LOG_INVENTORY_QUEST;
				case eInventoryActionType.Other: return ServerProperties.Properties.LOG_INVENTORY_OTHER;
			}
			return false;
		}
	}
}
