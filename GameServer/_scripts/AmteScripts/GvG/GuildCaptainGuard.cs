using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOL.GS.Scripts
{
	public class GuildCaptainGuard : AmteMob
	{
		public const long CLAIM_COST = 50 * 100 * 100; // 50g
		public const ushort AREA_RADIUS = 5500;
		public const int NEUTRAL_EMBLEM = 256;

		/// <summary>
		/// "Fausses" guildes : Albion, Hibernia, Midgard, Les Maitres du Temps, Citoyens d'Amtenael
		/// </summary>
		private static readonly string[] _systemGuildIds =
		{
			"063bbcc7-0005-4667-a9ba-402746c5ae15",
			"bdbc6f4a-b9f8-4316-b88b-9698e06cdd7b",
			"50d7af62-7142-4955-9f31-0c58ac1ac33f",
			"ce6f0b34-78bc-45a9-9f65-6e849d498f6c",
			"386c822f-996b-4db6-8bd8-121c07fc11cd",
		};

		public static readonly List<GuildCaptainGuard> allCaptains = new();

		private Guild _guild;

		public List<string> safeGuildIds = new();
		private readonly AmteCustomParam _safeGuildParam;

		public GuildCaptainGuard()
		{
			_safeGuildParam = new AmteCustomParam(
				"safeGuildIds",
				() => string.Join(";", safeGuildIds),
				v => safeGuildIds = v.Split(';').ToList(),
				"");
		}

		public GuildCaptainGuard(INpcTemplate npc)
			: base(npc)
		{
			_safeGuildParam = new AmteCustomParam(
				"safeGuildIds",
				() => string.Join(";", safeGuildIds),
				v => safeGuildIds = v.Split(';').ToList(),
				"");
		}

		public override AmteCustomParam GetCustomParam()
		{
			var param = base.GetCustomParam();
			param.next = _safeGuildParam;
			return param;
		}

		public Guild Guild => _guild;

		public override bool AddToWorld()
		{
			var r = base.AddToWorld();
			_guild = GuildMgr.GetGuildByName(GuildName);
			allCaptains.Add(this);
			return r;
		}

		public override bool RemoveFromWorld()
		{
			allCaptains.Remove(this);
			return base.RemoveFromWorld();
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			/*
			if (player.Client.Account.PrivLevel == 1 && !player.GuildRank.Claim)
			{
				player.Out.SendMessage($"Bonjour {player.Name}, je ne discute pas avec les bleus, circulez.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				return true;
			}
			*/
			var sameFaction = BreamorFactionMgr.IsSameFaction(this, player);
			var hasGuildClaim = player.Guild != null && player.GuildRank.Claim;
			var canGuildClaim = hasGuildClaim && Guild == player.Guild;
			var actions = new List<(bool, string)>
			{
				(!sameFaction && !BreamorFactionMgr.IsNeutral(player), "capturer le territoire pour ma faction"),
				(hasGuildClaim && Guild == null, "capturer le territoire pour ma guilde"),
				(canGuildClaim, "modifier les alliances"),
				((!BreamorFactionMgr.IsNeutral(player) && sameFaction) || canGuildClaim, "payer un nouveau garde"),
			};

			var title = player.GuildRank?.Title ?? BreamorFactionMgr.GetRank(player).Item1;
			var actionStr = string.Join('\n', actions.Where(act => act.Item1).Select(act => $"[{act.Item2}]"));
			player.Out.SendMessage($"Bonjour {title} {player.Name}, que puis-je faire pour vous ?\n\n{actionStr}", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			return true;
		}

		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text) || source is not GamePlayer player)
				return false;
			
			var sameFaction = BreamorFactionMgr.IsSameFaction(this, player);
			var hasGuildClaim = player.Guild != null && player.GuildRank.Claim;
			var canGuildClaim = hasGuildClaim && Guild == player.Guild;

			switch (text)
			{
				case "capturer le territoire pour ma guilde":
				{
					if (hasGuildClaim && Guild == null)
						Claim(player, player.Guild);
					return true;
				}
				case "capturer le territoire pour ma faction":
				{
					if (!sameFaction && !BreamorFactionMgr.IsNeutral(player))
						Claim(player, null);
					return true;
				}

				case "modifier les alliances":
					if (!canGuildClaim)
						return false;
					var guilds = GuildMgr.GetAllGuilds()
						.Where(g => !_systemGuildIds.Contains(g.GuildID) && g.GuildID != _guild.GuildID)
						.OrderBy(g => g.Name)
						.Select(g =>
						{
							var safe = safeGuildIds.Contains(g.GuildID);
							if (safe)
								return $"{g.Name}: [{g.ID}. attaquer à vue]";
							return $"{g.Name}: [{g.ID}. ne plus attaquer à vue]";
						})
						.Aggregate((a, b) => $"{a}\n{b}");
					var safeNoGuild = safeGuildIds.Contains("NOGUILD");
					guilds += "\nLes sans guildes: [256. ";
					guilds += (safeNoGuild ? "" : "ne plus ") + "attaquer à vue]";
					player.Out.SendMessage($"Voici la liste des guildes et leurs paramètres :\n${guilds}", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					return true;

				case "payer un nouveau garde":
					if ((!BreamorFactionMgr.IsNeutral(player) && sameFaction) || canGuildClaim)
						BuyGuard(player);
					return true;
			}

			// change guild allies
			if (!canGuildClaim)
				return false;
			var dotIdx = text.IndexOf('.');
			ushort id;
			if (dotIdx > 0 && ushort.TryParse(text[..dotIdx], out id))
			{
				var guild = GuildMgr.GetAllGuilds().FirstOrDefault(g => g.ID == id);
				if (guild == null && id != 256)
					return false;
				var guildID = guild == null ? "NOGUILD" : guild.GuildID;
				if (safeGuildIds.Contains(guildID))
					safeGuildIds.Remove(guildID);
				else
					safeGuildIds.Add(guildID);
				SaveIntoDatabase();
				return WhisperReceive(source, "modifier les alliances");
			}

			return false;
		}

		public IEnumerable<SimpleGvGGuard> GetGuardsInRadius(ushort radius = AREA_RADIUS)
		{
			foreach (var npc in GetNPCsInRadius(radius).OfType<SimpleGvGGuard>())
			{
				if (npc.Captain != this)
					continue;
				yield return npc;
			}
		}

		public void ResetArea(int newemblem, int oldemblem = NEUTRAL_EMBLEM)
		{
			foreach (var guard in GetGuardsInRadius())
				guard.Captain = this;
			foreach (var obj in GetItemsInRadius(AREA_RADIUS))
				if (obj is GameStaticItem item && item.Emblem == oldemblem)
					item.Emblem = newemblem;
		}

		public void BuyGuard(GamePlayer player)
		{
			player.Out.SendMessage($"Vous devez prendre contact avec un Game Master d'Amtenaël.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
		}

		public void Claim(GamePlayer player, Guild guild)
		{
			if (!Name.StartsWith("Capitaine") && !Name.StartsWith("Legatus") && !Name.StartsWith("Jarl"))
			{
				player.Out.SendMessage(
					"Vous devez demander à un GM pour ce type de territoire.",
					eChatType.CT_System,
					eChatLoc.CL_PopupWindow
				);
				return;
			}

			if (!GvGManager.IsOpen(player))
				return;

			if (GetGuardsInRadius(AREA_RADIUS * 2).Any(g => g.IsAlive))
			{
				player.Out.SendMessage(
					"Vous devez tuer tous les gardes avant de pouvoir prendre possession du territoire.",
					eChatType.CT_System,
					eChatLoc.CL_PopupWindow
				);
				return;
			}

			if (guild != null && !player.RemoveMoney(CLAIM_COST))
			{
				player.Out.SendMessage(
					"Vous n'avez pas assez d'argent pour prendre possession du territoire.",
					eChatType.CT_System,
					eChatLoc.CL_PopupWindow
				);
				return;
			}

			var oldguild = GuildMgr.GetGuildByName(GuildName);
			int emblem;
			if (guild == null)
			{
				BreamorFaction = BreamorFactionMgr.IsAverne(player) ? BreamorFactionMgr.GvGGuard_Avernes : BreamorFactionMgr.GvGGuard_Helviens;
				_guild = null;
				GuildName = BreamorFactionMgr.IsAverne(player) ? "Avernes" : "Helviens";
				emblem = BreamorFactionMgr.IsAverne(player) ? BreamorFactionMgr.GvGGuard_Avernes_Emblem : BreamorFactionMgr.GvGGuard_Helviens_Emblem;
			}
			else
			{
				BreamorFaction = 0;
				_guild = guild;
				GuildName = guild.Name;
				emblem = guild.Emblem;
			}
			SaveIntoDatabase();
			ResetArea(emblem, oldguild?.Emblem ?? NEUTRAL_EMBLEM);
			if (guild != null)
				player.Out.SendMessage(
					"Le territoire appartient maintenant à votre guilde, que voulez-vous faire ?\n\n[modifier les alliances]\n",
					eChatType.CT_System,
					eChatLoc.CL_PopupWindow
				);
			else
				player.Out.SendMessage(
					"Le territoire appartient maintenant à votre faction !",
					eChatType.CT_System,
					eChatLoc.CL_PopupWindow
				);
		}
	}
}