using Platform;

namespace GyroFlightModes
{
	internal static class VehicleAccess
	{
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
