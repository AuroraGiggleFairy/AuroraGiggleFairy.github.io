using UnityEngine.Scripting;

[Preserve]
public class IsInstigator : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		return invert != (target == _params.Instigator);
	}
}
