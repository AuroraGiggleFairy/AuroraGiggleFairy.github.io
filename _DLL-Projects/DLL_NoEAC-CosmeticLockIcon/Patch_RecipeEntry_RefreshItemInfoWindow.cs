using UnityEngine;
using HarmonyLib;

[HarmonyPatch(typeof(XUiC_RecipeEntry), nameof(XUiC_RecipeEntry.SetRecipeAndHasIngredients))]
public class Patch_RecipeEntry_RefreshItemInfoWindow
{
    static void Postfix(XUiC_RecipeEntry __instance, Recipe recipe, bool hasIngredients)
    {
        // Find the crafting info window in the current window group and call SetRecipe as vanilla does
        var craftingInfoWindow = __instance.windowGroup?.Controller as XUiC_CraftingInfoWindow;
        if (craftingInfoWindow != null)
        {
            // Log window type and recipe name for debug
            UnityEngine.Debug.Log($"[CosmeticLockIcon] Patch_RecipeEntry_RefreshItemInfoWindow: window type={craftingInfoWindow.GetType().Name}, recipe={(recipe != null ? recipe.GetName() : "null")}");
            craftingInfoWindow.SetRecipe(__instance);
        }
    }
}
