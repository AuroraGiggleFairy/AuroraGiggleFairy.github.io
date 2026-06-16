using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignTextSettings : XUiC_SignLayerSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string UnknownFontString = Localization.Get("lblSignFontUnknown");

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxFont;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDirection;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSpacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSoftness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDilate;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.TextSignLayer currentLayer;

	public override void Init()
	{
		base.Init();
		txtText = (XUiC_TextInput)GetChildById("txtText").GetChildById("value");
		txtText.OnChangeHandler += TxtText_OnChangeHandler;
		cbxFont = (XUiC_ComboBoxList<string>)GetChildById("cbxFont").GetChildById("value");
		cbxFont.OnValueChanged += CbxFont_OnValueChanged;
		cbxDirection = (XUiC_ComboBoxFloat)GetChildById("cbxDirection").GetChildById("value");
		cbxDirection.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Text Direction");
		};
		cbxSpacing = (XUiC_ComboBoxFloat)GetChildById("cbxSpacing").GetChildById("value");
		cbxSpacing.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Text Spacing");
		};
		cbxSoftness = (XUiC_ComboBoxFloat)GetChildById("cbxSoftness").GetChildById("value");
		cbxSoftness.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Text Softness");
		};
		cbxDilate = (XUiC_ComboBoxFloat)GetChildById("cbxDilate").GetChildById("value");
		cbxDilate.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Text Dilation");
		};
		SignData.TextSignLayer textSignLayer = new SignData.TextSignLayer();
		SetDefaultValue("cbxDirection", textSignLayer.direction);
		SetDefaultValue("cbxSpacing", textSignLayer.spacing);
		SetDefaultValue("cbxSoftness", textSignLayer.softness);
		SetDefaultValue("cbxDilate", textSignLayer.dilate);
		PopulateFontList();
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		currentLayer = layer as SignData.TextSignLayer;
		if (currentLayer == null)
		{
			txtText.Text = string.Empty;
			cbxDirection.Value = 0.0;
			cbxSpacing.Value = 0.0;
			cbxSoftness.Value = 0.0;
			cbxDilate.Value = 0.0;
			return;
		}
		txtText.Text = currentLayer.text;
		int num = cbxFont.Elements.IndexOf(currentLayer.font);
		if (num < 0 || num > cbxFont.Elements.Count - 1)
		{
			Log.Error("Error reading TextSignLayer data: could not find matching font with name \"" + currentLayer.font + "\".");
			cbxFont.MaxIndex = cbxFont.Elements.Count - 1;
			cbxFont.SelectedIndex = cbxFont.MaxIndex;
		}
		else
		{
			cbxFont.SelectedIndex = num;
			cbxFont.MaxIndex = cbxFont.Elements.Count - 2;
		}
		cbxDirection.Value = currentLayer.direction;
		cbxSpacing.Value = currentLayer.spacing;
		cbxSoftness.Value = currentLayer.softness;
		cbxDilate.Value = currentLayer.dilate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentLayer != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentLayer.direction = (float)cbxDirection.Value;
			currentLayer.spacing = (float)cbxSpacing.Value;
			currentLayer.softness = (float)cbxSoftness.Value;
			currentLayer.dilate = (float)cbxDilate.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (currentLayer != null && !_changeFromCode)
		{
			OnPreLayerSettingsChanged?.Invoke("Changed Text Content", arg2: false);
			currentLayer.text = _text;
			OnLayerSettingsChanged?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxFont_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		if (currentLayer != null)
		{
			OnPreLayerSettingsChanged?.Invoke("Changed Text Font", arg2: false);
			cbxFont.MaxIndex = cbxFont.Elements.Count - ((_newValue == UnknownFontString) ? 1 : 2);
			currentLayer.font = _newValue;
			OnLayerSettingsChanged?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PopulateFontList()
	{
		cbxFont.Elements.Clear();
		foreach (string fontName in SignDataManager.Instance.FontNames)
		{
			cbxFont.Elements.Add(fontName);
		}
		cbxFont.Elements.Add(UnknownFontString);
		cbxFont.MaxIndex = cbxFont.Elements.Count - 2;
	}
}
