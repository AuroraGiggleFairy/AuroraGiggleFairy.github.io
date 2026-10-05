using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MultiLookStorage
{
	// 3.3 is the path this mod is written for (ItemGrid, PlayerOwned, OnLockRequestServer).
	// The V32 methods are the 3.0-3.2 behavior, kept here so one DLL still runs on those versions:
	// bPlayerStorage, items, SlotLocks, HasSlotLocksSupport, bag GetSlots/SetSlots, XUi.LootContainer as ITileEntityLootable.
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
				Console.WriteLine("MultiLookStorage: version read failed: " + ex.Message);
			}

			bool legacyLoot = Type.GetType("ITileEntityLootable, Assembly-CSharp") != null;
			UseV33 = major > 0 ? major > 3 || (major == 3 && minor >= 30) : !legacyLoot;
			Console.WriteLine("MultiLookStorage: game " + display + "; path=" + (UseV33 ? "3.3+" : "3.0-3.2"));
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

		internal static PackedBoolArray SlotLocks(TEFeatureStorage storage)
		{
			if (storage == null)
			{
				return null;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? LocksV33(storage) : LocksV32(storage);
		}

		internal static TEFeatureStorage LootOf(XUi xui)
		{
			if (xui == null)
			{
				return null;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? LootV33(xui) : LootV32(xui);
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
		private static TEFeatureStorage LootV33(XUi xui)
		{
			return xui.LootContainer;
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

		private static TEFeatureStorage LootV32(XUi xui)
		{
			return GameVersion.Member(typeof(XUi), xui, "LootContainer") as TEFeatureStorage;
		}

		internal static void SetDragStack(XUiC_DragAndDropWindow window, ItemStack stack)
		{
			if (window == null)
			{
				return;
			}

			GameVersion.Initialize();
			if (GameVersion.UseV33)
			{
				SetDragV33(window, stack);
			}
			else
			{
				typeof(XUiC_DragAndDropWindow).GetProperty("CurrentStack")?.SetValue(window, stack, null);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void SetDragV33(XUiC_DragAndDropWindow window, ItemStack stack)
		{
			window.SetCurrentStack(stack);
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

		internal static void Read(ItemStack stack, PooledBinaryReader reader)
		{
			if (stack == null || reader == null)
			{
				return;
			}

			GameVersion.Initialize();
			if (GameVersion.UseV33)
			{
				ReadV33(stack, reader);
			}
			else
			{
				typeof(ItemStack).GetMethod("Read", new[] { typeof(System.IO.BinaryReader) })?.Invoke(stack, new object[] { reader });
			}
		}

		internal static void Write(ItemStack stack, System.IO.BinaryWriter writer)
		{
			if (stack == null || writer == null)
			{
				return;
			}

			GameVersion.Initialize();
			if (GameVersion.UseV33)
			{
				WriteV33(stack, (PooledBinaryWriter)writer);
			}
			else
			{
				typeof(ItemStack).GetMethod("Write", new[] { typeof(System.IO.BinaryWriter) })?.Invoke(stack, new object[] { writer });
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void ReadV33(ItemStack stack, PooledBinaryReader reader)
		{
			stack.Read(reader);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void WriteV33(ItemStack stack, PooledBinaryWriter writer)
		{
			stack.Write(writer);
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
