using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenu : XUiController
{
	public static string ID = "";

	public static bool openedOnce;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool shownNewsScreenOnce;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool clockSyncChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool snapMicPermissionChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> windowsToOpenGloballyWithMainMenu = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> windowsToCloseGloballyWithMainMenu = new List<string>();

	[XuiXmlAttribute("windows_to_open_globally", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeWindowsToOpenGlobally(string _value)
	{
		string[] array = _value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (!string.IsNullOrEmpty(text))
			{
				switch (text[0])
				{
				case '+':
				{
					List<string> list2 = windowsToOpenGloballyWithMainMenu;
					string text2 = text;
					list2.Add(text2.Substring(1, text2.Length - 1));
					break;
				}
				case '-':
				{
					List<string> list = windowsToCloseGloballyWithMainMenu;
					string text2 = text;
					list.Add(text2.Substring(1, text2.Length - 1));
					break;
				}
				default:
					windowsToOpenGloballyWithMainMenu.Add(text);
					windowsToCloseGloballyWithMainMenu.Add(text);
					break;
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		windowGroup.isEscClosable = false;
		RefreshBindings();
		if (PlatformApplicationManager.CheckRestartCoroutineReady() && AutomationRunner.Instance.RestartAllowed())
		{
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine());
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		bool num = !openedOnce;
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Menu);
		TitleStorageOverridesManager.Instance.FetchFromSource(null);
		ModEvents.SMainMenuOpenedData _data = new ModEvents.SMainMenuOpenedData(!openedOnce);
		ModEvents.MainMenuOpened.Invoke(ref _data);
		openedOnce = true;
		SaveDataUtils.SaveDataManager.CommitAsync();
		xui.playerUI.windowManager.Close("eacWarning");
		xui.playerUI.windowManager.Close("crossplayWarning");
		xui.playerUI.windowManager.ResetActionSets();
		OpenGlobalMenuWindows(xui);
		GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: false, _bOverrideState: false);
		xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		TriggerEffectManager.SetMainMenuLightbarColor();
		if (num)
		{
			ThreadManager.RunTaskAfterFrames(Localization.UnloadUnusedLanguages);
		}
		if (num && !string.IsNullOrEmpty(LaunchPrefs.RunAutomation.Value))
		{
			if (AutomationRunner.Instance.LoadScript(AutomationScript.LoadFromFile(LaunchPrefs.RunAutomation.Value)))
			{
				AutomationRunner.Instance.StartRuns();
			}
			else
			{
				Log.Error("[XUiC_MainMenu] Failed to load automation script '" + LaunchPrefs.RunAutomation.Value + "'.");
			}
		}
		if (num && MainMenuMono.IsQuickContinue)
		{
			quickContinue();
		}
		else if (!snapCheck())
		{
			clockSyncCheck();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void quickContinue()
	{
		Log.Out("QuickContinue mode, loading last savegame");
		GamePrefs.Instance.Load(GameIO.GetSaveGameDir() + "/gameOptions.sdf");
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			XUiC_MessageBoxWindowGroup.ShowNetworkError(xui, networkConnectionError);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool snapCheck()
	{
		if (snapMicPermissionChecked)
		{
			return false;
		}
		snapMicPermissionChecked = true;
		if (VoiceHelpers.IsSnapWithoutMicPermission() && (!DiscordManager.Instance.Settings.DiscordDisabled || GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled)))
		{
			XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiLinuxSnapNoMicPermissionTitle"), Localization.Get("xuiLinuxSnapNoMicPermissionText"));
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clockSyncCheck()
	{
		if (!clockSyncChecked && GameManager.ServerClockSync.RequestComplete)
		{
			clockSyncChecked = true;
			if (!GameManager.ServerClockSync.HasError && Math.Abs(GameManager.ServerClockSync.SecondsOffset) >= 120)
			{
				XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiSystemTimeSyncHeader"), Localization.Get("xuiSystemTimeSyncWarning"));
			}
		}
	}

	public static void Open(XUi _xuiInstance)
	{
		OpenGlobalMenuWindows(_xuiInstance);
		if (LaunchPrefs.SkipNewsScreen.Value || !string.IsNullOrEmpty(LaunchPrefs.RunAutomation.Value) || PlatformApplicationManager.GetLoadSaveGameState() != EPlatformLoadSaveGameState.Done || MainMenuMono.IsQuickContinue)
		{
			shownNewsScreenOnce = true;
		}
		if (InviteManager.Instance.HasPendingInvite())
		{
			shownNewsScreenOnce = true;
		}
		if (!shownNewsScreenOnce)
		{
			XUiC_NewsScreen.Open(_xuiInstance);
			shownNewsScreenOnce = true;
			return;
		}
		ModEvents.SMainMenuOpeningData _data = new ModEvents.SMainMenuOpeningData(openedOnce);
		if (ModEvents.MainMenuOpening.Invoke(ref _data).Item1 != ModEvents.EModEventResult.StopHandlersAndVanilla)
		{
			_xuiInstance.playerUI.windowManager.Open(ID, _bModal: true);
		}
	}

	public static void OpenGlobalMenuWindows(XUi _xui)
	{
		LocalPlayerUI playerUI = _xui.playerUI;
		GUIWindowManager windowManager = playerUI.windowManager;
		playerUI.nguiWindowManager.Show(EnumNGUIWindow.MainMenuBackground, _bEnable: false);
		foreach (string item in windowsToOpenGloballyWithMainMenu)
		{
			windowManager.Open(item, _bModal: false);
		}
	}

	public static void CloseGlobalMenuWindows(XUi _xui)
	{
		LocalPlayerUI playerUI = _xui.playerUI;
		GUIWindowManager windowManager = playerUI.windowManager;
		playerUI.nguiWindowManager.Show(EnumNGUIWindow.MainMenuBackground, _bEnable: false);
		foreach (string item in windowsToCloseGloballyWithMainMenu)
		{
			windowManager.Close(item);
		}
	}
}
