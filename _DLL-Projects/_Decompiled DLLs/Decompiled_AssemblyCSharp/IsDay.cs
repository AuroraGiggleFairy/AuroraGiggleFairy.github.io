using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsDay : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!invert)
		{
			return GameManager.Instance.World.IsDaytime();
		}
		return !GameManager.Instance.World.IsDaytime();
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} night time", invert ? "NOT " : ""));
	}
}
