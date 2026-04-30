using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace Discord;

internal static class ConcurrentHashSet
{
	private const int PROCESSOR_COUNT_REFRESH_INTERVAL_MS = 30000;

	private static volatile int s_processorCount;

	private static volatile int s_lastProcessorCountRefreshTicks;

	public static int DefaultConcurrencyLevel
	{
		get
		{
			int tickCount = Environment.TickCount;
			if (s_processorCount == 0 || tickCount - s_lastProcessorCountRefreshTicks >= 30000)
			{
				s_processorCount = Environment.ProcessorCount;
				s_lastProcessorCountRefreshTicks = tickCount;
			}
			return s_processorCount;
		}
	}
}
[DebuggerDisplay("Count = {Count}")]
internal class ConcurrentHashSet<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
{
	private sealed class Tables
	{
		internal readonly Node[] _buckets;

		internal readonly object[] _locks;

		internal volatile int[] _countPerLock;

		internal Tables(Node[] buckets, object[] locks, int[] countPerLock)
		{
			_buckets = buckets;
			_locks = locks;
			_countPerLock = countPerLock;
		}
	}

	private sealed class Node
	{
		internal readonly T _value;

		internal volatile Node _next;

		internal readonly int _hashcode;

		internal Node(T key, int hashcode, Node next)
		{
			_value = key;
			_next = next;
			_hashcode = hashcode;
		}
	}

	private const int DefaultCapacity = 31;

	private const int MaxLockNumber = 1024;

	private volatile Tables _tables;

	private readonly IEqualityComparer<T> _comparer;

	private readonly bool _growLockArray;

	private int _budget;

	private static int DefaultConcurrencyLevel => ConcurrentHashSet.DefaultConcurrencyLevel;

	public int Count
	{
		get
		{
			int num = 0;
			int locksAcquired = 0;
			try
			{
				AcquireAllLocks(ref locksAcquired);
				for (int i = 0; i < _tables._countPerLock.Length; i++)
				{
					num += _tables._countPerLock[i];
				}
				return num;
			}
			finally
			{
				ReleaseLocks(0, locksAcquired);
			}
		}
	}

	public bool IsEmpty
	{
		get
		{
			int locksAcquired = 0;
			try
			{
				AcquireAllLocks(ref locksAcquired);
				for (int i = 0; i < _tables._countPerLock.Length; i++)
				{
					if (_tables._countPerLock[i] != 0)
					{
						return false;
					}
				}
			}
			finally
			{
				ReleaseLocks(0, locksAcquired);
			}
			return true;
		}
	}

	public ReadOnlyCollection<T> Values
	{
		get
		{
			int locksAcquired = 0;
			try
			{
				AcquireAllLocks(ref locksAcquired);
				List<T> list = new List<T>();
				for (int i = 0; i < _tables._buckets.Length; i++)
				{
					for (Node node = _tables._buckets[i]; node != null; node = node._next)
					{
						list.Add(node._value);
					}
				}
				return new ReadOnlyCollection<T>(list);
			}
			finally
			{
				ReleaseLocks(0, locksAcquired);
			}
		}
	}

	private static int GetBucket(int hashcode, int bucketCount)
	{
		return (hashcode & 0x7FFFFFFF) % bucketCount;
	}

	private static void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
	{
		bucketNo = (hashcode & 0x7FFFFFFF) % bucketCount;
		lockNo = bucketNo % lockCount;
	}

