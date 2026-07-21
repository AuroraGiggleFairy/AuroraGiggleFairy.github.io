using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ItemTypeIconColor
{
    internal static class ItemTypeIconColorUiHelpers
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly object CacheLock = new object();
        private static readonly Dictionary<string, MemberInfo> MemberCache = new Dictionary<string, MemberInfo>(StringComparer.Ordinal);
        private static readonly HashSet<string> MissingMemberCache = new HashSet<string>(StringComparer.Ordinal);

        private static string BuildCacheKey(Type type, string memberName)
        {
            string typeName = type != null ? type.FullName : string.Empty;
            return typeName + "|" + memberName;
        }

        private static MemberInfo ResolveMember(Type type, string memberName)
        {
            if (type == null || string.IsNullOrEmpty(memberName))
            {
                return null;
            }

            string cacheKey = BuildCacheKey(type, memberName);
            lock (CacheLock)
            {
                MemberInfo cached;
                if (MemberCache.TryGetValue(cacheKey, out cached))
                {
                    return cached;
                }

                if (MissingMemberCache.Contains(cacheKey))
                {
                    return null;
                }
            }

            MemberInfo foundMember = null;
            var prop = type.GetProperty(memberName, InstanceFlags);
            if (prop != null && prop.GetIndexParameters().Length == 0)
            {
                foundMember = prop;
            }
            else
            {
                var field = type.GetField(memberName, InstanceFlags);
                if (field != null)
                {
                    foundMember = field;
                }
            }

            lock (CacheLock)
            {
                if (foundMember != null)
                {
                    MemberCache[cacheKey] = foundMember;
                }
                else
                {
                    MissingMemberCache.Add(cacheKey);
                }
            }

            return foundMember;
        }

        private static object GetMemberValue(object instance, string memberName)
        {
            if (instance == null || string.IsNullOrEmpty(memberName))
            {
                return null;
            }

            var type = instance.GetType();
            while (type != null)
            {
                MemberInfo member = ResolveMember(type, memberName);
                if (member != null)
                {
                    try
                    {
                        var prop = member as PropertyInfo;
                        if (prop != null)
                        {
                            object propValue = prop.GetValue(instance, null);
                            if (propValue != null)
                            {
                                return propValue;
                            }
                        }

                        var field = member as FieldInfo;
                        if (field != null)
                        {
                            object fieldValue = field.GetValue(instance);
                            if (fieldValue != null)
                            {
                                return fieldValue;
                            }
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }

                type = type.BaseType;
            }

            return null;
        }

        public static EntityPlayerLocal GetEntityPlayer(object controller)
        {
            if (controller == null)
            {
                return null;
            }

            var xuiController = controller as XUiController;
            if (xuiController != null && xuiController.xui != null && xuiController.xui.playerUI != null)
            {
                EntityPlayerLocal fastPlayer = xuiController.xui.playerUI.entityPlayer;
                if (fastPlayer != null)
                {
                    return fastPlayer;
                }
            }

            var localPlayer = GetMemberValue(controller, "localPlayer") as EntityPlayerLocal
                ?? GetMemberValue(controller, "LocalPlayer") as EntityPlayerLocal;
            if (localPlayer != null)
            {
                return localPlayer;
            }

            object xui = GetMemberValue(controller, "xui")
                ?? GetMemberValue(controller, "XUi")
                ?? GetMemberValue(controller, "_xui");
            if (xui == null)
            {
                return null;
            }

            object playerUI = GetMemberValue(xui, "playerUI")
                ?? GetMemberValue(xui, "PlayerUI")
                ?? GetMemberValue(xui, "_playerUI");
            if (playerUI == null)
            {
                return null;
            }

            return GetMemberValue(playerUI, "entityPlayer") as EntityPlayerLocal
                ?? GetMemberValue(playerUI, "_entityPlayer") as EntityPlayerLocal
                ?? GetMemberValue(playerUI, "localPlayer") as EntityPlayerLocal
                ?? GetMemberValue(playerUI, "LocalPlayer") as EntityPlayerLocal;
        }
    }

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
                var entityPlayer = ItemTypeIconColorUiHelpers.GetEntityPlayer(__instance);
                // Try ItemClass first
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null)
                {
                    if (entityPlayer == null)
                    {
                        return true;
                    }
                    if (itemClass.Properties.Values.ContainsKey("Unlocks"))
                    {
                        var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                        if (!string.IsNullOrEmpty(unlocks))
                        {
                            isUnlocked = XUiM_ItemStack.CheckKnown(entityPlayer, itemClass);
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
                var entityPlayer = ItemTypeIconColorUiHelpers.GetEntityPlayer(__instance);
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null && itemClass.Properties.Values.ContainsKey("Unlocks") && entityPlayer != null)
                {
                    var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                    if (!string.IsNullOrEmpty(unlocks))
                    {
                        isUnlocked = XUiM_ItemStack.CheckKnown(entityPlayer, itemClass);
                    }
                }
                if (itemClass == null || itemClass.Properties == null || itemClass.Properties.Values == null)
                {
                    return true;
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
                var entityPlayer = ItemTypeIconColorUiHelpers.GetEntityPlayer(__instance);
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
                if (entityPlayer == null)
                {
                    return true;
                }
                if (itemClass.Properties.Values.ContainsKey("Unlocks"))
                {
                    var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                    if (!string.IsNullOrEmpty(unlocks))
                    {
                        isUnlocked = XUiM_ItemStack.CheckKnown(entityPlayer, itemClass, itemStack.itemValue);
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
                var entityPlayer = ItemTypeIconColorUiHelpers.GetEntityPlayer(__instance);
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null && itemClass.Properties.Values.ContainsKey("Unlocks") && entityPlayer != null)
                {
                    var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                    if (!string.IsNullOrEmpty(unlocks))
                    {
                        isUnlocked = XUiM_ItemStack.CheckKnown(entityPlayer, itemClass);
                    }
                }
                if (itemClass == null || itemClass.Properties == null || itemClass.Properties.Values == null)
                {
                    return true;
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
                var entityPlayer = ItemTypeIconColorUiHelpers.GetEntityPlayer(__instance);
                if (itemClass != null && itemClass.Properties != null && itemClass.Properties.Values != null && itemClass.Properties.Values.ContainsKey("Unlocks") && entityPlayer != null)
                {
                    var unlocks = itemClass.Properties.Values["Unlocks"] as string;
                    if (!string.IsNullOrEmpty(unlocks))
                    {
                        isUnlocked = XUiM_ItemStack.CheckKnown(entityPlayer, itemClass, item.itemValue);
                    }
                }
                if (itemClass == null || itemClass.Properties == null || itemClass.Properties.Values == null)
                {
                    return true;
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
