using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionPinToolbeltMessage : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string messageKey = "";

	public override void Execute(MinEventParams _params)
	{
		base.Execute(_params);
		if (_params.Self is EntityPlayerLocal player)
		{
			GameManager.ShowTooltip(player, messageKey, _showImmediately: false, _pinTooltip: true);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool num = base.ParseXmlAttribute(_attribute);
		if (!num && _attribute.Name.LocalName == "message_key")
		{
			messageKey = _attribute.Value;
		}
		return num;
	}
}
