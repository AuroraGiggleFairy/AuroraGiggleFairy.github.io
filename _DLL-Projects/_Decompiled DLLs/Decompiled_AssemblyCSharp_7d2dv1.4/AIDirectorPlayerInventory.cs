using System;
using System.Collections.Generic;
using System.IO;

public struct AIDirectorPlayerInventory : IEquatable<AIDirectorPlayerInventory>
{
	public struct ItemId : IEquatable<ItemId>
	{
		public const int kNetworkSize = 4;

		public int id;

		public int count;

		public static ItemId FromStack(ItemStack stack)
		{
			ItemId result = default(ItemId);
			result.id = stack.itemValue.type;
			result.count = stack.count;
			return result;
		}

		public static ItemId Read(BinaryReader stream)
		{
			ItemId result = default(ItemId);
			result.id = stream.ReadInt16();
			result.count = stream.ReadInt16();
			return result;
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write((short)id);
			stream.Write((short)count);
		}

		public bool Equals(ItemId other)
		{
			if (id == other.id)
			{
				return count == other.count;
			}
			return false;
		}
	}

	public List<ItemId> bag;

	public List<ItemId> belt;

	public static AIDirectorPlayerInventory FromEntity(EntityAlive entity)
	{
		AIDirectorPlayerInventory result = default(AIDirectorPlayerInventory);
		result.bag = TrackedItemsFromBag(entity.bag);
		result.belt = TrackedItemsFromInventory(entity.inventory);
		return result;
	}

	public bool Equals(AIDirectorPlayerInventory other)
	{
		if (bag != null != (other.bag != null))
		{
			return false;
		}
		if (belt != null != (other.belt != null))
		{
			return false;
		}
		if (!OrderIndependantEquals(bag, other.bag))
		{
			return false;
		}
		return OrderIndependantEquals(belt, other.belt);
	}

	public static List<ItemId> TrackedItemsFromBag(Bag bag)
	{
		List<ItemId> list = null;
		ItemStack[] slots = bag.GetSlots();
		foreach (ItemStack itemStack in slots)
		{
			if (itemStack.IsEmpty())
			{
				continue;
			}
			ItemClass itemClass = itemStack.itemValue.ItemClass;
			if (itemClass != null && itemClass.Smell != null)
			{
				if (list == null)
				{
					list = new List<ItemId>();
				}
				AppendId(list, ItemId.FromStack(itemStack));
			}
		}
		return list;
	}

	public static List<ItemId> TrackedItemsFromInventory(Inventory inv)
	{
		List<ItemId> list = null;
		for (int i = 0; i < inv.GetItemCount(); i++)
		{
			ItemStack item = inv.GetItem(i);
			if (item.IsEmpty())
			{
				continue;
			}
			ItemClass itemClass = item.itemValue.ItemClass;
			if (itemClass != null && itemClass.Smell != null)
			{
				if (list == null)
				{
					list = new List<ItemId>();
				}
				AppendId(list, ItemId.FromStack(item));
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AppendId(List<ItemId> list, ItemId id)
	{
		for (int i = 0; i < list.Count; i++)
		{
			ItemId value = list[i];
			if (value.id == id.id)
			{
				value.count += id.count;
				list[i] = value;
				return;
			}
		}
		list.Add(id);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool OrderIndependantEquals(List<ItemId> a, List<ItemId> b)
	{
		if (a == null && b == null)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			ItemId item = a[i];
			if (!b.Contains(item))
			{
				return false;
			}
		}
		return true;
	}
}
