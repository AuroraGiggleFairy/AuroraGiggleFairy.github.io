using System.Collections.Generic;

public class BurstRoundCount : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue.IsEmpty())
		{
			return false;
		}
		if (!(_params.ItemValue.ItemClass.Actions[0] is ItemActionRanged itemActionRanged))
		{
			return false;
		}
		if (invert)
		{
			return !RequirementBase.compareValues(itemActionRanged.GetBurstCount(target.inventory.holdingItemData.actionData[0]), operation, value);
		}
		return RequirementBase.compareValues(itemActionRanged.GetBurstCount(target.inventory.holdingItemData.actionData[0]), operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Rounds in Magazine: {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
