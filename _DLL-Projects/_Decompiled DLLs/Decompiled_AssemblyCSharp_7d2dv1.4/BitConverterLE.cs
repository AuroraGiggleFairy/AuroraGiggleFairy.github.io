using System;

[PublicizedFrom(EAccessModifier.Internal)]
public static class BitConverterLE
{
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void GetUIntBytes(byte* bytes, byte[] _buffer)
	{
		if (BitConverter.IsLittleEndian)
		{
			_buffer[0] = *bytes;
			_buffer[1] = bytes[1];
			_buffer[2] = bytes[2];
			_buffer[3] = bytes[3];
		}
		else
		{
			_buffer[0] = bytes[3];
			_buffer[1] = bytes[2];
			_buffer[2] = bytes[1];
			_buffer[3] = *bytes;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void GetULongBytes(byte* bytes, byte[] _buffer)
	{
		if (BitConverter.IsLittleEndian)
		{
			_buffer[0] = *bytes;
			_buffer[1] = bytes[1];
			_buffer[2] = bytes[2];
			_buffer[3] = bytes[3];
			_buffer[4] = bytes[4];
			_buffer[5] = bytes[5];
			_buffer[6] = bytes[6];
			_buffer[7] = bytes[7];
		}
		else
		{
			_buffer[0] = bytes[7];
			_buffer[1] = bytes[6];
			_buffer[2] = bytes[5];
			_buffer[3] = bytes[4];
			_buffer[4] = bytes[3];
			_buffer[5] = bytes[2];
			_buffer[6] = bytes[1];
			_buffer[7] = *bytes;
		}
	}

	public unsafe static void GetBytes(float _value, byte[] _buffer)
	{
		GetUIntBytes((byte*)(&_value), _buffer);
	}

	public unsafe static void GetBytes(double value, byte[] _buffer)
	{
		GetULongBytes((byte*)(&value), _buffer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void UIntFromBytes(byte* _dst, byte[] _src, int _startIndex)
	{
		if (BitConverter.IsLittleEndian)
		{
			*_dst = _src[_startIndex];
			_dst[1] = _src[_startIndex + 1];
			_dst[2] = _src[_startIndex + 2];
			_dst[3] = _src[_startIndex + 3];
		}
		else
		{
			*_dst = _src[_startIndex + 3];
			_dst[1] = _src[_startIndex + 2];
			_dst[2] = _src[_startIndex + 1];
			_dst[3] = _src[_startIndex];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void ULongFromBytes(byte* _dst, byte[] _src, int _startIndex)
	{
		if (BitConverter.IsLittleEndian)
		{
			for (int i = 0; i < 8; i++)
			{
				_dst[i] = _src[_startIndex + i];
			}
		}
		else
		{
			for (int j = 0; j < 8; j++)
			{
				_dst[j] = _src[_startIndex + (7 - j)];
			}
		}
	}

	public unsafe static float ToSingle(byte[] _value, int _startIndex)
	{
		float result = default(float);
		UIntFromBytes((byte*)(&result), _value, _startIndex);
		return result;
	}

	public unsafe static double ToDouble(byte[] _value, int _startIndex)
	{
		double result = default(double);
		ULongFromBytes((byte*)(&result), _value, _startIndex);
		return result;
	}
}
