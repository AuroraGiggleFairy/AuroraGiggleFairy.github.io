using GUI_2;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchInfoWindowGroup : XUiController
{
	public XUiC_TwitchEntryDescriptionWindow descriptionWindow;

	public XUiC_TwitchEntryListWindow entryListWindow;

	public override void Init()
	{
		base.Init();
		descriptionWindow = GetChildByType<XUiC_TwitchEntryDescriptionWindow>();
		entryListWindow = GetChildByType<XUiC_TwitchEntryListWindow>();
		descriptionWindow.OwnerList = entryListWindow;
	}

	public void SetEntry(XUiC_TwitchActionEntry ta)
	{
		descriptionWindow.SetTwitchAction(ta);
	}

	public void SetEntry(XUiC_TwitchVoteInfoEntry tv)
	{
		descriptionWindow.SetTwitchVote(tv);
	}

	public void SetEntry(XUiC_TwitchActionHistoryEntry th)
	{
		descriptionWindow.SetTwitchHistory(th);
	}

	public void ClearEntries()
	{
		descriptionWindow.ClearEntries();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		GetChildByType<XUiC_WindowNonPagingHeader>();
		TwitchManager.Current.HasViewedSettings = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.playerUI.windowManager.Close("twitchwindowpaging");
	}
}
