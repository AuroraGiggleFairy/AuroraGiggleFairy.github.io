namespace Twitch;

public class BaseTwitchEventEntry
{
	public enum EventTypes
	{
		Bits,
		Subs,
		GiftSubs,
		Raid,
		Charity,
		ChannelPoints,
		HypeTrain,
		CreatorGoal
	}

	public string EventName = "";

	public string EventTitle = "";

	public bool SafeAllowed = true;

	public bool StartingCooldownAllowed;

	public bool CooldownAllowed = true;

	public bool VoteEventAllowed = true;

	public bool RewardsBitPot;

	public int PPAmount;

	public int SPAmount;

	public int PimpPotAdd;

	public int BitPotAdd;

	public int CooldownAdd;

	public EventTypes EventType;

	public virtual bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
	{
		return false;
	}

	public virtual string Description(TwitchEventActionEntry entry)
	{
		return $"{EventTitle}({entry.Count})";
	}

	public virtual void HandleInstant(string username, TwitchManager tm)
	{
		if (PimpPotAdd > 0)
		{
			tm.AddToPot(PimpPotAdd);
		}
		if (BitPotAdd > 0)
		{
			tm.AddToBitPot(BitPotAdd);
		}
		if (CooldownAdd > 0)
		{
			tm.AddCooldownAmount(CooldownAdd);
		}
		if (PPAmount > 0 || SPAmount > 0)
		{
			if (username == "")
			{
				tm.ViewerData.AddPointsAll(PPAmount, SPAmount);
				return;
			}
			ViewerEntry viewerEntry = tm.ViewerData.GetViewerEntry(username);
			viewerEntry.SpecialPoints += SPAmount;
			viewerEntry.StandardPoints += PPAmount;
		}
	}

	public virtual bool HandleEvent(string username, TwitchManager tm)
	{
		if (EventName == "")
		{
			return true;
		}
		if (!SafeAllowed && tm.IsSafe)
		{
			return false;
		}
		if (TwitchManager.BossHordeActive)
		{
			return false;
		}
		switch (tm.CooldownType)
		{
		case TwitchManager.CooldownTypes.Startup:
			if (!StartingCooldownAllowed)
			{
				return false;
			}
			break;
		default:
			if (!CooldownAllowed)
			{
				return false;
			}
			break;
		case TwitchManager.CooldownTypes.None:
			break;
		}
		if (!VoteEventAllowed && tm.VotingManager.VotingIsActive)
		{
			return false;
		}
		if (GameEventManager.Current.HandleAction(EventName, tm.LocalPlayer, tm.LocalPlayer, twitchActivated: false, username, "event", tm.AllowCrateSharing, allowRefunds: false))
		{
			GameEventManager.Current.HandleGameEventApproved(EventName, tm.LocalPlayer.entityId, username, "event");
			return true;
		}
		return false;
	}
}
