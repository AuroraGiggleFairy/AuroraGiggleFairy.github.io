using HarmonyLib;

[HarmonyPatch(typeof(XUiC_ItemStack), "GetBindingValueInternal")]
public class Patch_ItemStack_UI
{
	private static bool Prefix(ref bool __result, XUiC_ItemStack __instance, ref string _value, string _bindingName)
	{
		ItemClass itemClassOrMissing = __instance.itemClassOrMissing;
		EntityPlayerLocal entityPlayerLocal = __instance.xui?.playerUI?.entityPlayer;
		if ((_bindingName == "itemtypeicon" || _bindingName == "altitemtypeicon") && ArmorIconUIHarmonyPatches.TryGetCosmeticArmorIcon(itemClassOrMissing, entityPlayerLocal, _bindingName, out var icon))
		{
			_value = icon;
			__result = true;
			return false;
		}
		if ((_bindingName == "itemtypeicontint" || _bindingName == "altitemtypeiconcolor") && itemClassOrMissing is ItemClassArmor { IsCosmetic: not false } itemClassArmor && entityPlayerLocal != null)
		{
			(bool, EntitlementSetEnum)? tuple = entityPlayerLocal.equipment?.HasCosmeticUnlocked(itemClassArmor);
			if (tuple.HasValue && tuple.Value.Item1)
			{
				_value = __instance.altitemtypeiconcolorFormatter.Format(itemClassArmor.AltItemTypeIconColor);
				__result = true;
				return false;
			}
		}
		return true;
	}
}
