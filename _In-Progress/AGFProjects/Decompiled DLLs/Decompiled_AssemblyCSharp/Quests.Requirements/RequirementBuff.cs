using UnityEngine.Scripting;

namespace Quests.Requirements;

[Preserve]
public class RequirementBuff : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override void SetupRequirement()
	{
		string arg = Localization.Get("RequirementBuff_keyword");
		base.Description = $"{arg} {BuffManager.GetBuff(base.ID).Name}";
	}

	public override bool CheckRequirement()
	{
		if (!base.OwnerQuest.Active)
		{
			return true;
		}
		if (base.OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.HasBuff(base.ID))
		{
			return true;
		}
		return false;
	}

	public override BaseRequirement Clone()
	{
		return new RequirementBuff
		{
			ID = base.ID,
			Value = base.Value,
			Phase = base.Phase
		};
	}
}
