using System;
using System.IO;

public class PackedBoolArray
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	public int Length
	{
		get
		{
			return length;
		}
		set
		{
			if (value == length)
			{
				return;
			}
			if (value == 0)
			{
				data = null;
				length = value;
				return;
			}
			if (data == null)
			{
				data = new byte[calcArraySize(value)];
				length = value;
				return;
			}
			int num = calcArraySize(length);
			int num2 = calcArraySize(value);
			if (num2 > num)
			{
				byte[] destinationArray = new byte[num2];
				Array.Copy(data, destinationArray, num);
				data = destinationArray;
				length = value;
				return;
			}
			if (num2 < num)
			{
				byte[] destinationArray2 = new byte[num2];
				Array.Copy(data, destinationArray2, num2);
				data = destinationArray2;
			}
			length = num2 * 8;
			for (int i = value; i < num2 * 8; i++)
			{
				this[i] = false;
			}
			length = value;
		}
	}

	public int ByteSize => calcArraySize(Length);

	public bool this[int _i]
	{
		get
		{
			validateIndex(_i);
			return (data[_i / 8] & (1 << _i % 8)) != 0;
		}
		set
		{
			validateIndex(_i);
			if (value)
			{
				data[_i / 8] |= (byte)(1 << _i % 8);
			}
			else
			{
				data[_i / 8] &= (byte)(~(1 << _i % 8));
			}
		}
	}

	public PackedBoolArray(int _length = 0)
	{
		Length = _length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validateIndex(int _i)
	{
		if (_i < 0)
		{
			throw new IndexOutOfRangeException($"Index ({_i}) needs to be non-negative");
		}
		if (_i >= length)
		{
			throw new IndexOutOfRangeException($"Index ({_i}) needs to be lower than length ({Length})");
		}
	}

	public PackedBoolArray Clone()
	{
		PackedBoolArray packedBoolArray = new PackedBoolArray(Length);
		for (int i = 0; i < calcArraySize(Length); i++)
		{
			packedBoolArray.data[i] = data[i];
		}
		return packedBoolArray;
	}

	public void Write(Stream _targetStream)
	{
		StreamUtils.Write7BitEncodedInt(_targetStream, Length);
		if (Length > 0)
		{
			_targetStream.Write(data, 0, ByteSize);
		}
	}

	public void Write(BinaryWriter _targetWriter)
	{
		StreamUtils.Write7BitEncodedInt(_targetWriter.BaseStream, Length);
		if (Length > 0)
		{
			_targetWriter.Write(data);
		}
	}

	public void Write(PooledBinaryWriter _targetWriter)
	{
		_targetWriter.Write7BitEncodedInt(Length);
		if (Length > 0)
		{
			_targetWriter.Write(data);
		}
	}

	public void Read(Stream _sourceStream)
	{
		Length = StreamUtils.Read7BitEncodedInt(_sourceStream);
		if (Length > 0)
		{
			_sourceStream.Read(data, 0, ByteSize);
		}
	}

	public void Read(BinaryReader _sourceReader)
	{
		Length = StreamUtils.Read7BitEncodedInt(_sourceReader.BaseStream);
		if (Length > 0)
		{
			_sourceReader.Read(data, 0, ByteSize);
		}
	}

	public void Read(PooledBinaryReader _sourceReader)
	{
		Length = _sourceReader.Read7BitEncodedInt();
		if (Length > 0)
		{
			_sourceReader.Read(data, 0, ByteSize);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int calcArraySize(int _length)
	{
		return (_length + 7) / 8;
	}

	public void Clear()
	{
		for (int i = 0; i < ByteSize; i++)
		{
			data[i] = 0;
		}
	}
}
