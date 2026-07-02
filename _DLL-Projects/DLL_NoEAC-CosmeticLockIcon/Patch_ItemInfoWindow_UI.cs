using HarmonyLib;

[HarmonyPatch(typeof(XUiC_ItemInfoWindow), "GetBindingValueInternal")]
public class Patch_ItemInfoWindow_UI
{
	private static bool Prefix(ref bool __result, XUiC_ItemInfoWindow __instance, ref string value, string bindingName)
	{
		bool isIconBinding = bindingName == "itemtypeicon" || bindingName == "altitemtypeicon";
		bool isTintBinding = bindingName == "itemtypeicontint" || bindingName == "altitemtypeiconcolor";
		if (!isIconBinding && !isTintBinding)
		{
			return true;
		}

		ItemClass itemClass = __instance.itemClass;
		ItemStack itemStack = __instance.itemStack;
		ItemValue itemValue = ((itemStack == null || itemStack.IsEmpty()) ? null : itemStack.itemValue);
		EntityPlayerLocal entityPlayerLocal = CosmeticLockIconUiHelpers.GetEntityPlayerLocal(__instance);
		string icon;
		if (isIconBinding && ArmorIconUIHarmonyPatches.TryGetCosmeticArmorIcon(itemClass, entityPlayerLocal, bindingName, out icon, itemValue))
		{
			value = icon;
			__result = true;
			return false;
		}
		ItemClassArmor itemClassArmor = itemClass as ItemClassArmor;
		if (isTintBinding && itemClassArmor != null && itemClassArmor.IsCosmetic && entityPlayerLocal != null && !ArmorIconUIHarmonyPatches.HasMagnitudeIndicator(itemClassArmor, itemValue))
		{
			if (ArmorIconUIHarmonyPatches.IsCosmeticUnlocked(entityPlayerLocal, itemClassArmor))
			{
				value = __instance.altitemtypeiconcolorFormatter.Format(itemClassArmor.AltItemTypeIconColor);
				__result = true;
				return false;
			}
		}
		return true;
	}
}
