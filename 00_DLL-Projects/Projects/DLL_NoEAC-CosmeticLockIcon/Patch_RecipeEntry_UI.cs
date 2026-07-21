using HarmonyLib;

[HarmonyPatch(typeof(XUiC_RecipeEntry), "GetBindingValueInternal")]
public class Patch_RecipeEntry_UI
{
	private static bool Prefix(ref bool __result, XUiC_RecipeEntry __instance, ref string value, string bindingName)
	{
		bool isIconBinding = bindingName == "itemtypeicon" || bindingName == "altitemtypeicon";
		bool isTintBinding = bindingName == "itemtypeicontint" || bindingName == "altitemtypeiconcolor";
		if (!isIconBinding && !isTintBinding)
		{
			return true;
		}

		Recipe recipe = __instance.recipe;
		if (recipe == null)
		{
			return true;
		}
		ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
		EntityPlayerLocal entityPlayerLocal = CosmeticLockIconUiHelpers.GetEntityPlayerLocal(__instance);
		string icon;
		if (isIconBinding && ArmorIconUIHarmonyPatches.TryGetCosmeticArmorIcon(forId, entityPlayerLocal, bindingName, out icon))
		{
			value = icon;
			__result = true;
			return false;
		}
		ItemClassArmor itemClassArmor = forId as ItemClassArmor;
		if (isTintBinding && itemClassArmor != null && itemClassArmor.IsCosmetic && entityPlayerLocal != null && !ArmorIconUIHarmonyPatches.HasMagnitudeIndicator(itemClassArmor))
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
