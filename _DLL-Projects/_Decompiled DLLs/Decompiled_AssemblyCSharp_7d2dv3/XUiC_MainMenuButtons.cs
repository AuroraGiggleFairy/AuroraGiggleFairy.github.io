using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenuButtons : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Button btnPlayGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Button btnNewGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Button btnContinueGame;

	[XuiXmlBinding("online_mode")]
	public bool OnlineMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PlatformManager.MultiPlatform?.User?.UserStatus != EUserStatus.OfflineMode;
		}
	}

	public override void Init()
	{
		base.Init();
		if (TryGetChildByIdAndType<XUiC_Button>("btnPlayGame", out btnPlayGame))
		{
			btnPlayGame.OnPress += btnPlayGame_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnNewGame", out btnNewGame))
		{
			btnNewGame.OnPress += btnNewGame_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnContinueGame", out btnContinueGame))
		{
			btnContinueGame.OnPress += btnContinueGame_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnConnectToServer", out var _child))
		{
			_child.OnPress += btnConnectToServer_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnOptions", out var _child2))
		{
			_child2.OnPress += btnOptions_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnCredits", out var _child3))
		{
			_child3.OnPress += btnCredits_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnNews", out var _child4))
		{
			_child4.OnPress += btnNews_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnQuit", out var _child5))
		{
			_child5.OnPress += btnQuit_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnEditingTools", out var _child6))
		{
			_child6.OnPress += btnEditingTools_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnRWG", out var _child7))
		{
			_child7.OnPress += btnRWG_OnPressed;
		}
		if (TryGetChildByIdAndType<XUiC_Button>("btnDlc", out var _child8))
		{
			_child8.OnPress += btnDlc_OnPressed;
		}
		XUiC_NewsWindow[] childControllers = GetChildControllers<XUiC_NewsWindow>("");
		for (int i = 0; i < childControllers.Length; i++)
		{
			childControllers[i].NewsEntryClicked += NewsBoxOnNewsEntryClicked;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NewsBoxOnNewsEntryClicked(NewsManager.NewsEntry _newsEntry)
	{
		btnNews_OnPressed(this, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnPlayGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			xui.playerUI.windowManager.Open(XUiC_PlayGameMenu.ID, _bModal: true);
		}
		else
		{
			XUiC_EulaWindow.Open(xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNewGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			CheckProfile(XUiC_NewGame.ID);
		}
		else
		{
			XUiC_EulaWindow.Open(xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnContinueGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			CheckProfile(XUiC_ContinueGame.ID);
		}
		else
		{
			XUiC_EulaWindow.Open(xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnConnectToServer_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(xui);
			return;
		}
		XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, [PublicizedFrom(EAccessModifier.Private)] (bool _) =>
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
			xui.playerUI.windowManager.Close(windowGroup);
			xui.playerUI.windowManager.Open(XUiC_EditingTools.ID, _bModal: true);
		}
		else
		{
			XUiC_EulaWindow.Open(xui);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRWG_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(xui);
		}
		else
		{
			XUiC_WorldGenerationWindow.Open(xui, XUiC_MainMenu.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOptions_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.playerUI.windowManager.Open(XUiC_OptionsMenuNew.ParentSelector.WindowGroup, _bModal: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCredits_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.playerUI.windowManager.Open(XUiC_Credits.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNews_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		XUiC_NewsScreen.Open(xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDlc_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.playerUI.windowManager.Open(XUiC_DlcWindowNew.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnQuit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Application.Quit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckProfile(string _windowToOpen)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			XUiC_PlayerProfile.Open(xui, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				xui.playerUI.windowManager.Open(_windowToOpen, _bModal: true);
			});
		}
		else
		{
			xui.playerUI.windowManager.Open(_windowToOpen, _bModal: true);
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
			if (!btnNewGame.ViewComponent.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				break;
			}
			btnNewGame_OnPressed(btnNewGame, -1);
			if (!xui.playerUI.windowManager.IsWindowOpen(XUiC_NewGame.ID))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
			}
			else
			{
				PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			}
			break;
		case EPlatformLoadSaveGameState.ContinueGameOpen:
			if (!btnContinueGame.ViewComponent.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				break;
			}
			btnContinueGame_OnPressed(btnContinueGame, -1);
			if (!xui.playerUI.windowManager.IsWindowOpen(XUiC_ContinueGame.ID))
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

	public override void OnOpen()
	{
		base.OnOpen();
		btnNewGame?.SelectCursorElement(_withDelay: true);
	}
}
