using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_OptionEntryComboAbs : XUiC_OptionEntryAbs
{
	[XuiBindEvent("OnValueChangedGeneric", "comboGeneric")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnComboValueChanged(XUiController _sender)
	{
		if (base.ApplyImmediately)
		{
			immediatelyApplyCurrentSelection();
		}
		invokeValueChanged();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void immediatelyApplyCurrentSelection();

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionEntryComboAbs()
	{
	}
}
