using Newtonsoft.Json;

public class SubscriptionEventBase
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

	[JsonProperty("tier")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Tier { get; set; } = "";
}
