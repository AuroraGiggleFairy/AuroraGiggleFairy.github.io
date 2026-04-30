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
		if (!base.IsValid(_params))
		{
			return false;
		}
		return Compare(_params);
	}

	public virtual bool Compare(MinEventParams _params)
	{
		float valueA;
		switch (stat)
		{
		case StatTypes.Health:
			valueA = target.Health;
			break;
		case StatTypes.Stamina:
			valueA = target.Stamina;
			break;
		case StatTypes.Water:
			valueA = target.Stats.Water.Value;
			break;
		case StatTypes.Food:
			valueA = target.Stats.Food.Value;
			break;
		case StatTypes.Armor:
			valueA = target.equipment.CurrentLowestDurability;
			break;
		default:
			return false;
		}
		return invert != RequirementBase.compareValues(valueA, operation, value);
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
