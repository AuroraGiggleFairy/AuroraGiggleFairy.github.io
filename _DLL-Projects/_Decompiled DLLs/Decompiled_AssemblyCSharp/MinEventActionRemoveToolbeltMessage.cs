using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemoveToolbeltMessage : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string messageKey = "";

	public override void Execute(MinEventParams _params)
	{
		base.Execute(_params);
		if (_params.Self is EntityPlayerLocal player)
		{
			GameManager.RemovePinnedTooltip(player, messageKey);
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
