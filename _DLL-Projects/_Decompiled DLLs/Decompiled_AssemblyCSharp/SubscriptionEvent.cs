using Newtonsoft.Json;

public class SubscriptionEvent : SubscriptionEventBase
{
	[JsonProperty("is_gift")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsGift { get; set; }
}
