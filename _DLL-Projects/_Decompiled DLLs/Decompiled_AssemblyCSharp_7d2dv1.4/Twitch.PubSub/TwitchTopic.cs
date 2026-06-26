namespace Twitch.PubSub;

public class TwitchTopic
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string TopicString { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchTopic()
	{
	}

	public static TwitchTopic ChannelPoints(string channelId)
	{
		return new TwitchTopic
		{
			TopicString = $"channel-points-channel-v1.{channelId}"
		};
	}

	public static TwitchTopic Bits(string channelId)
	{
		return new TwitchTopic
		{
			TopicString = $"channel-bits-events-v2.{channelId}"
		};
	}

	public static TwitchTopic Subscription(string channelId)
	{
		return new TwitchTopic
		{
			TopicString = $"channel-subscribe-events-v1.{channelId}"
		};
	}

	public static TwitchTopic HypeTrain(string channelId)
	{
		return new TwitchTopic
		{
			TopicString = $"hype-train-events-v1.{channelId}"
		};
	}

	public static TwitchTopic CreatorGoal(string channelId)
	{
		return new TwitchTopic
		{
			TopicString = $"creator-goals-events-v1.{channelId}"
		};
	}
}
