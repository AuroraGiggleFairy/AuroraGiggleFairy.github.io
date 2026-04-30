using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace ItemTypeIconColor
{
    [HarmonyPatch(typeof(XUiC_ItemStack), "GetBindingValueInternal")]
    public class XUiC_ItemStack_GetBindingValueInternal_Patch
    {
        static bool Prefix(ref bool __result, XUiC_ItemStack __instance, ref string _value, string _bindingName)
        {
            // ...log removed...
            var itemClass = __instance.itemClassOrMissing;
            // No forced color for main icon
            if (_bindingName == "iconcolor")
            {
                // ...existing code for other colors (optional, can be omitted for pure override)...
            }
            if (_bindingName == "itemtypeicontint" || _bindingName == "locktypeicon")
            {
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null)
                {
                    // Check unlock state using AssemblyCSharp logic
                    bool isUnlocked = false;
                    if (itemClass.Properties.Values.ContainsKey("Unlocks") && __instance.xui != null && __instance.xui.playerUI != null && __instance.xui.playerUI.entityPlayer != null)
                    {
                        var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                        if (!string.IsNullOrEmpty(unlocks))
                        {
                            isUnlocked = XUiM_ItemStack.CheckKnown(__instance.xui.playerUI.entityPlayer, itemClass, __instance.itemStack.itemValue);
                        }
                    }
                    if (isUnlocked && itemClass.Properties.Values.ContainsKey("AltItemTypeIconColor"))
                    {
                        var altColorStr = itemClass.Properties.Values["AltItemTypeIconColor"] as string;
                        if (!string.IsNullOrEmpty(altColorStr))
                        {
                            Color altColor = ItemTypeIconColorPatch.ParseGameColor(altColorStr);
                            _value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(altColor.r * 255), Mathf.RoundToInt(altColor.g * 255), Mathf.RoundToInt(altColor.b * 255), Mathf.RoundToInt(altColor.a * 255));
                            __result = true;
                            return false;
                        }
                    }
                    // Always apply ItemTypeIconColor for locked badge, even if AltItemTypeIcon/Color is missing
                    if (!isUnlocked && itemClass.Properties.Values.ContainsKey("ItemTypeIconColor"))
                    {
                        var colorStr = itemClass.Properties.Values["ItemTypeIconColor"] as string;
                        if (!string.IsNullOrEmpty(colorStr))
                        {
                            Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                            _value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                            __result = true;
                            return false;
                        }
                    }
                }
                // Block context check
                Block block = null;
                var itemStack = __instance.itemStack;
                if (itemStack != null && itemStack.itemValue != null)
                {
                    var itemClassForBlock = ItemClass.GetForId(itemStack.itemValue.type);
                    if (itemClassForBlock != null)
                    {
                        block = itemClassForBlock.GetBlock();
                    }
                }
                if (block != null && block.Properties != null && block.Properties.Values != null)
                {
                    if (block.Properties.Values.ContainsKey("ItemTypeIconColor"))
                    {
                        var colorStr = block.Properties.Values["ItemTypeIconColor"] as string;
                        if (!string.IsNullOrEmpty(colorStr))
                        {
                            Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                            _value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
