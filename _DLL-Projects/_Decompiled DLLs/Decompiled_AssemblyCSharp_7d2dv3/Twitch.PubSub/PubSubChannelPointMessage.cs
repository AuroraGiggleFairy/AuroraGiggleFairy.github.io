using System;
using Newtonsoft.Json;

namespace Twitch.PubSub;

public class PubSubChannelPointMessage : BasePubSubMessage
{
	public class ChannelRedemptionData : EventArgs
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public Redemption redemption { get; set; }
	}

	public class Redemption
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public User user { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public Reward reward { get; set; }
	}

	public class User
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public string login { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string display_name { get; set; }
	}

	public class Reward
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public string title { get; set; }
	}

	public ChannelRedemptionData data;

	public static PubSubChannelPointMessage Deserialize(string message)
	{
		return JsonConvert.DeserializeObject<PubSubChannelPointMessage>(message);
	}
}
