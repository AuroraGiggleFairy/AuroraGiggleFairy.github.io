using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class UnixDateTimeConverter : DateTimeConverterBase
{
	internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public bool AllowPreEpoch { get; set; }

	public UnixDateTimeConverter()
		: this(allowPreEpoch: false)
	{
	}

	public UnixDateTimeConverter(bool allowPreEpoch)
	{
		AllowPreEpoch = allowPreEpoch;
	}

	public override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
		long num;
		if (value is DateTime dateTime)
		{
			num = (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
		}
		else
		{
			if (!(value is DateTimeOffset dateTimeOffset))
			{
				throw new JsonSerializationException("Expected date object value.");
			}
			num = (long)(dateTimeOffset.ToUniversalTime() - UnixEpoch).TotalSeconds;
		}
		if (!AllowPreEpoch && num < 0)
		{
			throw new JsonSerializationException("Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
		}
		writer.WriteValue(num);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer)
	{
		bool flag = ReflectionUtils.IsNullable(objectType);
		if (reader.TokenType == JsonToken.Null)
		{
			if (!flag)
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		long result;
		if (reader.TokenType == JsonToken.Integer)
		{
			result = (long)reader.Value;
		}
		else
		{
			if (reader.TokenType != JsonToken.String)
			{
				throw JsonSerializationException.Create(reader, "Unexpected token parsing date. Expected Integer or String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			if (!long.TryParse((string)reader.Value, out result))
			{
				throw JsonSerializationException.Create(reader, "Cannot convert invalid value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
		}
		if (AllowPreEpoch || result >= 0)
		{
			DateTime dateTime = UnixEpoch.AddSeconds(result);
			if ((flag ? Nullable.GetUnderlyingType(objectType) : objectType) == typeof(DateTimeOffset))
			{
				return new DateTimeOffset(dateTime, TimeSpan.Zero);
			}
			return dateTime;
		}
		throw JsonSerializationException.Create(reader, "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
	}
}
