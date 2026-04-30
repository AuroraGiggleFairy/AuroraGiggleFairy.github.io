namespace Twitch;

public class TwitchCreatorGoalEventEntry : BaseTwitchEventEntry
{
	public string GoalType = "Subs";

	public int RewardAmount = 100;

	public TwitchAction.PointTypes RewardType;

	public override bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
	{
		return name == GoalType;
	}
}
