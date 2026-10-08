using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ExpandedInteractionPrompts
{
	/// <summary>
	/// 3.2 stores ItemStack count/itemValue and ItemValue.Quality as fields.
	/// 3.3 exposes them as properties. A direct read compiles to only one of those.
	/// </summary>
	internal static class StackAccess
	{
		internal static int Count(ItemStack stack)
		{
			if (stack == null)
			{
				return 0;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? StackV33.Count(stack) : StackV32.Count(stack);
		}

		internal static ItemValue Value(ItemStack stack)
		{
			if (stack == null)
			{
				return null;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? StackV33.Value(stack) : StackV32.Value(stack);
		}

		internal static int Quality(ItemValue value)
		{
			if (value == null)
			{
				return 0;
			}

			GameVersion.Initialize();
			return GameVersion.UseV33 ? StackV33.Quality(value) : StackV32.Quality(value);
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
			public static int Quality(ItemValue value)
			{
				return value.Quality;
			}
		}

		private static class StackV32
		{
			private static FieldInfo countField;
			private static FieldInfo valueField;
			private static FieldInfo qualityField;
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
			public static int Quality(ItemValue value)
			{
				Resolve();
				object raw = qualityField?.GetValue(value);
				return raw == null ? 0 : Convert.ToInt32(raw);
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
				qualityField = typeof(ItemValue).GetField("Quality", flags);
			}
		}
	}
}
