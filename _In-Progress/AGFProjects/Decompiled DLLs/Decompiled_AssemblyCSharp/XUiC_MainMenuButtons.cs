using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenuButtons : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool anySaveFilesExist;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnNewGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnContinueGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;

	public override void Init()
	{
		base.Init();
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnNewGame", out btnNewGame))
		{
			btnNewGame.OnPressed += btnNewGame_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnContinueGame", out btnContinueGame))
		{
			btnContinueGame.OnPressed += btnContinueGame_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnConnectToServer", out var _child))
		{
			_child.OnPressed += btnConnectToServer_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnOptions", out var _child2))
		{
			_child2.OnPressed += btnOptions_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnCredits", out var _child3))
		{
			_child3.OnPressed += btnCredits_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnNews", out var _child4))
		{
			_child4.OnPressed += btnNews_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnQuit", out var _child5))
		{
			_child5.OnPressed += btnQuit_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnEditingTools", out var _child6))
		{
			_child6.OnPressed += btnEditingTools_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnRWG", out var _child7))
		{
			_child7.OnPressed += btnRWG_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_SimpleButton>("btnDlc", out var _child8))
		{
			_child8.OnPressed += btnDlc_OnPressed;
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
		});
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
		base.xui.FindWindowGroupByName("rwgeditor").GetChildByType<XUiC_WorldGenerationWindowGroup>().LastWindowID = XUiC_MainMenu.ID;
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
	public void btnDlc_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup);
		base.xui.playerUI.windowManager.Open(XUiC_DlcWindow.ID, _bModal: true);
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
		RefreshBindings();
		if (btnNewGame != null || btnContinueGame != null)
		{
			DoLoadSaveGameAutomation();
		}
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "has_saved_game"))
		{
			if (_bindingName == "online_mode")
			{
				_value = (PlatformManager.MultiPlatform?.User?.UserStatus != EUserStatus.OfflineMode).ToString();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = anySaveFilesExist.ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (btnContinueGame != null)
		{
			anySaveFilesExist = GameIO.GetPlayerSaves() > 0;
		}
		btnNewGame?.SelectCursorElement(_withDelay: true);
	}
}
