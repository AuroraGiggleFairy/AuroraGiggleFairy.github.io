using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignGroupSettings : XUiC_SignLayerSettings
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBakeColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBakeOffsets;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAffectChildren;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAffectPivot;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxColorMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxOffsetTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSoftnessOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDilateOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.GroupSignLayer currentLayer;

	public Action<XUiC_SignEditorWindow.AffectMode> OnAffectModePressed;

	public override void Init()
	{
		base.Init();
		btnBakeColor = GetChildById("btnBakeColor").GetChildByType<XUiC_SimpleButton>();
		btnBakeColor.OnPressed += BtnBakeColor_OnPressed;
		btnBakeOffsets = GetChildById("btnBakeOffsets").GetChildByType<XUiC_SimpleButton>();
		btnBakeOffsets.OnPressed += BtnBakeOffsets_OnPressed;
		btnAffectChildren = GetChildById("btnAffectChildren")?.GetChildByType<XUiC_SimpleButton>();
		btnAffectChildren.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController sender, int button) =>
		{
			OnAffectModePressed?.Invoke(XUiC_SignEditorWindow.AffectMode.Children);
		};
		btnAffectPivot = GetChildById("btnAffectPivot")?.GetChildByType<XUiC_SimpleButton>();
		btnAffectPivot.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController sender, int button) =>
		{
			OnAffectModePressed?.Invoke(XUiC_SignEditorWindow.AffectMode.Pivot);
		};
		cbxColorMode = GetChildById("cbxColorMode").GetChildByType<XUiC_ComboBoxList<string>>();
		cbxColorMode.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Group Color Mode");
		};
		cbxOffsetTarget = GetChildById("cbxOffsetTarget").GetChildByType<XUiC_ComboBoxList<string>>();
		cbxOffsetTarget.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Group Offset Target");
		};
		cbxSoftnessOffset = (XUiC_ComboBoxFloat)GetChildById("cbxSoftnessOffset").GetChildById("value");
		cbxSoftnessOffset.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Group Softness Offset");
		};
		cbxDilateOffset = (XUiC_ComboBoxFloat)GetChildById("cbxDilateOffset").GetChildById("value");
		cbxDilateOffset.OnValueChangedGeneric += [PublicizedFrom(EAccessModifier.Private)] (XUiController _) =>
		{
			OnValueChangedGeneric("Changed Group Dilate Offset");
		};
	}

	public void SetAffectModeVisual(XUiC_SignEditorWindow.AffectMode mode)
	{
		bool selected = mode == XUiC_SignEditorWindow.AffectMode.Children;
		bool selected2 = mode == XUiC_SignEditorWindow.AffectMode.Pivot;
		btnAffectChildren.Button.Selected = selected;
		btnAffectPivot.Button.Selected = selected2;
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		currentLayer = layer as SignData.GroupSignLayer;
		if (currentLayer == null)
		{
			cbxColorMode.SelectedIndex = 0;
			cbxOffsetTarget.SelectedIndex = 0;
			cbxSoftnessOffset.Value = 0.0;
			cbxDilateOffset.Value = 0.0;
		}
		else
		{
			cbxColorMode.SelectedIndex = (int)currentLayer.colorMode;
			cbxOffsetTarget.SelectedIndex = (int)currentLayer.offsetTarget;
			cbxSoftnessOffset.Value = currentLayer.softnessOffset;
			cbxDilateOffset.Value = currentLayer.dilateOffset;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (currentLayer != null)
		{
			OnPreLayerSettingsChanged?.Invoke(_changeDescription, arg2: false);
			currentLayer.colorMode = (SignData.GroupSignLayer.ColorMode)cbxColorMode.SelectedIndex;
			currentLayer.offsetTarget = (SignData.GroupSignLayer.OffsetTarget)cbxOffsetTarget.SelectedIndex;
			currentLayer.softnessOffset = (float)cbxSoftnessOffset.Value;
			currentLayer.dilateOffset = (float)cbxDilateOffset.Value;
			OnLayerSettingsChanged?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBakeColor_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (currentLayer != null)
		{
			OnPreLayerSettingsChanged?.Invoke("Baked Group Color", arg2: false);
			BakeColorRecursive(currentLayer, new List<SignDataManager.ColorOperation>());
			SetLayer(currentLayer);
			OnLayerSettingsChanged?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBakeOffsets_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (currentLayer != null)
		{
			OnPreLayerSettingsChanged?.Invoke("Baked Group Offsets", arg2: false);
			BakeOffsetsRecursive(currentLayer, default(SignDataManager.GroupOffsets));
			SetLayer(currentLayer);
			OnLayerSettingsChanged?.Invoke();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void BakeColorRecursive(SignData.GroupSignLayer group, List<SignDataManager.ColorOperation> colorOps)
	{
		Color color = group.renderSettings.color;
		bool flag = (group.colorMode == SignData.GroupSignLayer.ColorMode.Multiply && color == Color.white) || (group.colorMode == SignData.GroupSignLayer.ColorMode.Blend && color.a == 0f);
		if (!flag)
		{
			colorOps.Add(new SignDataManager.ColorOperation
			{
				color = group.renderSettings.color,
				mode = group.colorMode
			});
		}
		foreach (SignData.SignLayer layer in group.layers)
		{
			if (layer is SignData.GroupSignLayer groupSignLayer)
			{
				BakeColorRecursive(groupSignLayer, colorOps);
			}
			else
			{
				layer.renderSettings.color = SignDataManager.ColorOperation.EvaluateColor(layer.renderSettings.color, colorOps);
			}
		}
		if (!flag)
		{
			colorOps.RemoveAt(colorOps.Count - 1);
		}
		group.colorMode = SignData.GroupSignLayer.ColorMode.Multiply;
		group.renderSettings.color = Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void BakeOffsetsRecursive(SignData.GroupSignLayer group, SignDataManager.GroupOffsets offsets)
	{
		offsets = offsets.WithOffsets(group.softnessOffset, group.dilateOffset, group.offsetTarget);
		foreach (SignData.SignLayer layer in group.layers)
		{
			if (layer is SignData.GroupSignLayer groupSignLayer)
			{
				BakeOffsetsRecursive(groupSignLayer, offsets);
			}
			else if (!(layer is SignData.PolygonSignLayer polygonSignLayer))
			{
				if (!(layer is SignData.TextSignLayer textSignLayer))
				{
					if (layer is SignData.NoiseSignLayer noiseSignLayer)
					{
						noiseSignLayer.softness = Mathf.Max(0f, noiseSignLayer.softness + offsets.noiseSoftnessOffset);
						noiseSignLayer.dilate += offsets.noiseDilateOffset;
					}
				}
				else
				{
					textSignLayer.softness = Mathf.Clamp01(textSignLayer.softness + offsets.textSoftnessOffset);
					textSignLayer.dilate = Mathf.Clamp(textSignLayer.dilate + offsets.textDilateOffset, -1f, 1f);
				}
			}
			else
			{
				polygonSignLayer.softness = Mathf.Max(0f, polygonSignLayer.softness + offsets.shapeSoftnessOffset);
				polygonSignLayer.dilate += offsets.shapeDilateOffset;
			}
		}
		group.offsetTarget = SignData.GroupSignLayer.OffsetTarget.All;
		group.softnessOffset = 0f;
		group.dilateOffset = 0f;
	}
}
