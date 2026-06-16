using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ReportPlayer : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string id;

	[PublicizedFrom(EAccessModifier.Private)]
	public string initialText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumReportCategory initialCategory;

	[XuiBindComponent("cbxCategory", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPlayerReporting.PlayerReportCategory> cbxCategory;

	[XuiBindComponent("txtMessage", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtMessage;

	[XuiBindComponent("btnKick", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnKickPlayer;

	[XuiBindComponent("btnSend", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSend;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public string reportedPlayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public int reportedEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inputErrorMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerData reportedPlayerData;

	[XuiXmlBinding("reportedplayer")]
	public string ReportedPlayerName
	{
		get
		{
			return reportedPlayerName ?? "";
		}
		set
		{
			reportedPlayerName = value;
			IsDirty = true;
		}
	}

	public int ReportedEntityId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return reportedEntityId;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			reportedEntityId = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("playeronserver")]
	public bool PlayerOnServer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return ReportedEntityId != -1;
			}
			return false;
		}
	}

	[XuiXmlBinding("inputWarning")]
	public string InputErrorMessage
	{
		get
		{
			return inputErrorMessage ?? "";
		}
		set
		{
			inputErrorMessage = value;
			IsDirty = true;
		}
	}

	public static bool OpenDialogsModal
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.gameStateManager.IsGameStarted();
		}
	}

	public override void Init()
	{
		base.Init();
		id = base.WindowGroup.Id;
		IList<IPlayerReporting.PlayerReportCategory> list = PlatformManager.MultiPlatform.PlayerReporting?.ReportCategories();
		if (list == null)
		{
			return;
		}
		foreach (IPlayerReporting.PlayerReportCategory item in list)
		{
			cbxCategory.Elements.Add(item);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[XuiBindEvent("OnPress", "btnKickPlayer")]
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
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[XuiBindEvent("OnPress", "btnSend")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSend_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (reportedPlayerData == null)
		{
			reportSentMessageBox(_success: false);
			return;
		}
		string text = txtMessage.Text;
		if (!string.IsNullOrEmpty(initialText))
		{
			text = text + "\n" + string.Format(Localization.Get("xuiReportAutomatedMessage"), initialText, initialCategory);
		}
		PlatformManager.MultiPlatform.PlayerReporting?.ReportPlayer(reportedPlayerData.PrimaryId, cbxCategory.Value, text, reportSentMessageBox);
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void reportSentMessageBox(bool _success)
	{
		string text = (_success ? Localization.Get("xuiReportPlayerSuccess") : Localization.Get("xuiReportPlayerFail"));
		XUiC_MessageBoxWindowGroup.ShowOk(xui, Localization.Get("xuiReportPlayerHeader"), text, null, _openMainMenuOnClose: false, OpenDialogsModal);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		txtMessage.Text = initialText;
		InputErrorMessage = null;
		cbxCategory.SelectedIndex = cbxCategory.Elements.IndexOf(PlatformManager.MultiPlatform.PlayerReporting?.GetPlayerReportCategoryMapping(initialCategory));
		if (cbxCategory.SelectedIndex == -1)
		{
			cbxCategory.SelectedIndex = 0;
		}
		if (reportedPlayerData != null)
		{
			ReportedPlayerName = GeneratedTextManager.GetDisplayTextImmediately(reportedPlayerData.PlayerName, _checkBlockState: false);
			ReportedEntityId = GameManager.Instance.persistentPlayers?.GetPlayerData(reportedPlayerData.PrimaryId)?.EntityId ?? (-1);
		}
		else
		{
			Log.Out("Sign does not have an author, cannot report player.");
			xui.playerUI.windowManager.Close(windowGroup);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		reportedPlayerData = null;
	}

	[XuiBindEvent("OnInputErrorHandler", "txtMessage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void updateErrorMessage(XUiController _sender, string _errorMessage)
	{
		InputErrorMessage = _errorMessage;
	}

	public static void Open(PlayerData _reportedPlayerData, EnumReportCategory _initialCategory = EnumReportCategory.None, string _initialText = "")
	{
		if (!LocalPlayerUI.primaryUI.windowManager.IsWindowOpen(id))
		{
			XUiC_ReportPlayer childByType = LocalPlayerUI.primaryUI.xui.GetChildByType<XUiC_ReportPlayer>();
			childByType.reportedPlayerData = _reportedPlayerData;
			childByType.initialCategory = _initialCategory;
			childByType.initialText = _initialText;
			LocalPlayerUI.primaryUI.windowManager.Open(childByType.windowGroup, OpenDialogsModal);
		}
	}
}
