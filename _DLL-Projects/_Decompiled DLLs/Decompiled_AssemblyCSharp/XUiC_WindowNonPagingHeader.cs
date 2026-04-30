using UnityEngine.Scripting;

[Preserve]
public class XUiC_WindowNonPagingHeader : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblWindowName;

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("lblWindowName");
		if (childById != null)
		{
			lblWindowName = (XUiV_Label)childById.ViewComponent;
		}
	}

	public void SetHeader(string name)
	{
		if (lblWindowName != null)
		{
			lblWindowName.Text = name;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiC_FocusedBlockHealth.SetData(base.xui.playerUI, null, 0f);
		base.xui.dragAndDrop.InMenu = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.dragAndDrop.InMenu = false;
		if (base.xui.currentSelectedEntry != null)
		{
			base.xui.currentSelectedEntry.Selected = false;
		}
	}
}
