using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class InBiome : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeID;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.Biome == null)
		{
			return false;
		}
		if (!invert)
		{
			return biomeID == _params.Biome.m_Id;
		}
		return biomeID != _params.Biome.m_Id;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("{0}in biome {1}", invert ? "NOT " : "", biomeID));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "biome")
		{
			biomeID = StringParsers.ParseSInt32(_attribute.Value);
			return true;
		}
		return flag;
	}
}
