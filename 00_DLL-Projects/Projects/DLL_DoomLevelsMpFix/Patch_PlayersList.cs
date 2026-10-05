using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	[HarmonyPatch(typeof(XUiC_PlayersList), "updatePlayersList")]
	internal static class Patch_PlayersList
	{
		private static readonly System.Reflection.FieldInfo EntriesField = AccessTools.Field(typeof(XUiC_PlayersList), "playerEntries");
		private static readonly System.Reflection.FieldInfo SortedField = AccessTools.Field(typeof(XUiC_PlayersList), "sortedPlayerList");
		private static readonly System.Reflection.FieldInfo EnabledColorField = AccessTools.Field(typeof(XUiC_PlayersListEntry), "enabledColor");

		private static void Postfix(XUiC_PlayersList __instance)
		{
			try
			{
				Apply(__instance);
			}
			catch
			{
			}
		}

		private static void Apply(XUiC_PlayersList list)
		{
			XUiC_PlayersListEntry[] entries = EntriesField != null ? EntriesField.GetValue(list) as XUiC_PlayersListEntry[] : null;
			if (entries == null || entries.Length == 0)
			{
				entries = list.GetChildrenByType<XUiC_PlayersListEntry>();
			}

			if (entries == null || entries.Length == 0)
			{
				return;
			}

			IncludeMissing(list, entries);
			EntityPlayer local = list.xui?.playerUI?.entityPlayer;
			bool localAway = PartyHud.InLevel(local);
			int[] ids = local?.Party != null ? PartyIds.For(local.Party) : null;
			int leader = PartyIds.LeaderOf(local?.Party);
			World world = GameManager.Instance?.World;
			Color enabled = EnabledColorField != null ? (Color)EnabledColorField.GetValue(entries[0]) : Color.white;

			for (int i = 0; i < entries.Length; i++)
			{
				XUiC_PlayersListEntry entry = entries[i];
				if (entry?.PlayerData == null)
				{
					HideUnusedMarks(entry);
					continue;
				}

				int id = entry.PlayerData.EntityId;
				string rowName = entry.PlayerData.PlayerName?.DisplayName;
				if (id <= 0 || !NameMatches(rowName, id))
				{
					ClearLocation(entry);
					continue;
				}

				bool isLocal = local != null && id == local.entityId;
				bool inMap = InstanceSync.Known(id);
				EntityPlayer live = PartyMembers.Find(world, id);
				bool cachedOnline = InstanceSync.TryStats(id, out InstanceSync.Stats cached);
				if (live == null && !inMap && !cachedOnline)
				{
					ClearLocation(entry);
					if (localAway)
					{
						HideMapButton(entry);
					}

					continue;
				}

				if (isLocal)
				{
					ApplyLocalParty(entry, local, leader);
					ApplyLocation(entry, localAway, local?.Party, id, enabled);
					if (localAway)
					{
						HideMapButton(entry);
					}

					continue;
				}

				if (live == null)
				{
					entry.IsOffline = false;
					entry.EntityId = id;
					if (entry.Voice != null)
					{
						entry.Voice.IsVisible = true;
					}

					if (entry.Chat != null)
					{
						entry.Chat.IsVisible = true;
					}

					ApplyParty(entry, local, id, ids, leader);
					if (cachedOnline)
					{
						Paint(entry, cached, enabled);
						SetLabel(entry.PingText, cached.Ping < 0 ? "--" : cached.Ping.ToString(), enabled);
					}

					ApplyOverworld(entry, id, local, ids);
				}
				else if (Contains(ids, id))
				{
					ApplyParty(entry, local, id, ids, leader);
				}

				ApplyLocation(entry, inMap || PartyHud.InLevel(id, live), local?.Party, id, enabled);
				if (localAway)
				{
					HideMapButton(entry);
				}
			}
		}

		private static void HideMapButton(XUiC_PlayersListEntry entry)
		{
			if (entry == null)
			{
				return;
			}

			entry.ShowOnMapEnabled = false;
			if (entry.labelShowOnMap != null)
			{
				entry.labelShowOnMap.IsVisible = false;
			}

			if (entry.buttonShowOnMap != null)
			{
				entry.buttonShowOnMap.IsVisible = false;
				entry.buttonShowOnMap.Enabled = false;
			}

			if (entry.DistanceToFriend != null)
			{
				entry.DistanceToFriend.Text = "";
			}
		}

		private static void ApplyLocalParty(XUiC_PlayersListEntry entry, EntityPlayer local, int leader)
		{
			if (entry == null || local == null || !local.IsInParty() || leader <= 0)
			{
				return;
			}

			entry.PartyStatus = leader == local.entityId
				? XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InPartyAsLead
				: XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InParty;
		}

		private static void ApplyParty(XUiC_PlayersListEntry entry, EntityPlayer local, int id, int[] ids, int leader)
		{
			bool inParty = Contains(ids, id);
			bool iLead = false;
			if (local != null)
			{
				if (leader > 0)
				{
					iLead = leader == local.entityId;
				}
				else if (local.Party != null)
				{
					iLead = local.Party.Leader == local;
				}
				else
				{
					iLead = true;
				}
			}

			bool canInvite = local == null || local.Party == null || iLead;
			if (inParty)
			{
				PartyInvites.Forget(id);
				if (leader == id)
				{
					entry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyIsLead;
				}
				else if (iLead)
				{
					entry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyAsLead;
				}
				else
				{
					entry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InParty;
				}

				return;
			}

			if (PartyInvites.Has(id) && (local == null || local.Party == null))
			{
				entry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
				return;
			}

			if (local != null && local.IsInParty() && InstanceSync.PartyIsFull(id))
			{
				entry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_PartyFullAsLead;
				return;
			}

			entry.PartyStatus = canInvite
				? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead
				: XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoParty;
		}

		private static void ApplyOverworld(XUiC_PlayersListEntry entry, int id, EntityPlayer local, int[] ids)
		{
			if (entry == null || InstanceSync.Known(id))
			{
				return;
			}

			bool ally = false;
			PersistentPlayerData me = GameManager.Instance?.persistentLocalPlayer;
			if (me != null && entry.PlayerData != null)
			{
				ally = me.IsAlly(entry.PlayerData);
			}

			bool party = Contains(ids, id);
			if (!World.MapEnabled || (!ally && !party) || !InstanceSync.TryPos(id, out Vector3 pos))
			{
				return;
			}

			if (local == null || PartyHud.InLevel(local))
			{
				return;
			}

			entry.ShowOnMapEnabled = true;
			if (entry.DistanceToFriend != null)
			{
				float magnitude = (pos - local.GetPosition()).magnitude;
				entry.DistanceToFriend.Text = ValueDisplayFormatters.Distance(magnitude);
			}
		}

		private static void IncludeMissing(XUiC_PlayersList list, XUiC_PlayersListEntry[] entries)
		{
			List<PersistentPlayerData> sorted = SortedField != null ? SortedField.GetValue(list) as List<PersistentPlayerData> : null;
			PersistentPlayerList players = GameManager.Instance?.persistentPlayers;
			if (sorted == null || players == null)
			{
				return;
			}

			List<int> online = new List<int>();
			InstanceSync.CollectOnline(online);
			bool added = false;
			for (int i = 0; i < online.Count; i++)
			{
				int id = online[i];
				bool present = false;
				for (int j = 0; j < sorted.Count; j++)
				{
					if (sorted[j] != null && sorted[j].EntityId == id)
					{
						present = true;
						break;
					}
				}

				if (present)
				{
					continue;
				}

				PersistentPlayerData data = players.GetPlayerDataFromEntityID(id);
				if (data == null)
				{
					continue;
				}

				sorted.Add(data);
				added = true;
			}

			if (!added)
			{
				return;
			}

			sorted.Sort(Patch_PlayersListSort.ComparePlayers);
			XUiC_Paging pager = list.GetChildById("playerPager") as XUiC_Paging;
			int page = 0;
			if (pager != null)
			{
				pager.SetLastPageByElementsAndPageLength(sorted.Count, entries.Length);
				page = pager.GetPage();
			}

			XUiController count = list.GetChildById("numberOfPlayers");
			if (count?.ViewComponent is XUiV_Label countLabel)
			{
				countLabel.Text = sorted.Count.ToString();
			}

			int start = page * entries.Length;
			for (int i = 0; i < entries.Length; i++)
			{
				int index = start + i;
				if (index >= sorted.Count || sorted[index] == null)
				{
					ClearRow(entries[i]);
				}
				else
				{
					FillRow(entries[i], sorted[index], list);
				}
			}
		}

		private static void FillRow(XUiC_PlayersListEntry entry, PersistentPlayerData data, XUiC_PlayersList list)
		{
			if (entry == null || data == null)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			EntityPlayerLocal local = list?.xui?.playerUI?.entityPlayer;
			EntityPlayer live = PartyMembers.Find(world, data.EntityId);
			bool online = live != null || InstanceSync.IsSyncedOnline(data.EntityId);
			entry.PlayerData = data;
			entry.EntityId = online ? data.EntityId : -1;
			if (online)
			{
				entry.IsOffline = false;
			}
			if (entry.ViewComponent != null)
			{
				entry.ViewComponent.IsVisible = true;
			}

			string name = data.PlayerName?.DisplayName ?? data.PrimaryId?.CombinedString;
			if (entry.PlayerName != null)
			{
				entry.PlayerName.UpdatePlayerData(data.PlayerData, false, name);
			}

			if (!online || live == null)
			{
				if (entry.AdminSprite != null)
				{
					entry.AdminSprite.IsVisible = false;
				}

				if (entry.TwitchSprite != null)
				{
					entry.TwitchSprite.IsVisible = false;
				}

				if (entry.TwitchDisabledSprite != null)
				{
					entry.TwitchDisabledSprite.IsVisible = false;
				}

				if (!online)
				{
					SetLabel(entry.ZombieKillsText, "--", Color.white);
					SetLabel(entry.PlayerKillsText, "--", Color.white);
					SetLabel(entry.DeathsText, "--", Color.white);
					SetLabel(entry.LevelText, "--", Color.white);
					SetLabel(entry.GamestageText, "--", Color.white);
					SetLabel(entry.PingText, "--", Color.white);
					if (entry.DistanceToFriend != null)
					{
						entry.DistanceToFriend.Text = "--";
					}

					if (entry.Voice != null)
					{
						entry.Voice.IsVisible = false;
					}

					if (entry.Chat != null)
					{
						entry.Chat.IsVisible = false;
					}

					entry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.Offline;
					entry.ShowOnMapEnabled = false;
					entry.IsOffline = true;
				}
			}
			else
			{
				if (entry.AdminSprite != null)
				{
					entry.AdminSprite.IsVisible = live.IsAdmin;
				}

				SetLabel(entry.ZombieKillsText, live.KilledZombies.ToString(), Color.white);
				SetLabel(entry.PlayerKillsText, live.KilledPlayers.ToString(), Color.white);
				SetLabel(entry.DeathsText, live.Died.ToString(), Color.white);
				SetLabel(entry.LevelText, live.Progression != null ? live.Progression.GetLevel().ToString() : "--", Color.white);
				SetLabel(entry.GamestageText, live.gameStage.ToString(), Color.white);
				bool serverSelf = live == local && SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
				SetLabel(entry.PingText, serverSelf || live.pingToServer < 0 ? "--" : live.pingToServer.ToString(), Color.white);
				if (entry.Voice != null)
				{
					entry.Voice.IsVisible = live != local;
				}

				if (entry.Chat != null)
				{
					entry.Chat.IsVisible = live != local;
				}
			}

			if (entry.buttonReportPlayer != null)
			{
				entry.buttonReportPlayer.IsVisible = Platform.PlatformManager.MultiPlatform.PlayerReporting != null && (local == null || data.EntityId != local.entityId);
			}

			entry.IsLocalPlayer = local != null && data.EntityId == local.entityId;
		}

		private static void ClearRow(XUiC_PlayersListEntry entry)
		{
			if (entry == null)
			{
				return;
			}

			entry.EntityId = -1;
			entry.PlayerData = null;
			if (entry.PlayerName != null)
			{
				entry.PlayerName.ClearPlayerData();
			}

			entry.IsLocalPlayer = false;
			entry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.Offline;
			entry.ShowOnMapEnabled = false;
			if (entry.buttonReportPlayer != null)
			{
				entry.buttonReportPlayer.IsVisible = false;
			}

			HideUnusedMarks(entry);
		}

		private static void HideUnusedMarks(XUiC_PlayersListEntry entry)
		{
			ClearLocation(entry);
			if (entry == null)
			{
				return;
			}

			if (entry.labelAllyIcon != null)
			{
				entry.labelAllyIcon.IsVisible = false;
			}

			if (entry.labelPartyIcon != null)
			{
				entry.labelPartyIcon.IsVisible = false;
			}

			if (entry.labelShowOnMap != null)
			{
				entry.labelShowOnMap.IsVisible = false;
			}

			if (entry.DistanceToFriend != null)
			{
				entry.DistanceToFriend.IsVisible = false;
			}

			if (entry.buttonAllyIcon != null)
			{
				entry.buttonAllyIcon.IsVisible = false;
			}

			if (entry.buttonPartyIcon != null)
			{
				entry.buttonPartyIcon.IsVisible = false;
			}

			if (entry.buttonShowOnMap != null)
			{
				entry.buttonShowOnMap.IsVisible = false;
			}
		}

		private static void Paint(XUiC_PlayersListEntry entry, InstanceSync.Stats stats, Color enabled)
		{
			SetLabel(entry.LevelText, stats.Level > 0 ? stats.Level.ToString() : "--", enabled);
			SetLabel(entry.GamestageText, stats.GameStage > 0 ? stats.GameStage.ToString() : "--", enabled);
			SetLabel(entry.ZombieKillsText, stats.ZombieKills.ToString(), enabled);
			SetLabel(entry.PlayerKillsText, stats.PlayerKills.ToString(), enabled);
			SetLabel(entry.DeathsText, stats.Deaths.ToString(), enabled);
			if (entry.PlayerName != null)
			{
				entry.PlayerName.Color = enabled;
			}
		}

		private static void SetLabel(XUiV_Label label, string text, Color enabled)
		{
			if (label == null)
			{
				return;
			}

			label.Text = text;
			label.Color = enabled;
		}

		private static void ApplyLocation(XUiC_PlayersListEntry entry, bool away, Party party, int id, Color enabled)
		{
			if (entry == null)
			{
				return;
			}

			XUiV_Sprite back = PartyHud.ChildSprite(entry, "doomLevelBack");
			XUiV_Sprite icon = PartyHud.ChildSprite(entry, "doomLevelIcon");
			if (!away)
			{
				PartyHud.SetVisible(back, false);
				PartyHud.SetVisible(icon, false);
				return;
			}

			SetLabel(entry.DistanceToFriend, "", enabled);
			if (entry.labelShowOnMap != null)
			{
				entry.labelShowOnMap.IsVisible = false;
			}

			if (entry.buttonShowOnMap != null)
			{
				entry.buttonShowOnMap.IsVisible = false;
				entry.buttonShowOnMap.Enabled = false;
			}

			Color32 tint = PartyIds.ColorOf(party, id);
			PartyHud.SetBacking(back, true, tint);
			PartyHud.SetItemIcon(icon, true);
		}

		private static void ClearLocation(XUiC_PlayersListEntry entry)
		{
			if (entry == null)
			{
				return;
			}

			PartyHud.SetVisible(PartyHud.ChildSprite(entry, "doomLevelBack"), false);
			PartyHud.SetVisible(PartyHud.ChildSprite(entry, "doomLevelIcon"), false);
		}

		private static bool NameMatches(string rowName, int id)
		{
			string known = PartyIds.NameOf(id);
			if (string.IsNullOrEmpty(known) || string.IsNullOrEmpty(rowName))
			{
				return true;
			}

			return string.Equals(known, rowName, System.StringComparison.Ordinal);
		}

		private static bool Contains(int[] ids, int id)
		{
			if (ids == null)
			{
				return false;
			}

			for (int i = 0; i < ids.Length; i++)
			{
				if (ids[i] == id)
				{
					return true;
				}
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(XUiC_PlayersListEntry), "oniconPartyIconPress")]
	internal static class Patch_PartyInvitePress
	{
		private static bool Prefix(XUiC_PlayersListEntry __instance)
		{
			if (GameStats.GetBool(EnumGameStats.AutoParty))
			{
				return true;
			}

			XUiC_PlayersListEntry.EnumPartyStatus status = __instance.PartyStatus;
			bool sending = status == XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead
				|| status == XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_NoParty;
			bool accepting = status == XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
			if (!sending && !accepting)
			{
				return true;
			}

			EntityPlayerLocal local = __instance.xui?.playerUI?.entityPlayer;
			if (local == null || __instance.EntityId <= 0)
			{
				return true;
			}

			if (GameManager.Instance?.World?.GetEntity(__instance.EntityId) is EntityPlayer)
			{
				return true;
			}

			if (accepting)
			{
				Audio.Manager.PlayInsidePlayerHead("party_join");
				Send(NetPackagePartyActions.PartyActions.AcceptInvite, __instance.EntityId, local.entityId);
				return false;
			}

			string name = __instance.PlayerData?.PlayerName?.DisplayName ?? "";
			if (UnityEngine.Time.time <= __instance.lastTime)
			{
				GameManager.ShowTooltip(local, string.Format(Localization.Get("ttPartyInviteWait"), name));
				return false;
			}

			__instance.lastTime = UnityEngine.Time.time + 5f;
			GameManager.ShowTooltip(local, string.Format(Localization.Get("ttPartyInviteSent"), name));
			Send(NetPackagePartyActions.PartyActions.SendInvite, local.entityId, __instance.EntityId);
			return false;
		}

		private static void Send(NetPackagePartyActions.PartyActions action, int fromId, int toId)
		{
			NetPackagePartyActions package = NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(action, fromId, toId);
			ConnectionManager connection = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (connection == null)
			{
				return;
			}

			if (connection.IsServer)
			{
				connection.SendPackage(package);
			}
			else
			{
				connection.SendToServer(package);
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_PlayersList), nameof(XUiC_PlayersList.PlayerComparator))]
	internal static class Patch_PlayersListSort
	{
		private static void Postfix(PersistentPlayerData a, PersistentPlayerData b, ref int __result)
		{
			__result = ComparePlayers(a, b);
		}

		internal static int ComparePlayers(PersistentPlayerData a, PersistentPlayerData b)
		{
			int rankA = Rank(a);
			int rankB = Rank(b);
			if (rankA != rankB)
			{
				return rankA.CompareTo(rankB);
			}

			if (rankA >= 3)
			{
				return 0;
			}

			return LevelOf(b).CompareTo(LevelOf(a));
		}

		private static int Rank(PersistentPlayerData data)
		{
			if (data == null)
			{
				return 4;
			}

			EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (local != null && data.EntityId == local.entityId)
			{
				return 0;
			}

			bool online = (GameManager.Instance?.World?.GetEntity(data.EntityId) != null) || InstanceSync.IsSyncedOnline(data.EntityId);
			if (!online)
			{
				return 3;
			}

			PersistentPlayerData me = GameManager.Instance?.persistentLocalPlayer;
			if (me != null && me.IsAlly(data))
			{
				return 1;
			}

			return 2;
		}

		private static int LevelOf(PersistentPlayerData data)
		{
			EntityPlayer live = GameManager.Instance?.World?.GetEntity(data.EntityId) as EntityPlayer;
			if (live?.Progression != null)
			{
				return live.Progression.GetLevel();
			}

			if (data != null && InstanceSync.TryStats(data.EntityId, out InstanceSync.Stats stats))
			{
				return stats.Level;
			}

			return 0;
		}
	}

	[HarmonyPatch(typeof(XUiC_PlayersList), nameof(XUiC_PlayersList.ShowOnMap))]
	internal static class Patch_ShowOnMap
	{
		private static bool Prefix(XUiC_PlayersList __instance, int _playerId)
		{
			EntityPlayerLocal local = __instance?.xui?.playerUI?.entityPlayer;
			if (PartyHud.InLevel(local))
			{
				return false;
			}

			World world = GameManager.Instance?.World;
			if (world?.GetEntity(_playerId) != null)
			{
				return true;
			}

			if (InstanceSync.Known(_playerId) || !InstanceSync.TryPos(_playerId, out Vector3 pos))
			{
				return false;
			}
			XUiV_Window window = __instance?.xui?.GetWindow("mapArea");
			if (local == null || window?.Controller == null)
			{
				return false;
			}

			XUiC_WindowSelector.OpenSelectorAndWindow(local, "map");
			((XUiC_MapArea)window.Controller).PositionMapAt(pos);
			return false;
		}
	}
}
