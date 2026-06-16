using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetBigHead : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool instant;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			EntityAlive entityAlive = targets[i];
			if (enabled)
			{
				if (instant)
				{
					entityAlive.ForceBigHead();
				}
				else
				{
					entityAlive.SetBigHead();
				}
			}
			else if (instant)
			{
				entityAlive.ForceResetHead();
			}
			else
			{
				entityAlive.ResetHead();
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "instant"))
			{
				if (localName == "enabled")
				{
					enabled = StringParsers.ParseBool(_attribute.Value);
				}
			}
			else
			{
				instant = StringParsers.ParseBool(_attribute.Value);
			}
		}
		return flag;
	}
}
