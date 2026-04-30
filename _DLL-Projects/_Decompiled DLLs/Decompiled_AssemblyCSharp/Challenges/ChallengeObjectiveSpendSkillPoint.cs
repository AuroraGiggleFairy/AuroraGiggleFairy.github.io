using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveSpendSkillPoint : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string progressionName = "";

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.SpendSkillPoint;

	public override string DescriptionText => string.Format(Localization.Get("ObjectiveSpendSkillPoints_keyword"), Localization.Get("goAnyValue")) + ":";

	public override void HandleOnCreated()
	{
		base.HandleOnCreated();
		CreateRequirements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateRequirements()
	{
		if (ShowRequirements)
		{
			Owner.SetRequirementGroup(new RequirementObjectiveGroupWindowOpen("Skills"));
		}
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.SkillPointSpent += Current_SkillPointSpent;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.SkillPointSpent -= Current_SkillPointSpent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SkillPointSpent(string skillName)
	{
		if (progressionName == "" || progressionName.EqualsCaseInsensitive(skillName))
		{
			base.Current++;
		}
		if (base.Current >= MaxCount)
		{
			base.Current = MaxCount;
			CheckObjectiveComplete();
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("skill_name"))
		{
			progressionName = e.GetAttribute("skill_name");
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveSpendSkillPoint
		{
			progressionName = progressionName
		};
	}
}
