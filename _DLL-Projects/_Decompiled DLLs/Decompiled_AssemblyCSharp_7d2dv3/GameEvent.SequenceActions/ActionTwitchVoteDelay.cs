using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchVoteDelay : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string delayTimeText;

	public static string PropTime = "time";

	public override ActionCompleteStates OnPerformAction()
	{
		TwitchManager current = TwitchManager.Current;
		if (current.VotingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.WaitingForNextVote)
		{
			float floatValue = GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, delayTimeText, 5f);
			current.VotingManager.VoteStartDelayTimeRemaining += floatValue;
		}
		else
		{
			Debug.LogWarning("Error: VoteDelay set in wrong state. " + current.VotingManager.CurrentVoteState);
		}
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropTime, ref delayTimeText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchVoteDelay
		{
			delayTimeText = delayTimeText
		};
	}
}
