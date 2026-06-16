using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementAdmin : BaseDialogRequirement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override RequirementTypes RequirementType => RequirementTypes.Admin;

	public override void SetupRequirement()
	{
		string description = Localization.Get("RequirementAdmin_keyword");
		base.Description = description;
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
	}
}
