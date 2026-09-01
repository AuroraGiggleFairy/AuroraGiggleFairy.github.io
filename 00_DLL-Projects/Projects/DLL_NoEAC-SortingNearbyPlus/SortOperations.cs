using System.Collections.Generic;
using UnityEngine;

namespace SortingNearbyPlus
{
	internal static class SortOperations
	{
		public static int SortPlayerToChests(EntityPlayer player)
		{
			Bag bag = StorageUtil.GetPlayerBag(player);
			if (player == null || bag == null)
			{
				return 0;
			}

			World world = GameManager.Instance.World;
			Vector3i origin = new Vector3i(player.position);
			if (!NearbyScanner.TryGetBounds(origin, out Vector3i min, out Vector3i max))
			{
				return 0;
			}

			PlatformUserIdentifierAbs playerId = StorageUtil.GetPlayerId(player);
			List<NearbyScanner.ChestTarget> chests = NearbyScanner.FindPlayerChests(world, origin, min, max);
			List<NearbyScanner.ChestTarget> preferred = new List<NearbyScanner.ChestTarget>();
			List<NearbyScanner.ChestTarget> others = new List<NearbyScanner.ChestTarget>();
			foreach (NearbyScanner.ChestTarget chest in chests)
			{
				if (!StorageUtil.TryAsLock(chest.TileEntity, out ILockable lockable) || !lockable.IsLocked())
				{
					others.Add(chest);
					continue;
				}

				if (StorageUtil.CanPlayerOpenLock(lockable, playerId))
				{
					preferred.Add(chest);
				}
			}

			int moved = 0;
			moved += MoveBagAndOwnedVehicles(player, bag, playerId, world, min, max, preferred);
			moved += MoveBagAndOwnedVehicles(player, bag, playerId, world, min, max, others);
			return moved;
		}

		public static int SortBagToVehicles(EntityPlayer player, bool likeItemsOnly)
		{
			Bag bag = StorageUtil.GetPlayerBag(player);
			if (player == null || bag == null)
			{
				return 0;
			}

			World world = GameManager.Instance.World;
			Vector3i origin = new Vector3i(player.position);
			if (!NearbyScanner.TryGetBounds(origin, out Vector3i min, out Vector3i max))
			{
				return 0;
			}

			PlatformUserIdentifierAbs playerId = StorageUtil.GetPlayerId(player);
			List<EntityVehicle> vehicles = NearbyScanner.FindVehicles(world, player.position, min, max);
			List<EntityVehicle> owned = new List<EntityVehicle>();
			List<EntityVehicle> others = new List<EntityVehicle>();
			foreach (EntityVehicle vehicle in vehicles)
			{
				if (!VehicleAccess.CanPlayerAccess(vehicle, playerId) || !VehicleAccess.TryGetBag(vehicle, out _))
				{
					continue;
				}

				if (VehicleAccess.IsOwnedBy(vehicle, playerId))
				{
					owned.Add(vehicle);
				}
				else
				{
					others.Add(vehicle);
				}
			}

			int moved = 0;
			moved += MoveBagToVehicles(bag, owned, likeItemsOnly);
			moved += MoveBagToVehicles(bag, others, likeItemsOnly);
			return moved;
		}

		private static int MoveBagToVehicles(Bag bag, List<EntityVehicle> vehicles, bool likeItemsOnly)
		{
			int moved = 0;
			foreach (EntityVehicle vehicle in vehicles)
			{
				if (!VehicleAccess.TryGetBag(vehicle, out Bag dest))
				{
					continue;
				}

				int count = ItemMover.MoveLikeItemsFromBagToBag(bag, dest, likeItemsOnly);
				if (count > 0)
				{
					vehicle.SetBagModified();
					moved += count;
				}
			}

			return moved;
		}

		private static int MoveBagAndOwnedVehicles(
			EntityPlayer player,
			Bag bag,
			PlatformUserIdentifierAbs playerId,
			World world,
			Vector3i min,
			Vector3i max,
			List<NearbyScanner.ChestTarget> chests)
		{
			int moved = 0;
			foreach (NearbyScanner.ChestTarget chest in chests)
			{
				moved += ItemMover.MoveLikeItemsFromBag(bag, chest.Storage, true);
				StorageUtil.MarkModified(chest.TileEntity, chest.Storage);
			}

			foreach (EntityVehicle vehicle in NearbyScanner.FindVehicles(world, player.position, min, max))
			{
				if (!VehicleAccess.IsOwnedBy(vehicle, playerId) || !VehicleAccess.TryGetBag(vehicle, out Bag vehicleBag))
				{
					continue;
				}

				int vehicleMoved = 0;
				foreach (NearbyScanner.ChestTarget chest in chests)
				{
					vehicleMoved += ItemMover.MoveLikeItemsFromBag(vehicleBag, chest.Storage, true);
					StorageUtil.MarkModified(chest.TileEntity, chest.Storage);
				}

				if (vehicleMoved > 0)
				{
					vehicle.SetBagModified();
					moved += vehicleMoved;
				}
			}

			return moved;
		}
	}
}
