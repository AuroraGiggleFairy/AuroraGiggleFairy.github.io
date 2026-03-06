using UnityEngine.Scripting;

[Preserve]
public class IsSDCS : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		bool flag = target?.emodel as EModelSDCS != null;
		if (!invert)
		{
			return flag;
		}
		return !flag;
	}
}
