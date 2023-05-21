using System;

using DOL.GS.Keeps;

namespace DOL.GS.PropertyCalc
{
	/// <summary>
	/// The Max HP calculator
	///
	/// BuffBonusCategory1 is used for absolute HP buffs
	/// BuffBonusCategory2 unused
	/// BuffBonusCategory3 unused
	/// BuffBonusCategory4 unused
	/// BuffBonusMultCategory1 unused
	/// </summary>
	[PropertyCalculator(eProperty.MaxHealth)]
	public class MaxHealthCalculator : PropertyCalculator
	{
		public override int CalcValue(GameLiving living, eProperty property)
		{
			if (living is GamePlayer player)
			{
				int hpBase = player.CalculateMaxHealth(player.Level, player.GetModified(eProperty.Constitution));
				int buffBonus = living.BaseBuffBonusCategory[(int) property];
				if (buffBonus < 0)
					buffBonus = hpBase * buffBonus / -100;
				int itemBonus = living.ItemBonus[(int) property];
				int cap = Math.Max(player.Level * 4, 20) + // at least 20
				          Math.Min(living.ItemBonus[(int) eProperty.MaxHealthCapBonus], player.Level * 4);
				itemBonus = Math.Min(itemBonus, cap);
				if (player.HasAbility(Abilities.ScarsOfBattle) && player.Level >= 40)
				{
					int levelbonus = Math.Min(player.Level - 40, 10);
					hpBase = (int) (hpBase * (100 + levelbonus) * 0.01);
				}

				int abilityBonus = living.AbilityBonus[(int) property];

				return Math.Max(hpBase + itemBonus + buffBonus + abilityBonus, 1); // at least 1
			}

			if (living is GameNPC npc)
			{
				var constitution = living.GetModified(eProperty.Constitution);
				constitution -= 30;
				if (constitution < 0)
					constitution *= 2;

				// hp1 : from level
				// hp2 : from constitution
				var hp1 = NpcMaxHealthAtLevels[npc.Level];
				if (npc is TheurgistPet pet)
				{
					if (pet.Name.ToLower().StartsWith("air"))
						hp1 /= 3;
					else if (pet.Name.ToLower().StartsWith("ice"))
						hp1 /= 4;
					else if (pet.Name.ToLower().StartsWith("earth"))
						hp1 /= 6;
				}
				var hp2 = hp1 * constitution / 10000;

				var extra = living.GetModified(eProperty.ExtraHP);
				if (extra < 1)
					extra = 100;
				int hpBase = (hp1 + hp2) * extra / 100;
				int buffBonus = living.BaseBuffBonusCategory[(int) property];
				if (buffBonus < 0)
					buffBonus = hpBase * buffBonus / -100;
				int itemBonus = living.ItemBonus[(int) property];
				if (npc.Level > 40 && npc.HasAbility(Abilities.ScarsOfBattle))
				{
					int levelbonus = Math.Min(npc.Level - 40, 10);
					hpBase = (int) (hpBase * (100 + levelbonus) * 0.01);
				}

				int abilityBonus = living.AbilityBonus[(int) property];

				return Math.Max(hpBase + itemBonus + buffBonus + abilityBonus, 1); // at least 1
			}

			if (living is GameKeepComponent)
			{
				GameKeepComponent keepComp = living as GameKeepComponent;

				if (keepComp.Keep != null)
					return (keepComp.Keep.EffectiveLevel(keepComp.Keep.Level) + 1) * keepComp.Keep.BaseLevel * 200;

				return 0;
			}

			if (living is GameKeepDoor)
			{
				GameKeepDoor keepdoor = living as GameKeepDoor;

				if (keepdoor.Component != null && keepdoor.Component.Keep != null)
				{
					return (keepdoor.Component.Keep.EffectiveLevel(keepdoor.Component.Keep.Level) + 1) * keepdoor.Component.Keep.BaseLevel * 200;
				}

				return 0;

				//todo : use material too to calculate maxhealth
			}

			if (living.Level < 10)
			{
				return living.Level * 20 + 20 + living.BaseBuffBonusCategory[(int) property]; // default
			}

			// approx to original formula, thx to mathematica :)
			int hp = (int) (50 + 11 * living.Level + 0.548331 * living.Level * living.Level) + living.BaseBuffBonusCategory[(int) property];
			if (living.Level < 25)
			{
				hp += 20;
			}

			return hp;
		}

