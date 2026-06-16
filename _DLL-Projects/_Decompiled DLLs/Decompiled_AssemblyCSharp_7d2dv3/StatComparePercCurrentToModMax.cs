using UnityEngine.Scripting;

[Preserve]
public class StatComparePercCurrentToModMax : StatCompareAbs
{
	public override bool Compare(MinEventParams _params)
	{
		switch (stat)
		{
		case StatTypes.Health:
		{
			float modifiedMax2 = base.Stats.Health.ModifiedMax;
			if (modifiedMax2 <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(base.Stats.Health.Value / modifiedMax2, operation, value);
		}
		case StatTypes.Stamina:
		{
			float modifiedMax = base.Stats.Stamina.ModifiedMax;
			if (modifiedMax <= 0f)
			{
				return false;
			}
			return invert != RequirementBase.compareValues(base.Stats.Stamina.Value / modifiedMax, operation, value);
		}
		default:
			return false;
		}
	}
}
