namespace Twitch;

public class TwitchSubEventEntry : TwitchEventEntry
{
	public enum SubTierTypes
	{
		Any,
		Prime,
		Tier1,
		Tier2,
		Tier3
	}

	public SubTierTypes SubTier;

	public TwitchSubEventEntry()
	{
		RewardsBitPot = true;
	}

	public override bool IsValid(int amount = -1, string name = "", SubTierTypes subTier = SubTierTypes.Any)
	{
		if ((StartAmount == -1 || amount >= StartAmount) && (EndAmount == -1 || amount <= EndAmount))
		{
			if (SubTier != SubTierTypes.Any)
			{
				return SubTier == subTier;
			}
			return true;
		}
		return false;
	}

	public override string Description(TwitchEventActionEntry entry)
	{
		return EventTitle;
	}

	public static SubTierTypes GetSubTier(string subPlan)
	{
		return subPlan switch
		{
			"1000" => SubTierTypes.Tier1, 
			"2000" => SubTierTypes.Tier2, 
			"3000" => SubTierTypes.Tier3, 
			_ => SubTierTypes.Prime, 
		};
	}
}
