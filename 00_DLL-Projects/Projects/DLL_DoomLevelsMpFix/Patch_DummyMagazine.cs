using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Dummy rounds stay in the game. They are the projectile.
	/// An empty gun fills itself with the selected dummy round.
	/// That happens when the gun is created, moved, or emptied.
	/// A gun that is already drawn also gets its loaded round before the next trigger,
	/// so the first shot after a fill or a reload is a real shot.
	/// Scrap does not give that round back.
	/// Switching the BFG core changes which dummy round is selected. That round stays in the gun.
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

		/// <summary>
		/// The magazine count has to match before the trigger is accepted.
		/// A launcher also needs its one loaded mesh, or that trigger spends ammo and fires nothing.
		/// </summary>
		internal static void Chamber(ItemActionRanged action, ItemActionData data)
		{
			if (action == null || data?.invData == null || !UsesDummy(action))
			{
				return;
			}

			ItemValue item = data.invData.itemValue;
			if (item == null || ItemInventoryAccess.Holding(data.invData) == null)
			{
				return;
			}

			int max = action.GetMaxAmmoCount(data);
			int meta = ItemInventoryAccess.MetaOf(item);
			if (max > 0 && meta < max)
			{
				int index = ItemInventoryAccess.AmmoIndex(item);
				if (index < 0 || index >= action.MagazineItemNames.Length)
				{
					ItemInventoryAccess.SetAmmoIndex(item, 0);
				}

				ItemInventoryAccess.SetMeta(item, max);
				meta = max;
			}

			PlaceLoadedRound(action as ItemActionLauncher, data, meta);
		}

		internal static void PlaceLoadedRound(ItemActionLauncher launcher, ItemActionData data, int meta)
		{
			ItemActionLauncher.ItemActionDataLauncher held = data as ItemActionLauncher.ItemActionDataLauncher;
			if (launcher == null || held == null || meta <= 0 || !UsesDummy(launcher))
			{
				return;
			}

			for (int i = held.projectileTs.Count - 1; i >= 0; i--)
			{
				if (held.projectileTs[i] == null)
				{
					held.projectileTs.RemoveAt(i);
				}
			}

			if (held.projectileTs.Count > 0)
			{
				return;
			}

			Transform shot = launcher.instantiateProjectile(data);
			if (shot == null)
			{
				return;
			}

			held.projectileTs.Add(shot);
			if (RocketInMag.HidesLoadedRound(launcher))
			{
				shot.gameObject.SetActive(false);
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

			FillStacks(ItemInventoryAccess.SlotsOf(player.inventory));
			FillStacks(ItemInventoryAccess.SlotsOf(player.bag));
		}

		internal static void FillStack(ItemStack stack)
		{
			Fill(ItemInventoryAccess.ItemOf(stack));
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
				DummyMagazine.Chamber(__instance, _actionData);
			}
		}

		/// <summary>
		/// Runs before the magazine requirement, so the first trigger is accepted.
		/// </summary>
		[HarmonyPatch(typeof(ItemAction), nameof(ItemAction.CanExecute))]
		private static class Patch_ChamberBeforeTrigger
		{
			private static void Prefix(ItemAction __instance, ItemActionData _actionData)
			{
				DummyMagazine.Chamber(__instance as ItemActionRanged, _actionData);
			}
		}

		/// <summary>
		/// The mesh has to be on the gun before this shot looks for it.
		/// </summary>
		[HarmonyPatch(typeof(ItemActionLauncher), nameof(ItemActionLauncher.ItemActionEffects))]
		private static class Patch_ChamberBeforeLaunch
		{
			private static void Prefix(ItemActionLauncher __instance, ItemActionData _actionData, int _firingState)
			{
				if (_firingState == 0)
				{
					return;
				}

				ItemValue item = _actionData?.invData?.itemValue;
				DummyMagazine.PlaceLoadedRound(__instance, _actionData, ItemInventoryAccess.MetaOf(item));
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
				if (DummyMagazine.Find(ItemInventoryAccess.ItemOf(_stack)) == null)
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
				ItemClass ammo = ItemClass.GetItemClass(__instance.MagazineItemNames[next], false);
				if (entity != null && ammo != null)
				{
					__instance.SwapAmmoType(entity, ammo.Id);
				}

				return false;
			}
		}

		/// <summary>
		/// The BFG core switch still runs. The loaded dummy round is not handed back.
		/// </summary>
		[HarmonyPatch(typeof(ItemActionRanged), nameof(ItemActionRanged.removeCurrentlyLoadedAmmunition))]
		private static class Patch_SwapKeepsDummyAmmo
		{
			private static bool Prefix(ItemActionRanged __instance)
			{
				return !DummyMagazine.UsesDummy(__instance);
			}
		}

		/// <summary>
		/// The BFG radial number is shots the energy cells can pay for.
		/// One shot spends 40 cells. Both damage cores share that count.
		/// </summary>
		[HarmonyPatch(typeof(ItemActionAttack), nameof(ItemActionAttack.SetupRadial))]
		private static class Patch_BfgRadialShotCount
		{
			private const int CellsPerShot = 40;

			private static void Postfix(ItemActionAttack __instance, XUiC_Radial _xuiRadialWindow)
			{
				string[] names = __instance?.MagazineItemNames;
				if (_xuiRadialWindow?.menuItem == null || names == null || names.Length == 0)
				{
					return;
				}

				for (int i = 0; i < names.Length; i++)
				{
					if (names[i] != "DummyBFGCellEntity" && names[i] != "DummyBFGCellBlock")
					{
						return;
					}
				}

				ItemClass cells = ItemClass.GetItemClass("PlasmaCellDM", false);
				if (cells == null || _xuiRadialWindow.xui?.PlayerInventory == null)
				{
					return;
				}

				int shots = _xuiRadialWindow.xui.PlayerInventory.GetItemCount(cells.Id) / CellsPerShot;
				string count = shots.ToString();
				string atlas = shots > 0 ? "ItemIconAtlas" : "ItemIconAtlasGreyscale";
				XUiC_RadialEntry[] entries = _xuiRadialWindow.menuItem;
				for (int i = 0; i < entries.Length; i++)
				{
					XUiC_RadialEntry entry = entries[i];
					if (entry == null || entry.MenuItemIndex < 0)
					{
						continue;
					}

					entry.SetText(count);
					entry.SetAtlas(atlas);
				}
			}
		}
	}
}
