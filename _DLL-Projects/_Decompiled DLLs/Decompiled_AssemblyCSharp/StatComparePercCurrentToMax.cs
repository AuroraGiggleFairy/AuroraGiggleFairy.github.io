using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatComparePercCurrentToMax : StatCompareCurrent
{
	public override bool Compare(MinEventParams _params)
	{
		switch (stat)
		{
		case StatTypes.Health:
		{
			float max3 = target.Stats.Health.Max;
			if (max3 <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues((float)target.Health / max3, operation, value);
		}
		case StatTypes.Stamina:
		{
			float max2 = target.Stats.Stamina.Max;
			if (max2 <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(target.Stamina / max2, operation, value);
		}
		case StatTypes.Water:
		{
			float max4 = target.Stats.Water.Max;
			if (max4 <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(target.Stats.Water.Value / max4, operation, value);
		}
		case StatTypes.Food:
		{
			float max = target.Stats.Food.Max;
			if (max <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(target.Stats.Food.Value / max, operation, value);
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
