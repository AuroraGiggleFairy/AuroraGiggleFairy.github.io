using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MessageBoxWindowGroup : XUiController
{
	public enum MessageBoxTypes
	{
		Ok,
		OkCancel
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string title = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string text = "";

	public static string ID = "";

	public bool OpenMainMenuOnClose = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public MessageBoxTypes MessageBoxType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool leftButtonPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool rightButtonPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView returnNavigationTarget = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public (GUIWindow prevModalWindow, string url, Func<string, bool> browserOpenMethod) urlBoxData;

	public string Title
	{
		get
		{
			return title;
		}
		set
		{
			title = value;
			IsDirty = true;
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
			IsDirty = true;
		}
	}

	public event Action OnLeftButtonEvent;

	public event Action OnRightButtonEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "msgTitle":
			_value = title;
			return true;
		case "msgText":
			_value = text;
			return true;
		case "showleftbutton":
			_value = (MessageBoxType == MessageBoxTypes.OkCancel).ToString();
			return true;
		case "rightbuttontext":
			_value = ((MessageBoxType == MessageBoxTypes.Ok) ? "xuiOk" : "xuiCancel");
			return true;
		case "leftbuttontext":
			_value = ((MessageBoxType == MessageBoxTypes.Ok) ? "" : "xuiOk");
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnLeft = GetChildById("clickable2");
		if (btnLeft != null)
		{
			((XUiV_Button)btnLeft.ViewComponent).Controller.OnPress += leftButton_OnPress;
		}
		btnRight = GetChildById("clickable");
		if (btnRight != null)
		{
			((XUiV_Button)btnRight.ViewComponent).Controller.OnPress += rightButton_OnPress;
		}
		leftButtonPressed = false;
		rightButtonPressed = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void leftButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.OnLeftButtonEvent != null)
		{
			this.OnLeftButtonEvent();
		}
		leftButtonPressed = true;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rightButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.OnRightButtonEvent != null)
		{
			this.OnRightButtonEvent();
		}
		rightButtonPressed = true;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
		{
			rightButton_OnPress(this, -1);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		leftButtonPressed = false;
		rightButtonPressed = false;
		windowGroup.isEscClosable = false;
		base.xui.playerUI.CursorController.Locked = false;
		base.xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
		if (!OpenMainMenuOnClose)
		{
			base.xui.playerUI.CursorController.SetNavigationTargetLater(returnNavigationTarget);
		}
		if (MessageBoxType == MessageBoxTypes.OkCancel && !rightButtonPressed && !leftButtonPressed)
		{
			this.OnRightButtonEvent?.Invoke();
		}
		if (GameManager.Instance.World == null)
		{
			if (OpenMainMenuOnClose)
			{
				base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
			}
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine());
		}
	}

	public void ShowMessage(string _title, string _text, MessageBoxTypes _messageBoxType = MessageBoxTypes.Ok, Action _onLeftButton = null, Action _onRightButton = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _bCloseAllOpenWindows = true)
	{
		Text = _text;
		Title = _title;
		MessageBoxType = _messageBoxType;
		this.OnLeftButtonEvent = _onLeftButton;
		this.OnRightButtonEvent = _onRightButton;
		OpenMainMenuOnClose = _openMainMenuOnClose;
		if (windowGroup.isShowing)
		{
			OnOpen();
		}
		else
		{
			base.xui.playerUI.windowManager.Open(base.WindowGroup.ID, _modal, _bIsNotEscClosable: false, _bCloseAllOpenWindows);
		}
	}

	public void ShowNetworkError(NetworkConnectionError _error)
	{
		string arg = _error.ToStringCached();
		ShowMessage(_text: _error switch
		{
			NetworkConnectionError.InvalidPassword => Localization.Get("mmLblErrorWrongPassword"), 
			NetworkConnectionError.CreateSocketOrThreadFailure => string.Format(Localization.Get("mmLblErrorSocketFailure"), SingletonMonoBehaviour<ConnectionManager>.Instance.GetRequiredPortsString()), 
			NetworkConnectionError.RestartRequired => string.Format(Localization.Get("app_restartRequired")), 
			_ => string.Format(Localization.Get("mmLblErrorUnknown"), arg), 
		}, _title: Localization.Get("mmLblErrorServerInit"));
	}

	public void ShowUrlConfirmationDialog(string _url, string _displayUrl, bool _modal = false, Func<string, bool> _browserOpenMethod = null, string _title = null, string _text = null)
	{
		if (!string.IsNullOrEmpty(_url) && Utils.IsValidWebUrl(ref _url))
		{
			urlBoxData.prevModalWindow = base.xui.playerUI.windowManager.GetModalWindow();
			urlBoxData.url = _url;
			if (_browserOpenMethod == null)
			{
				_browserOpenMethod = ((PlatformManager.NativePlatform.Utils != null) ? new Func<string, bool>(PlatformManager.NativePlatform.Utils.OpenBrowser) : null);
			}
			urlBoxData.browserOpenMethod = _browserOpenMethod;
			if (_title == null)
			{
				_title = Localization.Get("xuiOpenUrlConfirmationTitle");
			}
			if (_text == null)
			{
				_text = Localization.Get("xuiOpenUrlConfirmationText");
			}
			_text = string.Format(_text, _displayUrl);
			bool openMainMenuOnClose = false;
			if (windowGroup.isShowing)
			{
				_modal = windowGroup.isModal;
				openMainMenuOnClose = OpenMainMenuOnClose;
			}
			ShowMessage(_title, _text, MessageBoxTypes.OkCancel, openPage, cancelOpenPage, openMainMenuOnClose, _modal);
			base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openPage()
	{
		urlBoxData.prevModalWindow = null;
		base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = true;
		urlBoxData.browserOpenMethod?.Invoke(urlBoxData.url);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cancelOpenPage()
	{
		urlBoxData.prevModalWindow = null;
		if (base.xui?.playerUI?.playerInput?.PermanentActions != null)
		{
			base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = true;
		}
	}

	public static void ShowMessageBox(XUi _xuiInstance, string _title, string _text, MessageBoxTypes _messageBoxType = MessageBoxTypes.Ok, Action _onOk = null, Action _onCancel = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _bCloseAllOpenWindows = true)
	{
		returnNavigationTarget = _xuiInstance.playerUI.CursorController.navigationTarget;
		((XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(ID)).ShowMessage(_title, _text, _messageBoxType, _onOk, _onCancel, _openMainMenuOnClose, _modal, _bCloseAllOpenWindows);
	}

	public static void ShowMessageBox(XUi _xuiInstance, string _title, string _text, Action _onOk = null, bool _openMainMenuOnClose = true, bool _modal = true)
	{
		returnNavigationTarget = _xuiInstance.playerUI.CursorController.navigationTarget;
		((XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(ID)).ShowMessage(_title, _text, MessageBoxTypes.Ok, null, _onOk, _openMainMenuOnClose, _modal);
	}

	public static void ShowUrlConfirmationDialog(XUi _xuiInstance, string _url, bool _modal = false, Func<string, bool> _browserOpenMethod = null, string _title = null, string _text = null, string _displayUrl = null)
	{
		returnNavigationTarget = _xuiInstance.playerUI.CursorController.navigationTarget;
		XUiC_MessageBoxWindowGroup obj = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(ID);
		if (_displayUrl == null)
		{
			_displayUrl = _url;
		}
		obj.ShowUrlConfirmationDialog(_url, _displayUrl, _modal, _browserOpenMethod, _title, _text);
	}
}
