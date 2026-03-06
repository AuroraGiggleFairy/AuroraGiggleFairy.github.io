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
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", _bModal: false);
		base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("character");
		if (base.xui.PlayerEquipment != null)
		{
			base.xui.PlayerEquipment.IsOpen = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		if (base.xui.PlayerEquipment != null)
		{
			base.xui.PlayerEquipment.IsOpen = false;
		}
	}
}
