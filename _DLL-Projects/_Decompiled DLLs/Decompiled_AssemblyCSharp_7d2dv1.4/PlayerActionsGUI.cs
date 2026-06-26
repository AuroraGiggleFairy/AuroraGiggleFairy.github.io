using InControl;

public class PlayerActionsGUI : PlayerActionsBase
{
	public PlayerAction Left;

	public PlayerAction Right;

	public PlayerAction Up;

	public PlayerAction Down;

	public PlayerTwoAxisAction Look;

	public PlayerAction DPad_Left;

	public PlayerAction DPad_Right;

	public PlayerAction DPad_Up;

	public PlayerAction DPad_Down;

	public PlayerAction Submit;

	public PlayerAction Cancel;

	public PlayerAction HalfStack;

	public PlayerAction Inspect;

	public PlayerAction FocusSearch;

	public PlayerAction LeftClick;

	public PlayerAction RightClick;

	public PlayerAction CameraLeft;

	public PlayerAction CameraRight;

	public PlayerAction CameraUp;

	public PlayerAction CameraDown;

	public PlayerTwoAxisAction Camera;

	public PlayerAction WindowPagingLeft;

	public PlayerAction WindowPagingRight;

	public PlayerAction PageUp;

	public PlayerAction PageDown;

	public PlayerAction RightStick;

	public PlayerAction LeftStick;

	public PlayerAction BackButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float LOWER_STICK_DEADZONE = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float UPPER_STICK_DEADZONE = 0.9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float LOWER_LEFT_STICKDEADZONE = 0.8f;

	public PlayerAction NavUp;

	public PlayerAction NavDown;

	public PlayerAction NavLeft;

	public PlayerAction NavRight;

	public PlayerTwoAxisAction Nav;

	public PlayerOneAxisAction TriggerAxis;

	public PlayerAction scrollUp;

	public PlayerAction scrollDown;

	public PlayerOneAxisAction scroll;

	public PlayerAction ActionUp;

	public PlayerAction ActionDown;

	public PlayerAction ActionLeft;

	public PlayerAction ActionRight;

	public PlayerAction Apply;

