using System;
using Newtonsoft.Json;

namespace Discord.Net.Converters;

internal class NullableConverter<T> : JsonConverter where T : struct
{
	private readonly JsonConverter _innerConverter;

	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public NullableConverter(JsonConverter innerConverter)
	{
		_innerConverter = innerConverter;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value == null)
		{
			return null;
		}
		T val = ((_innerConverter == null) ? serializer.Deserialize<T>(reader) : ((T)_innerConverter.ReadJson(reader, typeof(T), null, serializer)));
		return val;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		T? val = (T?)value;
		if (_innerConverter != null)
		{
			_innerConverter.WriteJson(writer, val.Value, serializer);
		}
		else
		{
			serializer.Serialize(writer, val.Value, typeof(T));
		}
	}
}
