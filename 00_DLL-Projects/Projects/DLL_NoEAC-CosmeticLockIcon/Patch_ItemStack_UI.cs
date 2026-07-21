using HarmonyLib;

[HarmonyPatch(typeof(XUiC_ItemStack), "GetBindingValueInternal")]
public class Patch_ItemStack_UI
{
	private static bool Prefix(ref bool __result, XUiC_ItemStack __instance, ref string _value, string _bindingName)
	{
		bool isIconBinding = _bindingName == "itemtypeicon" || _bindingName == "altitemtypeicon";
		bool isTintBinding = _bindingName == "itemtypeicontint" || _bindingName == "altitemtypeiconcolor";
		if (!isIconBinding && !isTintBinding)
		{
			return true;
		}

		ItemClass itemClassOrMissing = __instance.itemClassOrMissing;
		ItemStack itemStack = __instance.ItemStack;
		ItemValue itemValue = ((itemStack == null || itemStack.IsEmpty()) ? null : itemStack.itemValue);
		EntityPlayerLocal entityPlayerLocal = CosmeticLockIconUiHelpers.GetEntityPlayerLocal(__instance);
		string icon;
		if (isIconBinding && ArmorIconUIHarmonyPatches.TryGetCosmeticArmorIcon(itemClassOrMissing, entityPlayerLocal, _bindingName, out icon, itemValue))
		{
			_value = icon;
			__result = true;
			return false;
		}
		ItemClassArmor itemClassArmor = itemClassOrMissing as ItemClassArmor;
		if (isTintBinding && itemClassArmor != null && itemClassArmor.IsCosmetic && entityPlayerLocal != null && !ArmorIconUIHarmonyPatches.HasMagnitudeIndicator(itemClassArmor, itemValue))
		{
			if (ArmorIconUIHarmonyPatches.IsCosmeticUnlocked(entityPlayerLocal, itemClassArmor))
			{
				_value = __instance.altitemtypeiconcolorFormatter.Format(itemClassArmor.AltItemTypeIconColor);
				__result = true;
				return false;
			}
		}
		return true;
	}
}
