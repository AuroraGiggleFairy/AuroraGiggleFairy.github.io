using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAnimatorSetBool : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string property;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool value;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i].emodel != null && targets[i].emodel.avatarController != null)
			{
				targets[i].emodel.avatarController.UpdateBool(property, value);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "property")
			{
				property = _attribute.Value;
				return true;
			}
			if (localName == "value")
			{
				value = StringParsers.ParseBool(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
