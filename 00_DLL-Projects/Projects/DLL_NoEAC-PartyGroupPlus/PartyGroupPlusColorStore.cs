using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PartyGroupPlus
{
	public static class PartyGroupPlusColorStore
	{
		static readonly Dictionary<int, int> EntityToColor = new Dictionary<int, int>();
		static int PreferredColor = -1;

		public static event Action Changed;

		public static int GetPreferredColor()
		{
			return PreferredColor;
		}

		public static int GetLocalDisplayColor()
		{
			EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (local != null && TryGet(local.entityId, out int stored))
			{
				return stored;
			}

			return PreferredColor >= 0 ? PreferredColor : 0;
		}

		public static void EnsureLocalDisplayColor()
		{
			LoadPreference();
			EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (local == null || TryGet(local.entityId, out _))
			{
				return;
			}

			RequestPick(PreferredColor >= 0 ? PreferredColor : 0);
		}

		public static bool TryGet(int entityId, out int colorIndex)
		{
			return EntityToColor.TryGetValue(entityId, out colorIndex);
		}

		public static bool IsTakenInParty(Party party, int colorIndex, int exceptEntityId)
		{
			if (party == null)
			{
				return false;
			}

			for (int i = 0; i < party.MemberList.Count; i++)
			{
				EntityPlayer member = party.MemberList[i];
				if (member == null || member.entityId == exceptEntityId)
				{
					continue;
				}

				if (TryGet(member.entityId, out int taken) && taken == colorIndex)
				{
					return true;
				}
			}

			return false;
		}

		public static string TakenByName(Party party, int colorIndex)
		{
			if (party == null)
			{
				return string.Empty;
			}

			for (int i = 0; i < party.MemberList.Count; i++)
			{
				EntityPlayer member = party.MemberList[i];
				if (member == null)
				{
					continue;
				}

				if (TryGet(member.entityId, out int taken) && taken == colorIndex)
				{
					return member.PlayerDisplayName;
				}
			}

			return string.Empty;
		}

		public static void ClientSet(int entityId, int colorIndex)
		{
			EntityToColor[entityId] = colorIndex;
			ApplyToEntity(entityId);
			Changed?.Invoke();
		}

		public static void ClientClear(int entityId)
		{
			EntityToColor.Remove(entityId);
			Changed?.Invoke();
		}

		public static void RequestPick(int colorIndex)
		{
			if (colorIndex < 0 || colorIndex >= PartyGroupPlusPalette.Count)
			{
				return;
			}

			SavePreference(colorIndex);
			EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (local == null)
			{
				return;
			}

			if (local.Party != null && IsTakenInParty(local.Party, colorIndex, local.entityId))
			{
				return;
			}

			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ServerTryClaim(local.entityId, colorIndex);
				return;
			}

			ClientSet(local.entityId, colorIndex);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
				NetPackageManager.GetPackage<NetPackagePartyGroupPlusColor>().SetupRequest(colorIndex));
		}

		public static void RequestSync()
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
				if (local != null)
				{
					ServerSendPartySync(local);
				}

				return;
			}

			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
				NetPackageManager.GetPackage<NetPackagePartyGroupPlusColor>().SetupQuery());
		}

		public static void ServerAssignDefault(EntityPlayer player)
		{
			if (player == null || player.Party == null)
			{
				return;
			}

			int chosen = 0;
			if (TryGet(player.entityId, out int existing)
				&& !IsTakenInParty(player.Party, existing, player.entityId))
			{
				chosen = existing;
			}
			else
			{
				chosen = FirstFree(player.Party, player.entityId);
			}

			EntityToColor[player.entityId] = chosen;
			ApplyToPlayer(player);
			BroadcastSync(player.entityId, chosen);
			Changed?.Invoke();
		}

		public static void ServerRelease(int entityId)
		{
			if (!EntityToColor.Remove(entityId))
			{
				return;
			}

			BroadcastRelease(entityId);
			Changed?.Invoke();
		}

		public static void ServerTryClaim(int entityId, int colorIndex)
		{
			EntityPlayer player = GameManager.Instance?.World?.GetEntity(entityId) as EntityPlayer;
			if (player == null)
			{
				return;
			}

			if (colorIndex < 0 || colorIndex >= PartyGroupPlusPalette.Count)
			{
				return;
			}

			if (player.Party != null && IsTakenInParty(player.Party, colorIndex, entityId))
			{
				return;
			}

			EntityToColor[entityId] = colorIndex;
			ApplyToPlayer(player);
			BroadcastSync(entityId, colorIndex);
			Changed?.Invoke();
		}

		public static void ApplyLoadedPreference(int entityId)
		{
			LoadPreference();
			if (PreferredColor < 0)
			{
				return;
			}

			if (IsServer)
			{
				ServerTryClaim(entityId, PreferredColor);
				return;
			}

			RequestPick(PreferredColor);
		}

		public static void ServerSendPartySync(EntityPlayer player)
		{
			if (player == null)
			{
				return;
			}

			ServerSendColorSnapshot(player.entityId);
		}

		public static void ServerSendColorSnapshot(int targetEntityId)
		{
			if (targetEntityId < 0)
			{
				return;
			}

			List<int> ids = new List<int>(EntityToColor.Count);
			List<int> colors = new List<int>(EntityToColor.Count);
			foreach (KeyValuePair<int, int> pair in EntityToColor)
			{
				ids.Add(pair.Key);
				colors.Add(pair.Value);
			}

			int[] idArr = ids.ToArray();
			int[] colorArr = colors.ToArray();
			NetPackagePartyGroupPlusColor package = NetPackageManager.GetPackage<NetPackagePartyGroupPlusColor>()
				.SetupSyncAll(idArr, colorArr);

			EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
			if (local != null && local.entityId == targetEntityId)
			{
				ClientApplySyncAll(idArr, colorArr);
			}

			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
				package,
				_onlyClientsAttachedToAnEntity: false,
				targetEntityId);
		}

		public static void ClientApplySyncAll(int[] ids, int[] colors)
		{
			if (ids == null || colors == null)
			{
				return;
			}

			int n = ids.Length < colors.Length ? ids.Length : colors.Length;
			for (int i = 0; i < n; i++)
			{
				EntityToColor[ids[i]] = colors[i];
				ApplyToEntity(ids[i]);
			}

			Changed?.Invoke();
		}

		public static void ApplyToPlayer(EntityPlayer player)
		{
			if (player?.NavObject == null || !TryGet(player.entityId, out int color))
			{
				return;
			}

			player.NavObject.UseOverrideColor = true;
			player.NavObject.OverrideColor = PartyGroupPlusPalette.Get(color);
		}

		public static void ApplyToParty(Party party)
		{
			if (party == null)
			{
				return;
			}

			for (int i = 0; i < party.MemberList.Count; i++)
			{
				ApplyToPlayer(party.MemberList[i]);
			}
		}

		static void ApplyToEntity(int entityId)
		{
			if (GameManager.Instance?.World == null)
			{
				return;
			}

			ApplyToPlayer(GameManager.Instance.World.GetEntity(entityId) as EntityPlayer);
		}

		static int FirstFree(Party party, int exceptEntityId)
		{
			bool[] used = new bool[PartyGroupPlusPalette.Count];
			for (int i = 0; i < party.MemberList.Count; i++)
			{
				EntityPlayer member = party.MemberList[i];
				if (member == null || member.entityId == exceptEntityId)
				{
					continue;
				}

				if (TryGet(member.entityId, out int taken)
					&& taken >= 0 && taken < used.Length)
				{
					used[taken] = true;
				}
			}

			for (int i = 0; i < used.Length; i++)
			{
				if (!used[i])
				{
					return i;
				}
			}

			return 0;
		}

		static void BroadcastSync(int entityId, int colorIndex)
		{
			NetPackagePartyGroupPlusColor package = NetPackageManager.GetPackage<NetPackagePartyGroupPlusColor>()
				.SetupSync(entityId, colorIndex);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
		}

		static void BroadcastRelease(int entityId)
		{
			NetPackagePartyGroupPlusColor package = NetPackageManager.GetPackage<NetPackagePartyGroupPlusColor>()
				.SetupRelease(entityId);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package);
		}

		public static bool IsServer => SingletonMonoBehaviour<ConnectionManager>.Instance != null
			&& SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

		public static void LoadPreference()
		{
			try
			{
				string path = PreferencePath();
				if (string.IsNullOrEmpty(path) || !File.Exists(path))
				{
					path = LegacyPreferencePath();
				}

				if (string.IsNullOrEmpty(path) || !File.Exists(path))
				{
					return;
				}

				int parsed;
				if (int.TryParse(File.ReadAllText(path).Trim(), out parsed)
					&& parsed >= 0 && parsed < PartyGroupPlusPalette.Count)
				{
					PreferredColor = parsed;
				}
			}
			catch
			{
			}
		}

		static void SavePreference(int colorIndex)
		{
			PreferredColor = colorIndex;
			try
			{
				string path = PreferencePath();
				if (!string.IsNullOrEmpty(path))
				{
					File.WriteAllText(path, colorIndex.ToString());
				}
			}
			catch
			{
			}
		}

		static string PreferencePath()
		{
			string dir = GameIO.GetUserGameDataDir();
			if (string.IsNullOrEmpty(dir))
			{
				return null;
			}

			return Path.Combine(dir, "PartyGroupPlusColor.txt");
		}

		static string LegacyPreferencePath()
		{
			string dir = GameIO.GetUserGameDataDir();
			if (string.IsNullOrEmpty(dir))
			{
				return null;
			}

			return Path.Combine(dir, "BeyondPartySize8Color.txt");
		}
	}
}
