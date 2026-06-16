using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignLayer : XUiC_SignGridEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	public Action<XUiC_SignLayer> OnBecameCursorSelected;

	public Action<XUiC_SignLayer> OnClicked;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 multiSelectColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerGrid parentGrind;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignEditorWindow editorWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool layerValid = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController leftInsertPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController rightInsertPoint;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId signId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer layer
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int index
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override bool IsSelectable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			_ = signId;
			if (!signId.IsValid)
			{
				return IsPlaceholder;
			}
			return true;
		}
	}

	public override Color32 BackgroundColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (parentGrind == null || !parentGrind.MultiSelectedLayerIndices.Contains(index))
			{
				return XUiC_SignGridEntry.backgroundColor;
			}
			return multiSelectColor;
		}
	}

	public bool LayerValid => layerValid;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlaceholder
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float LeftInsertX => leftInsertPoint.ViewComponent.UiTransform.position.x;

	public float RightInsertX => rightInsertPoint.ViewComponent.UiTransform.position.x;

	public override void Init()
	{
		base.Init();
		signMaterial = GetChildById("signMaterial").ViewComponent as XUiV_Texture;
		parentGrind = GetParentByType<XUiC_SignLayerGrid>();
		editorWindow = GetParentByType<XUiC_SignEditorWindow>();
		signMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xUiV_Texture = signMaterial;
		xUiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(OnWillRender));
		signMaterial.Texture = Texture2D.whiteTexture;
		XUiC_SignEditorWindow xUiC_SignEditorWindow = editorWindow;
		xUiC_SignEditorWindow.OnRefreshed = (Action)Delegate.Combine(xUiC_SignEditorWindow.OnRefreshed, new Action(RefreshLayerValidity));
		rightInsertPoint = GetChildById("rightInsertPoint");
		leftInsertPoint = GetChildById("leftInsertPoint");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshLayerValidity()
	{
		layerValid = IsLayerValid(layer);
		[PublicizedFrom(EAccessModifier.Private)]
		bool IsLayerValid(SignData.SignLayer layer)
		{
			if (layer == null)
			{
				return false;
			}
			if (!editorWindow.signComplexityInfo.IsValid)
			{
				return false;
			}
			if (!editorWindow.signComplexityInfo.TryGetLayerComplexityInfo(layer, out var layerComplexityInfo))
			{
				return false;
			}
			if (layerComplexityInfo.MaxCompStackIndex > 7)
			{
				return false;
			}
			if (layerComplexityInfo.MaxUVStackIndex > 7)
			{
				return false;
			}
			return true;
		}
	}

	public void SetAsPlaceholder(GlobalSignId signId, int index)
	{
		IsPlaceholder = true;
		this.signId = signId;
		layer = null;
		this.index = index;
		signMaterial.IsVisible = signId.IsValid;
		base.ViewComponent.Enabled = signId.IsValid;
		viewComponent.ToolTip = Localization.Get("lblSignAddLayer");
		RefreshBindings();
	}

	public void SetLayer(GlobalSignId signId, SignData.SignLayer layer, int index)
	{
		IsPlaceholder = false;
		this.signId = signId;
		this.layer = layer;
		this.index = index;
		signMaterial.IsVisible = signId.IsValid;
		base.ViewComponent.Enabled = signId.IsValid;
		RefreshTooltip();
		RefreshBindings();
		RefreshLayerValidity();
	}

	public void RefreshTooltip()
	{
		if (signId.IsValid)
		{
			if (string.IsNullOrEmpty(layer.name))
			{
				viewComponent.ToolTip = string.Format("{0} {1}", Localization.Get("xuiPaintLayer"), (index + 1).ToString("00"));
			}
			else
			{
				viewComponent.ToolTip = layer.name;
			}
		}
		else
		{
			viewComponent.ToolTip = string.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleClick()
	{
		if (!IsPlaceholder)
		{
			OnClicked?.Invoke(this);
		}
	}

	public override void OnCursorSelected(bool _isActualElement)
	{
		base.OnCursorSelected(_isActualElement);
		OnBecameCursorSelected?.Invoke(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		if (IsPlaceholder)
		{
			SignDataManager.Instance.TryApplyRenderingData(signId, 1f, mat, null, SignUIStyle.AddLayer);
		}
		else if (layer != null)
		{
			bool flag = layer.renderSettings.mode == SignData.SignRenderSettings.Mode.ColorOnly || layer.renderSettings.mode == SignData.SignRenderSettings.Mode.ColorAndMask;
			SignUIStyle style = ((!layerValid) ? (flag ? SignUIStyle.LayerThumbnailInvalid : SignUIStyle.MaskThumbnailInvalid) : (flag ? SignUIStyle.LayerThumbnail : SignUIStyle.MaskThumbnail));
			SignDataManager.Instance.TryApplyRenderingData(signId, 1f, mat, layer, style);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetItemNameText(string name)
	{
		viewComponent.ToolTip = name;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (base.GetBindingValueInternal(ref value, bindingName))
		{
			return true;
		}
		switch (bindingName)
		{
		case "layernumber":
			value = (signId.IsValid ? (index + 1).ToString("00") : string.Empty);
			return true;
		case "maskicon":
			if (!signId.IsValid || layer == null)
			{
				value = "ui_game_symbol_lock";
				return true;
			}
			switch (layer.renderSettings.mode)
			{
			case SignData.SignRenderSettings.Mode.ColorOnly:
				value = "ui_game_symbol_lock";
				return true;
			case SignData.SignRenderSettings.Mode.ColorAndMask:
				value = "ui_game_symbol_signs_color_and_mask";
				return true;
			case SignData.SignRenderSettings.Mode.MaskOnly:
				value = "ui_game_symbol_signs_mask_only";
				return true;
			case SignData.SignRenderSettings.Mode.PunchOut:
				value = "ui_game_symbol_signs_punch_out";
				return true;
			default:
				return false;
			}
		case "showmaskicon":
			if (!signId.IsValid || layer == null)
			{
				value = false.ToString();
				return true;
			}
			value = (layer.renderSettings.mode != SignData.SignRenderSettings.Mode.ColorOnly).ToString();
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (base.ParseAttribute(name, value))
		{
			return true;
		}
		if (name == "multi_select_color")
		{
			multiSelectColor = StringParsers.ParseColor32(value);
			return true;
		}
		return false;
	}
}
