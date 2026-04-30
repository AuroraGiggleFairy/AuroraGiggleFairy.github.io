using HarmonyLib;

[HarmonyPatch(typeof(XUiC_ItemInfoWindow), "GetBindingValueInternal")]
public class Patch_ItemInfoWindow_UI
{
	private static bool Prefix(ref bool __result, XUiC_ItemInfoWindow __instance, ref string value, string bindingName)
	{
		ItemClass itemClass = __instance.itemClass;
		EntityPlayerLocal entityPlayerLocal = __instance.xui?.playerUI?.entityPlayer;
		if ((bindingName == "itemtypeicon" || bindingName == "altitemtypeicon") && ArmorIconUIHarmonyPatches.TryGetCosmeticArmorIcon(itemClass, entityPlayerLocal, bindingName, out var icon))
		{
			value = icon;
			__result = true;
			return false;
		}
		if ((bindingName == "itemtypeicontint" || bindingName == "altitemtypeiconcolor") && itemClass is ItemClassArmor { IsCosmetic: not false } itemClassArmor && entityPlayerLocal != null)
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
