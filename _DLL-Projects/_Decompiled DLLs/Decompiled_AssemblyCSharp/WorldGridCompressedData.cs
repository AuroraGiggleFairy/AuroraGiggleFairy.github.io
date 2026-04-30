using System;
using System.Runtime.CompilerServices;

public class WorldGridCompressedData<T> where T : unmanaged, IEquatable<T>
{
	public GridCompressedData<T> colors;

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

	public WorldGridCompressedData(T[] _colors, int _dimX, int _dimY, int _gridSizeX, int _gridSizeY)
		: this(_colors, _dimX, _dimY, _gridSizeX, _gridSizeY, 0, 0)
	{
	}

	public WorldGridCompressedData(T[] _colors, int _dimX, int _dimY, int _gridSizeX, int _gridSizeY, int _addXOffs, int _addYOffs)
	{
		colors = new GridCompressedData<T>(_dimX, _dimY, _gridSizeX, _gridSizeY);
		colors.FromArray(_colors);
		DimX = _dimX;
		DimY = _dimY;
		sizeXHalf = _dimX / 2;
		sizeYHalf = _dimY / 2;
		addXOffs = _addXOffs;
		addYOffs = _addYOffs;
		MinPos = new Vector2i(-sizeXHalf - addXOffs, -sizeYHalf - addYOffs);
		MaxPos = new Vector2i(sizeXHalf - addXOffs - 1, sizeYHalf - addYOffs - 1);
	}

	public WorldGridCompressedData(GridCompressedData<T> _data)
		: this(_data, 0, 0)
	{
	}

	public WorldGridCompressedData(GridCompressedData<T> _data, int _addXOffs, int _addYOffs)
	{
		colors = _data;
		DimX = _data.width;
		DimY = _data.height;
		sizeXHalf = DimX / 2;
		sizeYHalf = DimY / 2;
		addXOffs = _addXOffs;
		addYOffs = _addYOffs;
		MinPos = new Vector2i(-sizeXHalf - addXOffs, -sizeYHalf - addYOffs);
		MaxPos = new Vector2i(sizeXHalf - addXOffs - 1, sizeYHalf - addYOffs - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetData(int _x, int _y)
	{
		return colors.GetValue(_x + addXOffs + sizeXHalf, _y + addYOffs + sizeYHalf);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetData(int _offs)
	{
		return colors.GetValue(_offs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(int _x, int _y)
	{
		if (_x >= MinPos.x && _y >= MinPos.y && _x <= MaxPos.x)
		{
			return _y <= MaxPos.y;
		}
		return false;
	}
}
