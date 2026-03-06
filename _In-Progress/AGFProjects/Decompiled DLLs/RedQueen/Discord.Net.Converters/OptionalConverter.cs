using System;
using Newtonsoft.Json;

namespace Discord.Net.Converters;

internal class OptionalConverter<T> : JsonConverter
{
	private readonly JsonConverter _innerConverter;

	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public OptionalConverter(JsonConverter innerConverter)
	{
		_innerConverter = innerConverter;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		T value;
		if (_innerConverter != null)
		{
			object obj = _innerConverter.ReadJson(reader, typeof(T), null, serializer);
			if (obj is Optional<T>)
			{
				return obj;
			}
			value = (T)obj;
		}
		else
		{
			value = serializer.Deserialize<T>(reader);
		}
		return new Optional<T>(value);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		value = ((Optional<T>)value).Value;
		if (_innerConverter != null)
		{
			_innerConverter.WriteJson(writer, value, serializer);
		}
		else
		{
			serializer.Serialize(writer, value, typeof(T));
		}
	}
}
