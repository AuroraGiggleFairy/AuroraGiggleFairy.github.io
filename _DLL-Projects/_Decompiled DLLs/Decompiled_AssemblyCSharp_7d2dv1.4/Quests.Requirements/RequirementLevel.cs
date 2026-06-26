using System;
using UnityEngine.Scripting;

namespace Quests.Requirements;

[Preserve]
public class RequirementLevel : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int expectedLevel;

	public override void SetupRequirement()
	{
		string arg = Localization.Get("RequirementLevel_keyword");
		expectedLevel = Convert.ToInt32(base.Value);
		base.Description = $"{arg} {expectedLevel}";
	}

	public override bool CheckRequirement()
	{
		if (!base.OwnerQuest.Active)
		{
			return true;
		}
		return XUiM_Player.GetLevel(base.OwnerQuest.OwnerJournal.OwnerPlayer) >= expectedLevel;
	}

	public override BaseRequirement Clone()
	{
		return new RequirementLevel
		{
			ID = base.ID,
			Value = base.Value,
			Phase = base.Phase
		};
	}
}