	public PlayerActionsGUI()
	{
		base.Name = "gui";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		Left = CreatePlayerAction("GUI Left");
		Left.UserData = new PlayerActionData.ActionUserData("inpActGuiCursor", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Right = CreatePlayerAction("GUI Right");
		Right.UserData = new PlayerActionData.ActionUserData("inpActGuiCursor", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.None);
		Up = CreatePlayerAction("GUI Up");
		Up.UserData = new PlayerActionData.ActionUserData("inpActGuiCursor", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.None);
		Down = CreatePlayerAction("GUI Down");
		Down.UserData = new PlayerActionData.ActionUserData("inpActGuiCursor", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.None);
		Look = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
		LeftClick = CreatePlayerAction("GUI Left Click");
		LeftClick.UserData = new PlayerActionData.ActionUserData("inpActGuiLeftclick", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		RightClick = CreatePlayerAction("GUI RightClick");
		RightClick.UserData = new PlayerActionData.ActionUserData("inpActGuiRightclick", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		Submit = CreatePlayerAction("GUI Submit");
		Submit.UserData = new PlayerActionData.ActionUserData("inpActUiSelectTakeFullName", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Cancel = CreatePlayerAction("GUI Cancel");
		Cancel.UserData = new PlayerActionData.ActionUserData("inpActUiCancelName", null, PlayerActionData.GroupUI);
		HalfStack = CreatePlayerAction("GUI HalfStack");
		HalfStack.UserData = new PlayerActionData.ActionUserData("inpActTakeHalfName", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Inspect = CreatePlayerAction("GUI Inspect");
		Inspect.UserData = new PlayerActionData.ActionUserData("inpActInspectName", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly);
		FocusSearch = CreatePlayerAction("GUI FocusSearch");
		FocusSearch.UserData = new PlayerActionData.ActionUserData("inpActFocusSearchName", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		DPad_Left = CreatePlayerAction("GUI D-Pad Left");
		DPad_Left.UserData = new PlayerActionData.ActionUserData("inpActActionHotkey1Name", null, PlayerActionData.GroupUI);
		DPad_Right = CreatePlayerAction("GUI D-Pad Right");
		DPad_Right.UserData = new PlayerActionData.ActionUserData("inpActActionHotkey2Name", null, PlayerActionData.GroupUI);
		DPad_Up = CreatePlayerAction("GUI D-Pad Up");
		DPad_Up.UserData = new PlayerActionData.ActionUserData("inpActActionHotkey3Name", null, PlayerActionData.GroupUI);
		DPad_Down = CreatePlayerAction("GUI D-Pad Down");
		DPad_Down.UserData = new PlayerActionData.ActionUserData("inpActActionHotkey4Name", null, PlayerActionData.GroupUI);
		CameraLeft = CreatePlayerAction("GUI Camera Left");
		CameraLeft.UserData = new PlayerActionData.ActionUserData("inpActPageDownName", null, PlayerActionData.GroupUI);
		CameraRight = CreatePlayerAction("GUI Camera Right");
		CameraRight.UserData = new PlayerActionData.ActionUserData("inpActPageUpName", null, PlayerActionData.GroupUI);
		CameraUp = CreatePlayerAction("GUI Camera Up");
		CameraUp.UserData = new PlayerActionData.ActionUserData("inpActZoomInName", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly);
		CameraDown = CreatePlayerAction("GUI Camera Down");
		CameraDown.UserData = new PlayerActionData.ActionUserData("inpActZoomOutName", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Camera = CreateTwoAxisPlayerAction(CameraLeft, CameraRight, CameraDown, CameraUp);
		WindowPagingLeft = CreatePlayerAction("GUI Window Paging Up");
		WindowPagingLeft.UserData = new PlayerActionData.ActionUserData("inpActUiTabLeftName", null, PlayerActionData.GroupUI);
		WindowPagingRight = CreatePlayerAction("GUI Window Paging Down");
		WindowPagingRight.UserData = new PlayerActionData.ActionUserData("inpActUiTabRightName", null, PlayerActionData.GroupUI);
		PageDown = CreatePlayerAction("GUI Page Down");
		PageDown.UserData = new PlayerActionData.ActionUserData("inpActCategoryLeftName", null, PlayerActionData.GroupUI);
		PageUp = CreatePlayerAction("GUI Page Up");
		PageUp.UserData = new PlayerActionData.ActionUserData("inpActCategoryRightName", null, PlayerActionData.GroupUI);
		TriggerAxis = CreateOneAxisPlayerAction(PageUp, PageDown);
		BackButton = CreatePlayerAction("GUI Back Button");
		BackButton.UserData = new PlayerActionData.ActionUserData("inpActBackButton", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		RightStick = CreatePlayerAction("GUI Window RightStick In");
		RightStick.UserData = new PlayerActionData.ActionUserData("inpActQuickMoveName", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly);
		LeftStick = CreatePlayerAction("GUI Window LeftStick In");
		LeftStick.UserData = new PlayerActionData.ActionUserData("inpActTakeAllName", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly);
		NavUp = CreatePlayerAction("GUI Window Navigate Up");
		NavUp.UserData = new PlayerActionData.ActionUserData("inpActNavUp", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		NavDown = CreatePlayerAction("GUI Window Navigate Down");
		NavDown.UserData = new PlayerActionData.ActionUserData("inpActNavDown", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		NavLeft = CreatePlayerAction("GUI Window Navigate Left");
		NavLeft.UserData = new PlayerActionData.ActionUserData("inpActNavLeft", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		NavRight = CreatePlayerAction("GUI Window Navigate Right");
		NavRight.UserData = new PlayerActionData.ActionUserData("inpActNavRight", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		Nav = CreateTwoAxisPlayerAction(NavLeft, NavRight, NavDown, NavUp);
		scrollUp = CreatePlayerAction("GUI Scroll Up");
		scrollUp.UserData = new PlayerActionData.ActionUserData("inpScrollUp", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		scrollDown = CreatePlayerAction("GUI Scroll Down");
		scrollDown.UserData = new PlayerActionData.ActionUserData("inpscrollDown", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		scroll = CreateOneAxisPlayerAction(scrollDown, scrollUp);
		Apply = CreatePlayerAction("GUI Apply");
		Apply.UserData = new PlayerActionData.ActionUserData("inpActUiApply", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		ActionUp = CreatePlayerAction("GUI Action Up");
		ActionUp.UserData = new PlayerActionData.ActionUserData("inpActUp", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		ActionDown = CreatePlayerAction("GUI Action Down");
		ActionDown.UserData = new PlayerActionData.ActionUserData("inpActDown", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		ActionLeft = CreatePlayerAction("GUI Action Left");
		ActionLeft.UserData = new PlayerActionData.ActionUserData("inpActLeft", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		ActionRight = CreatePlayerAction("GUI Action Right");
		ActionRight.UserData = new PlayerActionData.ActionUserData("inpActRight", null, PlayerActionData.GroupUI, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		Left.Raw = false;
		Right.Raw = false;
		Up.Raw = false;
		Down.Raw = false;
		CameraUp.Raw = false;
		CameraDown.Raw = false;
		CameraLeft.StateThreshold = 0.25f;
		CameraRight.StateThreshold = 0.25f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		Left.AddDefaultBinding(InputControlType.LeftStickLeft);
		Right.AddDefaultBinding(InputControlType.LeftStickRight);
		Up.AddDefaultBinding(InputControlType.LeftStickUp);
		Down.AddDefaultBinding(InputControlType.LeftStickDown);
		DPad_Left.AddDefaultBinding(InputControlType.DPadLeft);
		DPad_Right.AddDefaultBinding(InputControlType.DPadRight);
		DPad_Up.AddDefaultBinding(InputControlType.DPadUp);
		DPad_Down.AddDefaultBinding(InputControlType.DPadDown);
		CameraLeft.AddDefaultBinding(InputControlType.RightStickLeft);
		CameraRight.AddDefaultBinding(InputControlType.RightStickRight);
		CameraUp.AddDefaultBinding(InputControlType.RightStickUp);
		CameraDown.AddDefaultBinding(InputControlType.RightStickDown);
		Submit.AddDefaultBinding(InputControlType.Action1);
		Cancel.AddDefaultBinding(InputControlType.Action2);
		HalfStack.AddDefaultBinding(InputControlType.Action3);
		Inspect.AddDefaultBinding(InputControlType.Action4);
		Apply.AddDefaultBinding(InputControlType.Start);
		Apply.AddDefaultBinding(InputControlType.Menu);
		Apply.AddDefaultBinding(InputControlType.Options);
		Apply.AddDefaultBinding(InputControlType.Plus);
		WindowPagingLeft.AddDefaultBinding(InputControlType.LeftBumper);
		WindowPagingRight.AddDefaultBinding(InputControlType.RightBumper);
		PageUp.AddDefaultBinding(InputControlType.RightTrigger);
		PageDown.AddDefaultBinding(InputControlType.LeftTrigger);
		RightStick.AddDefaultBinding(InputControlType.RightStickButton);
		LeftStick.AddDefaultBinding(InputControlType.LeftStickButton);
		NavUp.AddDefaultBinding(InputControlType.DPadUp);
		NavDown.AddDefaultBinding(InputControlType.DPadDown);
		NavLeft.AddDefaultBinding(InputControlType.DPadLeft);
		NavRight.AddDefaultBinding(InputControlType.DPadRight);
		scrollUp.AddDefaultBinding(InputControlType.RightStickUp);
		scrollDown.AddDefaultBinding(InputControlType.RightStickDown);
		BackButton.AddDefaultBinding(InputControlType.Back);
		BackButton.AddDefaultBinding(InputControlType.View);
		BackButton.AddDefaultBinding(InputControlType.TouchPadButton);
		BackButton.AddDefaultBinding(InputControlType.Minus);
		BackButton.AddDefaultBinding(InputControlType.Select);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		Left.AddDefaultBinding(Key.LeftArrow);
		Right.AddDefaultBinding(Key.RightArrow);
		Up.AddDefaultBinding(Key.UpArrow);
		Down.AddDefaultBinding(Key.DownArrow);
		DPad_Left.AddDefaultBinding(Key.A);
		DPad_Right.AddDefaultBinding(Key.S);
		DPad_Up.AddDefaultBinding(Key.W);
		DPad_Down.AddDefaultBinding(Key.D);
		Cancel.AddDefaultBinding(Key.Escape);
		FocusSearch.AddDefaultBinding(Key.F);
		LeftClick.AddDefaultBinding(Mouse.LeftButton);
		RightClick.AddDefaultBinding(Mouse.RightButton);
		scrollUp.AddDefaultBinding(Mouse.PositiveScrollWheel);
		scrollDown.AddDefaultBinding(Mouse.NegativeScrollWheel);
		scrollUp.AddDefaultBinding(Key.UpArrow);
		scrollDown.AddDefaultBinding(Key.DownArrow);
	}
}
