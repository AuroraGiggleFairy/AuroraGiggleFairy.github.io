using System.Collections;
using Audio;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector te;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorContainer container;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls controls;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSlotsSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowGridWidthDifference;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lootLabel;

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

	public override void Init()
	{
		base.Init();
		container = GetChildByType<XUiC_DewCollectorContainer>();
		controls = GetChildByType<XUiC_ContainerStandardControls>();
		if (controls != null)
		{
			controls.SortPressed = [PublicizedFrom(EAccessModifier.Private)] (PackedBoolArray _ignoredSlots) =>
			{
				ItemStack[] array = StackSortUtil.CombineAndSortStacks(te.items, 0, _ignoredSlots);
				for (int i = 0; i < array.Length; i++)
				{
					te.UpdateSlot(i, array[i]);
				}
				te.SetModified();
			};
			controls.MoveAllowed = [PublicizedFrom(EAccessModifier.Private)] (out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory) =>
			{
				_parentWindow = this;
				_grid = container;
				_inventory = base.xui.PlayerInventory;
				return true;
			};
			controls.MoveAllDone = [PublicizedFrom(EAccessModifier.Private)] (bool _allMoved, bool _anyMoved) =>
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
		lootLabel = GetChildById("lootName").ViewComponent as XUiV_Label;
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
		if ((!(GameManager.Instance == null) || GameManager.Instance.World != null) && !isClosing && base.ViewComponent != null && base.ViewComponent.IsVisible && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !base.xui.playerUI.windowManager.IsInputActive()) && (base.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
		{
			controls.MoveAll();
		}
	}

	public void SetTileEntity(TileEntityCollector _te)
	{
		te = _te;
		if (te != null)
		{
			lootLabel.Text = Localization.Get((te.blockValue.Block as BlockCollector).LootLabelKey);
			containerSlotsSize = te.GetContainerSize();
			windowWidth = new Vector2i(containerSlotsSize.x * container.GridCellSize.x, containerSlotsSize.y * container.GridCellSize.y).x + windowGridWidthDifference;
			te.HandleUpdate(GameManager.Instance.World);
			container.SetSlots(te, te.GetItems());
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
		SetTileEntity(null);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.OnClose();
	}

	public void OpenContainer()
	{
		container.SetSlots(te, te.GetItems());
		base.OnOpen();
		te.SetUserAccessing(_bUserAccessing: true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftStickButton, "igcoLootAll", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuLoot);
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeInventoryLater()
	{
		isClosing = true;
		yield return null;
		base.xui.playerUI.windowManager.CloseIfOpen("dewcollector");
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
			_value = Localization.Get("xuiDewCollector");
			return true;
		case "take_all_tooltip":
			_value = string.Format(Localization.Get("xuiLootTakeAllTooltip"), "[action:permanent:Reload:emptystring:KeyboardWithAngleBrackets]");
			return true;
		case "buttons_visible":
			_value = (windowWidth >= 450).ToString();
			return true;
		case "container_slots":
			_value = ContainerSlotsCount.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
