public class ArrayWithOffset<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public T[,] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeXHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeYHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int addXOffs;

	[PublicizedFrom(EAccessModifier.Private)]
	public int addYOffs;

	public int DimX;

	public int DimY;

	public Vector2i MinPos;

	public Vector2i MaxPos;

	public virtual T this[int _x, int _y]
	{
		get
		{
			return data[_x + addXOffs, _y + addYOffs];
		}
		set
		{
			data[_x + addXOffs, _y + addYOffs] = value;
		}
	}

	public ArrayWithOffset()
	{
	}

	public ArrayWithOffset(int _dimX, int _dimY)
		: this(_dimX, _dimY, 0, 0)
	{
	}

	public ArrayWithOffset(int _dimX, int _dimY, int _addXOffs, int _addYOffs)
	{
		DimX = _dimX;
		DimY = _dimY;
		data = new T[_dimX, _dimY];
		sizeXHalf = _dimX / 2;
		sizeYHalf = _dimY / 2;
		MinPos = new Vector2i(-sizeXHalf - _addXOffs, -sizeYHalf - _addYOffs);
		MaxPos = new Vector2i(sizeXHalf - _addXOffs - 1, sizeYHalf - _addYOffs - 1);
		addXOffs = _addXOffs + sizeXHalf;
		addYOffs = _addYOffs + sizeXHalf;
	}

	public bool Contains(int _x, int _y)
	{
		if (_x >= MinPos.x && _y >= MinPos.y && _x < MaxPos.x)
		{
			return _y < MaxPos.y;
		}
		return false;
	}

	public void CopyInto(ArrayWithOffset<T> _other)
	{
		for (int i = 0; i < data.GetLength(0); i++)
		{
			for (int j = 0; j < data.GetLength(1); j++)
			{
				_other.data[i, j] = data[i, j];
			}
		}
	}
}
