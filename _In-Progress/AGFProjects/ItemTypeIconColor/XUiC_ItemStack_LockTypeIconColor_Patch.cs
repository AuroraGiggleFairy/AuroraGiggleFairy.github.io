using HarmonyLib;
using UnityEngine;

namespace ItemTypeIconColor
{
    [HarmonyPatch(typeof(XUiC_ItemStack), "updateLockTypeIcon")]
    public class XUiC_ItemStack_updateLockTypeIcon_Patch
    {
        static void Postfix(XUiC_ItemStack __instance)
        {
            var lockTypeIcon = Traverse.Create(__instance).Field("lockTypeIcon").GetValue() as XUiV_Sprite;
            var itemClass = __instance.itemClass;
            if (lockTypeIcon != null && itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null)
            {
                var itemTypeIconName = itemClass.Properties.Values.ContainsKey("ItemTypeIcon") ? itemClass.Properties.Values["ItemTypeIcon"] as string : null;
                var altItemTypeIconName = itemClass.Properties.Values.ContainsKey("AltItemTypeIcon") ? itemClass.Properties.Values["AltItemTypeIcon"] as string : null;
                // Set LOCKED badge color (ItemTypeIcon)
                if (!string.IsNullOrEmpty(itemTypeIconName) && lockTypeIcon.SpriteName == $"ui_game_symbol_{itemTypeIconName}")
                {
                    if (itemClass.Properties.Values.ContainsKey("ItemTypeIconColor"))
                    {
                        var colorStr = itemClass.Properties.Values["ItemTypeIconColor"] as string;
                        if (!string.IsNullOrEmpty(colorStr))
                        {
                            Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                            lockTypeIcon.Color = color;
                        }
                        else
                        {
                            lockTypeIcon.Color = Color.white;
                        }
                    }
                    else
                    {
                        lockTypeIcon.Color = Color.white;
                    }
                }
                // Set UNLOCKED badge color (AltItemTypeIcon) and ensure override is prevented
                if (!string.IsNullOrEmpty(altItemTypeIconName) && lockTypeIcon.SpriteName == $"ui_game_symbol_{altItemTypeIconName}")
                {
                    // Always reset color to vanilla white before applying AltItemTypeIconColor
                    lockTypeIcon.Color = Color.white;
                    if (itemClass.Properties.Values.ContainsKey("AltItemTypeIconColor"))
                    {
                        var altColorStr = itemClass.Properties.Values["AltItemTypeIconColor"] as string;
                        if (!string.IsNullOrEmpty(altColorStr))
                        {
                            Color altColor = ItemTypeIconColorPatch.ParseGameColor(altColorStr);
                            lockTypeIcon.Color = altColor;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
            }
        }
    }
}
