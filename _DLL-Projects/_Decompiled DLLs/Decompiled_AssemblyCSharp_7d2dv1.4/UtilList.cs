using System;
using System.Collections.Generic;

public class UtilList<T> where T : class
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int capacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public int StartId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int EndId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsStandardState;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsFull;

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] InternArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public T NullValue;

	public int Capacity => capacity;

	public int Count => count;

	public T this[int Id]
	{
		get
		{
			if (Count == 0)
			{
				throw new ArgumentException("UtilList is EMPTY");
			}
			int num = StartId + Id;
			if (IsStandardState)
			{
				if (num >= EndId)
				{
					throw new ArgumentException("UtilList index is out of range");
				}
			}
			else if (num >= capacity)
			{
				num -= capacity;
				if (num >= EndId)
				{
					throw new ArgumentException("UtilList Index is out of range");
				}
			}
			return InternArray[num];
		}
		set
		{
			if (Count == 0)
			{
				throw new ArgumentException("UtilList is EMPTY");
			}
			int num = StartId + Id;
			if (IsStandardState)
			{
				if (num >= EndId)
				{
					throw new ArgumentException("UtilList : Index is out of range");
				}
			}
			else if (num >= capacity)
			{
				num -= capacity;
				if (num >= EndId)
				{
					throw new ArgumentException("UtilList : Index is out of range");
				}
			}
			InternArray[num] = value;
		}
	}

	public UtilList(int _capacity, T _NullValue)
	{
		capacity = _capacity;
		StartId = 0;
		EndId = 0;
		count = 0;
		IsStandardState = true;
		IsFull = false;
		InternArray = new T[_capacity];
		NullValue = _NullValue;
	}

	public void Clear()
	{
		StartId = 0;
		EndId = 0;
		count = 0;
		IsFull = false;
		IsStandardState = true;
	}

	public void Add(T NewItem)
	{
		if (IsFull)
		{
			throw new ArgumentException("Overflow : UtilList is full");
		}
		InternArray[EndId] = NewItem;
		if (++EndId == capacity)
		{
			EndId = 0;
			IsStandardState = false;
		}
		if (EndId == StartId)
		{
			IsFull = true;
		}
		count++;
	}

	public T Peek()
	{
		return InternArray[StartId];
	}

	public T Dequeue()
	{
		if (StartId == EndId && !IsFull)
		{
			throw new ArgumentException("UtilList is EMPTY");
		}
		int startId = StartId;
		StartId++;
		if (StartId == capacity)
		{
			StartId = 0;
			IsStandardState = true;
		}
		IsFull = false;
		count--;
		return InternArray[startId];
	}

	public void RemoveNFirst(int N)
	{
		if (count < N)
		{
			throw new ArgumentException("UtilList Out of range");
		}
		StartId += N;
		if (StartId >= capacity)
		{
			StartId -= capacity;
			IsStandardState = true;
		}
		count -= N;
		IsFull = count == capacity;
	}

	public void RemoveAt(List<int> IdList)
	{
		int num = IdList.Count;
		for (int i = 0; i < num; i++)
		{
			this[IdList[i]] = NullValue;
		}
		num = 0;
		for (int j = 0; j < Count; j++)
		{
			if (this[j] != null)
			{
				this[num++] = this[j];
			}
		}
		count = num;
		EndId = StartId + count;
		if (EndId >= capacity)
		{
			EndId -= capacity;
			IsStandardState = false;
		}
		else
		{
			IsStandardState = true;
		}
		IsFull = count == capacity;
	}
}
