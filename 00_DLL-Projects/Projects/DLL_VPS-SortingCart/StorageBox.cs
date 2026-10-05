using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SortingCart
{
	internal sealed class StorageBox
	{
		private readonly TEFeatureStorage feature;
		private readonly object legacyLoot;

		private StorageBox(TEFeatureStorage feature, object legacyLoot)
		{
			this.feature = feature;
			this.legacyLoot = legacyLoot;
		}

		public static StorageBox FromFeature(TEFeatureStorage feature)
		{
			return feature == null ? null : new StorageBox(feature, null);
		}

		public static StorageBox TryLegacy(object loot)
		{
			if (loot == null || MemberAccess.Get(loot, "items") == null && MemberAccess.Get(loot, "itemsArr") == null)
			{
				return null;
			}

			return new StorageBox(null, loot);
		}

		private object Loot => (object)feature ?? legacyLoot;

		public ItemStack[] Items => GameVersion.UseV33 ? StorageV33.Items(feature) : StorageV32.Items(Loot);

		public bool IsPlayerStorage => GameVersion.UseV33 ? StorageV33.IsPlayerStorage(feature) : StorageV32.IsPlayerStorage(Loot);

		public PackedBoolArray SlotLocks => GameVersion.UseV33 ? StorageV33.SlotLocks(feature) : StorageV32.SlotLocks(Loot);

		public bool IsSlotLocked(int index)
		{
			return GameVersion.UseV33 ? StorageV33.IsSlotLocked(feature, index) : StorageV32.IsSlotLocked(Loot, index);
		}

		public void UpdateSlot(int index, ItemStack stack)
		{
			if (feature != null)
			{
				feature.UpdateSlot(index, stack);
				return;
			}

			StorageV32.Invoke(legacyLoot, "UpdateSlot", index, stack);
		}

		public (bool anyMoved, bool allMoved) TryStackItem(int startIndex, ItemStack stack)
		{
			if (feature != null)
			{
				return feature.TryStackItem(startIndex, stack);
			}

			object result = StorageV32.Invoke(legacyLoot, "TryStackItem", startIndex, stack);
			return result is System.ValueTuple<bool, bool> pair ? pair : default;
		}

		public bool AddItem(ItemStack stack)
		{
			if (feature != null)
			{
				return feature.AddItem(stack);
			}

			return StorageV32.Invoke(legacyLoot, "AddItem", stack) is bool added && added;
		}

		public void SetModified()
		{
			if (feature != null)
			{
				feature.SetModified();
				return;
			}

			StorageV32.Invoke(legacyLoot, "SetModified");
		}

		public bool IsEmpty()
		{
			if (feature != null)
			{
				return feature.IsEmpty();
			}

			return StorageV32.Invoke(legacyLoot, "IsEmpty") is bool empty && empty;
		}

		public void ApplySortedItems(ItemStack[] sorted)
		{
			if (GameVersion.UseV33)
			{
				StorageV33.ApplySortedItems(feature, sorted);
			}
			else
			{
				StorageV32.ApplySortedItems(Loot, sorted);
			}
		}

		public ILockTarget AsLockTarget()
		{
			return Loot as ILockTarget;
		}

		private static class StorageV33
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			public static ItemStack[] Items(TEFeatureStorage feature)
			{
				return feature?.ItemGrid?.items;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static bool IsPlayerStorage(TEFeatureStorage feature)
			{
				return feature?.ItemGrid != null && feature.ItemGrid.PlayerOwned;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static PackedBoolArray SlotLocks(TEFeatureStorage feature)
			{
				return feature?.ItemGrid?.SlotLocks;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static bool IsSlotLocked(TEFeatureStorage feature, int index)
			{
				return feature?.ItemGrid != null && feature.ItemGrid.IsLocked(index);
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void ApplySortedItems(TEFeatureStorage feature, ItemStack[] sorted)
			{
				if (feature?.ItemGrid != null)
				{
					feature.ItemGrid.items = sorted;
				}
			}
		}

		private static class StorageV32
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			public static ItemStack[] Items(object loot)
			{
				return MemberAccess.Get(loot, "items") as ItemStack[] ?? MemberAccess.Get(loot, "itemsArr") as ItemStack[];
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static bool IsPlayerStorage(object loot)
			{
				return MemberAccess.Get(loot, "bPlayerStorage") is bool owned && owned;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static PackedBoolArray SlotLocks(object loot)
			{
				if (MemberAccess.Get(loot, "HasSlotLocksSupport") is bool supported && !supported)
				{
					return null;
				}

				return MemberAccess.Get(loot, "SlotLocks") as PackedBoolArray;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static bool IsSlotLocked(object loot, int index)
			{
				PackedBoolArray locks = SlotLocks(loot);
				return locks != null && index >= 0 && index < locks.Length && locks[index];
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void ApplySortedItems(object loot, ItemStack[] sorted)
			{
				MemberAccess.Set(loot, "items", sorted);
				if (MemberAccess.Get(loot, "items") != sorted)
				{
					MemberAccess.Set(loot, "itemsArr", sorted);
				}
			}

			public static object Invoke(object loot, string name, params object[] args)
			{
				if (loot == null)
				{
					return null;
				}

				MethodInfo method = loot.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				return method?.Invoke(loot, args);
			}
		}
	}
}
