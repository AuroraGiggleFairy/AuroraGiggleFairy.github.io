using Audio;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerRangedAmmoSlots : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnOn_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Color32 onColor = new Color32(250, byte.MaxValue, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Color32 offColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredRangedTrap tileEntity;

	public TileEntityPoweredRangedTrap TileEntity
	{
		get
		{
			return tileEntity;
		}
		set
		{
			tileEntity = value;
			SetSlots(tileEntity.ItemSlots);
		}
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerRangedTrapWindowGroup Owner { get; set; }

	public override void HandleSlotChangedEvent(int _slotNumber, ItemStack _stack)
	{
		base.HandleSlotChangedEvent(_slotNumber, _stack);
		tileEntity.SetSendSlots();
		tileEntity.SetModified();
	}

	public override void Init()
	{
		base.Init();
		btnOn = windowGroup.Controller.GetChildById("windowPowerTrapSlots").GetChildById("btnOn");
		btnOn_Background = (XUiV_Button)btnOn.GetChildById("clickable").ViewComponent;
		btnOn_Background.Controller.OnPress += btnOn_OnPress;
		XUiController childById = btnOn.GetChildById("lblOnOff");
		if (childById != null)
		{
			lblOnOff = (XUiV_Label)childById.ViewComponent;
		}
		childById = btnOn.GetChildById("sprOnOff");
		if (childById != null)
		{
			sprOnOff = (XUiV_Sprite)childById.ViewComponent;
		}
		isDirty = true;
		turnOff = Localization.Get("xuiUnlockAmmo");
		turnOn = Localization.Get("xuiLockAmmo");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOn_OnPress(XUiController _sender, int _mouseButton)
	{
		bool flag = false;
		for (int i = 0; i < TileEntity.ItemSlots.Length; i++)
		{
			if (!TileEntity.ItemSlots[i].IsEmpty())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Manager.PlayInsidePlayerHead("ui_denied");
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttRequiresAmmo"));
		}
		else
		{
			tileEntity.IsLocked = !tileEntity.IsLocked;
			tileEntity.SetModified();
			RefreshIsLocked(tileEntity.IsLocked);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setRequirements()
	{
		XUiC_ItemStack[] array = itemControllers;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				xUiC_RequiredItemStack.RequiredType = XUiC_RequiredItemStack.RequiredTypes.ItemClass;
				xUiC_RequiredItemStack.SetAllowedItemClasses(tileEntity.AmmoItems);
			}
		}
	}

	public virtual void SetSlots(ItemStack[] _stacks)
	{
		items = _stacks;
		base.SetStacks(_stacks);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		tileEntity.SetUserAccessing(_bUserAccessing: true);
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		IsDirty = true;
		setRequirements();
		RefreshIsLocked(tileEntity.IsLocked);
		base.xui.powerAmmoSlots = this;
		IsDormant = false;
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightBumper, "igcoPoweredTrapLockAmmo", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			tileEntity.SetUserAccessing(_bUserAccessing: false);
			GameManager.Instance.TEUnlockServer(tileEntity.GetClrIdx(), tileEntity.ToWorldPos(), tileEntity.entityId);
			tileEntity.SetModified();
		}
		IsDirty = true;
		tileEntity = null;
		base.xui.powerAmmoSlots = null;
		IsDormant = true;
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIsLocked(bool _isOn)
	{
		if (_isOn)
		{
			lblOnOff.Text = turnOff;
			if (sprOnOff != null)
			{
				sprOnOff.Color = onColor;
				sprOnOff.SpriteName = "ui_game_symbol_lock";
			}
		}
		else
		{
			lblOnOff.Text = turnOn;
			if (sprOnOff != null)
			{
				sprOnOff.Color = offColor;
				sprOnOff.SpriteName = "ui_game_symbol_unlock";
			}
		}
		XUiC_ItemStack[] array = itemControllers;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				xUiC_RequiredItemStack.ToolLock = _isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetLocked(bool _isOn)
	{
		XUiC_ItemStack[] array = itemControllers;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				xUiC_RequiredItemStack.ToolLock = _isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] _stackList)
	{
		base.UpdateBackend(_stackList);
		tileEntity.ItemSlots = _stackList;
		tileEntity.SetSendSlots();
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Update(float _dt)
	{
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && tileEntity != null)
		{
			base.Update(_dt);
			if (lastLocked != tileEntity.IsLocked)
			{
				lastLocked = tileEntity.IsLocked;
				RefreshIsLocked(tileEntity.IsLocked);
			}
			if (tileEntity.IsLocked)
			{
				SetSlots(tileEntity.ItemSlots);
			}
			if (base.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasPressed && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				btnOn_OnPress(null, 0);
			}
			RefreshBindings();
		}
	}

	public bool TryAddItemToSlot(ItemClass _itemClass, ItemStack _itemStack)
	{
		XUiC_ItemStack[] array = itemControllers;
		for (int i = 0; i < array.Length; i++)
		{
			((XUiC_RequiredItemStack)array[i]).TryStack(_itemStack);
			if (_itemStack.count == 0)
			{
				return true;
			}
		}
		return _itemStack.count == 0;
	}
}
