using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class TQueue<T> : ICollection, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<T> m_Queue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim LockQ = new ReaderWriterLockSlim();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object objSyncRoot = new object();

	public int Count
	{
		get
		{
			LockQ.EnterReadLock();
			try
			{
				return m_Queue.Count;
			}
			finally
			{
				LockQ.ExitReadLock();
			}
		}
	}

	public bool IsSynchronized => true;

	public object SyncRoot => objSyncRoot;

	public TQueue()
	{
		m_Queue = new Queue<T>();
	}

	public TQueue(int capacity)
	{
		m_Queue = new Queue<T>(capacity);
	}

	public TQueue(IEnumerable<T> collection)
	{
		m_Queue = new Queue<T>(collection);
	}

	public IEnumerator<T> GetEnumerator()
	{
		LockQ.EnterReadLock();
		Queue<T> queue;
		try
		{
			queue = new Queue<T>(m_Queue);
		}
		finally
		{
			LockQ.ExitReadLock();
		}
		foreach (T item in queue)
		{
			yield return item;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		LockQ.EnterReadLock();
		Queue<T> queue;
		try
		{
			queue = new Queue<T>(m_Queue);
		}
		finally
		{
			LockQ.ExitReadLock();
		}
		foreach (T item in queue)
		{
			yield return item;
		}
	}

	public void CopyTo(Array array, int index)
	{
		LockQ.EnterReadLock();
		try
		{
			m_Queue.ToArray().CopyTo(array, index);
		}
		finally
		{
			LockQ.ExitReadLock();
		}
	}

	public void CopyTo(T[] array, int index)
	{
		LockQ.EnterReadLock();
		try
		{
			m_Queue.CopyTo(array, index);
		}
		finally
		{
			LockQ.ExitReadLock();
		}
	}

	public void Enqueue(T item)
	{
		LockQ.EnterWriteLock();
		try
		{
			m_Queue.Enqueue(item);
		}
		finally
		{
			LockQ.ExitWriteLock();
		}
	}

	public T Dequeue()
	{
		LockQ.EnterWriteLock();
		try
		{
			return m_Queue.Dequeue();
		}
		finally
		{
			LockQ.ExitWriteLock();
		}
	}

	public void EnqueueAll(IEnumerable<T> ItemsToQueue)
	{
		LockQ.EnterWriteLock();
		try
		{
			foreach (T item in ItemsToQueue)
			{
				m_Queue.Enqueue(item);
			}
		}
		finally
		{
			LockQ.ExitWriteLock();
		}
	}

	public void EnqueueAll(TList<T> ItemsToQueue)
	{
		LockQ.EnterWriteLock();
		try
		{
			foreach (T item in ItemsToQueue)
			{
				m_Queue.Enqueue(item);
			}
		}
		finally
		{
			LockQ.ExitWriteLock();
		}
	}

	public TList<T> DequeueAll()
	{
		LockQ.EnterWriteLock();
		try
		{
			TList<T> tList = new TList<T>();
			while (m_Queue.Count > 0)
			{
				tList.Add(m_Queue.Dequeue());
			}
			return tList;
		}
		finally
		{
			LockQ.ExitWriteLock();
		}
	}

	public void Clear()
	{
		LockQ.EnterWriteLock();
		try
		{
			m_Queue.Clear();
		}
		finally
		{
			LockQ.ExitWriteLock();
		}
	}

	public bool Contains(T item)
	{
		LockQ.EnterReadLock();
		try
		{
			return m_Queue.Contains(item);
		}
		finally
		{
			LockQ.ExitReadLock();
		}
	}

	public T Peek()
	{
		LockQ.EnterReadLock();
		try
		{
			return m_Queue.Peek();
		}
		finally
		{
			LockQ.ExitReadLock();
		}
	}

	public T[] ToArray()
	{
		LockQ.EnterReadLock();
		try
		{
			return m_Queue.ToArray();
		}
		finally
		{
			LockQ.ExitReadLock();
		}
	}

	public void TrimExcess()
	{
		LockQ.EnterWriteLock();
		try
		{
			m_Queue.TrimExcess();
		}
		finally
		{
			LockQ.ExitWriteLock();
		}
	}
}
