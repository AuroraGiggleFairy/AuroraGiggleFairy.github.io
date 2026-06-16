using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemoveCVar : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string[] cvarNames;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			for (int j = 0; j < cvarNames.Length; j++)
			{
				targets[i].Buffs.SetCustomVar(cvarNames[j], 0f, (targets[i].isEntityRemote && !_params.Self.isEntityRemote) || _params.IsLocal);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "cvar")
		{
			cvarNames = _attribute.Value.Split(',');
			return true;
		}
		return flag;
	}
}
