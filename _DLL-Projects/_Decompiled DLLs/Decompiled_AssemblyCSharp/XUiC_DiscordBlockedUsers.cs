using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordBlockedUsers : XUiController
{
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		if (GetChildById("btnBack") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				base.xui.playerUI.windowManager.Close(windowGroup);
				base.xui.playerUI.windowManager.Open(windowGroup.openWindowOnEsc, _bModal: true);
			};
		}
	}
}
