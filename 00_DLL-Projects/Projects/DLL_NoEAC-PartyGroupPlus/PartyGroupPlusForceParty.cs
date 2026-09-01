using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PartyGroupPlus
{
	internal static class PartyGroupPlusForceParty
	{
		private static bool reenrolling;

		public static bool IsServer()
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			return manager != null && manager.IsServer;
		}

		public static void Apply(ForcePartyMode mode)
		{
			if (!IsServer() || GameManager.Instance?.World == null)
			{
				return;
			}

			GameStats.Set(EnumGameStats.AutoParty, mode == ForcePartyMode.Enforce);
			SyncGameStats();
			if (mode == ForcePartyMode.Enforce)
			{
				EnrollAll(force: true);
			}
			else if (mode == ForcePartyMode.Join)
			{
				EnrollAll(force: false);
			}
		}

		public static void ApplyLoaded()
		{
			Apply(PartyGroupPlusSettings.Mode);
		}

		public static void Enroll(EntityPlayer player, bool force)
		{
			if (!IsServer() || player == null || PartyGroupPlusSettings.Mode == ForcePartyMode.Off)
			{
				return;
			}

			Party serverParty = GetOrCreateServerParty();
			if (serverParty == null)
			{
				return;
			}

			if (!force && player.Party != null && player.Party != serverParty)
			{
				return;
			}

			if (serverParty.ContainsMember(player))
			{
				SendMemberListTo(player);
				PartyGroupPlusColorStore.ServerSendPartySync(player);
				return;
			}

			if (player.Party != null && player.Party != serverParty)
			{
				Party.ServerHandleLeaveParty(player, player.entityId);
			}

			if (serverParty.AddPlayer(player))
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
					NetPackageManager.GetPackage<NetPackagePartyData>().Setup(
						serverParty,
						player.entityId,
						NetPackagePartyData.PartyActions.AutoJoin));
				PartyGroupPlusColorStore.ServerSendPartySync(player);
			}
		}

		public static void SendMemberListTo(EntityPlayer player)
		{
			if (!IsServer() || player?.Party == null || player.entityId < 0)
			{
				return;
			}

			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
				NetPackageManager.GetPackage<NetPackagePartyData>().Setup(
					player.Party,
					player.entityId,
					NetPackagePartyData.PartyActions.AutoJoin),
				_onlyClientsAttachedToAnEntity: false,
				player.entityId);
		}

		public static void EnrollAll(bool force)
		{
			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null)
			{
				return;
			}

			for (int i = 0; i < world.Players.list.Count; i++)
			{
				Enroll(world.Players.list[i], force);
			}
		}

		public static void EnrollSoon(int entityId, RespawnType respawnType)
		{
			if (PartyGroupPlusSettings.Mode == ForcePartyMode.Off || !IsServer() || entityId < 0)
			{
				return;
			}

			if (respawnType == RespawnType.Died || respawnType == RespawnType.Teleport)
			{
				return;
			}

			if (PartyGroupPlusSettings.Mode == ForcePartyMode.Enforce)
			{
				GameStats.Set(EnumGameStats.AutoParty, true);
				SyncGameStats();
			}

			GameManager.Instance.StartCoroutine(EnrollWhenReady(entityId, PartyGroupPlusSettings.Mode == ForcePartyMode.Enforce));
		}

		public static void ReenrollIfEnforced(EntityPlayer player)
		{
			if (reenrolling || PartyGroupPlusSettings.Mode != ForcePartyMode.Enforce || player == null)
			{
				return;
			}

			GameManager.Instance.StartCoroutine(EnrollWhenReady(player.entityId, force: true));
		}

		public static string DescribeServerParty()
		{
			if (PartyGroupPlusSettings.ServerPartyId <= 0)
			{
				return "none yet";
			}

			if (!PartyManager.HasInstance)
			{
				return "#" + PartyGroupPlusSettings.ServerPartyId + " (not loaded)";
			}

			Party party = PartyManager.Current.GetParty(PartyGroupPlusSettings.ServerPartyId);
			if (party == null)
			{
				return "#" + PartyGroupPlusSettings.ServerPartyId + " (empty, will recreate)";
			}

			string leader = party.Leader != null ? party.Leader.PlayerDisplayName : "none";
			return "#" + party.PartyID + " leader=" + leader + " members=" + party.MemberList.Count;
		}

		private static Party GetOrCreateServerParty()
		{
			if (!IsServer() || GameManager.Instance?.World == null)
			{
				return null;
			}

			PartyManager manager = PartyManager.Current;
			Party party = manager.GetParty(1) ?? manager.CreateParty();
			if (party != null && party.PartyID != 1)
			{
				Party party1 = manager.GetParty(1);
				if (party1 != null)
				{
					party = party1;
				}
			}

			if (party != null)
			{
				PartyGroupPlusSettings.SetServerPartyId(party.PartyID);
			}

			return party;
		}

		private static bool IsInServerParty(EntityPlayer player)
		{
			Party serverParty = PartyManager.Current.GetParty(PartyGroupPlusSettings.ServerPartyId > 0
				? PartyGroupPlusSettings.ServerPartyId
				: 1);
			return player?.Party != null && serverParty != null && player.Party == serverParty;
		}

		private static EntityPlayer FindPlayer(int entityId)
		{
			if (entityId < 0 || GameManager.Instance?.World == null)
			{
				return null;
			}

			if (GameManager.Instance.World.GetEntity(entityId) is EntityPlayer fromWorld)
			{
				return fromWorld;
			}

			List<EntityPlayer> list = GameManager.Instance.World.Players?.list;
			if (list == null)
			{
				return null;
			}

			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && list[i].entityId == entityId)
				{
					return list[i];
				}
			}

			return null;
		}

		private static IEnumerator EnrollWhenReady(int entityId, bool force)
		{
			for (int i = 0; i < 300; i++)
			{
				yield return null;
				EntityPlayer player = FindPlayer(entityId);
				if (player == null)
				{
					continue;
				}

				reenrolling = true;
				try
				{
					Enroll(player, force);
				}
				finally
				{
					reenrolling = false;
				}

				if (force)
				{
					if (IsInServerParty(player))
					{
						yield break;
					}
				}
				else if (player.Party != null)
				{
					yield break;
				}
			}
		}

		private static void SyncGameStats()
		{
			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (manager == null || !manager.IsServer)
			{
				return;
			}

			manager.SendPackage(NetPackageManager.GetPackage<NetPackageGameStats>().Setup(GameStats.Instance));
		}
	}

	[HarmonyPatch(typeof(Party), nameof(Party.ServerHandleLeaveParty))]
	internal static class Patch_Party_Leave_Reenroll
	{
		public static void Postfix(EntityPlayer player)
		{
			PartyGroupPlusForceParty.ReenrollIfEnforced(player);
		}
	}

	[HarmonyPatch(typeof(Party), nameof(Party.ServerHandleKickParty))]
	internal static class Patch_Party_Kick_Reenroll
	{
		public static void Postfix(int entityID)
		{
			if (GameManager.Instance?.World?.GetEntity(entityID) is EntityPlayer player)
			{
				PartyGroupPlusForceParty.ReenrollIfEnforced(player);
			}
		}
	}

	[HarmonyPatch(typeof(Party), nameof(Party.ServerHandleAutoJoinParty))]
	internal static class Patch_Party_AutoJoin_ServerParty
	{
		public static bool Prefix(EntityPlayer joiningEntity)
		{
			if (!PartyGroupPlusForceParty.IsServer()
				|| PartyGroupPlusSettings.Mode == ForcePartyMode.Off
				|| joiningEntity == null)
			{
				return true;
			}

			PartyGroupPlusForceParty.Enroll(joiningEntity, PartyGroupPlusSettings.Mode == ForcePartyMode.Enforce);
			return false;
		}
	}

	[HarmonyPatch(typeof(GameManager), nameof(GameManager.PlayerSpawnedInWorld))]
	internal static class Patch_GameManager_Spawn_Enroll
	{
		public static void Postfix(ClientInfo _cInfo, RespawnType _respawnReason, int _entityId)
		{
			int entityId = _entityId;
			if (entityId < 0 && _cInfo != null)
			{
				entityId = _cInfo.entityId;
			}

			PartyGroupPlusForceParty.EnrollSoon(entityId, _respawnReason);
		}
	}
}
