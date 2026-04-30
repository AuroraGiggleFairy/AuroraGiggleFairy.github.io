using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogStatementWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite backgroundSprite;

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

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("statementLabel");
		if (childById != null)
		{
			label = (XUiV_Label)childById.ViewComponent;
		}
		childById = GetChildById("backgroundSprite");
		if (childById != null)
		{
			backgroundSprite = (XUiV_Sprite)childById.ViewComponent;
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

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "statement")
		{
			value = ((currentDialog != null && currentDialog.CurrentStatement != null) ? currentDialog.CurrentStatement.Text : "");
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
