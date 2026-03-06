using System;
using Newtonsoft.Json;

namespace Discord.Net.Converters;

internal class UnixTimestampConverter : JsonConverter
{
	public static readonly UnixTimestampConverter Instance = new UnixTimestampConverter();

	private const long MaxSaneMs = 10000000000000L;

	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value is double num && num < 10000000000000.0)
		{
			return new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(num);
		}
		if (reader.Value is long num2 && num2 < 10000000000000L)
		{
			return new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(num2);
		}
		return Optional<DateTimeOffset>.Unspecified;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(((DateTimeOffset)value).ToString("O"));
	}
}
