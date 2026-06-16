public class XUiM_PlayerEquipment : XUiModel
{
	public bool IsOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Equipment equipment;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStackGrid equipmentGrid;

	public Equipment Equipment => equipment;

	public static event XUiEvent_RefreshEquipment HandleRefreshEquipment;

	public static event XUiEvent_EquipmentSlotChanged SlotChanged;

	public XUiM_PlayerEquipment(XUi _xui, EntityPlayerLocal _player)
	{
		if ((bool)_player)
		{
			xui = _xui;
			equipment = _player.equipment;
		}
	}

	public ItemStack EquipItem(ItemStack _stack)
	{
		if (!(_stack.itemValue.ItemClass is ItemClassArmor { EquipSlot: var equipSlot } itemClassArmor))
		{
			RefreshEquipment();
			return _stack;
		}
		XUiC_EquipmentStack equipmentStack = GetEquipmentStack(equipSlot);
		if (equipmentStack == null)
		{
			return _stack;
		}
		ItemStack stackFromSlot = GetStackFromSlot(equipSlot);
		bool flag = true;
		if (!stackFromSlot.IsEmpty() && stackFromSlot.itemValue.ItemClass is ItemClassArmor itemClassArmor2)
		{
			if (itemClassArmor2.ReplaceByTag != null && !FastTags<TagGroup.Global>.Parse(itemClassArmor2.ReplaceByTag).Test_AnySet(itemClassArmor.ItemTags))
			{
				return _stack;
			}
			flag = itemClassArmor2.AllowUnEquip;
		}
		equipmentStack.ItemStack = _stack.Clone();
		QuestEventManager.Current.WoreItem(_stack.itemValue);
		RefreshEquipment();
		XUiM_PlayerEquipment.SlotChanged?.Invoke(this, equipSlot, _stack);
		if (!flag)
		{
			return ItemStack.Empty;
		}
		return stackFromSlot;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool IsWearing(ItemValue _itemValue)
	{
		if (!(_itemValue.ItemClass is ItemClassArmor { EquipSlot: var equipSlot }))
		{
			return false;
		}
		if (equipSlot == EquipmentSlots.Count)
		{
			return false;
		}
		return GetStackFromSlot(equipSlot).itemValue.type == _itemValue.type;
	}

	public bool IsEquipmentTypeWorn(EquipmentSlots _slot)
	{
		if (_slot == EquipmentSlots.Count)
		{
			return false;
		}
		return !GetStackFromSlot(_slot).itemValue.IsEmpty();
	}

	public void RefreshEquipment()
	{
		equipment.FireEventsForSetSlots();
		XUiM_PlayerEquipment.HandleRefreshEquipment?.Invoke(this);
		equipment.FireEventsForChangedSlots();
	}

	public ItemStack GetStackFromSlot(EquipmentSlots _slot)
	{
		ItemStack empty = ItemStack.Empty;
		ItemValue slotItem = Equipment.GetSlotItem((int)_slot);
		if (slotItem == null)
		{
			return empty;
		}
		empty.itemValue = slotItem;
		empty.count = 1;
		return empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStack GetEquipmentStack(EquipmentSlots _slot)
	{
		if (equipmentGrid == null)
		{
			equipmentGrid = xui.GetChildByType<XUiC_EquipmentStackGrid>();
		}
		return equipmentGrid?.GetSlot(_slot);
	}
}
