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
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, CharacterCreation);
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, CharacterCreation);
		}

		private static void CharacterCreation(DOLEvent e, object sender, EventArgs arguments)
		{
			if (arguments is not CharacterEventArgs charArgs)
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

			charArgs.Character.CraftingPrimarySkill = (int)eCraftingSkill.BasicCrafting;
			charArgs.Character.SerializedCraftingSkills = "1|1;2|1;3|1;4|1;6|1;7|1;8|1;9|1300;10|1;11|1;12|1;13|1500;14|1;15|1";
		}
	}
}
