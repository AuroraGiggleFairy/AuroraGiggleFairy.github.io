using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Quests.Requirements;

[Preserve]
public class RequirementGroup : BaseRequirement
{
	public enum GroupOperator
	{
		AND,
		OR
	}

	public List<BaseRequirement> ChildRequirements = new List<BaseRequirement>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GroupOperator Operator { get; set; }

	public override void SetupRequirement()
	{
		Operator = EnumUtils.Parse<GroupOperator>(base.Value);
		for (int i = 0; i < ChildRequirements.Count; i++)
		{
			ChildRequirements[i].OwnerQuest = base.OwnerQuest;
			ChildRequirements[i].SetupRequirement();
		}
		if (string.IsNullOrEmpty(base.ID))
		{
			if (ChildRequirements.Count > 0)
			{
				base.Description = ChildRequirements[0].Description;
			}
		}
		else
		{
			base.Description = Localization.Get(base.ID);
		}
	}

	public override bool CheckRequirement()
	{
		if (!base.OwnerQuest.Active)
		{
			return true;
		}
		bool result = Operator == GroupOperator.AND;
		for (int i = 0; i < ChildRequirements.Count; i++)
		{
			bool flag = ChildRequirements[i].CheckRequirement();
			if (Operator == GroupOperator.AND)
			{
				if (!flag)
				{
					return false;
				}
			}
			else if (Operator == GroupOperator.OR && flag)
			{
				return true;
			}
		}
		return result;
	}

	public override BaseRequirement Clone()
	{
		RequirementGroup requirementGroup = new RequirementGroup();
		requirementGroup.ID = base.ID;
		requirementGroup.Value = base.Value;
		requirementGroup.Phase = base.Phase;
		for (int i = 0; i < ChildRequirements.Count; i++)
		{
			requirementGroup.ChildRequirements.Add(ChildRequirements[i].Clone());
		}
		return requirementGroup;
	}
}
