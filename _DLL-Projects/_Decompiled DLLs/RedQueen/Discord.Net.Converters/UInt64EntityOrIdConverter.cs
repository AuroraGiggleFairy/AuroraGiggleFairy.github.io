using System;
using System.Globalization;
using Discord.API;
using Newtonsoft.Json;

namespace Discord.Net.Converters;

internal class UInt64EntityOrIdConverter<T> : JsonConverter
{
	private readonly JsonConverter _innerConverter;

	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public UInt64EntityOrIdConverter(JsonConverter innerConverter)
	{
		_innerConverter = innerConverter;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JsonToken tokenType = reader.TokenType;
		if (tokenType == JsonToken.Integer || tokenType == JsonToken.String)
		{
			return new EntityOrId<T>(ulong.Parse(reader.ReadAsString(), NumberStyles.None, CultureInfo.InvariantCulture));
		}
		T obj = ((_innerConverter == null) ? serializer.Deserialize<T>(reader) : ((T)_innerConverter.ReadJson(reader, typeof(T), null, serializer)));
		return new EntityOrId<T>(obj);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new InvalidOperationException();
	}
}
