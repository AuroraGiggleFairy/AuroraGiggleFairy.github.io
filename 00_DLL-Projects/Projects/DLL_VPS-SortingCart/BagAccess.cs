using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SortingCart
{
	internal static class BagAccess
	{
		public static ItemStack[] GetSlots(Bag bag)
		{
			if (bag == null)
			{
				return null;
			}

			return GameVersion.UseV33 ? BagV33.GetSlots(bag) : BagV32.GetSlots(bag);
		}

		public static void SetSlot(Bag bag, int index, ItemStack stack)
		{
			if (bag == null)
			{
				return;
			}

			if (GameVersion.UseV33)
			{
				BagV33.SetSlot(bag, index, stack);
			}
			else
			{
				BagV32.SetSlot(bag, index, stack);
			}
		}

		public static void SetSlots(Bag bag, ItemStack[] slots)
		{
			if (bag == null)
			{
				return;
			}

			if (GameVersion.UseV33)
			{
				BagV33.SetSlots(bag, slots);
			}
			else
			{
				BagV32.SetSlots(bag, slots);
			}
		}

		public static PackedBoolArray LockedSlots(Bag bag)
		{
			if (bag == null)
			{
				return null;
			}

			return GameVersion.UseV33 ? BagV33.LockedSlots(bag) : BagV32.LockedSlots(bag);
		}

		private static class BagV33
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			public static ItemStack[] GetSlots(Bag bag)
			{
				return bag.ItemGrid?.items;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void SetSlot(Bag bag, int index, ItemStack stack)
			{
				bag.SetSlot(index, stack);
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void SetSlots(Bag bag, ItemStack[] slots)
			{
				bag.SetSlots(slots);
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static PackedBoolArray LockedSlots(Bag bag)
			{
				return bag.LockedSlots;
			}
		}

		private static class BagV32
		{
			private static MethodInfo getSlots;
			private static MethodInfo setSlot;
			private static MethodInfo setSlots;
			private static bool resolved;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static ItemStack[] GetSlots(Bag bag)
			{
				Resolve(bag);
				return getSlots?.Invoke(bag, null) as ItemStack[];
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void SetSlot(Bag bag, int index, ItemStack stack)
			{
				Resolve(bag);
				setSlot?.Invoke(bag, new object[] { index, stack, true });
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void SetSlots(Bag bag, ItemStack[] slots)
			{
				Resolve(bag);
				setSlots?.Invoke(bag, new object[] { slots });
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static PackedBoolArray LockedSlots(Bag bag)
			{
				return MemberAccess.Get(bag, "LockedSlots") as PackedBoolArray;
			}

			private static void Resolve(Bag bag)
			{
				if (resolved || bag == null)
				{
					return;
				}

				resolved = true;
				Type type = bag.GetType();
				getSlots = type.GetMethod("GetSlots", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
				setSlot = type.GetMethod("SetSlot", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(int), typeof(ItemStack), typeof(bool) }, null);
				setSlots = type.GetMethod("SetSlots", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(ItemStack[]) }, null);
			}
		}
	}
}
