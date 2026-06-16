using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveBloodmoon : BaseChallengeObjective
{
	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Bloodmoon;

	public override string DescriptionText => Localization.Get("challengeObjectiveBloodMoonCompleted") + ":";

	public override string StatusText
	{
		get
		{
			if (!EntityFactory.EnemySpawnMode)
			{
				return "--";
			}
			return base.StatusText;
		}
	}

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		if (!EntityFactory.EnemySpawnMode)
		{
			base.Current = MaxCount;
			base.Complete = true;
			Owner.HandleComplete(showTooltip: false);
			if (Owner.ChallengeState == Challenge.ChallengeStates.Completed)
			{
				Owner.AutoCompleted = true;
				Owner.ChallengeState = Challenge.ChallengeStates.Redeemed;
			}
		}
		else
		{
			QuestEventManager.Current.BloodMoonSurvive += Current_BloodMoonSurvive;
		}
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
