using System;
using System.Collections.Generic;
using System.IO;

public class ItemStack
{
	public enum EnumDragType
	{
		DragTypeStart,
		DragTypeAdd,
		DragTypeExchange,
		DragTypeOther
	}

	public ItemValue itemValue;

	public int count;

	public static ItemStack Empty => new ItemStack(ItemValue.None, 0);

	public ItemStack()
	{
		itemValue = ItemValue.None;
		count = 0;
	}

	public ItemStack(ItemValue _itemValue, int _count)
	{
		itemValue = _itemValue;
		count = _count;
	}

	public ItemStack Clone()
	{
		if (itemValue != null)
		{
			return new ItemStack(itemValue.Clone(), count);
		}
		return new ItemStack(ItemValue.None, count);
	}

	public static ItemStack[] CreateArray(int _size)
	{
		ItemStack[] array = new ItemStack[_size];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Empty;
		}
		return array;
	}

	public bool IsEmpty()
	{
		if (count >= 1)
		{
			return itemValue.type == 0;
		}
		return true;
	}

	public void Clear()
	{
		itemValue.Clear();
		count = 0;
	}

	public static int AddToItemStackArray(ItemStack[] _itemStackArr, ItemStack _itemStack, int maxItemCount = -1)
	{
		int num = -1;
		int num2 = 0;
		while (num == -1 && num2 < _itemStackArr.Length)
		{
			if (_itemStackArr[num2].CanStackWith(_itemStack))
			{
				_itemStackArr[num2].count += _itemStack.count;
				_itemStack.count = 0;
				num = num2;
			}
			num2++;
		}
		int num3 = 0;
		while (num == -1 && num3 < _itemStackArr.Length && (maxItemCount == -1 || num3 != maxItemCount))
		{
			if (_itemStackArr[num3].IsEmpty())
			{
				_itemStackArr[num3] = _itemStack;
				num = num3;
			}
			num3++;
		}
		return num;
	}

	public ItemStack ReadOld(BinaryReader _br)
	{
		itemValue.ReadOld(_br);
		count = _br.ReadInt16();
		return this;
	}

	public ItemStack Read(BinaryReader _br)
	{
		count = _br.ReadUInt16();
		if (count > 0)
		{
			itemValue.Read(_br);
		}
		else
		{
			itemValue = ItemValue.None;
		}
		return this;
	}

	public ItemStack ReadDelta(BinaryReader _br, ItemStack _last)
	{
		itemValue.Read(_br);
		int num = _br.ReadInt16();
		count = _last.count + num;
		return this;
	}

	public void Write(BinaryWriter _bw)
	{
		int num = count;
		if (num > 65535)
		{
			num = 65535;
		}
		_bw.Write((ushort)num);
		if (count != 0)
		{
			itemValue.Write(_bw);
		}
	}

	public void WriteDelta(BinaryWriter _bw, ItemStack _last)
	{
		itemValue.Write(_bw);
		_bw.Write((short)(count - _last.count));
		_last.count += count - _last.count;
	}

	public static ItemStack[] ReadArray(BinaryReader _br)
	{
		int num = _br.ReadInt16();
		if (num == -1)
		{
			return null;
		}
		ItemStack[] array = CreateArray(num);
		for (int i = 0; i < num; i++)
		{
			array[i].Read(_br);
		}
		return array;
	}

	public static void WriteArray(BinaryWriter _bw, ItemStack[] _items)
	{
		if (_items == null)
		{
			_bw.Write((short)(-1));
			return;
		}
		_bw.Write((short)_items.Length);
		for (int i = 0; i < _items.Length; i++)
		{
			_items[i].Write(_bw);
		}
	}

	public override bool Equals(object _other)
	{
		if (!(_other is ItemStack itemStack))
		{
			return false;
		}
		if (itemStack.count == count)
		{
			return itemStack.itemValue.Equals(itemValue);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return itemValue.GetHashCode() * 13 + count;
	}

	public int StackTransferCount(ItemStack _other)
	{
		if (_other.itemValue.type != itemValue.type)
		{
			return 0;
		}
		return Math.Min(ItemClass.GetForId(itemValue.type).Stacknumber.Value - count, _other.count);
	}

	public bool CanStackWith(ItemStack _other, bool allowPartialStack = false)
	{
		int _count = _other.count;
		if (_other.itemValue != null && itemValue != null && _other.itemValue.type == itemValue.type && (itemValue.type >= Block.ItemsStartHere || _other.itemValue.TextureFullArray == itemValue.TextureFullArray || itemValue.IsShapeHelperBlock))
		{
			if (!allowPartialStack)
			{
				return CanStack(_other.count);
			}
			return CanStackPartly(ref _count);
		}
		return false;
	}

	public bool CanStack(int _count)
	{
		if (itemValue.type == 0)
		{
			return true;
		}
		return ItemClass.GetForId(itemValue.type).Stacknumber.Value >= _count + count;
	}

	public bool CanStackPartlyWith(ItemStack _other, out int _count)
	{
		_count = _other.count;
		if (CanStackWith(_other, allowPartialStack: true))
		{
			return CanStackPartly(ref _count);
		}
		return false;
	}

	public bool CanStackPartly(ref int _count)
	{
		if (itemValue.type == 0)
		{
			return false;
		}
		_count = Utils.FastMin(ItemClass.GetForId(itemValue.type).Stacknumber.Value - count, _count);
		return _count > 0;
	}

	public void Deactivate()
	{
		ItemClass.GetForId(itemValue.type)?.Deactivate(itemValue);
	}

	public static ItemStack[] Clone(IList<ItemStack> _itemStackArr)
	{
		if (_itemStackArr == null)
		{
			return null;
		}
		ItemStack[] array = new ItemStack[_itemStackArr.Count];
		for (int i = 0; i < array.Length; i++)
		{
			ItemStack itemStack = _itemStackArr[i];
			array[i] = ((itemStack != null) ? itemStack.Clone() : Empty);
		}
		return array;
	}

	public static ItemStack[] Clone(ItemStack[] _itemStackArr, int _startIdx, int _length)
	{
		if (_itemStackArr == null)
		{
			return null;
		}
		ItemStack[] array = new ItemStack[_length];
		for (int i = 0; i < _length; i++)
		{
			ItemStack itemStack = ((_startIdx + i < _itemStackArr.Length) ? _itemStackArr[_startIdx + i] : null);
			array[i] = ((itemStack != null) ? itemStack.Clone() : Empty);
		}
		return array;
	}

	public static ItemStack FromString(string _s)
	{
		ItemStack empty = Empty;
		int num = _s.IndexOf("=");
		if (num >= 0)
		{
			int result = 0;
			if (int.TryParse(_s.Substring(num + 1), out result))
			{
				empty.count = result;
			}
			_s = _s.Substring(0, num);
		}
		else
		{
			empty.count = 1;
		}
		empty.itemValue = ItemClass.GetItem(_s);
		return empty;
	}

	public override string ToString()
	{
		return itemValue?.ToString() + " cnt=" + count;
	}

	public static bool IsEmpty(ItemStack[] _slots)
	{
		if (_slots == null || _slots.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < _slots.Length; i++)
		{
			if (!_slots[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public bool CanMoveTo(XUiC_ItemStack.StackLocationTypes locationType)
	{
		bool result = true;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			result = itemClass.CanMoveToLocation(locationType);
		}
		return result;
	}
}
