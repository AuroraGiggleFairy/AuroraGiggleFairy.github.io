using System.Collections.Generic;

public class BucketHashSetList
{
	public List<long> list = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<long> elementsInList = new HashSet<long>();

	public OptimizedList<HashSetLong> buckets;

	public bool IsRecalc;

	public BucketHashSetList()
	{
		buckets = new OptimizedList<HashSetLong>(4);
	}

	public BucketHashSetList(int _noBuckets)
	{
		buckets = new OptimizedList<HashSetLong>(_noBuckets);
		for (int i = 0; i < _noBuckets; i++)
		{
			buckets.Add(new HashSetLong());
		}
	}

	public void Add(int _bucketIdx, long _value)
	{
		buckets.array[_bucketIdx].Add(_value);
	}

	public void Add(int _bucketIdx, HashSetLong _otherBucket)
	{
		buckets.array[_bucketIdx].UnionWithHashSetLong(_otherBucket);
	}

	public bool Contains(long _value)
	{
		if (!IsRecalc)
		{
			return false;
		}
		for (int i = 0; i < buckets.Count; i++)
		{
			if (buckets.array[i].Contains(_value))
			{
				return true;
			}
		}
		return false;
	}

	public void Remove(long _value)
	{
		list.Remove(_value);
		for (int i = 0; i < buckets.Count; i++)
		{
			if (buckets.array[i].Contains(_value))
			{
				buckets.array[i].Remove(_value);
				break;
			}
		}
	}

	public void Clear()
	{
		for (int i = 0; i < buckets.Count; i++)
		{
			buckets.array[i].Clear();
		}
		list.Clear();
		IsRecalc = false;
	}

	public void ExceptTarget(HashSetLong hash)
	{
		if (IsRecalc)
		{
			for (int i = 0; i < buckets.Count; i++)
			{
				hash.ExceptWithHashSetLong(buckets.array[i]);
			}
		}
	}

	public void RecalcHashSetList()
	{
		list.Clear();
		elementsInList.Clear();
		IsRecalc = true;
		for (int i = 0; i < buckets.Count; i++)
		{
			foreach (long item in buckets.array[i])
			{
				if (elementsInList.Add(item))
				{
					list.Add(item);
				}
			}
		}
	}

	public BucketHashSetList Clone()
	{
		BucketHashSetList bucketHashSetList = new BucketHashSetList(buckets.Count);
		for (int i = 0; i < buckets.Count; i++)
		{
			bucketHashSetList.buckets.array[i].UnionWithHashSetLong(buckets.array[i]);
		}
		return bucketHashSetList;
	}
}
