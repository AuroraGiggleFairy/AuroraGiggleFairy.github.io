using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsAlive : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target != null)
		{
			if (invert)
			{
				return !target.IsAlive();
			}
			return target.IsAlive();
		}
		return false;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Entity {0}Alive", invert ? "NOT " : ""));
	}
}
