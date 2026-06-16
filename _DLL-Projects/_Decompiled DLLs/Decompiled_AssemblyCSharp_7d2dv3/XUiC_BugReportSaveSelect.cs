using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportSaveSelect : XUiController
{
	public XUiC_BugReportSavesList List;

	public override void Init()
	{
		base.Init();
		List = GetChildByType<XUiC_BugReportSavesList>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		rebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rebuildList()
	{
		List.RebuildList(SaveInfoProvider.Instance.SaveEntryInfos);
	}
}
