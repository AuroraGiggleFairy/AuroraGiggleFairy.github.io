using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class CVarCompare : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvarCompareName;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		return invert != RequirementBase.compareValues(target.Buffs.GetCustomVar(cvarCompareName), operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add($"cvar.{cvarCompareName} {operation.ToStringCached()} {value.ToCultureInvariantString()}");
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "cvar")
		{
			cvarCompareName = _attribute.Value;
			return true;
		}
		return flag;
	}
}
