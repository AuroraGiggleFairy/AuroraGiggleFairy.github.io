using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2WindowGroup : XUiController
{
	public override void OnOpen()
	{
		base.OnOpen();
		xui.playerUI.windowManager.Open("windowpaging", _bModal: false);
		xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("creative2");
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.Close("windowpaging");
	}
}
