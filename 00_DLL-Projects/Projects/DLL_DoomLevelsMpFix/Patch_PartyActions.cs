using System.Collections.Generic;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	internal static class PartyInvites
	{
		private static readonly HashSet<int> From = new HashSet<int>();

		internal static void Remember(int fromId)
		{
			if (fromId > 0)
			{
				From.Add(fromId);
			}
		}

		internal static bool Has(int fromId)
		{
			return fromId > 0 && From.Contains(fromId);
		}

		internal static void Forget(int fromId)
		{
			if (fromId > 0)
			{
				From.Remove(fromId);
			}
		}

		internal static void Clear()
		{
			From.Clear();
		}
	}

	[HarmonyPatch(typeof(EntityPlayer), nameof(EntityPlayer.AddPartyInvite))]
	internal static class Patch_AddPartyInvite
	{
		private static bool Prefix(EntityPlayer __instance, int playerEntityID)
		{
			if (playerEntityID <= 0)
			{
				return false;
			}

			if (GameManager.Instance?.World?.GetEntity(playerEntityID) is EntityPlayer)
			{
				return true;
			}

			if (__instance is EntityPlayerLocal)
			{
				PartyInvites.Remember(playerEntityID);
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(EntityPlayer), nameof(EntityPlayer.RemovePartyInvite))]
	internal static class Patch_RemovePartyInvite
	{
		private static void Prefix(int playerEntityID)
		{
			PartyInvites.Forget(playerEntityID);
		}
	}

	[HarmonyPatch(typeof(EntityPlayer), nameof(EntityPlayer.HasPendingPartyInvite))]
	internal static class Patch_HasPendingPartyInvite
	{
		private static void Postfix(EntityPlayer __instance, int playerEntityID, ref bool __result)
		{
			if (!__result && __instance is EntityPlayerLocal && PartyInvites.Has(playerEntityID))
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(EntityPlayer), nameof(EntityPlayer.RemoveAllPartyInvites))]
	internal static class Patch_RemoveAllPartyInvites
	{
		private static void Postfix(EntityPlayer __instance)
		{
			if (__instance is EntityPlayerLocal)
			{
				PartyInvites.Clear();
			}
		}
	}

	[HarmonyPatch(typeof(NetPackagePartyActions), nameof(NetPackagePartyActions.ProcessPackage))]
	internal static class Patch_PartyActionsPackage
	{
		private static readonly System.Reflection.FieldInfo OpField = AccessTools.Field(typeof(NetPackagePartyActions), "currentOperation");
		private static readonly System.Reflection.FieldInfo ByField = AccessTools.Field(typeof(NetPackagePartyActions), "invitedByEntityID");
		private static readonly System.Reflection.FieldInfo ToField = AccessTools.Field(typeof(NetPackagePartyActions), "invitedEntityID");

		private static bool Prefix(NetPackagePartyActions __instance, World _world)
		{
			if (_world == null || OpField == null || ByField == null || ToField == null)
			{
				return true;
			}

			NetPackagePartyActions.PartyActions op = (NetPackagePartyActions.PartyActions)OpField.GetValue(__instance);
			int byId = (int)ByField.GetValue(__instance);
			int toId = (int)ToField.GetValue(__instance);
			EntityPlayer byEntity = _world.GetEntity(byId) as EntityPlayer;
			EntityPlayer toEntity = _world.GetEntity(toId) as EntityPlayer;
			if (byEntity != null && toEntity != null)
			{
				return true;
			}

			ConnectionManager connection = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (connection == null)
			{
				return true;
			}

			if (connection.IsClient && op == NetPackagePartyActions.PartyActions.SendInvite)
			{
				EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
				if (local != null && toId == local.entityId && byId > 0)
				{
					PartyInvites.Remember(byId);
					string name = PartyIds.NameOf(byId);
					if (string.IsNullOrEmpty(name))
					{
						name = "Player";
					}

					GameManager.ShowTooltip(local, string.Format(Localization.Get("ttPartyInviteReceived"), name));
					Audio.Manager.PlayInsidePlayerHead("party_invite_receive");
				}

				return false;
			}

			if (!connection.IsServer)
			{
				return false;
			}

			byEntity = byEntity ?? PartyMembers.Find(_world, byId);
			toEntity = toEntity ?? PartyMembers.Find(_world, toId);
			if (byEntity == null || toEntity == null)
			{
				return false;
			}

			switch (op)
			{
				case NetPackagePartyActions.PartyActions.SendInvite:
					if (byEntity.HasPendingPartyInvite(toId))
					{
						if (!byEntity.IsInParty())
						{
							Party.ServerHandleAcceptInvite(toEntity, byEntity);
						}
						else if (!toEntity.IsInParty())
						{
							Party.ServerHandleAcceptInvite(byEntity, toEntity);
						}
					}
					else if (!toEntity.IsInParty())
					{
						toEntity.AddPartyInvite(byId);
						connection.SendPackage(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, byId, toId));
					}

					return false;
				case NetPackagePartyActions.PartyActions.AcceptInvite:
					if (toEntity.Party == null)
					{
						Party.ServerHandleAcceptInvite(byEntity, toEntity);
					}

					return false;
				case NetPackagePartyActions.PartyActions.ChangeLead:
					Party.ServerHandleChangeLead(toEntity);
					return false;
				case NetPackagePartyActions.PartyActions.LeaveParty:
					if (toEntity.Party != null)
					{
						Party.ServerHandleLeaveParty(toEntity, toId);
					}

					return false;
				case NetPackagePartyActions.PartyActions.KickFromParty:
					if (toEntity.Party != null)
					{
						Party.ServerHandleKickParty(toId);
					}

					return false;
				default:
					return true;
			}
		}
	}
}
