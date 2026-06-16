using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsBlockedPlayersList : XUiC_OptionsDialogBase
{
	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BlockedPlayersList blockedPlayersList;

	public override bool SupportsDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	public override void OnClose()
	{
		base.OnClose();
		BlockedPlayerList.Instance?.MarkForWrite();
	}
}
