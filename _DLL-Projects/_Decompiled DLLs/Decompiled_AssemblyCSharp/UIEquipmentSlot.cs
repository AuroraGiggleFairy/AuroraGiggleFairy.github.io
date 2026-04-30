using UnityEngine;

[AddComponentMenu("NGUI/Examples/UI Equipment Slot")]
public class UIEquipmentSlot : UIItemSlot
{
	public InvEquipment equipment;

	public InvBaseItem.Slot slot;

	public override InvGameItem observedItem
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!(equipment != null))
			{
				return null;
			}
			return equipment.GetItem(slot);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override InvGameItem Replace(InvGameItem item)
	{
		if (!(equipment != null))
		{
			return item;
		}
		return equipment.Replace(slot, item);
	}
}
