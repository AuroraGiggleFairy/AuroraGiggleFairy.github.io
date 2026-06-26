using Newtonsoft.Json;

public class ChannelPointsRedemptionEvent
{
	[JsonProperty("user_id")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string UserId { get; set; } = "";

	[JsonProperty("user_login")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string UserLogin { get; set; } = "";

	[JsonProperty("user_name")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string UserName { get; set; } = "";

	[JsonProperty("reward")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public Reward Reward { get; set; } = new Reward();
}
