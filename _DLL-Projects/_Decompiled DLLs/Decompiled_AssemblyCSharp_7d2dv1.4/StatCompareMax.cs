using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatCompareMax : StatCompareCurrent
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!ParamsValid(_params))
		{
			return false;
		}
		switch (stat)
		{
		case StatTypes.Health:
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stats.Health.Max, operation, value);
			}
			return !RequirementBase.compareValues(target.Stats.Health.Max, operation, value);
		case StatTypes.Stamina:
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stats.Stamina.Max, operation, value);
			}
			return !RequirementBase.compareValues(target.Stats.Stamina.Max, operation, value);
		case StatTypes.Water:
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stats.Water.Max, operation, value);
			}
			return !RequirementBase.compareValues(target.Stats.Water.Max, operation, value);
		case StatTypes.Food:
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stats.Food.Max, operation, value);
			}
			return !RequirementBase.compareValues(target.Stats.Food.Max, operation, value);
		default:
			return false;
		}
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}'% {0}{2} {3}", invert ? "NOT " : "", stat.ToStringCached(), operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
