using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsBloodMoon : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		return SkyManager.IsBloodMoonVisible() ^ invert;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} blood moon", invert ? "NOT " : ""));
	}
}
