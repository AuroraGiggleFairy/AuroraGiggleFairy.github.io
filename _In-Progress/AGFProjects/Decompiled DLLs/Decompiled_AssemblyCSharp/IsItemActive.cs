using UnityEngine.Scripting;

[Preserve]
public class IsItemActive : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue == null)
		{
			return false;
		}
		if (_params.ItemValue.Activated > 0)
		{
			if (!invert)
			{
				return true;
			}
			return false;
		}
		if (!invert)
		{
			return false;
		}
		return true;
	}
}
