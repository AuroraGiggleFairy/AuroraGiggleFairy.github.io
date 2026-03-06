using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord.Net.Converters;

internal class ArrayConverter<T> : JsonConverter
{
	private readonly JsonConverter _innerConverter;

	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public ArrayConverter(JsonConverter innerConverter)
	{
		_innerConverter = innerConverter;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		List<T> list = new List<T>();
		if (reader.TokenType == JsonToken.StartArray)
		{
			reader.Read();
			while (reader.TokenType != JsonToken.EndArray)
			{
				T item = ((_innerConverter == null) ? serializer.Deserialize<T>(reader) : ((T)_innerConverter.ReadJson(reader, typeof(T), null, serializer)));
				list.Add(item);
				reader.Read();
			}
		}
		return list.ToArray();
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value != null)
		{
			writer.WriteStartArray();
			T[] array = (T[])value;
			for (int i = 0; i < array.Length; i++)
			{
				if (_innerConverter != null)
				{
					_innerConverter.WriteJson(writer, array[i], serializer);
				}
				else
				{
					serializer.Serialize(writer, array[i], typeof(T));
				}
			}
			writer.WriteEndArray();
		}
		else
		{
			writer.WriteNull();
		}
	}
}
