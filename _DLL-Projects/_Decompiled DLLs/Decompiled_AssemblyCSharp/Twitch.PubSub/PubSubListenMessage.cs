namespace Twitch.PubSub;

public class PubSubListenMessage : BasePubSubMessage
{
	public class PubSubListenData
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public string[] topics { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string auth_token { get; set; }
	}

	public PubSubListenData data;

	public PubSubListenMessage()
	{
		base.type = "LISTEN";
	}
}
