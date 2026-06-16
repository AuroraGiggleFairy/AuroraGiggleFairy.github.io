using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_KaleidoWarpSettings : XUiC_SignWarpSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.KaleidoWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxSides;

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
		cbxOffsetX = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetX").GetChildById("value");
		cbxOffsetX.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Kaleido Offset X");
		};
		cbxOffsetY = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetY").GetChildById("value");
		cbxOffsetY.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Kaleido Offset Y");
		};
		cbxOffsetScale = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetScale").GetChildById("value");
		cbxOffsetScale.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Kaleido Offset Scale");
		};
		cbxSides = (XUiC_ComboBoxInt)GetChildById("cbxSides").GetChildById("value");
		cbxSides.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Kaleido Sides");
		};
		cbxRotation = (XUiC_ComboBoxFloat)GetChildById("cbxRotation").GetChildById("value");
		cbxRotation.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Kaleido Rotation");
		};
		SignData.KaleidoWarp kaleidoWarp = new SignData.KaleidoWarp();
		SetDefaultValue("cbxOffsetX", kaleidoWarp.offset.x);
		SetDefaultValue("cbxOffsetY", kaleidoWarp.offset.y);
		SetDefaultValue("cbxOffsetScale", kaleidoWarp.offsetScale);
		SetDefaultValue("cbxSides", kaleidoWarp.sides);
		SetDefaultValue("cbxRotation", kaleidoWarp.rotation);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		currentWarp = warp as SignData.KaleidoWarp;
		if (currentWarp == null)
		{
			cbxOffsetX.Value = 0.0;
			cbxOffsetY.Value = 0.0;
			cbxOffsetScale.Value = 0.0;
			cbxSides.Value = 0L;
			cbxRotation.Value = 0.0;
		}
		else
		{
			cbxOffsetX.Value = currentWarp.offset.x;
			cbxOffsetY.Value = currentWarp.offset.y;
			cbxOffsetScale.Value = currentWarp.offsetScale;
			cbxSides.Value = currentWarp.sides;
			cbxRotation.Value = currentWarp.rotation;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentWarp != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentWarp.offset = new Vector2((float)cbxOffsetX.Value, (float)cbxOffsetY.Value);
			currentWarp.offsetScale = (float)cbxOffsetScale.Value;
			currentWarp.sides = (int)cbxSides.Value;
			currentWarp.rotation = (float)cbxRotation.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}
}
