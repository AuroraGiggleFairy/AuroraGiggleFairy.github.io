using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetNavObject : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string navObjectName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string overrideSprite = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string overrideText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string cvarToText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAdd = true;

	public override void Execute(MinEventParams _params)
	{
		if (navObjectName == "")
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			EntityAlive entityAlive = targets[i];
			if (isAdd)
			{
				entityAlive.AddNavObject(navObjectName, overrideSprite, (cvarToText != "") ? entityAlive.GetCVar(cvarToText).ToString() : overrideText);
			}
			else
			{
				entityAlive.RemoveNavObject(navObjectName);
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
			case "nav_object":
				navObjectName = _attribute.Value;
				return true;
			case "sprite":
				overrideSprite = _attribute.Value;
				return true;
			case "text":
				overrideText = _attribute.Value;
				return true;
			case "cvar_to_text":
				cvarToText = _attribute.Value;
				return true;
			case "add":
				isAdd = StringParsers.ParseBool(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
