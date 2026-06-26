using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsIndoors : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!invert)
		{
			return target.Stats.AmountEnclosed > 0f;
		}
		return target.Stats.AmountEnclosed <= 0f;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("{0}indoors", invert ? "NOT " : ""));
	}
}
