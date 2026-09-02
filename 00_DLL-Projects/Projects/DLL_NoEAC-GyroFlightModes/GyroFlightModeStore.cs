using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace GyroFlightModes
{
	internal static class GyroFlightModeStore
	{
		private static readonly Dictionary<int, GyroFlightMode> Modes = new Dictionary<int, GyroFlightMode>();
		private static readonly FieldInfo ActivationCommandsField = AccessTools.Field(typeof(Entity), "activationCommands");

		internal static bool TryGet(int entityId, out GyroFlightMode mode)
		{
			return Modes.TryGetValue(entityId, out mode);
		}

		internal static void Set(int entityId, GyroFlightMode mode)
		{
			Modes[entityId] = mode;
		}

		internal static void InvalidateActivationCache(EntityVehicle vehicle)
		{
			if (vehicle == null || ActivationCommandsField == null)
			{
				return;
			}

			ActivationCommandsField.SetValue(vehicle, null);
		}
	}
}
