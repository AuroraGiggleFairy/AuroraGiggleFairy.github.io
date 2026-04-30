using UnityEngine.Scripting;

[Preserve]
public class IsPrimaryAttack : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target == null || target.inventory.holdingItemItemValue.ItemClass.Actions[0] == null)
		{
			return false;
		}
		if (invert)
		{
			return !target.inventory.holdingItemItemValue.ItemClass.Actions[0].IsActionRunning(target.inventory.GetItemActionDataInSlot(target.inventory.holdingItemIdx, 0));
		}
		return target.inventory.holdingItemItemValue.ItemClass.Actions[0].IsActionRunning(target.inventory.GetItemActionDataInSlot(target.inventory.holdingItemIdx, 0));
	}
}
