using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal static class JsonConvert
{
	public static readonly string True = "true";

	public static readonly string False = "false";

	public static readonly string Null = "null";

	public static readonly string Undefined = "undefined";

	public static readonly string PositiveInfinity = "Infinity";

	public static readonly string NegativeInfinity = "-Infinity";

	public static readonly string NaN = "NaN";

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public static Func<JsonSerializerSettings> DefaultSettings
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	public static string ToString(DateTime value)
	{
		return ToString(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind);
	}

	public static string ToString(DateTime value, DateFormatHandling format, DateTimeZoneHandling timeZoneHandling)
	{
		DateTime value2 = DateTimeUtils.EnsureDateTime(value, timeZoneHandling);
		using StringWriter stringWriter = StringUtils.CreateStringWriter(64);
		stringWriter.Write('"');
		DateTimeUtils.WriteDateTimeString(stringWriter, value2, format, null, CultureInfo.InvariantCulture);
		stringWriter.Write('"');
		return stringWriter.ToString();
	}

	public static string ToString(DateTimeOffset value)
	{
		return ToString(value, DateFormatHandling.IsoDateFormat);
	}

	public static string ToString(DateTimeOffset value, DateFormatHandling format)
	{
		using StringWriter stringWriter = StringUtils.CreateStringWriter(64);
		stringWriter.Write('"');
		DateTimeUtils.WriteDateTimeOffsetString(stringWriter, value, format, null, CultureInfo.InvariantCulture);
		stringWriter.Write('"');
		return stringWriter.ToString();
	}

	public static string ToString(bool value)
	{
		if (!value)
		{
			return False;
		}
		return True;
	}

	public static string ToString(char value)
	{
		return ToString(char.ToString(value));
	}

	public static string ToString(Enum value)
	{
		return value.ToString("D");
	}

	public static string ToString(int value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(short value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	[CLSCompliant(false)]
	public static string ToString(ushort value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	[CLSCompliant(false)]
	public static string ToString(uint value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(long value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	private static string ToStringInternal(BigInteger value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	[CLSCompliant(false)]
	public static string ToString(ulong value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(float value)
	{
		return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
	}

	internal static string ToString(float value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
	{
		return EnsureFloatFormat(value, EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
	}

	private static string EnsureFloatFormat(double value, string text, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
	{
		if (floatFormatHandling == FloatFormatHandling.Symbol || (!double.IsInfinity(value) && !double.IsNaN(value)))
		{
			return text;
		}
		if (floatFormatHandling == FloatFormatHandling.DefaultValue)
		{
			if (nullable)
			{
				return Null;
			}
			return "0.0";
		}
		return quoteChar + text + quoteChar;
	}

	public static string ToString(double value)
	{
		return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
	}

	internal static string ToString(double value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
	{
		return EnsureFloatFormat(value, EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
	}

	private static string EnsureDecimalPlace(double value, string text)
	{
		if (double.IsNaN(value) || double.IsInfinity(value) || StringUtils.IndexOf(text, '.') != -1 || StringUtils.IndexOf(text, 'E') != -1 || StringUtils.IndexOf(text, 'e') != -1)
		{
			return text;
		}
		return text + ".0";
	}

	private static string EnsureDecimalPlace(string text)
	{
		if (StringUtils.IndexOf(text, '.') != -1)
		{
			return text;
		}
		return text + ".0";
	}

	public static string ToString(byte value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	[CLSCompliant(false)]
	public static string ToString(sbyte value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	public static string ToString(decimal value)
	{
		return EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
	}

	public static string ToString(Guid value)
	{
		return ToString(value, '"');
	}

	internal static string ToString(Guid value, char quoteChar)
	{
		string text = value.ToString("D", CultureInfo.InvariantCulture);
		string text2 = quoteChar.ToString(CultureInfo.InvariantCulture);
		return text2 + text + text2;
	}

	public static string ToString(TimeSpan value)
	{
		return ToString(value, '"');
	}

	internal static string ToString(TimeSpan value, char quoteChar)
	{
		return ToString(value.ToString(), quoteChar);
	}

	public static string ToString([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Uri value)
	{
		if (value == null)
		{
			return Null;
		}
		return ToString(value, '"');
	}

	internal static string ToString(Uri value, char quoteChar)
	{
		return ToString(value.OriginalString, quoteChar);
	}

	public static string ToString([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string value)
	{
		return ToString(value, '"');
	}

	public static string ToString([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string value, char delimiter)
	{
		return ToString(value, delimiter, StringEscapeHandling.Default);
	}

	public static string ToString([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] string value, char delimiter, StringEscapeHandling stringEscapeHandling)
	{
		if (delimiter != '"' && delimiter != '\'')
		{
			throw new ArgumentException("Delimiter must be a single or double quote.", "delimiter");
		}
		return JavaScriptUtils.ToEscapedJavaScriptString(value, delimiter, appendDelimiters: true, stringEscapeHandling);
	}

	public static string ToString([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		if (value == null)
		{
			return Null;
		}
		return ConvertUtils.GetTypeCode(value.GetType()) switch
		{
			PrimitiveTypeCode.String => ToString((string)value), 
			PrimitiveTypeCode.Char => ToString((char)value), 
			PrimitiveTypeCode.Boolean => ToString((bool)value), 
			PrimitiveTypeCode.SByte => ToString((sbyte)value), 
			PrimitiveTypeCode.Int16 => ToString((short)value), 
			PrimitiveTypeCode.UInt16 => ToString((ushort)value), 
			PrimitiveTypeCode.Int32 => ToString((int)value), 
			PrimitiveTypeCode.Byte => ToString((byte)value), 
			PrimitiveTypeCode.UInt32 => ToString((uint)value), 
			PrimitiveTypeCode.Int64 => ToString((long)value), 
			PrimitiveTypeCode.UInt64 => ToString((ulong)value), 
			PrimitiveTypeCode.Single => ToString((float)value), 
			PrimitiveTypeCode.Double => ToString((double)value), 
			PrimitiveTypeCode.DateTime => ToString((DateTime)value), 
			PrimitiveTypeCode.Decimal => ToString((decimal)value), 
			PrimitiveTypeCode.DBNull => Null, 
			PrimitiveTypeCode.DateTimeOffset => ToString((DateTimeOffset)value), 
			PrimitiveTypeCode.Guid => ToString((Guid)value), 
			PrimitiveTypeCode.Uri => ToString((Uri)value), 
			PrimitiveTypeCode.TimeSpan => ToString((TimeSpan)value), 
			PrimitiveTypeCode.BigInteger => ToStringInternal((BigInteger)value), 
			_ => throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType())), 
		};
	}

	[DebuggerStepThrough]
	public static string SerializeObject([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		return SerializeObject(value, (Type)null, (JsonSerializerSettings)null);
	}

	[DebuggerStepThrough]
	public static string SerializeObject([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, Formatting formatting)
	{
		return SerializeObject(value, formatting, (JsonSerializerSettings)null);
	}

	[DebuggerStepThrough]
	public static string SerializeObject([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, params JsonConverter[] converters)
	{
		JsonSerializerSettings settings = ((converters != null && converters.Length != 0) ? new JsonSerializerSettings
		{
			Converters = converters
		} : null);
		return SerializeObject(value, null, settings);
	}

	[DebuggerStepThrough]
	public static string SerializeObject([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, Formatting formatting, params JsonConverter[] converters)
	{
		JsonSerializerSettings settings = ((converters != null && converters.Length != 0) ? new JsonSerializerSettings
		{
			Converters = converters
		} : null);
		return SerializeObject(value, null, formatting, settings);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public static string SerializeObject(object value, JsonSerializerSettings settings)
	{
		return SerializeObject(value, null, settings);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public static string SerializeObject(object value, Type type, JsonSerializerSettings settings)
	{
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
		return SerializeObjectInternal(value, type, jsonSerializer);
	}

	[DebuggerStepThrough]
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public static string SerializeObject(object value, Formatting formatting, JsonSerializerSettings settings)
	{
		return SerializeObject(value, null, formatting, settings);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	public static string SerializeObject(object value, Type type, Formatting formatting, JsonSerializerSettings settings)
	{
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
		jsonSerializer.Formatting = formatting;
		return SerializeObjectInternal(value, type, jsonSerializer);
	}

	private static string SerializeObjectInternal([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Type type, JsonSerializer jsonSerializer)
	{
		StringWriter stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
		using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
		{
			jsonTextWriter.Formatting = jsonSerializer.Formatting;
			jsonSerializer.Serialize(jsonTextWriter, value, type);
		}
		return stringWriter.ToString();
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static object DeserializeObject(string value)
	{
		return DeserializeObject(value, (Type)null, (JsonSerializerSettings)null);
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static object DeserializeObject(string value, JsonSerializerSettings settings)
	{
		return DeserializeObject(value, null, settings);
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static object DeserializeObject(string value, Type type)
	{
		return DeserializeObject(value, type, (JsonSerializerSettings)null);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	[DebuggerStepThrough]
	public static T DeserializeObject<T>([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value)
	{
		return JsonConvert.DeserializeObject<T>(value, (JsonSerializerSettings)null);
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static T DeserializeAnonymousType<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(string value, T anonymousTypeObject)
	{
		return DeserializeObject<T>(value);
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static T DeserializeAnonymousType<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(string value, T anonymousTypeObject, JsonSerializerSettings settings)
	{
		return DeserializeObject<T>(value, settings);
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static T DeserializeObject<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T>(string value, params JsonConverter[] converters)
	{
		return (T)DeserializeObject(value, typeof(T), converters);
	}

	[DebuggerStepThrough]
	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static T DeserializeObject<T>([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value, JsonSerializerSettings settings)
	{
		return (T)DeserializeObject(value, typeof(T), settings);
	}

	[DebuggerStepThrough]
	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static object DeserializeObject(string value, Type type, params JsonConverter[] converters)
	{
		JsonSerializerSettings settings = ((converters != null && converters.Length != 0) ? new JsonSerializerSettings
		{
			Converters = converters
		} : null);
		return DeserializeObject(value, type, settings);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static object DeserializeObject([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value, Type type, JsonSerializerSettings settings)
	{
		ValidationUtils.ArgumentNotNull(value, "value");
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
		if (!jsonSerializer.IsCheckAdditionalContentSet())
		{
			jsonSerializer.CheckAdditionalContent = true;
		}
		using JsonTextReader reader = new JsonTextReader(new StringReader(value));
		return jsonSerializer.Deserialize(reader, type);
	}

	[DebuggerStepThrough]
	public static void PopulateObject(string value, object target)
	{
		PopulateObject(value, target, null);
	}

	public static void PopulateObject(string value, object target, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonSerializerSettings settings)
	{
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
		using JsonReader jsonReader = new JsonTextReader(new StringReader(value));
		jsonSerializer.Populate(jsonReader, target);
		if (settings == null || !settings.CheckAdditionalContent)
		{
			return;
		}
		while (jsonReader.Read())
		{
			if (jsonReader.TokenType != JsonToken.Comment)
			{
				throw JsonSerializationException.Create(jsonReader, "Additional text found in JSON string after finishing deserializing object.");
			}
		}
	}

	public static string SerializeXmlNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] XmlNode node)
	{
		return SerializeXmlNode(node, Formatting.None);
	}

	public static string SerializeXmlNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] XmlNode node, Formatting formatting)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
		return SerializeObject(node, formatting, xmlNodeConverter);
	}

	public static string SerializeXmlNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] XmlNode node, Formatting formatting, bool omitRootObject)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter
		{
			OmitRootObject = omitRootObject
		};
		return SerializeObject(node, formatting, xmlNodeConverter);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static XmlDocument DeserializeXmlNode(string value)
	{
		return DeserializeXmlNode(value, null);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static XmlDocument DeserializeXmlNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value, string deserializeRootElementName)
	{
		return DeserializeXmlNode(value, deserializeRootElementName, writeArrayAttribute: false);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static XmlDocument DeserializeXmlNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value, string deserializeRootElementName, bool writeArrayAttribute)
	{
		return DeserializeXmlNode(value, deserializeRootElementName, writeArrayAttribute, encodeSpecialCharacters: false);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static XmlDocument DeserializeXmlNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value, string deserializeRootElementName, bool writeArrayAttribute, bool encodeSpecialCharacters)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
		xmlNodeConverter.DeserializeRootElementName = deserializeRootElementName;
		xmlNodeConverter.WriteArrayAttribute = writeArrayAttribute;
		xmlNodeConverter.EncodeSpecialCharacters = encodeSpecialCharacters;
		return (XmlDocument)DeserializeObject(value, typeof(XmlDocument), xmlNodeConverter);
	}

	public static string SerializeXNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] XObject node)
	{
		return SerializeXNode(node, Formatting.None);
	}

	public static string SerializeXNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] XObject node, Formatting formatting)
	{
		return SerializeXNode(node, formatting, omitRootObject: false);
	}

	public static string SerializeXNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] XObject node, Formatting formatting, bool omitRootObject)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter
		{
			OmitRootObject = omitRootObject
		};
		return SerializeObject(node, formatting, xmlNodeConverter);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public static XDocument DeserializeXNode(string value)
	{
		return DeserializeXNode(value, null);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static XDocument DeserializeXNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value, string deserializeRootElementName)
	{
		return DeserializeXNode(value, deserializeRootElementName, writeArrayAttribute: false);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static XDocument DeserializeXNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value, string deserializeRootElementName, bool writeArrayAttribute)
	{
		return DeserializeXNode(value, deserializeRootElementName, writeArrayAttribute, encodeSpecialCharacters: false);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public static XDocument DeserializeXNode([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)] string value, string deserializeRootElementName, bool writeArrayAttribute, bool encodeSpecialCharacters)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
		xmlNodeConverter.DeserializeRootElementName = deserializeRootElementName;
		xmlNodeConverter.WriteArrayAttribute = writeArrayAttribute;
		xmlNodeConverter.EncodeSpecialCharacters = encodeSpecialCharacters;
		return (XDocument)DeserializeObject(value, typeof(XDocument), xmlNodeConverter);
	}
}
