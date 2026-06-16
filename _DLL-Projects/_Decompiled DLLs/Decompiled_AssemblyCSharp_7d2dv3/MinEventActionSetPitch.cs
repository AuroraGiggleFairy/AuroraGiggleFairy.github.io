using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetPitch : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float pitch = 1f;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].OverridePitch = pitch;
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "pitch")
		{
			pitch = StringParsers.ParseFloat(_attribute.Value);
			return true;
		}
		return flag;
	}
}
