using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetBigHead : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			EntityAlive entityAlive = targets[i];
			if (enabled)
			{
				entityAlive.SetBigHead();
			}
			else
			{
				entityAlive.ResetHead();
			}
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
