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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Account table
	/// </summary>
	[DataTable(TableName = "InventoryLog")]
	public class InventoryLog : DataObject
	{
		private DateTime m_createdAt;

		/// <summary>
		/// Create account row in DB
		/// </summary>
		public InventoryLog()
		{
			m_createdAt = DateTime.Now;
		}

		[DataElement(AllowDbNull = false)]
		public DateTime CreatedAt => m_createdAt;

		[DataElement(AllowDbNull = true)]
		public string Source { get; init; }
		[DataElement(AllowDbNull = true)]
		public string SourceId { get; init; }
		[DataElement(AllowDbNull = true)]
		public int SourcePlvl { get; init; }
		[DataElement(AllowDbNull = true)]
		public string Destination { get; init; }
		[DataElement(AllowDbNull = true)]
		public string DestinationId { get; init; }
		[DataElement(AllowDbNull = true)]
		public int DestinationPlvl { get; init; }
		[DataElement(AllowDbNull = true)]
		public string ItemTemplate { get; init; }
		[DataElement(AllowDbNull = true)]
		public string ItemUnique { get; init; }
		[DataElement(AllowDbNull = false)]
		public long Money { get; init; }
		[DataElement(AllowDbNull = false)]
		public int ItemCount { get; init; }
	}
}
