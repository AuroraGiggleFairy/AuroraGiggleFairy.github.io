using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class JavaScriptDateTimeConverter : DateTimeConverterBase
{
	public override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
		long value2;
		if (value is DateTime dateTime)
		{
			value2 = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(dateTime.ToUniversalTime());
		}
		else
		{
			if (!(value is DateTimeOffset dateTimeOffset))
			{
				throw new JsonSerializationException("Expected date object value.");
			}
			value2 = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(dateTimeOffset.ToUniversalTime().UtcDateTime);
		}
		writer.WriteStartConstructor("Date");
		writer.WriteValue(value2);
		writer.WriteEndConstructor();
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
		if (reader.TokenType != JsonToken.StartConstructor || !string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal))
		{
			throw JsonSerializationException.Create(reader, "Unexpected token or value when parsing date. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType, reader.Value));
		}
		if (!JavaScriptUtils.TryGetDateFromConstructorJson(reader, out var dateTime, out var errorMessage))
		{
			throw JsonSerializationException.Create(reader, errorMessage);
		}
		if ((ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType) == typeof(DateTimeOffset))
		{
			return new DateTimeOffset(dateTime);
		}
		return dateTime;
	}
}
