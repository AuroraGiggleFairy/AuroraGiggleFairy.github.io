using Newtonsoft.Json;

public class BitsUsedEvent
{
	[JsonProperty("is_anonymous")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsAnonymous { get; set; }

	[JsonProperty("user_id")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string UserId { get; set; } = "";

	[JsonProperty("user_login")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string UserLogin { get; set; } = "";

	[JsonProperty("user_name")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string UserName { get; set; } = "";

	[JsonProperty("broadcaster_user_id")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string BroadcasterUserId { get; set; } = "";

	[JsonProperty("broadcaster_user_login")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string BroadcasterUserLogin { get; set; } = "";

	[JsonProperty("broadcaster_user_name")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string BroadcasterUserName { get; set; } = "";

	[JsonProperty("bits")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Bits { get; set; }
}
