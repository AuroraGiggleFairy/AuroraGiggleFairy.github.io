using UnityEngine.Scripting;

[Preserve]
public class StatComparePercModMaxToMax : StatCompareAbs
{
	public override bool Compare(MinEventParams _params)
	{
		return stat switch
		{
			StatTypes.Health => invert != RequirementBase.compareValues(base.Stats.Health.ModifiedMaxPercent, operation, value), 
			StatTypes.Stamina => invert != RequirementBase.compareValues(base.Stats.Stamina.ModifiedMaxPercent, operation, value), 
			_ => false, 
		};
	}
}
