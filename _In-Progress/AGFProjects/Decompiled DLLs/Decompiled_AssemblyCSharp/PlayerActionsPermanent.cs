using InControl;

public class PlayerActionsPermanent : PlayerActionsBase
{
	public PlayerAction Reload;

	public PlayerAction Activate;

	public PlayerAction ToggleFlashlight;

	public PlayerAction Inventory;

	public PlayerAction Skills;

	public PlayerAction Quests;

	public PlayerAction Challenges;

	public PlayerAction Character;

	public PlayerAction Map;

	public PlayerAction Creative;

	public PlayerAction Scoreboard;

	public PlayerAction DebugControllerLeft;

	public PlayerAction DebugControllerRight;

	public PlayerAction Chat;

	public PlayerAction PushToTalk;

	public PlayerAction Cancel;

	public PlayerAction Swap;

	public PlayerAction PageTipsForward;

	public PlayerAction PageTipsBack;

	public PlayerAction QuickMenu;

	public PlayerAction CameraZoomIn;

	public PlayerAction CameraZoomOut;

	public PlayerOneAxisAction CameraZoom;

	public PlayerActionsPermanent()
	{
		base.Name = "permanent";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		Reload = CreatePlayerAction("Reload");
		Reload.UserData = new PlayerActionData.ActionUserData("inpActReloadTakeAllName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		Reload.FirstRepeatDelay = 0.3f;
		Activate = CreatePlayerAction("Activate");
		Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		ToggleFlashlight = CreatePlayerAction("ToggleFlashlight");
		ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleFlashlightName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		ToggleFlashlight.FirstRepeatDelay = 0.3f;
		Inventory = CreatePlayerAction("Inventory");
		Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		Character = CreatePlayerAction("Character");
		Character.UserData = new PlayerActionData.ActionUserData("inpActCharacterName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		Map = CreatePlayerAction("Map");
		Map.UserData = new PlayerActionData.ActionUserData("inpActMapName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		Skills = CreatePlayerAction("Skills");
		Skills.UserData = new PlayerActionData.ActionUserData("inpActSkillsName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		Quests = CreatePlayerAction("Quests");
		Quests.UserData = new PlayerActionData.ActionUserData("inpActQuestsName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		Challenges = CreatePlayerAction("Challenges");
		Challenges.UserData = new PlayerActionData.ActionUserData("inpActChallengesName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		Scoreboard = CreatePlayerAction("Scoreboard");
		Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		Creative = CreatePlayerAction("Creative");
		Creative.UserData = new PlayerActionData.ActionUserData("inpActCreativeName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		DebugControllerLeft = CreatePlayerAction("DebugControllerLeft");
		DebugControllerLeft.UserData = new PlayerActionData.ActionUserData("inpActDebugControllerLeftName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		DebugControllerRight = CreatePlayerAction("DebugControllerRight");
		DebugControllerRight.UserData = new PlayerActionData.ActionUserData("inpActDebugControllerRightName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		Chat = CreatePlayerAction("Chat");
		Chat.UserData = new PlayerActionData.ActionUserData("inpActChatName", null, PlayerActionData.GroupMp, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		PushToTalk = CreatePlayerAction("PushToTalk");
		PushToTalk.UserData = new PlayerActionData.ActionUserData("inpActPushToTalkName", null, PlayerActionData.GroupMp, PlayerActionData.EAppliesToInputType.Both, _allowRebind: true, _allowMultipleRebindings: false, _doNotDisplay: false, _defaultOnStartup: false);
		Cancel = CreatePlayerAction("Cancel");
		Cancel.UserData = new PlayerActionData.ActionUserData("inpActCancelName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		Swap = CreatePlayerAction("Swap");
		Swap.UserData = new PlayerActionData.ActionUserData("inpActSwapName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		PageTipsForward = CreatePlayerAction("PageTipsForward");
		PageTipsForward.UserData = new PlayerActionData.ActionUserData("inpActPageTipsForward", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		PageTipsBack = CreatePlayerAction("PageTipsBack");
		PageTipsBack.UserData = new PlayerActionData.ActionUserData("inpActPageTipsBack", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		QuickMenu = CreatePlayerAction("QuickMenu");
		QuickMenu.UserData = new PlayerActionData.ActionUserData("inpQuickMenuName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly);
		CameraZoomIn = CreatePlayerAction("ScrollUp");
		CameraZoomIn.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomInName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		CameraZoomOut = CreatePlayerAction("ScrollDown");
		CameraZoomOut.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomOutName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, _allowRebind: false, _allowMultipleRebindings: true, _doNotDisplay: true);
		CameraZoom = CreateOneAxisPlayerAction(CameraZoomOut, CameraZoomIn);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		DebugControllerLeft.AddDefaultBinding(InputControlType.LeftBumper);
		DebugControllerRight.AddDefaultBinding(InputControlType.RightBumper);
		Cancel.AddDefaultBinding(InputControlType.Action2);
		PageTipsForward.AddDefaultBinding(InputControlType.RightTrigger);
		PageTipsBack.AddDefaultBinding(InputControlType.LeftTrigger);
		PushToTalk.AddDefaultBinding(InputControlType.DPadRight);
		CameraZoomIn.AddDefaultBinding(InputControlType.RightTrigger);
		CameraZoomOut.AddDefaultBinding(InputControlType.LeftTrigger);
		ControllerRebindableActions.Clear();
		ControllerRebindableActions.Add(PushToTalk);
		ControllerRebindableActions.Add(Character);
		ControllerRebindableActions.Add(Map);
		ControllerRebindableActions.Add(Skills);
		ControllerRebindableActions.Add(Quests);
		ControllerRebindableActions.Add(Challenges);
		ControllerRebindableActions.Add(Scoreboard);
		ControllerRebindableActions.Add(Creative);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		base.ListenOptions.IncludeKeys = true;
		base.ListenOptions.IncludeMouseButtons = true;
		base.ListenOptions.IncludeMouseScrollWheel = true;
		Reload.AddDefaultBinding(Key.R);
		Activate.AddDefaultBinding(Key.E);
		ToggleFlashlight.AddDefaultBinding(Key.F);
		Inventory.AddDefaultBinding(Key.Tab);
		Skills.AddDefaultBinding(Key.N);
		Quests.AddDefaultBinding(Key.O);
		Challenges.AddDefaultBinding(Key.Y);
		Character.AddDefaultBinding(Key.B);
		Map.AddDefaultBinding(Key.M);
		Creative.AddDefaultBinding(Key.U);
		Scoreboard.AddDefaultBinding(Key.I);
		Chat.AddDefaultBinding(Key.T);
		PushToTalk.AddDefaultBinding(Key.V);
		Cancel.AddDefaultBinding(Key.Escape);
		Swap.AddDefaultBinding(Mouse.Button4);
		PageTipsForward.AddDefaultBinding(Mouse.LeftButton);
		PageTipsBack.AddDefaultBinding(Mouse.RightButton);
		QuickMenu.AddDefaultBinding(Key.Backquote);
		CameraZoomIn.AddDefaultBinding(Mouse.PositiveScrollWheel);
		CameraZoomOut.AddDefaultBinding(Mouse.NegativeScrollWheel);
	}
}
