using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	internal static class DoomVehicleGuard
	{
		internal static bool InLevel(EntityPlayer player)
		{
			if (player == null)
			{
				return false;
			}

			if (Instances.IsInside(player.entityId) || Instances.IsInstanceSpace(player.position))
			{
				return true;
			}

			return player.Buffs != null && player.Buffs.HasBuff("doomBuffInLevel");
		}

		internal static void LeaveVehicleBehind(EntityPlayer player)
		{
			if (player == null)
			{
				return;
			}

			Entity attached = player.AttachedToEntity;
			if (attached == null)
			{
				return;
			}

			bool dump = Instances.IsInstanceSpace(attached.position) || Instances.IsInside(player.entityId);
			Vector3 keep = attached.position;
			Vector3 rot = attached.rotation;
			player.Detach();
			if (dump)
			{
				World world = GameManager.Instance?.World;
				if (world != null)
				{
					world.RemoveEntity(attached.entityId, EnumRemoveEntityReason.Unloaded);
				}

				return;
			}

			attached.SetPosition(keep, true);
			attached.SetRotation(rot);
		}
	}

	[HarmonyPatch(typeof(ItemActionSpawnVehicle), nameof(ItemActionSpawnVehicle.ExecuteAction))]
	internal static class Patch_NoVehiclePlaceInLevel
	{
		private static bool Prefix(ItemActionData _actionData)
		{
			EntityPlayer player = ItemInventoryAccess.Holding(_actionData?.invData) as EntityPlayer;
			if (!DoomVehicleGuard.InLevel(player))
			{
				return true;
			}

			EntityPlayerLocal local = player as EntityPlayerLocal;
			if (local != null)
			{
				GameManager.ShowTooltip(local, "Vehicles cannot be used in a Doom level");
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(Instances), nameof(Instances.Teleport))]
	internal static class Patch_TeleportLeaveVehicle
	{
		private static void Prefix(EntityPlayer player)
		{
			DoomVehicleGuard.LeaveVehicleBehind(player);
		}
	}

	[HarmonyPatch(typeof(Instances), nameof(Instances.TeleportInLevel))]
	internal static class Patch_TeleportInLevelLeaveVehicle
	{
		private static void Prefix(EntityPlayer player)
		{
			DoomVehicleGuard.LeaveVehicleBehind(player);
		}
	}
}
