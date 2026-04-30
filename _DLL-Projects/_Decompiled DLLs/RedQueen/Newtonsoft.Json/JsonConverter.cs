using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal abstract class JsonConverter
{
	public virtual bool CanRead => true;

	public virtual bool CanWrite => true;

	public abstract void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer);

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public abstract object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer);

	public abstract bool CanConvert(Type objectType);
}
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal abstract class JsonConverter<[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T> : JsonConverter
{
	public sealed override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
		if (!((value != null) ? (value is T) : ReflectionUtils.IsNullable(typeof(T))))
		{
			throw new JsonSerializationException("Converter cannot write specified value to JSON. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
		}
		WriteJson(writer, (T)value, serializer);
	}

	public abstract void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T value, JsonSerializer serializer);

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public sealed override object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer)
	{
		bool flag = existingValue == null;
		if (!flag && !(existingValue is T))
		{
			throw new JsonSerializationException("Converter cannot read JSON with the specified existing value. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
		}
		return ReadJson(reader, objectType, flag ? default(T) : ((T)existingValue), !flag, serializer);
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public abstract T ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] T existingValue, bool hasExistingValue, JsonSerializer serializer);

	public sealed override bool CanConvert(Type objectType)
	{
		return typeof(T).IsAssignableFrom(objectType);
	}
}
