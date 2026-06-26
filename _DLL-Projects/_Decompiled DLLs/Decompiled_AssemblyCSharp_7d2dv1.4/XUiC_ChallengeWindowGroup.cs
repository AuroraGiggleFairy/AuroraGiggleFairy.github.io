using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeWindowGroup : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeEntryDescriptionWindow descriptionWindow;

	public override void Init()
	{
		base.Init();
		descriptionWindow = GetChildByType<XUiC_ChallengeEntryDescriptionWindow>();
	}

	public void SetEntry(XUiC_ChallengeEntry je)
	{
		descriptionWindow.SetChallenge(je);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.playerInput.GUIActions.Inspect.WasPressed)
		{
			descriptionWindow.CompleteCurrentChallenege();
		}
		if (base.xui.playerUI.playerInput.GUIActions.HalfStack.WasPressed)
		{
			descriptionWindow.TrackCurrentChallenege();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", _bModal: false);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoTrack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoComplete", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("challenges");
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
	}
}
