using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetOverrideLoot : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string altLoot = "";

	public override void Execute(MinEventParams _params)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] is EntityPlayer key)
			{
				if (altLoot == "")
				{
					LootContainer.OverrideItems.Remove(key);
				}
				else if (LootContainer.OverrideItems.ContainsKey(key))
				{
					LootContainer.OverrideItems[key] = altLoot.Split(',');
				}
				else
				{
					LootContainer.OverrideItems.Add(key, altLoot.Split(','));
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "items")
		{
			altLoot = _attribute.Value;
			return true;
		}
		return flag;
	}
}
