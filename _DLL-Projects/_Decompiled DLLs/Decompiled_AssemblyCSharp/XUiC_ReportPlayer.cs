using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ReportPlayer : XUiController
{
	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string initialText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static EnumReportCategory initialCategory = EnumReportCategory.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblReportedPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<IPlayerReporting.PlayerReportCategory> cbxCategory;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnKickPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inputErrorMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerData reportedPlayerData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string windowOnClose;

	public static void Open(PlayerData _reportedPlayerData, string _windowOnClose = "")
	{
		if (!LocalPlayerUI.primaryUI.windowManager.IsWindowOpen(ID))
		{
			XUiC_ReportPlayer childByType = ((XUiWindowGroup)LocalPlayerUI.primaryUI.windowManager.GetWindow(ID)).Controller.GetChildByType<XUiC_ReportPlayer>();
			childByType.reportedPlayerData = _reportedPlayerData;
			childByType.windowOnClose = _windowOnClose;
			initialText = "";
			LocalPlayerUI.primaryUI.windowManager.Open(ID, _bModal: true);
		}
	}

	public static void Open(PlayerData _reportedPlayerData, EnumReportCategory _initialCategory, string _intialText, string _windowOnClose = "")
	{
		initialCategory = _initialCategory;
		initialText = _intialText;
		Open(_reportedPlayerData, _windowOnClose);
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		lblReportedPlayer = (XUiV_Label)GetChildById("lblReportedPlayer").ViewComponent;
		cbxCategory = (XUiC_ComboBoxList<IPlayerReporting.PlayerReportCategory>)GetChildById("cbxCategory");
		IList<IPlayerReporting.PlayerReportCategory> list = PlatformManager.MultiPlatform.PlayerReporting?.ReportCategories();
		if (list != null)
		{
			foreach (IPlayerReporting.PlayerReportCategory item in list)
			{
				cbxCategory.Elements.Add(item);
			}
		}
		txtMessage = (XUiC_TextInput)GetChildById("txtMessage");
		txtMessage.OnInputErrorHandler += UpdateErrorMessage;
		((XUiC_SimpleButton)GetChildById("btnSend")).OnPressed += BtnSend_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
		btnKickPlayer = (XUiC_SimpleButton)GetChildById("btnKick");
		btnKickPlayer.OnPressed += BtnKick_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnKick_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (reportedPlayerData != null)
		{
			if (ConsoleHelper.ParseParamPartialNameOrId(reportedPlayerData.PlayerName.Text, out var _, out var _cInfo) == 1)
			{
				DateTime maxValue = DateTime.MaxValue;
				string text = "";
				if (_cInfo != null)
				{
					ClientInfo cInfo = _cInfo;
					string customReason = (string.IsNullOrEmpty(text) ? "" : text);
					GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, maxValue, customReason));
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[xui] Kick Succeeded");
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[xui] Failed to find player to kick");
		}
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSend_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (reportedPlayerData == null)
		{
			ReportSentMessageBox(_success: false);
			return;
		}
		string text = txtMessage.Text;
		if (!string.IsNullOrEmpty(initialText))
		{
			text = text + "\n" + string.Format(Localization.Get("xuiReportAutomatedMessage"), initialText, initialCategory);
		}
		PlatformManager.MultiPlatform.PlayerReporting?.ReportPlayer(reportedPlayerData.PrimaryId, cbxCategory.Value, text, ReportSentMessageBox);
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReportSentMessageBox(bool _success)
	{
		string text = (_success ? Localization.Get("xuiReportPlayerSuccess") : Localization.Get("xuiReportPlayerFail"));
		XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiReportPlayerHeader"), text);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		txtMessage.Text = initialText;
		inputErrorMessage = null;
		cbxCategory.SelectedIndex = cbxCategory.Elements.IndexOf(PlatformManager.MultiPlatform.PlayerReporting?.GetPlayerReportCategoryMapping(initialCategory));
		if (cbxCategory.SelectedIndex == -1)
		{
			cbxCategory.SelectedIndex = 0;
		}
		if (reportedPlayerData != null)
		{
			lblReportedPlayer.Text = GeneratedTextManager.GetDisplayTextImmediately(reportedPlayerData.PlayerName, _checkBlockState: false);
			int num = GameManager.Instance.persistentPlayers?.GetPlayerData(reportedPlayerData.PrimaryId)?.EntityId ?? (-1);
			btnKickPlayer.ViewComponent.IsVisible = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && num != -1;
		}
		else
		{
			Log.Out("Sign does not have an author, cannot report player.");
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		reportedPlayerData = null;
		if (!string.IsNullOrEmpty(windowOnClose))
		{
			LocalPlayerUI.primaryUI.windowManager.Open(windowOnClose, _bModal: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "inputWarning")
		{
			_value = inputErrorMessage;
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateErrorMessage(XUiController _sender, string _errorMessage)
	{
		inputErrorMessage = _errorMessage;
		RefreshBindings();
	}
}
