using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GamepadCallout : XUiController
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icon
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label action
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		icon = (XUiV_Sprite)GetChildById("icon").ViewComponent;
		action = (XUiV_Label)GetChildById("action").ViewComponent;
		icon.UIAtlas = UIUtils.IconAtlas.name;
	}

	public void SetupCallout(UIUtils.ButtonIcon _icon, string _action)
	{
		icon.SpriteName = UIUtils.GetSpriteName(_icon);
		action.Text = Localization.Get(_action);
	}
}
