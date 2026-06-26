using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EquipmentStackGrid : XUiController
{
	public enum UIEquipmentSlots
	{
		Headgear,
		Eyewear,
		Face,
		Shirt,
		Jacket,
		ChestArmor,
		Gloves,
		Backpack,
		Pants,
		Footwear,
		LegArmor
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController[] itemControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemValue[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> slotIndexList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_EquipmentStack> equipmentList = new List<XUiC_EquipmentStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool slotsSetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAwakeCalled;

	public XUiC_EquipmentStack ExtraSlot;

	public virtual XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Equipment;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_EquipmentStack>();
		itemControllers = childrenByType;
		bAwakeCalled = true;
		IsDirty = false;
		XUiM_PlayerEquipment.HandleRefreshEquipment += XUiM_PlayerEquipment_HandleRefreshEquipment;
		base.xui.OnShutdown += HandleShutdown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
		if (base.xui.PlayerEquipment == _playerEquipment)
		{
			IsDirty = true;
		}
	}

	public void HandleShutdown()
	{
		XUiM_PlayerEquipment.HandleRefreshEquipment -= XUiM_PlayerEquipment_HandleRefreshEquipment;
		base.xui.OnShutdown -= HandleShutdown;
	}

	public void SetEquipmentSlotForStack(EquipmentSlots equipSlot)
	{
		XUiC_EquipmentStack xUiC_EquipmentStack = (XUiC_EquipmentStack)itemControllers[(int)equipSlot];
		if (xUiC_EquipmentStack != null)
		{
			xUiC_EquipmentStack.EquipSlot = equipSlot;
			equipmentList.Add(xUiC_EquipmentStack);
		}
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (IsDirty)
		{
			if (!slotsSetup)
			{
				slotIndexList.Clear();
				equipmentList.Clear();
				for (int i = 0; i < 5 && i < itemControllers.Length; i++)
				{
					if (itemControllers[i] is XUiC_EquipmentStack xUiC_EquipmentStack)
					{
						xUiC_EquipmentStack.EquipSlot = (EquipmentSlots)i;
						equipmentList.Add(xUiC_EquipmentStack);
					}
				}
				if (ExtraSlot != null)
				{
					equipmentList.Add(ExtraSlot);
				}
				slotsSetup = true;
			}
			items = GetSlots();
			SetStacks(items);
			IsDirty = false;
		}
		base.Update(_dt);
	}

	public virtual ItemValue[] GetSlots()
	{
		Equipment equipment = base.xui.PlayerEquipment.Equipment;
		ItemValue[] array = new ItemValue[equipmentList.Count];
		for (int i = 0; i < equipmentList.Count; i++)
		{
			ItemValue itemValue = equipment.GetSlotItem(i);
			if (itemValue == null)
			{
				itemValue = ItemValue.None.Clone();
			}
			array[i] = itemValue;
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleBagContentsChanged(ItemValue[] _items)
	{
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && base.xui.playerUI.entityPlayer != null)
		{
			SetStacks(_items);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetStacks(ItemValue[] stackList)
	{
		if (stackList != null)
		{
			XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
			XUiC_CharacterFrameWindow childByType2 = base.xui.GetChildByType<XUiC_CharacterFrameWindow>();
			for (int i = 0; i < stackList.Length && equipmentList.Count > i && stackList.Length > i; i++)
			{
				XUiC_EquipmentStack xUiC_EquipmentStack = equipmentList[i];
				xUiC_EquipmentStack.SlotChangedEvent -= HandleSlotChangedEvent;
				xUiC_EquipmentStack.ItemValue = stackList[i];
				xUiC_EquipmentStack.SlotChangedEvent += HandleSlotChangedEvent;
				xUiC_EquipmentStack.SlotNumber = i;
				xUiC_EquipmentStack.InfoWindow = childByType;
				xUiC_EquipmentStack.FrameWindow = childByType2;
			}
		}
	}

	public void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (stack.IsEmpty())
		{
			base.xui.PlayerEquipment.Equipment.SetSlotItem(slotNumber, null);
			base.xui.PlayerEquipment.RefreshEquipment();
			return;
		}
		items[slotNumber] = stack.itemValue.Clone();
		base.xui.PlayerEquipment.Equipment.SetSlotItem(slotNumber, stack.itemValue);
		base.xui.PlayerEquipment.RefreshEquipment();
		QuestEventManager.Current.WoreItem(stack.itemValue);
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
		for (int i = 0; i < itemControllers.Length; i++)
		{
			itemControllers[i].Hovered(_isOver: false);
		}
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		IsDormant = true;
	}
}
