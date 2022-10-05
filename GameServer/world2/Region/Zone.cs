using DOL.Database;

namespace GS.World2;

public struct Zone
{
	public readonly ushort Id;
	public readonly string Name;
	public readonly int OffsetX;
	public readonly int OffsetY;
	public readonly int Width;
	public readonly int Height;
	public readonly int WaterLevel;
	public readonly bool IsLava;

	public Zone(Zones z)
	{
		Id = (ushort)z.ZoneID;
		Name = z.Name;
		OffsetX = z.OffsetX;
		OffsetY = z.OffsetY;
		Width = z.Width;
		Height = z.Height;
		WaterLevel = z.WaterLevel;
		IsLava = z.IsLava;
	}
}
