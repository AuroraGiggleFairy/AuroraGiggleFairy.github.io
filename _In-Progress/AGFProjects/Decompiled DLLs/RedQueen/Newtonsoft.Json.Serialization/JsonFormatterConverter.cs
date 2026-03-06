using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class JsonFormatterConverter : IFormatterConverter
{
	private readonly JsonSerializerInternalReader _reader;

	private readonly JsonISerializableContract _contract;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private readonly JsonProperty _member;

	public JsonFormatterConverter(JsonSerializerInternalReader reader, JsonISerializableContract contract, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonProperty member)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		ValidationUtils.ArgumentNotNull(contract, "contract");
		_reader = reader;
		_contract = contract;
		_member = member;
	}

	private T GetTokenValue<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(object value)
	{
		ValidationUtils.ArgumentNotNull(value, "value");
		return (T)System.Convert.ChangeType(((JValue)value).Value, typeof(T), CultureInfo.InvariantCulture);
	}

	public object Convert(object value, Type type)
	{
		ValidationUtils.ArgumentNotNull(value, "value");
		if (!(value is JToken token))
		{
			throw new ArgumentException("Value is not a JToken.", "value");
		}
		return _reader.CreateISerializableItem(token, type, _contract, _member);
	}

	public object Convert(object value, TypeCode typeCode)
	{
		ValidationUtils.ArgumentNotNull(value, "value");
		return System.Convert.ChangeType((value is JValue jValue) ? jValue.Value : value, typeCode, CultureInfo.InvariantCulture);
	}

	public bool ToBoolean(object value)
	{
		return GetTokenValue<bool>(value);
	}

	public byte ToByte(object value)
	{
		return GetTokenValue<byte>(value);
	}

	public char ToChar(object value)
	{
		return GetTokenValue<char>(value);
	}

	public DateTime ToDateTime(object value)
	{
		return GetTokenValue<DateTime>(value);
	}

	public decimal ToDecimal(object value)
	{
		return GetTokenValue<decimal>(value);
	}

	public double ToDouble(object value)
	{
		return GetTokenValue<double>(value);
	}

	public short ToInt16(object value)
	{
		return GetTokenValue<short>(value);
	}

	public int ToInt32(object value)
	{
		return GetTokenValue<int>(value);
	}

	public long ToInt64(object value)
	{
		return GetTokenValue<long>(value);
	}

	public sbyte ToSByte(object value)
	{
		return GetTokenValue<sbyte>(value);
	}

	public float ToSingle(object value)
	{
		return GetTokenValue<float>(value);
	}

	public string ToString(object value)
	{
		return GetTokenValue<string>(value);
	}

	public ushort ToUInt16(object value)
	{
		return GetTokenValue<ushort>(value);
	}

	public uint ToUInt32(object value)
	{
		return GetTokenValue<uint>(value);
	}

	public ulong ToUInt64(object value)
	{
		return GetTokenValue<ulong>(value);
	}
}
