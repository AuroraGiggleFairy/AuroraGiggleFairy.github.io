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
		if (xui.playerUI.playerInput.GUIActions.Inspect.WasPressed)
		{
			descriptionWindow.CompleteCurrentChallenege();
		}
		if (xui.playerUI.playerInput.GUIActions.HalfStack.WasPressed)
		{
			descriptionWindow.TrackCurrentChallenege();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		xui.playerUI.windowManager.Open("windowpaging", _bModal: false);
		xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoTrack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoComplete", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>()?.SetSelected("challenges");
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		xui.playerUI.windowManager.Close("windowpaging");
	}
}
