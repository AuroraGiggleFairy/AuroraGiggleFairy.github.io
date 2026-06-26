using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionLogMessage : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string message;

	public override void Execute(MinEventParams _params)
	{
		Log.Out("MinEventLogMessage: {0}", message);
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "message")
		{
			message = _attribute.Value;
			return true;
		}
		return flag;
	}
}
