using System;
using Platform;

namespace GyroFlightModes
{
	/// <summary>
	/// Public query surface for other AGF mods (Flight Assist). Resolve by type name:
	/// GyroFlightModes.GyroFlightModesApi
	/// </summary>
	public static class GyroFlightModesApi
	{
		public const string CommandId = "agf_flightmode";
		public const string CommandLocId = "agfFlightMode";
		public const string MetadataKey = "agfGyroFlightMode";
		public const string GyroVehicleName = "vehiclegyrocopter";
		public const string NavIconOriginal = "ui_game_symbol_gyrocopter";
		public const string NavIconHeli = "ui_game_symbol_drone";
		public const string RadialIconOriginal = "gyrocopter";
		public const string RadialIconHeli = "drone";

		public static event Action<EntityVehicle, GyroFlightMode> ModeChanged;

		public static bool IsGyro(EntityVehicle vehicle)
		{
			if (vehicle == null)
			{
				return false;
			}

			Vehicle data = vehicle.GetVehicle();
			if (data == null)
			{
				return false;
			}

			string name = data.GetName();
			return !string.IsNullOrEmpty(name) && string.Equals(name, GyroVehicleName, StringComparison.OrdinalIgnoreCase);
		}

		public static bool TryGetFlightMode(EntityVehicle vehicle, out GyroFlightMode mode)
		{
			mode = GyroFlightMode.Original;
			if (!IsGyro(vehicle))
			{
				return false;
			}

			mode = ReadMode(vehicle);
			return true;
		}

		/// <summary>
		/// Icon and localized mode name for an interaction-prompt pill.
		/// Other mods resolve this by name: GyroFlightModes.GyroFlightModesApi.TryGetFlightModePrompt
		/// </summary>
		public static bool TryGetFlightModePrompt(EntityVehicle vehicle, out string icon, out string displayName)
		{
			icon = null;
			displayName = null;
			if (!TryGetFlightMode(vehicle, out GyroFlightMode mode))
			{
				return false;
			}

			icon = GetNavIcon(mode);
			displayName = GetModeDisplayName(mode);
			return !string.IsNullOrEmpty(icon) && !string.IsNullOrEmpty(displayName);
		}

		internal static GyroFlightMode ReadMode(EntityVehicle vehicle)
		{
			if (vehicle != null && GyroFlightModeStore.TryGet(vehicle.entityId, out GyroFlightMode live))
			{
				return live;
			}

			return ReadPersistedMode(vehicle);
		}

		internal static GyroFlightMode ReadPersistedMode(EntityVehicle vehicle)
		{
			Vehicle data = vehicle?.GetVehicle();
			ItemValue itemValue = data?.itemValue;
			if (itemValue != null && itemValue.TryGetMetadata(MetadataKey, out int stored))
			{
				return stored == (int)GyroFlightMode.Heli ? GyroFlightMode.Heli : GyroFlightMode.Original;
			}

			return GyroFlightMode.Original;
		}

		internal static void WriteMode(EntityVehicle vehicle, GyroFlightMode mode)
		{
			if (vehicle != null)
			{
				GyroFlightModeStore.Set(vehicle.entityId, mode);
			}

			Vehicle data = vehicle?.GetVehicle();
			if (data?.itemValue == null)
			{
				return;
			}

			data.itemValue.SetMetadata(MetadataKey, (int)mode);
		}

		internal static bool ToggleMode(EntityVehicle vehicle)
		{
			if (!IsGyro(vehicle))
			{
				return false;
			}

			GyroFlightMode next = ReadMode(vehicle) == GyroFlightMode.Heli ? GyroFlightMode.Original : GyroFlightMode.Heli;
			SetMode(vehicle, next);
			return true;
		}

		internal static void SetMode(EntityVehicle vehicle, GyroFlightMode mode)
		{
			if (!IsGyro(vehicle))
			{
				return;
			}

			WriteMode(vehicle, mode);
			GyroControlApplier.Apply(vehicle, mode);
			GyroFlightModeStore.InvalidateActivationCache(vehicle);
			ModeChanged?.Invoke(vehicle, mode);
			Console.WriteLine("[GyroFlightModes] entity " + vehicle.entityId + " -> " + mode);
		}

		internal static void ApplySavedMode(EntityVehicle vehicle)
		{
			if (!IsGyro(vehicle))
			{
				return;
			}

			GyroFlightMode mode = ReadPersistedMode(vehicle);
			GyroFlightModeStore.Set(vehicle.entityId, mode);
			GyroControlApplier.Apply(vehicle, mode);
		}

		internal static string GetRadialIcon(GyroFlightMode mode)
		{
			return mode == GyroFlightMode.Heli ? RadialIconHeli : RadialIconOriginal;
		}

		internal static string GetNavIcon(GyroFlightMode mode)
		{
			return mode == GyroFlightMode.Heli ? NavIconHeli : NavIconOriginal;
		}

		internal static string GetModeLocKey(GyroFlightMode mode)
		{
			return mode == GyroFlightMode.Heli ? "xuiAGFFlightModeHeli" : "xuiAGFFlightModeOriginal";
		}

		internal static string GetModeDisplayName(GyroFlightMode mode)
		{
			return Localization.Get(GetModeLocKey(mode));
		}

		internal static bool CanLocalPlayerSwitch(EntityVehicle vehicle)
		{
			return IsGyro(vehicle) && VehicleAccess.CanPlayerAccess(vehicle, PlatformManager.InternalLocalUserIdentifier);
		}
	}
}
