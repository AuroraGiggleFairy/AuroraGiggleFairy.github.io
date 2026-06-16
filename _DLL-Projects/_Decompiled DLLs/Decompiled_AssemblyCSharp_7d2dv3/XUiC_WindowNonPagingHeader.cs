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
		XUiC_FocusedBlockHealth.SetData(xui.playerUI, null, 0f);
		xui.DragAndDropWindow.InMenu = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.DragAndDropWindow.InMenu = false;
		if (xui.CurrentSelectedEntry != null)
		{
			xui.CurrentSelectedEntry.IsSelected = false;
		}
	}
}
