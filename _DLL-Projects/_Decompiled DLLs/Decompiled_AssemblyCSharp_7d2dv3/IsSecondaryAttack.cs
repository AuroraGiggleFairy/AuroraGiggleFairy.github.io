using UnityEngine.Scripting;

[Preserve]
public class IsSecondaryAttack : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target == null || target.inventory.holdingItemItemValue.ItemClass.Actions[1] == null)
		{
			return false;
		}
		if (invert)
		{
			return !target.inventory.holdingItemItemValue.ItemClass.Actions[1].IsActionRunning(target.inventory.GetItemActionDataInSlot(target.inventory.holdingItemIdx, 1));
		}
		return target.inventory.holdingItemItemValue.ItemClass.Actions[1].IsActionRunning(target.inventory.GetItemActionDataInSlot(target.inventory.holdingItemIdx, 1));
	}
}
