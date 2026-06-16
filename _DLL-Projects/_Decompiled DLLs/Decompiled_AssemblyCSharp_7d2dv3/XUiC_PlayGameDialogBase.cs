using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_PlayGameDialogBase : XUiController
{
	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Event_BackOnPress(XUiController _sender, int _mouseButton)
	{
		close();
	}

	public override void OnOpen()
	{
		xui.GetChildByType<XUiC_PlayGameMenu>().SetWindow(windowGroup.Id);
		base.OnOpen();
		windowGroup.isEscClosable = false;
		xui.playerUI.windowManager.Close(XUiC_MainMenu.ID);
		xui.playerUI.windowManager.Open(XUiC_PlayGameMenu.ID, _bModal: false);
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.Close(XUiC_PlayGameMenu.ID);
		xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (handleDirtyUpdateDefault())
		{
			onDirtyUpdate();
		}
		if (XUiUtils.HotkeysAllowedFor(viewComponent ?? children[0].ViewComponent) && xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			close();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onDirtyUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void close()
	{
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_PlayGameDialogBase()
	{
	}
}
