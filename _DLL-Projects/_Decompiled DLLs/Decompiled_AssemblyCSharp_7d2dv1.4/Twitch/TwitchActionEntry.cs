namespace Twitch;

public class TwitchActionEntry
{
	public string UserName;

	public EntityPlayer Target;

	public bool ReadyForRemove;

	public TwitchVoteEntry VoteEntry;

	public TwitchAction Action;

	public bool IsSent;

	public bool ChannelNotify = true;

	public bool IsBitAction;

	public bool IsReRun;

	public bool IsRespawn;

	public int SpecialPointsUsed;

	public int StandardPointsUsed;

	public int BitsUsed;

	public int CreditsUsed;

	public TwitchActionHistoryEntry HistoryEntry;

	public int ActionCost => Action.CurrentCost;

	public TwitchActionHistoryEntry SetupHistoryEntry(ViewerEntry viewerEntry)
	{
		string target = ((Target != null) ? Target.EntityName : "");
		HistoryEntry = new TwitchActionHistoryEntry(UserName, viewerEntry.UserColor, Action, null, null)
		{
			UserID = viewerEntry.UserID,
			PointsSpent = Action.CurrentCost,
			Target = target
		};
		HistoryEntry.ActionEntry = this;
		return HistoryEntry;
	}
}
