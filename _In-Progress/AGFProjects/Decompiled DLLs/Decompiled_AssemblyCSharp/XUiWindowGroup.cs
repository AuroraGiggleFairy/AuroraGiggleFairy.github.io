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

	public XUiController Controller;

	public bool LeftPanelVAlignTop = true;

	public bool RightPanelVAlignTop = true;

	public bool UseStackPanelAlignment;

	public bool BoundsCalculated;

	public int StackPanelYOffset = 457;

	public int StackPanelPadding = 9;

	public bool openBackpackOnOpen;

	public bool closeCompassOnOpen;

	public string defaultSelectedView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi mXUi;

	[PublicizedFrom(EAccessModifier.Private)]
	public EHasActionSetFor hasActionSetFor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasActionSetThisOpen = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	public string ID => id;

	public XUi xui
	{
		get
		{
			return mXUi;
		}
		set
		{
			mXUi = value;
			playerUI = mXUi.playerUI;
			windowManager = playerUI.windowManager;
			nguiWindowManager = playerUI.nguiWindowManager;
		}
	}

	public bool Initialized => initialized;

	public XUiWindowGroup(string _id, EHasActionSetFor _hasActionSetFor = EHasActionSetFor.Both, string _defaultSelectedName = "")
		: base(_id)
	{
		hasActionSetFor = _hasActionSetFor;
		defaultSelectedView = _defaultSelectedName;
	}

	public void Init()
	{
		if (initialized)
		{
			return;
		}
		Controller.Init();
		for (int i = 0; i < Controller.Children.Count; i++)
		{
			XUiController xUiController = Controller.Children[i];
			if (xUiController.ViewComponent != null)
			{
				xUiController.ViewComponent.IsVisible = false;
				string name = xUiController.ViewComponent.UiTransform.parent.name;
				UseStackPanelAlignment |= name.EqualsCaseInsensitive("left") || name.EqualsCaseInsensitive("right");
			}
		}
		windowManager.Add(id, this);
		initialized = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Controller.OnOpen();
		if (!string.IsNullOrEmpty(defaultSelectedView))
		{
			if (defaultSelectedView.StartsWith("bp."))
			{
				XUiC_BackpackWindow.defaultSelectedElement = defaultSelectedView.Remove(0, 3);
			}
			else
			{
				XUiController childById = Controller.GetChildById(defaultSelectedView);
				if (childById != null)
				{
					childById.SelectCursorElement(_withDelay: true);
				}
				else
				{
					Log.Warning("Could not find selectable element {0} in WindowGroup {1}", defaultSelectedView, ID);
				}
			}
		}
		if (closeCompassOnOpen)
		{
			windowManager.CloseIfOpen("compass");
		}
		if (openBackpackOnOpen && GameManager.Instance != null)
		{
			windowManager.OpenIfNotOpen("backpack", _bModal: false);
		}
		xui.RecenterWindowGroup(this);
		switch (hasActionSetFor)
		{
		case EHasActionSetFor.Both:
			hasActionSetThisOpen = true;
			break;
		case EHasActionSetFor.OnlyController:
			hasActionSetThisOpen = PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard;
			break;
		case EHasActionSetFor.OnlyKeyboard:
			hasActionSetThisOpen = PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard;
			break;
		case EHasActionSetFor.None:
			hasActionSetThisOpen = false;
			break;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (xui.dragAndDrop != null)
		{
			xui.dragAndDrop.PlaceItemBackInInventory();
		}
		Controller.OnClose();
		if (openBackpackOnOpen && GameManager.Instance != null)
		{
			windowManager.CloseIfOpen("backpack");
		}
		if (xui.currentToolTip != null)
		{
			xui.currentToolTip.ToolTip = "";
		}
		if (xui.currentPopupMenu != null)
		{
			xui.currentPopupMenu.ClearItems();
		}
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
		foreach (XUiController child in Controller.Children)
		{
			if (child.ViewComponent is XUiV_Window { IsInStackpanel: not false })
			{
				return true;
			}
		}
		return false;
	}
}
