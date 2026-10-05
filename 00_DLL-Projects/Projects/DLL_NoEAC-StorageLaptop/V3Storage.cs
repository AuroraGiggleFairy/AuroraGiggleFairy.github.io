using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StorageLaptop
{
	// 3.3 is the path this mod is written for (ItemGrid, PlayerOwned).
	// The V32 methods are the 3.0-3.2 behavior, kept here so one DLL still runs on those versions:
	// bPlayerStorage, items, SlotLocks, HasSlotLocksSupport, bag GetSlots/SetSlots/LockedSlots.
	internal static class GameVersion
	{
		private static bool initialized;

		internal static bool UseV33 { get; private set; }

		internal static void Initialize()
		{
			if (initialized)
			{
				return;
			}

			initialized = true;
			int major = 0;
			int minor = 0;
			string display = "Unknown";
			try
			{
				Type constantsType = Type.GetType("Constants, Assembly-CSharp");
				object versionInfo = constantsType != null ? Member(constantsType, null, "cVersionInformation") : null;
				if (versionInfo != null)
				{
					major = AsInt(Member(versionInfo.GetType(), versionInfo, "Major"));
					minor = AsInt(Member(versionInfo.GetType(), versionInfo, "Minor"));
					display = Member(versionInfo.GetType(), versionInfo, "LongString") as string ?? display;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[StorageLaptop] version read failed: " + ex.Message);
			}

			bool legacyLoot = Type.GetType("ITileEntityLootable, Assembly-CSharp") != null;
			UseV33 = major > 0 ? major > 3 || (major == 3 && minor >= 30) : !legacyLoot;
			Console.WriteLine("[StorageLaptop] game " + display + "; path=" + (UseV33 ? "3.3+" : "3.0-3.2"));
		}

		private static int AsInt(object value)
		{
			return value is int number ? number : 0;
		}

		internal static object Member(Type type, object instance, string name)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo property = type.GetProperty(name, flags);
			if (property != null)
			{
				return property.GetValue(instance, null);
			}

			return type.GetField(name, flags)?.GetValue(instance);
		}
	}

	internal static class StorageAccess
	{
		internal static bool IsPlayerOwned(TEFeatureStorage storage)
		{
			if (storage == null)
			{
				return false;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? OwnedV33(storage) : OwnedV32(storage);
		}

		internal static ItemStack[] Items(TEFeatureStorage storage)
		{
			if (storage == null)
			{
				return null;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? ItemsV33(storage) : ItemsV32(storage);
		}

		internal static bool SlotLocked(TEFeatureStorage storage, int index)
		{
			if (storage == null || index < 0)
			{
				return false;
			}

			GameVersion.Initialize();
			PackedBoolArray locks = GameVersion.UseV33 ? LocksV33(storage) : LocksV32(storage);
			return locks != null && index < locks.Length && locks[index];
		}

		internal static ItemStack[] BagSlots(Bag bag)
		{
			if (bag == null)
			{
				return null;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? BagSlotsV33(bag) : BagSlotsV32(bag);
		}

		internal static void SetBagSlots(Bag bag, ItemStack[] slots)
		{
			if (bag == null)
			{
				return;
			}

			GameVersion.Initialize();
			if (GameVersion.UseV33)
			{
				SetBagSlotsV33(bag, slots);
			}
			else
			{
				SetBagSlotsV32(bag, slots);
			}
		}

		internal static bool BagSlotLocked(Bag bag, int index)
		{
			if (bag == null || index < 0)
			{
				return false;
			}

			GameVersion.Initialize();
			PackedBoolArray locks = GameVersion.UseV33 ? BagLocksV33(bag) : BagLocksV32(bag);
			return locks != null && index < locks.Length && locks[index];
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool OwnedV33(TEFeatureStorage storage)
		{
			ItemStackGrid grid = storage.ItemGrid;
			return grid != null && grid.PlayerOwned;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static ItemStack[] ItemsV33(TEFeatureStorage storage)
		{
			return storage.ItemGrid?.items;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static PackedBoolArray LocksV33(TEFeatureStorage storage)
		{
			return storage.ItemGrid?.SlotLocks;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static ItemStack[] BagSlotsV33(Bag bag)
		{
			return bag.ItemGrid?.items;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void SetBagSlotsV33(Bag bag, ItemStack[] slots)
		{
			bag.SetSlots(slots);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static PackedBoolArray BagLocksV33(Bag bag)
		{
			return bag.LockedSlots;
		}

		private static bool OwnedV32(TEFeatureStorage storage)
		{
			object value = GameVersion.Member(typeof(TEFeatureStorage), storage, "bPlayerStorage");
			return value is bool owned && owned;
		}

		private static ItemStack[] ItemsV32(TEFeatureStorage storage)
		{
			return GameVersion.Member(typeof(TEFeatureStorage), storage, "items") as ItemStack[];
		}

		private static PackedBoolArray LocksV32(TEFeatureStorage storage)
		{
			object support = GameVersion.Member(typeof(TEFeatureStorage), storage, "HasSlotLocksSupport");
			if (support is bool supported && !supported)
			{
				return null;
			}

			return GameVersion.Member(typeof(TEFeatureStorage), storage, "SlotLocks") as PackedBoolArray;
		}

		private static ItemStack[] BagSlotsV32(Bag bag)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			MethodInfo method = typeof(Bag).GetMethod("GetSlots", flags, null, Type.EmptyTypes, null);
			return method?.Invoke(bag, null) as ItemStack[];
		}

		private static void SetBagSlotsV32(Bag bag, ItemStack[] slots)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			MethodInfo method = typeof(Bag).GetMethod("SetSlots", flags, null, new[] { typeof(ItemStack[]) }, null);
			method?.Invoke(bag, new object[] { slots });
		}

		private static PackedBoolArray BagLocksV32(Bag bag)
		{
			return GameVersion.Member(typeof(Bag), bag, "LockedSlots") as PackedBoolArray;
		}
	}

	internal static class StackAccess
	{
		private static PropertyInfo countProperty;
		private static FieldInfo countField;
		private static PropertyInfo valueProperty;
		private static FieldInfo valueField;
		private static bool resolved;

		internal static int Count(ItemStack stack)
		{
			if (stack == null)
			{
				return 0;
			}

			Ensure();
			object value = countProperty != null ? countProperty.GetValue(stack, null) : countField?.GetValue(stack);
			return value is int count ? count : 0;
		}

		internal static void SetCount(ItemStack stack, int count)
		{
			if (stack == null)
			{
				return;
			}

			Ensure();
			if (countProperty != null && countProperty.CanWrite)
			{
				countProperty.SetValue(stack, count, null);
				return;
			}

			countField?.SetValue(stack, count);
		}

		internal static ItemValue Value(ItemStack stack)
		{
			if (stack == null)
			{
				return null;
			}

			Ensure();
			object value = valueProperty != null ? valueProperty.GetValue(stack, null) : valueField?.GetValue(stack);
			return value as ItemValue;
		}

		private static void Ensure()
		{
			if (resolved)
			{
				return;
			}

			resolved = true;
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			countProperty = typeof(ItemStack).GetProperty("count", flags);
			if (countProperty == null)
			{
				countField = typeof(ItemStack).GetField("count", flags);
			}

			valueProperty = typeof(ItemStack).GetProperty("itemValue", flags);
			if (valueProperty == null)
			{
				valueField = typeof(ItemStack).GetField("itemValue", flags);
			}
		}
	}
}
