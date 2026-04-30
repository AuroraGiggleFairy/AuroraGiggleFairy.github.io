using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization;

internal delegate void SerializationCallback(object o, StreamingContext context);
