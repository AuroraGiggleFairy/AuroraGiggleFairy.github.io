using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class TargetRange : TargetedCompareRequirementBase
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
		if (_params.Self != null && _params.Other != null)
		{
			if (_params.Self != _params.Other)
			{
				if (invert)
				{
					return !RequirementBase.compareValues(_params.Self.GetDistance(_params.Other), operation, value);
				}
				return RequirementBase.compareValues(_params.Self.GetDistance(_params.Other), operation, value);
			}
			return false;
		}
		return false;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("TargetRange: {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
