using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementCanTrade : BaseDialogRequirement
{
	public override RequirementTypes RequirementType => RequirementTypes.CanTrade;

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return SandboxOptionManager.GetBool(global::SandboxOptions.SandboxOptions.TradersEnabled);
	}
}
