using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class PlayerLevel : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target.Progression != null)
		{
			int level = target.Progression.GetLevel();
			if (invert)
			{
				return !RequirementBase.compareValues(level, operation, value);
			}
			return RequirementBase.compareValues(level, operation, value);
		}
		return false;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Player level {0} {1} {2}", invert ? "NOT" : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
