using System.IO;

public class SmartArray
{
	public byte[] _array;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _lXPow;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _lYPow;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _lZPow;

	public int _size;

	public int _halfSize;

	public SmartArray(int xPow, int yPow, int zPos)
	{
		_lXPow = xPow;
		_lYPow = yPow;
		_lZPow = zPos;
		_size = (1 << _lXPow) * (1 << _lYPow) * (1 << _lZPow);
		_halfSize = _size / 2;
		_array = new byte[_halfSize];
	}

	public void clear()
	{
		for (int i = 0; i < _array.Length; i++)
		{
			_array[i] = 0;
		}
	}

	public void write(BinaryWriter stream)
	{
		stream.Write(_array);
	}

	public void read(BinaryReader stream)
	{
		_array = stream.ReadBytes(_halfSize);
	}

	public byte get(int x, int y, int z)
	{
		int num = (x << _lXPow << _lYPow) + (y << _lXPow) + z;
		if (num < _halfSize)
		{
			return (byte)(_array[num] & 0xF);
		}
		return (byte)((_array[num % _halfSize] >> 4) & 0xF);
	}

	public void set(int x, int y, int z, byte b)
	{
		int num = (x << _lXPow << _lYPow) + (y << _lXPow) + z;
		int num2 = 0;
		if (num < _halfSize)
		{
			num2 = (_array[num] & 0xF0) | (b & 0xF);
			_array[num] = (byte)num2;
		}
		else
		{
			num2 = ((b << 4) & 0xF0) | (_array[num % _halfSize] & 0xF);
			_array[num % _halfSize] = (byte)num2;
		}
	}

	public int size()
	{
		return _size;
	}

	public int sizePacked()
	{
		return _halfSize;
	}

	public void copyFrom(SmartArray _other)
	{
		_other._array.CopyTo(_array, 0);
	}

	public int GetUsedMem()
	{
		return _array.Length;
	}
}
