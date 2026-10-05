using System;
using System.Reflection;
using System.Runtime.CompilerServices;

// One DLL for 3.0–3.2 and 3.3+. Direct 3.3 calls stay in NoInlining helpers so a 3.2
// game never JITs them. 3.2-only members are read by reflection.
internal static class TractorCompat
{
	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	internal static bool UseV33 { get; private set; }

	internal static string DisplayString { get; private set; } = "Unknown";

	internal static void Initialize()
	{
		ReadVersion();
		bool legacyMods = typeof(Vehicle).GetMethod("SetItemValueMods", Flags) != null;
		if (DisplayString != "Unknown")
		{
			int major = ReadMajor();
			int minor = ReadMinor();
			UseV33 = major > 3 || (major == 3 && minor >= 30);
		}
		else
		{
			UseV33 = !legacyMods;
		}

		Log.Out("[LawnTractorV3Fix] game " + DisplayString + "; path=" + (UseV33 ? "3.3+" : "pre-3.3"));
	}

	internal static void ApplyMowerMods(Vehicle vehicle, ItemValue itemValue)
	{
		if (vehicle != null && vehicle.FindPart("mower") is VPMower mower)
		{
			mower.UpdateModifications(Modifications(itemValue));
		}
	}

	internal static ItemValue[] Modifications(ItemValue itemValue)
	{
		if (itemValue == null)
		{
			return null;
		}

		return UseV33 ? V33.Modifications(itemValue) : V32.Modifications(itemValue);
	}

	internal static ItemStack[] GetSlots(Bag bag)
	{
		if (bag == null)
		{
			return null;
		}

		return UseV33 ? V33.GetSlots(bag) : V32.GetSlots(bag);
	}

	internal static void SetSlots(Bag bag, ItemStack[] slots)
	{
		if (bag == null || slots == null)
		{
			return;
		}

		if (UseV33)
		{
			V33.SetSlots(bag, slots);
		}
		else
		{
			V32.SetSlots(bag, slots);
		}
	}

	internal static int ItemType(ItemValue itemValue)
	{
		if (itemValue == null)
		{
			return 0;
		}

		return UseV33 ? V33.ItemType(itemValue) : V32.ItemType(itemValue);
	}

	internal static ItemValue StackValue(ItemStack stack)
	{
		if (stack == null)
		{
			return null;
		}

		return UseV33 ? V33.StackValue(stack) : V32.StackValue(stack);
	}

	internal static int StackCount(ItemStack stack)
	{
		if (stack == null)
		{
			return 0;
		}

		return UseV33 ? V33.StackCount(stack) : V32.StackCount(stack);
	}

	internal static void SetStack(ItemStack stack, ItemValue itemValue, int count)
	{
		if (stack == null)
		{
			return;
		}

		if (UseV33)
		{
			V33.SetStack(stack, itemValue, count);
		}
		else
		{
			V32.SetStack(stack, itemValue, count);
		}
	}

	private static int major;
	private static int minor;

	private static int ReadMajor()
	{
		return major;
	}

	private static int ReadMinor()
	{
		return minor;
	}

	private static void ReadVersion()
	{
		try
		{
			Type constantsType = Type.GetType("Constants, Assembly-CSharp");
			if (constantsType == null)
			{
				return;
			}

			object versionInfo = GetStatic(constantsType, "cVersionInformation");
			if (versionInfo != null)
			{
				major = AsInt(GetInstance(versionInfo, "Major"));
				minor = AsInt(GetInstance(versionInfo, "Minor"));
				string text = GetInstance(versionInfo, "LongString") as string;
				if (!string.IsNullOrEmpty(text))
				{
					DisplayString = text;
					return;
				}
			}

			major = AsInt(GetStatic(constantsType, "cVersionMajor"));
			minor = AsInt(GetStatic(constantsType, "cVersionMinor"));
			if (major > 0)
			{
				DisplayString = "V " + major + "." + minor;
			}
		}
		catch (Exception exception)
		{
			Log.Warning("[LawnTractorV3Fix] Could not read the game version.");
			Log.Exception(exception);
		}
	}

