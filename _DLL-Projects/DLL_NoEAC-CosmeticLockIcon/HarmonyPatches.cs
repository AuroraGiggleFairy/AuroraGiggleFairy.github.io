using System.Reflection;
using HarmonyLib;

public static class HarmonyPatches
{
	[HarmonyPatch(typeof(TileEntityWorkstation), "AddCraftComplete")]
	public class Patch_CraftComplete
	{
		private static void Postfix(TileEntityWorkstation __instance, int crafterEntityID, ItemValue itemCrafted, string recipeName, string itemScrapped, int craftExpGain, int craftedCount)
		{
			EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(crafterEntityID) as EntityPlayer;
			if (!(entityPlayer == null))
			{
				ItemClass itemClass = ItemClass.GetItemClass(itemCrafted.ItemClass.GetItemName());
				if (itemClass != null)
				{
					entityPlayer.equipment.UnlockCosmeticItem(itemClass);
				}
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeStack), "outputStack")]
	public class Patch_OutputStack
	{
		private static void Postfix(XUiC_RecipeStack __instance)
		{
			Recipe recipe = __instance.recipe;
			EntityPlayerLocal entityPlayerLocal = __instance.xui?.playerUI?.entityPlayer;
			if (recipe != null && !(entityPlayerLocal == null) && recipe.IsScrap && recipe.ingredients.Count > 0)
			{
				ItemClass itemClass = recipe.ingredients[0].itemValue.ItemClass;
				if (itemClass != null && entityPlayerLocal.equipment != null)
				{
					entityPlayerLocal.equipment.UnlockCosmeticItem(itemClass);
					(__instance.windowGroup?.Controller)?.RefreshBindingsSelfAndChildren();
				}
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_ItemStack), "updateLockTypeIcon")]
	public class Patch_UpdateLockTypeIcon
	{
		private static void Postfix(XUiC_ItemStack __instance)
		{
			ItemClass itemClassOrMissing = __instance.itemClassOrMissing;
			EntityPlayerLocal entityPlayerLocal = __instance.xui?.playerUI?.entityPlayer;
			if (itemClassOrMissing != null && !(entityPlayerLocal == null))
			{
				string text = ((entityPlayerLocal.equipment == null || !entityPlayerLocal.equipment.HasCosmeticUnlocked(itemClassOrMissing).isUnlocked) ? (itemClassOrMissing.Properties.Values.ContainsKey("ItemTypeIcon") ? itemClassOrMissing.Properties.Values["ItemTypeIcon"] : null) : (itemClassOrMissing.Properties.Values.ContainsKey("AltItemTypeIcon") ? itemClassOrMissing.Properties.Values["AltItemTypeIcon"] : null));
				if (!string.IsNullOrEmpty(text) && __instance.itemIconSprite != null)
				{
					__instance.itemIconSprite.SpriteName = "ui_game_symbol_" + text;
				}
			}
		}
	}

	public static void ApplyPatches()
	{
		var harmony = new Harmony("com.agfprojects.cosmeticlockicon");
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}
}
