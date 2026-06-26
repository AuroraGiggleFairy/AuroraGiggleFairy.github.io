using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsBlockedPlayersList : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BlockedPlayersList blockedPlayersList;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		blockedPlayersList = GetChildByType<XUiC_BlockedPlayersList>();
		(GetChildById("btnBack") as XUiC_SimpleButton).OnPressed += BtnBack_OnPressed;
	}

	public override void OnClose()
	{
		base.OnClose();
		BlockedPlayerList.Instance?.MarkForWrite();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, _bModal: true);
	}
}
