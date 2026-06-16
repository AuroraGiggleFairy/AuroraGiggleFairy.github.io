using System;
using System.Collections;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BagContainer : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Window window;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls standardControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public string containerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onBagModified;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowGridWidthDifference;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Bag Bag
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector2i GridCellSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Vehicle;
		}
	}

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
				RefreshBindings();
			}
		}
	}

	public override ItemStack[] GetSlots()
	{
		return items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] _stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] _stackList)
	{
	}

	public override void Init()
	{
		base.Init();
		window = (XUiV_Window)viewComponent;
		grid = (XUiV_Grid)GetChildById("queue").ViewComponent;
		GridCellSize = new Vector2i(grid.CellWidth, grid.CellHeight);
		standardControls = GetChildByType<XUiC_ContainerStandardControls>();
		if (standardControls == null)
		{
			return;
		}
		standardControls.GetLockedSlotsFromStorage = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			Bag bag = Bag;
			return bag.LockedSlots ?? (bag.LockedSlots = new PackedBoolArray(Bag.SlotCount));
		};
		standardControls.SetLockedSlotsToStorage = [PublicizedFrom(EAccessModifier.Private)] (PackedBoolArray _slots) =>
		{
			Bag.LockedSlots = _slots;
		};
		standardControls.ApplyLockedSlotStates = ApplyLockedSlotStates;
		standardControls.UpdateLockedSlotStates = UpdateLockedSlots;
		standardControls.LockModeToggled = [PublicizedFrom(EAccessModifier.Private)] () =>
		{
			UserLockMode = !UserLockMode;
		};
		standardControls.SortPressed = btnSort_OnPress;
		standardControls.MoveAllowed = [PublicizedFrom(EAccessModifier.Private)] (out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory) =>
		{
			_parentWindow = this;
			_grid = this;
			_inventory = xui.PlayerInventory;
			return true;
		};
		standardControls.MoveAllDone = [PublicizedFrom(EAccessModifier.Private)] (bool _allMoved, bool _anyMoved) =>
		{
			if (_anyMoved)
			{
				Manager.BroadcastPlayByLocalPlayer(XUiM_Player.GetPlayer().position, "UseActions/takeall1");
			}
			if (_allMoved)
			{
				ThreadManager.StartCoroutine(closeInventoryLater());
			}
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSort_OnPress(PackedBoolArray _ignoredSlots)
	{
		if (Bag != null)
		{
			ItemStack[] slots = StackSortUtil.CombineAndSortStacks(Bag.GetSlots(), 0, _ignoredSlots);
			Bag.SetSlots(slots);
		}
	}

	public void SetBag(Bag _bag, LootContainer _lootContainer, string _containerName, Action _onModified = null)
	{
		if (_bag == null || _lootContainer == null)
		{
			return;
		}
		Bag = _bag;
		containerName = _containerName;
		onBagModified = _onModified;
		grid.Columns = _lootContainer.size.x;
		grid.Rows = _lootContainer.size.y;
		ItemStack[] slots = _bag.GetSlots();
		int num = Mathf.CeilToInt((float)slots.Length / (float)grid.Columns);
		windowWidth = new Vector2i(grid.Columns * GridCellSize.x, num * GridCellSize.y).x + windowGridWidthDifference;
		_bag.OnBackpackItemsChangedInternal += OnBagItemChangedInternal;
		items = slots;
		XUiC_ItemInfoWindow childByType = xui.GetChildByType<XUiC_ItemInfoWindow>();
		int num2 = slots.Length;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			xUiC_ItemStack.SlotNumber = i;
			xUiC_ItemStack.SlotChangedEvent -= HandleBagSlotChangedEvent;
			xUiC_ItemStack.InfoWindow = childByType;
			xUiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			xUiC_ItemStack.UnlockStack();
			if (i < num2)
			{
				xUiC_ItemStack.ForceSetItemStack(items[i]);
				itemControllers[i].ViewComponent.IsVisible = true;
				xUiC_ItemStack.SlotChangedEvent += HandleBagSlotChangedEvent;
			}
			else
			{
				xUiC_ItemStack.ItemStack = ItemStack.Empty;
				itemControllers[i].ViewComponent.IsVisible = false;
			}
		}
		RefreshBindings();
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

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		if (IsDirty)
		{
			base.ViewComponent.IsVisible = Bag != null;
			RefreshBindings();
			IsDirty = false;
		}
		base.Update(_dt);
		if (windowGroup.isShowing)
		{
			if (!xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				wasReleased = true;
			}
			if (wasReleased)
			{
				if (xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					activeKeyDown = true;
				}
				if (xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown)
				{
					activeKeyDown = false;
					OnClose();
					xui.playerUI.windowManager.CloseAllOpenModalWindows();
				}
			}
			if (!isClosing && base.ViewComponent != null && base.ViewComponent.IsVisible && items != null && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !xui.playerUI.windowManager.IsInputActive()) && (xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
			{
				standardControls.MoveAll();
			}
		}
		UpdateInput();
	}

	public void HandleBagSlotChangedEvent(int _slotNumber, ItemStack _stack)
	{
		if (Bag != null)
		{
			Bag.SetSlot(_slotNumber, _stack);
		}
	}

	public void OnBagItemChangedInternal()
	{
		if (Bag != null)
		{
			ItemStack[] slots = Bag.GetSlots();
			for (int i = 0; i < slots.Length; i++)
			{
				SetItemInSlot(i, slots[i]);
			}
			onBagModified?.Invoke();
		}
	}

	public void SetItemInSlot(int _i, ItemStack _stack)
	{
		if (_i < itemControllers.Length)
		{
			itemControllers[_i].ItemStack = _stack;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		wasReleased = false;
		activeKeyDown = false;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		UserLockMode = false;
		if (Bag != null)
		{
			Bag.OnBackpackItemsChangedInternal -= OnBagItemChangedInternal;
		}
		Bag = null;
		onBagModified = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator closeInventoryLater()
	{
		isClosing = true;
		yield return null;
		xui.playerUI.windowManager.Close("bagStorage");
		isClosing = false;
	}

	public bool AddItem(ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Backpack))
		{
			return false;
		}
		if (Bag == null)
		{
			return false;
		}
		Bag.TryStackItem(0, _itemStack);
		if (_itemStack.count > 0)
		{
			return Bag.AddItem(_itemStack);
		}
		return false;
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "window_grid_width_difference")
		{
			windowGridWidthDifference = StringParsers.ParseSInt32(_value);
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "windowWidth":
			_value = windowWidth.ToString();
			return true;
		case "take_all_tooltip":
			_value = string.Format(Localization.Get("xuiLootTakeAllTooltip"), "[action:permanent:Reload:emptystring:KeyboardWithAngleBrackets]");
			return true;
		case "buttons_visible":
			_value = (windowWidth >= 450).ToString();
			return true;
		case "container_slots":
			_value = (Bag?.SlotCount ?? 0).ToString();
			return true;
		case "userlockmode":
			_value = UserLockMode.ToString();
			return true;
		case "hasslotlocks":
			_value = (standardControls?.LockedSlots != null).ToString();
			return true;
		case "lootcontainer_name":
			_value = ((!string.IsNullOrEmpty(containerName)) ? containerName : Localization.Get("xuiStorage"));
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLockedSlotStates(PackedBoolArray _lockedSlots)
	{
		XUiC_ItemStack[] itemStackControllers = GetItemStackControllers();
		for (int i = 0; i < itemStackControllers.Length; i++)
		{
			itemStackControllers[i].UserLockedSlot = _lockedSlots != null && i < _lockedSlots.Length && _lockedSlots[i];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLockedSlots(XUiC_ContainerStandardControls _csc)
	{
		if (_csc != null && Bag != null)
		{
			int slotCount = Bag.SlotCount;
			PackedBoolArray packedBoolArray = _csc.LockedSlots ?? new PackedBoolArray(slotCount);
			if (packedBoolArray.Length != slotCount)
			{
				packedBoolArray.Length = slotCount;
			}
			XUiC_ItemStack[] itemStackControllers = GetItemStackControllers();
			for (int i = 0; i < itemStackControllers.Length && i < packedBoolArray.Length; i++)
			{
				packedBoolArray[i] = itemStackControllers[i].UserLockedSlot;
			}
			_csc.LockedSlots = packedBoolArray;
		}
	}
}
