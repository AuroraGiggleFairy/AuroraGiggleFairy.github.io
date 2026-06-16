using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class NotHasBuff : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!invert)
		{
			return !target.Buffs.HasBuff(buffName);
		}
		return target.Buffs.HasBuff(buffName);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "buff")
		{
			buffName = _attribute.Value.ToLower();
			return true;
		}
		return flag;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Target does {0}have buff '{1}'", (!invert) ? "NOT " : "", buffName));
	}
}
