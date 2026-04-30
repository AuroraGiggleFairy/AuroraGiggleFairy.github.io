using System;
using System.Collections;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MultiplayerPrivilegeNotification : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string MenuWindowID;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string InGameWindowID;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblResolvingPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblInvalidPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel header;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel content;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel buttons;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resolving;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eulaAccepted;

	[PublicizedFrom(EAccessModifier.Private)]
	public CoroutineCancellationToken cancellationToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action cancellationCleanupAction;

	public static XUiC_MultiplayerPrivilegeNotification GetWindow()
	{
		if (GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return (XUiC_MultiplayerPrivilegeNotification)((XUiWindowGroup)LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.GetWindow(InGameWindowID)).Controller;
		}
		return (XUiC_MultiplayerPrivilegeNotification)((XUiWindowGroup)LocalPlayerUI.primaryUI.windowManager.GetWindow(MenuWindowID)).Controller;
	}

	public static void Close()
	{
		string text = GetWindow()?.ID;
		if (!string.IsNullOrEmpty(text))
		{
			LocalPlayerUI.primaryUI.windowManager.Close(text);
		}
	}

	public override void Init()
	{
		base.Init();
		if (base.WindowGroup.ID.Contains("menu", StringComparison.OrdinalIgnoreCase))
		{
			MenuWindowID = base.WindowGroup.ID;
		}
		else
		{
			if (!base.WindowGroup.ID.Contains("ingame", StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception("Found Window Group for XUiC_MultiplayerPrivilegeNotification, name didn't contain \"menu\" or \"ingame\"");
			}
			InGameWindowID = base.WindowGroup.ID;
		}
		ID = base.WindowGroup.ID;
		btnCancel = (XUiC_SimpleButton)GetChildById("btnCancel");
		btnCancel.OnPressed += BtnCancel_OnPressed;
		btnClose = (XUiC_SimpleButton)GetChildById("btnClose");
		btnClose.OnPressed += BtnClose_OnPressed;
		lblResolvingPrivileges = (XUiV_Label)GetChildById("lblResolvingPrivileges").ViewComponent;
		lblInvalidPrivileges = (XUiV_Label)GetChildById("lblInvalidPrivileges").ViewComponent;
		header = (XUiV_Panel)GetChildById("header").ViewComponent;
		content = (XUiV_Panel)GetChildById("content").ViewComponent;
		buttons = (XUiV_Panel)GetChildById("buttons").ViewComponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (cancellationToken != null)
		{
			cancellationToken.Cancel();
		}
		else
		{
			CloseWindow(_success: false);
		}
		btnCancel.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClose_OnPressed(XUiController _sender, int _mouseButton)
	{
		CloseWindow(_success: false);
	}

	public override void OnClose()
	{
		if (cancellationToken != null)
		{
			cancellationToken.Cancel();
		}
		lblResolvingPrivileges.IsVisible = false;
		lblInvalidPrivileges.IsVisible = false;
		btnCancel.ViewComponent.IsVisible = false;
		btnClose.ViewComponent.IsVisible = false;
		base.OnClose();
		if (XUiC_ProgressWindow.IsWindowOpen())
		{
			CloseWindow(_success: false);
		}
	}

	public bool ResolvePrivilegesWithDialog(EUserPerms _permissions, Action<bool> _resolutionComplete, float _delayDisplay = -1f, bool _usingProgressWindow = false, Action _cancellationCleanupAction = null)
	{
		if (resolving)
		{
			return false;
		}
		if (_permissions == (EUserPerms)0)
		{
			Log.Error("No privileges specified.");
			return false;
		}
		resolving = true;
		cancellationToken = new CoroutineCancellationToken();
		cancellationCleanupAction = _cancellationCleanupAction;
		if (_usingProgressWindow)
		{
			string text = Localization.Get("lblResolvingPrivileges") + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				cancellationToken.Cancel();
			});
		}
		else
		{
			base.xui.playerUI.windowManager.Open(ID, _bModal: false);
			SetContentVisibility(_visible: false);
			btnCancel.Enabled = true;
			btnCancel.ViewComponent.IsVisible = true;
			btnClose.Enabled = false;
			btnClose.ViewComponent.IsVisible = false;
			lblResolvingPrivileges.IsVisible = true;
			lblInvalidPrivileges.IsVisible = false;
		}
		if (_delayDisplay < 0f)
		{
			_delayDisplay = GetDefaultPlatformDelay();
		}
		ThreadManager.StartCoroutine(DelayPanelVisibility(_delayDisplay));
		ThreadManager.StartCoroutine(ResolvePrivilegesCoroutine(_permissions, _resolutionComplete, _usingProgressWindow));
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ResolvePrivilegesCoroutine(EUserPerms _permissions, Action<bool> _resolutionComplete, bool _usingProgressWindow)
	{
		if (_permissions != 0)
		{
			yield return PermissionsManager.ResolvePermissions(_permissions, _canPrompt: true, cancellationToken);
		}
		resolving = false;
		if (cancellationToken?.IsCancelled() ?? false)
		{
			cancellationCleanupAction?.Invoke();
			CloseWindow(_success: false);
			yield break;
		}
		eulaAccepted = GameManager.HasAcceptedLatestEula();
		bool flag = eulaAccepted && (PermissionsManager.GetPermissions() & _permissions) == _permissions;
		if (_usingProgressWindow)
		{
			if (flag)
			{
				CloseWindow(_success: true);
			}
			else
			{
				string text = ((!eulaAccepted) ? Localization.Get("uiPermissionsEula") : (PermissionsManager.GetPermissionDenyReason(_permissions) ?? Localization.Get("lblInvalidPrivileges")));
				text = text + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
				XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					CloseWindow(_success: false);
				});
			}
		}
		else if (flag)
		{
			CloseWindow(_success: true);
		}
		else
		{
			lblResolvingPrivileges.IsVisible = false;
			string text2 = (eulaAccepted ? PermissionsManager.GetPermissionDenyReason(_permissions) : Localization.Get("uiPermissionsEula"));
			if (!string.IsNullOrEmpty(text2))
			{
				btnCancel.Enabled = false;
				btnCancel.ViewComponent.IsVisible = false;
				btnClose.Enabled = true;
				btnClose.ViewComponent.IsVisible = true;
				lblInvalidPrivileges.Text = text2;
				lblInvalidPrivileges.IsVisible = true;
				SetContentVisibility(_visible: true);
			}
			else
			{
				CloseWindow(_success: false);
			}
		}
		_resolutionComplete?.Invoke(flag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseWindow(bool _success)
	{
		if (!XUiC_ProgressWindow.IsWindowOpen())
		{
			base.xui.playerUI.windowManager.Close(ID);
			return;
		}
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		if (!_success && !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			LocalPlayerUI.primaryUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
			if (!GameManager.HasAcceptedLatestEula())
			{
				XUiC_EulaWindow.Open(base.xui);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDefaultPlatformDelay()
	{
		return 0.2f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetContentVisibility(bool _visible)
	{
		header.IsVisible = _visible;
		content.IsVisible = _visible;
		buttons.IsVisible = _visible;
		if (_visible)
		{
			if (btnClose.ViewComponent.IsVisible)
			{
				base.xui.playerUI.CursorController.SetNavigationLockView(buttons, btnClose.ViewComponent);
				btnClose.SelectCursorElement(_withDelay: true);
			}
			else if (btnCancel.ViewComponent.IsVisible)
			{
				base.xui.playerUI.CursorController.SetNavigationLockView(buttons, btnCancel.ViewComponent);
				btnCancel.SelectCursorElement(_withDelay: true);
			}
		}
		else
		{
			base.xui.playerUI.CursorController.SetNavigationLockView(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DelayPanelVisibility(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		if (resolving)
		{
			SetContentVisibility(_visible: true);
		}
	}
}
