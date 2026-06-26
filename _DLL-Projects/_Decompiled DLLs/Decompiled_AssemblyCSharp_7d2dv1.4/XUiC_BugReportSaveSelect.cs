using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportSaveSelect : XUiController
{
	public XUiC_BugReportSavesList list;

	public override void Init()
	{
		base.Init();
		list = GetChildByType<XUiC_BugReportSavesList>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RebuildList()
	{
		list.RebuildList(SaveInfoProvider.Instance.SaveEntryInfos);
	}
}
