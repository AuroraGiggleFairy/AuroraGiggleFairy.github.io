using System;
using System.Reflection;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// 3.2 stores these as fields. 3.3 exposes them as properties.
	/// A direct read compiles to only one of those and breaks the other version.
	/// ItemStack.itemValue and count are that split. So is the toolbelt and bag grid:
	/// 3.3 Inventory and Bag expose ItemGrid, and 3.2 walks GetSlots or Bag.items.
	/// </summary>
	internal static class ItemInventoryAccess
	{
		private static readonly PropertyInfo StackItemValue = typeof(ItemStack).GetProperty("itemValue");
		private static readonly FieldInfo StackItemValueField = typeof(ItemStack).GetField("itemValue");
		private static readonly PropertyInfo StackCount = typeof(ItemStack).GetProperty("count");
		private static readonly FieldInfo StackCountField = typeof(ItemStack).GetField("count");
		private static readonly PropertyInfo InventoryGrid = typeof(Inventory).GetProperty("ItemGrid");
		private static readonly MethodInfo InventorySlots = typeof(Inventory).GetMethod("GetSlots", Type.EmptyTypes);
		private static readonly PropertyInfo BagGrid = typeof(Bag).GetProperty("ItemGrid");
		private static readonly FieldInfo BagGridField = typeof(Bag).GetField("itemGrid");
		private static readonly FieldInfo BagItems = typeof(Bag).GetField("items");
		private static readonly MethodInfo BagSlots = typeof(Bag).GetMethod("GetSlots", Type.EmptyTypes);
		private static readonly PropertyInfo HolderProperty = typeof(ItemInventoryData).GetProperty("holdingEntity");
		private static readonly FieldInfo HolderField = typeof(ItemInventoryData).GetField("holdingEntity");

		internal static EntityAlive Holding(ItemInventoryData data)
		{
			if (data == null)
			{
				return null;
			}

			if (HolderProperty != null)
			{
				return (EntityAlive)HolderProperty.GetValue(data, null);
			}

			return HolderField != null ? (EntityAlive)HolderField.GetValue(data) : null;
		}

		internal static int MetaOf(ItemValue value)
		{
			if (value == null)
			{
				return 0;
			}

			PropertyInfo property = typeof(ItemValue).GetProperty("Meta");
			if (property != null)
			{
				return Convert.ToInt32(property.GetValue(value, null));
			}

			FieldInfo field = typeof(ItemValue).GetField("Meta") ?? typeof(ItemValue).GetField("meta");
			return field != null ? Convert.ToInt32(field.GetValue(value)) : 0;
		}

		internal static void SetMeta(ItemValue value, int meta)
		{
			if (value == null)
			{
				return;
			}

			PropertyInfo property = typeof(ItemValue).GetProperty("Meta");
			if (property != null)
			{
				property.SetValue(value, meta, null);
				return;
			}

			FieldInfo field = typeof(ItemValue).GetField("Meta") ?? typeof(ItemValue).GetField("meta");
			if (field != null)
			{
				field.SetValue(value, meta);
			}
		}

		internal static int AmmoIndex(ItemValue value)
		{
			if (value == null)
			{
				return 0;
			}

			PropertyInfo property = typeof(ItemValue).GetProperty("SelectedAmmoTypeIndex");
			if (property != null)
			{
				return Convert.ToInt32(property.GetValue(value, null));
			}

			FieldInfo field = typeof(ItemValue).GetField("SelectedAmmoTypeIndex");
			if (field == null)
			{
				field = typeof(ItemValue).GetField("selectedAmmoTypeIndex");
			}

			return field != null ? Convert.ToInt32(field.GetValue(value)) : 0;
		}

		internal static void SetAmmoIndex(ItemValue value, int index)
		{
			if (value == null)
			{
				return;
			}

			byte ammo = (byte)index;
			PropertyInfo property = typeof(ItemValue).GetProperty("SelectedAmmoTypeIndex");
			if (property != null)
			{
				property.SetValue(value, ammo, null);
				return;
			}

			FieldInfo field = typeof(ItemValue).GetField("SelectedAmmoTypeIndex")
				?? typeof(ItemValue).GetField("selectedAmmoTypeIndex");
			if (field != null)
			{
				field.SetValue(value, ammo);
			}
		}

		internal static ItemValue ItemOf(ItemStack stack)
		{
			if (stack == null)
			{
				return null;
			}

			if (StackItemValue != null)
			{
				return (ItemValue)StackItemValue.GetValue(stack, null);
			}

			return StackItemValueField != null ? (ItemValue)StackItemValueField.GetValue(stack) : null;
		}

		internal static int CountOf(ItemStack stack)
		{
			if (stack == null)
			{
				return 0;
			}

			if (StackCount != null)
			{
				return Convert.ToInt32(StackCount.GetValue(stack, null));
			}

			return StackCountField != null ? Convert.ToInt32(StackCountField.GetValue(stack)) : 0;
		}

		internal static void SetCount(ItemStack stack, int count)
		{
			if (stack == null)
			{
				return;
			}

			if (StackCount != null)
			{
				StackCount.SetValue(stack, count, null);
				return;
			}

			if (StackCountField != null)
			{
				StackCountField.SetValue(stack, count);
			}
		}

		internal static ItemStack[] SlotsOf(Inventory inventory)
		{
			if (inventory == null)
			{
				return null;
			}

			if (InventoryGrid != null)
			{
				return ItemsOf(InventoryGrid.GetValue(inventory, null));
			}

			return InventorySlots != null ? InventorySlots.Invoke(inventory, null) as ItemStack[] : null;
		}

		internal static ItemStack[] SlotsOf(Bag bag)
		{
			if (bag == null)
			{
				return null;
			}

			if (BagGrid != null)
			{
				return ItemsOf(BagGrid.GetValue(bag, null));
			}

			if (BagGridField != null)
			{
				return ItemsOf(BagGridField.GetValue(bag));
			}

			if (BagItems != null)
			{
				return BagItems.GetValue(bag) as ItemStack[];
			}

			return BagSlots != null ? BagSlots.Invoke(bag, null) as ItemStack[] : null;
		}

		internal static object GridOf(Inventory inventory)
		{
			if (inventory == null || InventoryGrid == null)
			{
				return null;
			}

			return InventoryGrid.GetValue(inventory, null);
		}

		internal static object GridOf(Bag bag)
		{
			if (bag == null)
			{
				return null;
			}

			if (BagGrid != null)
			{
				return BagGrid.GetValue(bag, null);
			}

			return BagGridField != null ? BagGridField.GetValue(bag) : null;
		}

		internal static void NotifySlot(object grid, int index)
		{
			if (grid == null)
			{
				return;
			}

			MethodInfo method = grid.GetType().GetMethod("OnSlotChanged", new Type[] { typeof(int) });
			if (method != null)
			{
				method.Invoke(grid, new object[] { index });
			}
		}

		private static ItemStack[] ItemsOf(object grid)
		{
			if (grid == null)
			{
				return null;
			}

			FieldInfo items = grid.GetType().GetField("items");
			if (items != null)
			{
				return items.GetValue(grid) as ItemStack[];
			}

			PropertyInfo itemsProperty = grid.GetType().GetProperty("Items");
			return itemsProperty != null ? itemsProperty.GetValue(grid, null) as ItemStack[] : null;
		}
	}
}
