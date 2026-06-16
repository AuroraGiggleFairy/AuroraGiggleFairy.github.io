using UnityEngine.Scripting;

[Preserve]
public class XUiC_ExitingGame : XUiController
{
	public static string ID;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)viewComponent).ForceVisible(1f);
	}
}
