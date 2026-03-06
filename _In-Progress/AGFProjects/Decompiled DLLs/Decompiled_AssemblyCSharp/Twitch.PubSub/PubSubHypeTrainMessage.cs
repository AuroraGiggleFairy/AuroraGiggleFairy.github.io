using System;
using UnityEngine;

namespace Twitch.PubSub;

public class PubSubHypeTrainMessage : BasePubSubMessage
{
	public class HypeTrainData : EventArgs
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public string user_name { get; set; }
	}

	public HypeTrainData data;

	public static PubSubHypeTrainMessage Deserialize(string message)
	{
		Debug.LogWarning("HypeTrainMessage:\n" + message);
		return new PubSubHypeTrainMessage();
	}
}
