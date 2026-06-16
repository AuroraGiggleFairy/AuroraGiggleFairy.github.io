using UnityEngine.Scripting;

[Preserve]
public class StatCompareMax : StatCompareAbs
{
	public override bool Compare(MinEventParams _params)
	{
		float max;
		switch (stat)
		{
		case StatTypes.Health:
			max = base.Stats.Health.Max;
			break;
		case StatTypes.Stamina:
			max = base.Stats.Stamina.Max;
			break;
		case StatTypes.Water:
			max = base.Stats.Water.Max;
			break;
		case StatTypes.Food:
			max = base.Stats.Food.Max;
			break;
		default:
			return false;
		}
		return invert != RequirementBase.compareValues(max, operation, value);
	}
}
