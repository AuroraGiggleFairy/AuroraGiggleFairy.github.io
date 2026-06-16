using Challenges;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionTrackChallenge : BaseQuestAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropChallenge = "challenge";

	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		EntityPlayerLocal ownerPlayer = ownerQuest.OwnerJournal.OwnerPlayer;
		XUi xui = ownerPlayer.PlayerUI.xui;
		if (xui.QuestTracker.TrackedQuest == null && xui.QuestTracker.TrackedChallenge == null && xui.Recipes.TrackedRecipe == null && ownerPlayer.challengeJournal.ChallengeDictionary.ContainsKey(ID))
		{
			Challenge challenge = ownerPlayer.challengeJournal.ChallengeDictionary[ID];
			if (challenge.ChallengeGroup.IsActive)
			{
				xui.QuestTracker.TrackedChallenge = challenge;
			}
		}
	}

	public override BaseQuestAction Clone()
	{
		QuestActionTrackChallenge questActionTrackChallenge = new QuestActionTrackChallenge();
		CopyValues(questActionTrackChallenge);
		return questActionTrackChallenge;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropChallenge, ref ID);
	}
}
