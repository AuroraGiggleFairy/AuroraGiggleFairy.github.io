using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PerspectiveWarpSettings : XUiC_SignWarpSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.PerspectiveWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotationX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotationY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotationZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxStrength;

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
		cbxRotationX = (XUiC_ComboBoxFloat)GetChildById("cbxRotationX").GetChildById("value");
		cbxRotationX.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Perspective Rotation X");
		};
		cbxRotationY = (XUiC_ComboBoxFloat)GetChildById("cbxRotationY").GetChildById("value");
		cbxRotationY.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Perspective Rotation Y");
		};
		cbxRotationZ = (XUiC_ComboBoxFloat)GetChildById("cbxRotationZ").GetChildById("value");
		cbxRotationZ.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Perspective Rotation Z");
		};
		cbxStrength = (XUiC_ComboBoxFloat)GetChildById("cbxStrength").GetChildById("value");
		cbxStrength.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Perspective Strength");
		};
		SignData.PerspectiveWarp perspectiveWarp = new SignData.PerspectiveWarp();
		SetDefaultValue("cbxRotationX", perspectiveWarp.rotation.x);
		SetDefaultValue("cbxRotationY", perspectiveWarp.rotation.y);
		SetDefaultValue("cbxRotationZ", perspectiveWarp.rotation.z);
		SetDefaultValue("cbxStrength", perspectiveWarp.strength);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		currentWarp = warp as SignData.PerspectiveWarp;
		if (currentWarp == null)
		{
			cbxRotationX.Value = 0.0;
			cbxRotationY.Value = 0.0;
			cbxRotationZ.Value = 0.0;
			cbxStrength.Value = 0.0;
		}
		else
		{
			cbxRotationX.Value = currentWarp.rotation.x;
			cbxRotationY.Value = currentWarp.rotation.y;
			cbxRotationZ.Value = currentWarp.rotation.z;
			cbxStrength.Value = currentWarp.strength;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentWarp != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentWarp.rotation = new Vector3((float)cbxRotationX.Value, (float)cbxRotationY.Value, (float)cbxRotationZ.Value);
			currentWarp.strength = (float)cbxStrength.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
