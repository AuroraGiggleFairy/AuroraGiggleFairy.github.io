namespace Twitch;

public class TwitchActionHistoryEntry
{
	public enum EntryStates
	{
		Waiting,
		Completed,
		Reimbursed,
		Despawned,
		Refunded
	}

	public string UserName;

	public string UserColor;

	public string Target;

	public int UserID;

	public TwitchAction Action;

	public TwitchVote Vote;

	public TwitchActionEntry ActionEntry;

	public TwitchEventActionEntry EventEntry;

	public int PointsSpent;

	public bool HasRetried;

	public string ActionTime;

	public EntryStates EntryState;

	public string Command
	{
		get
		{
			if (Action != null)
			{
				return $"{Action.Command}({PointsSpent})";
			}
			if (Vote != null)
			{
				return Vote.VoteDescription;
			}
			if (EventEntry != null)
			{
				return EventEntry.Event.Description(EventEntry);
			}
			return "";
		}
	}

	public string Title
	{
		get
		{
			if (Action != null)
			{
				return Action.Title;
			}
			if (Vote != null)
			{
				return Vote.VoteDescription;
			}
			if (EventEntry != null)
			{
				return EventEntry.Event.EventTitle;
			}
			return "";
		}
	}

	public string Description
	{
		get
		{
			if (Action != null)
			{
				return Action.Description;
			}
			if (Vote != null)
			{
				return Vote.Description;
			}
			if (EventEntry != null)
			{
				return EventEntry.Event.EventTitle;
			}
			return "";
		}
	}

	public string HistoryType
	{
		get
		{
			if (Action != null)
			{
				return "action";
			}
			if (Vote != null)
			{
				return "vote";
			}
			if (EventEntry != null)
			{
				return "event";
			}
			return "";
		}
	}

	public bool IsRefunded => EntryState == EntryStates.Refunded;

	public TwitchActionHistoryEntry(string username, string usercolor, TwitchAction action, TwitchVote vote, TwitchEventActionEntry eventEntry)
	{
		UserName = username;
		Action = action;
		Vote = vote;
		EventEntry = eventEntry;
		UserColor = usercolor;
		(int Days, int Hours, int Minutes) tuple = GameUtils.WorldTimeToElements(GameManager.Instance.World.worldTime);
		int item = tuple.Days;
		int item2 = tuple.Hours;
		int item3 = tuple.Minutes;
		ActionTime = string.Format("{0} {1}, {2:00}:{3:00}", Localization.Get("xuiDay"), item, item2, item3);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool IsValid()
	{
		if (UserName != null)
		{
			if ((Action == null || Action.Command == null) && Vote == null)
			{
				return EventEntry != null;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void Refund()
	{
		if (EntryState != EntryStates.Refunded)
		{
			TwitchManager current = TwitchManager.Current;
			current.ViewerData.ReimburseAction(UserName, PointsSpent, Action);
			current.LocalPlayer.PlayOneShot("ui_vending_purchase");
			EntryState = EntryStates.Refunded;
		}
	}

	public void Retry()
	{
		if (!HasRetried)
		{
			if (Action != null)
			{
				TwitchManager.Current.HandleExtensionMessage(UserID, $"{Action.Command} {Target}", isRerun: true, 0, 0);
			}
			else if (EventEntry != null)
			{
				EventEntry.IsSent = false;
				EventEntry.IsRetry = true;
				TwitchManager.Current.EventQueue.Add(EventEntry);
			}
			HasRetried = true;
		}
	}

	public bool CanRetry()
	{
		if (HasRetried)
		{
			return false;
		}
		TwitchManager.CooldownTypes cooldownType = TwitchManager.Current.CooldownType;
		if (Action != null)
		{
			if (TwitchManager.Current.VotingManager.VotingIsActive)
			{
				return false;
			}
			if (!Action.IgnoreCooldown)
			{
				switch (cooldownType)
				{
				case TwitchManager.CooldownTypes.Time:
				case TwitchManager.CooldownTypes.BloodMoonDisabled:
				case TwitchManager.CooldownTypes.QuestDisabled:
					return false;
				case TwitchManager.CooldownTypes.MaxReachedWaiting:
				case TwitchManager.CooldownTypes.SafeCooldown:
					if (Action.WaitingBlocked)
					{
						return false;
					}
					break;
				}
				return !HasRetried;
			}
			return !HasRetried;
		}
		if (EventEntry != null)
		{
			if (!EventEntry.Event.CooldownAllowed)
			{
				switch (cooldownType)
				{
				case TwitchManager.CooldownTypes.Time:
				case TwitchManager.CooldownTypes.BloodMoonDisabled:
				case TwitchManager.CooldownTypes.QuestDisabled:
					return false;
				case TwitchManager.CooldownTypes.MaxReachedWaiting:
					return false;
				}
			}
			if (!EventEntry.Event.StartingCooldownAllowed && cooldownType == TwitchManager.CooldownTypes.Startup)
			{
				return false;
			}
			if (!EventEntry.Event.VoteEventAllowed && TwitchManager.Current.VotingManager.VotingIsActive)
			{
				return false;
			}
			return !HasRetried;
		}
		return false;
	}

	public bool CanRefund()
	{
		if (PointsSpent > 0 && EntryState != EntryStates.Refunded && EntryState != EntryStates.Reimbursed)
		{
			return Action != null;
		}
		return false;
	}
}
