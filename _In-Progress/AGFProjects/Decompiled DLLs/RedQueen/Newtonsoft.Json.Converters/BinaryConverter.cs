using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class BinaryConverter : JsonConverter
{
	private const string BinaryTypeName = "System.Data.Linq.Binary";

	private const string BinaryToArrayName = "ToArray";

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private static ReflectionObject _reflectionObject;

	public override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		byte[] byteArray = GetByteArray(value);
		writer.WriteValue(byteArray);
	}

	private byte[] GetByteArray(object value)
	{
		if (value.GetType().FullName == "System.Data.Linq.Binary")
		{
			EnsureReflectionObject(value.GetType());
			return (byte[])_reflectionObject.GetValue(value, "ToArray");
		}
		if (value is SqlBinary sqlBinary)
		{
			return sqlBinary.Value;
		}
		throw new JsonSerializationException("Unexpected value type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
	}

	private static void EnsureReflectionObject(Type t)
	{
		if (_reflectionObject == null)
		{
			_reflectionObject = ReflectionObject.Create(t, t.GetConstructor(new Type[1] { typeof(byte[]) }), "ToArray");
		}
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			if (!ReflectionUtils.IsNullable(objectType))
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		byte[] array;
		if (reader.TokenType == JsonToken.StartArray)
		{
			array = ReadByteArray(reader);
		}
		else
		{
			if (reader.TokenType != JsonToken.String)
			{
				throw JsonSerializationException.Create(reader, "Unexpected token parsing binary. Expected String or StartArray, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			array = Convert.FromBase64String(reader.Value.ToString());
		}
		Type type = (ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType);
		if (type.FullName == "System.Data.Linq.Binary")
		{
			EnsureReflectionObject(type);
			return _reflectionObject.Creator(array);
		}
		if (type == typeof(SqlBinary))
		{
			return new SqlBinary(array);
		}
		throw JsonSerializationException.Create(reader, "Unexpected object type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, objectType));
	}

	private byte[] ReadByteArray(JsonReader reader)
	{
		List<byte> list = new List<byte>();
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
			case JsonToken.Integer:
				list.Add(Convert.ToByte(reader.Value, CultureInfo.InvariantCulture));
				break;
			case JsonToken.EndArray:
				return list.ToArray();
			default:
				throw JsonSerializationException.Create(reader, "Unexpected token when reading bytes: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			case JsonToken.Comment:
				break;
			}
		}
		throw JsonSerializationException.Create(reader, "Unexpected end when reading bytes.");
	}

	public override bool CanConvert(Type objectType)
	{
		if (objectType.FullName == "System.Data.Linq.Binary")
		{
			return true;
		}
		if (objectType == typeof(SqlBinary) || objectType == typeof(SqlBinary?))
		{
			return true;
		}
		return false;
	}
}
