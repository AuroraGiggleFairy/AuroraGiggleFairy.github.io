using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAnimatorSetInt : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string property;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int value;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i].emodel != null && targets[i].emodel.avatarController != null)
			{
				targets[i].emodel.avatarController.UpdateInt(property, value);
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
				value = int.Parse(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
