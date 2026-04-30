using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class ExpandoObjectConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer)
	{
		return ReadValue(reader);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private object ReadValue(JsonReader reader)
	{
		if (!reader.MoveToContent())
		{
			throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
		}
		switch (reader.TokenType)
		{
		case JsonToken.StartObject:
			return ReadObject(reader);
		case JsonToken.StartArray:
			return ReadList(reader);
		default:
			if (JsonTokenUtils.IsPrimitiveToken(reader.TokenType))
			{
				return reader.Value;
			}
			throw JsonSerializationException.Create(reader, "Unexpected token when converting ExpandoObject: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
	}

	private object ReadList(JsonReader reader)
	{
		IList<object> list = new List<object>();
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
			case JsonToken.EndArray:
				return list;
			case JsonToken.Comment:
				continue;
			}
			object item = ReadValue(reader);
			list.Add(item);
		}
		throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
	}

	private object ReadObject(JsonReader reader)
	{
		IDictionary<string, object> dictionary = new ExpandoObject();
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
			case JsonToken.PropertyName:
			{
				string key = reader.Value.ToString();
				if (!reader.Read())
				{
					throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
				}
				object value = ReadValue(reader);
				dictionary[key] = value;
				break;
			}
			case JsonToken.EndObject:
				return dictionary;
			}
		}
		throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ExpandoObject);
	}
}
