using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization;

internal delegate void SerializationErrorCallback(object o, StreamingContext context, ErrorContext errorContext);
