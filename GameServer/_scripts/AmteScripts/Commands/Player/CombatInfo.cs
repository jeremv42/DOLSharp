namespace DOL.GS.Commands
{
    [Cmd("&combatinfo", ePrivLevel.Player, "Toggle combatinfo flag.", "/combatinfo")]
    public class CombatInfoCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (IsSpammingCommand(client.Player, "combatinfo"))
                return;

            client.Player.CombatInfo = !client.Player.CombatInfo;
            DisplayMessage(client, $"Combat Info now set to {(client.Player.CombatInfo ? "ON" : "OFF")}.");
        }
    }
}