		/// <summary>
        /// Returns the hits cap for this living.
        /// </summary>
        /// <param name="living">The living the cap is to be determined for.</param>
        /// <returns></returns>
        public static int GetItemBonusCap(GameLiving living)
        {
            if (living == null) return 0;
            return living.Level * 4;
        }

        /// <summary>
        /// Returns the hits cap increase for the this living.
        /// </summary>
        /// <param name="living">The living the cap increase is to be determined for.</param>
        /// <returns></returns>
        public static int GetItemBonusCapIncrease(GameLiving living)
        {
            if (living == null) return 0;
            int itemBonusCapIncreaseCap = GetItemBonusCapIncreaseCap(living);
            int itemBonusCapIncrease = living.ItemBonus[(int)(eProperty.MaxHealthCapBonus)];
            return Math.Min(itemBonusCapIncrease, itemBonusCapIncreaseCap);
        }

        /// <summary>
        /// Returns the cap for hits cap increase for this living.
        /// </summary>
        /// <param name="living">The living the value is to be determined for.</param>
        /// <returns>The cap increase cap for this living.</returns>
        public static int GetItemBonusCapIncreaseCap(GameLiving living)
        {
            if (living == null) return 0;
            return living.Level * 4;
        }
        
        public static readonly int[] NpcMaxHealthAtLevels =
		{
			18, // 0
			24,
			34,
			48,
			66,
			89, // 5
			115,
			145,
			179,
			217,
			241, // 10
			265,
			290,
			317,
			344,
			373, // 15
			403,
			434,
			467,
			500,
			535, // 20
			570,
			607,
			645,
			684,
			725, // 25
			766,
			809,
			853,
			898,
			944, // 30
			991,
			1039,
			1089,
			1139,
			1191, // 35
			1244,
			1298,
			1354,
			1410,
			1468, // 40
			1526,
			1586,
			1647,
			1709,
			1773, // 45
			1837,
			1903,
			1970,
			2038,
			2107, // 50
			2177,
			2248,
			2321,
			2394,
			2469, // 55
			2545,
			2622,
			2701,
			2780,
			2861, // 60
			2942,
			3025,
			3109,
			3194,
			3281, // 65
			3368,
			3457,
			3547,
			3638,
			3730, // 70
			3823,
			3917,
			4013,
			4109,
			4207, // 75
			4306,
			4406,
			4508,
			4610,
			4714, // 80
			4818,
			4924,
			5031,
			5139,
			5249, // 85
			5359,
			5471,
			5584,
			5698,
			5813, // 90
			5929,
			6046,
			6165,
			6284,
			6405, // 95
			6527,
			6650,
			6775,
			6900, // 99
			// other values, should be useless
			7027,
			7223,
			7421,
			7623,
			7827,
			8035,
			8246,
			8460,
			8677,
			8897,
			9121,
			9347,
			9577,
			9809,
			10045,
			10284,
			10526,
			10772,
			11020,
			11272,
			11527,
			11785,
			12046,
			12310,
			12578,
			12849,
			13123,
			13400,
			13680,
			13964,
			14251,
			14541,
			14834,
			15131,
			15431,
			15734,
			16040,
			16350,
			16663,
			16979,
			17298,
			17621,
			17947,
			18276,
			18609,
			18945,
			19284,
			19626,
			19972,
			20321,
			20674,
			21029,
			21388,
			21751,
			22117,
			22486,
			22858,
			23234,
			23613,
			23995,
			24381,
			24770,
			25163,
			25559,
			25958,
			26361,
			26767,
			27176,
			27589,
			28005,
			28424,
			28847,
			29274,
			29704,
			30137,
			30573,
			31013,
			31457,
			31904,
			32354,
			32808,
			33265,
			33725,
			34189,
			34657,
			35128,
			35602,
			36080,
			36561,
			37046,
			37534,
			38025,
			38520,
			39019,
			39521,
			40027,
			40536,
			41048,
			41564,
			42083,
			42606,
			43133,
			43663,
			44196,
			44733,
			45273,
			45817,
			46365,
			46916,
			47470,
			48028,
			48590,
			49155,
			49723,
			50295,
			50871,
			51450,
			52033,
			52619,
			53209,
			53802,
			54399,
			55000,
			55604,
			56211,
			56822,
			57437,
			58055,
			58677,
			59302,
			59931,
			60564,
			61200,
			61839,
			62483,
			63130,
			63780,
			64434,
			65092,
			65753,
			66418,
			67086,
			67758,
			68434,
			69113,
			69796,
			70482,
			71172,
			71866,
			72563,
			73264,
			73969,
			74677,
			75389,
			76104,
			76823,
		};
	}
}
