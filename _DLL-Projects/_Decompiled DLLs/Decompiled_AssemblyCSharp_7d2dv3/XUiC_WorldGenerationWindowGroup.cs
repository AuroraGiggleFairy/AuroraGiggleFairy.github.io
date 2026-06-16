using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldGenerationWindowGroup : XUiC_EditingToolsDialogBase
{
	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationWindow window;

	public override void Init()
	{
		base.Init();
		ID = windowGroup.Id;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void close()
	{
		window.StartClose();
	}
}
