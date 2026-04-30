using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatComparePercModMaxToMax : StatCompareCurrent
{
	public override bool Compare(MinEventParams _params)
	{
		return stat switch
		{
			StatTypes.Health => invert != RequirementBase.compareValues(target.Stats.Health.ModifiedMaxPercent, operation, value), 
			StatTypes.Stamina => invert != RequirementBase.compareValues(target.Stats.Stamina.ModifiedMaxPercent, operation, value), 
			_ => false, 
		};
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}'% {0}{2} {3}", invert ? "NOT " : "", stat.ToStringCached(), operation.ToStringCached(), value.ToCultureInvariantString()));
	}
}
