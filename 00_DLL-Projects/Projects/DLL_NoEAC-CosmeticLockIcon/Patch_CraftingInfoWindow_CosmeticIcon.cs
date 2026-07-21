using HarmonyLib;

[HarmonyPatch(typeof(XUiC_CraftingInfoWindow), "GetBindingValueInternal")]
public class Patch_CraftingInfoWindow_CosmeticIcon
{
    static bool Prefix(ref bool __result, ref string value, string bindingName, XUiC_CraftingInfoWindow __instance)
    {
        bool isIconBinding = bindingName == "itemtypeicon";
        bool isTintBinding = bindingName == "itemtypeicontint" || bindingName == "altitemtypeiconcolor";
        if (!isIconBinding && !isTintBinding)
        {
            return true;
        }

        if (__instance.recipe != null)
        {
            var itemClass = ItemClass.GetForId(__instance.recipe.itemValueType);
            var player = CosmeticLockIconUiHelpers.GetEntityPlayerLocal(__instance);
            if (itemClass is ItemClassArmor itemClassArmor && itemClassArmor.IsCosmetic && player != null && !ArmorIconUIHarmonyPatches.HasMagnitudeIndicator(itemClassArmor))
            {
                if (ArmorIconUIHarmonyPatches.IsCosmeticUnlocked(player, itemClassArmor))
                {
                    if (isIconBinding)
                    {
                        value = itemClass.AltItemTypeIcon;
                        __result = true;
                        return false;
                    }
                    if (isTintBinding)
                    {
                        value = __instance.altitemtypeiconcolorFormatter.Format(itemClassArmor.AltItemTypeIconColor);
                        __result = true;
                        return false;
                    }
                }
            }
        }
        // Let vanilla handle all other cases
        return true;
    }
}
