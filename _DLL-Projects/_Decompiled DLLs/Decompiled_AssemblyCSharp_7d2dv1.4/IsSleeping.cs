using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsSleeping : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		EntityEnemy entityEnemy = target as EntityEnemy;
		if (entityEnemy == null)
		{
			return false;
		}
		if (!invert)
		{
			return entityEnemy.IsSleeping;
		}
		return !entityEnemy.IsSleeping;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("{0}sleeping", invert ? "NOT " : ""));
	}
}
