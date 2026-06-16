using System;
using System.Collections;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_MessageBoxWindowGroup : XUiController
{
	[UnityEngine.Scripting.Preserve]
	public class MessageButton : XUiController
	{
		[XuiBindComponent(true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_Button button;

		[PublicizedFrom(EAccessModifier.Private)]
		public string caption;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerAction hotkey;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action callback;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool enabled;

		[PublicizedFrom(EAccessModifier.Private)]
		public float enabledDelay;

		[PublicizedFrom(EAccessModifier.Private)]
		public float holdTime;

		[XuiXmlBinding("msg_btn_used")]
		public bool IsUsed => !string.IsNullOrEmpty(Caption);

		[XuiXmlBinding("msg_btn_caption_raw")]
		public string Caption
		{
			get
			{
				return caption ?? "";
			}
			set
			{
				if (!(caption == value))
				{
					caption = value;
					IsDirty = true;
				}
			}
		}

		[XuiXmlBinding("msg_btn_caption")]
		public string CaptionWithSuffix => Caption + (HoldToConfirm ? (" (" + Localization.Get("xuiHoldButton") + ")") : string.Empty);

		public PlayerAction Hotkey
		{
			get
			{
				return hotkey;
			}
			set
			{
				if (hotkey != value)
				{
					hotkey = value;
					IsDirty = true;
				}
			}
		}

		[XuiXmlBinding("msg_btn_action")]
		public string HotkeyAction => hotkey?.Name ?? "";

		[XuiXmlBinding("msg_btn_hashotkey")]
		public bool HasHotkey => hotkey != null;

		public Action Callback
		{
			get
			{
				return callback;
			}
			set
			{
				callback = value;
			}
		}

		[XuiXmlBinding("msg_btn_enabled")]
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (enabled != value)
				{
					enabled = value;
					IsDirty = true;
				}
			}
		}

		[XuiXmlBinding("msg_btn_enableddelay")]
		public float EnabledDelay
		{
			get
			{
				return enabledDelay;
			}
			set
			{
				if (!Mathf.Approximately(enabledDelay, value))
				{
					enabledDelay = value;
					IsDirty = true;
				}
			}
		}

		[XuiXmlBinding("msg_btn_hold_time")]
		public float HoldTime
		{
			get
			{
				return holdTime;
			}
			set
			{
				if (!Mathf.Approximately(holdTime, value))
				{
					holdTime = value;
					IsDirty = true;
				}
			}
		}

		public bool HoldToConfirm => HoldTime > 0f;

		public void Clear()
		{
			Caption = "";
			Hotkey = null;
			Callback = null;
			Enabled = false;
			EnabledDelay = 0f;
			HoldTime = 0f;
		}

		public void DefaultConfirm(string _captionKey, Action _callback, bool _enabled = true, float _enabledDelay = 0f, float _holdTime = 0f, bool _holdTimeOnKbdMouse = false)
		{
			Set(_captionKey, xui.playerUI.playerInput.GUIActions.Submit, _callback, _enabled, _enabledDelay, _holdTime, _holdTimeOnKbdMouse);
		}

		public void DefaultCancel(string _captionKey, Action _callback, bool _enabled = true, float _enabledDelay = 0f, float _holdTime = 0f, bool _holdTimeOnKbdMouse = false)
		{
			Set(_captionKey, xui.playerUI.playerInput.GUIActions.Cancel, _callback, _enabled, _enabledDelay, _holdTime, _holdTimeOnKbdMouse);
		}

		public void Set(string _captionKey, PlayerAction _hotkey, Action _callback, bool _enabled = true, float _enabledDelay = 0f, float _holdTime = 0f, bool _holdTimeOnKbdMouse = false)
		{
			Caption = Localization.Get(_captionKey);
			Hotkey = _hotkey;
			Callback = _callback;
			Enabled = _enabled;
			EnabledDelay = _enabledDelay;
			if (!_holdTimeOnKbdMouse && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				HoldTime = 0f;
			}
			else
			{
				HoldTime = _holdTime;
			}
		}

		[XuiBindEvent("OnPress", "button")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void OnButtonPressed(XUiController _sender, int _mouseButton)
		{
			Execute();
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			if (EnabledDelay > 0f)
			{
				EnabledDelay -= _dt;
				if (EnabledDelay <= 0f)
				{
					Enabled = true;
				}
			}
			handleDirtyUpdateDefault();
			if (hotkey != null && Enabled)
			{
				if (hotkey.WasPressed)
				{
					button.MouseUpDown(_pressed: true);
				}
				if (hotkey.WasReleased)
				{
					button.MouseUpDown(_pressed: false);
					button.Pressed(-1);
				}
			}
		}

		public void Execute()
		{
			ThreadManager.StartCoroutine(executeCallbackLater(Callback));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator executeCallbackLater(Action _callback)
		{
			yield return null;
			xui.playerUI.windowManager.Close(windowGroup);
			_callback?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id = "";

	[XuiBindComponent("buttons.", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MessageButton[] buttons;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_FullScreenCollider fullScreenCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pressButtonOnOutsideClick = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openMainMenuOnClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool windowGroupEscCloseableBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public (GUIWindow prevModalWindow, string url, Func<string, bool> browserOpenMethod) urlBoxData;

	[XuiXmlBinding("msgTitle")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Title
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("msgText")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Text
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	public MessageButton[] Buttons => buttons;

	public override void Init()
	{
		base.Init();
		id = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		windowGroup.isEscClosable = false;
		xui.playerUI.CursorController.Locked = false;
		xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!openMainMenuOnClose && windowGroupEscCloseableBefore)
		{
			GUIWindow modalWindow = xui.playerUI.windowManager.GetModalWindow();
			if (modalWindow != null)
			{
				modalWindow.isEscClosable = true;
			}
		}
		if (GameManager.Instance.World == null && openMainMenuOnClose)
		{
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine());
			xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
		}
	}

	[XuiBindEvent("OnPress", "fullScreenCollider")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void FullScreenCollider_OnPress(XUiController _sender, int _mouseButton)
	{
		if (pressButtonOnOutsideClick >= 0 && pressButtonOnOutsideClick < buttons.Length)
		{
			MessageButton messageButton = buttons[pressButtonOnOutsideClick];
			if (messageButton.IsUsed && messageButton.Enabled)
			{
				messageButton.Execute();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showMessage(string _title, string _text, bool _openMainMenuOnClose = true, bool _modal = true, int _buttonOnOutsideClick = -1)
	{
		Text = _text;
		Title = _title;
		pressButtonOnOutsideClick = _buttonOnOutsideClick;
		openMainMenuOnClose = _openMainMenuOnClose;
		GUIWindow modalWindow = xui.playerUI.windowManager.GetModalWindow();
		if (modalWindow != null)
		{
			windowGroupEscCloseableBefore = modalWindow.isEscClosable;
			modalWindow.isEscClosable = false;
		}
		if (windowGroup.isShowing)
		{
			OnOpen();
		}
		else
		{
			xui.playerUI.windowManager.Open(base.WindowGroup, _modal);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openPage()
	{
		urlBoxData.prevModalWindow = null;
		urlBoxData.browserOpenMethod?.Invoke(urlBoxData.url);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cancelOpenPage()
	{
		urlBoxData.prevModalWindow = null;
	}

	public static void ShowCustom(XUi _xuiInstance, string _title, string _text, Action<XUiC_MessageBoxWindowGroup> _setupCallback, bool _openMainMenuOnClose = true, bool _modal = true, int _buttonOnOutsideClick = -1)
	{
		XUiC_MessageBoxWindowGroup xUiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(id);
		for (int i = 0; i < xUiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xUiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		_setupCallback?.Invoke(xUiC_MessageBoxWindowGroup);
		xUiC_MessageBoxWindowGroup.showMessage(_title, _text, _openMainMenuOnClose, _modal);
	}

	public static void ShowOk(XUi _xuiInstance, string _title, string _text, Action _onOk = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _confirmOnOutsideClick = false)
	{
		XUiC_MessageBoxWindowGroup xUiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(id);
		for (int i = 0; i < xUiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xUiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		xUiC_MessageBoxWindowGroup.buttons[0].DefaultConfirm("xuiOk", _onOk);
		xUiC_MessageBoxWindowGroup.showMessage(_title, _text, _openMainMenuOnClose, _modal, (!_confirmOnOutsideClick) ? (-1) : 0);
	}

	public static void ShowOkCancel(XUi _xuiInstance, string _title, string _text, Action _onOk = null, Action _onCancel = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _cancelOnOutsideClick = false)
	{
		XUiC_MessageBoxWindowGroup xUiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(id);
		for (int i = 0; i < xUiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xUiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		xUiC_MessageBoxWindowGroup.buttons[0].DefaultConfirm("xuiOk", _onOk);
		xUiC_MessageBoxWindowGroup.buttons[2].DefaultCancel("xuiCancel", _onCancel);
		xUiC_MessageBoxWindowGroup.showMessage(_title, _text, _openMainMenuOnClose, _modal, _cancelOnOutsideClick ? 2 : (-1));
	}

	public static void ShowConfirmCancel(XUi _xuiInstance, string _title, string _text, Action _onConfirm = null, Action _onCancel = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _cancelOnOutsideClick = false)
	{
		XUiC_MessageBoxWindowGroup xUiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(id);
		for (int i = 0; i < xUiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xUiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		xUiC_MessageBoxWindowGroup.buttons[0].DefaultConfirm("btnConfirm", _onConfirm);
		xUiC_MessageBoxWindowGroup.buttons[2].DefaultCancel("xuiCancel", _onCancel);
		xUiC_MessageBoxWindowGroup.showMessage(_title, _text, _openMainMenuOnClose, _modal, _cancelOnOutsideClick ? 2 : (-1));
	}

	public static void ShowUrlConfirmationDialog(XUi _xuiInstance, string _url, bool _modal = false, Func<string, bool> _browserOpenMethod = null, string _title = null, string _text = null, string _displayUrl = null, bool _cancelOnOutsideClick = false)
	{
		XUiC_MessageBoxWindowGroup xUiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(id);
		if (!string.IsNullOrEmpty(_url) && Utils.IsValidWebUrl(ref _url))
		{
			if (_displayUrl == null)
			{
				_displayUrl = _url;
			}
			xUiC_MessageBoxWindowGroup.urlBoxData.url = _url;
			xUiC_MessageBoxWindowGroup.urlBoxData.browserOpenMethod = _browserOpenMethod ?? ((PlatformManager.NativePlatform.Utils != null) ? new Func<string, bool>(PlatformManager.NativePlatform.Utils.OpenBrowser) : null);
			for (int i = 0; i < xUiC_MessageBoxWindowGroup.buttons.Length; i++)
			{
				xUiC_MessageBoxWindowGroup.buttons[i].Clear();
			}
			xUiC_MessageBoxWindowGroup.buttons[0].DefaultConfirm("lblContextActionOpen", xUiC_MessageBoxWindowGroup.openPage);
			xUiC_MessageBoxWindowGroup.buttons[2].DefaultCancel("xuiCancel", xUiC_MessageBoxWindowGroup.cancelOpenPage);
			if (_title == null)
			{
				_title = Localization.Get("xuiOpenUrlConfirmationTitle");
			}
			if (_text == null)
			{
				_text = Localization.Get("xuiOpenUrlConfirmationText");
			}
			_text = string.Format(_text, _displayUrl);
			bool flag = false;
			if (xUiC_MessageBoxWindowGroup.windowGroup.isShowing)
			{
				_modal = xUiC_MessageBoxWindowGroup.windowGroup.isModal;
				flag = xUiC_MessageBoxWindowGroup.openMainMenuOnClose;
			}
			xUiC_MessageBoxWindowGroup.showMessage(_title, _text, flag, _modal, _cancelOnOutsideClick ? 2 : (-1));
		}
	}

	public static void ShowNetworkError(XUi _xuiInstance, NetworkConnectionError _error, bool _confirmOnOutsideClick = false)
	{
		string arg = _error.ToStringCached();
		ShowOk(_text: _error switch
		{
			NetworkConnectionError.InvalidPassword => Localization.Get("mmLblErrorWrongPassword"), 
			NetworkConnectionError.CreateSocketOrThreadFailure => string.Format(Localization.Get("mmLblErrorSocketFailure"), SingletonMonoBehaviour<ConnectionManager>.Instance.GetRequiredPortsString()), 
			NetworkConnectionError.RestartRequired => string.Format(Localization.Get("app_restartRequired")), 
			_ => string.Format(Localization.Get("mmLblErrorUnknown"), arg), 
		}, _xuiInstance: _xuiInstance, _title: Localization.Get("mmLblErrorServerInit"), _onOk: null, _openMainMenuOnClose: true, _modal: true, _confirmOnOutsideClick: _confirmOnOutsideClick);
	}

	public static void Close(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.Close(id);
	}

	public static bool IsShowing(XUi _xuiInstance)
	{
		return _xuiInstance.playerUI.windowManager.IsWindowOpen(id);
	}
}
