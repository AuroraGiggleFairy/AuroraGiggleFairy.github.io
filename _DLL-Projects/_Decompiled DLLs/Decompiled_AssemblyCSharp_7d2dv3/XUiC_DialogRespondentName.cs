using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogRespondentName : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog currentDialog;

	public Dialog CurrentDialog
	{
		get
		{
			return currentDialog;
		}
		set
		{
			currentDialog = value;
			RefreshBindings();
			IsDirty = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		currentDialog = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "respondentname")
		{
			value = ((xui.Dialog.Respondent != null) ? Localization.Get(xui.Dialog.Respondent.EntityName) : "");
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public void Refresh()
	{
		RefreshBindings();
		IsDirty = true;
	}
}
