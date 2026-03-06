using UnityEngine.Scripting;

[Preserve]
public class HoldingItemBroken : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target == null)
		{
			return false;
		}
		bool flag = target.inventory.holdingItemItemValue.PercentUsesLeft <= 0f;
		if (!invert)
		{
			return flag;
		}
		return !flag;
	}
}
