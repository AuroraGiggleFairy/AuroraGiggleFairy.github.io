using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetItemInSlot : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string itemName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlots slot = EquipmentSlots.BiomeBadge;

	public override void Execute(MinEventParams _params)
	{
		ItemValue item = ItemClass.GetItem(itemName);
		if (item.ItemClass is ItemClassArmor itemClassArmor && itemClassArmor.EquipSlot == slot)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				targets[i].equipment.SetSlotItem((int)slot, item);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "item_name")
			{
				itemName = _attribute.Value;
				return true;
			}
			if (localName == "equip_slot")
			{
				Enum.TryParse<EquipmentSlots>(_attribute.Value, out slot);
				return true;
			}
		}
		return flag;
	}
}
