using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAnimatorSetWalkType : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int value;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			EntityAlive entityAlive = targets[i];
			if ((bool)entityAlive)
			{
				entityAlive.SetWalkType(value);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "value")
		{
			value = int.Parse(_attribute.Value);
			return true;
		}
		return flag;
	}
}
