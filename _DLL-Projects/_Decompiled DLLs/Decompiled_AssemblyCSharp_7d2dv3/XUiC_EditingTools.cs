using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditingTools : XUiController
{
	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WindowSelector parentSelector;

	[XuiBindEvent("WindowSelected", "parentSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onWindowSelected(XUiC_WindowSelector _sender, string _windowId)
	{
		GUIWindowManager windowManager = xui.playerUI.windowManager;
		switch (_windowId)
		{
		case "rwgPreviewer":
			if (!XUiC_WorldGenerationWindow.IsWindowOpen(xui))
			{
				XUiC_WorldGenerationWindow.Open(xui, XUiC_MainMenu.ID);
			}
			break;
		case "poiEditor":
			windowManager.Open(XUiC_EditingToolsPoiEditor.ID, _bModal: true);
			break;
		case "worldEditor":
			windowManager.Open(XUiC_WorldEditor.ID, _bModal: true);
			break;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		RefreshBindings();
	}
}
