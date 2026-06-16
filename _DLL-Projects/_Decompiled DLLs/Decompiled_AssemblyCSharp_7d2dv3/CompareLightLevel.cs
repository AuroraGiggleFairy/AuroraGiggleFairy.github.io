using System.Collections.Generic;

public class CompareLightLevel : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (target == null)
		{
			return false;
		}
		if (!invert)
		{
			return RequirementBase.compareValues(target.GetLightBrightness(), operation, value);
		}
		return !RequirementBase.compareValues(target.GetLightBrightness(), operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("light level '{0}'% {1}{2} {3}", target.GetLightBrightness().ToCultureInvariantString(), invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
