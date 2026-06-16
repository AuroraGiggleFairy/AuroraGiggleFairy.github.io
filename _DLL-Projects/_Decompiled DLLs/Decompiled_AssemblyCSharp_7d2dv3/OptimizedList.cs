using System;
using System.Collections.Generic;

[Serializable]
public class OptimizedList<T>
{
	public T[] array;

	public int Count;

	public int length;

	public int DoubleSize;

	public bool IsValueType;

	public int Length => Count;

	public OptimizedList()
		: this(10)
	{
		IsValueType = typeof(T).IsValueType;
	}

	public OptimizedList(int Capacity)
	{
		Count = 0;
		array = new T[Capacity];
		length = Capacity;
		DoubleSize = 0;
		IsValueType = typeof(T).IsValueType;
	}

	public OptimizedList(OptimizedList<T> L)
	{
		if (L.Count == 0)
		{
			Count = 0;
			array = new T[2];
			length = 2;
			return;
		}
		Count = 0;
		array = new T[L.Count];
		length = L.Count;
		DoubleSize = 0;
		IsValueType = typeof(T).IsValueType;
		AddRange(L);
	}

	public OptimizedList(T[] L)
	{
		if (L.Length == 0)
		{
			Count = 0;
			array = new T[2];
			length = 2;
			return;
		}
		Count = 0;
		array = new T[L.Length];
		length = L.Length;
		DoubleSize = 0;
		IsValueType = typeof(T).IsValueType;
		AddRange(L);
	}

	public OptimizedList(int Capacity, int DoubleSize)
	{
		Count = 0;
		array = new T[Capacity];
		length = Capacity;
		this.DoubleSize = DoubleSize;
		IsValueType = typeof(T).IsValueType;
	}

	public T Last()
	{
		if (Count > 0)
		{
			return array[Count - 1];
		}
		return default(T);
	}

	public void Add(T value)
	{
		if (length == Count)
		{
			length = ((DoubleSize == 0) ? (Count * 2) : (Count + DoubleSize));
			T[] destinationArray = new T[length];
			Array.Copy(array, 0, destinationArray, 0, Count);
			array = destinationArray;
		}
		array[Count++] = value;
	}

	public void Add(ref T value)
	{
		if (length == Count)
		{
			length = ((DoubleSize == 0) ? (Count * 2) : (Count + DoubleSize));
			T[] destinationArray = new T[length];
			Array.Copy(array, 0, destinationArray, 0, Count);
			array = destinationArray;
		}
		array[Count++] = value;
	}

	public void AddSafe(T value)
	{
		array[Count++] = value;
	}

	public void AddSafe(T valueA, T valueB, T valueC, T valueD)
	{
		if (length - (Count + 4) <= 0)
		{
			length = ((DoubleSize == 0) ? (Count * 2 + 4) : (Count + DoubleSize + 4));
			T[] destinationArray = new T[length];
			Array.Copy(array, 0, destinationArray, 0, Count);
			array = destinationArray;
		}
		array[Count++] = valueA;
		array[Count++] = valueB;
		array[Count++] = valueC;
		array[Count++] = valueD;
	}

	public void AddRange(T[] values)
	{
		int num = values.Length;
		if (length - (Count + num) <= 0)
		{
			T[] destinationArray = new T[Count * 2 + num];
			Array.Copy(array, 0, destinationArray, 0, Count);
			array = destinationArray;
			length = Count * 2 + num;
		}
		Array.Copy(values, 0, array, Count, num);
		Count += num;
	}

	public void AddRange(OptimizedList<T> values)
	{
		if (values != null && values.Count != 0)
		{
			int num = values.Length;
			if (length - (Count + num) <= 0)
			{
				length = Count * 2 + num;
				T[] destinationArray = new T[length];
				Array.Copy(array, 0, destinationArray, 0, Count);
				array = destinationArray;
			}
			Array.Copy(values.array, 0, array, Count, num);
			Count += num;
		}
	}

	public bool Remove(T obj)
	{
		int num = Array.IndexOf(array, obj, 0, Count);
		if (num >= 0)
		{
			Count--;
			if (num < Count)
			{
				Array.Copy(array, num + 1, array, num, Count - num);
			}
			array[Count] = default(T);
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		if ((uint)index < (uint)Count)
		{
			Count--;
			if (index < Count)
			{
				Array.Copy(array, index + 1, array, index, Count - index);
			}
			array[Count] = default(T);
		}
	}

	public bool Contains(T obj)
	{
		if (Array.IndexOf(array, obj, 0, Count) >= 0)
		{
			return true;
		}
		return false;
	}

	public void Set(T[] values)
	{
		array = values;
		length = values.Length;
		Count = length;
	}

	public void Clear()
	{
		if (Count > 0)
		{
			Array.Clear(array, 0, array.Length);
		}
		Count = 0;
	}

	public T[] ToArray()
	{
		if (Count == 0)
		{
			return null;
		}
		T[] array = new T[Count];
		Array.Copy(this.array, array, Count);
		return array;
	}

	public void CheckArray(int Size)
	{
		if (length - (Count + Size) <= 0)
		{
			length = ((DoubleSize == 0) ? (Count * 2 + Size) : (Count + DoubleSize + Size));
			T[] destinationArray = new T[length];
			Array.Copy(array, 0, destinationArray, 0, Count);
			array = destinationArray;
		}
	}

	public void Sort(IComparer<T> comparer)
	{
		Sort(0, Count, comparer);
	}

	public void Sort(int index, int count, IComparer<T> comparer)
	{
		if (length - index >= count)
		{
			Array.Sort(array, index, count, comparer);
		}
	}
}
