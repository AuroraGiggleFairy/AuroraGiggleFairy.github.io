using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordLogin : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

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
	public string text;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		if (GetChildById("btnOk") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				closeAndOpenNextWindow();
			};
		}
		if (GetChildById("btnCancel") is XUiC_SimpleButton xUiC_SimpleButton2)
		{
			xUiC_SimpleButton2.OnPressed += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _, int _) =>
			{
				DiscordManager.Instance.AuthManager.AbortAuth();
			};
		}
		if (GetChildById("btnSettings") is XUiC_SimpleButton xUiC_SimpleButton3)
		{
			xUiC_SimpleButton3.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				base.xui.playerUI.windowManager.Close(ID);
				base.xui.GetChildByType<XUiC_OptionsAudio>()?.OpenAtTab("xuiOptionsAudioDiscord");
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenNextWindow()
	{
		base.xui.playerUI.windowManager.Close(ID);
		if (onCloseWithOk == null)
		{
			base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
		}
		else
		{
			onCloseWithOk();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.nguiWindowManager.Show(EnumNGUIWindow.Loading, _bEnable: false);
		windowGroup.isEscClosable = false;
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		DiscordManager.Instance.UserAuthorizationResult -= authResult;
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (isDone)
		{
			PlayerActionsGUI gUIActions = base.xui.playerUI.playerInput.GUIActions;
			if (gUIActions.Apply.WasReleased || gUIActions.Cancel.WasReleased)
			{
				closeAndOpenNextWindow();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void authResult(bool _isDone, DiscordManager.EFullAccountLoginResult _fullAccResult, DiscordManager.EProvisionalAccountLoginResult _provisionalAccResult, bool _isExpectedSuccess)
	{
		LocalPlayerUI playerUI = base.xui.playerUI;
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
		isDone = _isDone;
		cancellable = _fullAccResult == DiscordManager.EFullAccountLoginResult.RequestingAuth;
		text = BuildText();
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "status_text":
			_value = text ?? "";
			return true;
		case "show_settings_button":
			_value = showSettingsButton.ToString();
			return true;
		case "done":
			_value = isDone.ToString();
			return true;
		case "cancellable":
			_value = cancellable.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public static void Open(Action _onCloseWithOk = null, bool _showSettingsButton = true, bool _waitForResultToShow = false, bool _skipOnSuccess = false, bool _modal = true, bool _cancellable = false)
	{
		XUi xUi = LocalPlayerUI.primaryUI.xui;
		LocalPlayerUI playerUI = xUi.playerUI;
		XUiC_DiscordLogin childByType = xUi.GetWindowGroupById(ID).Controller.GetChildByType<XUiC_DiscordLogin>();
		DiscordManager.Instance.UserAuthorizationResult += childByType.authResult;
		childByType.text = Localization.Get("xuiDiscordLoginLoggingIn");
		childByType.isDone = false;
		childByType.cancellable = _cancellable;
		childByType.onCloseWithOk = _onCloseWithOk;
		childByType.showSettingsButton = _showSettingsButton;
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
