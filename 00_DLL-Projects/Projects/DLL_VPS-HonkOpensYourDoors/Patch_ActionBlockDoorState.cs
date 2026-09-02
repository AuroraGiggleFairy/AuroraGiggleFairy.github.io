using GameEvent.SequenceActions;
using HarmonyLib;
using Platform;
using UnityEngine;

namespace HonkOpensYourDoors
{
	[HarmonyPatch(typeof(ActionBlockDoorState), "UpdateBlock")]
	internal static class Patch_ActionBlockDoorState_UpdateBlock
	{
		private const string HonkSequenceName = "honk_trader_doors";

		public static bool Prefix(
			ActionBlockDoorState __instance,
			World world,
			Vector3i currentPos,
			BlockValue blockValue,
			bool ___traderOnly,
			ref BlockChangeInfo __result)
		{
			try
			{
				GameEventActionSequence sequence = __instance.Owner;
				if (sequence == null || sequence.Name != HonkSequenceName || ___traderOnly)
				{
					return true;
				}

				EntityPlayer honker = sequence.Requester ?? (sequence.Target as EntityPlayer);
				if (!IsHonkablePlayerDoor(world, currentPos, blockValue, honker))
				{
					__result = null;
					return false;
				}

				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError("[HonkOpensYourDoors] UpdateBlock prefix failed: " + ex);
				__result = null;
				return false;
			}
		}

		private static bool IsHonkablePlayerDoor(
			World world,
			Vector3i currentPos,
			BlockValue blockValue,
			EntityPlayer honker)
		{
			if (blockValue.isair || blockValue.Block is BlockPoweredDoor)
			{
				return false;
			}

			TileEntityComposite tileEntity = world.GetTileEntity(currentPos) as TileEntityComposite;
			if (tileEntity == null || !tileEntity.PlayerPlaced)
			{
				return false;
			}

			if (!tileEntity.TryGetSelfOrFeature<TEFeatureDoor>(out _))
			{
				return false;
			}

			if (!tileEntity.TryGetSelfOrFeature<TEFeatureLockable>(out TEFeatureLockable lockable)
				|| !lockable.IsLocked())
			{
				return true;
			}

			return PlayerHasLockAccess(lockable, honker);
		}

		private static bool PlayerHasLockAccess(TEFeatureLockable lockable, EntityPlayer honker)
		{
			if (honker == null)
			{
				return false;
			}

			PlatformUserIdentifierAbs playerId = GameManager.Instance?.persistentPlayers
				?.GetPlayerDataFromEntityID(honker.entityId)?.PrimaryId;
			if (playerId == null)
			{
				return false;
			}

			return lockable.IsUserAllowed(playerId) || lockable.IsOwner(playerId);
		}
	}
}
