using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsCorpse : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (base.IsValid(_params) && target.IsCorpse())
		{
			return !invert;
		}
		return invert;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Entity {0}IsCorpse", invert ? "NOT " : ""));
	}
}
