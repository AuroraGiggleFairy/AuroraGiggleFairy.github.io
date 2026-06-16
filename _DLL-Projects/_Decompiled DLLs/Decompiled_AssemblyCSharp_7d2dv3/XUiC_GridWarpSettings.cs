using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GridWarpSettings : XUiC_SignWarpSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.GridWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController cbxAspectRow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxAspect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController cbxShiftRow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxShift;

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
		cbxMode = GetChildById("cbxMode").GetChildByType<XUiC_ComboBoxList<string>>();
		cbxMode.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnModeChanged();
		};
		cbxOffsetX = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetX").GetChildById("value");
		cbxOffsetX.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Grid Offset X");
		};
		cbxOffsetY = (XUiC_ComboBoxFloat)GetChildById("cbxOffsetY").GetChildById("value");
		cbxOffsetY.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Grid Offset Y");
		};
		cbxRotation = (XUiC_ComboBoxFloat)GetChildById("cbxRotation").GetChildById("value");
		cbxRotation.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Grid Rotation");
		};
		cbxScale = (XUiC_ComboBoxFloat)GetChildById("cbxScale").GetChildById("value");
		cbxScale.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Grid Scale");
		};
		cbxAspectRow = GetChildById("cbxAspect");
		cbxAspect = (XUiC_ComboBoxFloat)cbxAspectRow.GetChildById("value");
		cbxAspect.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Grid Aspect");
		};
		cbxShiftRow = GetChildById("cbxShift");
		cbxShift = (XUiC_ComboBoxFloat)cbxShiftRow.GetChildById("value");
		cbxShift.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Grid Shift");
		};
		SignData.GridWarp gridWarp = new SignData.GridWarp();
		SetDefaultValue("cbxMode", gridWarp.mode);
		SetDefaultValue("cbxOffsetX", gridWarp.offset.x);
		SetDefaultValue("cbxOffsetY", gridWarp.offset.y);
		SetDefaultValue("cbxRotation", gridWarp.rotation);
		SetDefaultValue("cbxScale", gridWarp.scale);
		SetDefaultValue("cbxAspect", gridWarp.aspect);
		SetDefaultValue("cbxShift", gridWarp.shift);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnModeChanged()
	{
		OnValueChangedGeneric("Changed Grid Mode");
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		currentWarp = warp as SignData.GridWarp;
		if (currentWarp == null)
		{
			cbxMode.SelectedIndex = 1;
			cbxOffsetX.Value = 0.0;
			cbxOffsetY.Value = 0.0;
			cbxRotation.Value = 0.0;
			cbxScale.Value = 1.0;
			cbxAspect.Value = 1.0;
			cbxShift.Value = 0.0;
			UpdateConditionalVisibility();
		}
		else
		{
			cbxMode.SelectedIndex = (int)currentWarp.mode;
			cbxOffsetX.Value = currentWarp.offset.x;
			cbxOffsetY.Value = currentWarp.offset.y;
			cbxRotation.Value = currentWarp.rotation;
			cbxScale.Value = currentWarp.scale;
			cbxAspect.Value = currentWarp.aspect;
			cbxShift.Value = currentWarp.shift;
			UpdateConditionalVisibility();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentWarp != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentWarp.mode = (SignData.GridWarp.Mode)cbxMode.SelectedIndex;
			currentWarp.offset = new Vector2((float)cbxOffsetX.Value, (float)cbxOffsetY.Value);
			currentWarp.rotation = (float)cbxRotation.Value;
			currentWarp.scale = (float)cbxScale.Value;
			currentWarp.aspect = (float)cbxAspect.Value;
			currentWarp.shift = (float)cbxShift.Value;
			UpdateConditionalVisibility();
			OnLayerSettingsChanged?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateConditionalVisibility()
	{
		bool isVisible = cbxMode.SelectedIndex == 1;
		cbxAspectRow.ViewComponent.IsVisible = isVisible;
		cbxShiftRow.ViewComponent.IsVisible = isVisible;
	}
}
