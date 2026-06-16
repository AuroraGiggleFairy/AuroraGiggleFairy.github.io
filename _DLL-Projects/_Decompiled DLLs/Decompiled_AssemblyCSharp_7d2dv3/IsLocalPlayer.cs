using UnityEngine.Scripting;

[Preserve]
public class IsLocalPlayer : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!invert)
		{
			return target as EntityPlayerLocal != null;
		}
		return !(target as EntityPlayerLocal != null);
	}
}
