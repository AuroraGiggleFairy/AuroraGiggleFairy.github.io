using InControl;

public class PlayerActionsGlobal : PlayerActionsBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerActionsGlobal m_Instance;

	public PlayerAction Console;

	public PlayerAction ShowDebugData;

	public PlayerAction Fullscreen;

	public PlayerAction DebugSpawn;

	public PlayerAction DebugGameEvent;

	public PlayerAction SwitchHUD;

	public PlayerAction ShowFPS;

	public PlayerAction Screenshot;

	public PlayerAction DebugScreenshot;

	public PlayerAction BackgroundedScreenshot;

	public static PlayerActionsGlobal Instance => m_Instance;

	public static void Init()
	{
		if (m_Instance == null)
		{
			m_Instance = new PlayerActionsGlobal();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsGlobal()
	{
		base.Name = "global";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		Console = CreatePlayerAction("Console");
		Console.UserData = new PlayerActionData.ActionUserData("inpActConsoleName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		ShowDebugData = CreatePlayerAction("Show Debug Data");
		ShowDebugData.UserData = new PlayerActionData.ActionUserData("inpActShowDebugDataName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		Fullscreen = CreatePlayerAction("Fullscreen");
		Fullscreen.UserData = new PlayerActionData.ActionUserData("inpActFullscreenName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		DebugSpawn = CreatePlayerAction("DebugSpawn");
		DebugSpawn.UserData = new PlayerActionData.ActionUserData("inpActDebugSpawnName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		DebugGameEvent = CreatePlayerAction("DebugGameEvent");
		DebugGameEvent.UserData = new PlayerActionData.ActionUserData("inpActDebugGameEventName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		SwitchHUD = CreatePlayerAction("SwitchHUD");
		SwitchHUD.UserData = new PlayerActionData.ActionUserData("inpActSwitchHUDName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		ShowFPS = CreatePlayerAction("ShowFPS");
		ShowFPS.UserData = new PlayerActionData.ActionUserData("inpActShowFPSName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		Screenshot = CreatePlayerAction("Screenshot");
		Screenshot.UserData = new PlayerActionData.ActionUserData("inpActScreenshotName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		DebugScreenshot = CreatePlayerAction("DebugScreenshot");
		DebugScreenshot.UserData = new PlayerActionData.ActionUserData("inpActDebugScreenshotName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, _allowRebind: false);
		BackgroundedScreenshot = CreatePlayerAction("BackgroundedScreenshot");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		if (!Submission.Enabled)
		{
			Console.AddDefaultBinding(Key.F1);
			ShowDebugData.AddDefaultBinding(Key.F3);
			Fullscreen.AddDefaultBinding(Key.F4);
			DebugSpawn.AddDefaultBinding(Key.F6);
			DebugGameEvent.AddDefaultBinding(Key.F6, Key.Shift);
			SwitchHUD.AddDefaultBinding(Key.F7);
			ShowFPS.AddDefaultBinding(Key.F8);
			Screenshot.AddDefaultBinding(Key.F9);
			BackgroundedScreenshot.AddDefaultBinding(Key.F10);
			DebugScreenshot.AddDefaultBinding(Key.F11);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
	}
}
