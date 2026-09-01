namespace SortingNearbyPlus
{
	internal static class VehicleAccess
	{
		public static bool TryGetBag(EntityVehicle vehicle, out Bag bag)
		{
			bag = null;
			if (vehicle == null)
			{
				return false;
			}

			Vehicle data = vehicle.GetVehicle();
			if (data == null || !data.HasStorage())
			{
				return false;
			}

			bag = vehicle.bag;
			return bag != null && bag.SlotCount > 0;
		}

		public static bool IsOwnedBy(EntityVehicle vehicle, PlatformUserIdentifierAbs playerId)
		{
			if (vehicle == null || playerId == null)
			{
				return false;
			}

			PlatformUserIdentifierAbs owner = vehicle.GetOwner();
			return owner != null && owner.Equals(playerId);
		}

		public static bool CanPlayerAccess(EntityVehicle vehicle, PlatformUserIdentifierAbs playerId)
		{
			if (vehicle == null)
			{
				return false;
			}

			if (!vehicle.IsLocked())
			{
				return true;
			}

			if (playerId == null)
			{
				return false;
			}

			return vehicle.IsOwner(playerId) || vehicle.IsUserAllowed(playerId);
		}
	}
}
