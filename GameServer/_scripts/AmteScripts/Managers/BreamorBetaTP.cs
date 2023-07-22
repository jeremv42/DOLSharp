using System;
using System.Linq;
using System.Reflection;
using AmteScripts.Managers;
using DOL.Events;
using log4net;

namespace DOL.GS.Scripts;

public static class BreamorBetaTP
{
	private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	private static RegionTimer _timer;

	[ScriptLoadedEvent]
	public static void OnServerStarted(DOLEvent e, object sender, EventArgs args)
	{
		log.Info("BreamorBetaTP: Started");
		_timer = new RegionTimer(WorldMgr.GetRegion(1).TimeManager)
		{
			Callback = _CheckPlayerPositions
		};
		_timer.Start(1000);
	}

	private static int _CheckPlayerPositions(RegionTimer callingtimer)
	{
		var authorizedZones = new ushort[] { 167, 168, 169, 170 };
		foreach (var client in WorldMgr.GetAllPlayingClients())
		{
			if (client?.Player?.CurrentRegionID != 163 || client.Account.PrivLevel > 1)
				continue;
			var player = client.Player;
			if (authorizedZones.Contains(player.CurrentZone.ID))
				continue;
			player.SendMessage("La béta ne vous permet pas d'aller plus loin dans cette zone.");
			player.MoveToBind();
		}
		return 1000;
	}

	[ScriptUnloadedEvent]
	public static void OnServerStopped(DOLEvent e, object sender, EventArgs args)
	{
		_timer.Stop();
		log.Info("BreamorBetaTP: Stopped");
	}
}