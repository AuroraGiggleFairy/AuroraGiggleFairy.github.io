namespace SortingCart
{
	internal static class ItemMover
	{
		public static int MoveLikeItems(StorageBox source, StorageBox dest, bool likeItemsOnly)
		{
			if (source?.Items == null || dest?.Items == null)
			{
				return 0;
			}

			int moved = 0;
			for (int s = 0; s < source.Items.Length; s++)
			{
				if (ItemStack.Empty.Equals(source.Items[s]) || source.IsSlotLocked(s))
				{
					continue;
				}

				moved += MoveStack(source.Items, s, dest, likeItemsOnly, () => source.UpdateSlot(s, source.Items[s]));
			}

			if (moved > 0)
			{
				source.SetModified();
				dest.SetModified();
				SortDest(dest);
			}

			return moved;
		}

		public static int MoveLikeItemsFromBag(Bag bag, StorageBox dest, bool likeItemsOnly)
		{
			if (bag == null || dest?.Items == null)
			{
				return 0;
			}

			ItemStack[] slots = BagAccess.GetSlots(bag);
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

				int before = StackAccess.Count(slots[s]);
				MoveStack(slots, s, dest, likeItemsOnly, null);
				int after = ItemStack.Empty.Equals(slots[s]) ? 0 : StackAccess.Count(slots[s]);
				if (after != before)
				{
					BagAccess.SetSlot(bag, s, after == 0 ? ItemStack.Empty : slots[s]);
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

			ItemStack[] sourceSlots = BagAccess.GetSlots(source);
			ItemStack[] destSlots = BagAccess.GetSlots(dest);
			if (sourceSlots == null || destSlots == null)
			{
				return 0;
			}

			int moved = MoveLikeItemsToBagSlots(sourceSlots, source, destSlots, BagAccess.LockedSlots(dest), likeItemsOnly);
			if (moved > 0)
			{
				BagAccess.SetSlots(dest, destSlots);
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

				int before = StackAccess.Count(sourceSlots[s]);
				MoveStackToArray(sourceSlots, s, destItems, destLocks, likeItemsOnly);
				int after = ItemStack.Empty.Equals(sourceSlots[s]) ? 0 : StackAccess.Count(sourceSlots[s]);
				if (after != before)
				{
					BagAccess.SetSlot(source, s, after == 0 ? ItemStack.Empty : sourceSlots[s]);
					moved += before - after;
				}
			}

			return moved;
		}

		public static int MoveLikeItemsToBagSlots(StorageBox source, ItemStack[] destItems, PackedBoolArray destLocks, bool likeItemsOnly)
		{
			if (source?.Items == null || destItems == null)
			{
				return 0;
			}

			int moved = 0;
			for (int s = 0; s < source.Items.Length; s++)
			{
				if (ItemStack.Empty.Equals(source.Items[s]) || source.IsSlotLocked(s))
				{
					continue;
				}

				moved += MoveStackToArray(source.Items, s, destItems, destLocks, likeItemsOnly);
			}

			if (moved > 0)
			{
				source.SetModified();
			}

			return moved;
		}

		private static int MoveStack(ItemStack[] sourceItems, int sourceIndex, StorageBox dest, bool likeItemsOnly, System.Action afterChange)
		{
			ItemStack stack = sourceItems[sourceIndex];
			if (stack == null || stack.IsEmpty())
			{
				return 0;
			}

			int start = StackAccess.Count(stack);
			bool foundMatch = false;
			ItemValue stackValue = StackAccess.Value(stack);
			for (int t = 0; t < dest.Items.Length; t++)
			{
				if (dest.IsSlotLocked(t))
				{
					continue;
				}

				ItemValue destValue = StackAccess.Value(dest.Items[t]);
				if (destValue == null || stackValue == null || destValue.ItemClass != stackValue.ItemClass)
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
				return start - StackAccess.Count(stack);
			}

			if ((foundMatch || !likeItemsOnly) && !stack.IsEmpty() && dest.AddItem(stack))
			{
				sourceItems[sourceIndex] = ItemStack.Empty;
				afterChange?.Invoke();
				return start;
			}

			return start - StackAccess.Count(stack);
		}

		private static int MoveStackToArray(ItemStack[] sourceItems, int sourceIndex, ItemStack[] destItems, PackedBoolArray destLocks, bool likeItemsOnly)
		{
			ItemStack stack = sourceItems[sourceIndex];
			if (stack == null || stack.IsEmpty())
			{
				return 0;
			}

			int start = StackAccess.Count(stack);
			ItemValue stackValue = StackAccess.Value(stack);
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

				ItemValue destValue = StackAccess.Value(destItems[t]);
				if (destValue == null || stackValue == null || destValue.ItemClass != stackValue.ItemClass)
				{
					continue;
				}

				foundMatch = true;
				int space = destValue.ItemClass.Stacknumber.Value - StackAccess.Count(destItems[t]);
				if (space <= 0)
				{
					continue;
				}

				int stackCount = StackAccess.Count(stack);
				int take = stackCount < space ? stackCount : space;
				StackAccess.SetCount(destItems[t], StackAccess.Count(destItems[t]) + take);
				StackAccess.SetCount(stack, stackCount - take);
				if (StackAccess.Count(stack) <= 0)
				{
					sourceItems[sourceIndex] = ItemStack.Empty;
					return start;
				}
			}

			if (likeItemsOnly && !foundMatch)
			{
				return start - StackAccess.Count(stack);
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

			return start - StackAccess.Count(stack);
		}

		private static bool IsLocked(PackedBoolArray locks, int index)
		{
			return locks != null && index >= 0 && index < locks.Length && locks[index];
		}

		private static void SortDest(StorageBox dest)
		{
			dest.ApplySortedItems(StackSortUtil.CombineAndSortStacks(dest.Items, 0, dest.SlotLocks));
		}

		private static void SortBag(Bag dest)
		{
			ItemStack[] slots = BagAccess.GetSlots(dest);
			if (slots == null)
			{
				return;
			}

			BagAccess.SetSlots(dest, StackSortUtil.CombineAndSortStacks(slots, 0, BagAccess.LockedSlots(dest)));
		}
	}
}
