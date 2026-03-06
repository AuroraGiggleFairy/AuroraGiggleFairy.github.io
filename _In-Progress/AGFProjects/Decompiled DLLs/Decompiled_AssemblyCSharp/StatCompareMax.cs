using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatCompareMax : StatCompareCurrent
{
	public override bool Compare(MinEventParams _params)
	{
		float max;
		switch (stat)
		{
		case StatTypes.Health:
			max = target.Stats.Health.Max;
			break;
		case StatTypes.Stamina:
			max = target.Stats.Stamina.Max;
			break;
		case StatTypes.Water:
			max = target.Stats.Water.Max;
			break;
		case StatTypes.Food:
			max = target.Stats.Food.Max;
			break;
		default:
			return false;
		}
		return invert != RequirementBase.compareValues(max, operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}'% {0}{2} {3}", invert ? "NOT " : "", stat.ToStringCached(), operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
