using HarmonyLib;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Dummy rounds stay in the game. They are the projectile.
	/// An empty gun fills itself with the selected dummy round.
	/// That happens when the gun is created, moved, or emptied.
	/// Scrap does not give that round back. The BFG can still switch cores.
	/// </summary>
	internal static class DummyMagazine
	{
		internal static bool UsesDummy(ItemActionAttack action)
		{
			string[] names = action?.MagazineItemNames;
			if (names == null || names.Length == 0)
			{
				return false;
			}

			for (int i = 0; i < names.Length; i++)
			{
				if (string.IsNullOrEmpty(names[i]) || !names[i].StartsWith("Dummy"))
				{
					return false;
				}
			}

			return true;
		}

		internal static void FillIfEmpty(ItemActionRanged action, ItemActionData data)
		{
			ItemValue item = data?.invData?.itemValue;
			if (action == null || item == null || !UsesDummy(action) || ItemInventoryAccess.MetaOf(item) > 0)
			{
				return;
			}

			int max = action.GetMaxAmmoCount(data);
			if (max > 0)
			{
				ItemInventoryAccess.SetMeta(item, max);
			}
		}

		internal static void Fill(ItemValue item)
		{
			ItemActionRanged ranged = Find(item);
			if (ranged == null)
			{
				return;
			}

			// The rocket hold patch briefly sets the count to 1 so the gun does not grow a pile of missiles.
			if (ranged is ItemActionLauncher && ItemInventoryAccess.MetaOf(item) == 1)
			{
				return;
			}

			int max = ranged.GetInitialMeta(item);
			if (max <= 0 || ItemInventoryAccess.MetaOf(item) >= max)
			{
				return;
			}

			int index = ItemInventoryAccess.AmmoIndex(item);
			if (index < 0 || index >= ranged.MagazineItemNames.Length)
			{
				ItemInventoryAccess.SetAmmoIndex(item, 0);
			}

			ItemInventoryAccess.SetMeta(item, max);
		}

		internal static void FillPlayer(EntityPlayer player)
		{
			if (player == null)
			{
				return;
			}

			FillGrid(player.inventory?.ItemGrid);
			FillGrid(player.bag?.itemGrid);
		}

		internal static void FillStack(ItemStack stack)
		{
			Fill(stack?.itemValue);
		}

		internal static void FillStacks(ItemStack[] slots)
		{
			if (slots == null)
			{
				return;
			}

			for (int i = 0; i < slots.Length; i++)
			{
				FillStack(slots[i]);
			}
		}

		private static void FillGrid(ItemStackGrid grid)
		{
			if (grid == null)
			{
				return;
			}

			int length = grid.Length;
			for (int i = 0; i < length; i++)
			{
				Fill(grid.GetItem(i)?.itemValue);
			}
		}

		private static ItemActionRanged Find(ItemValue item)
		{
			ItemClass itemClass = item?.ItemClass;
			if (itemClass?.Actions == null)
			{
				return null;
			}

			for (int i = 0; i < itemClass.Actions.Length; i++)
			{
				if (itemClass.Actions[i] is ItemActionRanged ranged && UsesDummy(ranged))
				{
					return ranged;
				}
			}

			return null;
		}

		private static int NextIndex(ItemActionRanged action, ItemValue item, int current, bool excludeNonUnderwater)
		{
			string[] names = action.MagazineItemNames;
			int count = names.Length;
			for (int step = 1; step <= count; step++)
			{
				int index = (current + step) % count;
				if (!excludeNonUnderwater)
				{
					return index;
				}

				ItemClass ammo = ItemClass.GetItemClass(names[index], false);
				if (ammo == null || ammo.UsableUnderwater)
				{
					return index;
				}
			}

			return current;
		}

		/// <summary>
		/// Runs before the rocket hold patch clamps the count to one mesh.
		/// </summary>
		[HarmonyPatch(typeof(ItemActionLauncher), nameof(ItemActionLauncher.StartHolding))]
		private static class Patch_FillLauncherOnHold
		{
			[HarmonyPriority(Priority.First)]
			private static void Prefix(ItemActionLauncher __instance, ItemActionData _actionData)
			{
				DummyMagazine.FillIfEmpty(__instance, _actionData);
			}
		}

		[HarmonyPatch(typeof(ItemActionRanged), nameof(ItemActionRanged.OnHoldingUpdate))]
		private static class Patch_KeepFilledWhileHeld
		{
			private static void Postfix(ItemActionRanged __instance, ItemActionData _actionData)
			{
				if (!DummyMagazine.UsesDummy(__instance))
				{
					return;
				}

				ItemValue item = _actionData?.invData?.itemValue;
				int meta = ItemInventoryAccess.MetaOf(item);
				if (item == null || meta > 0)
				{
					return;
				}

				int max = __instance.GetMaxAmmoCount(_actionData);
				if (max > 0)
				{
					ItemInventoryAccess.SetMeta(item, max);
				}
			}
		}

		[HarmonyPatch(typeof(ItemValue), "set_Meta")]
		private static class Patch_RefillWhenCleared
		{
			private static void Postfix(ItemValue __instance, int value)
			{
				if (value <= 0)
				{
					DummyMagazine.Fill(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Inventory), nameof(Inventory.SetItem), new System.Type[] { typeof(int), typeof(ItemStack) })]
		private static class Patch_FillToolbeltStack
		{
			private static void Prefix(ItemStack _itemStack)
			{
				DummyMagazine.FillStack(_itemStack);
			}
		}

		[HarmonyPatch(typeof(Inventory), nameof(Inventory.SetItem), new System.Type[] { typeof(int), typeof(ItemValue), typeof(int) })]
		private static class Patch_FillToolbeltValue
		{
			private static void Prefix(ItemValue _itemValue)
			{
				DummyMagazine.Fill(_itemValue);
			}
		}

		[HarmonyPatch(typeof(Inventory), nameof(Inventory.SetSlots))]
		private static class Patch_FillToolbeltSlots
		{
			private static void Prefix(ItemStack[] _slots)
			{
				DummyMagazine.FillStacks(_slots);
			}
		}

		[HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new System.Type[] { typeof(ItemStack) })]
		private static class Patch_FillToolbeltAdd
		{
			private static void Prefix(ItemStack _itemStack)
			{
				DummyMagazine.FillStack(_itemStack);
			}
		}

		[HarmonyPatch(typeof(Bag), nameof(Bag.SetSlot))]
		private static class Patch_FillBagSlot
		{
			private static void Prefix(ItemStack _stack)
			{
				DummyMagazine.FillStack(_stack);
			}
		}

		[HarmonyPatch(typeof(Bag), nameof(Bag.SetSlots))]
		private static class Patch_FillBagSlots
		{
			private static void Prefix(ItemStack[] _slots)
			{
				DummyMagazine.FillStacks(_slots);
			}
		}

		[HarmonyPatch(typeof(Bag), nameof(Bag.AddItem))]
		private static class Patch_FillBagAdd
		{
			private static void Prefix(ItemStack _itemStack)
			{
				DummyMagazine.FillStack(_itemStack);
			}
		}

		/// <summary>
		/// Scrap still destroys a scrappable gun. The dummy round stays out of the bag.
		/// </summary>
		[HarmonyPatch(typeof(ItemActionEntryScrap), "HandleRemoveAmmo")]
		private static class Patch_ScrapSkipsDummyAmmo
		{
			private static bool Prefix(ItemStack _stack, ref ItemStack __result)
			{
				if (DummyMagazine.Find(_stack?.itemValue) == null)
				{
					return true;
				}

				__result = _stack;
				return false;
			}
		}

		[HarmonyPatch(typeof(ItemActionRanged), nameof(ItemActionRanged.CycleAmmoType))]
		private static class Patch_CycleDummyCore
		{
			private static bool Prefix(ItemActionRanged __instance, ItemActionData _actionData, bool excludeNonUnderwaterAmmoTypes)
			{
				if (!DummyMagazine.UsesDummy(__instance) || __instance.MagazineItemNames.Length < 2)
				{
					return true;
				}

				ItemValue item = _actionData?.invData?.itemValue;
				if (item == null)
				{
					return true;
				}

				int current = ItemInventoryAccess.AmmoIndex(item);
				if (current < 0 || current >= __instance.MagazineItemNames.Length)
				{
					current = 0;
				}

				int next = DummyMagazine.NextIndex(__instance, item, current, excludeNonUnderwaterAmmoTypes);
				ItemInventoryAccess.SetAmmoIndex(item, next);
				int max = __instance.GetMaxAmmoCount(_actionData);
				if (max > 0)
				{
					ItemInventoryAccess.SetMeta(item, max);
				}

				EntityAlive entity = ItemInventoryAccess.Holding(_actionData.invData);
				if (entity != null)
				{
					__instance.SwapAmmoType(entity, next);
				}

				return false;
			}
		}
	}
}
