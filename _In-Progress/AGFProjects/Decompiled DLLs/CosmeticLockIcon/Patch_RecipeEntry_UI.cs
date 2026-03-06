using HarmonyLib;

[HarmonyPatch(typeof(XUiC_RecipeEntry), "GetBindingValueInternal")]
public class Patch_RecipeEntry_UI
{
	private static bool Prefix(ref bool __result, XUiC_RecipeEntry __instance, ref string value, string bindingName)
	{
		Recipe recipe = __instance.recipe;
		if (recipe == null)
		{
			return true;
		}
		ItemClass forId = ItemClass.GetForId(recipe.itemValueType);
		EntityPlayerLocal entityPlayerLocal = __instance.xui?.playerUI?.entityPlayer;
		if ((bindingName == "itemtypeicon" || bindingName == "altitemtypeicon") && ArmorIconUIHarmonyPatches.TryGetCosmeticArmorIcon(forId, entityPlayerLocal, bindingName, out var icon))
		{
			value = icon;
			__result = true;
			return false;
		}
		if ((bindingName == "itemtypeicontint" || bindingName == "altitemtypeiconcolor") && forId is ItemClassArmor { IsCosmetic: not false } itemClassArmor && entityPlayerLocal != null)
		{
			(bool, EntitlementSetEnum)? tuple = entityPlayerLocal.equipment?.HasCosmeticUnlocked(itemClassArmor);
			if (tuple.HasValue && tuple.Value.Item1)
			{
				value = __instance.altitemtypeiconcolorFormatter.Format(itemClassArmor.AltItemTypeIconColor);
				__result = true;
				return false;
			}
		}
		return true;
	}
}
