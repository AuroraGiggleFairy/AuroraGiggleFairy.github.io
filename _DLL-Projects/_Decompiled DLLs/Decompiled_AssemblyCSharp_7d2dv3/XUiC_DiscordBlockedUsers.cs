using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordBlockedUsers : XUiController
{
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		if (GetChildById("btnBack") is XUiC_Button xUiC_Button)
		{
			xUiC_Button.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				xui.playerUI.windowManager.Close(windowGroup);
			};
		}
	}
}
