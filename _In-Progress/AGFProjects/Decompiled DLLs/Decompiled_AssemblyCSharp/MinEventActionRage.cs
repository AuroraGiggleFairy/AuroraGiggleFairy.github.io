using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRage : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedPercent = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rageTime = 60f;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			EntityHuman entityHuman = targets[i] as EntityHuman;
			if (entityHuman != null)
			{
				if (enabled)
				{
					entityHuman.StartRage(speedPercent, rageTime + 1f);
				}
				else
				{
					entityHuman.StopRage();
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "speed":
				speedPercent = StringParsers.ParseFloat(_attribute.Value);
				return true;
			case "time":
				rageTime = StringParsers.ParseFloat(_attribute.Value);
				return true;
			case "enabled":
				enabled = StringParsers.ParseBool(_attribute.Value);
				break;
			}
		}
		return flag;
	}
}
