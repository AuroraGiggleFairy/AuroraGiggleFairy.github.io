using HarmonyLib;
using System;

public static class CosmeticArmorUnlockUtils
{
    // Replace this with your actual logic for checking if the cosmetic armor is unlocked for the player
    public static bool IsCosmeticArmorUnlocked(ItemClass itemClass, EntityPlayer player)
    {
        if (!(itemClass is ItemClassArmor itemClassArmor))
            return false;
        if (!itemClassArmor.IsCosmetic || player == null)
            return false;
        // Use the same logic as other UI patches
        var equipment = player.equipment;
        if (equipment == null)
            return false;
        var result = equipment.HasCosmeticUnlocked(itemClassArmor);
        return result is { Item1: true };
    }
}

[HarmonyPatch(typeof(XUiC_CraftingInfoWindow), "GetBindingValueInternal")]
public class Patch_CraftingInfoWindow_CosmeticIcon
{
    static bool Prefix(ref bool __result, ref string value, string bindingName, XUiC_CraftingInfoWindow __instance)
    {
        if (__instance.recipe != null)
        {
            var itemClass = ItemClass.GetForId(__instance.recipe.itemValueType);
            var player = __instance.xui.playerUI.entityPlayer;
            if (itemClass is ItemClassArmor itemClassArmor && itemClassArmor.IsCosmetic && player != null)
            {
                var equipment = player.equipment;
                var tuple = equipment?.HasCosmeticUnlocked(itemClassArmor);
                if (tuple.HasValue && tuple.Value.Item1)
                {
                    if (bindingName == "itemtypeicon")
                    {
                        value = itemClass.AltItemTypeIcon;
                        __result = true;
                        return false;
                    }
                    if (bindingName == "itemtypeicontint" || bindingName == "altitemtypeiconcolor")
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
