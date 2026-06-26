using System;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenu : XUiController
{
	public static string ID = "";

	public static bool openedOnce;

	public static bool shownNewsScreenOnce;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowEditTools;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool anySaveFilesExist;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnNewGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnContinueGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnConnectToServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnEditingTools;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRWG;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCredits;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnNews;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnQuit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnNewGame = GetChildById("btnNewGame") as XUiC_SimpleButton;
		btnContinueGame = GetChildById("btnContinueGame") as XUiC_SimpleButton;
		btnConnectToServer = GetChildById("btnConnectToServer") as XUiC_SimpleButton;
		btnOptions = GetChildById("btnOptions") as XUiC_SimpleButton;
		btnCredits = GetChildById("btnCredits") as XUiC_SimpleButton;
		btnNews = GetChildById("btnNews") as XUiC_SimpleButton;
		btnQuit = GetChildById("btnQuit") as XUiC_SimpleButton;
		btnNewGame.OnPressed += btnNewGame_OnPressed;
		btnContinueGame.OnPressed += btnContinueGame_OnPressed;
		btnConnectToServer.OnPressed += btnConnectToServer_OnPressed;
		btnOptions.OnPressed += btnOptions_OnPressed;
		btnCredits.OnPressed += btnCredits_OnPressed;
		if (btnNews != null)
		{
			btnNews.OnPressed += btnNews_OnPressed;
		}
		if (btnQuit != null)
		{
			btnQuit.OnPressed += btnQuit_OnPressed;
		}
		btnEditingTools = GetChildById("btnEditingTools") as XUiC_SimpleButton;
		if (btnEditingTools != null)
		{
			btnEditingTools.OnPressed += btnEditingTools_OnPressed;
		}
		btnRWG = GetChildById("btnRWG") as XUiC_SimpleButton;
		if (btnRWG != null)
		{
			btnRWG.OnPressed += btnRWG_OnPressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNewGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			XUiC_NewContinueGame.SetIsContinueGame(base.xui, _continueGame: false);
			CheckProfile(XUiC_NewContinueGame.ID);
		}
		else
		{
			XUiC_EulaWindow.Open(base.xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnContinueGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			XUiC_NewContinueGame.SetIsContinueGame(base.xui, _continueGame: true);
			CheckProfile(XUiC_NewContinueGame.ID);
		}
		else
		{
			XUiC_EulaWindow.Open(base.xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnConnectToServer_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(base.xui);
			return;
		}
		if (wdwMultiplayerPrivileges == null)
		{
			wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		}
		wdwMultiplayerPrivileges?.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, [PublicizedFrom(EAccessModifier.Private)] (bool result) =>
		{
			if (PermissionsManager.IsMultiplayerAllowed())
			{
				CheckProfile(XUiC_ServerBrowser.ID);
			}
		}, EUserPerms.Crossplay);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnEditingTools_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
			base.xui.playerUI.windowManager.Open(XUiC_EditingTools.ID, _bModal: true);
		}
		else
		{
			XUiC_EulaWindow.Open(base.xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRWG_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(base.xui);
			return;
		}
		base.xui.FindWindowGroupByName("rwgeditor").GetChildByType<XUiC_WorldGenerationWindowGroup>().LastWindowID = ID;
		base.xui.playerUI.windowManager.Open("rwgeditor", _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOptions_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCredits_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_Credits.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNews_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		XUiC_NewsScreen.Open(base.xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnQuit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Application.Quit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckProfile(string _windowToOpen)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			XUiC_OptionsProfiles.Open(base.xui, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				base.xui.playerUI.windowManager.Open(_windowToOpen, _bModal: true);
			});
		}
		else
		{
			base.xui.playerUI.windowManager.Open(_windowToOpen, _bModal: true);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		windowGroup.isEscClosable = false;
		RefreshBindings();
		bool _checkRestartAtMainMenu = true;
		bool num = PlatformManager.MultiPlatform.JoinSessionGameInviteListener != null && PlatformManager.MultiPlatform.JoinSessionGameInviteListener.IsProcessingIntent(out _checkRestartAtMainMenu);
		if (_checkRestartAtMainMenu)
		{
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine());
		}
		if (num)
		{
			string text = Localization.Get("lblReceivedGameInvite");
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PlatformManager.MultiPlatform.JoinSessionGameInviteListener?.Cancel();
				LocalPlayerUI.primaryUI.windowManager.Open(ID, _bModal: true);
			});
		}
		DoLoadSaveGameAutomation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoLoadSaveGameAutomation()
	{
		EPlatformLoadSaveGameState loadSaveGameState = PlatformApplicationManager.GetLoadSaveGameState();
		switch (loadSaveGameState)
		{
		case EPlatformLoadSaveGameState.NewGameOpen:
			if (!btnNewGame.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				break;
			}
			btnNewGame_OnPressed(btnNewGame, -1);
			if (!base.xui.playerUI.windowManager.IsWindowOpen(XUiC_NewContinueGame.ID))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
			}
			else
			{
				PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			}
			break;
		case EPlatformLoadSaveGameState.ContinueGameOpen:
			if (!btnContinueGame.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				break;
			}
			btnContinueGame_OnPressed(btnContinueGame, -1);
			if (!base.xui.playerUI.windowManager.IsWindowOpen(XUiC_NewContinueGame.ID))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
			}
			else
			{
				PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			}
			break;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "systemtime_hr":
			_value = DateTime.Now.Hour.ToString();
			return true;
		case "systemtime_min":
			_value = DateTime.Now.Minute.ToString();
			return true;
		case "systemtime_sec":
			_value = DateTime.Now.Second.ToString();
			return true;
		case "has_saved_game":
			_value = anySaveFilesExist.ToString();
			return true;
		default:
			return base.GetBindingValue(ref _value, _bindingName);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Menu);
		TitleStorageOverridesManager.Instance.FetchFromSource(null);
		openedOnce = true;
		SaveDataUtils.SaveDataManager.CommitAsync();
		anySaveFilesExist = GameIO.GetPlayerSaves() > 0;
		base.xui.playerUI.windowManager.Close("eacWarning");
		base.xui.playerUI.windowManager.Close("crossplayWarning");
		XUiC_MainMenuPlayerName.OpenIfNotOpen(base.xui);
		if (base.xui.playerUI.ActionSetManager != null)
		{
			base.xui.playerUI.ActionSetManager.Reset();
			base.xui.playerUI.ActionSetManager.Push(base.xui.playerUI.playerInput);
			if (base.xui.playerUI.windowManager.IsWindowOpen(GUIWindowConsole.ID))
			{
				base.xui.playerUI.ActionSetManager.Push(base.xui.playerUI.playerInput.GUIActions);
			}
		}
		btnConnectToServer.Enabled = PlatformManager.MultiPlatform.User.UserStatus != EUserStatus.OfflineMode;
		base.xui.playerUI.nguiWindowManager.Show(EnumNGUIWindow.MainMenuBackground, _bEnable: false);
		base.xui.playerUI.windowManager.Open("menuBackground", _bModal: false);
		base.xui.playerUI.windowManager.Open("mainMenuLogo", _bModal: false);
		GameManager.Instance.SetCursorEnabledOverride(_bOverrideOn: false, _bOverrideState: false);
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		GetChildById("btnNewGame").SelectCursorElement(_withDelay: true);
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", _bModal: false);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.RemoveCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.Menu, _enabled: true);
		TriggerEffectManager.SetMainMenuLightbarColor();
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.Close("mainMenuLogo");
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		XUiC_MainMenuPlayerName.Close(base.xui);
	}

	public static void Open(XUi _xuiInstance)
	{
		if (LaunchPrefs.SkipNewsScreen.Value || PlatformApplicationManager.GetLoadSaveGameState() != EPlatformLoadSaveGameState.Done)
		{
			shownNewsScreenOnce = true;
		}
		IJoinSessionGameInviteListener joinSessionGameInviteListener = PlatformManager.NativePlatform.JoinSessionGameInviteListener;
		if (joinSessionGameInviteListener != null && joinSessionGameInviteListener.HasPendingIntent())
		{
			shownNewsScreenOnce = true;
		}
		if (!shownNewsScreenOnce)
		{
			XUiC_NewsScreen.Open(_xuiInstance);
			shownNewsScreenOnce = true;
		}
		else
		{
			_xuiInstance.playerUI.windowManager.Open(ID, _bModal: true);
		}
	}
}
