using InControl;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_KeyboardBindingEntry : XUiController
{
	public PlayerAction action;

	public XUiV_Label label;

	public XUiV_Label value;

	public XUiV_Button unbind;

	public XUiV_Button button;

	public override void Init()
	{
		base.Init();
		label = GetChildById("label").ViewComponent as XUiV_Label;
		value = GetChildById("value").ViewComponent as XUiV_Label;
		unbind = GetChildById("unbind").ViewComponent as XUiV_Button;
		button = GetChildById("background").ViewComponent as XUiV_Button;
	}

	public void SetAction(PlayerAction _action)
	{
		action = _action;
		PlayerActionData.ActionUserData actionUserData = (PlayerActionData.ActionUserData)_action.UserData;
		base.ViewComponent.UiTransform.gameObject.name = "Entry_" + actionUserData.LocalizedName;
		label.Text = actionUserData.LocalizedName;
		button.ToolTip = actionUserData.LocalizedDescription;
		if (actionUserData.allowRebind)
		{
			unbind.ToolTip = Localization.Get("xuiRemoveBinding");
			return;
		}
		unbind.ForceHide = true;
		XUiV_Button xUiV_Button = unbind;
		XUiV_Button xUiV_Button2 = unbind;
		bool isSnappable = (unbind.IsVisible = false);
		xUiV_Button.IsNavigatable = (xUiV_Button2.IsSnappable = isSnappable);
		unbind.UiTransform.gameObject.SetActive(value: false);
		button.ForceHide = true;
		XUiV_Button xUiV_Button3 = button;
		XUiV_Button xUiV_Button4 = button;
		isSnappable = (button.IsVisible = false);
		xUiV_Button3.IsNavigatable = (xUiV_Button4.IsSnappable = isSnappable);
		button.UiTransform.gameObject.SetActive(value: false);
	}

	public void Hide()
	{
		base.ViewComponent.UiTransform.gameObject.name = "Hidden Entry";
		unbind.ForceHide = true;
		XUiV_Button xUiV_Button = unbind;
		XUiV_Button xUiV_Button2 = unbind;
		bool isSnappable = (unbind.IsVisible = false);
		xUiV_Button.IsNavigatable = (xUiV_Button2.IsSnappable = isSnappable);
		button.ForceHide = true;
		XUiV_Button xUiV_Button3 = button;
		XUiV_Button xUiV_Button4 = button;
		isSnappable = (button.IsVisible = false);
		xUiV_Button3.IsNavigatable = (xUiV_Button4.IsSnappable = isSnappable);
		button.UiTransform.gameObject.SetActive(value: false);
		unbind.UiTransform.gameObject.SetActive(value: false);
	}
}
