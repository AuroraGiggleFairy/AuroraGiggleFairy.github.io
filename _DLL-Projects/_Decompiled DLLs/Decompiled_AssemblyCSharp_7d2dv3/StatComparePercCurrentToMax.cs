using UnityEngine.Scripting;

[Preserve]
public class StatComparePercCurrentToMax : StatCompareAbs
{
	public override bool Compare(MinEventParams _params)
	{
		switch (stat)
		{
		case StatTypes.Health:
		{
			float max3 = base.Stats.Health.Max;
			if (max3 <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(base.Stats.Health.Value / max3, operation, value);
		}
		case StatTypes.Stamina:
		{
			float max2 = base.Stats.Stamina.Max;
			if (max2 <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(base.Stats.Stamina.Value / max2, operation, value);
		}
		case StatTypes.Water:
		{
			float max4 = target.Stats.Water.Max;
			if (max4 <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(base.Stats.Water.Value / max4, operation, value);
		}
		case StatTypes.Food:
		{
			float max = target.Stats.Food.Max;
			if (max <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(base.Stats.Food.Value / max, operation, value);
		}
		default:
			return false;
		}
	}
}
