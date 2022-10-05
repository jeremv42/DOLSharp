using System.Collections.Generic;

namespace GS.World2;

public class WorldMgr
{
	public readonly List<RegionInstance> RegionInstances = new();

	public readonly Dictionary<ushort, RegionTemplate> Regions = new();
}