using System;

public class IntHashMap
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IntHashMapEntry[] slots;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public int threshold;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float growFactor = 0.75f;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int versionStamp;

	public IntHashMap()
	{
		threshold = 12;
		slots = new IntHashMapEntry[16];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int computeHash(int _v)
	{
		_v ^= (_v >> 20) ^ (_v >> 12);
		return _v ^ (_v >> 7) ^ (_v >> 4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int getSlotIndex(int _p1, int _p2)
	{
		return _p1 & (_p2 - 1);
	}

	public object lookup(int _p)
	{
		int p = computeHash(_p);
		for (IntHashMapEntry intHashMapEntry = slots[getSlotIndex(p, slots.Length)]; intHashMapEntry != null; intHashMapEntry = intHashMapEntry.nextEntry)
		{
			if (intHashMapEntry.hashEntry == _p)
			{
				return intHashMapEntry.valueEntry;
			}
		}
		return null;
	}

	public bool containsItem(int _item)
	{
		return lookupEntry(_item) != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IntHashMapEntry lookupEntry(int _item)
	{
		int p = computeHash(_item);
		for (IntHashMapEntry intHashMapEntry = slots[getSlotIndex(p, slots.Length)]; intHashMapEntry != null; intHashMapEntry = intHashMapEntry.nextEntry)
		{
			if (intHashMapEntry.hashEntry == _item)
			{
				return intHashMapEntry;
			}
		}
		return null;
	}

	public void addKey(int _key, object _value)
	{
		int num = computeHash(_key);
		int slotIndex = getSlotIndex(num, slots.Length);
		for (IntHashMapEntry intHashMapEntry = slots[slotIndex]; intHashMapEntry != null; intHashMapEntry = intHashMapEntry.nextEntry)
		{
			if (intHashMapEntry.hashEntry == _key)
			{
				intHashMapEntry.valueEntry = _value;
			}
		}
		versionStamp++;
		insert(num, _key, _value, slotIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void grow(int _size)
	{
		if (slots.Length == 1073741824)
		{
			threshold = int.MaxValue;
			return;
		}
		IntHashMapEntry[] other = new IntHashMapEntry[_size];
		copyTo(other);
		slots = other;
		threshold = (int)((float)_size * 0.75f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void copyTo(IntHashMapEntry[] _other)
	{
		IntHashMapEntry[] array = slots;
		int p = _other.Length;
		for (int i = 0; i < array.Length; i++)
		{
			IntHashMapEntry intHashMapEntry = array[i];
			if (intHashMapEntry != null)
			{
				array[i] = null;
				do
				{
					IntHashMapEntry nextEntry = intHashMapEntry.nextEntry;
					int slotIndex = getSlotIndex(intHashMapEntry.slotHash, p);
					intHashMapEntry.nextEntry = _other[slotIndex];
					_other[slotIndex] = intHashMapEntry;
					intHashMapEntry = nextEntry;
				}
				while (intHashMapEntry != null);
			}
		}
	}

	public object removeObject(int _key)
	{
		return removeEntry(_key)?.valueEntry;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IntHashMapEntry removeEntry(int _key)
	{
		int slotIndex = getSlotIndex(computeHash(_key), slots.Length);
		IntHashMapEntry intHashMapEntry = slots[slotIndex];
		IntHashMapEntry intHashMapEntry2 = intHashMapEntry;
		while (intHashMapEntry2 != null)
		{
			IntHashMapEntry nextEntry = intHashMapEntry2.nextEntry;
			if (intHashMapEntry2.hashEntry == _key)
			{
				versionStamp++;
				count--;
				if (intHashMapEntry == intHashMapEntry2)
				{
					slots[slotIndex] = nextEntry;
				}
				else
				{
					intHashMapEntry.nextEntry = nextEntry;
				}
				return intHashMapEntry2;
			}
			intHashMapEntry = intHashMapEntry2;
			intHashMapEntry2 = nextEntry;
		}
		return intHashMapEntry2;
	}

	public void clearMap()
	{
		versionStamp++;
		IntHashMapEntry[] array = slots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = null;
		}
		count = 0;
	}

	public void map(Action<object> callback)
	{
		for (int i = 0; i < slots.Length; i++)
		{
			for (IntHashMapEntry intHashMapEntry = slots[i]; intHashMapEntry != null; intHashMapEntry = intHashMapEntry.nextEntry)
			{
				callback(intHashMapEntry.valueEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void insert(int _i1, int _i2, object _object, int _key)
	{
		IntHashMapEntry entry = slots[_key];
		slots[_key] = new IntHashMapEntry(_i1, _i2, _object, entry);
		if (count++ >= threshold)
		{
			grow(2 * slots.Length);
		}
	}

	public static int getHash(int _v)
	{
		return computeHash(_v);
	}
}
