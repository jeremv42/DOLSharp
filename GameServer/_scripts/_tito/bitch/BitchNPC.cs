using DOL.AI.Brain;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts;

public class BitchNPC : AmteMob
{
	public BitchNPC()
	{}
	
	public BitchNPC(INpcTemplate template) : base(template)
	{}

	public override bool AddToWorld()
	{
		if (!base.AddToWorld())
			return false;

		// mettre du code ici
		SetOwnBrain(new BitchNPCBrain());
		return true;
	}

	public override bool Interact(GamePlayer player)
	{
		if (!base.Interact(player))
			return false;

		player.Out.SendMessage("Coucou [bitch] !", eChatType.CT_System, eChatLoc.CL_PopupWindow);
		return true;
	}

	public override bool WhisperReceive(GameLiving source, string text)
	{
		if (!base.WhisperReceive(source, text) || source is not GamePlayer player)
			return false;

		if (text == "bitch")
			player.Out.SendMessage("Yep, I'm Tito the Bitch!", eChatType.CT_System, eChatLoc.CL_PopupWindow);
		return true;
	}
}
