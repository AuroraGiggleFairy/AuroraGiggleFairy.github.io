using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignColorSettings : XUiC_SignLayerSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AdvancedColorPicker colorPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxMaskMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer currentLayer;

	public Action OnMaskModeChanged;

	public override void Init()
	{
		base.Init();
		colorPicker = GetChildByType<XUiC_AdvancedColorPicker>();
		XUiC_AdvancedColorPicker xUiC_AdvancedColorPicker = colorPicker;
		xUiC_AdvancedColorPicker.OnColorChanged = (Action<Color>)Delegate.Combine(xUiC_AdvancedColorPicker.OnColorChanged, new Action<Color>(OnColorChangedHandler));
		cbxMaskMode = GetChildById("cbxMaskMode").GetChildByType<XUiC_ComboBoxList<string>>();
		cbxMaskMode.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Mask Mode");
			OnMaskModeChanged?.Invoke();
		};
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		currentLayer = layer;
		colorPicker.SetColor(currentLayer?.renderSettings.color ?? Color.clear);
		cbxMaskMode.SelectedIndex = (int)(currentLayer?.renderSettings.mode ?? SignData.SignRenderSettings.Mode.ColorOnly);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnColorChangedHandler(Color color)
	{
		OnPreLayerSettingsChanged?.Invoke("Changed Color", arg2: false);
		currentLayer.renderSettings.color = color;
		OnLayerSettingsChanged?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
		currentLayer.renderSettings.mode = (SignData.SignRenderSettings.Mode)cbxMaskMode.SelectedIndex;
		OnLayerSettingsChanged?.Invoke();
	}
}
