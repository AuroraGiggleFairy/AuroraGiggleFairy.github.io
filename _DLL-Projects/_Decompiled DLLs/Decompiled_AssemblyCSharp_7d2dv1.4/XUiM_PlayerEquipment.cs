public class XUiM_PlayerEquipment : XUiModel
{
	public bool IsOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public Equipment equipment;

	public Equipment Equipment => equipment;

	public static event XUiEvent_RefreshEquipment HandleRefreshEquipment;

	[PublicizedFrom(EAccessModifier.Private)]
	static XUiM_PlayerEquipment()
	{
	}

	public XUiM_PlayerEquipment(XUi _xui, EntityPlayerLocal _player)
	{
		if ((bool)_player)
		{
			xui = _xui;
			equipment = _player.equipment;
		}
	}

	public ItemStack EquipItem(ItemStack stack)
	{
		if (stack.itemValue.ItemClass is ItemClassArmor { EquipSlot: var equipSlot })
		{
			ItemStack stackFromSlot = GetStackFromSlot(equipSlot);
			if (!stackFromSlot.IsEmpty() && stackFromSlot.itemValue.ItemClass is ItemClassArmor itemClassArmor2)
			{
				equipment.SetSlotItem((int)itemClassArmor2.EquipSlot, null);
			}
			equipment.SetSlotItem((int)equipSlot, stack.itemValue.Clone());
			QuestEventManager.Current.WoreItem(stack.itemValue);
			RefreshEquipment();
			return stackFromSlot;
		}
		RefreshEquipment();
		return stack;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool IsWearing(ItemValue itemValue)
	{
		if (itemValue.ItemClass is ItemClassArmor { EquipSlot: var equipSlot })
		{
			if (equipSlot == EquipmentSlots.Count)
			{
				return false;
			}
			return GetStackFromSlot(equipSlot).itemValue.type == itemValue.type;
		}
		return false;
	}

	public bool IsEquipmentTypeWorn(EquipmentSlots slot)
	{
		if (slot == EquipmentSlots.Count)
		{
			return false;
		}
		return !GetStackFromSlot(slot).itemValue.IsEmpty();
	}

	public void RefreshEquipment()
	{
		equipment.FireEventsForSetSlots();
		if (XUiM_PlayerEquipment.HandleRefreshEquipment != null)
		{
			XUiM_PlayerEquipment.HandleRefreshEquipment(this);
		}
		equipment.FireEventsForChangedSlots();
	}

	public ItemStack GetStackFromSlot(EquipmentSlots slot)
	{
		ItemStack itemStack = ItemStack.Empty.Clone();
		ItemValue slotItem = Equipment.GetSlotItem((int)slot);
		if (slotItem != null)
		{
			itemStack.itemValue = slotItem;
			itemStack.count = 1;
			return itemStack;
		}
		return itemStack;
	}
}
