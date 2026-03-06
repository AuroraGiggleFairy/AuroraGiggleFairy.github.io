using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class HasBuff : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] buffNames;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		int num = buffNames.Length;
		for (int i = 0; i < num; i++)
		{
			if (target.Buffs.HasBuff(buffNames[i]))
			{
				return !invert;
			}
		}
		return invert;
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "buff")
		{
			buffNames = _attribute.Value.ToLower().Split(RequirementBase.commaSeparator);
			return true;
		}
		return flag;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Target does {0}have buff '{1}(0)'", invert ? "NOT " : "", buffNames));
	}
}
