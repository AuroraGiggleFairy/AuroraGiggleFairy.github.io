using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class RoundsInMagazine : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue.IsEmpty() || !(_params.ItemValue.ItemClass.Actions[0] is ItemActionRanged))
		{
			return false;
		}
		if (invert)
		{
			return !RequirementBase.compareValues(_params.ItemValue.Meta, operation, value);
		}
		return RequirementBase.compareValues(_params.ItemValue.Meta, operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Rounds in Magazine: {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
