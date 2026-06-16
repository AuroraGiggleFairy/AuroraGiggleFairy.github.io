using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditingToolsPoiEditor : XUiC_EditingToolsDialogBase
{
	public static string ID = "";

	[XuiBindComponent("btnStart", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnStart;

	[XuiBindEvent("OnPress", "btnStart")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		GameUtils.StartSinglePrefabEditingWithMessage();
	}

	public override void Init()
	{
		base.Init();
		ID = windowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(viewComponent ?? children[0].ViewComponent) && xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			BtnStart_OnPressed(null, 0);
		}
	}
}
