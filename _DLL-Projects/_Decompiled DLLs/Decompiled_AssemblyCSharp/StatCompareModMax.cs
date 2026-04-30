using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatCompareModMax : StatCompareCurrent
{
	public override bool Compare(MinEventParams _params)
	{
		float modifiedMax;
		switch (stat)
		{
		case StatTypes.Health:
			modifiedMax = target.Stats.Health.ModifiedMax;
			break;
		case StatTypes.Stamina:
			modifiedMax = target.Stats.Stamina.ModifiedMax;
			break;
		case StatTypes.Water:
			modifiedMax = target.Stats.Water.ModifiedMax;
			break;
		case StatTypes.Food:
			modifiedMax = target.Stats.Food.ModifiedMax;
			break;
		default:
			return false;
		}
		return invert != RequirementBase.compareValues(modifiedMax, operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}'% {0}{2} {3}", invert ? "NOT " : "", stat.ToStringCached(), operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
