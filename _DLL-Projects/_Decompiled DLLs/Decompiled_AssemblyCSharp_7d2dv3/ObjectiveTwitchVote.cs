using System;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveTwitchVote : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum TwitchVoteStates
	{
		Start,
		Waiting,
		Complete
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string voteType = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeRemaining = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropVoteType = "vote_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public TwitchVoteStates GameEventState;

	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override bool ShowInQuestLog => false;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveAssemble_keyword");
	}

	public override void SetupDisplay()
	{
		base.Description = "";
		StatusText = "";
	}

	public override void Update(float updateTime)
	{
		TwitchManager current = TwitchManager.Current;
		switch (GameEventState)
		{
		case TwitchVoteStates.Start:
		{
			if (voteType == "" || !current.IsReady || !current.VotingManager.VotingEnabled)
			{
				base.CurrentValue = 1;
				Refresh();
				break;
			}
			TwitchVotingManager votingManager = current.VotingManager;
			votingManager.VoteStarted = (OnGameEventVoteAction)Delegate.Combine(votingManager.VoteStarted, new OnGameEventVoteAction(VoteStarted));
			current.VotingManager.QueueVote(voteType);
			GameEventState = TwitchVoteStates.Waiting;
			break;
		}
		case TwitchVoteStates.Complete:
			base.CurrentValue = 1;
			Refresh();
			break;
		case TwitchVoteStates.Waiting:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void VoteStarted()
	{
		if (TwitchManager.Current.VotingManager.CurrentVoteType.Name == voteType)
		{
			GameEventState = TwitchVoteStates.Complete;
		}
	}

	public override void Refresh()
	{
		if (!base.Complete)
		{
			base.Complete = base.CurrentValue == 1;
			if (base.Complete)
			{
				base.OwnerQuest.RefreshQuestCompletion();
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveTwitchVote objectiveTwitchVote = new ObjectiveTwitchVote();
		CopyValues(objectiveTwitchVote);
		objectiveTwitchVote.voteType = voteType;
		return objectiveTwitchVote;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropVoteType, ref voteType);
	}
}
