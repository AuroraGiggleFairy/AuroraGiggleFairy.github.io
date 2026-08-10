using System;

namespace FuelAutoShutOff
{
	/// <summary>
	/// Work detection for fuel-using workstations.
	/// Productive craft/smelt finishing extinguishes.
	/// Keep-burning (heat / screamer bait) only when craft queue is empty and
	/// primary smelting slots are empty. Leftover smelt input with full bins extinguishes.
	/// </summary>
	public static class FuelAutoShutOffLogic
	{
		public const int ModuleFuel = 3;
		public const int ModuleMaterialInput = 4;

		public static bool IsServerContext()
		{
			try
			{
				return SingletonMonoBehaviour<ConnectionManager>.Instance != null
					&& SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
			}
			catch
			{
				return false;
			}
		}

		public static bool UsesFuel(bool[] isModuleUsed)
		{
			return isModuleUsed != null
				&& isModuleUsed.Length > ModuleFuel
				&& isModuleUsed[ModuleFuel];
		}

		/// <summary>
		/// Productive work still in progress (crafting and/or smelting that can proceed).
		/// </summary>
		public static bool HasProductiveWork(
			TileEntityWorkstation station,
			RecipeQueueItem[] queue,
			ItemStack[] input,
			bool[] isModuleUsed,
			float[] currentMeltTimesLeft)
		{
			if (station == null)
			{
				return false;
			}

			if (HasCraftingWork(queue))
			{
				return true;
			}

			return HasSmeltingWork(station, input, isModuleUsed, currentMeltTimesLeft);
		}

		/// <summary>
		/// Intentional empty burn: nothing crafting and nothing in primary smelt slots.
		/// </summary>
		public static bool IsHeatOnlyBurn(
			TileEntityWorkstation station,
			RecipeQueueItem[] queue,
			ItemStack[] input,
			bool[] isModuleUsed)
		{
			if (HasCraftingWork(queue))
			{
				return false;
			}

			if (station == null
				|| isModuleUsed == null
				|| isModuleUsed.Length <= ModuleMaterialInput
				|| !isModuleUsed[ModuleMaterialInput])
			{
				// Campfire / chem with no smelt module: empty craft queue is enough.
				return true;
			}

			return PrimarySmeltSlotsEmpty(station, input);
		}

		/// <summary>
		/// Extinguish when productive work ends, or when smelt slots still hold items
		/// that cannot proceed (for example full material bins). Heat-only burns stay on
		/// unless productive work finished this tick.
		/// </summary>
		public static bool ShouldExtinguish(
			TileEntityWorkstation station,
			RecipeQueueItem[] queue,
			ItemStack[] input,
			bool[] isModuleUsed,
			float[] currentMeltTimesLeft,
			bool hadProductiveWorkThisTick)
		{
			if (station == null || !UsesFuel(isModuleUsed))
			{
				return false;
			}

			if (HasProductiveWork(station, queue, input, isModuleUsed, currentMeltTimesLeft))
			{
				return false;
			}

			if (IsHeatOnlyBurn(station, queue, input, isModuleUsed))
			{
				return hadProductiveWorkThisTick;
			}

			// Craft empty, but primary smelt slots still have items that cannot proceed.
			return true;
		}

		public static bool HasCraftingWork(RecipeQueueItem[] queue)
		{
			if (queue == null)
			{
				return false;
			}

			for (int i = 0; i < queue.Length; i++)
			{
				RecipeQueueItem item = queue[i];
				if (item == null)
				{
					continue;
				}

				if (item.IsCrafting)
				{
					return true;
				}

				if (item.Multiplier > 0 && item.Recipe != null)
				{
					return true;
				}
			}

			return false;
		}

		public static bool PrimarySmeltSlotsEmpty(TileEntityWorkstation station, ItemStack[] input)
		{
			if (station == null || input == null)
			{
				return true;
			}

			int primaryCount = GetPrimarySmeltSlotCount(station, input);
			for (int i = 0; i < primaryCount; i++)
			{
				ItemStack stack = input[i];
				if (stack != null && !stack.IsEmpty())
				{
					return false;
				}
			}

			return true;
		}

		public static int GetPrimarySmeltSlotCount(TileEntityWorkstation station, ItemStack[] input)
		{
			if (station == null || input == null)
			{
				return 0;
			}

			string[] materialNames = station.MaterialNames;
			int primaryCount = station.InputSlotCount;
			if (materialNames != null && materialNames.Length > 0)
			{
				primaryCount = Math.Min(station.InputSlotCount, input.Length - materialNames.Length);
			}

			if (primaryCount <= 0)
			{
				primaryCount = Math.Min(station.InputSlotCount, input.Length);
			}

			return Math.Max(0, primaryCount);
		}

		public static bool HasSmeltingWork(
			TileEntityWorkstation station,
			ItemStack[] input,
			bool[] isModuleUsed,
			float[] currentMeltTimesLeft)
		{
			if (station == null
				|| input == null
				|| isModuleUsed == null
				|| isModuleUsed.Length <= ModuleMaterialInput
				|| !isModuleUsed[ModuleMaterialInput])
			{
				return false;
			}

			string[] materialNames = station.MaterialNames;
			if (materialNames == null || materialNames.Length == 0)
			{
				return false;
			}

			int primaryCount = GetPrimarySmeltSlotCount(station, input);
			for (int i = 0; i < primaryCount; i++)
			{
				ItemStack stack = input[i];
				if (stack == null || stack.IsEmpty())
				{
					continue;
				}

				// Room check first: an active melt timer does not count as work if the
				// matching material bin cannot accept this item's weight (full or not
				// enough space for the next melt unit, e.g. sand near max).
				if (!CanContinueSmeltingStack(station, stack, materialNames, input))
				{
					continue;
				}

				if (currentMeltTimesLeft != null
					&& i < currentMeltTimesLeft.Length
					&& currentMeltTimesLeft[i] >= 0f
					&& currentMeltTimesLeft[i] != int.MinValue)
				{
					return true;
				}

				// Can melt into the bin (timer may not have started yet).
				return true;
			}

			return false;
		}

		/// <summary>
		/// True when the stack matches a forge material and its material bin still has room.
		/// Full bins / non-matching materials do not count as pending work.
		/// </summary>
		public static bool CanContinueSmeltingStack(
			TileEntityWorkstation station,
			ItemStack stack,
			string[] materialNames,
			ItemStack[] input)
		{
			if (stack == null || stack.IsEmpty() || materialNames == null || input == null)
			{
				return false;
			}

			ItemClass itemClass = ItemClass.GetForId(stack.itemValue.type);
			if (itemClass == null || itemClass.MadeOfMaterial == null || itemClass.MadeOfMaterial.ForgeCategory == null)
			{
				return false;
			}

			string category = itemClass.MadeOfMaterial.ForgeCategory;
			int weight = itemClass.GetWeight();
			if (weight <= 0)
			{
				return false;
			}

			for (int m = 0; m < materialNames.Length; m++)
			{
				if (!category.EqualsCaseInsensitive(materialNames[m]))
				{
					continue;
				}

				ItemClass unitClass = ItemClass.GetItemClass("unit_" + materialNames[m]);
				if (unitClass == null)
				{
					return false;
				}

				int binIndex = station.InputSlotCount + m;
				if (binIndex < 0 || binIndex >= input.Length)
				{
					return false;
				}

				ItemStack bin = input[binIndex];
				int binCount = (bin == null || bin.IsEmpty() || bin.itemValue.type == 0) ? 0 : bin.count;
				return binCount + weight <= unitClass.MaxCount;
			}

			return false;
		}
	}
}
