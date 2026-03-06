public class Array3DWithOffset<T>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public T[] data;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int addX;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int addY;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int addZ;

	public int DimX;

	public int DimY;

	public int DimZ;

	public virtual T this[int _x, int _y, int _z]
	{
		get
		{
			return data[GetIndex(_x, _y, _z)];
		}
		set
		{
			data[GetIndex(_x, _y, _z)] = value;
		}
	}

	public Array3DWithOffset()
	{
	}

	public Array3DWithOffset(int _dimX, int _dimY, int _dimZ)
	{
		DimX = _dimX;
		DimY = _dimY;
		DimZ = _dimZ;
		data = new T[_dimX * _dimY * _dimZ];
		addX = _dimX / 2;
		addY = _dimY / 2;
		addZ = _dimZ / 2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetIndex(int _x, int _y, int _z)
	{
		return _x + addX + (_z + addZ) * DimX + (_y + addY) * DimZ * DimX;
	}

	public bool Contains(int _x, int _y, int _z)
	{
		if (_x >= -addX && _y >= -addY && _x < addX - 1 && _y < addY - 1 && _z < addZ - 1)
		{
			return _z < addZ - 1;
		}
		return false;
	}
}
