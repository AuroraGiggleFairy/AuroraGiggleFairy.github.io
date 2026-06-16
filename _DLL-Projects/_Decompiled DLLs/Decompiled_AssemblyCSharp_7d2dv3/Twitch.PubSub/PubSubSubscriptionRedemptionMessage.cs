using Newtonsoft.Json;

namespace Twitch.PubSub;

public class PubSubSubscriptionRedemptionMessage : BasePubSubMessage
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int benefit_end_month { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string user_name { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string channel_name { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string user_id { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string channel_id { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string sub_plan { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string sub_plan_name { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int months { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int cumulative_months { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string context { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool is_gift { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int multi_month_duration { get; set; }

	public static PubSubSubscriptionRedemptionMessage Deserialize(string message)
	{
		return JsonConvert.DeserializeObject<PubSubSubscriptionRedemptionMessage>(message);
	}
}
