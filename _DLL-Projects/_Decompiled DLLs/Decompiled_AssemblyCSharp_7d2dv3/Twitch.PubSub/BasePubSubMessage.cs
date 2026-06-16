using System;

namespace Twitch.PubSub;

public class BasePubSubMessage
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string type
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string nonce { get; set; } = Guid.NewGuid().ToString().Replace("-", "");

	public virtual void ReceiveData(string data)
	{
	}
}
