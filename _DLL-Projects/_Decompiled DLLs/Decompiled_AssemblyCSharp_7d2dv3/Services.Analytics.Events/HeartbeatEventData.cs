using System.ComponentModel;
using Newtonsoft.Json;

namespace Services.Analytics.Events;

public class HeartbeatEventData : BaseEventData
{
	public override string EventType => "heartbeat";

	[JsonProperty(PropertyName = "heartbeat_ts")]
	[Description("Timestamp of the heartbeat. Sent every 5 minutes after login")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string HeartbeatTimestamp { get; set; }

	[JsonProperty(PropertyName = "server_id")]
	[Description("Id of the connected server, null if not in one")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ServerId { get; set; }
}
