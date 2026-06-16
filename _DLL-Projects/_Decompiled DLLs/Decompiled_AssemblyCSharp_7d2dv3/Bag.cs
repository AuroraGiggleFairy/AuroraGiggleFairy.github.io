using System;
using System.Collections.Generic;
using System.IO;

public class Bag : IInventory
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const byte Version = 1;

	public bool Touched;

	public PackedBoolArray LockedSlots;

	public PreferenceTracker preferences;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	public int SlotCount
	{
		get
		{
			if (items != null)
			{
				return items.Length;
			}
			return 0;
		}
	}

	public event XUiEvent_BackpackItemsChangedInternal OnBackpackItemsChangedInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bag()
	{
	}

	public Bag(int _size)
	{
		items = ItemStack.CreateArray(_size);
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write((byte)1);
		ItemStack[] slots = GetSlots();
		bw.Write((ushort)slots.Length);
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i].Write(bw);
		}
		bool flag = LockedSlots != null;
		bw.Write(flag);
		if (flag)
		{
			LockedSlots.Write(bw);
		}
		bw.Write(Touched);
		bool flag2 = preferences != null;
		bw.Write(flag2);
		if (flag2)
		{
			if (!(bw is PooledBinaryWriter bw2))
			{
				throw new InvalidOperationException("[Bag] Writing preferences requires PooledBinaryWriter.");
			}
			preferences.Write(bw2);
		}
	}

	public static Bag Read(BinaryReader br)
	{
		return new Bag().ReadInto(br);
	}

	public Bag ReadInto(BinaryReader br)
	{
		byte b = br.ReadByte();
		int num = br.ReadUInt16();
		if (items == null || SlotCount != num)
		{
			items = ItemStack.CreateArray(num);
		}
		for (int i = 0; i < num; i++)
		{
			items[i].Read(br);
		}
		if (br.ReadBoolean())
		{
			LockedSlots = LockedSlots ?? new PackedBoolArray();
			LockedSlots.Read(br);
		}
		else
		{
			LockedSlots = null;
		}
		if (b >= 1)
		{
			Touched = br.ReadBoolean();
			if (br.ReadBoolean())
			{
				if (preferences == null)
				{
					preferences = new PreferenceTracker(-1);
				}
				if (!(br is PooledBinaryReader br2))
				{
					throw new InvalidOperationException("[Bag] Reading preferences requires PooledBinaryReader.");
				}
				preferences.Read(br2);
			}
			else
			{
				preferences = null;
			}
		}
		else
		{
			preferences = null;
		}
		return this;
	}

	public Bag Clone()
	{
		return new Bag
		{
			items = ItemStack.Clone(items),
			LockedSlots = LockedSlots?.Clone(),
			Touched = Touched,
			preferences = preferences?.Clone()
		};
	}

	public void Clear()
	{
		ItemStack[] array = items;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
		}
		onBackpackChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onBackpackChanged()
	{
		this.OnBackpackItemsChangedInternal?.Invoke();
	}

	public bool CanStackNoEmpty(ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].CanStackPartlyWith(_itemStack, out var _))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEmpty()
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public bool CanStack(ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsEmpty() || slots[i].CanStackWith(_itemStack))
			{
				return true;
			}
		}
		return false;
	}

	public (bool anyMoved, bool allMoved) TryStackItem(int startIndex, ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Backpack))
		{
			return (anyMoved: false, allMoved: false);
		}
		ItemStack[] slots = GetSlots();
		int num = 0;
		bool item = false;
		for (int i = startIndex; i < slots.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == slots[i].itemValue.type && slots[i].CanStackPartly(ref num))
			{
				slots[i].count += num;
				_itemStack.count -= num;
				onBackpackChanged();
				item = true;
				if (_itemStack.count == 0)
				{
					return (anyMoved: true, allMoved: true);
				}
			}
		}
		return (anyMoved: item, allMoved: false);
	}

	public bool CanTakeItem(ItemStack _itemStack)
	{
		ItemStack[] slots = GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].CanStackPartlyWith(_itemStack, out var _))
			{
				return true;
			}
			if (slots[i].IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public ItemStack[] GetSlots()
	{
		return items;
	}

	public void SetSlots(ItemStack[] _slots)
	{
		items = _slots;
		onBackpackChanged();
	}

	public void SetSlot(int index, ItemStack _stack, bool callChangedEvent = true)
	{
		if (index < items.Length)
		{
			items[index] = _stack;
			if (callChangedEvent)
			{
				onBackpackChanged();
			}
		}
	}

	public bool AddItem(ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Backpack))
		{
			return false;
		}
		bool num = ItemStack.AddToItemStackArray(items, _itemStack) >= 0;
		if (num)
		{
			onBackpackChanged();
		}
		return num;
	}

	public int DecItem(ItemValue _itemValue, int _count, bool _ignoreModdedItems = false, IList<ItemStack> _removedItems = null)
	{
		int num = _count;
		ItemStack[] slots = GetSlots();
		int num2 = 0;
		while (_count > 0 && num2 < GetSlots().Length)
		{
			if (slots[num2].itemValue.type == _itemValue.type && (!_ignoreModdedItems || !slots[num2].itemValue.HasModSlots || !slots[num2].itemValue.HasMods()))
			{
				if (ItemClass.GetForId(slots[num2].itemValue.type).CanStack())
				{
					int count = slots[num2].count;
					int num3 = ((count >= _count) ? _count : count);
					_removedItems?.Add(new ItemStack(slots[num2].itemValue.Clone(), num3));
					slots[num2].count -= num3;
					_count -= num3;
					if (slots[num2].count <= 0)
					{
						slots[num2].Clear();
					}
				}
				else
				{
					_removedItems?.Add(slots[num2].Clone());
					slots[num2].Clear();
					_count--;
				}
			}
			num2++;
		}
		SetSlots(slots);
		return num - _count;
	}

	public int GetUsedSlotCount()
	{
		ItemStack[] slots = GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].IsEmpty())
			{
				num++;
			}
		}
		return num;
	}

	public int GetItemCount(ItemValue _itemValue, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		ItemStack[] slots = GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && slots[i].itemValue.type == _itemValue.type && (_seed == -1 || _seed == slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].count;
			}
		}
		return num;
	}

	public int GetItemCount(FastTags<TagGroup.Global> itemTags, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		ItemStack[] slots = GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && !slots[i].itemValue.IsEmpty() && slots[i].itemValue.ItemClass.ItemTags.Test_AnySet(itemTags) && (_seed == -1 || _seed == slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].count;
			}
		}
		return num;
	}

	public bool HasItem(ItemValue _item)
	{
		return GetItemCount(_item) > 0;
	}
}
