using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatComparePercCurrentToModMax : StatCompareCurrent
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
			float modifiedMax2 = target.Stats.Health.ModifiedMax;
			if (modifiedMax2 <= 0f)
			{
				return false;
			}
			if (!invert)
			{
				return RequirementBase.compareValues((float)target.Health / modifiedMax2, operation, value);
			}
			return !RequirementBase.compareValues((float)target.Health / modifiedMax2, operation, value);
		}
		case StatTypes.Stamina:
		{
			float modifiedMax = target.Stats.Stamina.ModifiedMax;
			if (modifiedMax <= 0f)
			{
				return false;
			}
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stamina / modifiedMax, operation, value);
			}
			return !RequirementBase.compareValues(target.Stamina / modifiedMax, operation, value);
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
