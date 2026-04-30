using HarmonyLib;
using UnityEngine;

namespace ItemTypeIconColor
{
    // Patch for XUiC_TraderItemEntry
    [HarmonyPatch(typeof(XUiC_TraderItemEntry), "GetBindingValueInternal")]
    public class XUiC_TraderItemEntry_GetBindingValueInternal_Patch
    {
        static bool Prefix(ref bool __result, XUiC_TraderItemEntry __instance, ref string value, string bindingName)
        {
            var itemClass = __instance?.itemClass;
            var item = __instance?.item;
            Block block = null;
            if (item != null && item.itemValue != null)
            {
                var itemClassForBlock = ItemClass.GetForId(item.itemValue.type);
                if (itemClassForBlock != null)
                {
                    block = itemClassForBlock.GetBlock();
                }
            }
            if (bindingName == "itemtypeicontint")
            {
                bool isUnlocked = false;
                // Try ItemClass first
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null)
                {
                    if (__instance.xui == null || __instance.xui.playerUI == null || __instance.xui.playerUI.entityPlayer == null)
                    {
                        return true;
                    }
                    if (itemClass.Properties.Values.ContainsKey("Unlocks"))
                    {
                        var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                        if (!string.IsNullOrEmpty(unlocks))
                        {
                            isUnlocked = XUiM_ItemStack.CheckKnown(__instance.xui.playerUI.entityPlayer, itemClass);
                        }
                    }
                    if (isUnlocked && itemClass.Properties.Values.ContainsKey("AltItemTypeIconColor"))
                    {
                        var altColorStr = itemClass.Properties.Values["AltItemTypeIconColor"] as string;
                        if (!string.IsNullOrEmpty(altColorStr))
                        {
                            Color altColor = ItemTypeIconColorPatch.ParseGameColor(altColorStr);
                            value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(altColor.r * 255), Mathf.RoundToInt(altColor.g * 255), Mathf.RoundToInt(altColor.b * 255), Mathf.RoundToInt(altColor.a * 255));
                            __result = true;
                            return false;
                        }
                    }
                    if (!isUnlocked && itemClass.Properties.Values.ContainsKey("ItemTypeIconColor"))
                    {
                        var colorStr = itemClass.Properties.Values["ItemTypeIconColor"] as string;
                        if (!string.IsNullOrEmpty(colorStr))
                        {
                            Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                            value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                            __result = true;
                            return false;
                        }
                    }
                }
                // Try Block if ItemClass is not present or doesn't have the property
                if (block != null && block.Properties != null && block.Properties.Values != null)
                {
                    if (block.Properties.Values.ContainsKey("ItemTypeIconColor"))
                    {
                        var colorStr = block.Properties.Values["ItemTypeIconColor"] as string;
                        if (!string.IsNullOrEmpty(colorStr))
                        {
                            Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                            value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }

    // Patch for XUiC_RecipeEntry
    [HarmonyPatch(typeof(XUiC_RecipeEntry), "GetBindingValueInternal")]
    public class XUiC_RecipeEntry_GetBindingValueInternal_Patch
    {
        static bool Prefix(ref bool __result, XUiC_RecipeEntry __instance, ref string value, string bindingName)
        {
            var recipe = __instance.recipe;
            if (bindingName == "itemtypeicontint" && recipe != null)
            {
                var itemClass = ItemClass.GetForId(recipe.itemValueType);
                bool isUnlocked = false;
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null && itemClass.Properties.Values.ContainsKey("Unlocks") && __instance.xui != null && __instance.xui.playerUI != null && __instance.xui.playerUI.entityPlayer != null)
                {
                    var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                    if (!string.IsNullOrEmpty(unlocks))
                    {
                        isUnlocked = XUiM_ItemStack.CheckKnown(__instance.xui.playerUI.entityPlayer, itemClass);
                    }
                }
                if (isUnlocked && itemClass.Properties.Values.ContainsKey("AltItemTypeIconColor"))
                {
                    var altColorStr = itemClass.Properties.Values["AltItemTypeIconColor"] as string;
                    if (!string.IsNullOrEmpty(altColorStr))
                    {
                        Color altColor = ItemTypeIconColorPatch.ParseGameColor(altColorStr);
                        value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(altColor.r * 255), Mathf.RoundToInt(altColor.g * 255), Mathf.RoundToInt(altColor.b * 255), Mathf.RoundToInt(altColor.a * 255));
                        __result = true;
                        return false;
                    }
                }
                if (!isUnlocked && itemClass.Properties.Values.ContainsKey("ItemTypeIconColor"))
                {
                    var colorStr = itemClass.Properties.Values["ItemTypeIconColor"] as string;
                    if (!string.IsNullOrEmpty(colorStr))
                    {
                        Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                        value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                        __result = true;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    // Patch for XUiC_ItemInfoWindow
    [HarmonyPatch(typeof(XUiC_ItemInfoWindow), "GetBindingValueInternal")]
    public class XUiC_ItemInfoWindow_GetBindingValueInternal_Patch
    {
        static bool Prefix(ref bool __result, XUiC_ItemInfoWindow __instance, ref string value, string bindingName)
        {
            var itemClass = __instance?.itemClass;
            var itemStack = __instance?.itemStack;
            if (bindingName == "itemtypeicontint")
            {
                bool isUnlocked = false;
                if (itemStack == null)
                {
                    return true;
                }
                if (itemStack.IsEmpty())
                {
                    return true;
                }
                if (itemClass == null)
                {
                    return true;
                }
                if (itemClass.Properties == null || itemClass.Properties.Values == null)
                {
                    return true;
                }
                if (__instance.xui == null || __instance.xui.playerUI == null || __instance.xui.playerUI.entityPlayer == null)
                {
                    return true;
                }
                if (itemClass.Properties.Values.ContainsKey("Unlocks"))
                {
                    var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                    if (!string.IsNullOrEmpty(unlocks))
                    {
                        isUnlocked = XUiM_ItemStack.CheckKnown(__instance.xui.playerUI.entityPlayer, itemClass, itemStack.itemValue);
                    }
                }
                if (isUnlocked && itemClass.Properties.Values.ContainsKey("AltItemTypeIconColor"))
                {
                    var altColorStr = itemClass.Properties.Values["AltItemTypeIconColor"] as string;
                    if (!string.IsNullOrEmpty(altColorStr))
                    {
                        Color altColor = ItemTypeIconColorPatch.ParseGameColor(altColorStr);
                        value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(altColor.r * 255), Mathf.RoundToInt(altColor.g * 255), Mathf.RoundToInt(altColor.b * 255), Mathf.RoundToInt(altColor.a * 255));
                        __result = true;
                        return false;
                    }
                }
                if (!isUnlocked && itemClass.Properties.Values.ContainsKey("ItemTypeIconColor"))
                {
                    var colorStr = itemClass.Properties.Values["ItemTypeIconColor"] as string;
                    if (!string.IsNullOrEmpty(colorStr))
                    {
                        Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                        value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                        __result = true;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    // Patch for XUiC_CraftingInfoWindow
    [HarmonyPatch(typeof(XUiC_CraftingInfoWindow), "GetBindingValueInternal")]
    public class XUiC_CraftingInfoWindow_GetBindingValueInternal_Patch
    {
        static bool Prefix(ref bool __result, XUiC_CraftingInfoWindow __instance, ref string value, string bindingName)
        {
            var recipe = __instance.recipe;
            if (bindingName == "itemtypeicontint" && recipe != null)
            {
                var itemClass = ItemClass.GetForId(recipe.itemValueType);
                bool isUnlocked = false;
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null && itemClass.Properties.Values.ContainsKey("Unlocks") && __instance.xui != null && __instance.xui.playerUI != null && __instance.xui.playerUI.entityPlayer != null)
                {
                    var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                    if (!string.IsNullOrEmpty(unlocks))
                    {
                        isUnlocked = XUiM_ItemStack.CheckKnown(__instance.xui.playerUI.entityPlayer, itemClass);
                    }
                }
                if (isUnlocked && itemClass.Properties.Values.ContainsKey("AltItemTypeIconColor"))
                {
                    var altColorStr = itemClass.Properties.Values["AltItemTypeIconColor"] as string;
                    if (!string.IsNullOrEmpty(altColorStr))
                    {
                        Color altColor = ItemTypeIconColorPatch.ParseGameColor(altColorStr);
                        value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(altColor.r * 255), Mathf.RoundToInt(altColor.g * 255), Mathf.RoundToInt(altColor.b * 255), Mathf.RoundToInt(altColor.a * 255));
                        __result = true;
                        return false;
                    }
                }
                if (!isUnlocked && itemClass.Properties.Values.ContainsKey("ItemTypeIconColor"))
                {
                    var colorStr = itemClass.Properties.Values["ItemTypeIconColor"] as string;
                    if (!string.IsNullOrEmpty(colorStr))
                    {
                        Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                        value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                        __result = true;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    // Patch for XUiC_QuestTurnInEntry
    [HarmonyPatch(typeof(XUiC_QuestTurnInEntry), "GetBindingValueInternal")]
    public class XUiC_QuestTurnInEntry_GetBindingValueInternal_Patch
    {
        static bool Prefix(ref bool __result, XUiC_QuestTurnInEntry __instance, ref string value, string bindingName)
        {
            var item = __instance.item;
            if (bindingName == "itemtypeicontint" && item != null && !item.IsEmpty())
            {
                var itemClass = item.itemValue.ItemClass;
                bool isUnlocked = false;
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null && itemClass.Properties.Values.ContainsKey("Unlocks") && __instance.xui != null && __instance.xui.playerUI != null && __instance.xui.playerUI.entityPlayer != null)
                {
                    var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                    if (!string.IsNullOrEmpty(unlocks))
                    {
                        isUnlocked = XUiM_ItemStack.CheckKnown(__instance.xui.playerUI.entityPlayer, itemClass, item.itemValue);
                    }
                }
                if (isUnlocked && itemClass.Properties.Values.ContainsKey("AltItemTypeIconColor"))
                {
                    var altColorStr = itemClass.Properties.Values["AltItemTypeIconColor"] as string;
                    if (!string.IsNullOrEmpty(altColorStr))
                    {
                        Color altColor = ItemTypeIconColorPatch.ParseGameColor(altColorStr);
                        value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(altColor.r * 255), Mathf.RoundToInt(altColor.g * 255), Mathf.RoundToInt(altColor.b * 255), Mathf.RoundToInt(altColor.a * 255));
                        __result = true;
                        return false;
                    }
                }
                if (!isUnlocked && itemClass.Properties.Values.ContainsKey("ItemTypeIconColor"))
                {
                    var colorStr = itemClass.Properties.Values["ItemTypeIconColor"] as string;
                    if (!string.IsNullOrEmpty(colorStr))
                    {
                        Color color = ItemTypeIconColorPatch.ParseGameColor(colorStr);
                        value = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                        __result = true;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
