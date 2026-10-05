using System.Reflection;
using System.Runtime.CompilerServices;

namespace SortingCart
{
	internal static class StackAccess
	{
		public static int Count(ItemStack stack)
		{
			return GameVersion.UseV33 ? StackV33.Count(stack) : StackV32.Count(stack);
		}

		public static void SetCount(ItemStack stack, int count)
		{
			if (GameVersion.UseV33)
			{
				StackV33.SetCount(stack, count);
			}
			else
			{
				StackV32.SetCount(stack, count);
			}
		}

		public static ItemValue Value(ItemStack stack)
		{
			return GameVersion.UseV33 ? StackV33.Value(stack) : StackV32.Value(stack);
		}

		private static class StackV33
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			public static int Count(ItemStack stack)
			{
				return stack.count;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void SetCount(ItemStack stack, int count)
			{
				stack.count = count;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static ItemValue Value(ItemStack stack)
			{
				return stack.itemValue;
			}
		}

		private static class StackV32
		{
			private static FieldInfo countField;
			private static FieldInfo valueField;
			private static bool resolved;

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static int Count(ItemStack stack)
			{
				Resolve();
				return countField?.GetValue(stack) is int count ? count : 0;
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static void SetCount(ItemStack stack, int count)
			{
				Resolve();
				countField?.SetValue(stack, count);
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			public static ItemValue Value(ItemStack stack)
			{
				Resolve();
				return valueField?.GetValue(stack) as ItemValue;
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
			}
		}
	}
}
