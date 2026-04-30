using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Newtonsoft.Json;

[Serializable]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class JsonSerializationException : JsonException
{
	public int LineNumber { get; }

	public int LinePosition { get; }

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public string Path
	{
		[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
		get;
	}

	public JsonSerializationException()
	{
	}

	public JsonSerializationException(string message)
		: base(message)
	{
	}

	public JsonSerializationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public JsonSerializationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public JsonSerializationException(string message, string path, int lineNumber, int linePosition, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception innerException)
		: base(message, innerException)
	{
		Path = path;
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}

	internal static JsonSerializationException Create(JsonReader reader, string message)
	{
		return Create(reader, message, null);
	}

	internal static JsonSerializationException Create(JsonReader reader, string message, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception ex)
	{
		return Create(reader as IJsonLineInfo, reader.Path, message, ex);
	}

	internal static JsonSerializationException Create([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] IJsonLineInfo lineInfo, string path, string message, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception ex)
	{
		message = JsonPosition.FormatMessage(lineInfo, path, message);
		int lineNumber;
		int linePosition;
		if (lineInfo != null && lineInfo.HasLineInfo())
		{
			lineNumber = lineInfo.LineNumber;
			linePosition = lineInfo.LinePosition;
		}
		else
		{
			lineNumber = 0;
			linePosition = 0;
		}
		return new JsonSerializationException(message, path, lineNumber, linePosition, ex);
	}
}
