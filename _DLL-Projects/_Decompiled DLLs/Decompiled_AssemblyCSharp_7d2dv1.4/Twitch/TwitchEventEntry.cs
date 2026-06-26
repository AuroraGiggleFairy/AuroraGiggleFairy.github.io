namespace Twitch;

public class TwitchEventEntry : BaseTwitchEventEntry
{
	public int StartAmount = -1;

	public int EndAmount = -1;

	public override bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
	{
		if (StartAmount == -1 || amount >= StartAmount)
		{
			if (EndAmount != -1)
			{
				return amount <= EndAmount;
			}
			return true;
		}
		return false;
	}
}
