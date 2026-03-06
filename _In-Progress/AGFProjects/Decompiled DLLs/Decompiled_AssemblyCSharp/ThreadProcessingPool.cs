using System;
using System.Collections.Generic;

public class ThreadProcessingPool
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ThreadProcessing> pool;

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

	public ThreadProcessingPool(int initialCapacity, int initialCount)
	{
		_poolLock = new object();
		pool = new List<ThreadProcessing>(initialCapacity);
		for (int i = 0; i < initialCount; i++)
		{
			pool.Add(new ThreadProcessing());
		}
	}

	public ThreadProcessing GetObject(List<ThreadInfoParam> _JobList)
	{
		lock (_poolLock)
		{
			if (pool.Count == 0)
			{
				return new ThreadProcessing(_JobList);
			}
			ThreadProcessing threadProcessing = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
			threadProcessing.Init(_JobList);
			return threadProcessing;
		}
	}

	public void ReturnObject(ThreadProcessing item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("ThreadProcessing is null");
		}
		lock (_poolLock)
		{
			if (!pool.Contains(item))
			{
				pool.Add(item);
				return;
			}
			throw new InvalidOperationException("ThreadProcessing already in pool");
		}
	}
}
