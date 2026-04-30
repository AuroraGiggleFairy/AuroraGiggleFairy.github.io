using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapEnterWaypoint : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	public override void Init()
	{
		base.Init();
		txtInput = (XUiC_TextInput)windowGroup.Controller.GetChildById("waypointInput");
		if (txtInput != null)
		{
			txtInput.Text = string.Empty;
		}
		if (txtInput != null)
		{
			txtInput.OnSubmitHandler += waypointOnSubmitHandler;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waypointOnInputAbortedHandler(XUiController _sender)
	{
		((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).closeAllPopups();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waypointOnSubmitHandler(XUiController _sender, string _text)
	{
		((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).OnWaypointCreated(_text);
	}

	public void Show(Vector2i _position)
	{
		XUiV_Window window = base.xui.GetWindow("mapAreaEnterWaypointName");
		window.Position = _position;
		window.IsVisible = true;
		txtInput.Text = string.Empty;
		txtInput.SelectOrVirtualKeyboard(_delayed: true);
	}
}
