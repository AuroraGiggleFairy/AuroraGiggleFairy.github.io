namespace Twitch;

public class GiftSubEntry
{
	public float TimeRemaining = 1f;

	public string UserName = "";

	public int UserID = -1;

	public int SubCount;

	public TwitchSubEventEntry.SubTierTypes Tier = TwitchSubEventEntry.SubTierTypes.Tier1;

	public GiftSubEntry(string userName, int userID, TwitchSubEventEntry.SubTierTypes tier, int subCount)
	{
		UserName = userName;
		UserID = userID;
		TimeRemaining = 1f;
		SubCount = subCount;
		Tier = tier;
	}

	public void AddSub()
	{
		SubCount++;
		TimeRemaining = 1f;
	}

	public bool Update(float deltaTime)
	{
		TimeRemaining -= deltaTime;
		return TimeRemaining <= 0f;
	}
}
