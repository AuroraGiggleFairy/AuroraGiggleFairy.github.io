using System.Collections.Generic;

public class MemoryPooledObject<T> where T : IMemoryPoolableObject, new()
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public int poolSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxCapacity;

	public MemoryPooledObject(int _maxCapacity)
	{
		pool = new List<T>(_maxCapacity);
		maxCapacity = _maxCapacity;
	}

	public void SetCapacity(int _maxCapacity)
	{
		pool.Capacity = _maxCapacity;
		maxCapacity = _maxCapacity;
	}

	public T AllocSync(bool _bReset)
	{
		lock (pool)
		{
			return Alloc(_bReset);
		}
	}

	public T Alloc(bool _bReset)
	{
		T result;
		if (poolSize == 0)
		{
			result = new T();
		}
		else
		{
			poolSize--;
			result = pool[poolSize];
			pool[poolSize] = default(T);
		}
		if (_bReset)
		{
			result.Reset();
		}
		return result;
	}

	public void FreeSync(IList<T> _array)
	{
		lock (pool)
		{
			for (int i = 0; i < _array.Count; i++)
			{
				T val = _array[i];
				if (val != null)
				{
					Free(val);
					_array[i] = default(T);
				}
			}
		}
	}

	public void FreeSync(Queue<T> _queue)
	{
		lock (pool)
		{
			while (_queue.Count > 0)
			{
				Free(_queue.Dequeue());
			}
		}
	}

	public void FreeSync(T _t)
	{
		lock (pool)
		{
			Free(_t);
		}
	}

	public void Free(T[] _array)
	{
		for (int i = 0; i < _array.Length; i++)
		{
			T val = _array[i];
			if (val != null)
			{
				Free(val);
				_array[i] = default(T);
			}
		}
	}

	public void Free(List<T> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			T val = _list[i];
			if (val != null)
			{
				Free(val);
			}
		}
		_list.Clear();
	}

	public void Cleanup()
	{
		lock (pool)
		{
			for (int i = 0; i < poolSize; i++)
			{
				pool[i]?.Cleanup();
			}
			pool.Clear();
			poolSize = 0;
		}
	}

	public void Free(T _t)
	{
		if (poolSize >= pool.Count && poolSize < maxCapacity)
		{
			_t.Reset();
			pool.Add(_t);
			poolSize++;
		}
		else if (poolSize < maxCapacity)
		{
			_t.Reset();
			pool[poolSize++] = _t;
		}
		else
		{
			_t.Cleanup();
		}
	}

	public int GetPoolSize()
	{
		return poolSize;
	}
}
