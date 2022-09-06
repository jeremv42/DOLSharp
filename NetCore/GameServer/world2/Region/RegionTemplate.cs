using System.Linq;
using DOL.Database;

namespace GS.World2;

public class RegionTemplate
{
	public readonly ushort Id;
	public readonly string Name;
	public readonly bool DivingEnabled;
	public readonly bool HousingEnabled;
	public readonly string ClassType;
	public readonly Zone[] Zones;

	public RegionTemplate(DBRegions region)
	{
		Id = region.RegionID;
		Name = region.Name;
		DivingEnabled = region.DivingEnabled;
		HousingEnabled = region.HousingEnabled;
		ClassType = region.ClassType;
		Zones = DOL.GS.GameServer.Database.SelectObjects<Zones>(z => z.RegionID == Id).Select(z => new Zone(z)).ToArray();
	}
}
