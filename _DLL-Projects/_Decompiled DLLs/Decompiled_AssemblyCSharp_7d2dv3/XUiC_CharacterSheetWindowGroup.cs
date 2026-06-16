using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterSheetWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ActiveBuffList buffList;

	public override void Init()
	{
		base.Init();
		buffList = GetChildByType<XUiC_ActiveBuffList>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (buffList != null)
		{
			buffList.setFirstEntry = true;
		}
		xui.playerUI.windowManager.Open("windowpaging", _bModal: false);
		xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("character");
		if (xui.PlayerEquipment != null)
		{
			xui.PlayerEquipment.IsOpen = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.Close("windowpaging");
		if (xui.PlayerEquipment != null)
		{
			xui.PlayerEquipment.IsOpen = false;
		}
	}
}
