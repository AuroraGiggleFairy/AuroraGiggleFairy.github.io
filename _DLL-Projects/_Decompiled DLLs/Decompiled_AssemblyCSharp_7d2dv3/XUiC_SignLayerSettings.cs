using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_SignLayerSettings : XUiController
{
	public Action<string, bool> OnPreLayerSettingsChanged;

	public Action OnLayerSettingsChanged;

	public abstract void SetLayer(SignData.SignLayer layer);

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetDefaultValue(string id, object value)
	{
		if (GetChildById(id) is XUiC_SignEditorControl xUiC_SignEditorControl)
		{
			xUiC_SignEditorControl.defaultValue = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SignLayerSettings()
	{
	}
}