	private static object GetStatic(Type type, string name)
	{
		PropertyInfo property = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null)
		{
			return property.GetValue(null, null);
		}

		FieldInfo field = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		return field != null ? field.GetValue(null) : null;
	}

	private static object GetInstance(object instance, string name)
	{
		Type type = instance.GetType();
		PropertyInfo property = type.GetProperty(name, Flags);
		if (property != null && property.GetIndexParameters().Length == 0)
		{
			return property.GetValue(instance, null);
		}

		FieldInfo field = type.GetField(name, Flags);
		return field != null ? field.GetValue(instance) : null;
	}

	private static int AsInt(object value)
	{
		return value is int number ? number : 0;
	}

	private static class V33
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ItemValue[] Modifications(ItemValue itemValue)
		{
			int count = itemValue.ModificationCount;
			ItemValue[] mods = new ItemValue[count];
			for (int i = 0; i < count; i++)
			{
				mods[i] = itemValue.GetModification(i);
			}

			return mods;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ItemStack[] GetSlots(Bag bag)
		{
			return bag.ItemGrid != null ? bag.ItemGrid.items : null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void SetSlots(Bag bag, ItemStack[] slots)
		{
			bag.SetSlots(slots, false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int ItemType(ItemValue itemValue)
		{
			return itemValue.type;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ItemValue StackValue(ItemStack stack)
		{
			return stack.itemValue;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int StackCount(ItemStack stack)
		{
			return stack.count;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void SetStack(ItemStack stack, ItemValue itemValue, int count)
		{
			stack.itemValue = itemValue;
			stack.count = count;
		}
	}

	private static class V32
	{
		private static FieldInfo modifications;
		private static FieldInfo itemType;
		private static FieldInfo stackValue;
		private static FieldInfo stackCount;
		private static MethodInfo getSlots;
		private static MethodInfo setSlots;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ItemValue[] Modifications(ItemValue itemValue)
		{
			if (modifications == null)
			{
				modifications = typeof(ItemValue).GetField("Modifications", Flags);
			}

			return modifications != null ? modifications.GetValue(itemValue) as ItemValue[] : null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ItemStack[] GetSlots(Bag bag)
		{
			if (getSlots == null)
			{
				getSlots = typeof(Bag).GetMethod("GetSlots", Flags, null, Type.EmptyTypes, null);
			}

			return getSlots != null ? getSlots.Invoke(bag, null) as ItemStack[] : null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void SetSlots(Bag bag, ItemStack[] slots)
		{
			if (setSlots == null)
			{
				setSlots = typeof(Bag).GetMethod("SetSlots", Flags, null, new Type[] { typeof(ItemStack[]) }, null);
			}

			if (setSlots != null)
			{
				setSlots.Invoke(bag, new object[] { slots });
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int ItemType(ItemValue itemValue)
		{
			if (itemType == null)
			{
				itemType = typeof(ItemValue).GetField("type", Flags);
			}

			object value = itemType != null ? itemType.GetValue(itemValue) : null;
			return value is int number ? number : 0;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ItemValue StackValue(ItemStack stack)
		{
			if (stackValue == null)
			{
				stackValue = typeof(ItemStack).GetField("itemValue", Flags);
			}

			return stackValue != null ? stackValue.GetValue(stack) as ItemValue : null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int StackCount(ItemStack stack)
		{
			if (stackCount == null)
			{
				stackCount = typeof(ItemStack).GetField("count", Flags);
			}

			object value = stackCount != null ? stackCount.GetValue(stack) : null;
			return value is int number ? number : 0;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void SetStack(ItemStack stack, ItemValue itemValue, int count)
		{
			if (stackValue == null)
			{
				stackValue = typeof(ItemStack).GetField("itemValue", Flags);
			}

			if (stackCount == null)
			{
				stackCount = typeof(ItemStack).GetField("count", Flags);
			}

			if (stackValue != null)
			{
				stackValue.SetValue(stack, itemValue);
			}

			if (stackCount != null)
			{
				stackCount.SetValue(stack, count);
			}
		}
	}
}
