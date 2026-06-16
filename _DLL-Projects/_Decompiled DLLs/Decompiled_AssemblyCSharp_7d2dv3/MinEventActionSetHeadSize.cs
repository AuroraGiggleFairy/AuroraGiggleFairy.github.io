using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetHeadSize : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float standard = 1f;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].SetHeadSize(standard);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "size")
		{
			standard = StringParsers.ParseFloat(_attribute.Value);
			return true;
		}
		return flag;
	}
}
