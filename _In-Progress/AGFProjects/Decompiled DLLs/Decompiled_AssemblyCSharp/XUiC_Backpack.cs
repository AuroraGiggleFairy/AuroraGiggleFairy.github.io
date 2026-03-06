using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Backpack : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool wasHoldingItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxSlotCount;

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Backpack;
		}
	}

	public void RefreshBackpackSlots()
	{
		SetStacks(GetSlots());
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		RefreshBackpackSlots();
	}

	public override ItemStack[] GetSlots()
	{
		return base.xui.PlayerInventory.GetBackpackItemStacks();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.xui.PlayerInventory.SetBackpackItemStacks(stackList);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		wasHoldingItem = !base.xui.dragAndDrop.CurrentStack.IsEmpty();
		UpdateCallouts(wasHoldingItem);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		if (EffectManager.GetValue(PassiveEffects.ShuffledBackpack, null, 0f, base.xui.playerUI.entityPlayer) > 0f)
		{
			ItemStack[] slots = GetSlots();
			GameRandom rand = base.xui.playerUI.entityPlayer.rand;
			for (int i = 0; i < slots.Length * 2; i++)
			{
				int num = rand.RandomRange(slots.Length);
				int num2 = rand.RandomRange(slots.Length);
				if (!itemControllers[num].StackLock && !itemControllers[num2].StackLock)
				{
					ItemStack itemStack = slots[num];
					slots[num] = slots[num2];
					slots[num2] = itemStack;
				}
			}
			SetStacks(slots);
		}
		else
		{
			SetStacks(GetSlots());
		}
		base.xui.PlayerInventory.OnBackpackItemsChanged += PlayerInventory_OnBackpackItemsChanged;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.PlayerInventory.OnBackpackItemsChanged -= PlayerInventory_OnBackpackItemsChanged;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.xui.playerUI.entityPlayer.AimingGun = false;
		if (base.ViewComponent.IsVisible)
		{
			bool flag = !base.xui.dragAndDrop.CurrentStack.IsEmpty();
			if (flag != wasHoldingItem)
			{
				wasHoldingItem = flag;
				UpdateCallouts(flag);
			}
		}
		if (maxSlotCount != base.xui.playerUI.entityPlayer.bag.MaxItemCount)
		{
			RefreshBackpackSlots();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCallouts(bool _holdingItem)
	{
		string action = string.Format(Localization.Get("igcoItemActions"), InControlExtensions.GetBlankDPadSourceString());
		XUiC_GamepadCalloutWindow calloutWindow = base.xui.calloutWindow;
		if (calloutWindow != null)
		{
			calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			if (!_holdingItem)
			{
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, action, XUiC_GamepadCalloutWindow.CalloutType.Menu);
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			}
			calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		maxSlotCount = base.xui.playerUI.entityPlayer.bag.MaxItemCount;
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		int num = 0;
		for (int i = 0; i < stackList.Length && itemControllers.Length > i && stackList.Length > i; i++)
		{
			num = i;
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			xUiC_ItemStack.SlotChangedEvent -= handleSlotChangedDelegate;
			xUiC_ItemStack.ItemStack = stackList[i].Clone();
			xUiC_ItemStack.SlotChangedEvent += handleSlotChangedDelegate;
			xUiC_ItemStack.SlotNumber = i;
			xUiC_ItemStack.InfoWindow = childByType;
			xUiC_ItemStack.StackLocation = StackLocation;
			bool flag = i >= maxSlotCount;
			if (xUiC_ItemStack.AttributeLock != flag)
			{
				xUiC_ItemStack.AttributeLock = flag;
			}
		}
		for (int j = num; j < itemControllers.Length; j++)
		{
			XUiC_ItemStack xUiC_ItemStack2 = itemControllers[j];
			bool flag2 = j >= maxSlotCount;
			if (xUiC_ItemStack2.AttributeLock != flag2)
			{
				xUiC_ItemStack2.AttributeLock = flag2;
			}
		}
	}
}
