using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoginXBOX : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRetry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOffline;

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

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnRetry = (XUiC_SimpleButton)GetChildById("btnRetry");
		btnRetry.OnPressed += BtnRetry_OnPressed;
		btnOffline = (XUiC_SimpleButton)GetChildById("btnOffline");
		btnOffline.OnPressed += BtnOffline_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "title":
			_value = ((offendingPlatform == null) ? "" : string.Format(Localization.Get("xuiSteamLogin"), offendingPlatform.PlatformDisplayName));
			return true;
		case "caption":
			_value = ((offendingPlatform == null) ? "" : string.Format(Localization.Get("xuiSteamLoginFailure"), offendingPlatform.PlatformDisplayName));
			return true;
		case "reason":
			_value = ((offendingPlatform == null) ? "" : string.Format(Localization.Get("xuiSteamLoginReason" + statusReason.ToStringCached()), offendingPlatform.PlatformDisplayName, statusReasonAdditionalText));
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRetry_OnPressed(XUiController _sender, int _mouseButton)
	{
		btnOffline.Enabled = false;
		btnRetry.Enabled = false;
		offendingPlatform = null;
		wantOffline = false;
		RefreshBindings();
		_sender.xui.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiSteamLoginProgressSignIn") + "...", _toUpper: false);
		PlatformManager.MultiPlatform.User.Login(updateState);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOffline_OnPressed(XUiController _sender, int _mouseButton)
	{
		btnOffline.Enabled = false;
		btnRetry.Enabled = false;
		offendingPlatform = null;
		RefreshBindings();
		wantOffline = true;
		PlatformManager.MultiPlatform.User.PlayOffline(updateState);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateState(IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText)
	{
		if (_platform.User.UserStatus == EUserStatus.LoggedIn || (wantOffline && _platform.User.UserStatus == EUserStatus.OfflineMode))
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
			onLoginComplete?.Invoke();
			onLoginComplete = null;
			return;
		}
		base.xui.playerUI.windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: true);
		btnRetry.Enabled = _platform.Api.ClientApiStatus != EApiStatus.PermanentError;
		btnOffline.Enabled = _platform.User.UserStatus == EUserStatus.OfflineMode;
		offendingPlatform = _platform;
		statusReason = _statusReason;
		statusReasonAdditionalText = _statusReasonAdditionalText;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Open(XUi _xuiInstance, IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText, Action _onLoginComplete)
	{
		XUiC_LoginXBOX childByType = _xuiInstance.FindWindowGroupByName(ID).GetChildByType<XUiC_LoginXBOX>();
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
