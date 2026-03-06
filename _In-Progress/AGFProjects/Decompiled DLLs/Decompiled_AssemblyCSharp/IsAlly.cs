using UnityEngine.Scripting;

[Preserve]
public class IsAlly : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target is EntityPlayer entityPlayer)
		{
			bool flag = entityPlayer.IsFriendOfLocalPlayer && !(target is EntityPlayerLocal);
			if (!invert)
			{
				return flag;
			}
			return !flag;
		}
		return false;
	}
}
