using System;
using System.Collections.Generic;

public class MemoryPooledArray<T> where T : new()
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<T[]>[] pools;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] poolSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxCapacity;

	public MemoryPooledArray()
	{
		pools = new List<T[]>[MemoryPooledArraySizes.poolElements.Length];
		for (int i = 0; i < pools.Length; i++)
		{
			pools[i] = new List<T[]>();
		}
		poolSize = new int[MemoryPooledArraySizes.poolElements.Length];
	}

	public T[] Alloc(int _minSize = 0)
	{
		lock (pools)
		{
			int num = sizeToIdx(_minSize);
			T[] result;
			if (poolSize[num] == 0)
			{
				result = new T[MemoryPooledArraySizes.poolElements[num]];
			}
			else
			{
				int index = --poolSize[num];
				result = pools[num][index];
				pools[num][index] = null;
			}
			return result;
		}
	}

	public T[] Grow(T[] _array)
	{
		return Grow(_array, _array.Length + 1);
	}

	public T[] Grow(T[] _array, int _minSize)
	{
		T[] array = Alloc(_minSize);
		Array.Copy(_array, array, _array.Length);
		Free(_array);
		return array;
	}

	public void Free(T[] _array)
	{
		if (_array == null)
		{
			return;
		}
		lock (pools)
		{
			int num = sizeToIdx(_array.Length);
			if (poolSize[num] >= pools[num].Count)
			{
				pools[num].Add(_array);
				poolSize[num]++;
			}
			else
			{
				pools[num][poolSize[num]++] = _array;
			}
		}
	}

	public void FreeAll()
	{
		lock (pools)
		{
			for (int i = 0; i < pools.Length; i++)
			{
				pools[i].Clear();
				poolSize[i] = 0;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeToIdx(int _size)
	{
		int num = -1;
		for (int i = 0; i < MemoryPooledArraySizes.poolElements.Length; i++)
		{
			if (MemoryPooledArraySizes.poolElements[i] >= _size)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			throw new Exception("Array length in pool not supported " + _size);
		}
		return num;
	}

	public int GetCount()
	{
		int num = 0;
		for (int i = 0; i < pools.Length; i++)
		{
			if (pools[i] != null)
			{
				num += pools[i].Count;
			}
		}
		return num;
	}

	public int GetCount(int _poolIndex)
	{
		if (_poolIndex >= 0 && _poolIndex < pools.Length)
		{
			return pools[_poolIndex]?.Count ?? 0;
		}
		return 0;
	}

	public long GetElementsCount()
	{
		int num = 0;
		for (int i = 0; i < pools.Length; i++)
		{
			if (pools[i] != null)
			{
				num += pools[i].Count * MemoryPooledArraySizes.poolElements[i];
			}
		}
		return num;
	}
}
