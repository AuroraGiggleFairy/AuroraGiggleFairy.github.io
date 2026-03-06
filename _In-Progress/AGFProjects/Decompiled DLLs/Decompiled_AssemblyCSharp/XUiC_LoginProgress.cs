using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoginProgress : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator textAnimator;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		XUiV_Label label = GetChildById("lblText").ViewComponent as XUiV_Label;
		textAnimator = new TextEllipsisAnimator(string.Empty, label);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.ViewComponent.IsVisible)
		{
			textAnimator.GetNextAnimatedString(_dt);
		}
	}

	public static void Open(XUi xui, string message)
	{
		XUiC_LoginProgress childByType = xui.FindWindowGroupByName(ID).GetChildByType<XUiC_LoginProgress>();
		xui.playerUI.windowManager.Open(ID, _bModal: true, _bIsNotEscClosable: true);
		childByType.textAnimator.SetBaseString(message + "...");
	}
}
