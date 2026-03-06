using System;
using System.IO;

public struct TextureFullArray : IEquatable<TextureFullArray>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe fixed long values[1];

	public static readonly TextureFullArray _default = new TextureFullArray(0L);

	public unsafe long this[int index]
	{
		get
		{
			if (index < 0 || index >= 1)
			{
				throw new IndexOutOfRangeException($"Index {index} is outside of the valid range of min: 0, max: {1}.");
			}
			return values[index];
		}
		set
		{
			if (index < 0 || index >= 1)
			{
				throw new IndexOutOfRangeException($"Index {index} is outside of the valid range of min: 0, max: {1}.");
			}
			values[index] = value;
		}
	}

	public static TextureFullArray Default => _default;

	public bool IsDefault => Equals(_default);

	public TextureFullArray(long _fillValue)
	{
		Fill(_fillValue);
	}

	public unsafe void Fill(long _fillValue)
	{
		for (int i = 0; i < 1; i++)
		{
			values[i] = _fillValue;
		}
	}

	public unsafe void Read(BinaryReader _br, int count = 1)
	{
		int i;
		for (i = 0; i < count; i++)
		{
			long num = _br.ReadInt64();
			if (i < 1)
			{
				values[i] = num;
			}
		}
		for (; i < 1; i++)
		{
			values[i] = values[0];
		}
	}

	public unsafe void Write(BinaryWriter _bw)
	{
		for (int i = 0; i < 1; i++)
		{
			_bw.Write(values[i]);
		}
	}

	public unsafe bool Equals(TextureFullArray other)
	{
		for (int i = 0; i < 1; i++)
		{
			if (values[i] != other.values[i])
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is TextureFullArray other)
		{
			return Equals(other);
		}
		return false;
	}

	public unsafe override int GetHashCode()
	{
		int num = 17;
		for (int i = 0; i < 1; i++)
		{
			num = num * 31 + values[i].GetHashCode();
		}
		return num;
	}

	public static bool operator ==(TextureFullArray left, TextureFullArray right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TextureFullArray left, TextureFullArray right)
	{
		return !left.Equals(right);
	}
}
