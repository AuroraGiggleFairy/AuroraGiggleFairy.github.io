using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignPolygonSettings : XUiC_SignLayerSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxSides;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSmoothness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxStarify;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSoftness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDilate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController cbxFrequencyRow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxFrequency;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.PolygonSignLayer currentLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMaxSoftness;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMinDilate;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMaxDilate;

	public override void Init()
	{
		base.Init();
		cbxMode = GetChildById("cbxMode").GetChildByType<XUiC_ComboBoxList<string>>();
		cbxMode.OnValueChangedGeneric += OnModeChanged;
		cbxSides = (XUiC_ComboBoxInt)GetChildById("cbxSides").GetChildById("value");
		cbxSides.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Polygon Sides");
		};
		cbxSmoothness = (XUiC_ComboBoxFloat)GetChildById("cbxSmoothness").GetChildById("value");
		cbxSmoothness.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Polygon Smoothness");
		};
		cbxStarify = (XUiC_ComboBoxFloat)GetChildById("cbxStarify").GetChildById("value");
		cbxStarify.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Polygon Starify");
		};
		cbxSoftness = (XUiC_ComboBoxFloat)GetChildById("cbxSoftness").GetChildById("value");
		cbxSoftness.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Polygon Softness");
		};
		cbxDilate = (XUiC_ComboBoxFloat)GetChildById("cbxDilate").GetChildById("value");
		cbxDilate.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Polygon Dilation");
		};
		cbxFrequencyRow = GetChildById("cbxFrequency");
		cbxFrequency = (XUiC_ComboBoxFloat)cbxFrequencyRow.GetChildById("value");
		cbxFrequency.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Polygon Frequency");
		};
		defaultMaxSoftness = (float)cbxSoftness.Max;
		defaultMinDilate = (float)cbxDilate.Min;
		defaultMaxDilate = (float)cbxDilate.Max;
		SignData.PolygonSignLayer polygonSignLayer = new SignData.PolygonSignLayer();
		SetDefaultValue("cbxSides", polygonSignLayer.sides);
		SetDefaultValue("cbxSmoothness", polygonSignLayer.smoothness);
		SetDefaultValue("cbxStarify", polygonSignLayer.starify);
		SetDefaultValue("cbxSoftness", polygonSignLayer.softness);
		SetDefaultValue("cbxDilate", polygonSignLayer.dilate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnModeChanged(XUiController _sender)
	{
		UpdateDefaultDilate();
		OnValueChangedGeneric("Changed Shape Mode");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDefaultDilate()
	{
		if (currentLayer.softness == 0f && currentLayer.shapeMode switch
		{
			SignData.PolygonSignLayer.ShapeMode.Line => currentLayer.dilate == 0.02f, 
			SignData.PolygonSignLayer.ShapeMode.Ripple => currentLayer.dilate == 0.1f, 
			_ => currentLayer.dilate == 0f, 
		})
		{
			switch ((SignData.PolygonSignLayer.ShapeMode)(byte)cbxMode.SelectedIndex)
			{
			case SignData.PolygonSignLayer.ShapeMode.Line:
				cbxDilate.Value = 0.019999999552965164;
				break;
			case SignData.PolygonSignLayer.ShapeMode.Ripple:
				cbxDilate.Value = 0.10000000149011612;
				break;
			default:
				cbxDilate.Value = 0.0;
				break;
			}
		}
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		currentLayer = layer as SignData.PolygonSignLayer;
		if (currentLayer == null)
		{
			cbxMode.SelectedIndex = 0;
			cbxSides.Value = 3L;
			cbxSmoothness.Value = 0.0;
			cbxStarify.Value = 0.0;
			cbxSoftness.Value = 0.0;
			cbxDilate.Value = 0.0;
			cbxFrequency.Value = 5.0;
			cbxFrequencyRow.ViewComponent.IsVisible = false;
		}
		else
		{
			cbxSoftness.Max = Mathf.Max(currentLayer.softness, defaultMaxSoftness);
			cbxDilate.Min = Mathf.Min(currentLayer.dilate, defaultMinDilate);
			cbxDilate.Max = Mathf.Max(currentLayer.dilate, defaultMaxDilate);
			cbxMode.SelectedIndex = (int)currentLayer.shapeMode;
			cbxSides.Value = currentLayer.sides;
			cbxSmoothness.Value = currentLayer.smoothness;
			cbxStarify.Value = currentLayer.starify;
			cbxSoftness.Value = currentLayer.softness;
			cbxDilate.Value = currentLayer.dilate;
			cbxFrequency.Value = currentLayer.frequency;
			cbxFrequencyRow.ViewComponent.IsVisible = currentLayer.shapeMode == SignData.PolygonSignLayer.ShapeMode.Ripple;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentLayer != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentLayer.shapeMode = (SignData.PolygonSignLayer.ShapeMode)cbxMode.SelectedIndex;
			currentLayer.sides = (int)cbxSides.Value;
			currentLayer.smoothness = (float)cbxSmoothness.Value;
			currentLayer.starify = (float)cbxStarify.Value;
			currentLayer.softness = (float)cbxSoftness.Value;
			currentLayer.dilate = (float)cbxDilate.Value;
			currentLayer.frequency = (float)cbxFrequency.Value;
			cbxFrequencyRow.ViewComponent.IsVisible = currentLayer.shapeMode == SignData.PolygonSignLayer.ShapeMode.Ripple;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
