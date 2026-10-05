using System.Reflection;

internal static class ItemStacks
{
	private static PropertyInfo property;
	private static FieldInfo field;
	private static bool resolved;

	public static ItemValue Value(ItemStack stack)
	{
		if (stack == null || stack.IsEmpty())
		{
			return null;
		}

		if (!resolved)
		{
			resolved = true;
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			property = typeof(ItemStack).GetProperty("itemValue", flags);
			if (property == null)
			{
				field = typeof(ItemStack).GetField("itemValue", flags);
			}
		}

		if (property != null)
		{
			return property.GetValue(stack) as ItemValue;
		}

		return field?.GetValue(stack) as ItemValue;
	}
}
