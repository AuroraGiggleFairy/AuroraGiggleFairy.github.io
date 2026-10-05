using System.Reflection;

public static class StackCompat
{
	private static PropertyInfo countProperty;
	private static FieldInfo countField;
	private static PropertyInfo valueProperty;
	private static FieldInfo valueField;
	private static bool resolved;

	public static int Count(ItemStack stack)
	{
		if (stack == null)
		{
			return 0;
		}

		Ensure();
		object value = countProperty != null ? countProperty.GetValue(stack, null) : countField?.GetValue(stack);
		return value is int count ? count : 0;
	}

	public static void SetCount(ItemStack stack, int count)
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

	public static ItemValue Value(ItemStack stack)
	{
		if (stack == null)
		{
			return null;
		}

		Ensure();
		object value = valueProperty != null ? valueProperty.GetValue(stack, null) : valueField?.GetValue(stack);
		return value as ItemValue;
	}

	public static ItemStack[] Slots(object holder)
	{
		if (holder == null)
		{
			return null;
		}

		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		MethodInfo getSlots = holder.GetType().GetMethod("GetSlots", flags, null, System.Type.EmptyTypes, null);
		if (getSlots != null)
		{
			return getSlots.Invoke(holder, null) as ItemStack[];
		}

		object grid = holder.GetType().GetProperty("ItemGrid", flags)?.GetValue(holder, null);
		if (grid == null)
		{
			return null;
		}

		return grid.GetType().GetField("items", flags)?.GetValue(grid) as ItemStack[];
	}

	public static int IntMember(object holder, string name)
	{
		if (holder == null)
		{
			return 0;
		}

		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		PropertyInfo property = holder.GetType().GetProperty(name, flags);
		if (property != null)
		{
			object value = property.GetValue(holder, null);
			return value is int number ? number : 0;
		}

		FieldInfo field = holder.GetType().GetField(name, flags);
		if (field != null)
		{
			object value = field.GetValue(holder);
			return value is int number ? number : 0;
		}

		return 0;
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
