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

	public override void Init()
	{
		ID = windowGroup.ID;
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
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
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
		if (LockedItem.CheckPassword(text, PlatformManager.InternalLocalUserIdentifier, out var changed))
		{
			if (LockedItem.LocalPlayerIsOwner())
			{
				if (changed)
				{
					if (text.Length == 0)
					{
						GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "passcodeRemoved");
					}
					else
					{
						GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "passcodeSet");
					}
				}
				Manager.PlayInsidePlayerHead("Misc/password_set");
			}
			else
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "passcodeAccepted");
				Manager.PlayInsidePlayerHead("Misc/password_pass");
			}
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		}
		else
		{
			Manager.PlayInsidePlayerHead("Misc/password_fail");
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "passcodeRejected");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.entityPlayer.PlayOneShot("open_sign");
		base.xui.playerUI.CursorController.SetNavigationLockView(viewComponent, txtPassword.ViewComponent);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.entityPlayer.PlayOneShot("close_sign");
		LockedItem = null;
		base.xui.playerUI.CursorController.SetNavigationLockView(null);
	}

	public static void Open(LocalPlayerUI _playerUi, ILockable _lockedItem)
	{
		_playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_KeypadWindow>().LockedItem = _lockedItem;
		_playerUi.windowManager.Open(ID, _bModal: true);
	}
}
