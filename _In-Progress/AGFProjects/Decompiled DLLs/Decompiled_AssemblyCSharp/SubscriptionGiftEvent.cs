using Newtonsoft.Json;

public class SubscriptionGiftEvent : SubscriptionEventBase
{
	[JsonProperty("is_anonymous")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsAnonymous { get; set; }

	[JsonProperty("total")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Total { get; set; }
}
