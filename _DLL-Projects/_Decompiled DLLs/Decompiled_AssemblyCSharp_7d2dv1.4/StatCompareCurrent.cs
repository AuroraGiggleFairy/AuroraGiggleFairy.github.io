using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class StatCompareCurrent : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatTypes
	{
		None,
		Health,
		Stamina,
		Food,
		Water,
		Armor
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatTypes stat;

	public override bool IsValid(MinEventParams _params)
	{
		if (!ParamsValid(_params))
		{
			return false;
		}
		switch (stat)
		{
		case StatTypes.Health:
			if (!invert)
			{
				return RequirementBase.compareValues(target.Health, operation, value);
			}
			return !RequirementBase.compareValues(target.Health, operation, value);
		case StatTypes.Stamina:
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stamina, operation, value);
			}
			return !RequirementBase.compareValues(target.Stamina, operation, value);
		case StatTypes.Water:
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stats.Water.Value, operation, value);
			}
			return !RequirementBase.compareValues(target.Stats.Water.Value, operation, value);
		case StatTypes.Food:
			if (!invert)
			{
				return RequirementBase.compareValues(target.Stats.Food.Value, operation, value);
			}
			return !RequirementBase.compareValues(target.Stats.Food.Value, operation, value);
		case StatTypes.Armor:
			if (!invert)
			{
				return RequirementBase.compareValues(target.equipment.CurrentLowestDurability, operation, value);
			}
			return !RequirementBase.compareValues(target.equipment.CurrentLowestDurability, operation, value);
		default:
			return false;
		}
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}' {0} {2} {3}", invert ? "NOT" : "", stat.ToStringCached(), operation.ToStringCached(), value.ToCultureInvariantString()));
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
