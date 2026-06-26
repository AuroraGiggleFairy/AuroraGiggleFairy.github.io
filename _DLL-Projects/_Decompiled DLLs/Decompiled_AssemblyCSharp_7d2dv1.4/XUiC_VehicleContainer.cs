using System.Collections;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleContainer : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasStorage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Window window;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls controls;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSlotsSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle currentVehicleEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowGridWidthDifference;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	public int containerSlotsCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return containerSlotsSize.x * containerSlotsSize.y;
		}
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

	public override ItemStack[] GetSlots()
	{
		return items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
	}

	public override void Init()
	{
		base.Init();
		window = (XUiV_Window)viewComponent;
		grid = (XUiV_Grid)GetChildById("queue").ViewComponent;
		GridCellSize = new Vector2i(grid.CellWidth, grid.CellHeight);
		controls = GetChildByType<XUiC_ContainerStandardControls>();
		if (controls == null)
		{
			return;
		}
		controls.SortPressed = btnSort_OnPress;
		controls.MoveAllowed = [PublicizedFrom(EAccessModifier.Private)] (out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory) =>
		{
			_parentWindow = this;
			_grid = this;
			_inventory = base.xui.PlayerInventory;
			return true;
		};
		controls.MoveAllDone = [PublicizedFrom(EAccessModifier.Private)] (bool _allMoved, bool _anyMoved) =>
		{
			if (_anyMoved)
			{
				Manager.BroadcastPlayByLocalPlayer(currentVehicleEntity.position + Vector3.one * 0.5f, "UseActions/takeall1");
			}
			if (_allMoved)
			{
				ThreadManager.StartCoroutine(closeInventoryLater());
			}
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSort_OnPress(bool[] _ignoredSlos)
	{
		if (base.xui.vehicle.GetVehicle() != null)
		{
			ItemStack[] slots = StackSortUtil.CombineAndSortStacks(base.xui.vehicle.bag.GetSlots(), 0, _ignoredSlos);
			base.xui.vehicle.bag.SetSlots(slots);
		}
	}

	public void SetSlots(ItemStack[] stackList)
	{
		if (stackList == null || base.xui.vehicle.GetVehicle() == null)
		{
			return;
		}
		currentVehicleEntity = base.xui.vehicle;
		containerSlotsSize = currentVehicleEntity.lootContainer.GetContainerSize();
		windowWidth = new Vector2i(containerSlotsSize.x * GridCellSize.x, containerSlotsSize.y * GridCellSize.y).x + windowGridWidthDifference;
		base.xui.vehicle.bag.OnBackpackItemsChangedInternal += OnBagItemChangedInternal;
		items = stackList;
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		grid.Columns = containerSlotsSize.x;
		grid.Rows = containerSlotsSize.y;
		int num = stackList.Length;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_ItemStack xUiC_ItemStack = itemControllers[i];
			xUiC_ItemStack.SlotNumber = i;
			xUiC_ItemStack.SlotChangedEvent -= HandleLootSlotChangedEvent;
			xUiC_ItemStack.InfoWindow = childByType;
			xUiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			xUiC_ItemStack.UnlockStack();
			if (i < num)
			{
				xUiC_ItemStack.ForceSetItemStack(items[i]);
				itemControllers[i].ViewComponent.IsVisible = true;
				xUiC_ItemStack.SlotChangedEvent += HandleLootSlotChangedEvent;
			}
			else
			{
				xUiC_ItemStack.ItemStack = ItemStack.Empty.Clone();
				itemControllers[i].ViewComponent.IsVisible = false;
			}
		}
		RefreshBindings(_forceAll: true);
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (IsDirty)
		{
			hasStorage = base.xui.vehicle.GetVehicle().HasStorage();
			base.ViewComponent.IsVisible = hasStorage;
			IsDirty = false;
		}
		base.Update(_dt);
		if (!windowGroup.isShowing)
		{
			return;
		}
		if (!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
		{
			wasReleased = true;
		}
		if (wasReleased)
		{
			if (base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				activeKeyDown = true;
			}
			if (base.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown)
			{
				activeKeyDown = false;
				OnClose();
				base.xui.playerUI.windowManager.CloseAllOpenWindows();
			}
		}
		if (!isClosing && base.ViewComponent != null && base.ViewComponent.IsVisible && items != null && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !base.xui.playerUI.windowManager.IsInputActive()) && (base.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
		{
			controls.MoveAll();
		}
	}

	public void HandleLootSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (!(base.xui.vehicle == null))
		{
			base.xui.vehicle.bag.SetSlot(slotNumber, stack);
		}
	}

	public void OnBagItemChangedInternal()
	{
		if (!(base.xui.vehicle == null))
		{
			ItemStack[] slots = base.xui.vehicle.bag.GetSlots();
			for (int i = 0; i < slots.Length; i++)
			{
				SetItemInSlot(i, slots[i]);
			}
			base.xui.vehicle.SetBagModified();
		}
	}

	public void SetItemInSlot(int i, ItemStack stack)
	{
		if (i < itemControllers.Length)
		{
			itemControllers[i].ItemStack = stack;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		window.TargetAlpha = 1f;
		base.ViewComponent.OnOpen();
		base.ViewComponent.IsVisible = true;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		currentVehicleEntity = null;
		if (!(base.xui.vehicle == null))
		{
			base.xui.vehicle.bag.OnBackpackItemsChangedInternal -= OnBagItemChangedInternal;
			window.TargetAlpha = 0f;
			base.xui.vehicle = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator closeInventoryLater()
	{
		yield return null;
		base.xui.playerUI.windowManager.CloseIfOpen("vehicleStorage");
		isClosing = false;
	}

	public bool AddItem(ItemStack itemStack)
	{
		base.xui.vehicle.bag.TryStackItem(0, itemStack);
		if (itemStack.count > 0 && base.xui.vehicle.bag.AddItem(itemStack))
		{
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "window_grid_width_difference")
		{
			windowGridWidthDifference = StringParsers.ParseSInt32(_value);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
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
			_value = containerSlotsCount.ToString();
			return true;
		default:
			return base.GetBindingValue(ref _value, _bindingName);
		}
	}
}
