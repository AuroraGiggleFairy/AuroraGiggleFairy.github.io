using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Linq;

[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 0, 1 })]
[_003C101b90a7_002Ded16_002D473d_002D9ce2_002D3a947e6817ed_003ENullableContext(1)]
internal abstract class AsyncEnumerableSorterBase<[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TElement, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(2)] TKey> : AsyncEnumerableSorter<TElement>
{
	private readonly IComparer<TKey> _comparer;

	private readonly bool _descending;

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })]
	protected readonly AsyncEnumerableSorter<TElement> _next;

	[_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })]
	protected TKey[] _keys;

	public AsyncEnumerableSorterBase(IComparer<TKey> comparer, bool descending, [_003C24de4114_002D6bff_002D4b93_002D86b9_002D76223a10670c_003ENullable(new byte[] { 2, 1 })] AsyncEnumerableSorter<TElement> next)
	{
		_comparer = comparer;
		_descending = descending;
		_next = next;
	}

	internal sealed override int CompareAnyKeys(int index1, int index2)
	{
		int num = _comparer.Compare(_keys[index1], _keys[index2]);
		if (num == 0)
		{
			if (_next != null)
			{
				return _next.CompareAnyKeys(index1, index2);
			}
			return index1 - index2;
		}
		if (_descending == num > 0)
		{
			return -1;
		}
		return 1;
	}

	private int CompareKeys(int index1, int index2)
	{
		if (index1 != index2)
		{
			return CompareAnyKeys(index1, index2);
		}
		return 0;
	}

	protected override void QuickSort(int[] keys, int lo, int hi)
	{
		Array.Sort(keys, lo, hi - lo + 1, Comparer<int>.Create(CompareAnyKeys));
	}

	protected override void PartialQuickSort(int[] map, int left, int right, int minIndexInclusive, int maxIndexInclusive)
	{
		do
		{
			int num = left;
			int num2 = right;
			int index = map[num + (num2 - num >> 1)];
			while (true)
			{
				if (num < map.Length && CompareKeys(index, map[num]) > 0)
				{
					num++;
					continue;
				}
				while (num2 >= 0 && CompareKeys(index, map[num2]) < 0)
				{
					num2--;
				}
				if (num > num2)
				{
					break;
				}
				if (num < num2)
				{
					int num3 = map[num];
					map[num] = map[num2];
					map[num2] = num3;
				}
				num++;
				num2--;
				if (num > num2)
				{
					break;
				}
			}
			if (minIndexInclusive >= num)
			{
				left = num + 1;
			}
			else if (maxIndexInclusive <= num2)
			{
				right = num2 - 1;
			}
			if (num2 - left <= right - num)
			{
				if (left < num2)
				{
					PartialQuickSort(map, left, num2, minIndexInclusive, maxIndexInclusive);
				}
				left = num;
			}
			else
			{
				if (num < right)
				{
					PartialQuickSort(map, num, right, minIndexInclusive, maxIndexInclusive);
				}
				right = num2;
			}
		}
		while (left < right);
	}

	protected override int QuickSelect(int[] map, int right, int idx)
	{
		int num = 0;
		do
		{
			int num2 = num;
			int num3 = right;
			int index = map[num2 + (num3 - num2 >> 1)];
			while (true)
			{
				if (num2 < map.Length && CompareKeys(index, map[num2]) > 0)
				{
					num2++;
					continue;
				}
				while (num3 >= 0 && CompareKeys(index, map[num3]) < 0)
				{
					num3--;
				}
				if (num2 > num3)
				{
					break;
				}
				if (num2 < num3)
				{
					int num4 = map[num2];
					map[num2] = map[num3];
					map[num3] = num4;
				}
				num2++;
				num3--;
				if (num2 > num3)
				{
					break;
				}
			}
			if (num2 <= idx)
			{
				num = num2 + 1;
			}
			else
			{
				right = num3 - 1;
			}
			if (num3 - num <= right - num2)
			{
				if (num < num3)
				{
					right = num3;
				}
				num = num2;
			}
			else
			{
				if (num2 < right)
				{
					num = num2;
				}
				right = num3;
			}
		}
		while (num < right);
		return map[idx];
	}

	protected override int Min(int[] map, int count)
	{
		int num = 0;
		for (int i = 1; i < count; i++)
		{
			if (CompareKeys(map[i], map[num]) < 0)
			{
				num = i;
			}
		}
		return map[num];
	}
}
