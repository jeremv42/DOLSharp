/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// MoneyLootGenerator
    /// At the moment this generaotr only adds money to the loot
    /// </summary>
    public class LootGeneratorGems : LootGeneratorBase
    {
        private static readonly List<string> _gemTemplateIds = new List<string> { "Lo_gem", "Um_gem", "On_gem", "Ee_gem", "Pal_gem", "Mon_gem", "Ros_gem", "Zo_gem", "Kath_gem", "Ra_gem" };
        private static readonly Dictionary<string, float> _gemValue = new Dictionary<string, float> {
            { "Lo_gem", 5},
            { "Um_gem", 8},
            { "On_gem", 12},
            { "Ee_gem", 20},
            { "Pal_gem", 25},
            { "Mon_gem", 30},
            { "Ros_gem", 35},
            { "Zo_gem", 38},
            { "Kath_gem", 42},
            { "Ra_gem", 45},
        };
        private readonly ItemTemplate[] _gems;

        public LootGeneratorGems() : base()
        {
            this._gems = GameServer.Database.SelectObjects<ItemTemplate>(it => _gemTemplateIds.Contains(it.Id_nb)).ToArray();
        }
        private int Chance(int mobLevel, string gem)
        {
            if (!_gemValue.TryGetValue(gem, out float value))
                return 0;
            return (int)(MathF.Sin((mobLevel + (50 - value)) / 45 * ((50 - value) / 50)) * 1000);
        }

        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
        {
            LootList loot = base.GenerateLoot(mob, killer);
            if (Util.Chance(90))
                return loot;

            var gems = new LootList(1);
            foreach (var item in _gems)
                gems.AddRandom(Chance(mob.Level, item.Id_nb), item, 1);

            foreach (var item in gems.GetLoot())
                loot.AddFixed(item, 1);
            return loot;
        }
    }
}
