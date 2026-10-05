using System.Reflection;

namespace SortingCart
{
	internal static class MemberAccess
	{
		public static object Get(object target, string name)
		{
			if (target == null)
			{
				return null;
			}

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo property = target.GetType().GetProperty(name, flags);
			if (property != null)
			{
				return property.GetValue(target);
			}

			FieldInfo field = target.GetType().GetField(name, flags);
			return field?.GetValue(target);
		}

		public static void Set(object target, string name, object value)
		{
			if (target == null)
			{
				return;
			}

			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo property = target.GetType().GetProperty(name, flags);
			if (property != null && property.CanWrite)
			{
				property.SetValue(target, value);
				return;
			}

			FieldInfo field = target.GetType().GetField(name, flags);
			field?.SetValue(target, value);
		}
	}
}
