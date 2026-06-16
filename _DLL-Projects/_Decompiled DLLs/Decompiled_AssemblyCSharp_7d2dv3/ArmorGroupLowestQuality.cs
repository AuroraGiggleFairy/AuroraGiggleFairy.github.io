using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class ArmorGroupLowestQuality : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string armorGroupName;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		int armorGroupLowestQuality = target.equipment.GetArmorGroupLowestQuality(armorGroupName);
		if (invert)
		{
			return !RequirementBase.compareValues(armorGroupLowestQuality, operation, value);
		}
		return RequirementBase.compareValues(armorGroupLowestQuality, operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("ArmorGroupLowestQuality: {0}{1} {2}", invert ? "NOT " : "", operation.ToStringCached(), value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "group_name")
		{
			armorGroupName = _attribute.Value;
			return true;
		}
		return flag;
	}
}
