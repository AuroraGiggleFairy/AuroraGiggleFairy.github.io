using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Newtonsoft.Json;

[Serializable]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class JsonException : Exception
{
	public JsonException()
	{
	}

	public JsonException(string message)
		: base(message)
	{
	}

	public JsonException(string message, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] Exception innerException)
		: base(message, innerException)
	{
	}

	public JsonException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal static JsonException Create(IJsonLineInfo lineInfo, string path, string message)
	{
		message = JsonPosition.FormatMessage(lineInfo, path, message);
		return new JsonException(message);
	}
}
