using System;
using System.Collections.Generic;

public class ThreadContainerPool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ThreadContainer> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object _poolLock;

	public int Count
	{
		get
		{
			lock (_poolLock)
			{
				return pool.Count;
			}
		}
	}

	public ThreadContainerPool(int initialCapacity, int initialCount)
	{
		_poolLock = new object();
		pool = new List<ThreadContainer>(initialCapacity);
		for (int i = 0; i < initialCount; i++)
		{
			pool.Add(new ThreadContainer());
		}
	}

	public ThreadContainer GetObject(DistantTerrain _TerExt, DistantChunk _DChunk, DistantChunkBasicMesh _BMesh, bool _WasReset)
	{
		lock (_poolLock)
		{
			if (pool.Count == 0)
			{
				return new ThreadContainer(_TerExt, _DChunk, _BMesh, _WasReset);
			}
			ThreadContainer threadContainer = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			threadContainer.Init(_TerExt, _DChunk, _BMesh, _WasReset);
			return threadContainer;
		}
	}

	public void ReturnObject(ThreadContainer item, bool IsClearItem)
	{
		if (item == null)
		{
			throw new ArgumentNullException("ThreadContainer is null");
		}
		lock (_poolLock)
		{
			if (!pool.Contains(item))
			{
				item.Clear(IsClearItem);
				pool.Add(item);
				return;
			}
			throw new InvalidOperationException("ThreadContainer already in pool");
		}
	}
}
