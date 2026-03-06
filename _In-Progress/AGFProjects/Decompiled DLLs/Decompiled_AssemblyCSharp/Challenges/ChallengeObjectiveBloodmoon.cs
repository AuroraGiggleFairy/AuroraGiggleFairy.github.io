using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveBloodmoon : BaseChallengeObjective
{
	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Bloodmoon;

	public override string DescriptionText => Localization.Get("challengeObjectiveBloodMoonCompleted") + ":";

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.BloodMoonSurvive += Current_BloodMoonSurvive;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.BloodMoonSurvive -= Current_BloodMoonSurvive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BloodMoonSurvive()
	{
		if (!CheckBaseRequirements())
		{
			base.Current++;
			CheckObjectiveComplete();
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveBloodmoon();
	}
}