	public ConcurrentHashSet()
		: this(DefaultConcurrencyLevel, 31, true, (IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public ConcurrentHashSet(int concurrencyLevel, int capacity)
		: this(concurrencyLevel, capacity, false, (IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public ConcurrentHashSet(IEnumerable<T> collection)
		: this(collection, (IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public ConcurrentHashSet(IEqualityComparer<T> comparer)
		: this(DefaultConcurrencyLevel, 31, true, comparer)
	{
	}

	public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
		: this(comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		InitializeFromCollection(collection);
	}

	public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
		: this(concurrencyLevel, 31, false, comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		InitializeFromCollection(collection);
	}

	public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer)
		: this(concurrencyLevel, capacity, false, comparer)
	{
	}

	internal ConcurrentHashSet(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<T> comparer)
	{
		if (concurrencyLevel < 1)
		{
			throw new ArgumentOutOfRangeException("concurrencyLevel");
		}
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		if (capacity < concurrencyLevel)
		{
			capacity = concurrencyLevel;
		}
		object[] array = new object[concurrencyLevel];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new object();
		}
		int[] countPerLock = new int[array.Length];
		Node[] array2 = new Node[capacity];
		_tables = new Tables(array2, array, countPerLock);
		_comparer = comparer;
		_growLockArray = growLockArray;
		_budget = array2.Length / array.Length;
	}

	private void InitializeFromCollection(IEnumerable<T> collection)
	{
		foreach (T item in collection)
		{
			if (item == null)
			{
				throw new ArgumentNullException("key");
			}
			if (!TryAddInternal(item, _comparer.GetHashCode(item), acquireLock: false))
			{
				throw new ArgumentException();
			}
		}
		if (_budget == 0)
		{
			_budget = _tables._buckets.Length / _tables._locks.Length;
		}
	}

	public bool ContainsKey(T value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("key");
		}
		return ContainsKeyInternal(value, _comparer.GetHashCode(value));
	}

	private bool ContainsKeyInternal(T value, int hashcode)
	{
		Tables tables = _tables;
		int bucket = GetBucket(hashcode, tables._buckets.Length);
		for (Node node = Volatile.Read(ref tables._buckets[bucket]); node != null; node = node._next)
		{
			if (hashcode == node._hashcode && _comparer.Equals(node._value, value))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryAdd(T value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("key");
		}
		return TryAddInternal(value, _comparer.GetHashCode(value), acquireLock: true);
	}

	private bool TryAddInternal(T value, int hashcode, bool acquireLock)
	{
		checked
		{
			Tables tables;
			bool flag;
			while (true)
			{
				tables = _tables;
				GetBucketAndLockNo(hashcode, out var bucketNo, out var lockNo, tables._buckets.Length, tables._locks.Length);
				flag = false;
				bool lockTaken = false;
				try
				{
					if (acquireLock)
					{
						Monitor.Enter(tables._locks[lockNo], ref lockTaken);
					}
					if (tables != _tables)
					{
						continue;
					}
					for (Node node = tables._buckets[bucketNo]; node != null; node = node._next)
					{
						if (hashcode == node._hashcode && _comparer.Equals(node._value, value))
						{
							return false;
						}
					}
					Volatile.Write(ref tables._buckets[bucketNo], new Node(value, hashcode, tables._buckets[bucketNo]));
					tables._countPerLock[lockNo]++;
					if (tables._countPerLock[lockNo] > _budget)
					{
						flag = true;
					}
					break;
				}
				finally
				{
					if (lockTaken)
					{
						Monitor.Exit(tables._locks[lockNo]);
					}
				}
			}
			if (flag)
			{
				GrowTable(tables);
			}
			return true;
		}
	}

	public bool TryRemove(T value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("key");
		}
		return TryRemoveInternal(value);
	}

	private bool TryRemoveInternal(T value)
	{
		int hashCode = _comparer.GetHashCode(value);
		while (true)
		{
			Tables tables = _tables;
			GetBucketAndLockNo(hashCode, out var bucketNo, out var lockNo, tables._buckets.Length, tables._locks.Length);
			lock (tables._locks[lockNo])
			{
				if (tables != _tables)
				{
					continue;
				}
				Node node = null;
				for (Node node2 = tables._buckets[bucketNo]; node2 != null; node2 = node2._next)
				{
					if (hashCode == node2._hashcode && _comparer.Equals(node2._value, value))
					{
						if (node == null)
						{
							Volatile.Write(ref tables._buckets[bucketNo], node2._next);
						}
						else
						{
							node._next = node2._next;
						}
						value = node2._value;
						tables._countPerLock[lockNo]--;
						return true;
					}
					node = node2;
				}
				break;
			}
		}
		value = default(T);
		return false;
	}

	public void Clear()
	{
		int locksAcquired = 0;
		try
		{
			AcquireAllLocks(ref locksAcquired);
			Tables tables = (_tables = new Tables(new Node[31], _tables._locks, new int[_tables._countPerLock.Length]));
			_budget = Math.Max(1, tables._buckets.Length / tables._locks.Length);
		}
		finally
		{
			ReleaseLocks(0, locksAcquired);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		Node[] buckets = _tables._buckets;
		for (int i = 0; i < buckets.Length; i++)
		{
			for (Node current = Volatile.Read(ref buckets[i]); current != null; current = current._next)
			{
				yield return current._value;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void GrowTable(Tables tables)
	{
		int locksAcquired = 0;
		try
		{
			AcquireLocks(0, 1, ref locksAcquired);
			if (tables != _tables)
			{
				return;
			}
			long num = 0L;
			for (int i = 0; i < tables._countPerLock.Length; i++)
			{
				num += tables._countPerLock[i];
			}
			if (num < tables._buckets.Length / 4)
			{
				_budget = 2 * _budget;
				if (_budget < 0)
				{
					_budget = int.MaxValue;
				}
				return;
			}
			int j = 0;
			bool flag = false;
			try
			{
				for (j = checked(tables._buckets.Length * 2 + 1); j % 3 == 0 || j % 5 == 0 || j % 7 == 0; j = checked(j + 2))
				{
				}
				if (j > 2146435071)
				{
					flag = true;
				}
			}
			catch (OverflowException)
			{
				flag = true;
			}
			if (flag)
			{
				j = 2146435071;
				_budget = int.MaxValue;
			}
			AcquireLocks(1, tables._locks.Length, ref locksAcquired);
			object[] array = tables._locks;
			if (_growLockArray && tables._locks.Length < 1024)
			{
				array = new object[tables._locks.Length * 2];
				Array.Copy(tables._locks, 0, array, 0, tables._locks.Length);
				for (int k = tables._locks.Length; k < array.Length; k++)
				{
					array[k] = new object();
				}
			}
			Node[] array2 = new Node[j];
			int[] array3 = new int[array.Length];
			for (int l = 0; l < tables._buckets.Length; l++)
			{
				Node node = tables._buckets[l];
				checked
				{
					while (node != null)
					{
						Node next = node._next;
						GetBucketAndLockNo(node._hashcode, out var bucketNo, out var lockNo, array2.Length, array.Length);
						array2[bucketNo] = new Node(node._value, node._hashcode, array2[bucketNo]);
						array3[lockNo]++;
						node = next;
					}
				}
			}
			_budget = Math.Max(1, array2.Length / array.Length);
			_tables = new Tables(array2, array, array3);
		}
		finally
		{
			ReleaseLocks(0, locksAcquired);
		}
	}

	private void AcquireAllLocks(ref int locksAcquired)
	{
		AcquireLocks(0, 1, ref locksAcquired);
		AcquireLocks(1, _tables._locks.Length, ref locksAcquired);
	}

	private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
	{
		object[] locks = _tables._locks;
		for (int i = fromInclusive; i < toExclusive; i++)
		{
			bool lockTaken = false;
			try
			{
				Monitor.Enter(locks[i], ref lockTaken);
			}
			finally
			{
				if (lockTaken)
				{
					locksAcquired++;
				}
			}
		}
	}

	private void ReleaseLocks(int fromInclusive, int toExclusive)
	{
		for (int i = fromInclusive; i < toExclusive; i++)
		{
			Monitor.Exit(_tables._locks[i]);
		}
	}
}
