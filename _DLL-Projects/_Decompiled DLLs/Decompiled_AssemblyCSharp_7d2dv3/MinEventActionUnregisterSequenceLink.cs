using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionUnregisterSequenceLink : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string sequenceLink = "";

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] != null)
			{
				GameEventManager.Current.UnRegisterLink(_params.Self as EntityPlayer, sequenceLink);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool num = base.ParseXmlAttribute(_attribute);
		if (!num && _attribute.Name.LocalName == "sequence_link")
		{
			sequenceLink = _attribute.Value;
		}
		return num;
	}
}
