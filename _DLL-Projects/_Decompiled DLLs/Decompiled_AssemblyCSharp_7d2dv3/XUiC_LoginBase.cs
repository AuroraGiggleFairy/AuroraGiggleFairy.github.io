using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoginBase : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[XuiBindComponent("btnRetry", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnRetry;

	[XuiBindComponent("btnOffline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnOffline;

	[XuiBindComponent("btnExit", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnExit;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform offendingPlatform;

	[PublicizedFrom(EAccessModifier.Private)]
	public EApiStatusReason statusReason;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusReasonAdditionalText;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wantOffline;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onLoginComplete;

	public virtual string MsgTitleKey
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return "xuiSteamLogin";
		}
	}

	[XuiXmlBinding("title")]
	public string MsgTitle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (offendingPlatform != null)
			{
				return string.Format(Localization.Get(MsgTitleKey), offendingPlatform.PlatformDisplayName);
			}
			return "";
		}
	}

	[XuiXmlBinding("caption")]
	public string MsgCaption
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (offendingPlatform != null)
			{
				return string.Format(Localization.Get("xuiSteamLoginFailure"), offendingPlatform.PlatformDisplayName);
			}
			return "";
		}
	}

	[XuiXmlBinding("reason")]
	public string MsgReason
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (offendingPlatform != null)
			{
				return string.Format(Localization.Get("xuiSteamLoginReason" + statusReason.ToStringCached()), offendingPlatform.PlatformDisplayName, statusReasonAdditionalText);
			}
			return "";
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnRetry")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRetry_OnPressed(XUiController _sender, int _mouseButton)
	{
		btnOffline.ViewComponent.Enabled = false;
		btnRetry.ViewComponent.Enabled = false;
		if (btnExit != null)
		{
			btnExit.ViewComponent.Enabled = false;
		}
		offendingPlatform = null;
		wantOffline = false;
		RefreshBindings();
		_sender.xui.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiSteamLoginProgressSignIn") + "...", _toUpper: false);
		PlatformManager.MultiPlatform.User.Login(updateState);
	}

	[XuiBindEvent("OnPress", "btnOffline")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOffline_OnPressed(XUiController _sender, int _mouseButton)
	{
		btnOffline.ViewComponent.Enabled = false;
		btnRetry.ViewComponent.Enabled = false;
		if (btnExit != null)
		{
			btnExit.ViewComponent.Enabled = false;
		}
		offendingPlatform = null;
		RefreshBindings();
		wantOffline = true;
		PlatformManager.MultiPlatform.User.PlayOffline(updateState);
	}

	[XuiBindEvent("OnPress", "btnExit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnExit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Application.Quit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateState(IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText)
	{
		if (_platform.User.UserStatus == EUserStatus.LoggedIn || (wantOffline && _platform.User.UserStatus == EUserStatus.OfflineMode))
		{
			xui.playerUI.windowManager.Close(windowGroup);
			onLoginComplete?.Invoke();
			onLoginComplete = null;
			return;
		}
		xui.playerUI.windowManager.Open(windowGroup, _bModal: true, _bIsNotEscClosable: true);
		btnRetry.ViewComponent.Enabled = _platform.Api.ClientApiStatus != EApiStatus.PermanentError;
		btnOffline.ViewComponent.Enabled = _platform.User.UserStatus == EUserStatus.OfflineMode;
		if (btnExit != null)
		{
			btnExit.ViewComponent.Enabled = true;
		}
		offendingPlatform = _platform;
		statusReason = _statusReason;
		statusReasonAdditionalText = _statusReasonAdditionalText;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Open(XUi _xuiInstance, IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText, Action _onLoginComplete)
	{
		XUiC_LoginBase childByType = _xuiInstance.FindWindowGroupByName(ID).GetChildByType<XUiC_LoginBase>();
		childByType.onLoginComplete = _onLoginComplete;
		childByType.updateState(_platform, _statusReason, _statusReasonAdditionalText);
	}

	public static void Login(XUi _xuiInstance, Action _onLoginComplete)
	{
		_xuiInstance.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiSteamLoginProgressSignIn") + "...", _toUpper: false);
		PlatformManager.MultiPlatform.User.Login([PublicizedFrom(EAccessModifier.Internal)] (IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText) =>
		{
			if (_platform.Api.ClientApiStatus != EApiStatus.Ok)
			{
				Open(_xuiInstance, _platform, _statusReason, _statusReasonAdditionalText, _onLoginComplete);
			}
			else if (_platform.User.UserStatus != EUserStatus.LoggedIn || _statusReason != EApiStatusReason.Ok)
			{
				Open(_xuiInstance, _platform, _statusReason, _statusReasonAdditionalText, _onLoginComplete);
			}
			else
			{
				_onLoginComplete();
			}
		});
	}
}
