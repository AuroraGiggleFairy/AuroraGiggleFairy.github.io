using System.Collections.Generic;

public class ArrayDynamicFast<T>
{
	public T[] Data;

	public bool[] DataAvail;

	public int Count;

	public int Size;

	public ArrayDynamicFast(int _size)
	{
		Size = _size;
		Data = new T[_size];
		DataAvail = new bool[_size];
		Count = 0;
	}

	public int Contains(T _v)
	{
		if (Count == 0)
		{
			return -1;
		}
		if (_v == null)
		{
			for (int i = 0; i < Data.Length; i++)
			{
				if (DataAvail[i] && Data[i] == null)
				{
					return i;
				}
			}
		}
		else
		{
			EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			for (int j = 0; j < Data.Length; j++)
			{
				if (DataAvail[j] && equalityComparer.Equals(Data[j], _v))
				{
					return j;
				}
			}
		}
		return -1;
	}

	public void Clear()
	{
		for (int i = 0; i < Data.Length; i++)
		{
			DataAvail[i] = false;
		}
	}

	public void Add(int _idx, T _texId)
	{
		if (_idx == -1)
		{
			for (int i = 0; i < Size; i++)
			{
				if (!DataAvail[i])
				{
					_idx = i;
					break;
				}
			}
		}
		if (_idx != -1)
		{
			Data[_idx] = _texId;
			DataAvail[_idx] = true;
			Count++;
		}
	}
}
