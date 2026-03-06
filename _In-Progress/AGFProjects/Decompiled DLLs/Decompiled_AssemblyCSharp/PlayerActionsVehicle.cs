using InControl;

public class PlayerActionsVehicle : PlayerActionsBase
{
	public PlayerTwoAxisAction Move;

	public PlayerAction MoveLeft;

	public PlayerAction MoveRight;

	public PlayerAction MoveForward;

	public PlayerAction MoveBack;

	public PlayerTwoAxisAction Look;

	public PlayerAction LookLeft;

	public PlayerAction LookRight;

	public PlayerAction LookUp;

	public PlayerAction LookDown;

	public PlayerAction Turbo;

	public PlayerAction Brake;

	public PlayerAction Hop;

	public PlayerAction Activate;

	public PlayerAction ToggleFlashlight;

	public PlayerAction HonkHorn;

	public PlayerAction Menu;

	public PlayerAction Inventory;

	public PlayerAction Scoreboard;

	public PlayerAction ToggleTurnMode;

	public PlayerAction ScrollUp;

	public PlayerAction ScrollDown;

	public PlayerOneAxisAction Scroll;

	public PlayerTwoAxisAction LeftStick;

	public PlayerAction LeftStickLeft;

	public PlayerAction LeftStickRight;

	public PlayerAction LeftStickForward;

	public PlayerAction LeftStickBack;

