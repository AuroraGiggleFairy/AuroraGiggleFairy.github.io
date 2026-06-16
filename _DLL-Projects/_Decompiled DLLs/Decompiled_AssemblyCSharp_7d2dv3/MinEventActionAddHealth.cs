using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddHealth : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int health;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showSplatter = true;

	public override void Execute(MinEventParams _params)
	{
		if (targets == null)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			int num = ((!cvarRef) ? health : ((int)targets[i].Buffs.GetCustomVar(refCvarName)));
			targets[i].AddHealth(num);
			if (showSplatter && num < 0 && targets[i] is EntityPlayerLocal entityPlayerLocal)
			{
				entityPlayerLocal.ForceBloodSplatter();
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "health"))
			{
				if (localName == "show_splatter")
				{
					showSplatter = StringParsers.ParseBool(_attribute.Value);
				}
			}
			else if (_attribute.Value.StartsWith("@"))
			{
				cvarRef = true;
				refCvarName = _attribute.Value.Substring(1);
			}
			else
			{
				health = StringParsers.ParseSInt32(_attribute.Value);
			}
		}
		return flag;
	}
}
