using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetScale : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float scale = 1f;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			EntityAlive entityAlive = targets[i];
			entityAlive.OverrideSize = scale;
			entityAlive.SetScale(scale);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "scale")
		{
			scale = StringParsers.ParseFloat(_attribute.Value);
			return true;
		}
		return flag;
	}
}
