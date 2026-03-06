using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class StringEnumConverter : JsonConverter
{
	[Obsolete("StringEnumConverter.CamelCaseText is obsolete. Set StringEnumConverter.NamingStrategy with CamelCaseNamingStrategy instead.")]
	public bool CamelCaseText
	{
		get
		{
			if (!(NamingStrategy is CamelCaseNamingStrategy))
			{
				return false;
			}
			return true;
		}
		set
		{
			if (value)
			{
				if (!(NamingStrategy is CamelCaseNamingStrategy))
				{
					NamingStrategy = new CamelCaseNamingStrategy();
				}
			}
			else if (NamingStrategy is CamelCaseNamingStrategy)
			{
				NamingStrategy = null;
			}
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public NamingStrategy NamingStrategy
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get;
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		set;
	}

	public bool AllowIntegerValues { get; set; } = true;

	public StringEnumConverter()
	{
	}

	[Obsolete("StringEnumConverter(bool) is obsolete. Create a converter with StringEnumConverter(NamingStrategy, bool) instead.")]
	public StringEnumConverter(bool camelCaseText)
	{
		if (camelCaseText)
		{
			NamingStrategy = new CamelCaseNamingStrategy();
		}
	}

	public StringEnumConverter(NamingStrategy namingStrategy, bool allowIntegerValues = true)
	{
		NamingStrategy = namingStrategy;
		AllowIntegerValues = allowIntegerValues;
	}

	public StringEnumConverter(Type namingStrategyType)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, null);
	}

	public StringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, namingStrategyParameters);
	}

	public StringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters, bool allowIntegerValues)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, namingStrategyParameters);
		AllowIntegerValues = allowIntegerValues;
	}

	public override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Enum obj = (Enum)value;
		if (!Newtonsoft.Json.Utilities.EnumUtils.TryToString(obj.GetType(), value, NamingStrategy, out var name))
		{
			if (!AllowIntegerValues)
			{
				throw JsonSerializationException.Create(null, writer.ContainerPath, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, obj.ToString("D")), null);
			}
			writer.WriteValue(value);
		}
		else
		{
			writer.WriteValue(name);
		}
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			if (!ReflectionUtils.IsNullableType(objectType))
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		bool flag = ReflectionUtils.IsNullableType(objectType);
		Type type = (flag ? Nullable.GetUnderlyingType(objectType) : objectType);
		try
		{
			if (reader.TokenType == JsonToken.String)
			{
				string value = reader.Value?.ToString();
				if (StringUtils.IsNullOrEmpty(value) && flag)
				{
					return null;
				}
				return Newtonsoft.Json.Utilities.EnumUtils.ParseEnum(type, NamingStrategy, value, !AllowIntegerValues);
			}
			if (reader.TokenType == JsonToken.Integer)
			{
				if (!AllowIntegerValues)
				{
					throw JsonSerializationException.Create(reader, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, reader.Value));
				}
				return ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, type);
			}
		}
		catch (Exception ex)
		{
			throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(reader.Value), objectType), ex);
		}
		throw JsonSerializationException.Create(reader, "Unexpected token {0} when parsing enum.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
	}

	public override bool CanConvert(Type objectType)
	{
		return (ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType).IsEnum();
	}
}
