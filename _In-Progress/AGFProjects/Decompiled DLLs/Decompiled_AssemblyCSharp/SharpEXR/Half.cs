using System;
using System.Diagnostics;
using System.Globalization;

namespace SharpEXR;

[Serializable]
public struct Half : IComparable, IFormattable, IConvertible, IComparable<Half>, IEquatable<Half>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[PublicizedFrom(EAccessModifier.Internal)]
	public ushort value;

	public static readonly Half Epsilon = ToHalf(1);

	public static readonly Half MaxValue = ToHalf(31743);

	public static readonly Half MinValue = ToHalf(64511);

	public static readonly Half NaN = ToHalf(65024);

	public static readonly Half NegativeInfinity = ToHalf(64512);

	public static readonly Half PositiveInfinity = ToHalf(31744);

	public Half(float value)
	{
		this = HalfHelper.SingleToHalf(value);
	}

	public Half(int value)
		: this((float)value)
	{
	}

	public Half(long value)
		: this((float)value)
	{
	}

	public Half(double value)
		: this((float)value)
	{
	}

	public Half(decimal value)
		: this((float)value)
	{
	}

	public Half(uint value)
		: this((float)value)
	{
	}

	public Half(ulong value)
		: this((float)value)
	{
	}

	public static Half Negate(Half half)
	{
		return -half;
	}

	public static Half Add(Half half1, Half half2)
	{
		return half1 + half2;
	}

	public static Half Subtract(Half half1, Half half2)
	{
		return half1 - half2;
	}

	public static Half Multiply(Half half1, Half half2)
	{
		return half1 * half2;
	}

	public static Half Divide(Half half1, Half half2)
	{
		return half1 / half2;
	}

	public static Half operator +(Half half)
	{
		return half;
	}

	public static Half operator -(Half half)
	{
		return HalfHelper.Negate(half);
	}

	public static Half operator ++(Half half)
	{
		return (Half)((float)half + 1f);
	}

	public static Half operator --(Half half)
	{
		return (Half)((float)half - 1f);
	}

	public static Half operator +(Half half1, Half half2)
	{
		return (Half)((float)half1 + (float)half2);
	}

	public static Half operator -(Half half1, Half half2)
	{
		return (Half)((float)half1 - (float)half2);
	}

	public static Half operator *(Half half1, Half half2)
	{
		return (Half)((float)half1 * (float)half2);
	}

	public static Half operator /(Half half1, Half half2)
	{
		return (Half)((float)half1 / (float)half2);
	}

	public static bool operator ==(Half half1, Half half2)
	{
		if (!IsNaN(half1))
		{
			return half1.value == half2.value;
		}
		return false;
	}

	public static bool operator !=(Half half1, Half half2)
	{
		return half1.value != half2.value;
	}

	public static bool operator <(Half half1, Half half2)
	{
		return (float)half1 < (float)half2;
	}

	public static bool operator >(Half half1, Half half2)
	{
		return (float)half1 > (float)half2;
	}

	public static bool operator <=(Half half1, Half half2)
	{
		if (!(half1 == half2))
		{
			return half1 < half2;
		}
		return true;
	}

	public static bool operator >=(Half half1, Half half2)
	{
		if (!(half1 == half2))
		{
			return half1 > half2;
		}
		return true;
	}

	public static implicit operator Half(byte value)
	{
		return new Half((float)(int)value);
	}

	public static implicit operator Half(short value)
	{
		return new Half((float)value);
	}

	public static implicit operator Half(char value)
	{
		return new Half((float)(int)value);
	}

	public static implicit operator Half(int value)
	{
		return new Half((float)value);
	}

	public static implicit operator Half(long value)
	{
		return new Half((float)value);
	}

	public static explicit operator Half(float value)
	{
		return new Half(value);
	}

	public static explicit operator Half(double value)
	{
		return new Half((float)value);
	}

	public static explicit operator Half(decimal value)
	{
		return new Half((float)value);
	}

	public static explicit operator byte(Half value)
	{
		return (byte)(float)value;
	}

	public static explicit operator char(Half value)
	{
		return (char)(float)value;
	}

	public static explicit operator short(Half value)
	{
		return (short)(float)value;
	}

	public static explicit operator int(Half value)
	{
		return (int)(float)value;
	}

	public static explicit operator long(Half value)
	{
		return (long)(float)value;
	}

	public static implicit operator float(Half value)
	{
		return HalfHelper.HalfToSingle(value);
	}

	public static implicit operator double(Half value)
	{
		return (float)value;
	}

	public static explicit operator decimal(Half value)
	{
		return (decimal)(float)value;
	}

	public static implicit operator Half(sbyte value)
	{
		return new Half((float)value);
	}

	public static implicit operator Half(ushort value)
	{
		return new Half((float)(int)value);
	}

	public static implicit operator Half(uint value)
	{
		return new Half((float)value);
	}

	public static implicit operator Half(ulong value)
	{
		return new Half((float)value);
	}

	public static explicit operator sbyte(Half value)
	{
		return (sbyte)(float)value;
	}

	public static explicit operator ushort(Half value)
	{
		return (ushort)(float)value;
	}

	public static explicit operator uint(Half value)
	{
		return (uint)(float)value;
	}

	public static explicit operator ulong(Half value)
	{
		return (ulong)(float)value;
	}

	public int CompareTo(Half other)
	{
		int result = 0;
		if (this < other)
		{
			result = -1;
		}
		else if (this > other)
		{
			result = 1;
		}
		else if (this != other)
		{
			if (!IsNaN(this))
			{
				result = 1;
			}
			else if (!IsNaN(other))
			{
				result = -1;
			}
		}
		return result;
	}

	public int CompareTo(object obj)
	{
		int num = 0;
		if (obj == null)
		{
			return 1;
		}
		if (obj is Half)
		{
			return CompareTo((Half)obj);
		}
		throw new ArgumentException("Object must be of type Half.");
	}

	public bool Equals(Half other)
	{
		if (!(other == this))
		{
			if (IsNaN(other))
			{
				return IsNaN(this);
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		bool result = false;
		if (obj is Half half && (half == this || (IsNaN(half) && IsNaN(this))))
		{
			result = true;
		}
		return result;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public TypeCode GetTypeCode()
	{
		return (TypeCode)255;
	}

	public static byte[] GetBytes(Half value)
	{
		return BitConverter.GetBytes(value.value);
	}

	public static ushort GetBits(Half value)
	{
		return value.value;
	}

	public static Half ToHalf(byte[] value, int startIndex)
	{
		return ToHalf((ushort)BitConverter.ToInt16(value, startIndex));
	}

	public static Half ToHalf(ushort bits)
	{
		return new Half
		{
			value = bits
		};
	}

	public static int Sign(Half value)
	{
		if (value < 0)
		{
			return -1;
		}
		if (value > 0)
		{
			return 1;
		}
		if (value != 0)
		{
			throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
		}
		return 0;
	}

	public static Half Abs(Half value)
	{
		return HalfHelper.Abs(value);
	}

	public static Half Max(Half value1, Half value2)
	{
		if (!(value1 < value2))
		{
			return value1;
		}
		return value2;
	}

	public static Half Min(Half value1, Half value2)
	{
		if (!(value1 < value2))
		{
			return value2;
		}
		return value1;
	}

	public static bool IsNaN(Half half)
	{
		return HalfHelper.IsNaN(half);
	}

	public static bool IsInfinity(Half half)
	{
		return HalfHelper.IsInfinity(half);
	}

	public static bool IsNegativeInfinity(Half half)
	{
		return HalfHelper.IsNegativeInfinity(half);
	}

	public static bool IsPositiveInfinity(Half half)
	{
		return HalfHelper.IsPositiveInfinity(half);
	}

	public static Half Parse(string value)
	{
		return (Half)float.Parse(value, CultureInfo.InvariantCulture);
	}

	public static Half Parse(string value, IFormatProvider provider)
	{
		return (Half)float.Parse(value, provider);
	}

	public static Half Parse(string value, NumberStyles style)
	{
		return (Half)float.Parse(value, style, CultureInfo.InvariantCulture);
	}

	public static Half Parse(string value, NumberStyles style, IFormatProvider provider)
	{
		return (Half)float.Parse(value, style, provider);
	}

	public static bool TryParse(string value, out Half result)
	{
		if (float.TryParse(value, out var result2))
		{
			result = (Half)result2;
			return true;
		}
		result = default(Half);
		return false;
	}

	public static bool TryParse(string value, NumberStyles style, IFormatProvider provider, out Half result)
	{
		bool result2 = false;
		if (float.TryParse(value, style, provider, out var result3))
		{
			result = (Half)result3;
			result2 = true;
		}
		else
		{
			result = default(Half);
		}
		return result2;
	}

	public override string ToString()
	{
		return ((float)this).ToString(CultureInfo.InvariantCulture);
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return ((float)this).ToString(formatProvider);
	}

	public string ToString(string format)
	{
		return ((float)this).ToString(format, CultureInfo.InvariantCulture);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return ((float)this).ToString(format, formatProvider);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	TypeCode IConvertible.GetTypeCode()
	{
		return GetTypeCode();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return Convert.ToBoolean(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return Convert.ToByte(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	char IConvertible.ToChar(IFormatProvider provider)
	{
		throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "Char"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", "Half", "DateTime"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return Convert.ToDecimal(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return Convert.ToDouble(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return Convert.ToInt16(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return Convert.ToInt32(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return Convert.ToInt64(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return Convert.ToSByte(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	string IConvertible.ToString(IFormatProvider provider)
	{
		return Convert.ToString(this, CultureInfo.InvariantCulture);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	object IConvertible.ToType(Type conversionType, IFormatProvider provider)
	{
		return ((IConvertible)(float)this).ToType(conversionType, provider);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return Convert.ToUInt16(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return Convert.ToUInt64(this);
	}
}
