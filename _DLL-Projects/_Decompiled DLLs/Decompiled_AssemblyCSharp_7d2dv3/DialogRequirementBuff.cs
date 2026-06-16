using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementBuff : BaseDialogRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override RequirementTypes RequirementType => RequirementTypes.Buff;

	public override void SetupRequirement()
	{
		string arg = Localization.Get("RequirementBuff_keyword");
		base.Description = $"{arg} {BuffManager.GetBuff(base.ID).Name}";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return false;
	}
}
