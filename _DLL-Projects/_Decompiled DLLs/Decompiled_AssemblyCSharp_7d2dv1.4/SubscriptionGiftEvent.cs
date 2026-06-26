using Newtonsoft.Json;

public class SubscriptionGiftEvent : SubscriptionEventBase
{
	[JsonProperty("is_anonymous")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsAnonymous { get; set; }
}
