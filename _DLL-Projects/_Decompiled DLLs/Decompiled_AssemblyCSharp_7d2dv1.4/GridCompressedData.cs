using System;

public class GridCompressedData<T> where T : unmanaged, IEquatable<T>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public T[][] cells;

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] sameValues;

	public int cellSizeX;

	public int cellSizeY;

	public int widthCells;

	public int heightCells;

	public int width;

	public int height;

	public GridCompressedData(int _width, int _height, int _cellSizeX, int _cellSizeY)
	{
		if (_width % _cellSizeX != 0)
		{
			throw new Exception($"Cell width must be a multiple of data width. Cell width: {_cellSizeX}, width: {_width}");
		}
		if (_height % _cellSizeY != 0)
		{
			throw new Exception($"Cell height must be a multiple of data height. Cell height: {_cellSizeX}, height: {_width}");
		}
		width = _width;
		height = _height;
		cellSizeX = _cellSizeX;
		cellSizeY = _cellSizeY;
		widthCells = _width / cellSizeX;
		heightCells = _height / cellSizeY;
		int num = widthCells * heightCells;
		cells = new T[num][];
		sameValues = new T[num];
	}

	public void SetValue(int _x, int _y, T value)
	{
		int num = _x / cellSizeX + _y / cellSizeY * widthCells;
		T[] array = cells[num];
		if (array == null)
		{
			T val = sameValues[num];
			if (value.Equals(val))
			{
				return;
			}
			array = (cells[num] = new T[cellSizeX * cellSizeY]);
			Array.Fill(array, val);
		}
		int num2 = _x % cellSizeX + _y % cellSizeY * cellSizeX;
		array[num2] = value;
	}

	public void SetValue(int _cellIndex, int _cellX, int _cellY, T _value)
	{
		T[] array = cells[_cellIndex];
		if (array == null)
		{
			T value = sameValues[_cellIndex];
			if (_value.Equals(sameValues[_cellIndex]))
			{
				return;
			}
			array = (cells[_cellIndex] = new T[cellSizeX * cellSizeY]);
			Array.Fill(array, value);
		}
		int num = _cellX + _cellY * cellSizeX;
		array[num] = _value;
	}

	public void SetSameValue(int _cellIndex, T _value)
	{
		sameValues[_cellIndex] = _value;
		cells[_cellIndex] = null;
	}

	public void Fill(T _value)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			sameValues[i] = _value;
			cells[i] = null;
		}
	}

	public T GetValue(int _x, int _y)
	{
		int num = _x / cellSizeX + _y / cellSizeY * widthCells;
		T[] array = cells[num];
		if (array == null)
		{
			return sameValues[num];
		}
		int num2 = _x % cellSizeX + _y % cellSizeY * cellSizeX;
		return array[num2];
	}

	public T GetValue(int _offs)
	{
		return GetValue(_offs % width, _offs / width);
	}

	public void CheckSameValue(int _cellIndex)
	{
		T[] array = cells[_cellIndex];
		if (array != null && CheckSameValue(cells[_cellIndex]))
		{
			sameValues[_cellIndex] = array[0];
			cells[_cellIndex] = null;
		}
	}

	public void CheckSameValues()
	{
		for (int i = 0; i < sameValues.Length; i++)
		{
			CheckSameValue(i);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CheckSameValue(T[] _cell)
	{
		T val = _cell[0];
		for (int i = 1; i < _cell.Length; i++)
		{
			if (!val.Equals(_cell[i]))
			{
				return false;
			}
		}
		return true;
	}

	public T[] ToArray()
	{
		T[] array = new T[widthCells * cellSizeX * heightCells * cellSizeY];
		for (int i = 0; i < cells.Length; i++)
		{
			int num = i % widthCells;
			int num2 = i / widthCells;
			int num3 = num * cellSizeX + num2 * cellSizeY * width;
			T[] array2 = cells[i];
			if (array2 == null)
			{
				T value = sameValues[i];
				for (int j = 0; j < cellSizeY; j++)
				{
					Array.Fill(array, value, num3, cellSizeX);
					num3 += width;
				}
				continue;
			}
			int num4 = 0;
			for (int k = 0; k < cellSizeY; k++)
			{
				Array.Copy(array2, num4, array, num3, cellSizeX);
				num3 += width;
				num4 += cellSizeX;
			}
		}
		return array;
	}

	public void FromArray(T[] _pixs)
	{
		if (_pixs.Length != width * height)
		{
			throw new Exception($"Source array does not contain enough data. Expected length: {width * height}, Actual length: {_pixs.Length}");
		}
		int num = 0;
		for (int i = 0; i < heightCells; i++)
		{
			for (int j = 0; j < widthCells; j++)
			{
				int num2 = i * cellSizeY;
				int num3 = j * cellSizeX;
				T value = _pixs[num3 + num2 * width];
				SetSameValue(num, value);
				for (int k = 0; k < cellSizeY; k++)
				{
					for (int l = 0; l < cellSizeX; l++)
					{
						int num4 = num3 + l + (num2 + k) * width;
						SetValue(num, l, k, _pixs[num4]);
					}
				}
				num++;
			}
		}
	}

	public int EstimateOwnedBytes()
	{
		return 0 + MemoryTracker.GetSize(sameValues) + MemoryTracker.GetSize(cells);
	}
}
