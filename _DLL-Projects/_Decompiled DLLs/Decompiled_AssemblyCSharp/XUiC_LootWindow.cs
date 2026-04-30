using System.Collections;
using Audio;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable te;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_LootContainer lootContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls standardControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootContainerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSlotsSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowGridWidthDifference;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	public int ContainerSlotsCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return containerSlotsSize.x * containerSlotsSize.y;
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

	public override void Init()
	{
		base.Init();
		lootContainer = GetChildByType<XUiC_LootContainer>();
		standardControls = GetChildByType<XUiC_ContainerStandardControls>();
		if (standardControls != null)
		{
			standardControls.GetLockedSlotsFromStorage = [PublicizedFrom(EAccessModifier.Private)] () => (!te.HasSlotLocksSupport) ? null : te.SlotLocks;
			standardControls.SetLockedSlotsToStorage = [PublicizedFrom(EAccessModifier.Private)] (PackedBoolArray _slots) =>
			{
				if (te.HasSlotLocksSupport)
				{
					te.SlotLocks = _slots;
				}
			};
			standardControls.ApplyLockedSlotStates = ApplyLockedSlotStates;
			standardControls.UpdateLockedSlotStates = UpdateLockedSlots;
			standardControls.LockModeToggled = [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				UserLockMode = !UserLockMode;
			};
			standardControls.SortPressed = [PublicizedFrom(EAccessModifier.Private)] (PackedBoolArray _ignoredSlots) =>
			{
				ItemStack[] array = StackSortUtil.CombineAndSortStacks(te.items, 0, _ignoredSlots);
				for (int i = 0; i < array.Length; i++)
				{
					te.UpdateSlot(i, array[i]);
				}
				te.SetModified();
			};
			standardControls.MoveAllowed = [PublicizedFrom(EAccessModifier.Private)] (out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory) =>
			{
				_parentWindow = this;
				_grid = lootContainer;
				_inventory = base.xui.PlayerInventory;
				return true;
			};
			standardControls.MoveAllDone = [PublicizedFrom(EAccessModifier.Private)] (bool _allMoved, bool _anyMoved) =>
			{
				if (_anyMoved)
				{
					Manager.BroadcastPlayByLocalPlayer(te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "UseActions/takeall1");
				}
				if (_allMoved)
				{
					ThreadManager.StartCoroutine(closeInventoryLater());
				}
			};
		}
		RegisterForInputStyleChanges();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (windowGroup.isShowing)
		{
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
					base.xui.playerUI.windowManager.CloseAllOpenWindows();
				}
			}
		}
		if (te != null)
		{
			Vector3 vector = te.ToWorldCenterPos();
			if (vector != Vector3.zero)
			{
				float num = Constants.cCollectItemDistance + 30f;
				float sqrMagnitude = (base.xui.playerUI.entityPlayer.position - vector).sqrMagnitude;
				if (sqrMagnitude > num * num)
				{
					Log.Out("Loot Window closed at distance {0}", Mathf.Sqrt(sqrMagnitude));
					base.xui.playerUI.windowManager.CloseAllOpenWindows();
					CloseContainer(ignoreCloseSound: false);
				}
			}
		}
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && !isClosing && base.ViewComponent != null && base.ViewComponent.IsVisible && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !base.xui.playerUI.windowManager.IsInputActive()) && (base.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
		{
			standardControls.MoveAll();
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

	public void SetTileEntityChest(string _lootContainerName, ITileEntityLootable _te)
	{
		te = _te;
		lootContainerName = _lootContainerName;
		if (te == null)
		{
			return;
		}
		containerSlotsSize = te.GetContainerSize();
		windowWidth = new Vector2i(containerSlotsSize.x * lootContainer.GridCellSize.x, containerSlotsSize.y * lootContainer.GridCellSize.y).x + windowGridWidthDifference;
		lootContainer.SetSlots(te, te.items);
		ITileEntitySignable selfOrFeature = _te.GetSelfOrFeature<ITileEntitySignable>();
		if (selfOrFeature != null)
		{
			GeneratedTextManager.GetDisplayText(selfOrFeature.GetAuthoredText(), [PublicizedFrom(EAccessModifier.Private)] (string containerName) =>
			{
				if (!string.IsNullOrEmpty(containerName))
				{
					lootContainerName = containerName;
				}
				RefreshBindings(_forceAll: true);
			}, _runCallbackIfReadyNow: true, _checkBlockState: true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString);
		}
		else
		{
			RefreshBindings(_forceAll: true);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isClosing = false;
	}

	public override void OnClose()
	{
		wasReleased = false;
		activeKeyDown = false;
		UserLockMode = false;
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		entityPlayer.MinEventContext.TileEntity = te;
		if (te.EntityId == -1)
		{
			entityPlayer.MinEventContext.BlockValue = te.blockValue;
		}
		entityPlayer.FireEvent(MinEventTypes.onSelfCloseLootContainer);
	}

	public void OpenContainer()
	{
		lootContainer.SetSlots(te, te.items);
		base.OnOpen();
		te.SetUserAccessing(_bUserAccessing: true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftStickButton, "igcoLootAll", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		RefreshBindings(_forceAll: true);
		lootContainer.SelectCursorElement(_withDelay: true);
	}

	public void CloseContainer(bool ignoreCloseSound)
	{
		if (te == null)
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		if (!ignoreCloseSound)
		{
			LootContainer lootContainer = LootContainer.GetLootContainer(te.lootListName);
			if (lootContainer != null && lootContainer.soundClose != null)
			{
				Vector3 position = te.ToWorldPos().ToVector3() + Vector3.one * 0.5f;
				if (te.EntityId != -1 && GameManager.Instance.World != null)
				{
					Entity entity = GameManager.Instance.World.GetEntity(te.EntityId);
					if (entity != null)
					{
						position = entity.GetPosition();
					}
				}
				Manager.BroadcastPlayByLocalPlayer(position, lootContainer.soundClose);
			}
		}
		Vector3i blockPos = te.ToWorldPos();
		ITileEntityLootable selfOrFeature = GameManager.Instance.World.GetTileEntity(te.GetClrIdx(), blockPos).GetSelfOrFeature<ITileEntityLootable>();
		if ((selfOrFeature == null || !selfOrFeature.IsRemoving) && selfOrFeature == te)
		{
			te.SetModified();
		}
		te.SetUserAccessing(_bUserAccessing: false);
		instance.TEUnlockServer(te.GetClrIdx(), blockPos, te.EntityId);
		SetTileEntityChest("", null);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.OnClose();
	}

	public PreferenceTracker GetPreferenceTrackerFromTileEntity()
	{
		return te?.preferences;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeInventoryLater()
	{
		isClosing = true;
		yield return null;
		base.xui.playerUI.windowManager.CloseIfOpen("looting");
		isClosing = false;
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "windowWidth":
			_value = windowWidth.ToString();
			return true;
		case "lootcontainer_name":
			_value = ((!string.IsNullOrEmpty(lootContainerName)) ? lootContainerName : Localization.Get("xuiLoot"));
			return true;
		case "take_all_tooltip":
			if (base.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				_value = string.Format(Localization.Get("xuiLootTakeAllTooltip"), "[action:permanent:Reload:emptystring:KeyboardWithAngleBrackets]");
			}
			else
			{
				_value = string.Format(Localization.Get("xuiLootTakeAllTooltip"), base.xui.playerUI.playerInput.GUIActions.LeftStick.GetBindingString(_forController: true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle));
			}
			return true;
		case "buttons_visible":
			_value = (windowWidth >= 450).ToString();
			return true;
		case "container_slots":
			_value = ContainerSlotsCount.ToString();
			return true;
		case "userlockmode":
			_value = UserLockMode.ToString();
			return true;
		case "hasslotlocks":
			_value = (te?.HasSlotLocksSupport ?? false).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLockedSlotStates(PackedBoolArray _lockedSlots)
	{
		XUiC_ItemStack[] itemStackControllers = lootContainer.GetItemStackControllers();
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
			int containerSlotsCount = ContainerSlotsCount;
			PackedBoolArray packedBoolArray = _csc.LockedSlots ?? new PackedBoolArray(containerSlotsCount);
			if (packedBoolArray.Length != containerSlotsCount)
			{
				packedBoolArray.Length = containerSlotsCount;
			}
			XUiC_ItemStack[] itemStackControllers = lootContainer.GetItemStackControllers();
			for (int i = 0; i < itemStackControllers.Length && i < packedBoolArray.Length; i++)
			{
				packedBoolArray[i] = itemStackControllers[i].UserLockedSlot;
			}
			_csc.LockedSlots = packedBoolArray;
		}
	}
}
