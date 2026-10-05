using System.Reflection;
using System.Runtime.CompilerServices;

namespace FuelAutoShutOff
{
	/// <summary>
	/// 3.2 stores ItemStack count/itemValue and ItemValue.type as fields.
	/// 3.3 exposes them as properties. A direct read compiles to only one of those.
	/// </summary>
	internal static class StackAccess
	{
		internal static int Count(ItemStack stack)
		{
			GameVersion.Initialize();
			return GameVersion.UseV33 ? StackV33.Count(stack) : StackV32.Count(stack);
		}

		internal static ItemValue Value(ItemStack stack)
		{
			GameVersion.Initialize();
			return GameVersion.UseV33 ? StackV33.Value(stack) : StackV32.Value(stack);
		}

		internal static int TypeId(ItemValue value)
		{
			if (value == null)
			{
				return 0;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? StackV33.TypeId(value) : StackV32.TypeId(value);
		}

		private static class StackV33
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			public static int Count(ItemStack stack)
			{
				return stack.count;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static ItemValue Value(ItemStack stack)
			{
				return stack.itemValue;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static int TypeId(ItemValue value)
			{
				return value.type;
			}
		}

		private static class StackV32
		{
			private static FieldInfo countField;
			private static FieldInfo valueField;
			private static FieldInfo typeField;
			private static bool resolved;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static int Count(ItemStack stack)
			{
				Resolve();
				return countField?.GetValue(stack) is int count ? count : 0;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static ItemValue Value(ItemStack stack)
			{
				Resolve();
				return valueField?.GetValue(stack) as ItemValue;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static int TypeId(ItemValue value)
			{
				Resolve();
				return typeField?.GetValue(value) is int type ? type : 0;
			}

			private static void Resolve()
			{
				if (resolved)
				{
					return;
				}

				resolved = true;
				const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				countField = typeof(ItemStack).GetField("count", flags);
				valueField = typeof(ItemStack).GetField("itemValue", flags);
				typeField = typeof(ItemValue).GetField("type", flags);
			}
		}
	}
}
