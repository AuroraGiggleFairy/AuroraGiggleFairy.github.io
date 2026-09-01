using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionCallGameEvent : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string eventName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string sequenceLink = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool allowClientCall;

	public override void Execute(MinEventParams _params)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !allowClientCall)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] != null)
			{
				GameEventManager.Current.HandleAction(eventName, _params.Self as EntityPlayer, targets[i], twitchActivated: false, "", "", crateShare: false, allowRefunds: true, sequenceLink);
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
			case "event":
				eventName = _attribute.Value;
				break;
			case "sequence_link":
				sequenceLink = _attribute.Value;
				break;
			case "allow_client_call":
				allowClientCall = StringParsers.ParseBool(_attribute.Value);
				break;
			}
		}
		return flag;
	}
}
