using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class IsStatAtMax : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatTypes
	{
		None,
		Health,
		Stamina,
		Food,
		Water
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatTypes stat;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		switch (stat)
		{
		case StatTypes.Health:
			if (target.Stats.Health.Max - target.Stats.Health.Value < 0.1f)
			{
				return !invert;
			}
			return invert;
		case StatTypes.Stamina:
			if (target.Stats.Stamina.Max - target.Stats.Stamina.Value < 0.1f)
			{
				return !invert;
			}
			return invert;
		case StatTypes.Water:
			if (target.Stats.Water.Max - target.Stats.Water.Value < 0.1f)
			{
				return !invert;
			}
			return invert;
		case StatTypes.Food:
			if (target.Stats.Food.Max - target.Stats.Food.Value < 0.1f)
			{
				return !invert;
			}
			return invert;
		default:
			return false;
		}
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "stat")
		{
			stat = EnumUtils.Parse<StatTypes>(_attribute.Value, _ignoreCase: true);
			return true;
		}
		return flag;
	}
}
