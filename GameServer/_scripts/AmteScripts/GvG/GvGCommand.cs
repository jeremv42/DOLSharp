using System;
using DOL.Events;
using DOL.GS.Scripts;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&gvg",
		ePrivLevel.GM,
		"Gestion du GvG",
		"/gvg <on|off> active ou désactive le gvg")]
	public class GvGCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1])
			{
				case "on":
					GvGManager.ForceOpen = true;
					break;
				case "off":
					GvGManager.ForceOpen = false;
					break;
				default:
					DisplaySyntax(client);
					break;
			}
		}
	}
}
