using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Newtonsoft.Json;

[Serializable]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class JsonWriterException : JsonException
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public string Path
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get;
	}

	public JsonWriterException()
	{
	}

	public JsonWriterException(string message)
		: base(message)
	{
	}

	public JsonWriterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public JsonWriterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public JsonWriterException(string message, string path, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception innerException)
		: base(message, innerException)
	{
		Path = path;
	}

	internal static JsonWriterException Create(JsonWriter writer, string message, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception ex)
	{
		return Create(writer.ContainerPath, message, ex);
	}

	internal static JsonWriterException Create(string path, string message, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception ex)
	{
		message = JsonPosition.FormatMessage(null, path, message);
		return new JsonWriterException(message, path, ex);
	}
}
