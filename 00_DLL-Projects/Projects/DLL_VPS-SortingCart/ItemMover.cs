namespace SortingCart
{
	internal static class ItemMover
	{
		public static int MoveLikeItems(ITileEntityLootable source, ITileEntityLootable dest, bool likeItemsOnly)
		{
			if (source == null || dest == null || source.items == null || dest.items == null)
			{
				return 0;
			}

			int moved = 0;
			for (int s = 0; s < source.items.Length; s++)
			{
				if (ItemStack.Empty.Equals(source.items[s]) || StorageUtil.IsSlotLocked(source, s))
				{
					continue;
				}

				moved += MoveStack(source.items, s, dest, likeItemsOnly, () => source.UpdateSlot(s, source.items[s]));
			}

			if (moved > 0)
			{
				source.SetModified();
				dest.SetModified();
				SortDest(dest);
			}

			return moved;
		}

		public static int MoveLikeItemsFromBag(Bag bag, ITileEntityLootable dest, bool likeItemsOnly)
		{
			if (bag == null || dest == null || dest.items == null)
			{
				return 0;
			}

			ItemStack[] slots = bag.GetSlots();
			if (slots == null)
			{
				return 0;
			}

			int moved = 0;
			for (int s = 0; s < slots.Length; s++)
			{
				if (ItemStack.Empty.Equals(slots[s]) || StorageUtil.IsBagSlotLocked(bag, s))
				{
					continue;
				}

				int before = slots[s].count;
				MoveStack(slots, s, dest, likeItemsOnly, null);
				int after = ItemStack.Empty.Equals(slots[s]) ? 0 : slots[s].count;
				if (after != before)
				{
					bag.SetSlot(s, after == 0 ? ItemStack.Empty : slots[s], true);
					moved += before - after;
				}
			}

			if (moved > 0)
			{
				dest.SetModified();
				SortDest(dest);
			}

			return moved;
		}

		public static int MoveLikeItemsFromBagToBag(Bag source, Bag dest, bool likeItemsOnly)
		{
			if (source == null || dest == null)
			{
				return 0;
			}

			ItemStack[] sourceSlots = source.GetSlots();
			ItemStack[] destSlots = dest.GetSlots();
			if (sourceSlots == null || destSlots == null)
			{
				return 0;
			}

			int moved = MoveLikeItemsToBagSlots(sourceSlots, source, destSlots, dest.LockedSlots, likeItemsOnly);
			if (moved > 0)
			{
				dest.SetSlots(destSlots);
				SortBag(dest);
			}

			return moved;
		}

		private static int MoveLikeItemsToBagSlots(ItemStack[] sourceSlots, Bag source, ItemStack[] destItems, PackedBoolArray destLocks, bool likeItemsOnly)
		{
			int moved = 0;
			for (int s = 0; s < sourceSlots.Length; s++)
			{
				if (ItemStack.Empty.Equals(sourceSlots[s]) || StorageUtil.IsBagSlotLocked(source, s))
				{
					continue;
				}

				int before = sourceSlots[s].count;
				MoveStackToArray(sourceSlots, s, destItems, destLocks, likeItemsOnly);
				int after = ItemStack.Empty.Equals(sourceSlots[s]) ? 0 : sourceSlots[s].count;
				if (after != before)
				{
					source.SetSlot(s, after == 0 ? ItemStack.Empty : sourceSlots[s], true);
					moved += before - after;
				}
			}

			return moved;
		}

		public static int MoveLikeItemsToBagSlots(ITileEntityLootable source, ItemStack[] destItems, PackedBoolArray destLocks, bool likeItemsOnly)
		{
			if (source == null || destItems == null)
			{
				return 0;
			}

			int moved = 0;
			for (int s = 0; s < source.items.Length; s++)
			{
				if (ItemStack.Empty.Equals(source.items[s]) || StorageUtil.IsSlotLocked(source, s))
				{
					continue;
				}

				moved += MoveStackToArray(source.items, s, destItems, destLocks, likeItemsOnly);
			}

			if (moved > 0)
			{
				source.SetModified();
			}

			return moved;
		}

		private static int MoveStack(ItemStack[] sourceItems, int sourceIndex, ITileEntityLootable dest, bool likeItemsOnly, System.Action afterChange)
		{
			ItemStack stack = sourceItems[sourceIndex];
			if (stack == null || stack.IsEmpty())
			{
				return 0;
			}

			int start = stack.count;
			bool foundMatch = false;
			for (int t = 0; t < dest.items.Length; t++)
			{
				if (StorageUtil.IsSlotLocked(dest, t))
				{
					continue;
				}

				if (dest.items[t].itemValue.ItemClass != stack.itemValue.ItemClass)
				{
					continue;
				}

				foundMatch = true;
				(_, bool allMoved) = dest.TryStackItem(t, stack);
				if (allMoved)
				{
					sourceItems[sourceIndex] = ItemStack.Empty;
					afterChange?.Invoke();
					return start;
				}
			}

			if (likeItemsOnly && !foundMatch)
			{
				return start - stack.count;
			}

			if ((foundMatch || !likeItemsOnly) && !stack.IsEmpty() && dest.AddItem(stack))
			{
				sourceItems[sourceIndex] = ItemStack.Empty;
				afterChange?.Invoke();
				return start;
			}

			return start - stack.count;
		}

		private static int MoveStackToArray(ItemStack[] sourceItems, int sourceIndex, ItemStack[] destItems, PackedBoolArray destLocks, bool likeItemsOnly)
		{
			ItemStack stack = sourceItems[sourceIndex];
			if (stack == null || stack.IsEmpty())
			{
				return 0;
			}

			int start = stack.count;
			bool foundMatch = false;
			for (int t = 0; t < destItems.Length; t++)
			{
				if (IsLocked(destLocks, t))
				{
					continue;
				}

				if (destItems[t] == null || destItems[t].IsEmpty())
				{
					continue;
				}

				if (destItems[t].itemValue.ItemClass != stack.itemValue.ItemClass)
				{
					continue;
				}

				foundMatch = true;
				int space = destItems[t].itemValue.ItemClass.Stacknumber.Value - destItems[t].count;
				if (space <= 0)
				{
					continue;
				}

				int take = stack.count < space ? stack.count : space;
				destItems[t].count += take;
				stack.count -= take;
				if (stack.count <= 0)
				{
					sourceItems[sourceIndex] = ItemStack.Empty;
					return start;
				}
			}

			if (likeItemsOnly && !foundMatch)
			{
				return start - stack.count;
			}

			if (foundMatch || !likeItemsOnly)
			{
				for (int t = 0; t < destItems.Length; t++)
				{
					if (IsLocked(destLocks, t))
					{
						continue;
					}

					if (destItems[t] == null || destItems[t].IsEmpty())
					{
						destItems[t] = stack.Clone();
						sourceItems[sourceIndex] = ItemStack.Empty;
						return start;
					}
				}
			}

			return start - stack.count;
		}

		private static bool IsLocked(PackedBoolArray locks, int index)
		{
			return locks != null && index >= 0 && index < locks.Length && locks[index];
		}

		private static void SortDest(ITileEntityLootable dest)
		{
			PackedBoolArray locked = dest.HasSlotLocksSupport ? dest.SlotLocks : null;
			dest.items = StackSortUtil.CombineAndSortStacks(dest.items, 0, locked);
		}

		private static void SortBag(Bag dest)
		{
			ItemStack[] slots = dest.GetSlots();
			if (slots == null)
			{
				return;
			}

			dest.SetSlots(StackSortUtil.CombineAndSortStacks(slots, 0, dest.LockedSlots));
		}
	}
}
