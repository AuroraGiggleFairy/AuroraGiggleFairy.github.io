using System;
using System.Collections.Generic;

public class ThreadInfoParamPool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ThreadInfoParam> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ThreadInfoParam> poolBig;

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

	public int CountBig
	{
		get
		{
			lock (_poolLock)
			{
				return poolBig.Count;
			}
		}
	}

	public ThreadInfoParamPool(int initialCapacity, int initialCapacityBig, int initialCount)
	{
		_poolLock = new object();
		pool = new List<ThreadInfoParam>(initialCapacity);
		poolBig = new List<ThreadInfoParam>(initialCapacityBig);
		for (int i = 0; i < initialCount; i++)
		{
			pool.Add(new ThreadInfoParam());
			poolBig.Add(new ThreadInfoParam());
		}
	}

	public ThreadInfoParam GetObject(DistantChunkMap _CMap, int _ResLevel, int _OutId)
	{
		lock (_poolLock)
		{
			if (pool.Count == 0)
			{
				return new ThreadInfoParam(_CMap, _ResLevel, _OutId, _IsBigCapacity: false);
			}
			ThreadInfoParam threadInfoParam = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			threadInfoParam.Init(_CMap, _ResLevel, _OutId, _IsBigCapacity: false);
			return threadInfoParam;
		}
	}

	public ThreadInfoParam GetObjectBig(DistantChunkMap _CMap, int _ResLevel, int _OutId)
	{
		lock (_poolLock)
		{
			if (poolBig.Count == 0)
			{
				return new ThreadInfoParam(_CMap, _ResLevel, _OutId, _IsBigCapacity: true);
			}
			ThreadInfoParam threadInfoParam = poolBig[poolBig.Count - 1];
			poolBig.RemoveAt(poolBig.Count - 1);
			threadInfoParam.Init(_CMap, _ResLevel, _OutId, _IsBigCapacity: true);
			return threadInfoParam;
		}
	}

	public void ReturnObject(ThreadInfoParam item, ThreadContainerPool TmpThContPool = null)
	{
		if (item == null)
		{
			throw new ArgumentNullException("ThreadInfoParam is null");
		}
		item.ClearAll(TmpThContPool);
		lock (_poolLock)
		{
			if (item.IsBigCapacity)
			{
				if (poolBig.Contains(item))
				{
					throw new InvalidOperationException("ThreadInfoParam Big already in pool");
				}
				poolBig.Add(item);
			}
			else
			{
				if (pool.Contains(item))
				{
					throw new InvalidOperationException("ThreadInfoParam already in pool");
				}
				pool.Add(item);
			}
		}
	}
}
