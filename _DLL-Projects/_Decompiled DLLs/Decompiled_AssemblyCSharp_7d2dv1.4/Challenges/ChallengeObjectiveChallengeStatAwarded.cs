using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveChallengeStatAwarded : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string challengeStat = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statText = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.ChallengeStatAwarded;

	public override string DescriptionText => Localization.Get(statText);

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.ChallengeAwardCredit += Current_ChallengeAwardCredit;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.ChallengeAwardCredit -= Current_ChallengeAwardCredit;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ChallengeAwardCredit(string stat, int awardCount)
	{
		if (challengeStat.EqualsCaseInsensitive(stat))
		{
			base.Current += awardCount;
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("challenge_stat"))
		{
			challengeStat = e.GetAttribute("challenge_stat");
		}
		if (e.HasAttribute("stat_text_key"))
		{
			statText = Localization.Get(e.GetAttribute("stat_text_key"));
		}
		else if (e.HasAttribute("stat_text"))
		{
			statText = e.GetAttribute("stat_text");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveChallengeStatAwarded
		{
			challengeStat = challengeStat,
			statText = statText
		};
	}
}
