using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOL.GS
{
	public class BreamorFactionMgr
	{
		public const int Zone_Min = -1000000;
		public const int Zone_Neutral_Min = -10000;
		public const int Zone_Neutral_Max = 10000;
		public const int Zone_Max = 1000000;

		public static bool IsNeutral(GameLiving living)
		{
			return Zone_Neutral_Min <= living.BreamorFaction && living.BreamorFaction <= Zone_Neutral_Max;
		}

		public static bool IsHelvien(GameLiving living)
		{
			return living.BreamorFaction < Zone_Neutral_Min;
		}

		public static bool IsAverne(GameLiving living)
		{
			return living.BreamorFaction > Zone_Neutral_Max;
		}

		public static void UpdateFromKill(GamePlayer killer, GameLiving killed, float damageRatio)
		{
			if (killer == null || killed == null)
				return;

			if (IsNeutral(killed)) // nothing to update for a neutral kill
				return;

			if (killer.GetConLevel(killed) < -2) // no change if gray
				return;

			var killedValue = Math.Abs(killed.BreamorFaction) * damageRatio / 100;
			if (killed.BreamorFaction > 0)
				killedValue = -killedValue;

			if ((killed.BreamorFaction < Zone_Neutral_Min && killer.BreamorFaction < Zone_Neutral_Min) || (killed.BreamorFaction > Zone_Neutral_Max && killer.BreamorFaction > Zone_Neutral_Max))
				killedValue *= 10;

			Gain(killer, (int)killedValue);
		}

		public static void Gain(GamePlayer player, int amount)
		{
			player.BreamorFaction = Math.Clamp(player.BreamorFaction + amount, Zone_Min, Zone_Max);
			if (player.Client.Account.PrivLevel > 1)
				player.SendMessage($"faction changed: {amount}points (new faction: {player.BreamorFaction})", PacketHandler.eChatType.CT_Important, PacketHandler.eChatLoc.CL_SystemWindow);
		}
	}
}
