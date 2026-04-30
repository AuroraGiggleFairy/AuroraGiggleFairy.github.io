using System.Collections.Generic;

public class ArrayWithOffsetSparse<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, T> myData;

	public T EmptyValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int addX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int addY;

	public int DimX;

	public int DimY;

	public virtual T this[int _x, int _y]
	{
		get
		{
			if (!myData.ContainsKey(makeKey(_x, _y)))
			{
				return EmptyValue;
			}
			return myData[makeKey(_x, _y)];
		}
		set
		{
			myData[makeKey(_x, _y)] = value;
		}
	}

	public ArrayWithOffsetSparse(int _dimX, int _dimY, T _emptyValue)
	{
		DimX = _dimX;
		DimY = _dimY;
		EmptyValue = _emptyValue;
		myData = new Dictionary<long, T>();
		addX = _dimX / 2;
		addY = _dimY / 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long makeKey(int _x, int _y)
	{
		return (((_y + addY) & 0xFFFFFFFFu) << 32) | ((_x + addX) & 0xFFFFFFFFu);
	}

	public bool Contains(int _x, int _y)
	{
		if (_x >= -addX && _y >= -addY && _x < addX - 1)
		{
			return _y < addY - 1;
		}
		return false;
	}
}
