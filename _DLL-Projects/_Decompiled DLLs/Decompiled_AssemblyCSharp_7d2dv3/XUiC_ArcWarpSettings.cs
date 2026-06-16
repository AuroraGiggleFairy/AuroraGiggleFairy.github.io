using UnityEngine.Scripting;

[Preserve]
public class XUiC_ArcWarpSettings : XUiC_SignWarpSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.ArcWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxWidth;

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
		cbxRotation = (XUiC_ComboBoxFloat)GetChildById("cbxRotation").GetChildById("value");
		cbxRotation.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Arc Rotation");
		};
		cbxRadius = (XUiC_ComboBoxFloat)GetChildById("cbxRadius").GetChildById("value");
		cbxRadius.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Arc Radius");
		};
		cbxWidth = (XUiC_ComboBoxFloat)GetChildById("cbxWidth").GetChildById("value");
		cbxWidth.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Arc Width");
		};
		SignData.ArcWarp arcWarp = new SignData.ArcWarp();
		SetDefaultValue("cbxRotation", arcWarp.rotation);
		SetDefaultValue("cbxRadius", arcWarp.radius);
		SetDefaultValue("cbxWidth", arcWarp.width);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		currentWarp = warp as SignData.ArcWarp;
		if (currentWarp == null)
		{
			cbxRotation.Value = 0.0;
			cbxRadius.Value = 0.0;
			cbxWidth.Value = 0.0;
		}
		else
		{
			cbxRotation.Value = currentWarp.rotation;
			cbxRadius.Value = currentWarp.radius;
			cbxWidth.Value = currentWarp.width;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentWarp != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentWarp.rotation = (float)cbxRotation.Value;
			currentWarp.radius = (float)cbxRadius.Value;
			currentWarp.width = (float)cbxWidth.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
