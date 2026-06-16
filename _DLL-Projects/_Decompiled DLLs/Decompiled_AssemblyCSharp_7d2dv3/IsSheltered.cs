using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsSheltered : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target is EntityPlayerLocal entityPlayerLocal)
		{
			return invert != entityPlayerLocal.shelterPercent > 0f;
		}
		return false;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("{0}sheltered", invert ? "NOT " : ""));
	}
}
