using HarmonyLib;
using UnityEngine;

namespace ItemTypeIconColor
{
    [HarmonyPatch(typeof(XUiC_ItemStack), "get_ItemIconColor")]
    public class XUiC_ItemStack_ItemIconColor_Patch
    {
        static bool Prefix(XUiC_ItemStack __instance, ref string __result)
        {
            var itemClass = __instance.itemClassOrMissing;
            var itemStack = __instance.itemStack;
            if (itemClass == null || itemStack == null)
            {
                return false;
            }
            if (itemClass.Properties != null && itemClass.Properties.Values != null && itemClass.Properties.Values.ContainsKey("ItemTypeIcon") && !string.IsNullOrEmpty(itemClass.Properties.Values["ItemTypeIcon"] as string)
                && ItemTypeIconColorPatch.ItemColors.TryGetValue(itemClass.Name, out Color color) && color != Color.white)
            {
                string colorString = string.Format("{0},{1},{2},{3}", Mathf.RoundToInt(color.r * 255), Mathf.RoundToInt(color.g * 255), Mathf.RoundToInt(color.b * 255), Mathf.RoundToInt(color.a * 255));
                __result = colorString;
                return false;
            }
            // Fallback to vanilla logic
            Color32 v = itemClass.GetIconTint(itemStack.itemValue);
            string fallbackColor = __instance.itemiconcolorFormatter.Format(v);
            __result = fallbackColor;
            return false;
        }
    }
}
