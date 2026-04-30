using Newtonsoft.Json;

public class SubscriptionMessageEvent : SubscriptionEventBase
{
	[JsonProperty("cumulative_months")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int CumulativeMonths { get; set; }

	[JsonProperty("streak_months")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string StreakMonths { get; set; }

	[JsonProperty("duration_months")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string DurationMonths { get; set; } = "";
}
