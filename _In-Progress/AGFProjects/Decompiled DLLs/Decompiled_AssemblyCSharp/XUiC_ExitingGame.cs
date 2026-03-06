using UnityEngine.Scripting;

[Preserve]
public class XUiC_ExitingGame : XUiController
{
	public static string ID = "";

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.ViewComponent.IsVisible = true;
		((XUiV_Window)viewComponent).ForceVisible(1f);
	}

	public override void Cleanup()
	{
		base.Cleanup();
	}
}
