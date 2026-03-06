using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionTwitchStartVote : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string voteType = "";

	public static string PropVoteType = "vote_type";

	public override ActionCompleteStates OnPerformAction()
	{
		if (voteType == "")
		{
			return ActionCompleteStates.InCompleteRefund;
		}
		TwitchManager current = TwitchManager.Current;
		if (!current.TwitchActive || !current.VotingManager.VotingEnabled)
		{
			return ActionCompleteStates.InCompleteRefund;
		}
		current.VotingManager.QueueVote(voteType);
		return ActionCompleteStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropVoteType, ref voteType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionTwitchStartVote
		{
			voteType = voteType
		};
	}
}
