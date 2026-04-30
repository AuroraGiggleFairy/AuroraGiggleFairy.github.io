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
			RefreshBindings(_forceAll: true);
			IsDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings();
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
			value = ((base.xui.Dialog.Respondent != null) ? Localization.Get(base.xui.Dialog.Respondent.EntityName) : "");
			return true;
		}
		return false;
	}

	public void Refresh()
	{
		RefreshBindings(_forceAll: true);
		IsDirty = true;
	}
}
