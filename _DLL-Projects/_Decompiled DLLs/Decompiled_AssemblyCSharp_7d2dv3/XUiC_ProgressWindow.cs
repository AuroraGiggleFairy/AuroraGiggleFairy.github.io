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

	[XuiBindComponent("lblProgress", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblProgress;

	public string ProgressText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			lblProgress.Text = value;
		}
	}

	[XuiXmlBinding("use_shadow")]
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
		ID = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (escapeDelegate != null && xui.playerUI.playerInput != null && (xui.playerUI.playerInput.Menu.WasPressed || xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed))
		{
			escapeDelegate();
		}
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)base.ViewComponent).ForceVisible(1f);
		xui.playerUI.CursorController.SetNavigationTarget(null);
		xui.playerUI.CursorController.Locked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		escapeDelegate = null;
		xui.playerUI.CursorController.Locked = false;
	}

	public static bool IsWindowOpen()
	{
		return LocalPlayerUI.primaryUI.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>()?.IsOpen ?? false;
	}

	public static void Open(LocalPlayerUI _playerUi, string _text, Action _escDelegate = null, bool _modal = true, bool _notEscClosable = true, bool _useShadow = false)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiC_ProgressWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>();
			childByType.ProgressText = _text;
			_playerUi.windowManager.Open(ID, _modal, _notEscClosable);
			childByType.escapeDelegate = _escDelegate;
			childByType.UseShadow = _useShadow;
		}
	}

	public static void Close(LocalPlayerUI _playerUi)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			_playerUi.windowManager.Close(ID);
		}
	}

	public static void SetText(LocalPlayerUI _playerUi, string _text, bool _clearEscDelegate = true)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiC_ProgressWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>();
			childByType.ProgressText = _text;
			if (_clearEscDelegate)
			{
				childByType.escapeDelegate = null;
			}
		}
	}

	public static void SetEscDelegate(LocalPlayerUI _playerUi, Action _escapeDelegate)
	{
		_playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_ProgressWindow>().escapeDelegate = _escapeDelegate;
	}
}
