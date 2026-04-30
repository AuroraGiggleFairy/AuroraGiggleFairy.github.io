using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementSkill : BaseDialogRequirement
{
	public override RequirementTypes RequirementType => RequirementTypes.Skill;

	public override string GetRequiredDescription(EntityPlayer player)
	{
		ProgressionValue progressionValue = player.Progression.GetProgressionValue(base.ID);
		return $"({Localization.Get(progressionValue.ProgressionClass.NameKey)} {Convert.ToInt32(base.Value)})";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return player.Progression.GetProgressionValue(base.ID).Level > Convert.ToInt32(base.Value);
	}
}
