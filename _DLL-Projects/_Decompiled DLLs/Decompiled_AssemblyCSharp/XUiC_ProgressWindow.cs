using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ProgressWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useShadow;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action escapeDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	public string baseText;

	public string ProgressText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			lblProgress.Text = value;
		}
	}

	public bool UseShadow
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return useShadow;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (useShadow != value)
			{
				IsDirty = true;
				useShadow = value;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		lblProgress = (XUiV_Label)GetChildById("lblProgress").ViewComponent;
		ellipsisAnimator = new TextEllipsisAnimator(null, lblProgress);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		ellipsisAnimator.GetNextAnimatedString(_dt);
		if (escapeDelegate != null && base.xui.playerUI.playerInput != null && (base.xui.playerUI.playerInput.Menu.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed))
		{
			escapeDelegate();
		}
		if (IsDirty)
		{
			RefreshBindings(_forceAll: true);
			IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)base.ViewComponent).Panel.alpha = 1f;
		base.xui.playerUI.CursorController.SetNavigationTarget(null);
		base.xui.playerUI.CursorController.Locked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		escapeDelegate = null;
		base.xui.playerUI.CursorController.Locked = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "use_shadow")
		{
			_value = useShadow.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static bool IsWindowOpen()
	{
		return LocalPlayerUI.primaryUI.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>()?.IsOpen ?? false;
	}

	public static void Open(LocalPlayerUI _playerUi, string _text, Action _escDelegate = null, bool _modal = true, bool _escClosable = true, bool _closeOpenWindows = true, bool _useShadow = false)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiC_ProgressWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>();
			childByType.baseText = _text;
			childByType.ellipsisAnimator.SetBaseString(childByType.baseText);
			_playerUi.windowManager.Open(ID, _modal, _escClosable, _closeOpenWindows);
			childByType.escapeDelegate = _escDelegate;
			childByType.UseShadow = _useShadow;
		}
	}

	public static void Close(LocalPlayerUI _playerUi)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			_playerUi.windowManager.CloseIfOpen(ID);
		}
	}

	public static void SetText(LocalPlayerUI _playerUi, string _text, bool _clearEscDelegate = true)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiC_ProgressWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>();
			childByType.baseText = _text;
			childByType.ellipsisAnimator.SetBaseString(childByType.baseText);
			if (_clearEscDelegate)
			{
				childByType.escapeDelegate = null;
			}
		}
	}

	public static string GetText(LocalPlayerUI _playerUi)
	{
		return _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>().baseText;
	}

	public static void SetEscDelegate(LocalPlayerUI _playerUi, Action _escapeDelegate)
	{
		_playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>().escapeDelegate = _escapeDelegate;
	}
}
