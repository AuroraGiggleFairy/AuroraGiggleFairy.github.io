using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionCVarLogValue : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string cvarName;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			Log.Out("CVarLogValue: {0} == {1}", cvarName, targets[i].Buffs.GetCustomVar(cvarName).ToCultureInvariantString());
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "cvar")
		{
			cvarName = _attribute.Value;
			return true;
		}
		return flag;
	}
}
