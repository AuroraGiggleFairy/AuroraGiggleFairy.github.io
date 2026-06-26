using System.Collections.Generic;

public class NPCIsAlert : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target.IsAlive())
		{
			if (invert)
			{
				return !target.IsAlert;
			}
			return target.IsAlert;
		}
		return false;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Entity {0}Alert", invert ? "NOT " : ""));
	}
}
