using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordLogin : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[XuiBindComponent("btnOk", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnOk;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnSettings", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCloseWithOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showSettingsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool skipOnSuccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openModal;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cancellable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusText;

	[XuiXmlBinding("status_text")]
	public string StatusText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return statusText ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			statusText = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("show_settings_button")]
	public bool ShowSettingsButton
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return showSettingsButton;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			showSettingsButton = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("done")]
	public bool IsDone
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return isDone;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			isDone = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("cancellable")]
	public bool Cancellable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return cancellable;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			cancellable = value;
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnOk")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOk_OnPress(XUiController _sender, int _mouseButton)
	{
		closeAndOpenNextWindow();
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCancel_OnPress(XUiController _sender, int _mouseButton)
	{
		DiscordManager.Instance.AuthManager.AbortAuth();
	}

	[XuiBindEvent("OnPress", "btnSettings")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSettings_OnPress(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.GetChildByType<XUiC_OptionsAudio>()?.OpenAtTab("xuiOptionsAudioDiscord");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenNextWindow()
	{
		xui.playerUI.windowManager.Close(windowGroup);
		if (onCloseWithOk == null)
		{
			xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
		}
		else
		{
			onCloseWithOk();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.playerUI.nguiWindowManager.Show(EnumNGUIWindow.Loading, _bEnable: false);
		windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		DiscordManager.Instance.UserAuthorizationResult -= authResult;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		updateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInput()
	{
		if (IsDone)
		{
			PlayerActionsGUI gUIActions = xui.playerUI.playerInput.GUIActions;
			if (gUIActions.Apply.WasReleased || gUIActions.Cancel.WasReleased)
			{
				closeAndOpenNextWindow();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void authResult(bool _isDone, DiscordManager.EFullAccountLoginResult _fullAccResult, DiscordManager.EProvisionalAccountLoginResult _provisionalAccResult, bool _isExpectedSuccess)
	{
		LocalPlayerUI playerUI = xui.playerUI;
		playerUI.nguiWindowManager.Show(EnumNGUIWindow.Loading, _bEnable: false);
		if (!windowGroup.isShowing && (_isDone || _fullAccResult == DiscordManager.EFullAccountLoginResult.RequestingAuth))
		{
			if (skipOnSuccess && _isExpectedSuccess)
			{
				DiscordManager.Instance.UserAuthorizationResult -= authResult;
				closeAndOpenNextWindow();
				return;
			}
			openNow(playerUI, openModal);
		}
		updateUi(_isDone, _fullAccResult, _provisionalAccResult, _isExpectedSuccess);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUi(bool _isDone, DiscordManager.EFullAccountLoginResult _fullAccResult, DiscordManager.EProvisionalAccountLoginResult _provisionalAccResult, bool _isExpectedSuccess)
	{
		IsDone = _isDone;
		Cancellable = _fullAccResult == DiscordManager.EFullAccountLoginResult.RequestingAuth;
		StatusText = BuildText();
		RefreshBindings();
		[PublicizedFrom(EAccessModifier.Internal)]
		string BuildFullAccFailureText()
		{
			string text = Localization.Get("xuiDiscordLoginFullAccount" + _fullAccResult.ToStringCached());
			if (_provisionalAccResult == DiscordManager.EProvisionalAccountLoginResult.None || _provisionalAccResult == DiscordManager.EProvisionalAccountLoginResult.NotSupported)
			{
				return text;
			}
			return BuildProvisionalFallbackText(text);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		string BuildProvisionalFallbackText(string _fullAccFailureText)
		{
			string text = Localization.Get("xuiDiscordLoginFallback" + _provisionalAccResult.ToStringCached());
			return _fullAccFailureText + "\n\n" + text;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		string BuildProvisionalOnlyText()
		{
			return Localization.Get("xuiDiscordLoginProvisional" + _provisionalAccResult.ToStringCached());
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		string BuildText()
		{
			if (!_isDone)
			{
				switch (_fullAccResult)
				{
				case DiscordManager.EFullAccountLoginResult.RequestingAuth:
					return Localization.Get("xuiDiscordLoginFullAccountRequestingAuth");
				case DiscordManager.EFullAccountLoginResult.AuthAccepted:
					return Localization.Get("xuiDiscordLoginFullAccountAuthAccepted");
				case DiscordManager.EFullAccountLoginResult.AuthCancelled:
					return Localization.Get("xuiDiscordLoginFullAccountAuthCancelledTryingProvisional");
				case DiscordManager.EFullAccountLoginResult.AuthFailed:
					return Localization.Get("xuiDiscordLoginFullAccountAuthFailedTryingProvisional");
				}
			}
			if (_fullAccResult == DiscordManager.EFullAccountLoginResult.None)
			{
				return BuildProvisionalOnlyText();
			}
			if (_fullAccResult == DiscordManager.EFullAccountLoginResult.Success)
			{
				return Localization.Get("xuiDiscordLoginFullAccountSuccess");
			}
			return BuildFullAccFailureText();
		}
	}

	public static void Open(Action _onCloseWithOk = null, bool _showSettingsButton = true, bool _waitForResultToShow = false, bool _skipOnSuccess = false, bool _modal = true, bool _cancellable = false)
	{
		XUi xUi = LocalPlayerUI.primaryUI.xui;
		LocalPlayerUI playerUI = xUi.playerUI;
		XUiC_DiscordLogin childByType = xUi.GetWindowGroupById(ID).Controller.GetChildByType<XUiC_DiscordLogin>();
		DiscordManager.Instance.UserAuthorizationResult += childByType.authResult;
		childByType.StatusText = Localization.Get("xuiDiscordLoginLoggingIn");
		childByType.IsDone = false;
		childByType.Cancellable = _cancellable;
		childByType.onCloseWithOk = _onCloseWithOk;
		childByType.ShowSettingsButton = _showSettingsButton;
		childByType.skipOnSuccess = _skipOnSuccess;
		childByType.openModal = _modal;
		if (_waitForResultToShow)
		{
			playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiDiscordLoginProgress") + "...", _toUpper: false);
		}
		else
		{
			openNow(playerUI, _modal);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void openNow(LocalPlayerUI _playerUi, bool _modal = true)
	{
		_playerUi.windowManager.Open(ID, _modal, _bIsNotEscClosable: true);
	}
}
