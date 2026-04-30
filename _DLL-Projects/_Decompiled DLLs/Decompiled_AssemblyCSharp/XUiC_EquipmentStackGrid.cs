using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EquipmentStackGrid : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EnumDictionary<EquipmentSlots, XUiC_EquipmentStack> itemControllers = new EnumDictionary<EquipmentSlots, XUiC_EquipmentStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue[] items;

	public override void Init()
	{
		base.Init();
		XUiC_EquipmentStack[] childrenByType = GetChildrenByType<XUiC_EquipmentStack>();
		RegisterExtraSlots(childrenByType);
		IsDirty = false;
		XUiM_PlayerEquipment.HandleRefreshEquipment += XUiM_PlayerEquipment_HandleRefreshEquipment;
		base.xui.OnShutdown += handleShutdown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
		if (base.xui.PlayerEquipment == _playerEquipment)
		{
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleShutdown()
	{
		XUiM_PlayerEquipment.HandleRefreshEquipment -= XUiM_PlayerEquipment_HandleRefreshEquipment;
		base.xui.OnShutdown -= handleShutdown;
	}

	public void RegisterExtraSlots(IEnumerable<XUiC_EquipmentStack> _slots)
	{
		foreach (XUiC_EquipmentStack _slot in _slots)
		{
			if (_slot.EquipSlot == EquipmentSlots.Count)
			{
				Log.Error("[XUi] EquipmentStack slot does not have a proper 'slot' value");
			}
			itemControllers[_slot.EquipSlot] = _slot;
			_slot.SlotChangedEvent += HandleSlotChangedEvent;
		}
	}

	public override void Update(float _dt)
	{
		if (!(GameManager.Instance == null) || GameManager.Instance.World != null)
		{
			if (IsDirty)
			{
				items = GetSlots();
				SetStacks(items);
				IsDirty = false;
			}
			base.Update(_dt);
		}
	}

	public virtual ItemValue[] GetSlots()
	{
		Equipment equipment = base.xui.PlayerEquipment.Equipment;
		ItemValue[] array = new ItemValue[12];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = equipment.GetSlotItem(i) ?? ItemValue.None.Clone();
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetStacks(ItemValue[] _stackList)
	{
		if (_stackList == null)
		{
			return;
		}
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		XUiC_CharacterFrameWindow childByType2 = base.xui.GetChildByType<XUiC_CharacterFrameWindow>();
		for (int i = 0; i < _stackList.Length; i++)
		{
			XUiC_EquipmentStack slot = GetSlot((EquipmentSlots)i);
			if (slot != null)
			{
				slot.SlotChangedEvent -= HandleSlotChangedEvent;
				slot.ItemValue = _stackList[i];
				slot.SlotChangedEvent += HandleSlotChangedEvent;
				slot.InfoWindow = childByType;
				slot.FrameWindow = childByType2;
			}
		}
	}

	public void HandleSlotChangedEvent(int _slotNumber, ItemStack _stack)
	{
		if (items == null)
		{
			items = GetSlots();
		}
		if (!_stack.IsEmpty())
		{
			string soundPlace = _stack.itemValue.ItemClass.SoundPlace;
			if (!string.IsNullOrEmpty(soundPlace))
			{
				Manager.PlayInsidePlayerHead(soundPlace);
			}
		}
		if (_stack.IsEmpty())
		{
			base.xui.PlayerEquipment.Equipment.SetSlotItem(_slotNumber, null);
			base.xui.PlayerEquipment.RefreshEquipment();
			return;
		}
		items[_slotNumber] = _stack.itemValue.Clone();
		base.xui.PlayerEquipment.Equipment.SetSlotItem(_slotNumber, _stack.itemValue);
		base.xui.PlayerEquipment.RefreshEquipment();
		QuestEventManager.Current.WoreItem(_stack.itemValue);
	}

	public XUiC_EquipmentStack GetSlot(EquipmentSlots _slotType)
	{
		itemControllers.TryGetValue(_slotType, out var value);
		return value;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		IsDirty = true;
		IsDormant = false;
	}

	public override void OnClose()
	{
		foreach (KeyValuePair<EquipmentSlots, XUiC_EquipmentStack> itemController in itemControllers)
		{
			itemController.Deconstruct(out var _, out var value);
			value.Hovered(_isOver: false);
		}
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		IsDormant = true;
	}
}
