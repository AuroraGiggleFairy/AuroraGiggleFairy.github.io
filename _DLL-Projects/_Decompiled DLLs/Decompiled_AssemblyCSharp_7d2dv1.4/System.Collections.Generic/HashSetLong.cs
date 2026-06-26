using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Collections.Generic;

[Serializable]
[DebuggerDisplay("Count={Count}")]
public class HashSetLong : ICollection<long>, IEnumerable<long>, IEnumerable, ISerializable, IDeserializationCallback
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct Link
	{
		public int HashCode;

		public int Next;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class HashSetEqualityComparer : IEqualityComparer<HashSetLong>
	{
		public bool Equals(HashSetLong lhs, HashSetLong rhs)
		{
			if (lhs == rhs)
			{
				return true;
			}
			if (lhs == null || rhs == null || lhs.Count != rhs.Count)
			{
				return false;
			}
			foreach (long lh in lhs)
			{
				if (!rhs.Contains(lh))
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(HashSetLong hashset)
		{
			if (hashset == null)
			{
				return 0;
			}
			IEqualityComparer<long> equalityComparer = EqualityComparer<long>.Default;
			int num = 0;
			foreach (long item in hashset)
			{
				num ^= equalityComparer.GetHashCode(item);
			}
			return num;
		}
	}

	[Serializable]
	public struct Enumerator : IEnumerator<long>, IEnumerator, IDisposable
	{
		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public HashSetLong hashset;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public int next;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public int stamp;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public long current;

		public long Current => current;

		object IEnumerator.Current
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				CheckState();
				if (next <= 0)
				{
					throw new InvalidOperationException("Current is not valid");
				}
				return current;
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public Enumerator(HashSetLong hashset)
		{
			this = default(Enumerator);
			this.hashset = hashset;
			stamp = hashset.generation;
		}

		public bool MoveNext()
		{
			CheckState();
			if (next < 0)
			{
				return false;
			}
			while (next < hashset.touched)
			{
				int num = next++;
				if (hashset.GetLinkHashCode(num) != 0)
				{
					current = hashset.slots[num];
					return true;
				}
			}
			next = -1;
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		void IEnumerator.Reset()
		{
			CheckState();
			next = 0;
		}

		public void Dispose()
		{
			hashset = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CheckState()
		{
			if (hashset == null)
			{
				throw new ObjectDisposedException(null);
			}
			if (hashset.generation != stamp)
			{
				throw new InvalidOperationException("HashSet have been modified while it was iterated over");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static class PrimeHelper
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly int[] primes_table = new int[34]
		{
			11, 19, 37, 73, 109, 163, 251, 367, 557, 823,
			1237, 1861, 2777, 4177, 6247, 9371, 14057, 21089, 31627, 47431,
			71143, 106721, 160073, 240101, 360163, 540217, 810343, 1215497, 1823231, 2734867,
			4102283, 6153409, 9230113, 13845163
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool TestPrime(int x)
		{
			if ((x & 1) != 0)
			{
				int num = (int)Math.Sqrt(x);
				for (int i = 3; i < num; i += 2)
				{
					if (x % i == 0)
					{
						return false;
					}
				}
				return true;
			}
			return x == 2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int CalcPrime(int x)
		{
			for (int i = (x & -2) - 1; i < int.MaxValue; i += 2)
			{
				if (TestPrime(i))
				{
					return i;
				}
			}
			return x;
		}

		public static int ToPrime(int x)
		{
			for (int i = 0; i < primes_table.Length; i++)
			{
				if (x <= primes_table[i])
				{
					return primes_table[i];
				}
			}
			return CalcPrime(x);
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int INITIAL_SIZE = 10;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float DEFAULT_LOAD_FACTOR = 0.9f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int NO_SLOT = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const int HASH_FLAG = int.MinValue;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] table;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Link[] links;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public long[] slots;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int touched;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int empty_slot;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int threshold;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public IEqualityComparer<long> comparer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SerializationInfo si;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int generation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly HashSetEqualityComparer setComparer = new HashSetEqualityComparer();

	public int Count => count;

	public IEqualityComparer<long> Comparer => comparer;

	bool ICollection<long>.IsReadOnly
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return false;
		}
	}

	public HashSetLong()
	{
		Init(10, null);
	}

	public HashSetLong(IEqualityComparer<long> comparer)
	{
		Init(10, comparer);
	}

	public HashSetLong(IEnumerable<long> collection)
		: this(collection, null)
	{
	}

	public HashSetLong(IEnumerable<long> collection, IEqualityComparer<long> comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		int capacity = 0;
		if (collection is ICollection<long> collection2)
		{
			capacity = collection2.Count;
		}
		Init(capacity, comparer);
		foreach (long item in collection)
		{
			Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSetLong(SerializationInfo info, StreamingContext context)
	{
		si = info;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(int capacity, IEqualityComparer<long> comparer)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		this.comparer = comparer ?? EqualityComparer<long>.Default;
		if (capacity == 0)
		{
			capacity = 10;
		}
		capacity = (int)((float)capacity / 0.9f) + 1;
		InitArrays(capacity);
		generation = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitArrays(int size)
	{
		table = new int[size];
		links = new Link[size];
		empty_slot = -1;
		slots = new long[size];
		touched = 0;
		threshold = (int)((float)table.Length * 0.9f);
		if (threshold == 0 && table.Length != 0)
		{
			threshold = 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SlotsContainsAt(int index, int hash, long item)
	{
		int num = table[index] - 1;
		while (num != -1)
		{
			Link link = links[num];
			if (link.HashCode == hash && item == slots[num])
			{
				return true;
			}
			num = link.Next;
		}
		return false;
	}

	public void CopyTo(long[] array)
	{
		CopyTo(array, 0, count);
	}

	public void CopyTo(long[] array, int arrayIndex)
	{
		CopyTo(array, arrayIndex, count);
	}

	public void CopyTo(long[] array, int arrayIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (arrayIndex > array.Length)
		{
			throw new ArgumentException("index larger than largest valid index of array");
		}
		if (array.Length - arrayIndex < count)
		{
			throw new ArgumentException("Destination array cannot hold the requested elements!");
		}
		int i = 0;
		int num = 0;
		for (; i < touched; i++)
		{
			if (num >= count)
			{
				break;
			}
			if (GetLinkHashCode(i) != 0)
			{
				array[arrayIndex++] = slots[i];
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Resize()
	{
		int num = PrimeHelper.ToPrime((table.Length << 1) | 1);
		int[] array = new int[num];
		Link[] array2 = new Link[num];
		for (int i = 0; i < table.Length; i++)
		{
			for (int num2 = table[i] - 1; num2 != -1; num2 = links[num2].Next)
			{
				int num3 = ((array2[num2].HashCode = ((int)slots[num2] ^ (int)(slots[num2] >> 32)) | int.MinValue) & 0x7FFFFFFF) % num;
				array2[num2].Next = array[num3] - 1;
				array[num3] = num2 + 1;
			}
		}
		table = array;
		links = array2;
		long[] destinationArray = new long[num];
		Array.Copy(slots, 0, destinationArray, 0, touched);
		slots = destinationArray;
		threshold = (int)((float)num * 0.9f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetLinkHashCode(int index)
	{
		return links[index].HashCode & int.MinValue;
	}

	public bool Add(long item)
	{
		int num = ((int)item ^ (int)(item >> 32)) | int.MinValue;
		int num2 = (num & 0x7FFFFFFF) % table.Length;
		if (SlotsContainsAt(num2, num, item))
		{
			return false;
		}
		if (++count > threshold)
		{
			Resize();
			num2 = (num & 0x7FFFFFFF) % table.Length;
		}
		int num3 = empty_slot;
		if (num3 == -1)
		{
			num3 = touched++;
		}
		else
		{
			empty_slot = links[num3].Next;
		}
		links[num3].HashCode = num;
		links[num3].Next = table[num2] - 1;
		table[num2] = num3 + 1;
		slots[num3] = item;
		generation++;
		return true;
	}

	public void Clear()
	{
		count = 0;
		Array.Clear(table, 0, table.Length);
		Array.Clear(slots, 0, slots.Length);
		Array.Clear(links, 0, links.Length);
		empty_slot = -1;
		touched = 0;
		generation++;
	}

	public bool Contains(long item)
	{
		int num = ((int)item ^ (int)(item >> 32)) | int.MinValue;
		int index = (num & 0x7FFFFFFF) % table.Length;
		return SlotsContainsAt(index, num, item);
	}

	public bool Remove(long item)
	{
		int num = ((int)item ^ (int)(item >> 32)) | int.MinValue;
		int num2 = (num & 0x7FFFFFFF) % table.Length;
		int num3 = table[num2] - 1;
		if (num3 == -1)
		{
			return false;
		}
		int num4 = -1;
		do
		{
			Link link = links[num3];
			if (link.HashCode == num && slots[num3] == item)
			{
				break;
			}
			num4 = num3;
			num3 = link.Next;
		}
		while (num3 != -1);
		if (num3 == -1)
		{
			return false;
		}
		count--;
		if (num4 == -1)
		{
			table[num2] = links[num3].Next + 1;
		}
		else
		{
			links[num4].Next = links[num3].Next;
		}
		links[num3].Next = empty_slot;
		empty_slot = num3;
		links[num3].HashCode = 0;
		slots[num3] = 0L;
		generation++;
		return true;
	}

	public int RemoveWhere(Predicate<long> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		List<long> list = new List<long>();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				long current = enumerator.Current;
				if (match(current))
				{
					list.Add(current);
				}
			}
		}
		foreach (long item in list)
		{
			Remove(item);
		}
		return list.Count;
	}

	public void TrimExcess()
	{
		Resize();
	}

	public void IntersectWith(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		HashSetLong other_set = ToSet(other);
		RemoveWhere([PublicizedFrom(EAccessModifier.Internal)] (long item) => !other_set.Contains(item));
	}

	public void ExceptWithHashSetLong(HashSetLong other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (long item in other)
		{
			Remove(item);
		}
	}

	public void ExceptWith(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (long item in other)
		{
			Remove(item);
		}
	}

	public bool Overlaps(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (long item in other)
		{
			if (Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool SetEquals(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		HashSetLong hashSetLong = ToSet(other);
		if (count != hashSetLong.Count)
		{
			return false;
		}
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				long current = enumerator.Current;
				if (!hashSetLong.Contains(current))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SymmetricExceptWith(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (long item in ToSet(other))
		{
			if (!Add(item))
			{
				Remove(item);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong ToSet(IEnumerable<long> enumerable)
	{
		HashSetLong hashSetLong = enumerable as HashSetLong;
		if (hashSetLong == null || !Comparer.Equals(hashSetLong.Comparer))
		{
			hashSetLong = new HashSetLong(enumerable, Comparer);
		}
		return hashSetLong;
	}

	public void UnionWithHashSetLong(HashSetLong other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (long item in other)
		{
			Add(item);
		}
	}

	public void UnionWith(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (long item in other)
		{
			Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckIsSubsetOf(HashSetLong other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				long current = enumerator.Current;
				if (!other.Contains(current))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsSubsetOf(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (count == 0)
		{
			return true;
		}
		HashSetLong hashSetLong = ToSet(other);
		if (count > hashSetLong.Count)
		{
			return false;
		}
		return CheckIsSubsetOf(hashSetLong);
	}

	public bool IsProperSubsetOf(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (count == 0)
		{
			return true;
		}
		HashSetLong hashSetLong = ToSet(other);
		if (count >= hashSetLong.Count)
		{
			return false;
		}
		return CheckIsSubsetOf(hashSetLong);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckIsSupersetOf(HashSetLong other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (long item in other)
		{
			if (!Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsSupersetOf(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		HashSetLong hashSetLong = ToSet(other);
		if (count < hashSetLong.Count)
		{
			return false;
		}
		return CheckIsSupersetOf(hashSetLong);
	}

	public bool IsProperSupersetOf(IEnumerable<long> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		HashSetLong hashSetLong = ToSet(other);
		if (count <= hashSetLong.Count)
		{
			return false;
		}
		return CheckIsSupersetOf(hashSetLong);
	}

	public static IEqualityComparer<HashSetLong> CreateSetComparer()
	{
		return setComparer;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("Version", generation);
		info.AddValue("Comparer", comparer, typeof(IEqualityComparer<long>));
		info.AddValue("Capacity", (table != null) ? table.Length : 0);
		if (table != null)
		{
			long[] array = new long[count];
			CopyTo(array);
			info.AddValue("Elements", array, typeof(long[]));
		}
	}

	public virtual void OnDeserialization(object sender)
	{
		if (si == null)
		{
			return;
		}
		generation = (int)si.GetValue("Version", typeof(int));
		comparer = (IEqualityComparer<long>)si.GetValue("Comparer", typeof(IEqualityComparer<long>));
		int num = (int)si.GetValue("Capacity", typeof(int));
		empty_slot = -1;
		if (num > 0)
		{
			table = new int[num];
			slots = new long[num];
			long[] array = (long[])si.GetValue("Elements", typeof(long[]));
			if (array == null)
			{
				throw new SerializationException("Missing Elements");
			}
			for (int i = 0; i < array.Length; i++)
			{
				Add(array[i]);
			}
		}
		else
		{
			table = null;
		}
		si = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator<long> IEnumerable<long>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	void ICollection<long>.Add(long item)
	{
		Add(item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
