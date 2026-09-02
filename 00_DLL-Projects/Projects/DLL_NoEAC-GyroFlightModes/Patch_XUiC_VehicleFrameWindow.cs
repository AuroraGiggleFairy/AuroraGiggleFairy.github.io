using Audio;
using HarmonyLib;

namespace GyroFlightModes
{
	[HarmonyPatch(typeof(XUiC_VehicleFrameWindow), nameof(XUiC_VehicleFrameWindow.Init))]
	internal static class Patch_XUiC_VehicleFrameWindow_Init
	{
		public static void Postfix(XUiC_VehicleFrameWindow __instance)
		{
			XUiC_SimpleButton btnOriginal = __instance.GetChildById("btnFlightModeOriginal") as XUiC_SimpleButton;
			XUiC_SimpleButton btnHeli = __instance.GetChildById("btnFlightModeHeli") as XUiC_SimpleButton;
			if (btnOriginal == null || btnHeli == null)
			{
				return;
			}

			btnOriginal.OnPressed += (_, __) => OnModePressed(__instance, GyroFlightMode.Original, btnOriginal, btnHeli);
			btnHeli.OnPressed += (_, __) => OnModePressed(__instance, GyroFlightMode.Heli, btnOriginal, btnHeli);
		}

		private static void OnModePressed(XUiC_VehicleFrameWindow window, GyroFlightMode mode, XUiC_SimpleButton btnOriginal, XUiC_SimpleButton btnHeli)
		{
			EntityVehicle vehicle = window.Vehicle;
			if (!GyroFlightModesApi.CanLocalPlayerSwitch(vehicle))
			{
				return;
			}

			bool alreadyMode = GyroFlightModesApi.ReadMode(vehicle) == mode;
			GyroFlightModesApi.SetMode(vehicle, mode);
			RefreshSelected(btnOriginal, btnHeli, mode);
			window.RefreshBindings();
			window.IsDirty = true;
			if (alreadyMode)
			{
				return;
			}

			Manager.PlayInsidePlayerHead("misc/unlocking");
			EntityPlayerLocal player = window.xui?.playerUI?.entityPlayer;
			if (player != null)
			{
				GameManager.ShowTooltip(player, Localization.Get("xuiAGFFlightMode") + ": " + GyroFlightModesApi.GetModeDisplayName(mode));
			}
		}

		internal static void RefreshWindowButtons(XUiC_VehicleFrameWindow window)
		{
			if (window == null)
			{
				return;
			}

			XUiC_SimpleButton btnOriginal = window.GetChildById("btnFlightModeOriginal") as XUiC_SimpleButton;
			XUiC_SimpleButton btnHeli = window.GetChildById("btnFlightModeHeli") as XUiC_SimpleButton;
			EntityVehicle vehicle = window.Vehicle;
			if (!GyroFlightModesApi.IsGyro(vehicle))
			{
				RefreshSelected(btnOriginal, btnHeli, GyroFlightMode.Original);
				return;
			}

			RefreshSelected(btnOriginal, btnHeli, GyroFlightModesApi.ReadMode(vehicle));
		}

		internal static void RefreshSelected(XUiC_SimpleButton btnOriginal, XUiC_SimpleButton btnHeli, GyroFlightMode mode)
		{
			bool heli = mode == GyroFlightMode.Heli;
			if (btnOriginal?.Button != null)
			{
				btnOriginal.Button.Selected = !heli;
			}

			if (btnHeli?.Button != null)
			{
				btnHeli.Button.Selected = heli;
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_VehicleFrameWindow), nameof(XUiC_VehicleFrameWindow.Vehicle), MethodType.Setter)]
	internal static class Patch_XUiC_VehicleFrameWindow_SetVehicle
	{
		public static void Postfix(XUiC_VehicleFrameWindow __instance)
		{
			Patch_XUiC_VehicleFrameWindow_Init.RefreshWindowButtons(__instance);
		}
	}

	[HarmonyPatch(typeof(XUiC_VehicleFrameWindow), nameof(XUiC_VehicleFrameWindow.OnOpen))]
	internal static class Patch_XUiC_VehicleFrameWindow_OnOpen
	{
		public static void Postfix(XUiC_VehicleFrameWindow __instance)
		{
			Patch_XUiC_VehicleFrameWindow_Init.RefreshWindowButtons(__instance);
		}
	}

	[HarmonyPatch(typeof(XUiC_VehicleFrameWindow), "GetBindingValueInternal")]
	internal static class Patch_XUiC_VehicleFrameWindow_Bindings
	{
		public static bool Prefix(XUiC_VehicleFrameWindow __instance, ref bool __result, ref string value, string bindingName)
		{
			if (bindingName != "showagfflightmode"
				&& bindingName != "agfflightmodeoriginalselected"
				&& bindingName != "agfflightmodeheliselected")
			{
				return true;
			}

			EntityVehicle vehicle = __instance.Vehicle;
			bool isGyro = GyroFlightModesApi.IsGyro(vehicle);
			GyroFlightMode mode = isGyro ? GyroFlightModesApi.ReadMode(vehicle) : GyroFlightMode.Original;

			switch (bindingName)
			{
				case "showagfflightmode":
					value = isGyro ? "true" : "false";
					break;
				case "agfflightmodeoriginalselected":
					value = (isGyro && mode == GyroFlightMode.Original) ? "true" : "false";
					break;
				case "agfflightmodeheliselected":
					value = (isGyro && mode == GyroFlightMode.Heli) ? "true" : "false";
					break;
			}

			__result = true;
			return false;
		}
	}
}
