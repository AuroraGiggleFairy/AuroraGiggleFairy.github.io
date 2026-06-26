using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementCheckCVar : BaseDialogRequirement
{
	public override RequirementTypes RequirementType => RequirementTypes.CVar;

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		int num = (int)player.GetCVar(base.ID);
		LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
		return num == StringParsers.ParseSInt32(base.Value);
	}
}
