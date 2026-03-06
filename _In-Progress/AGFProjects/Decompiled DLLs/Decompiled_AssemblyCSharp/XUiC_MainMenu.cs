using System;
using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenu : XUiController
{
	public static string ID = "";

	public static bool openedOnce;

	public static bool shownNewsScreenOnce;

	public static bool clockSyncChecked;

	public static bool snapMicPermissionChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> windowsToOpenGloballyWithMainMenu = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> windowsToCloseGloballyWithMainMenu = new List<string>();

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		windowGroup.isEscClosable = false;
		RefreshBindings();
		if (PlatformApplicationManager.CheckRestartCoroutineReady())
		{
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine());
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "windows_to_open_globally")
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
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
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
		base.xui.playerUI.windowManager.Close("eacWarning");
		base.xui.playerUI.windowManager.Close("crossplayWarning");
		XUiC_MainMenuPlayerName.OpenIfNotOpen(base.xui);
		base.xui.playerUI.windowManager.ResetActionSets();
		OpenGlobalMenuWindows(base.xui);
		GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: false, _bOverrideState: false);
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", _bModal: false);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.RemoveCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.Menu, _enabled: true);
		TriggerEffectManager.SetMainMenuLightbarColor();
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
			((XUiC_MessageBoxWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller).ShowNetworkError(networkConnectionError);
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
			XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiLinuxSnapNoMicPermissionTitle"), Localization.Get("xuiLinuxSnapNoMicPermissionText"), null, true, true);
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
				XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiSystemTimeSyncHeader"), Localization.Get("xuiSystemTimeSyncWarning"), null, true, true);
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		XUiC_MainMenuPlayerName.Close(base.xui);
	}

	public static void Open(XUi _xuiInstance)
	{
		OpenGlobalMenuWindows(_xuiInstance);
		if (LaunchPrefs.SkipNewsScreen.Value || PlatformApplicationManager.GetLoadSaveGameState() != EPlatformLoadSaveGameState.Done || MainMenuMono.IsQuickContinue)
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
			windowManager.OpenIfNotOpen(item, _bModal: false);
		}
	}

	public static void CloseGlobalMenuWindows(XUi _xui)
	{
		XUiC_MainMenuPlayerName.Close(_xui);
		LocalPlayerUI playerUI = _xui.playerUI;
		GUIWindowManager windowManager = playerUI.windowManager;
		playerUI.nguiWindowManager.Show(EnumNGUIWindow.MainMenuBackground, _bEnable: false);
		foreach (string item in windowsToCloseGloballyWithMainMenu)
		{
			windowManager.Close(item);
		}
	}
}
