using System;
using System.Collections;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MultiplayerPrivilegeNotification : XUiController
{
	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnClose", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnClose;

	[XuiBindComponent("lblResolvingPrivileges", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblResolvingPrivileges;

	[XuiBindComponent("lblInvalidPrivileges", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblInvalidPrivileges;

	[XuiBindComponent("header", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView header;

	[XuiBindComponent("content", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView content;

	[XuiBindComponent("buttons", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView buttons;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resolving;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eulaAccepted;

	[PublicizedFrom(EAccessModifier.Private)]
	public CoroutineCancellationToken cancellationToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action cancellationCleanupAction;

	public float DefaultPlatformDelay
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 0.2f;
		}
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (cancellationToken != null)
		{
			cancellationToken.Cancel();
		}
		else
		{
			closeWindow(_success: false);
		}
		btnCancel.ViewComponent.Enabled = false;
	}

	[XuiBindEvent("OnPress", "btnClose")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClose_OnPressed(XUiController _sender, int _mouseButton)
	{
		closeWindow(_success: false);
	}

	public override void OnClose()
	{
		cancellationToken?.Cancel();
		lblResolvingPrivileges.IsVisible = false;
		lblInvalidPrivileges.IsVisible = false;
		btnCancel.ViewComponent.IsVisible = false;
		btnClose.ViewComponent.IsVisible = false;
		base.OnClose();
		if (XUiC_ProgressWindow.IsWindowOpen())
		{
			closeWindow(_success: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resolvePrivilegesWithDialog(EUserPerms _permissions, Action<bool> _resolutionComplete, float _delayDisplay = -1f, bool _usingProgressWindow = false, Action _cancellationCleanupAction = null)
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
			XUiC_ProgressWindow.Open(xui.playerUI, text, [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				cancellationToken.Cancel();
			});
		}
		else
		{
			xui.playerUI.windowManager.Open(windowGroup, _bModal: false);
			setContentVisibility(_visible: false);
			btnCancel.ViewComponent.Enabled = true;
			btnCancel.ViewComponent.IsVisible = true;
			btnClose.ViewComponent.Enabled = false;
			btnClose.ViewComponent.IsVisible = false;
			lblResolvingPrivileges.IsVisible = true;
			lblInvalidPrivileges.IsVisible = false;
		}
		if (_delayDisplay < 0f)
		{
			_delayDisplay = DefaultPlatformDelay;
		}
		ThreadManager.StartCoroutine(delayPanelVisibility(_delayDisplay));
		ThreadManager.StartCoroutine(resolvePrivilegesCoroutine(_permissions, _resolutionComplete, _usingProgressWindow));
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delayPanelVisibility(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		if (resolving)
		{
			setContentVisibility(_visible: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator resolvePrivilegesCoroutine(EUserPerms _permissions, Action<bool> _resolutionComplete, bool _usingProgressWindow)
	{
		if (_permissions != 0)
		{
			yield return PermissionsManager.ResolvePermissions(_permissions, _canPrompt: true, cancellationToken);
		}
		resolving = false;
		if (cancellationToken?.IsCancelled() ?? false)
		{
			cancellationCleanupAction?.Invoke();
			closeWindow(_success: false);
			yield break;
		}
		eulaAccepted = GameManager.HasAcceptedLatestEula();
		bool flag = eulaAccepted && (PermissionsManager.GetPermissions() & _permissions) == _permissions;
		if (_usingProgressWindow)
		{
			if (flag)
			{
				closeWindow(_success: true);
			}
			else
			{
				string text = ((!eulaAccepted) ? Localization.Get("uiPermissionsEula") : (PermissionsManager.GetPermissionDenyReason(_permissions) ?? Localization.Get("lblInvalidPrivileges")));
				text = text + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
				XUiC_ProgressWindow.Open(xui.playerUI, text, [PublicizedFrom(EAccessModifier.Private)] () =>
				{
					closeWindow(_success: false);
				});
			}
		}
		else if (flag)
		{
			closeWindow(_success: true);
		}
		else
		{
			lblResolvingPrivileges.IsVisible = false;
			string text2 = (eulaAccepted ? PermissionsManager.GetPermissionDenyReason(_permissions) : Localization.Get("uiPermissionsEula"));
			if (!string.IsNullOrEmpty(text2))
			{
				btnCancel.ViewComponent.Enabled = false;
				btnCancel.ViewComponent.IsVisible = false;
				btnClose.ViewComponent.Enabled = true;
				btnClose.ViewComponent.IsVisible = true;
				lblInvalidPrivileges.Text = text2;
				lblInvalidPrivileges.IsVisible = true;
				setContentVisibility(_visible: true);
			}
			else
			{
				closeWindow(_success: false);
			}
		}
		_resolutionComplete?.Invoke(flag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeWindow(bool _success)
	{
		if (!XUiC_ProgressWindow.IsWindowOpen())
		{
			xui.playerUI.windowManager.Close(windowGroup);
			return;
		}
		XUiC_ProgressWindow.Close(xui.playerUI);
		if (!_success && !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
			if (!GameManager.HasAcceptedLatestEula())
			{
				XUiC_EulaWindow.Open(xui);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setContentVisibility(bool _visible)
	{
		header.IsVisible = _visible;
		content.IsVisible = _visible;
		buttons.IsVisible = _visible;
		if (_visible)
		{
			if (btnClose.ViewComponent.IsVisible)
			{
				xui.playerUI.CursorController.SetNavigationLockView(buttons, btnClose.ViewComponent);
				btnClose.SelectCursorElement(_withDelay: true);
			}
			else if (btnCancel.ViewComponent.IsVisible)
			{
				xui.playerUI.CursorController.SetNavigationLockView(buttons, btnCancel.ViewComponent);
				btnCancel.SelectCursorElement(_withDelay: true);
			}
		}
		else
		{
			xui.playerUI.CursorController.SetNavigationLockView(null);
		}
	}

	public static bool ResolvePrivilegesWithDialog(EUserPerms _permissions, Action<bool> _resolutionComplete, float _delayDisplay = -1f, bool _usingProgressWindow = false, Action _cancellationCleanupAction = null)
	{
		return LocalPlayerUI.primaryUI.xui.GetChildByType<XUiC_MultiplayerPrivilegeNotification>().resolvePrivilegesWithDialog(_permissions, _resolutionComplete, _delayDisplay, _usingProgressWindow, _cancellationCleanupAction);
	}

	public static void Close()
	{
		LocalPlayerUI primaryUI = LocalPlayerUI.primaryUI;
		primaryUI.windowManager.Close(primaryUI.xui.GetChildByType<XUiC_MultiplayerPrivilegeNotification>().windowGroup);
	}
}
