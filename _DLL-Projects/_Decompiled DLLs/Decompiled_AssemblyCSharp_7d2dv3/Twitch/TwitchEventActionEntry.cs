namespace Twitch;

public class TwitchEventActionEntry
{
	public string UserName;

	public byte Tier;

	public short Count;

	public bool IsSent;

	public bool IsRetry;

	public bool ReadyForRemove;

	public TwitchActionHistoryEntry HistoryEntry;

	public BaseTwitchEventEntry Event;

	public bool HandleEvent(TwitchManager tm)
	{
		if (Event.HandleEvent(UserName, tm))
		{
			IsSent = true;
			return true;
		}
		return false;
	}
}
