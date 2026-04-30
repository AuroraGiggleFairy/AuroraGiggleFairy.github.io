using System.Collections.Generic;
using UniLinq;

public static class StackSortUtil
{
	public static ItemStack[] CombineAndSortStacks(ItemStack[] _stacks, int _ignoreSlots = 0, PackedBoolArray _ignoredSlots = null)
	{
		for (int i = _ignoreSlots; i < _stacks.Length - 1; i++)
		{
			if (_stacks[i].IsEmpty() && !IsIgnoredSlot(_ignoreSlots, _ignoredSlots, i))
			{
				for (int j = i + 1; j < _stacks.Length; j++)
				{
					if (!_stacks[j].IsEmpty() && !IsIgnoredSlot(_ignoreSlots, _ignoredSlots, j))
					{
						_stacks[i] = _stacks[j];
						_stacks[j] = ItemStack.Empty.Clone();
						break;
					}
				}
			}
			if (_stacks[i].IsEmpty())
			{
				continue;
			}
			ItemClass itemClass = _stacks[i].itemValue.ItemClass;
			int num = itemClass.Stacknumber.Value - _stacks[i].count;
			if (itemClass.HasQuality || num == 0)
			{
				continue;
			}
			for (int k = i + 1; k < _stacks.Length; k++)
			{
				if (IsIgnoredSlot(_ignoreSlots, _ignoredSlots, k))
				{
					continue;
				}
				if (_stacks[i].itemValue.type == _stacks[k].itemValue.type)
				{
					int num2 = Utils.FastMin(_stacks[k].count, num);
					_stacks[i].count += num2;
					_stacks[k].count -= num2;
					num -= num2;
					if (_stacks[k].count == 0)
					{
						_stacks[k] = ItemStack.Empty.Clone();
					}
				}
				if (num == 0)
				{
					break;
				}
			}
		}
		_stacks = SortStacks(_stacks, _ignoreSlots, _ignoredSlots);
		return _stacks;
	}

	public static ItemStack[] SortStacks(ItemStack[] _stacks, int _ignoreSlots = 0, PackedBoolArray _ignoredSlots = null)
	{
		ItemStack[] array = new ItemStack[_stacks.Length];
		for (int i = 0; i < _stacks.Length; i++)
		{
			if (IsIgnoredSlot(_ignoreSlots, _ignoredSlots, i))
			{
				array[i] = _stacks[i];
				_stacks[i] = ItemStack.Empty.Clone();
			}
		}
		IEnumerator<ItemStack> enumerator = _stacks.OrderBy(getGroup).ThenBy(getName).ThenBy([PublicizedFrom(EAccessModifier.Internal)] (ItemStack _stack) => _stack.itemValue.Quality)
			.ThenByDescending([PublicizedFrom(EAccessModifier.Internal)] (ItemStack _stack) => _stack.itemValue.UseTimes)
			.GetEnumerator();
		bool flag = enumerator.MoveNext();
		int num;
		for (num = 0; num < array.Length && flag; num++)
		{
			if (!IsIgnoredSlot(_ignoreSlots, _ignoredSlots, num))
			{
				array[num] = enumerator.Current;
				flag = enumerator.MoveNext();
			}
		}
		for (; num < array.Length; num++)
		{
			array[num] = ItemStack.Empty.Clone();
		}
		enumerator.Dispose();
		return array;
	}

	public static bool IsIgnoredSlot(int _ignoreSlots, PackedBoolArray _ignoredSlots, int _slot)
	{
		if (_slot >= _ignoreSlots)
		{
			if (_ignoredSlots != null && _slot < _ignoredSlots.Length)
			{
				return _ignoredSlots[_slot];
			}
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getGroup(ItemStack _stack)
	{
		ItemClass forId = ItemClass.GetForId(_stack.itemValue.type);
		if (forId == null)
		{
			return "zzzzz";
		}
		if (forId.IsBlock())
		{
			return "aaaaa";
		}
		return forId.Groups[0];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getName(ItemStack _stack)
	{
		ItemClass forId = ItemClass.GetForId(_stack.itemValue.type);
		if (forId == null)
		{
			return "zzzzz";
		}
		return forId.GetLocalizedItemName();
	}
}