	public PlayerActionsVehicle()
	{
		base.Name = "vehicle";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		MoveForward = CreatePlayerAction("Forward");
		MoveForward.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveForwardName", null, PlayerActionData.GroupVehicle);
		MoveBack = CreatePlayerAction("Back");
		MoveBack.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveBackName", null, PlayerActionData.GroupVehicle);
		MoveLeft = CreatePlayerAction("Left");
		MoveLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveLeftName", null, PlayerActionData.GroupVehicle);
		MoveRight = CreatePlayerAction("Right");
		MoveRight.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveRightName", null, PlayerActionData.GroupVehicle);
		Move = CreateTwoAxisPlayerAction(MoveLeft, MoveRight, MoveBack, MoveForward);
		LookLeft = CreatePlayerAction("LookLeft");
		LookLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleLookName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly);
		LookRight = CreatePlayerAction("LookRight");
		LookUp = CreatePlayerAction("LookUp");
		LookDown = CreatePlayerAction("LookDown");
		Look = CreateTwoAxisPlayerAction(LookLeft, LookRight, LookDown, LookUp);
		Turbo = CreatePlayerAction("Run");
		Turbo.UserData = new PlayerActionData.ActionUserData("inpActVehicleTurboName", null, PlayerActionData.GroupVehicle);
		Brake = CreatePlayerAction("Jump");
		Brake.UserData = new PlayerActionData.ActionUserData("inpActVehicleBrakeName", null, PlayerActionData.GroupVehicle);
		Hop = CreatePlayerAction("Crouch");
		Hop.UserData = new PlayerActionData.ActionUserData("inpActVehicleHopName", null, PlayerActionData.GroupVehicle);
		Activate = CreatePlayerAction("Activate");
		Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Activate.FirstRepeatDelay = 0.3f;
		ToggleFlashlight = CreatePlayerAction("ToggleFlashlight");
		ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActVehicleToggleLightName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly);
		HonkHorn = CreatePlayerAction("HonkHorn");
		HonkHorn.UserData = new PlayerActionData.ActionUserData("inpActHonkHornName", null, PlayerActionData.GroupVehicle);
		Menu = CreatePlayerAction("Menu");
		Menu.UserData = new PlayerActionData.ActionUserData("inpActMenuName", null, PlayerActionData.GroupVehicle);
		Inventory = CreatePlayerAction("Inventory");
		Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Scoreboard = CreatePlayerAction("Scoreboard");
		Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly);
		ToggleTurnMode = CreatePlayerAction("ToggleTurnMode");
		ToggleTurnMode.UserData = new PlayerActionData.ActionUserData("inpActToggleTurnMode", null, PlayerActionData.GroupVehicle);
		ScrollUp = CreatePlayerAction("ScrollUp");
		ScrollUp.UserData = new PlayerActionData.ActionUserData("inpActCameraZoomInName", null, PlayerActionData.GroupVehicle);
		ScrollDown = CreatePlayerAction("ScrollDown");
		ScrollDown.UserData = new PlayerActionData.ActionUserData("inpActCameraZoomOutName", null, PlayerActionData.GroupVehicle);
		Scroll = CreateOneAxisPlayerAction(ScrollDown, ScrollUp);
		LeftStickLeft = CreatePlayerAction("LeftStickLeft");
		LeftStickLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickLeft", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		LeftStickRight = CreatePlayerAction("LeftStickRight");
		LeftStickRight.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickRight", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		LeftStickForward = CreatePlayerAction("LeftStickForward");
		LeftStickForward.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickForward", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		LeftStickBack = CreatePlayerAction("LeftStickBack");
		LeftStickBack.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickBack", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: false, _doNotDisplay: true);
		LeftStick = CreateTwoAxisPlayerAction(LeftStickLeft, LeftStickRight, LeftStickBack, LeftStickForward);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);
		MoveForward.AddDefaultBinding(InputControlType.RightTrigger);
		MoveBack.AddDefaultBinding(InputControlType.LeftTrigger);
		LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
		LookRight.AddDefaultBinding(InputControlType.RightStickRight);
		LookUp.AddDefaultBinding(InputControlType.RightStickUp);
		LookDown.AddDefaultBinding(InputControlType.RightStickDown);
		Turbo.AddDefaultBinding(InputControlType.RightBumper);
		Brake.AddDefaultBinding(InputControlType.Action3);
		Hop.AddDefaultBinding(InputControlType.Action1);
		Activate.AddDefaultBinding(InputControlType.Action4);
		ToggleFlashlight.AddDefaultBinding(InputControlType.DPadLeft);
		HonkHorn.AddDefaultBinding(InputControlType.LeftBumper);
		Menu.AddDefaultBinding(InputControlType.Menu);
		Menu.AddDefaultBinding(InputControlType.Options);
		Menu.AddDefaultBinding(InputControlType.Start);
		Inventory.AddDefaultBinding(InputControlType.Action2);
		Scoreboard.AddDefaultBinding(InputControlType.View);
		Scoreboard.AddDefaultBinding(InputControlType.TouchPadButton);
		Scoreboard.AddDefaultBinding(InputControlType.Back);
		ToggleTurnMode.AddDefaultBinding(InputControlType.RightStickButton);
		ScrollUp.AddDefaultBinding(InputControlType.DPadUp);
		ScrollDown.AddDefaultBinding(InputControlType.DPadDown);
		LeftStickLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		LeftStickRight.AddDefaultBinding(InputControlType.LeftStickRight);
		LeftStickForward.AddDefaultBinding(InputControlType.LeftStickUp);
		LeftStickBack.AddDefaultBinding(InputControlType.LeftStickDown);
		ControllerRebindableActions.Clear();
		ControllerRebindableActions.Add(MoveForward);
		ControllerRebindableActions.Add(MoveBack);
		ControllerRebindableActions.Add(Hop);
		ControllerRebindableActions.Add(Brake);
		ControllerRebindableActions.Add(Inventory);
		ControllerRebindableActions.Add(Activate);
		ControllerRebindableActions.Add(Turbo);
		ControllerRebindableActions.Add(HonkHorn);
		ControllerRebindableActions.Add(ToggleFlashlight);
		ControllerRebindableActions.Add(ToggleTurnMode);
		ControllerRebindableActions.Add(ScrollUp);
		ControllerRebindableActions.Add(ScrollDown);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		base.ListenOptions.IncludeKeys = true;
		base.ListenOptions.IncludeMouseButtons = true;
		base.ListenOptions.IncludeMouseScrollWheel = true;
		MoveLeft.AddDefaultBinding(Key.A);
		MoveRight.AddDefaultBinding(Key.D);
		MoveForward.AddDefaultBinding(Key.W);
		MoveBack.AddDefaultBinding(Key.S);
		LookLeft.AddDefaultBinding(Mouse.NegativeX);
		LookRight.AddDefaultBinding(Mouse.PositiveX);
		LookUp.AddDefaultBinding(Mouse.PositiveY);
		LookDown.AddDefaultBinding(Mouse.NegativeY);
		Turbo.AddDefaultBinding(Key.LeftShift);
		Brake.AddDefaultBinding(Key.Space);
		Hop.AddDefaultBinding(Key.C);
		HonkHorn.AddDefaultBinding(Key.X);
		Menu.AddDefaultBinding(Key.Escape);
		ToggleTurnMode.AddDefaultBinding(Mouse.LeftButton);
		ScrollUp.AddDefaultBinding(Mouse.PositiveScrollWheel);
		ScrollDown.AddDefaultBinding(Mouse.NegativeScrollWheel);
	}
}
