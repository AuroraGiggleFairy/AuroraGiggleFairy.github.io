using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignNoiseSettings : XUiC_SignLayerSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxSeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxDetail;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSoftness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDilate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxFade;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.NoiseSignLayer currentLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMaxSoftness;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMinDilate;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMaxDilate;

	public override void Init()
	{
		base.Init();
		cbxSeed = (XUiC_ComboBoxInt)GetChildById("cbxSeed").GetChildById("value");
		cbxSeed.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Noise Seed");
		};
		cbxDetail = (XUiC_ComboBoxInt)GetChildById("cbxDetail").GetChildById("value");
		cbxDetail.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Noise Detail");
		};
		cbxSoftness = (XUiC_ComboBoxFloat)GetChildById("cbxSoftness").GetChildById("value");
		cbxSoftness.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Noise Softness");
		};
		cbxDilate = (XUiC_ComboBoxFloat)GetChildById("cbxDilate").GetChildById("value");
		cbxDilate.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Noise Dilation");
		};
		cbxFade = (XUiC_ComboBoxFloat)GetChildById("cbxFade").GetChildById("value");
		cbxFade.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Noise Fade");
		};
		defaultMaxSoftness = (float)cbxSoftness.Max;
		defaultMinDilate = (float)cbxDilate.Min;
		defaultMaxDilate = (float)cbxDilate.Max;
		SignData.NoiseSignLayer noiseSignLayer = new SignData.NoiseSignLayer();
		SetDefaultValue("cbxSeed", noiseSignLayer.seed);
		SetDefaultValue("cbxDetail", noiseSignLayer.detail);
		SetDefaultValue("cbxSoftness", noiseSignLayer.softness);
		SetDefaultValue("cbxDilate", noiseSignLayer.dilate);
		SetDefaultValue("cbxFade", noiseSignLayer.fade);
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		currentLayer = layer as SignData.NoiseSignLayer;
		if (currentLayer == null)
		{
			cbxSeed.Value = 0L;
			cbxDetail.Value = 1L;
			cbxSoftness.Value = 0.0;
			cbxDilate.Value = 0.0;
			cbxFade.Value = 0.0;
		}
		else
		{
			cbxSoftness.Max = Mathf.Max(currentLayer.softness, defaultMaxSoftness);
			cbxDilate.Min = Mathf.Min(currentLayer.dilate, defaultMinDilate);
			cbxDilate.Max = Mathf.Max(currentLayer.dilate, defaultMaxDilate);
			cbxSeed.Value = currentLayer.seed;
			cbxDetail.Value = currentLayer.detail;
			cbxSoftness.Value = currentLayer.softness;
			cbxDilate.Value = currentLayer.dilate;
			cbxFade.Value = currentLayer.fade;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentLayer != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentLayer.seed = (int)cbxSeed.Value;
			currentLayer.detail = (int)cbxDetail.Value;
			currentLayer.softness = (float)cbxSoftness.Value;
			currentLayer.dilate = (float)cbxDilate.Value;
			currentLayer.fade = (float)cbxFade.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
