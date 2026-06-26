using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabEditorHelp : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiV_Label> labels = new List<XUiV_Label>();

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		GetChildById("outclick").OnPress += Close_OnPress;
		findLabels(this);
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(ID);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		foreach (XUiV_Label label in labels)
		{
			label.ForceTextUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void findLabels(XUiController _controller)
	{
		foreach (XUiController child in _controller.Children)
		{
			if (child.ViewComponent is XUiV_Label item)
			{
				labels.Add(item);
			}
			findLabels(child);
		}
	}
}
