using UnityEngine.Scripting;

[Preserve]
public class CatapultStrainAmount : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		ItemValue itemValue = _params.ItemValue;
		if (itemValue == null)
		{
			return false;
		}
		if (itemValue.ItemClass.Actions[0] is ItemActionCatapult itemActionCatapult)
		{
			float strainPercent = itemActionCatapult.GetStrainPercent(_params.ItemInventoryData.actionData[0]);
			if (invert)
			{
				return !RequirementBase.compareValues(strainPercent, operation, value);
			}
			return RequirementBase.compareValues(strainPercent, operation, value);
		}
		if (_params.ItemActionData is ItemActionLauncher.ItemActionDataLauncher { lastAttackStrainPercent: var lastAttackStrainPercent })
		{
			if (invert)
			{
				return !RequirementBase.compareValues(lastAttackStrainPercent, operation, value);
			}
			return RequirementBase.compareValues(lastAttackStrainPercent, operation, value);
		}
		return false;
	}
}
