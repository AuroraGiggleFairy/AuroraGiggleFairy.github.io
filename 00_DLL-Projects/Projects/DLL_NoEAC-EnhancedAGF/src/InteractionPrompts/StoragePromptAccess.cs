using System.Reflection;

namespace ExpandedInteractionPrompts
{
	internal static class StoragePromptAccess
	{
		private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private static PropertyInfo itemGridProperty;
		private static bool resolved;

		public static ItemStack[] Items(TEFeatureStorage storage)
		{
			if (storage == null)
				return null;

			object grid = Grid(storage);
			if (grid != null)
				return Member(grid, "items") as ItemStack[];

			return Member(storage, "items") as ItemStack[];
		}

		public static bool IsPlayerStorage(TEFeatureStorage storage)
		{
			if (storage == null)
				return false;

			object grid = Grid(storage);
			object value = grid != null ? Member(grid, "PlayerOwned") : Member(storage, "bPlayerStorage");
			return value is bool owned && owned;
		}

		public static bool IsTouched(TEFeatureStorage storage)
		{
			if (storage == null)
				return false;

			object grid = Grid(storage);
			object value = grid != null ? Member(grid, "Touched") : Member(storage, "bTouched");
			return value is bool touched && touched;
		}

		public static ulong WorldTimeTouched(TEFeatureStorage storage)
		{
			if (storage == null)
				return 0;

			object grid = Grid(storage);
			object value = grid != null ? Member(grid, "WorldTimeTouched") : Member(storage, "worldTimeTouched");
			return value is ulong time ? time : 0;
		}

		public static ItemStack[] BagSlots(object bag)
		{
			if (bag == null)
				return null;

			MethodInfo getSlots = bag.GetType().GetMethod("GetSlots", Flags, null, System.Type.EmptyTypes, null);
			if (getSlots != null)
				return getSlots.Invoke(bag, null) as ItemStack[];

			object grid = bag.GetType().GetProperty("ItemGrid", Flags)?.GetValue(bag, null);
			return grid == null ? null : Member(grid, "items") as ItemStack[];
		}

		private static object Grid(TEFeatureStorage storage)
		{
			if (!resolved)
			{
				resolved = true;
				itemGridProperty = typeof(TEFeatureStorage).GetProperty("ItemGrid", Flags);
			}

			return itemGridProperty?.GetValue(storage, null);
		}

		private static object Member(object target, string name)
		{
			if (target == null)
				return null;

			PropertyInfo property = target.GetType().GetProperty(name, Flags);
			if (property != null)
				return property.GetValue(target, null);

			return target.GetType().GetField(name, Flags)?.GetValue(target);
		}
	}
}
