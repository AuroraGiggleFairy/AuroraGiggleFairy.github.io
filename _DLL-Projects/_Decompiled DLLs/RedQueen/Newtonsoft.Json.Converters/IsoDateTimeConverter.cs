using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class IsoDateTimeConverter : DateTimeConverterBase
{
	private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

	private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private string _dateTimeFormat;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	private CultureInfo _culture;

	public DateTimeStyles DateTimeStyles
	{
		get
		{
			return _dateTimeStyles;
		}
		set
		{
			_dateTimeStyles = value;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public string DateTimeFormat
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get
		{
			return _dateTimeFormat ?? string.Empty;
		}
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set
		{
			_dateTimeFormat = (StringUtils.IsNullOrEmpty(value) ? null : value);
		}
	}

	public CultureInfo Culture
	{
		get
		{
			return _culture ?? CultureInfo.CurrentCulture;
		}
		set
		{
			_culture = value;
		}
	}

	public override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
		string value2;
		if (value is DateTime dateTime)
		{
			if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
			{
				dateTime = dateTime.ToUniversalTime();
			}
			value2 = dateTime.ToString(_dateTimeFormat ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK", Culture);
		}
		else
		{
			if (!(value is DateTimeOffset dateTimeOffset))
			{
				throw new JsonSerializationException("Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {0}.".FormatWith(CultureInfo.InvariantCulture, ReflectionUtils.GetObjectType(value)));
			}
			if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
			{
				dateTimeOffset = dateTimeOffset.ToUniversalTime();
			}
			value2 = dateTimeOffset.ToString(_dateTimeFormat ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK", Culture);
		}
		writer.WriteValue(value2);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer)
	{
		bool flag = ReflectionUtils.IsNullableType(objectType);
		if (reader.TokenType == JsonToken.Null)
		{
			if (!flag)
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		Type type = (flag ? Nullable.GetUnderlyingType(objectType) : objectType);
		if (reader.TokenType == JsonToken.Date)
		{
			if (type == typeof(DateTimeOffset))
			{
				if (!(reader.Value is DateTimeOffset))
				{
					return new DateTimeOffset((DateTime)reader.Value);
				}
				return reader.Value;
			}
			if (reader.Value is DateTimeOffset dateTimeOffset)
			{
				return dateTimeOffset.DateTime;
			}
			return reader.Value;
		}
		if (reader.TokenType != JsonToken.String)
		{
			throw JsonSerializationException.Create(reader, "Unexpected token parsing date. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		string text = reader.Value?.ToString();
		if (StringUtils.IsNullOrEmpty(text) && flag)
		{
			return null;
		}
		if (type == typeof(DateTimeOffset))
		{
			if (!StringUtils.IsNullOrEmpty(_dateTimeFormat))
			{
				return DateTimeOffset.ParseExact(text, _dateTimeFormat, Culture, _dateTimeStyles);
			}
			return DateTimeOffset.Parse(text, Culture, _dateTimeStyles);
		}
		if (!StringUtils.IsNullOrEmpty(_dateTimeFormat))
		{
			return DateTime.ParseExact(text, _dateTimeFormat, Culture, _dateTimeStyles);
		}
		return DateTime.Parse(text, Culture, _dateTimeStyles);
	}
}
