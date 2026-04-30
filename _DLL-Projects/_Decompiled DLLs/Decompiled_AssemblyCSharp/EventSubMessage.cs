using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class EventSubMessage
{
	[JsonProperty("metadata")]
	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public EventSubMetadata Metadata { get; set; } = new EventSubMetadata();

	[JsonProperty("payload")]
	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public JObject Payload { get; set; }
}
