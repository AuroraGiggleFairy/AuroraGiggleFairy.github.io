using UnityEngine;

[AddComponentMenu("NGUI/Examples/UI Storage Slot")]
public class UIStorageSlot : UIItemSlot
{
	public UIItemStorage storage;

	public int slot;

	public override InvGameItem observedItem
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!(storage != null))
			{
				return null;
			}
			return storage.GetItem(slot);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override InvGameItem Replace(InvGameItem item)
	{
		if (!(storage != null))
		{
			return item;
		}
		return storage.Replace(slot, item);
	}
}
