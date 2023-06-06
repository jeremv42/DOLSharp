using System;
using System.Numerics;
using AmteScripts.Managers;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;

namespace DOL.GS
{
	public class AmtePlayer : GamePlayer
	{
		public DateTime LastDeath = DateTime.MinValue;

		public DateTime LastActivity = DateTime.Now;
		public Vector3 LastPosition = Vector3.Zero;

		public AmtePlayer(GameClient client, DOLCharacters dbChar) : base(client, dbChar) {}

		public override int BountyPointsValue
		{
			get
			{
				if (RvrManager.Instance.IsInRvr(this))
					return (int)(1 + Level * 0.6);
				return 0;
			}
		}

		public override void Die(GameObject killer)
		{
			base.Die(killer);

			if (RvrManager.Instance.IsInRvr(this))
				return;

			LastDeath = DateTime.Now;
		}

		public void SendMessage(string message, eChatType type = eChatType.CT_System, eChatLoc loc = eChatLoc.CL_SystemWindow)
		{
			Out.SendMessage(message, type, loc);
		}

		public override void CraftItem(ushort itemID)
		{
			if (JailMgr.IsPrisoner(this))
				return;

			LastActivity = DateTime.Now;
			base.CraftItem(itemID);
		}

		public override void UseSlot(int slot, int type)
		{
			if (JailMgr.IsPrisoner(this))
				return;
			base.UseSlot(slot, type);
		}
	}
}
