using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DOL.AI.Brain;
using DOL.GS.Scripts;

namespace DOL.GS
{
	public class BreamorFactionMgr
	{
		public const int Zone_Min = -10_000_000;
		public const int Zone_Helviens = -200_000;
		public const int Zone_Neutral_Min = -100_000;
		public const int Zone_Neutral_Max = 100_000;
		public const int Zone_Avernes = -200_000;
		public const int Zone_Max = 10_000_000;

		public static readonly Dictionary<int, (string, string)> Zone_Ranks = new()
		{
			// Helviens
			{ -8_500_000, ("Ard-Rioga", "Grand Roi") },
			{ -6_000_000, ("Hersir", "Chef Militaire") },
			{ -4_000_000, ("Jarl", "Chef de Clan") },
			{ -2_500_000, ("Tánaiste", "Héritier") },
			{ -1_500_000, ("Huskarl", "Homme de confiance") },
			{ -1_000_000, ("Ollamh", "Érudit") },
			{ -0_650_000, ("Gaíoch", "Guerrier") },
			{ -0_400_000, ("Brughaid", "Fermier") },
			{ -0_200_000, ("Faoileán", "Cerf") },
			{ -0_100_000, ("Trall", "Esclave") },

			{ +0_100_000, ("Coigreach", "Neutre") },

			// Avernes
			{ +0_200_000, ("Puer", "Serviteur") },
			{ +0_400_000, ("Discipulus", "Apprenti") },
			{ +0_650_000, ("Miles", "Soldat") },
			{ +1_000_000, ("Centurio", "Centurion") },
			{ +1_500_000, ("Praefectus", "Préfet") },
			{ +2_500_000, ("Marchog", "Chevalier") },
			{ +4_000_000, ("Legatus", "Légat") },
			{ +6_000_000, ("Consul", "Consul") },
			{ +8_500_000, ("Seithfed", "Seigneur") },
			{ +10_000_000, ("Pendragon", "Chef Suprême") },
		};

		public static bool IsNeutral(GameLiving living)
		{
			return Zone_Neutral_Min <= living.BreamorFaction && living.BreamorFaction <= Zone_Neutral_Max;
		}

		public static bool IsNeutralLarge(GameLiving living)
		{
			return Zone_Helviens <= living.BreamorFaction && living.BreamorFaction <= Zone_Avernes;
		}

		public static bool IsHelvien(GameLiving living)
		{
			return living.BreamorFaction < Zone_Neutral_Min;
		}

		public static bool IsAverne(GameLiving living)
		{
			return living.BreamorFaction > Zone_Neutral_Max;
		}

		public static bool IsSameFaction(GameLiving a, GameLiving b)
		{
			if (IsAverne(a) && IsAverne(b))
				return true;
			if (IsHelvien(a) && IsHelvien(b))
				return true;
			return false;
		}

		public static bool CanAttack(GameLiving attackerA, GameLiving defenderA)
		{
			var attacker = ((attackerA as GameNPC)?.Brain as IControlledBrain)?.GetLivingOwner() ?? attackerA;
			var defender = ((defenderA as GameNPC)?.Brain as IControlledBrain)?.GetLivingOwner() ?? defenderA;

			// Neutrals can always be attacked
			if (IsNeutralLarge(attacker) || IsNeutralLarge(defender))
				return true;
			// Helviens can attack Avernes
			if (attacker.BreamorFaction < Zone_Helviens && defender.BreamorFaction > Zone_Avernes)
				return true;
			// Avernes can attack Helviens 
			if (attacker.BreamorFaction > Zone_Avernes && defender.BreamorFaction < Zone_Helviens)
				return true;

			// Same faction: PvP is forbidden, PvE is ok
			if (attacker is GamePlayer && defender is GamePlayer)
				return false;
			if (attacker is GameNPC && defender is GameNPC)
				return false;

			return true;
		}

		public static (string, string) GetRank(GameLiving living)
		{
			foreach (var (key, rank) in Zone_Ranks)
				if (living.BreamorFaction <= key)
					return rank;
			return Zone_Ranks.Last().Value;
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
