using HarmonyLib;

namespace GyroFlightModes
{
	[HarmonyPatch(typeof(XUiC_Radial), "SelectionEffect")]
	internal static class Patch_XUiC_Radial_SelectionEffect
	{
		public static void Postfix(XUiC_Radial __instance, XUiC_RadialEntry _entry, bool _selected)
		{
			XUiController sub = __instance.GetChildById("selectionSub");
			XUiV_Label subLabel = sub?.ViewComponent as XUiV_Label;
			if (subLabel == null)
			{
				return;
			}

			if (!_selected || _entry == null)
			{
				subLabel.Text = string.Empty;
				return;
			}

			XUiC_Radial.RadialContextEntity entityCtx = __instance.context as XUiC_Radial.RadialContextEntity;
			if (entityCtx?.Commands == null
				|| _entry.CommandIndex < 0
				|| _entry.CommandIndex >= entityCtx.Commands.Length
				|| entityCtx.Commands[_entry.CommandIndex].commandId != GyroFlightModesApi.CommandId)
			{
				subLabel.Text = string.Empty;
				return;
			}

			EntityVehicle vehicle = entityCtx.EntityFocused as EntityVehicle;
			if (!GyroFlightModesApi.IsGyro(vehicle))
			{
				subLabel.Text = string.Empty;
				return;
			}

			subLabel.Text = GyroFlightModesApi.GetModeDisplayName(GyroFlightModesApi.ReadMode(vehicle));
		}
	}
}
