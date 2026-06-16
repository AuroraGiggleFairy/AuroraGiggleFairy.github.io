using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetDancing : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].SetDancing(enabled);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool num = base.ParseXmlAttribute(_attribute);
		if (!num && _attribute.Name.LocalName == "enabled")
		{
			enabled = StringParsers.ParseBool(_attribute.Value);
		}
		return num;
	}
}
