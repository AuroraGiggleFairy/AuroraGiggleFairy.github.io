using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsAttachedToEntity : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!invert)
		{
			return target.AttachedToEntity != null;
		}
		return !(target.AttachedToEntity != null);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0}Attached To Entity", invert ? "NOT " : ""));
	}
}
