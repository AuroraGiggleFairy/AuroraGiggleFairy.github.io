using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class VersionConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		if (value is Version)
		{
			writer.WriteValue(value.ToString());
			return;
		}
		throw new JsonSerializationException("Expected Version object value");
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		if (reader.TokenType == JsonToken.String)
		{
			try
			{
				return new Version((string)reader.Value);
			}
			catch (Exception ex)
			{
				throw JsonSerializationException.Create(reader, "Error parsing version string: {0}".FormatWith(CultureInfo.InvariantCulture, reader.Value), ex);
			}
		}
		throw JsonSerializationException.Create(reader, "Unexpected token or value when parsing version. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType, reader.Value));
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Version);
	}
}
