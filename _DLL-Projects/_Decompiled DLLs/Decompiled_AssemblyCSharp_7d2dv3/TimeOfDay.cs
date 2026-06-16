using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class TimeOfDay : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong timeValue;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!isSetup)
		{
			timeValue = GameUtils.DayTimeToWorldTime(1, (int)value / 100, (int)value % 100);
			isSetup = true;
		}
		ulong num = GameManager.Instance.World.worldTime % 24000;
		if (invert)
		{
			return !RequirementBase.compareValues(num, operation, timeValue);
		}
		return RequirementBase.compareValues(num, operation, timeValue);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("time of day {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
