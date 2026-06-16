using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_StretchWarpSettings : XUiC_SignWarpSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.StretchWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxExponent;

	public override SignData.SignWarp CurrentWarp
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return currentWarp;
		}
	}

	public override void Init()
	{
		base.Init();
		cbxOffsetX = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetX").GetChildById("value");
		cbxOffsetX.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Stretch Offset X");
		};
		cbxOffsetY = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetY").GetChildById("value");
		cbxOffsetY.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Stretch Offset Y");
		};
		cbxRotation = (XUiC_ComboBoxFloat)GetChildById("cbxRotation").GetChildById("value");
		cbxRotation.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Stretch Rotation");
		};
		cbxDistance = (XUiC_ComboBoxFloat)GetChildById("cbxDistance").GetChildById("value");
		cbxDistance.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Stretch Distance");
		};
		cbxWidth = (XUiC_ComboBoxFloat)GetChildById("cbxWidth").GetChildById("value");
		cbxWidth.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Stretch Width");
		};
		cbxExponent = (XUiC_ComboBoxFloat)GetChildById("cbxExponent").GetChildById("value");
		cbxExponent.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Stretch Exponent");
		};
		SignData.StretchWarp stretchWarp = new SignData.StretchWarp();
		SetDefaultValue("cbxOffsetX", stretchWarp.offset.x);
		SetDefaultValue("cbxOffsetY", stretchWarp.offset.y);
		SetDefaultValue("cbxRotation", stretchWarp.rotation);
		SetDefaultValue("cbxDistance", stretchWarp.distance);
		SetDefaultValue("cbxWidth", stretchWarp.width);
		SetDefaultValue("cbxExponent", stretchWarp.exponent);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		currentWarp = warp as SignData.StretchWarp;
		if (currentWarp == null)
		{
			cbxOffsetX.Value = 0.0;
			cbxOffsetY.Value = 0.0;
			cbxRotation.Value = 0.0;
			cbxDistance.Value = 0.0;
			cbxWidth.Value = 0.0;
			cbxExponent.Value = 0.0;
		}
		else
		{
			cbxOffsetX.Value = currentWarp.offset.x;
			cbxOffsetY.Value = currentWarp.offset.y;
			cbxRotation.Value = currentWarp.rotation;
			cbxDistance.Value = currentWarp.distance;
			cbxWidth.Value = currentWarp.width;
			cbxExponent.Value = currentWarp.exponent;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentWarp != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentWarp.offset = new Vector2((float)cbxOffsetX.Value, (float)cbxOffsetY.Value);
			currentWarp.rotation = (float)cbxRotation.Value;
			currentWarp.distance = (float)cbxDistance.Value;
			currentWarp.width = (float)cbxWidth.Value;
			currentWarp.exponent = (float)cbxExponent.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
