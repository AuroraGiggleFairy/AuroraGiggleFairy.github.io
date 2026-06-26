using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddHealth : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int health;

	public override void Execute(MinEventParams _params)
	{
		if (targets == null)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].AddHealth(health);
			if (health < 0 && targets[i] is EntityPlayerLocal entityPlayerLocal)
			{
				entityPlayerLocal.ForceBloodSplatter();
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool num = base.ParseXmlAttribute(_attribute);
		if (!num && _attribute.Name.LocalName == "health")
		{
			health = StringParsers.ParseSInt32(_attribute.Value);
		}
		return num;
	}
}
