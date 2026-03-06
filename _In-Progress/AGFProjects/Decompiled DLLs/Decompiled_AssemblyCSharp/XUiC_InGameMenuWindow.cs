using System.Collections;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InGameMenuWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnInvite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnHelp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnExit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnExportPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnTpPoi;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUnstuck;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOpenConsole;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBugReport;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ConfirmationPrompt unstuckPrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool continueGamePause;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string ServerInfoWindowGroupName = "serverinfowindow";

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverInfoWindowGroup;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		unstuckPrompt = GetChildByType<XUiC_ConfirmationPrompt>();
		btnInvite = GetChildById("btnInvite").GetChildByType<XUiC_SimpleButton>();
		btnInvite.OnPressed += BtnInvite_OnPressed;
		btnOptions = GetChildById("btnOptions").GetChildByType<XUiC_SimpleButton>();
		btnOptions.OnPressed += BtnOptions_OnPressed;
		btnHelp = GetChildById("btnHelp").GetChildByType<XUiC_SimpleButton>();
		btnHelp.OnPressed += BtnHelp_OnPressed;
		btnSave = GetChildById("btnSave").GetChildByType<XUiC_SimpleButton>();
		btnSave.OnPressed += BtnSave_OnPressed;
		btnExit = GetChildById("btnExit").GetChildByType<XUiC_SimpleButton>();
		btnExit.OnPressed += BtnExit_OnPressed;
		btnExportPrefab = GetChildById("btnExportPrefab").GetChildByType<XUiC_SimpleButton>();
		btnExportPrefab.OnPressed += BtnExportPrefab_OnPressed;
		btnTpPoi = GetChildById("btnTpPoi").GetChildByType<XUiC_SimpleButton>();
		btnTpPoi.OnPressed += BtnTpPoi_OnPressed;
		btnUnstuck = GetChildById("btnUnstuck")?.GetChildByType<XUiC_SimpleButton>();
		if (btnUnstuck != null)
		{
			btnUnstuck.OnPressed += BtnUnstuck_OnPressed;
		}
		btnOpenConsole = GetChildById("btnOpenConsole").GetChildByType<XUiC_SimpleButton>();
		btnOpenConsole.OnPressed += BtnOpenConsole_OnPressed;
		btnBugReport = GetChildById("btnBugReport").GetChildByType<XUiC_SimpleButton>();
		btnBugReport.OnPressed += BtnBugReport_OnPressed;
		XUiController xUiController = base.xui.FindWindowGroupByName(ServerInfoWindowGroupName);
		if (xUiController != null)
		{
			serverInfoWindowGroup = xUiController.WindowGroup.ID;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnInvite_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		PlatformManager.NativePlatform.MultiplayerInvitationDialog?.ShowInviteDialog();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOptions_OnPressed(XUiController _sender, int _mouseButton)
	{
		continueGamePause = true;
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		LocalPlayerUI.primaryUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnHelp_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_PrefabEditorHelp.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.IsActive())
		{
			XUiC_SaveDirtyPrefab.Show(base.xui, savePrefab, XUiC_SaveDirtyPrefab.EMode.ForceSave);
			return;
		}
		GameManager.Instance.SaveLocalPlayerData();
		GameManager.Instance.SaveWorld();
		GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], Localization.Get("xuiWorldEditorSaved"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void savePrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnExit_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.IsActive() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			XUiC_SaveDirtyPrefab.Show(base.xui, exitGame);
		}
		else
		{
			exitGame(XUiC_SaveDirtyPrefab.ESelectedAction.DontSave);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void exitGame(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			base.xui.playerUI.windowManager.Open(ID, _bModal: true);
			return;
		}
		GameManager.Instance.SetActiveBlockTool(null);
		if (PlatformApplicationManager.IsRestartRequired)
		{
			ThreadManager.StartCoroutine(DisconnectAfterDisplayingExitingGameMessage());
		}
		else
		{
			GameManager.Instance.Disconnect();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DisconnectAfterDisplayingExitingGameMessage()
	{
		yield return GameManager.Instance.ShowExitingGameUICoroutine();
		GameManager.Instance.Disconnect();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnExportPrefab_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		XUiC_ExportPrefab.Open(base.xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTpPoi_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_PoiTeleportMenu.ID, _bModal: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUnstuck_OnPressed(XUiController _sender, int _mouseButton)
	{
		unstuckPrompt.ShowPrompt(Localization.Get("xuiMenuUnstuck"), Localization.Get("xuiMenuUnstuckConfirmation"), Localization.Get("xuiCancel"), Localization.Get("btnOk"), UnstuckConfirmed);
		[PublicizedFrom(EAccessModifier.Private)]
		void UnstuckConfirmed(XUiC_ConfirmationPrompt.Result result)
		{
			if (result == XUiC_ConfirmationPrompt.Result.Confirmed)
			{
				base.xui.playerUI.entityPlayer.RequestUnstuck();
				base.xui.playerUI.windowManager.Close(ID);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOpenConsole_OnPressed(XUiController _sender, int _mouseButton)
	{
		GameManager.Instance.SetConsoleWindowVisible(!GameManager.Instance.m_GUIConsole.isShowing);
		base.xui.playerUI.windowManager.Close(ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBugReport_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		XUiC_BugReportWindow.Open(base.xui, _fromMainMenu: false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (PlatformManager.NativePlatform.MultiplayerInvitationDialog != null)
		{
			int num = SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() + ((!GameManager.IsDedicatedServer) ? 1 : 0);
			int num2 = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
			btnInvite.Enabled = PlatformManager.NativePlatform.MultiplayerInvitationDialog.CanShow && (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || num < num2);
			btnInvite.ViewComponent.IsVisible = !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || SingletonMonoBehaviour<ConnectionManager>.Instance.HasRunningServers;
		}
		else
		{
			btnInvite.ViewComponent.IsVisible = false;
		}
		btnSave.ViewComponent.IsVisible = GameManager.Instance.IsEditMode() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		btnHelp.ViewComponent.IsVisible = GameManager.Instance.IsEditMode();
		btnExportPrefab.ViewComponent.IsVisible = GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
		btnTpPoi.ViewComponent.IsVisible = GameManager.Instance.GetDynamicPrefabDecorator() != null && (GameManager.Instance.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		continueGamePause = false;
		GameManager.Instance.Pause(_bOn: true);
		XUiC_FocusedBlockHealth.SetData(base.xui.playerUI, null, 0f);
		base.xui.playerUI.windowManager.Close("toolbelt");
		XUi.InGameMenuOpen = true;
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			base.xui.playerUI.windowManager.Open(XUiC_EditorPanelSelector.ID, _bModal: false);
		}
		if (serverInfoWindowGroup != null)
		{
			base.xui.playerUI.windowManager.Open(serverInfoWindowGroup, _bModal: false);
		}
		if (btnInvite.ViewComponent.IsVisible)
		{
			btnInvite.SelectCursorElement(_withDelay: true);
		}
		else
		{
			btnOptions.SelectCursorElement(_withDelay: true);
		}
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.Close(XUiC_EditorPanelSelector.ID);
		if (!continueGamePause)
		{
			GameManager.Instance.Pause(_bOn: false);
			XUi.InGameMenuOpen = false;
		}
		if (serverInfoWindowGroup != null)
		{
			base.xui.playerUI.windowManager.Close(serverInfoWindowGroup);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (PrefabEditModeManager.Instance.IsActive())
		{
			bool enabled = PrefabEditModeManager.Instance.VoxelPrefab != null;
			btnSave.Enabled = enabled;
		}
		btnExportPrefab.Enabled = BlockToolSelection.Instance.SelectionActive;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "creativeenabled":
			_value = AchievementUtils.IsCreativeModeActive().ToString();
			return true;
		case "bug_reporting":
			_value = BacktraceUtils.BugReportFeature.ToString();
			return true;
		case "console_button":
			_value = GamePrefs.GetBool(EnumGamePrefs.OptionsShowConsoleButton).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
