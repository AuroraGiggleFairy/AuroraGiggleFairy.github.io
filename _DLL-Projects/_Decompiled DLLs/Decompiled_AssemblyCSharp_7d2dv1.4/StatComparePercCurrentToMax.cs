using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatComparePercCurrentToMax : StatCompareCurrent
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
		{
			float max2 = target.Stats.Health.Max;
			if (max2 <= 0f)
			{
				return false;
			}
			if (!invert)
			{
				return RequirementBase.compareValues((float)target.Health / max2, operation, value);
			}
			return !RequirementBase.compareValues((float)target.Health / max2, operation, value);
		}
		case StatTypes.Stamina:
		{
			float max4 = target.Stats.Stamina.Max;
			if (max4 <= 0f)
			{
				return false;
			}
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stamina / max4, operation, value);
			}
			return !RequirementBase.compareValues(target.Stamina / max4, operation, value);
		}
		case StatTypes.Water:
		{
			float max3 = target.Stats.Water.Max;
			if (max3 <= 0f)
			{
				return false;
			}
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stats.Water.Value / max3, operation, value);
			}
			return !RequirementBase.compareValues(target.Stats.Water.Value / max3, operation, value);
		}
		case StatTypes.Food:
		{
			float max = target.Stats.Food.Max;
			if (max <= 0f)
			{
				return false;
			}
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stats.Food.Value / max, operation, value);
			}
			return !RequirementBase.compareValues(target.Stats.Food.Value / max, operation, value);
		}
		default:
			return false;
		}
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}'% {0}{2} {3}", invert ? "NOT " : "", stat.ToStringCached(), operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
