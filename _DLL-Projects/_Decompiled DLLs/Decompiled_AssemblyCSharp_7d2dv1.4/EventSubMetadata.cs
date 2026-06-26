using System;
using Newtonsoft.Json;

[Serializable]
public class EventSubMetadata
{
	[JsonProperty("message_id")]
	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string MessageId { get; set; } = string.Empty;

	[JsonProperty("message_type")]
	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string MessageType { get; set; } = string.Empty;

	[JsonProperty("message_timestamp")]
	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string MessageTimestamp { get; set; } = string.Empty;
}
