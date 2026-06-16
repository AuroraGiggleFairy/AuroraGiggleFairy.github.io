using UnityEngine.Scripting;

[Preserve]
public class StatCompareCurrent : StatCompareAbs
{
	public override bool Compare(MinEventParams _params)
	{
		float currentLowestDurability;
		switch (stat)
		{
		case StatTypes.Health:
			currentLowestDurability = base.Stats.Health.Value;
			break;
		case StatTypes.Stamina:
			currentLowestDurability = base.Stats.Stamina.Value;
			break;
		case StatTypes.Water:
			currentLowestDurability = base.Stats.Water.Value;
			break;
		case StatTypes.Food:
			currentLowestDurability = base.Stats.Food.Value;
			break;
		case StatTypes.Armor:
			currentLowestDurability = target.equipment.CurrentLowestDurability;
			break;
		default:
			return false;
		}
		return invert != RequirementBase.compareValues(currentLowestDurability, operation, value);
	}
}
