using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRefreshPerks : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string attribute = "";

	public override void Execute(MinEventParams _params)
	{
		if (_params.Self.Progression != null)
		{
			_params.Self.Progression.RefreshPerks(attribute);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "attribute")
		{
			attribute = _attribute.Value;
			return true;
		}
		return flag;
	}
}
