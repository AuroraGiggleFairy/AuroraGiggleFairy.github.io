using System;
using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_KeypadWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockable LockedItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCloseCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> checkInteraction;

	public override void Init()
	{
		ID = windowGroup.Id;
		base.Init();
		txtPassword = (XUiC_TextInput)GetChildById("txtPassword");
		txtPassword.OnSubmitHandler += TxtPassword_OnSubmitHandler;
		XUiC_SimpleButton xUiC_SimpleButton = (XUiC_SimpleButton)GetChildById("btnCancel");
		xUiC_SimpleButton.OnPressed += BtnCancel_OnPressed;
		xUiC_SimpleButton.ViewComponent.NavUpTarget = txtPassword.ViewComponent;
		XUiC_SimpleButton obj = (XUiC_SimpleButton)GetChildById("btnOk");
		obj.OnPressed += BtnOk_OnPressed;
		obj.ViewComponent.NavUpTarget = txtPassword.ViewComponent;
		txtPassword.ViewComponent.NavDownTarget = xUiC_SimpleButton.ViewComponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnInputAbortedHandler(XUiController _sender)
	{
		xui.playerUI.windowManager.Close(base.WindowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
	{
		BtnOk_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		string text = txtPassword.Text;
		string hashForPassword = LockedItem.GetHashForPassword(text);
		if (LockedItem.IsOwner(PlatformManager.InternalLocalUserIdentifier) && hashForPassword != LockedItem.GetPasswordHash())
		{
			LockedItem.SetPasswordHash(hashForPassword, PlatformManager.InternalLocalUserIdentifier);
			if (text.Length == 0)
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, "passcodeRemoved");
			}
			else
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, "passcodeSet");
			}
		}
		if (LockedItem.CheckPasswordHash(hashForPassword, PlatformManager.InternalLocalUserIdentifier))
		{
			if (LockedItem.LocalPlayerIsOwner())
			{
				Manager.PlayInsidePlayerHead("Misc/password_set");
			}
			else
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, "passcodeAccepted");
				Manager.PlayInsidePlayerHead("Misc/password_pass");
			}
			xui.playerUI.windowManager.Close(base.WindowGroup);
		}
		else
		{
			Manager.PlayInsidePlayerHead("Misc/password_fail");
			GameManager.ShowTooltip(xui.playerUI.entityPlayer, "passcodeRejected");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(base.WindowGroup);
	}

	public static void Open(LocalPlayerUI _playerUi, ILockable _lockedItem, Action _onClose = null, Func<bool> _interactionCheck = null)
	{
		XUiC_KeypadWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_KeypadWindow>();
		childByType.LockedItem = _lockedItem;
		childByType.onCloseCallback = _onClose;
		childByType.checkInteraction = _interactionCheck;
		_playerUi.windowManager.Open(ID, _bModal: true);
	}

	public override void Update(float _dt)
	{
		if (checkInteraction != null && !checkInteraction())
		{
			xui.playerUI.windowManager.Close(ID);
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.playerUI.entityPlayer.PlayOneShot("open_sign");
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.entityPlayer.PlayOneShot("close_sign");
		onCloseCallback?.Invoke();
		LockedItem = null;
		onCloseCallback = null;
		checkInteraction = null;
	}
}
