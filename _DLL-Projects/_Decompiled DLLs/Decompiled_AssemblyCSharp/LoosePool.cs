using System;
using System.Collections.Generic;

public class LoosePool<T> where T : new()
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] poolElements = new int[22]
	{
		64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768,
		65536, 131072, 262144, 524288, 1048576, 2097152, 4194304, 8388608, 16777216, 33554432,
		67108864, 134217728
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] maxSize = new int[22]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 15,
		20, 20, 10, 10, 10, 10, 5, 5, 2, 2,
		2, 2
	};

	public bool EnforceMaxSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T[]>[] pools;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] poolSize;

	public bool AllowHigherPool = true;

	public LoosePool()
	{
		pools = new List<T[]>[poolElements.Length];
		for (int i = 0; i < pools.Length; i++)
		{
			pools[i] = new List<T[]>();
		}
		poolSize = new int[poolElements.Length];
	}

	public T[] Alloc(int _minSize = 0)
	{
		lock (pools)
		{
			int num = sizeToIdx(_minSize, AllowHigherPool);
			T[] result;
			if (poolSize[num] == 0)
			{
				result = new T[poolElements[num]];
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

	public void Free(T[] _array)
	{
		if (_array == null)
		{
			return;
		}
		lock (pools)
		{
			int num = sizeToIdx(_array.Length, allowHigher: false);
			int num2 = maxSize[num];
			if (_array.Length != poolElements[num])
			{
				Log.Out("removing item as it does not match the cache size");
			}
			else if (!EnforceMaxSize || num2 <= 0 || poolSize[num] < num2)
			{
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
	public int sizeToIdx(int _size, bool allowHigher)
	{
		int num = -1;
		for (int i = 0; i < poolElements.Length; i++)
		{
			if (poolElements[i] >= _size)
			{
				if (num == -1)
				{
					num = i;
				}
				if (!allowHigher || poolSize[i] > 0)
				{
					return i;
				}
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

	public int GetTotalItems()
	{
		int num = 0;
		int[] array = poolSize;
		foreach (int num2 in array)
		{
			num += num2;
		}
		return num;
	}

	public string GetSize(out int totalBytes)
	{
		string text = "";
		totalBytes = 0;
		for (int i = 0; i < pools.Length; i++)
		{
			int num = poolSize[i];
			int num2 = poolElements[i];
			int num3 = 4 * num2 * num;
			totalBytes += num3;
			text += $"{num2} x {num} = {num3}\n";
		}
		text = text + "Bytes: " + totalBytes + "\n";
		text = text + "KBytes: " + totalBytes / 1024 + "\n";
		return text + "MBytes: " + totalBytes / 1024 / 1024;
	}
}
