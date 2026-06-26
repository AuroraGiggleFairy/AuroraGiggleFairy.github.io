using Newtonsoft.Json;

namespace Twitch.PubSub;

public class PubSubGoalMessage : BasePubSubMessage
{
	public class GoalData
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public Goal goal { get; set; }
	}

	public class Goal
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public string contributionType { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public string state { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public int currentContributions { get; set; }

		[field: PublicizedFrom(EAccessModifier.Private)]
		public int targetContributions { get; set; }
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public new string type { get; set; }

	public string TheType => type;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GoalData data { get; set; }

	public static PubSubGoalMessage Deserialize(string message)
	{
		return JsonConvert.DeserializeObject<PubSubGoalMessage>(message);
	}
}
