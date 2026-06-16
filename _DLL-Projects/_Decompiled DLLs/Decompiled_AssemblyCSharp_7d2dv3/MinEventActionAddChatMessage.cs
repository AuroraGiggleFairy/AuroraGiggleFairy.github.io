using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddChatMessage : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string message;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] is EntityPlayerLocal entityPlayer)
			{
				XUiC_ChatOutput.AddMessage(LocalPlayerUI.GetUIForPlayer(entityPlayer).xui, EnumGameMessages.PlainTextLocal, message, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.Server);
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params))
		{
			return message != null;
		}
		return false;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "message")
			{
				if (message == null || message == "")
				{
					message = _attribute.Value;
				}
				return true;
			}
			if (localName == "message_key")
			{
				if (Localization.Exists(_attribute.Value))
				{
					message = Localization.Get(_attribute.Value);
				}
				return true;
			}
		}
		return flag;
	}
}
