using System;
using HarmonyLib;

namespace TenCraftingSlots
{
	/// <summary>
	/// Matches the Doom HUD crafting queue XML (2 rows x 5 cols = 10).
	/// Vanilla TileEntityWorkstation still constructs RecipeQueueItem[4]; without this,
	/// XML-only 5th+ slots error on workstation open/sync (v3.0 breakage).
	/// </summary>
	internal static class CraftingSlotUtil
	{
		public const int QueueSize = 10;

		public static void EnsureQueueSize(TileEntityWorkstation te)
		{
			if (te == null)
			{
				return;
			}

			RecipeQueueItem[] current = te.Queue;
			if (current != null && current.Length >= QueueSize)
			{
				return;
			}

			RecipeQueueItem[] expanded = new RecipeQueueItem[QueueSize];
			int copy = current == null ? 0 : Math.Min(current.Length, QueueSize);
			for (int i = 0; i < copy; i++)
			{
				expanded[i] = current[i];
			}

			for (int i = 0; i < QueueSize; i++)
			{
				if (expanded[i] == null)
				{
					expanded[i] = new RecipeQueueItem();
				}
			}

			te.Queue = expanded;
		}
	}

	[HarmonyPatch(typeof(TileEntityWorkstation), MethodType.Constructor, new Type[] { typeof(Chunk) })]
	public static class Patch_TileEntityWorkstation_Ctor
	{
		public static void Postfix(TileEntityWorkstation __instance)
		{
			CraftingSlotUtil.EnsureQueueSize(__instance);
		}
	}

	/// <summary>Expand after save/load / net sync when an older 4-slot queue is read.</summary>
	[HarmonyPatch(typeof(TileEntityWorkstation), "readRecipeStackArray")]
	public static class Patch_TileEntityWorkstation_readRecipeStackArray
	{
		public static void Postfix(TileEntityWorkstation __instance)
		{
			CraftingSlotUtil.EnsureQueueSize(__instance);
		}
	}

	/// <summary>Guarantee TE length matches UI before queue is copied into the 10-slot grid.</summary>
	[HarmonyPatch(typeof(XUiC_WorkstationWindowGroup), nameof(XUiC_WorkstationWindowGroup.OnOpen))]
	public static class Patch_XUiC_WorkstationWindowGroup_OnOpen
	{
		public static void Prefix(XUiC_WorkstationWindowGroup __instance)
		{
			try
			{
				if (__instance?.WorkstationData?.TileEntity != null)
				{
					CraftingSlotUtil.EnsureQueueSize(__instance.WorkstationData.TileEntity);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[10CraftingSlots] OnOpen ensure failed: " + ex.Message);
			}
		}
	}
}
