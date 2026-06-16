using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignTransformSettings : XUiC_SignLayerSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPositionA;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPositionB;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtScaleA;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtScaleB;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer currentLayer;

	public override void Init()
	{
		base.Init();
		txtPositionA = (XUiC_TextInput)GetChildById("txtPosition").GetChildById("valueA");
		txtPositionA.OnChangeHandler += OnChangeHandler;
		txtPositionB = (XUiC_TextInput)GetChildById("txtPosition").GetChildById("valueB");
		txtPositionB.OnChangeHandler += OnChangeHandler;
		txtRotation = (XUiC_TextInput)GetChildById("txtRotation").GetChildById("value");
		txtRotation.OnChangeHandler += OnChangeHandler;
		txtScaleA = (XUiC_TextInput)GetChildById("txtScale").GetChildById("valueA");
		txtScaleA.OnChangeHandler += OnChangeHandler;
		txtScaleB = (XUiC_TextInput)GetChildById("txtScale").GetChildById("valueB");
		txtScaleB.OnChangeHandler += OnChangeHandler;
		SetDefaultValue("txtPosition", (SignData.SignTransform.Defaults.position.x, SignData.SignTransform.Defaults.position.y));
		SetDefaultValue("txtRotation", SignData.SignTransform.Defaults.rotation);
		SetDefaultValue("txtScale", (SignData.SignTransform.Defaults.scale.x, SignData.SignTransform.Defaults.scale.y));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (currentLayer == null || _changeFromCode)
		{
			return;
		}
		if (!float.TryParse(_text, out var result))
		{
			Log.Error("Failed to parse value \"" + _text + "\" for transform adjustment.");
			return;
		}
		if (_sender == txtPositionA)
		{
			OnPreLayerSettingsChanged?.Invoke("Changed Transform Position", arg2: false);
			currentLayer.transform.position.x = result;
		}
		else if (_sender == txtPositionB)
		{
			OnPreLayerSettingsChanged?.Invoke("Changed Transform Position", arg2: false);
			currentLayer.transform.position.y = result;
		}
		else if (_sender == txtRotation)
		{
			OnPreLayerSettingsChanged?.Invoke("Changed Transform Rotation", arg2: false);
			currentLayer.transform.rotation = result;
		}
		else if (_sender == txtScaleA)
		{
			OnPreLayerSettingsChanged?.Invoke("Changed Transform Scale", arg2: false);
			currentLayer.transform.scale.x = result;
		}
		else if (_sender == txtScaleB)
		{
			OnPreLayerSettingsChanged?.Invoke("Changed Transform Scale", arg2: false);
			currentLayer.transform.scale.y = result;
		}
		OnLayerSettingsChanged?.Invoke();
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		currentLayer = layer;
		if (currentLayer == null)
		{
			txtPositionA.Text = string.Empty;
			txtPositionB.Text = string.Empty;
			txtRotation.Text = string.Empty;
			txtScaleA.Text = string.Empty;
			txtScaleB.Text = string.Empty;
		}
		else
		{
			txtPositionA.Text = layer.transform.position.x.ToString("F4");
			txtPositionB.Text = layer.transform.position.y.ToString("F4");
			txtRotation.Text = layer.transform.rotation.ToString("F4");
			txtScaleA.Text = layer.transform.scale.x.ToString("F4");
			txtScaleB.Text = layer.transform.scale.y.ToString("F4");
		}
	}
}
