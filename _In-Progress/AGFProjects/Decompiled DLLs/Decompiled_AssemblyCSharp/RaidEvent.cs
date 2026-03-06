using Newtonsoft.Json;

public class RaidEvent
{
	[JsonProperty("from_broadcaster_user_id")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string RaiderID { get; set; }

	[JsonProperty("from_broadcaster_user_login")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string RaiderUserName { get; set; }

	[JsonProperty("viewers")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int viewerCount { get; set; }
}
