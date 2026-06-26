using System;
using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchVoteList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	public List<TwitchVoteEntry> voteList = new List<TwitchVoteEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TwitchVoteEntry> voteEntries = new List<XUiC_TwitchVoteEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVotingManager votingManager;

	public XUiC_TwitchWindow Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineCount = 1;

	public float GetHeight()
	{
		return 90f;
	}

	public override void Init()
	{
		base.Init();
		XUiC_TwitchVoteEntry[] childrenByType = GetChildrenByType<XUiC_TwitchVoteEntry>();
		for (int i = 0; i < childrenByType.Length; i++)
		{
			if (childrenByType[i] != null)
			{
				voteEntries.Add(childrenByType[i]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void VoteStateChanged()
	{
		if (lineCount == votingManager.NeededLines)
		{
			isDirty = true;
			voteList.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void VoteEndedChanged()
	{
		if (lineCount == votingManager.NeededLines)
		{
			isDirty = true;
			voteList.Clear();
		}
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			Owner.IsDirty = true;
			if (lineCount != votingManager.NeededLines)
			{
				for (int i = 0; i < voteEntries.Count; i++)
				{
					voteEntries[i].Vote = null;
					voteEntries[i].isWinner = false;
				}
			}
			else if (votingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.EventActive || votingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.WaitingForActive)
			{
				SetupWinner();
				votingManager.WinnerShowing = true;
				Owner.IsDirty = true;
			}
			else if (votingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.WaitingForNextVote)
			{
				votingManager.WinnerShowing = false;
				for (int j = 0; j < voteEntries.Count; j++)
				{
					voteEntries[j].Vote = null;
					voteEntries[j].isWinner = false;
				}
			}
			else if (votingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.VoteStarted)
			{
				SetupForVote();
				votingManager.WinnerShowing = false;
			}
			else
			{
				for (int k = 0; k < voteEntries.Count; k++)
				{
					voteEntries[k].Vote = null;
					voteEntries[k].isWinner = false;
				}
			}
			isDirty = false;
		}
		if (votingManager.UIDirty)
		{
			for (int l = 0; l < voteEntries.Count; l++)
			{
				voteEntries[l].isDirty = true;
			}
			votingManager.UIDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupForVote()
	{
		if (voteList.Count == 0)
		{
			SetupCommandList();
		}
		int num = 0;
		for (int i = 0; i < voteList.Count; i++)
		{
			if (num >= voteEntries.Count)
			{
				break;
			}
			if (voteEntries[num] != null)
			{
				voteEntries[num].Vote = voteList[i];
				voteEntries[num].isWinner = false;
				num++;
			}
		}
		for (int j = num; j < voteEntries.Count; j++)
		{
			voteEntries[j].Vote = null;
			voteEntries[j].isWinner = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupWinner()
	{
		voteList.Clear();
		voteList.Add(votingManager.CurrentEvent);
		int num = 0;
		for (int i = 0; i < voteList.Count; i++)
		{
			if (num >= voteEntries.Count)
			{
				break;
			}
			if (voteEntries[num] != null)
			{
				voteEntries[num].Vote = voteList[i];
				voteEntries[num].isWinner = true;
				num++;
			}
		}
		for (int j = num; j < voteEntries.Count; j++)
		{
			voteEntries[j].Vote = null;
			voteEntries[j].isWinner = false;
		}
	}

	public void SetupCommandList()
	{
		voteList.Clear();
		voteList.AddRange(votingManager.voteList);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		votingManager = TwitchManager.Current.VotingManager;
		isDirty = true;
		TwitchVotingManager twitchVotingManager = votingManager;
		twitchVotingManager.VoteStarted = (OnGameEventVoteAction)Delegate.Remove(twitchVotingManager.VoteStarted, new OnGameEventVoteAction(VoteStateChanged));
		TwitchVotingManager twitchVotingManager2 = votingManager;
		twitchVotingManager2.VoteStarted = (OnGameEventVoteAction)Delegate.Combine(twitchVotingManager2.VoteStarted, new OnGameEventVoteAction(VoteStateChanged));
		TwitchVotingManager twitchVotingManager3 = votingManager;
		twitchVotingManager3.VoteEventStarted = (OnGameEventVoteAction)Delegate.Remove(twitchVotingManager3.VoteEventStarted, new OnGameEventVoteAction(VoteStateChanged));
		TwitchVotingManager twitchVotingManager4 = votingManager;
		twitchVotingManager4.VoteEventStarted = (OnGameEventVoteAction)Delegate.Combine(twitchVotingManager4.VoteEventStarted, new OnGameEventVoteAction(VoteStateChanged));
		TwitchVotingManager twitchVotingManager5 = votingManager;
		twitchVotingManager5.VoteEventEnded = (OnGameEventVoteAction)Delegate.Remove(twitchVotingManager5.VoteEventEnded, new OnGameEventVoteAction(VoteEndedChanged));
		TwitchVotingManager twitchVotingManager6 = votingManager;
		twitchVotingManager6.VoteEventEnded = (OnGameEventVoteAction)Delegate.Combine(twitchVotingManager6.VoteEventEnded, new OnGameEventVoteAction(VoteEndedChanged));
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (name == "line_count")
		{
			lineCount = StringParsers.ParseSInt16(value);
			return true;
		}
		return base.ParseAttribute(name, value, _parent);
	}
}
