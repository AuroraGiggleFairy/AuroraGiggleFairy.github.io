using System.Collections.Generic;
using InControl;

public class PlayerActionsLocal : PlayerActionsBase
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

	public PlayerAction Run;

	public PlayerAction Jump;

	public PlayerAction Crouch;

	public PlayerAction ToggleCrouch;

	public PlayerAction Activate;

	public PlayerAction Drop;

	public PlayerAction Swap;

	public PlayerAction Reload;

	public PlayerAction Primary;

	public PlayerAction Secondary;

	public PlayerAction ToggleFlashlight;

	public PlayerAction God;

	public PlayerAction Fly;

	public PlayerAction Invisible;

	public PlayerAction IncSpeed;

	public PlayerAction DecSpeed;

	public PlayerAction GodAlternate;

	public PlayerAction TeleportAlternate;

	public PlayerAction CameraChange;

	public PlayerAction CameraFunction;

	public PlayerAction SelectionFill;

	public PlayerAction SelectionClear;

	public PlayerAction SelectionSet;

	public PlayerAction SelectionRotate;

	public PlayerAction SelectionDelete;

	public PlayerAction SelectionMoveMode;

	public PlayerAction FocusCopyBlock;

	public PlayerAction Prefab;

	public PlayerAction DetachCamera;

	public PlayerAction ToggleDCMove;

	public PlayerAction LockFreeCamera;

	public PlayerAction DensityM1;

	public PlayerAction DensityP1;

	public PlayerAction DensityM10;

	public PlayerAction DensityP10;

	public PlayerAction ScrollUp;

	public PlayerAction ScrollDown;

	public PlayerOneAxisAction Scroll;

	public PlayerAction Menu;

	public PlayerAction Inventory;

	public PlayerAction Scoreboard;

	public PlayerAction InventorySlot1;

	public PlayerAction InventorySlot2;

	public PlayerAction InventorySlot3;

	public PlayerAction InventorySlot4;

	public PlayerAction InventorySlot5;

	public PlayerAction InventorySlot6;

	public PlayerAction InventorySlot7;

	public PlayerAction InventorySlot8;

	public PlayerAction InventorySlot9;

	public PlayerAction InventorySlot10;

	public PlayerAction InventorySlotLeft;

	public PlayerAction InventorySlotRight;

	public PlayerAction AiFreeze;

	public PlayerAction QuickMenu;

	public float leftStickDeadzone = 0.1f;

	public float rightStickDeadzone = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlayerAction> InventoryActions = new List<PlayerAction>();

	public eControllerJoystickLayout joystickLayout;

	public int InventorySlotWasPressed
	{
		get
		{
			for (int i = 0; i < InventoryActions.Count; i++)
			{
				if (InventoryActions[i].WasPressed)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public int InventorySlotWasReleased
	{
		get
		{
			for (int i = 0; i < InventoryActions.Count; i++)
			{
				if (InventoryActions[i].WasReleased)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public int InventorySlotIsPressed
	{
		get
		{
			for (int i = 0; i < InventoryActions.Count; i++)
			{
				if (InventoryActions[i].IsPressed)
				{
					return i;
				}
			}
			return -1;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsGUI GUIActions { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsVehicle VehicleActions { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsPermanent PermanentActions { get; }

	public PlayerActionsLocal()
	{
		base.Name = "local";
		GUIActions = new PlayerActionsGUI
		{
			Enabled = false
		};
		PermanentActions = new PlayerActionsPermanent
		{
			Enabled = true
		};
		VehicleActions = new PlayerActionsVehicle
		{
			Enabled = false
		};
		base.UserData = new PlayerActionData.ActionSetUserData(PermanentActions);
		VehicleActions.UserData = new PlayerActionData.ActionSetUserData(PermanentActions);
		GUIActions.UserData = new PlayerActionData.ActionSetUserData(PermanentActions);
		PermanentActions.UserData = new PlayerActionData.ActionSetUserData(this, VehicleActions, GUIActions);
		InventoryActions.Add(InventorySlot1);
		InventoryActions.Add(InventorySlot2);
		InventoryActions.Add(InventorySlot3);
		InventoryActions.Add(InventorySlot4);
		InventoryActions.Add(InventorySlot5);
		InventoryActions.Add(InventorySlot6);
		InventoryActions.Add(InventorySlot7);
		InventoryActions.Add(InventorySlot8);
		InventoryActions.Add(InventorySlot9);
		InventoryActions.Add(InventorySlot10);
		InputManager.OnActiveDeviceChanged += [PublicizedFrom(EAccessModifier.Private)] (InputDevice inputDevice) =>
		{
			UpdateDeadzones();
		};
		UpdateDeadzones();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		MoveForward = CreatePlayerAction("Forward");
		MoveForward.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveForwardName", null, PlayerActionData.GroupPlayerControl);
		MoveBack = CreatePlayerAction("Back");
		MoveBack.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveBackName", null, PlayerActionData.GroupPlayerControl);
		MoveLeft = CreatePlayerAction("Left");
		MoveLeft.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveLeftName", null, PlayerActionData.GroupPlayerControl);
		MoveRight = CreatePlayerAction("Right");
		MoveRight.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveRightName", null, PlayerActionData.GroupPlayerControl);
		Move = CreateTwoAxisPlayerAction(MoveLeft, MoveRight, MoveBack, MoveForward);
		LookLeft = CreatePlayerAction("LookLeft");
		LookLeft.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookLeft", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		LookRight = CreatePlayerAction("LookRight");
		LookRight.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookRight", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		LookUp = CreatePlayerAction("LookUp");
		LookUp.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookUp", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		LookDown = CreatePlayerAction("LookDown");
		LookDown.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookDown", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Look = CreateTwoAxisPlayerAction(LookLeft, LookRight, LookDown, LookUp);
		Primary = CreatePlayerAction("Primary");
		Primary.UserData = new PlayerActionData.ActionUserData("inpActPlayerPrimaryName", null, PlayerActionData.GroupPlayerControl);
		Primary.StateThreshold = 0.25f;
		Secondary = CreatePlayerAction("Secondary");
		Secondary.UserData = new PlayerActionData.ActionUserData("inpActPlayerSecondaryName", null, PlayerActionData.GroupPlayerControl);
		Secondary.StateThreshold = 0.25f;
		Run = CreatePlayerAction("Run");
		Run.UserData = new PlayerActionData.ActionUserData("inpActPlayerRunName", null, PlayerActionData.GroupPlayerControl);
		Jump = CreatePlayerAction("Jump");
		Jump.UserData = new PlayerActionData.ActionUserData("inpActPlayerJumpName", null, PlayerActionData.GroupPlayerControl);
		Crouch = CreatePlayerAction("Crouch");
		Crouch.UserData = new PlayerActionData.ActionUserData("inpActPlayerCrouchName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		ToggleCrouch = CreatePlayerAction("ToggleCrouch");
		ToggleCrouch.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleCrouchName", null, PlayerActionData.GroupPlayerControl);
		ScrollUp = CreatePlayerAction("ScrollUp");
		ScrollUp.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomInName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: true, _allowMultipleRebindings: true);
		ScrollDown = CreatePlayerAction("ScrollDown");
		ScrollDown.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomOutName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: true, _allowMultipleRebindings: true);
		Scroll = CreateOneAxisPlayerAction(ScrollDown, ScrollUp);
		Activate = CreatePlayerAction("Activate");
		Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Activate.FirstRepeatDelay = 0.3f;
		Drop = CreatePlayerAction("Drop");
		Drop.UserData = new PlayerActionData.ActionUserData("inpActPlayerDropName", null, PlayerActionData.GroupPlayerControl);
		Swap = CreatePlayerAction("Swap");
		Swap.UserData = new PlayerActionData.ActionUserData("inpActSwapName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Reload = CreatePlayerAction("Reload");
		Reload.UserData = new PlayerActionData.ActionUserData("inpActPlayerReloadName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Reload.FirstRepeatDelay = 0.3f;
		ToggleFlashlight = CreatePlayerAction("ToggleFlashlight");
		ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleFlashlightName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		ToggleFlashlight.FirstRepeatDelay = 0.3f;
		InventorySlot1 = CreatePlayerAction("Inventory1");
		InventorySlot1.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot1Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot2 = CreatePlayerAction("Inventory2");
		InventorySlot2.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot2Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot3 = CreatePlayerAction("Inventory3");
		InventorySlot3.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot3Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot4 = CreatePlayerAction("Inventory4");
		InventorySlot4.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot4Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot5 = CreatePlayerAction("Inventory5");
		InventorySlot5.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot5Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot6 = CreatePlayerAction("Inventory6");
		InventorySlot6.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot6Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot7 = CreatePlayerAction("Inventory7");
		InventorySlot7.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot7Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot8 = CreatePlayerAction("Inventory8");
		InventorySlot8.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot8Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot9 = CreatePlayerAction("Inventory9");
		InventorySlot9.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot9Name", null, PlayerActionData.GroupToolbelt);
		InventorySlot10 = CreatePlayerAction("Inventory10");
		InventorySlot10.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot10Name", null, PlayerActionData.GroupToolbelt);
		InventorySlotLeft = CreatePlayerAction("InventorySelectLeft");
		InventorySlotLeft.UserData = new PlayerActionData.ActionUserData("inpActInventorySlotLeftName", null, PlayerActionData.GroupToolbelt);
		InventorySlotRight = CreatePlayerAction("InventorySelectRight");
		InventorySlotRight.UserData = new PlayerActionData.ActionUserData("inpActInventorySlotRightName", null, PlayerActionData.GroupToolbelt);
		Menu = CreatePlayerAction("Menu");
		Menu.UserData = new PlayerActionData.ActionUserData("inpActMenuName", null, PlayerActionData.GroupMenu);
		God = CreatePlayerAction("God");
		God.UserData = new PlayerActionData.ActionUserData("inpActGodModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		Fly = CreatePlayerAction("Fly");
		Fly.UserData = new PlayerActionData.ActionUserData("inpActFlyModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		Invisible = CreatePlayerAction("Invisible");
		Invisible.UserData = new PlayerActionData.ActionUserData("inpActInvisibleModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		IncSpeed = CreatePlayerAction("IncSpeed");
		IncSpeed.UserData = new PlayerActionData.ActionUserData("inpActIncGodSpeedName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		DecSpeed = CreatePlayerAction("DecSpeed");
		DecSpeed.UserData = new PlayerActionData.ActionUserData("inpActDecGodSpeedName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		GodAlternate = CreatePlayerAction("GodAlternate");
		GodAlternate.UserData = new PlayerActionData.ActionUserData("inpActGodAlternateModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		TeleportAlternate = CreatePlayerAction("TeleportAlternate");
		TeleportAlternate.UserData = new PlayerActionData.ActionUserData("inpActTeleportAlternateModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		CameraChange = CreatePlayerAction("CameraChange");
		CameraChange.UserData = new PlayerActionData.ActionUserData("inpActCameraChangeName", null, PlayerActionData.GroupPlayerControl);
		CameraFunction = CreatePlayerAction("FlipCamera");
		CameraFunction.UserData = new PlayerActionData.ActionUserData("inpActFlipCameraName", null, PlayerActionData.GroupPlayerControl);
		DetachCamera = CreatePlayerAction("DetachCamera");
		DetachCamera.UserData = new PlayerActionData.ActionUserData("inpActDetachCameraName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		ToggleDCMove = CreatePlayerAction("ToggleDCMove");
		ToggleDCMove.UserData = new PlayerActionData.ActionUserData("inpActToggleDCMoveName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		LockFreeCamera = CreatePlayerAction("LockFreeCamera");
		LockFreeCamera.UserData = new PlayerActionData.ActionUserData("inpActLockFreeCameraName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		SelectionFill = CreatePlayerAction("SelectionFill");
		SelectionFill.UserData = new PlayerActionData.ActionUserData("inpActSelectionFillName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		SelectionClear = CreatePlayerAction("SelectionClear");
		SelectionClear.UserData = new PlayerActionData.ActionUserData("inpActSelectionClearName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		SelectionSet = CreatePlayerAction("SelectionSet");
		SelectionSet.UserData = new PlayerActionData.ActionUserData("inpActSelectionSetName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		SelectionRotate = CreatePlayerAction("SelectionRotate");
		SelectionRotate.UserData = new PlayerActionData.ActionUserData("inpActSelectionRotateName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		SelectionDelete = CreatePlayerAction("SelectionDelete");
		SelectionDelete.UserData = new PlayerActionData.ActionUserData("inpActSelectionDeleteName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		SelectionMoveMode = CreatePlayerAction("SelectionMoveMode");
		SelectionMoveMode.UserData = new PlayerActionData.ActionUserData("inpActSelectionMoveModeName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		FocusCopyBlock = CreatePlayerAction("FocusCopyBlock");
		FocusCopyBlock.UserData = new PlayerActionData.ActionUserData("inpActFocusCopyBlockName", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		Prefab = CreatePlayerAction("Prefab");
		Prefab.UserData = new PlayerActionData.ActionUserData("inpActPrefabName", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		DensityM1 = CreatePlayerAction("DensityM1");
		DensityM1.UserData = new PlayerActionData.ActionUserData("inpActDensityM1Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		DensityP1 = CreatePlayerAction("DensityP1");
		DensityP1.UserData = new PlayerActionData.ActionUserData("inpActDensityP1Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		DensityM10 = CreatePlayerAction("DensityM10");
		DensityM10.UserData = new PlayerActionData.ActionUserData("inpActDensityM10Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		DensityP10 = CreatePlayerAction("DensityP10");
		DensityP10.UserData = new PlayerActionData.ActionUserData("inpActDensityP10Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		Inventory = CreatePlayerAction("Inventory");
		Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		Scoreboard = CreatePlayerAction("Scoreboard");
		Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.ControllerOnly);
		QuickMenu = CreatePlayerAction("QuickMenu");
		QuickMenu.UserData = new PlayerActionData.ActionUserData("inpQuickMenuName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly);
		AiFreeze = CreatePlayerAction("AiFreeze");
		AiFreeze.UserData = new PlayerActionData.ActionUserData("inpActAiFreezeName", null, PlayerActionData.GroupDebugFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		ConfigureJoystickLayout();
		Run.AddDefaultBinding(InputControlType.LeftStickButton);
		Jump.AddDefaultBinding(InputControlType.Action1);
		ToggleCrouch.AddDefaultBinding(InputControlType.Action2);
		Activate.AddDefaultBinding(InputControlType.Action4);
		Reload.AddDefaultBinding(InputControlType.Action3);
		Primary.AddDefaultBinding(InputControlType.RightTrigger);
		Secondary.AddDefaultBinding(InputControlType.LeftTrigger);
		InventorySlotLeft.AddDefaultBinding(InputControlType.LeftBumper);
		InventorySlotRight.AddDefaultBinding(InputControlType.RightBumper);
		Swap.AddDefaultBinding(InputControlType.DPadLeft);
		Menu.AddDefaultBinding(InputControlType.Menu);
		Menu.AddDefaultBinding(InputControlType.Options);
		Menu.AddDefaultBinding(InputControlType.Start);
		Menu.AddDefaultBinding(InputControlType.Plus);
		Inventory.AddDefaultBinding(InputControlType.DPadUp);
		Scoreboard.AddDefaultBinding(InputControlType.View);
		Scoreboard.AddDefaultBinding(InputControlType.TouchPadButton);
		Scoreboard.AddDefaultBinding(InputControlType.Back);
		GodAlternate.AddDefaultBinding(InputControlType.Action4);
		TeleportAlternate.AddDefaultBinding(InputControlType.Action1);
		QuickMenu.AddDefaultBinding(InputControlType.DPadDown);
		CameraFunction.AddDefaultBinding(InputControlType.RightStickButton);
		ControllerRebindableActions.Clear();
		ControllerRebindableActions.Add(Jump);
		ControllerRebindableActions.Add(ToggleCrouch);
		ControllerRebindableActions.Add(Reload);
		ControllerRebindableActions.Add(Activate);
		ControllerRebindableActions.Add(Run);
		ControllerRebindableActions.Add(Primary);
		ControllerRebindableActions.Add(Secondary);
		ControllerRebindableActions.Add(Inventory);
		ControllerRebindableActions.Add(Swap);
		ControllerRebindableActions.Add(QuickMenu);
		ControllerRebindableActions.Add(CameraFunction);
		ControllerRebindableActions.Add(CameraChange);
		ControllerRebindableActions.Add(ToggleFlashlight);
		ControllerRebindableActions.Add(Drop);
		ControllerRebindableActions.Add(InventorySlotRight);
		ControllerRebindableActions.Add(InventorySlotLeft);
		ControllerRebindableActions.Add(InventorySlot1);
		ControllerRebindableActions.Add(InventorySlot2);
		ControllerRebindableActions.Add(InventorySlot3);
		ControllerRebindableActions.Add(InventorySlot4);
		ControllerRebindableActions.Add(InventorySlot5);
		ControllerRebindableActions.Add(InventorySlot6);
		ControllerRebindableActions.Add(InventorySlot7);
		ControllerRebindableActions.Add(InventorySlot8);
		ControllerRebindableActions.Add(InventorySlot9);
		ControllerRebindableActions.Add(InventorySlot10);
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
		Run.AddDefaultBinding(Key.LeftShift);
		Jump.AddDefaultBinding(Key.Space);
		Crouch.AddDefaultBinding(Key.C);
		ToggleCrouch.AddDefaultBinding(Key.LeftControl);
		Drop.AddDefaultBinding(Key.G);
		God.AddDefaultBinding(Key.Q);
		Fly.AddDefaultBinding(Key.H);
		Invisible.AddDefaultBinding(Key.PadDivide);
		IncSpeed.AddDefaultBinding(Key.Shift, Key.Equals);
		DecSpeed.AddDefaultBinding(Key.Shift, Key.Minus);
		CameraFunction.AddDefaultBinding(Mouse.MiddleButton);
		Primary.AddDefaultBinding(Mouse.LeftButton);
		Secondary.AddDefaultBinding(Mouse.RightButton);
		SelectionFill.AddDefaultBinding(Key.L);
		SelectionClear.AddDefaultBinding(Key.J);
		SelectionSet.AddDefaultBinding(Key.Z);
		SelectionRotate.AddDefaultBinding(Key.X);
		SelectionDelete.AddDefaultBinding(Key.Backspace);
		SelectionMoveMode.AddDefaultBinding(Key.Insert);
		FocusCopyBlock.AddDefaultBinding(Mouse.MiddleButton);
		Prefab.AddDefaultBinding(Key.K);
		DetachCamera.AddDefaultBinding(Key.P);
		ToggleDCMove.AddDefaultBinding(Key.LeftBracket);
		LockFreeCamera.AddDefaultBinding(Key.Pad1);
		DensityM1.AddDefaultBinding(Key.RightArrow);
		DensityP1.AddDefaultBinding(Key.LeftArrow);
		DensityM10.AddDefaultBinding(Key.UpArrow);
		DensityP10.AddDefaultBinding(Key.DownArrow);
		ScrollUp.AddDefaultBinding(Mouse.PositiveScrollWheel);
		ScrollDown.AddDefaultBinding(Mouse.NegativeScrollWheel);
		Menu.AddDefaultBinding(Key.Escape);
		InventorySlot1.AddDefaultBinding(Key.Key1);
		InventorySlot2.AddDefaultBinding(Key.Key2);
		InventorySlot3.AddDefaultBinding(Key.Key3);
		InventorySlot4.AddDefaultBinding(Key.Key4);
		InventorySlot5.AddDefaultBinding(Key.Key5);
		InventorySlot6.AddDefaultBinding(Key.Key6);
		InventorySlot7.AddDefaultBinding(Key.Key7);
		InventorySlot8.AddDefaultBinding(Key.Key8);
		InventorySlot9.AddDefaultBinding(Key.Key9);
		InventorySlot10.AddDefaultBinding(Key.Key0);
		InventorySlotRight.AddDefaultBinding(Mouse.NegativeScrollWheel);
		InventorySlotLeft.AddDefaultBinding(Mouse.PositiveScrollWheel);
		AiFreeze.AddDefaultBinding(Key.PadMultiply);
	}

	public void SetDeadzones(float _left, float _right)
	{
		leftStickDeadzone = _left;
		rightStickDeadzone = _right;
		UpdateDeadzones();
	}

	public void UpdateDeadzones()
	{
		InputManager.ActiveDevice.LeftStick.LowerDeadZone = leftStickDeadzone;
		InputManager.ActiveDevice.RightStick.LowerDeadZone = rightStickDeadzone;
	}

	public void SetJoyStickLayout(eControllerJoystickLayout layout)
	{
		joystickLayout = layout;
		ConfigureJoystickLayout();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConfigureJoystickLayout()
	{
		switch (joystickLayout)
		{
		case eControllerJoystickLayout.Standard:
			MoveForward.AddDefaultBinding(InputControlType.LeftStickUp);
			MoveForward.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickUp));
			MoveBack.AddDefaultBinding(InputControlType.LeftStickDown);
			MoveBack.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
			MoveLeft.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);
			MoveRight.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			LookUp.AddDefaultBinding(InputControlType.RightStickUp);
			LookUp.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
			LookDown.AddDefaultBinding(InputControlType.RightStickDown);
			LookDown.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
			LookLeft.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			LookRight.AddDefaultBinding(InputControlType.RightStickRight);
			LookRight.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			break;
		case eControllerJoystickLayout.Southpaw:
			MoveForward.AddDefaultBinding(InputControlType.RightStickUp);
			MoveForward.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
			MoveBack.AddDefaultBinding(InputControlType.RightStickDown);
			MoveBack.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			MoveLeft.AddDefaultBinding(InputControlType.RightStickLeft);
			MoveLeft.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			MoveRight.AddDefaultBinding(InputControlType.RightStickRight);
			MoveRight.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			LookUp.AddDefaultBinding(InputControlType.LeftStickUp);
			LookUp.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickUp));
			LookDown.AddDefaultBinding(InputControlType.LeftStickDown);
			LookDown.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			LookLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
			LookLeft.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			LookRight.AddDefaultBinding(InputControlType.LeftStickRight);
			LookRight.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			break;
		case eControllerJoystickLayout.Legacy:
			MoveForward.AddDefaultBinding(InputControlType.LeftStickUp);
			MoveForward.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickUp));
			MoveBack.AddDefaultBinding(InputControlType.LeftStickDown);
			MoveBack.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			MoveLeft.AddDefaultBinding(InputControlType.RightStickLeft);
			MoveLeft.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			MoveRight.AddDefaultBinding(InputControlType.RightStickRight);
			MoveRight.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			LookUp.AddDefaultBinding(InputControlType.RightStickUp);
			LookUp.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
			LookDown.AddDefaultBinding(InputControlType.RightStickDown);
			LookDown.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			LookLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
			LookLeft.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			LookRight.AddDefaultBinding(InputControlType.LeftStickRight);
			LookRight.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			break;
		case eControllerJoystickLayout.LegacySouthpaw:
			MoveForward.AddDefaultBinding(InputControlType.RightStickUp);
			MoveForward.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
			MoveBack.AddDefaultBinding(InputControlType.RightStickDown);
			MoveBack.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
			MoveLeft.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);
			MoveRight.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			LookUp.AddDefaultBinding(InputControlType.LeftStickUp);
			LookUp.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickUp));
			LookDown.AddDefaultBinding(InputControlType.LeftStickDown);
			LookDown.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
			LookLeft.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			LookRight.AddDefaultBinding(InputControlType.RightStickRight);
			LookRight.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			break;
		}
	}

	public bool AnyGUIActionPressed()
	{
		foreach (PlayerAction action in GUIActions.Actions)
		{
			if (action.WasPressed)
			{
				return true;
			}
		}
		return false;
	}
}
