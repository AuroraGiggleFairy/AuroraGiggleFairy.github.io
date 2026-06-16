using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionGetBuffDuration : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string reference;

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && _params.Buff?.BuffClass != null)
		{
			return !string.IsNullOrEmpty(reference);
		}
		return false;
	}

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].Buffs.SetCustomVar(reference, _params.Buff.BuffClass.DurationMax);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "reference")
		{
			reference = _attribute.Value;
			return true;
		}
		return flag;
	}
}
