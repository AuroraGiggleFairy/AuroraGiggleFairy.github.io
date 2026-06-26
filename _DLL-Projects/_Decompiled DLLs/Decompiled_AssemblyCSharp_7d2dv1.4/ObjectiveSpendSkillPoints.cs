using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveSpendSkillPoints : BaseObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int pointCount;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Number;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveSpendSkillPoints_keyword");
		pointCount = Convert.ToInt32(Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(keyword, (ID != null) ? ID : "Any");
		StatusText = $"{base.CurrentValue}/{pointCount}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.SkillPointSpent += Current_SkillPointSpent;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.SkillPointSpent -= Current_SkillPointSpent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SkillPointSpent(string skillName)
	{
		if (!base.Complete)
		{
			if ((ID == null || skillName.EqualsCaseInsensitive(ID)) && base.OwnerQuest.CheckRequirements())
			{
				base.CurrentValue++;
			}
			Refresh();
		}
	}

	public override void Refresh()
	{
		if (base.CurrentValue > pointCount)
		{
			base.CurrentValue = (byte)pointCount;
		}
		base.Complete = base.CurrentValue >= pointCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion();
		}
	}

	public override void RemoveObjectives()
	{
	}

	public override BaseObjective Clone()
	{
		ObjectiveSpendSkillPoints objectiveSpendSkillPoints = new ObjectiveSpendSkillPoints();
		CopyValues(objectiveSpendSkillPoints);
		return objectiveSpendSkillPoints;
	}
}
