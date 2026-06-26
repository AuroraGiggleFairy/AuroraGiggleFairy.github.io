using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemovePart : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string partName;

	public override void Execute(MinEventParams _params)
	{
		if (!(_params.Self == null))
		{
			_params.Self.RemovePart(partName);
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && _params.Self != null)
		{
			return partName != null;
		}
		return false;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "part")
		{
			partName = _attribute.Value;
			return true;
		}
		return flag;
	}
}
