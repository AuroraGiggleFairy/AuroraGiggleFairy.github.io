using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionShowToolbeltMessage : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string message;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sound;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] as EntityPlayerLocal != null)
			{
				if (sound != null)
				{
					GameManager.ShowTooltip(targets[i] as EntityPlayerLocal, message, string.Empty, sound);
				}
				else
				{
					GameManager.ShowTooltip(targets[i] as EntityPlayerLocal, message);
				}
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
			switch (_attribute.Name.LocalName)
			{
			case "message":
				if (message == null || message == "")
				{
					message = _attribute.Value;
				}
				return true;
			case "message_key":
				if (Localization.Exists(_attribute.Value))
				{
					message = Localization.Get(_attribute.Value);
				}
				return true;
			case "sound":
				if (_attribute.Value != "")
				{
					sound = _attribute.Value;
				}
				return true;
			}
		}
		return flag;
	}
}
