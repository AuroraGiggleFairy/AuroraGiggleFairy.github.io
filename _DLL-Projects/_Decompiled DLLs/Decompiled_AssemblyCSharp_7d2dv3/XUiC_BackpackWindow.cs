using UnityEngine.Scripting;

[Preserve]
public class XUiC_BackpackWindow : XUiController
{
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
				xui.playerUI.windowManager.GetModalWindow().isEscClosable = !userLockMode;
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
			standardControls.GetLockedSlotsFromStorage = [PublicizedFrom(EAccessModifier.Private)] () => xui.playerUI.entityPlayer.bag.LockedSlots;
			standardControls.SetLockedSlotsToStorage = [PublicizedFrom(EAccessModifier.Private)] (PackedBoolArray _slots) =>
			{
				xui.playerUI.entityPlayer.bag.LockedSlots = _slots;
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
		if (xui.AssembleItem?.CurrentItem != null)
		{
			return false;
		}
		if (xui.LootContainer != null)
		{
			_dstInventory = xui.LootContainer;
			return true;
		}
		if (xui.FindWindowGroupByName(XUiC_BagStorageWindowGroup.ID) is XUiC_BagStorageWindowGroup { Bag: not null } xUiC_BagStorageWindowGroup)
		{
			_dstInventory = xUiC_BagStorageWindowGroup.Bag;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInventory_OnPress(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.entityPlayer.EmptyBackpack();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSort_OnPress(PackedBoolArray _ignoredSlots)
	{
		ItemStack itemStack = null;
		if (xui.AssembleItem.CurrentItemStackController != null)
		{
			itemStack = xui.AssembleItem.CurrentItemStackController.ItemStack;
		}
		xui.PlayerInventory.SortStacks(0, _ignoredSlots);
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
			int slotCount = xui.PlayerInventory.Backpack.SlotCount;
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
			if (XUi.IsGameRunning() && xui != null && xui.PlayerInventory != null)
			{
				value = currencyFormatter.Format(xui.PlayerInventory.CurrencyAmount);
			}
			return true;
		case "currencyicon":
			value = TraderInfo.CurrencyItem;
			return true;
		case "lootingorvehiclestorage":
		{
			value = TryGetMoveDestinationInventory(out var _).ToString();
			return true;
		}
		case "creativewindowopen":
			value = xui.playerUI.windowManager.IsWindowOpen("creative").ToString();
			return true;
		case "userlockmode":
			value = UserLockMode.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.PlayerInventory.RefreshCurrency();
		xui.PlayerInventory.OnCurrencyChanged += PlayerInventory_OnCurrencyChanged;
		if (!string.IsNullOrEmpty(defaultSelectedElement))
		{
			GetChildById(defaultSelectedElement).SelectCursorElement(_withDelay: true);
			defaultSelectedElement = "";
		}
		GameManager.Instance.StartCoroutine(xui.playerUI.entityPlayer.CancelInventoryActions([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
		}, holsterWeapon: false));
	}

	public override void OnClose()
	{
		base.OnClose();
		UserLockMode = false;
		if (xui != null && xui.PlayerInventory != null)
		{
			xui.PlayerInventory.OnCurrencyChanged -= PlayerInventory_OnCurrencyChanged;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		UpdateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInput()
	{
		PlayerActionsLocal playerInput = xui.playerUI.playerInput;
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
