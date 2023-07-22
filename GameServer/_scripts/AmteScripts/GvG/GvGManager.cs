using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts;

public static class GvGManager
{
	public static bool ForceOpen = false;

	public static bool IsOpen(GamePlayer player)
	{
		if (ForceOpen)
			return true;

		if (DateTime.Now.DayOfWeek != DayOfWeek.Monday || DateTime.Now.Hour < 21 || DateTime.Now.Hour > 23)
		{
			player.Out.SendMessage(
				"Il n'est pas possible de capturer des territoires aujourd'hui à cette heure-ci.\n" +
				"Pour le moment, les forts ne sont capturables que le lundi entre 21h et 23h.\n",
				eChatType.CT_System,
				eChatLoc.CL_PopupWindow
			);
			return false;
		}

		return true;
	}
}
