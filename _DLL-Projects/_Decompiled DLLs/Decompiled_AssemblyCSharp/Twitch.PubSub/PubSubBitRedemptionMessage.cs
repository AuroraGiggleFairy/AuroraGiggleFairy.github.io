using System;
using Newtonsoft.Json;

namespace Twitch.PubSub;

public class PubSubBitRedemptionMessage : BasePubSubMessage
{
	public class BitRedemptionData : EventArgs
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public string user_name { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string channel_name { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string user_id { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string channel_id { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string chat_message { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public int bits_used { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public int total_bits_used { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool is_anonymous { get; set; }
	}

	public BitRedemptionData data;

	public static PubSubBitRedemptionMessage Deserialize(string message)
	{
		return JsonConvert.DeserializeObject<PubSubBitRedemptionMessage>(message);
	}
}
