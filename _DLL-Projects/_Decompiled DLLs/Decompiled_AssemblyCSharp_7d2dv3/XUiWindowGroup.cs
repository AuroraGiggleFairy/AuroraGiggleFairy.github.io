using System.Collections.Generic;
using Platform;

public class XUiWindowGroup : GUIWindow
{
	public enum EHasActionSetFor
	{
		Both,
		OnlyController,
		OnlyKeyboard,
		None
	}

	public readonly XUi xui;

	public XUiController Controller;

	public readonly bool LeftPanelVAlignTop = true;

	public readonly bool RightPanelVAlignTop = true;

	public readonly int StackPanelYOffset = 457;

	public readonly int StackPanelPadding = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool openBackpackOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool closeCompassOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string defaultSelectedView;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EHasActionSetFor hasActionSetFor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasActionSetThisOpen = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiV_Window> windows = new List<XUiV_Window>();

	public readonly IReadOnlyList<XUiV_Window> Windows;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cleanedUp;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Initialized
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public XUiWindowGroup(XUi _xui, string _id, EHasActionSetFor _hasActionSetFor, string _defaultSelectedName, int _stackPanelYOffset, int _stackPanelPadding, bool _openBackpackOnOpen, bool _closeCompassOnOpen)
		: base(_id)
	{
		Windows = windows.AsReadOnly();
		xui = _xui;
		playerUI = xui.playerUI;
		windowManager = playerUI.windowManager;
		hasActionSetFor = _hasActionSetFor;
		defaultSelectedView = _defaultSelectedName;
		if (_stackPanelYOffset != int.MinValue)
		{
			StackPanelYOffset = _stackPanelYOffset;
		}
		if (_stackPanelPadding != int.MinValue)
		{
			StackPanelPadding = _stackPanelPadding;
		}
		openBackpackOnOpen = _openBackpackOnOpen;
		closeCompassOnOpen = _closeCompassOnOpen;
	}

	public void Init()
	{
		if (Initialized)
		{
			return;
		}
		Controller.AutoBindComponents();
		Controller.Init();
		Controller.AutoBindEvents();
		foreach (XUiController child in Controller.Children)
		{
			if (child.ViewComponent != null)
			{
				windows.Add((XUiV_Window)child.ViewComponent);
				child.ViewComponent.IsVisible = false;
			}
		}
		windowManager.Add(this);
		Initialized = true;
	}

	public override void Cleanup()
	{
		if (!cleanedUp)
		{
			cleanedUp = true;
			CursorControllerAbs cursorController = xui.playerUI.CursorController;
			if (cursorController.navigationTarget != null && cursorController.navigationTarget.Controller.IsChildOf(Controller))
			{
				cursorController.SetNavigationTarget(null);
			}
			Controller.Cleanup();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Controller.OnOpen();
		applyDefaultSelectedView();
		if (closeCompassOnOpen)
		{
			windowManager.Close("compass");
		}
		if (openBackpackOnOpen && GameManager.Instance != null)
		{
			windowManager.Open("backpack", _bModal: false);
		}
		xui.RecenterWindowGroup(this);
		hasActionSetThisOpen = hasActionSetFor switch
		{
			EHasActionSetFor.Both => true, 
			EHasActionSetFor.OnlyController => PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard, 
			EHasActionSetFor.OnlyKeyboard => PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard, 
			EHasActionSetFor.None => false, 
			_ => hasActionSetThisOpen, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyDefaultSelectedView()
	{
		if (string.IsNullOrEmpty(defaultSelectedView))
		{
			return;
		}
		if (defaultSelectedView.StartsWith("bp."))
		{
			XUiC_BackpackWindow.defaultSelectedElement = defaultSelectedView.Remove(0, 3);
			return;
		}
		XUiController childById = Controller.GetChildById(defaultSelectedView);
		if (childById != null)
		{
			childById.SelectCursorElement(_withDelay: true);
		}
		else
		{
			Log.Warning("Could not find selectable element " + defaultSelectedView + " in WindowGroup " + Id);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.DragAndDropWindow?.PlaceItemBackInInventory();
		Controller.OnClose();
		if (openBackpackOnOpen && GameManager.Instance != null)
		{
			windowManager.Close("backpack");
		}
		if (xui.ToolTipWindow != null)
		{
			xui.ToolTipWindow.ToolTip = "";
		}
		xui.PopupMenuWindow?.Close();
	}

	public override bool HasActionSet()
	{
		return hasActionSetThisOpen;
	}

	public bool HasStackPanelWindows()
	{
		if (Controller == null)
		{
			return false;
		}
		foreach (XUiV_Window window in windows)
		{
			if (window.IsInStackPanel)
			{
				return true;
			}
		}
		return false;
	}
}
