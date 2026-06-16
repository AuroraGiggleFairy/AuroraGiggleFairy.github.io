using UnityEngine.Scripting;

[Preserve]
public class StatCompareModMax : StatCompareAbs
{
	public override bool Compare(MinEventParams _params)
	{
		float modifiedMax;
		switch (stat)
		{
		case StatTypes.Health:
			modifiedMax = base.Stats.Health.ModifiedMax;
			break;
		case StatTypes.Stamina:
			modifiedMax = base.Stats.Stamina.ModifiedMax;
			break;
		case StatTypes.Water:
			modifiedMax = base.Stats.Water.ModifiedMax;
			break;
		case StatTypes.Food:
			modifiedMax = base.Stats.Food.ModifiedMax;
			break;
		default:
			return false;
		}
		return invert != RequirementBase.compareValues(modifiedMax, operation, value);
	}
}
