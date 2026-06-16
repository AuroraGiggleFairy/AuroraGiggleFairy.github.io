using System;

public class ArrayListMP<T> where T : new()
{
	public readonly MemoryPooledArray<T> pool;

	public T[] Items;

	public int Count;

	public T this[int idx]
	{
		get
		{
			return Items[idx];
		}
		set
		{
			Items[idx] = value;
		}
	}

	public ArrayListMP(MemoryPooledArray<T> _pool, int _minSize = 0)
	{
		pool = _pool;
		Count = 0;
		if (_minSize > 0)
		{
			Items = pool.Alloc(_minSize);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~ArrayListMP()
	{
		if (Items != null)
		{
			pool.Free(Items);
			Items = null;
		}
	}

	public void Add(T _item)
	{
		if (Items == null)
		{
			Items = pool.Alloc();
		}
		if (Count >= Items.Length)
		{
			Items = pool.Grow(Items);
		}
		Items[Count++] = _item;
	}

	public void Clear()
	{
		Count = 0;
		if (Items != null)
		{
			pool.Free(Items);
			Items = null;
		}
	}

	public T[] ToArray()
	{
		if (Items == null)
		{
			return new T[0];
		}
		T[] array = new T[Count];
		Array.Copy(Items, array, Count);
		return array;
	}

	public void AddRange(T[] _range)
	{
		AddRange(_range, 0, _range.Length);
	}

	public void AddRange(T[] _range, int _offs, int _count)
	{
		if (_range != null && _range.Length != 0)
		{
			if (Items == null)
			{
				Items = pool.Alloc(_count);
			}
			if (Count + _count >= Items.Length)
			{
				Items = pool.Grow(Items, Count + _count);
			}
			Array.Copy(_range, _offs, Items, Count, _count);
			Count += _count;
		}
	}

	public int Alloc(int _count)
	{
		if (Items == null)
		{
			Items = pool.Alloc(_count);
		}
		else if (Count + _count > Items.Length)
		{
			Items = pool.Grow(Items, Count + _count);
		}
		int count = Count;
		Count += _count;
		return count;
	}

	public void Grow(int newSize)
	{
		if (Items == null)
		{
			Items = pool.Alloc(newSize);
		}
		else if (newSize > Items.Length)
		{
			Items = pool.Grow(Items, newSize);
		}
	}
}
