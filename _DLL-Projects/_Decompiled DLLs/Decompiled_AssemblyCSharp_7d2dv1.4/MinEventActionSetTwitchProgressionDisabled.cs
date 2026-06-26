using System.Xml.Linq;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetTwitchProgressionDisabled : MinEventActionTargetedBase
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool disabled
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Execute(MinEventParams _params)
	{
		if (targets == null)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] is EntityPlayerLocal { TwitchEnabled: not false })
			{
				TwitchManager.Current.OverrideProgession = disabled;
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool num = base.ParseXmlAttribute(_attribute);
		if (!num && _attribute.Name.LocalName == "disabled")
		{
			disabled = StringParsers.ParseBool(_attribute.Value);
		}
		return num;
	}
}
