using Audio;
using HarmonyLib;

namespace GyroFlightModes
{
	[HarmonyPatch(typeof(EntityVehicle), "SetupDevices")]
	internal static class Patch_EntityVehicle_SetupDevices
	{
		public static void Postfix(EntityVehicle __instance)
		{
			if (!GyroFlightModesApi.IsGyro(__instance))
			{
				return;
			}

			GyroControlApplier.CaptureOriginalFromXml(__instance);
			GyroControlApplier.Apply(__instance, GyroFlightModesApi.ReadPersistedMode(__instance));
		}
	}

	[HarmonyPatch(typeof(EntityVehicle), nameof(EntityVehicle.Read))]
	internal static class Patch_EntityVehicle_Read
	{
		public static void Postfix(EntityVehicle __instance)
		{
			GyroFlightModesApi.ApplySavedMode(__instance);
		}
	}

	[HarmonyPatch(typeof(EntityVehicle), nameof(EntityVehicle.PostInit))]
	internal static class Patch_EntityVehicle_PostInit
	{
		public static void Postfix(EntityVehicle __instance)
		{
			GyroFlightModesApi.ApplySavedMode(__instance);
		}
	}

	[HarmonyPatch(typeof(EntityVehicle), "InitLocalActivationCommands")]
	internal static class Patch_EntityVehicle_InitLocalActivationCommands
	{
		public static void Postfix(EntityVehicle __instance, System.Action<EntityActivationCommand> _addCallback)
		{
			if (_addCallback == null || !GyroFlightModesApi.IsGyro(__instance))
			{
				return;
			}

			string icon = GyroFlightModesApi.GetRadialIcon(GyroFlightModesApi.ReadMode(__instance));
			_addCallback(new EntityActivationCommand(GyroFlightModesApi.CommandId, icon, null, GyroFlightModesApi.CommandLocId));
		}
	}

	[HarmonyPatch(typeof(Entity), nameof(Entity.ActivateEntityCommand))]
	internal static class Patch_Entity_ActivateEntityCommand
	{
		public static void Postfix(Entity __instance, EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
		{
			if (!(__instance is EntityVehicle vehicle))
			{
				return;
			}

			if (_command.commandId != GyroFlightModesApi.CommandId)
			{
				return;
			}

			if (!GyroFlightModesApi.CanLocalPlayerSwitch(vehicle))
			{
				return;
			}

			GyroFlightModesApi.ToggleMode(vehicle);
			Notify(_playerFocusing, vehicle);
		}

		private static void Notify(EntityPlayerLocal player, EntityVehicle vehicle)
		{
			string title = Localization.Get("xuiAGFFlightMode");
			string modeName = GyroFlightModesApi.GetModeDisplayName(GyroFlightModesApi.ReadMode(vehicle));
			if (player != null)
			{
				GameManager.ShowTooltip(player, title + ": " + modeName);
			}

			Manager.PlayInsidePlayerHead("misc/unlocking");
		}
	}

	[HarmonyPatch(typeof(Entity), nameof(Entity.UpdateActivationCommands))]
	internal static class Patch_Entity_UpdateActivationCommands
	{
		public static void Postfix(Entity __instance, EntityPlayerLocal _playerFocusing)
		{
			if (!(__instance is EntityVehicle vehicle) || !GyroFlightModesApi.IsGyro(vehicle))
			{
				return;
			}

			EntityActivationCommand[] commands = vehicle.GetActivationCommands();
			if (commands == null)
			{
				return;
			}

			bool allowed = GyroFlightModesApi.CanLocalPlayerSwitch(vehicle);
			string icon = GyroFlightModesApi.GetRadialIcon(GyroFlightModesApi.ReadMode(vehicle));
			for (int i = 0; i < commands.Length; i++)
			{
				if (commands[i].commandId != GyroFlightModesApi.CommandId)
				{
					continue;
				}

				EntityActivationCommand command = commands[i];
				command.enabled = allowed;
				command.icon = icon;
				commands[i] = command;
				break;
			}
		}
	}
}
