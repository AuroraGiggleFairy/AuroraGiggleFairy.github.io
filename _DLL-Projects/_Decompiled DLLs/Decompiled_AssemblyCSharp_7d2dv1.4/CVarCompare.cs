using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class CVarCompare : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvarName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int cvarNameHash;

	public override bool IsValid(MinEventParams _params)
	{
		if (!ParamsValid(_params))
		{
			return false;
		}
		if (invert)
		{
			return !RequirementBase.compareValues(target.Buffs.GetCustomVar(cvarName), operation, value);
		}
		return RequirementBase.compareValues(target.Buffs.GetCustomVar(cvarName), operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add($"cvar.{cvarName} {operation.ToStringCached()} {value.ToCultureInvariantString()}");
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "cvar")
		{
			cvarName = _attribute.Value;
			cvarNameHash = cvarName.GetHashCode();
			return true;
		}
		return flag;
	}
}
