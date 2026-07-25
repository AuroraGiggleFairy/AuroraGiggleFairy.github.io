using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Autominers
{
	[HarmonyPatch(typeof(Block), nameof(Block.CanPlaceBlockAt))]
	public static class Patch_CanPlaceBlockAt
	{
		public static bool Prefix(ref bool __result, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
		{
			if (!ModUtil.IsAutominerWorkstation(_blockValue.Block))
			{
				return true;
			}

			List<string> allowedBlocks = ModUtil.GetMiningSourceBlocks(_blockValue.Block);
			string underBlock = _world.GetBlock(_blockPos + ModUtil.GetOffset(_blockValue)).Block.GetBlockName();
			if (!allowedBlocks.ContainsCaseInsensitive(underBlock))
			{
				__result = false;
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(BlockWorkstation), nameof(BlockWorkstation.GetBlockActivationCommands))]
	public static class Patch_GetBlockActivationCommands
	{
		public static bool Prefix(ref BlockActivationCommand[] __result, BlockActivationCommand[] ___cmds, WorldBase _world, BlockValue _blockValue, Vector3i _blockPos)
		{
			if (!_blockValue.Block.Tags.Test_AnySet(FastTags<TagGroup.Global>.Parse("miningMachine")))
			{
				return true;
			}

			bool canPickup = _blockValue.Block.Properties.GetBool("CanPickup");
			TileEntityWorkstation te = _world.GetTileEntity(_blockPos) as TileEntityWorkstation;
			bool playerPlaced = te != null && te.IsPlayerPlaced;
			___cmds[1].enabled = canPickup && playerPlaced;
			__result = ___cmds;
			return false;
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeList), "GetRecipeData")]
	public static class Patch_GetRecipeData
	{
		public static void Postfix(XUiC_RecipeList __instance)
		{
			if (__instance.Workstation != "miningMachine")
			{
				return;
			}

			XUiC_WorkstationWindowGroup group = __instance.windowGroup.Controller.GetChildByType<XUiC_WorkstationWindowGroup>();
			if (group?.WorkstationData?.TileEntity == null)
			{
				return;
			}

			TileEntityWorkstation te = group.WorkstationData.TileEntity;
			Vector3i checkPos = te.ToWorldPos() + ModUtil.GetOffset(te.blockValue);
			string underBlock = GameManager.Instance.World.GetBlock(checkPos).Block.GetBlockName();
			if (!ModUtil.TryGetMiningOutputs(te.blockValue.Block, underBlock, out string outputsCsv))
			{
				__instance.recipes = new List<Recipe>();
			}
			else
			{
				List<string> itemNames = outputsCsv.Split(',').ToList();
				List<Recipe> filtered = new List<Recipe>();
				foreach (Recipe recipe in __instance.recipes)
				{
					if (itemNames.Contains(recipe.GetOutputItemClass().GetItemName()))
					{
						filtered.Add(recipe);
					}
				}
				__instance.recipes = filtered;
			}

			__instance.Page = 0;
			__instance.IsDirty = true;
			__instance.resortRecipes = true;
			__instance.pageChanged = true;
		}
	}

	[HarmonyPatch(typeof(XUiM_Recipes), nameof(XUiM_Recipes.FilterRecipesByName))]
	public static class Patch_FilterRecipesByName
	{
		public static void Postfix(ref List<Recipe> __result)
		{
			if (__result == null || __result.Count == 0)
			{
				return;
			}

			__result.RemoveAll(r => r != null && ModUtil.IsAutominerCraftingArea(r.craftingArea));
		}
	}

	[HarmonyPatch(typeof(XUiC_ItemActionList), "AddActionListEntry")]
	public static class Patch_AddActionListEntry
	{
		public static bool Prefix(BaseItemActionEntry actionEntry)
		{
			if (!(actionEntry is ItemActionEntryFavorite) || !(actionEntry.ItemController is XUiC_RecipeEntry recipeEntry))
			{
				return true;
			}

			Recipe recipe = recipeEntry.Recipe;
			if (recipe != null && ModUtil.IsAutominerCraftingArea(recipe.craftingArea))
			{
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(ItemActionEntryCraft), "RefreshEnabled")]
	public static class Patch_RefreshItemActionEntryCraft
	{
		public static void Postfix(ItemActionEntryCraft __instance)
		{
			if (!(__instance.ItemController is XUiC_RecipeEntry recipeEntry))
			{
				return;
			}

			Recipe recipe = recipeEntry.Recipe;
			if (recipe != null && ModUtil.IsAutominerCraftingArea(recipe.craftingArea))
			{
				__instance.ActionName = Localization.Get("lblContextActionMHarvest", false);
				__instance.IconName = "ui_game_symbol_miningmachine";
			}
		}
	}

	[HarmonyPatch(typeof(RenderDisplacedCube), "disableAllComponents")]
	public static class Patch_DisableAllComponents
	{
		public static void Postfix(Transform _transform)
		{
			_transform.FindInChilds("arrowHelper")?.gameObject.SetActive(true);
		}
	}
}
