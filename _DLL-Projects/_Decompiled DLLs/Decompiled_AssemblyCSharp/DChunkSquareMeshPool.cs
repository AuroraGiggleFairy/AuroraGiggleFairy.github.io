using System;
using System.Collections.Generic;

public class DChunkSquareMeshPool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<DChunkSquareMesh>[] pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object _poolLock;

	public int Count
	{
		get
		{
			lock (_poolLock)
			{
				int num = 0;
				for (int i = 0; i < pool.Length; i++)
				{
					num += pool[i].Count;
				}
				return num;
			}
		}
	}

	public DChunkSquareMeshPool(int initialCapacity, int NbLODLevel)
	{
		_poolLock = new object();
		pool = new List<DChunkSquareMesh>[NbLODLevel];
		for (int i = 0; i < NbLODLevel; i++)
		{
			pool[i] = new List<DChunkSquareMesh>(initialCapacity);
		}
	}

	public DChunkSquareMesh GetObject(DistantChunkMap DCMap, int LODLevel)
	{
		lock (_poolLock)
		{
			List<DChunkSquareMesh> list = pool[LODLevel];
			if (list.Count == 0)
			{
				return new DChunkSquareMesh(DCMap, LODLevel);
			}
			DChunkSquareMesh result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result;
		}
	}

	public void ReturnObject(DChunkSquareMesh item, int LODLevel)
	{
		if (item == null)
		{
			throw new ArgumentNullException("DChunkSquareMesh is null");
		}
		lock (_poolLock)
		{
			if (!pool[LODLevel].Contains(item))
			{
				pool[LODLevel].Add(item);
				return;
			}
			throw new InvalidOperationException("ThreadProcessing already in pool");
		}
	}
}
