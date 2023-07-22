using System;
using DOL.Events;
using DOL.GS;

namespace AmteScripts.Management
{
	public static class StartLocation
	{
		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			//We want to be notified whenever a new character is created
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
		}

		private static void CharacterCreation(DOLEvent e, object sender, EventArgs arguments)
		{
			CharacterEventArgs charArgs = arguments as CharacterEventArgs;
			if (charArgs == null)
				return;
			charArgs.Character.GuildID = "17118d10-a7e9-4aee-82e5-cd6ca50c0c33";
			charArgs.Character.GuildRank = 8;
			charArgs.Character.Region = 163;
			charArgs.Character.Xpos = 609510;
			charArgs.Character.Ypos = 376820;
			charArgs.Character.Zpos = 8904;
			charArgs.Character.Direction = 3726;

			charArgs.Character.BindRegion = 163;
			charArgs.Character.BindXpos = 609510;
			charArgs.Character.BindYpos = 376820;
			charArgs.Character.BindZpos = 8904;
			charArgs.Character.BindHeading = 3726;
		}
	}
}
