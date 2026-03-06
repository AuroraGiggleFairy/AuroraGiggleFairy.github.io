using UnityEngine.Scripting;

[Preserve]
public class XUiC_BackpackWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Backpack backpackGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls standardControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt currencyFormatter = new CachedStringFormatterInt();

	public static string defaultSelectedElement;

	public bool UserLockMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return userLockMode;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != userLockMode)
			{
				if (userLockMode)
				{
					UpdateLockedSlots(standardControls);
				}
				standardControls?.LockModeChanged(value);
				userLockMode = value;
				base.WindowGroup.isEscClosable = !userLockMode;
				base.xui.playerUI.windowManager.GetModalWindow().isEscClosable = !userLockMode;
				RefreshBindings();
			}
		}
	}

	public override void Init()
	{
		base.Init();
		backpackGrid = GetChildByType<XUiC_Backpack>();
		standardControls = GetChildByType<XUiC_ContainerStandardControls>();
		if (standardControls != null)
		{
			standardControls.GetLockedSlotsFromStorage = [PublicizedFrom(EAccessModifier.Private)] () => base.xui.playerUI.entityPlayer.bag.LockedSlots;
			standardControls.SetLockedSlotsToStorage = [PublicizedFrom(EAccessModifier.Private)] (PackedBoolArray _slots) =>
			{
				base.xui.playerUI.entityPlayer.bag.LockedSlots = _slots;
			};
			standardControls.ApplyLockedSlotStates = ApplyLockedSlotStates;
			standardControls.UpdateLockedSlotStates = UpdateLockedSlots;
			standardControls.LockModeToggled = [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				UserLockMode = !UserLockMode;
			};
			standardControls.SortPressed = BtnSort_OnPress;
			standardControls.MoveAllowed = [PublicizedFrom(EAccessModifier.Private)] (out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory) =>
			{
				_parentWindow = this;
				_grid = backpackGrid;
				return TryGetMoveDestinationInventory(out _inventory);
			};
		}
		XUiController childById = GetChildById("btnClearInventory");
		if (childById != null)
		{
			childById.OnPress += BtnClearInventory_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetMoveDestinationInventory(out IInventory _dstInventory)
	{
		_dstInventory = null;
		if (base.xui.AssembleItem?.CurrentItem != null)
		{
			return false;
		}
		bool flag = base.xui.vehicle != null && base.xui.vehicle.GetVehicle().HasStorage();
		bool flag2 = base.xui.lootContainer != null && base.xui.lootContainer.EntityId == -1;
		bool flag3 = base.xui.lootContainer != null && GameManager.Instance.World.GetEntity(base.xui.lootContainer.EntityId) is EntityDrone;
		if (!flag && !flag2 && !flag3)
		{
			return false;
		}
		if (flag && base.xui.FindWindowGroupByName(XUiC_VehicleStorageWindowGroup.ID).GetChildByType<XUiC_VehicleContainer>() == null)
		{
			return false;
		}
		if (flag3)
		{
			_dstInventory = base.xui.lootContainer;
		}
		else
		{
			IInventory inventory;
			if (!flag2)
			{
				IInventory bag = base.xui.vehicle.bag;
				inventory = bag;
			}
			else
			{
				IInventory bag = base.xui.lootContainer;
				inventory = bag;
			}
			_dstInventory = inventory;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInventory_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.entityPlayer.EmptyBackpack();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSort_OnPress(PackedBoolArray _ignoredSlots)
	{
		ItemStack itemStack = null;
		if (base.xui.AssembleItem.CurrentItemStackController != null)
		{
			itemStack = base.xui.AssembleItem.CurrentItemStackController.ItemStack;
		}
		base.xui.PlayerInventory.SortStacks(0, _ignoredSlots);
		if (itemStack != null)
		{
			GetChildByType<XUiC_ItemStackGrid>().AssembleLockSingleStack(itemStack);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLockedSlotStates(PackedBoolArray _lockedSlots)
	{
		XUiC_ItemStack[] itemStackControllers = backpackGrid.GetItemStackControllers();
		for (int i = 0; i < itemStackControllers.Length; i++)
		{
			itemStackControllers[i].UserLockedSlot = _lockedSlots != null && i < _lockedSlots.Length && _lockedSlots[i];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLockedSlots(XUiC_ContainerStandardControls _csc)
	{
		if (_csc != null)
		{
			int slotCount = base.xui.PlayerInventory.Backpack.SlotCount;
			PackedBoolArray packedBoolArray = _csc.LockedSlots ?? new PackedBoolArray(slotCount);
			if (packedBoolArray.Length != slotCount)
			{
				packedBoolArray.Length = slotCount;
			}
			XUiC_ItemStack[] itemStackControllers = backpackGrid.GetItemStackControllers();
			for (int i = 0; i < itemStackControllers.Length && i < packedBoolArray.Length; i++)
			{
				packedBoolArray[i] = itemStackControllers[i].UserLockedSlot;
			}
			_csc.LockedSlots = packedBoolArray;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "currencyamount":
			value = "0";
			if (XUi.IsGameRunning() && base.xui != null && base.xui.PlayerInventory != null)
			{
				value = currencyFormatter.Format(base.xui.PlayerInventory.CurrencyAmount);
			}
			return true;
		case "currencyicon":
			value = TraderInfo.CurrencyItem;
			return true;
		case "lootingorvehiclestorage":
		{
			bool flag = base.xui.vehicle != null && base.xui.vehicle.GetVehicle().HasStorage();
			bool flag2 = base.xui.lootContainer != null && base.xui.lootContainer.EntityId == -1;
			bool flag3 = base.xui.lootContainer != null && GameManager.Instance.World.GetEntity(base.xui.lootContainer.EntityId) is EntityDrone;
			value = (flag || flag2 || flag3).ToString();
			return true;
		}
		case "creativewindowopen":
			value = base.xui.playerUI.windowManager.IsWindowOpen("creative").ToString();
			return true;
		case "userlockmode":
			value = UserLockMode.ToString();
			return true;
		default:
			return false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (localPlayer == null)
		{
			localPlayer = base.xui.playerUI.entityPlayer;
		}
		base.xui.PlayerInventory.RefreshCurrency();
		base.xui.PlayerInventory.OnCurrencyChanged += PlayerInventory_OnCurrencyChanged;
		RefreshBindings();
		if (!string.IsNullOrEmpty(defaultSelectedElement))
		{
			GetChildById(defaultSelectedElement).SelectCursorElement(_withDelay: true);
			defaultSelectedElement = "";
		}
		GameManager.Instance.StartCoroutine(localPlayer.CancelInventoryActions([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
		}, holsterWeapon: false));
	}

	public override void OnClose()
	{
		base.OnClose();
		UserLockMode = false;
		if (base.xui != null && base.xui.PlayerInventory != null)
		{
			base.xui.PlayerInventory.OnCurrencyChanged -= PlayerInventory_OnCurrencyChanged;
		}
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		PlayerActionsLocal playerInput = base.xui.playerUI.playerInput;
		if (UserLockMode && (playerInput.GUIActions.Cancel.WasPressed || playerInput.PermanentActions.Cancel.WasPressed))
		{
			UserLockMode = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnCurrencyChanged()
	{
		RefreshBindings();
	}
}
