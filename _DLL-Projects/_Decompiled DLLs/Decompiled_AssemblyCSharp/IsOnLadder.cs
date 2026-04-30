using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsOnLadder : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!invert)
		{
			return target.IsInElevator();
		}
		return !target.IsInElevator();
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} On Ladder", invert ? "NOT " : ""));
	}
}
