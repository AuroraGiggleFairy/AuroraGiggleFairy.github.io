using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabEditorHelp : XUiController
{
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		GetChildById("outclick").OnPress += Close_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close_OnPress(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
	}
}
