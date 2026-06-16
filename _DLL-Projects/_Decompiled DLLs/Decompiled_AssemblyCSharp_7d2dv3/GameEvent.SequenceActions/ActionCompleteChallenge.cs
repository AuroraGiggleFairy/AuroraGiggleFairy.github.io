using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionCompleteChallenge : ActionBaseClientAction
{
	public string ChallengeID = "";

	public bool ForceRedeem;

	public static string PropChallengeID = "challenges";

	public static string PropForceRedeem = "force_redeem";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayerLocal entityPlayerLocal)
		{
			string[] array = ChallengeID.ToLower().Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				entityPlayerLocal.challengeJournal.ChallengeDictionary[array[i]]?.CompleteChallenge(ForceRedeem);
			}
			entityPlayerLocal.PlayerUI.xui.QuestTracker.TrackedChallenge = null;
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropChallengeID, ref ChallengeID);
		properties.ParseBool(PropForceRedeem, ref ForceRedeem);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionCompleteChallenge
		{
			ChallengeID = ChallengeID,
			ForceRedeem = ForceRedeem
		};
	}
}
