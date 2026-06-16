using UnityEngine.Scripting;

[Preserve]
public class IsFPV : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target as EntityPlayerLocal != null)
		{
			if (!invert)
			{
				return (target as EntityPlayerLocal).bFirstPersonView;
			}
			return !(target as EntityPlayerLocal).bFirstPersonView;
		}
		if (!invert)
		{
			return false;
		}
		return true;
	}
}
