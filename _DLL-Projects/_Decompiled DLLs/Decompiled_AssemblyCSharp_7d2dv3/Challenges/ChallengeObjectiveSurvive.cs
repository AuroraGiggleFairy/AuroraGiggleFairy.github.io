using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveSurvive : BaseChallengeObjective
{
	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Survive;

	public override string DescriptionText => Localization.Get("challengeObjectiveSurvive") + ":";

	public override string StatusText => $"{XUiM_PlayerBuffs.GetTimeString((float)current * 60f)}/{XUiM_PlayerBuffs.GetTimeString((float)MaxCount * 60f)}";

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.TimeSurvive += Current_TimeSurvive;
		base.Current = (int)base.Player.longestLife;
		if (base.Current >= MaxCount)
		{
			base.Current = MaxCount;
			CheckObjectiveComplete();
		}
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.TimeSurvive -= Current_TimeSurvive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_TimeSurvive(float val)
	{
		if (!CheckBaseRequirements())
		{
			base.Current = (int)val;
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveSurvive();
	}
}
