using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkewWarpSettings : XUiC_SignWarpSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SkewWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSkewX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSkewY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotation;

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
		cbxSkewX = (XUiC_ComboBoxFloat)GetChildById("cbxSkewX").GetChildById("value");
		cbxSkewX.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Skew X");
		};
		cbxSkewY = (XUiC_ComboBoxFloat)GetChildById("cbxSkewY").GetChildById("value");
		cbxSkewY.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Skew Y");
		};
		cbxRotation = (XUiC_ComboBoxFloat)GetChildById("cbxRotation").GetChildById("value");
		cbxRotation.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Skew Rotation");
		};
		SignData.SkewWarp skewWarp = new SignData.SkewWarp();
		SetDefaultValue("cbxSkewX", skewWarp.amount.x);
		SetDefaultValue("cbxSkewY", skewWarp.amount.y);
		SetDefaultValue("cbxRotation", skewWarp.rotation);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		currentWarp = warp as SignData.SkewWarp;
		if (currentWarp == null)
		{
			cbxSkewX.Value = 0.0;
			cbxSkewY.Value = 0.0;
			cbxRotation.Value = 0.0;
		}
		else
		{
			cbxSkewX.Value = currentWarp.amount.x;
			cbxSkewY.Value = currentWarp.amount.y;
			cbxRotation.Value = currentWarp.rotation;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentWarp != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentWarp.amount = new Vector2((float)cbxSkewX.Value, (float)cbxSkewY.Value);
			currentWarp.rotation = (float)cbxRotation.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
