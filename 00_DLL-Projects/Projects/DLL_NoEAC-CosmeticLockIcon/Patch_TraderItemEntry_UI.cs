using HarmonyLib;

[HarmonyPatch(typeof(XUiC_TraderItemEntry), "GetBindingValueInternal")]
public class Patch_TraderItemEntry_UI
{
	private static bool Prefix(ref bool __result, XUiC_TraderItemEntry __instance, ref string value, string bindingName)
	{
		bool isIconBinding = bindingName == "itemtypeicon" || bindingName == "altitemtypeicon";
		bool isTintBinding = bindingName == "itemtypeicontint" || bindingName == "altitemtypeiconcolor";
		if (!isIconBinding && !isTintBinding)
		{
			return true;
		}

		ItemStack item = __instance.item;
		if (item == null || item.IsEmpty())
		{
			return true;
		}
		ItemClass itemClass = item.itemValue.ItemClass;
		EntityPlayerLocal entityPlayerLocal = CosmeticLockIconUiHelpers.GetEntityPlayerLocal(__instance);
		string icon;
		if (isIconBinding && ArmorIconUIHarmonyPatches.TryGetCosmeticArmorIcon(itemClass, entityPlayerLocal, bindingName, out icon, item.itemValue))
		{
			value = icon;
			__result = true;
			return false;
		}
		ItemClassArmor itemClassArmor = itemClass as ItemClassArmor;
		if (isTintBinding && itemClassArmor != null && itemClassArmor.IsCosmetic && entityPlayerLocal != null && !ArmorIconUIHarmonyPatches.HasMagnitudeIndicator(itemClassArmor, item.itemValue))
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
