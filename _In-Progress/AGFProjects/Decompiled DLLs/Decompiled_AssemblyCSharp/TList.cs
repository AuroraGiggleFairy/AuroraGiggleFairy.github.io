using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

public class TList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<T> m_TList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim LockList = new ReaderWriterLockSlim();

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_Disposed;

	public bool Disposed
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Thread.VolatileRead(ref m_Disposed) == 1;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			Thread.VolatileWrite(ref m_Disposed, value ? 1 : 0);
		}
	}

	public int Capacity
	{
		get
		{
			LockList.EnterReadLock();
			try
			{
				return m_TList.Capacity;
			}
			finally
			{
				LockList.ExitReadLock();
			}
		}
		set
		{
			LockList.EnterWriteLock();
			try
			{
				m_TList.Capacity = value;
			}
			finally
			{
				LockList.ExitWriteLock();
			}
		}
	}

	public int Count
	{
		get
		{
			LockList.EnterReadLock();
			try
			{
				return m_TList.Count;
			}
			finally
			{
				LockList.ExitReadLock();
			}
		}
	}

	public bool IsReadOnly => false;

	public TList()
	{
		m_TList = new List<T>();
	}

	public TList(int capacity)
	{
		m_TList = new List<T>(capacity);
	}

	public TList(IEnumerable<T> collection)
	{
		m_TList = new List<T>(collection);
	}

	public IEnumerator GetEnumerator()
	{
		LockList.EnterReadLock();
		List<T> list;
		try
		{
			list = new List<T>(m_TList);
		}
		finally
		{
			LockList.ExitReadLock();
		}
		foreach (T item in list)
		{
			yield return item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		LockList.EnterReadLock();
		List<T> list;
		try
		{
			list = new List<T>(m_TList);
		}
		finally
		{
			LockList.ExitReadLock();
		}
		foreach (T item in list)
		{
			yield return item;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			Disposed = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~TList()
	{
		Dispose(disposing: false);
	}

	public void Add(T item)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.Add(item);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void AddRange(IEnumerable<T> collection)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.AddRange(collection);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public bool AddIfNotExist(T item)
	{
		LockList.EnterWriteLock();
		try
		{
			if (m_TList.Contains(item))
			{
				return false;
			}
			m_TList.Add(item);
			return true;
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public ReadOnlyCollection<T> AsReadOnly()
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.AsReadOnly();
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int BinarySearch(T item)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.BinarySearch(item);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int BinarySearch(T item, IComparer<T> comparer)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.BinarySearch(item, comparer);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.BinarySearch(index, count, item, comparer);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public void Clear()
	{
		LockList.EnterReadLock();
		try
		{
			m_TList.Clear();
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public bool Contains(T item)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.Contains(item);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.ConvertAll(converter);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		LockList.EnterReadLock();
		try
		{
			m_TList.CopyTo(array, arrayIndex);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public bool Exists(Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.Exists(match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public T Find(Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.Find(match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public List<T> FindAll(Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.FindAll(match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int FindIndex(Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.FindIndex(match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int FindIndex(int startIndex, Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.FindIndex(startIndex, match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.FindIndex(startIndex, count, match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public T FindLast(Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.FindLast(match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int FindLastIndex(Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.FindLastIndex(match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int FindLastIndex(int startIndex, Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.FindLastIndex(startIndex, match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int FindLastIndex(int startIndex, int count, Predicate<T> match)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.FindLastIndex(startIndex, count, match);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public void ForEach(Action<T> action)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.ForEach(action);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public List<T> GetRange(int index, int count)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.GetRange(index, count);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int IndexOf(T item)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.IndexOf(item);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int IndexOf(T item, int index)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.IndexOf(item, index);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int IndexOf(T item, int index, int count)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.IndexOf(item, index, count);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public void Insert(int index, T item)
	{
		LockList.ExitWriteLock();
		try
		{
			m_TList.Insert(index, item);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void InsertRange(int index, IEnumerable<T> range)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.InsertRange(index, range);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public int LastIndexOf(T item)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.LastIndexOf(item);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int LastIndexOf(T item, int index)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.LastIndexOf(item, index);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public int LastIndexOf(T item, int index, int count)
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.LastIndexOf(item, index, count);
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public bool Remove(T item)
	{
		LockList.EnterWriteLock();
		try
		{
			return m_TList.Remove(item);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public int RemoveAll(Predicate<T> match)
	{
		LockList.EnterWriteLock();
		try
		{
			return m_TList.RemoveAll(match);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void RemoveAt(int index)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.RemoveAt(index);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void RemoveRange(int index, int count)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.RemoveRange(index, count);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void Reverse()
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.Reverse();
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void Reverse(int index, int count)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.Reverse(index, count);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void Sort()
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.Sort();
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void Sort(Comparison<T> comparison)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.Sort(comparison);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void Sort(IComparer<T> comparer)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.Sort(comparer);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public void Sort(int index, int count, IComparer<T> comparer)
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.Sort(index, count, comparer);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public T[] ToArray()
	{
		LockList.EnterReadLock();
		try
		{
			return m_TList.ToArray();
		}
		finally
		{
			LockList.ExitReadLock();
		}
	}

	public void TrimExcess()
	{
		LockList.EnterWriteLock();
		try
		{
			m_TList.TrimExcess();
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}

	public bool TrueForAll(Predicate<T> match)
	{
		LockList.EnterWriteLock();
		try
		{
			return m_TList.TrueForAll(match);
		}
		finally
		{
			LockList.ExitWriteLock();
		}
	}
}
