using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignLayerDragAndDropIcon : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId signId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer layer;

	public override void Init()
	{
		base.Init();
		signMaterial = GetChildById("signMaterial").ViewComponent as XUiV_Texture;
		XUiV_Texture xUiV_Texture = signMaterial;
		xUiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(OnWillRender));
		signMaterial.Texture = Texture2D.whiteTexture;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
		if (viewComponent.IsVisible)
		{
			Vector2 screenPosition = xui.playerUI.CursorController.GetScreenPosition();
			Vector3 position = xui.playerUI.camera.ScreenToWorldPoint(screenPosition);
			Transform transform = xui.transform;
			position.z = transform.position.z - 3f * transform.lossyScale.z;
			base.ViewComponent.UiTransform.position = position;
		}
	}

	public void SetLayer(GlobalSignId _signId, SignData.SignLayer _layer)
	{
		signId = _signId;
		layer = _layer;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		if (layer != null)
		{
			SignUIStyle style = ((layer.renderSettings.mode == SignData.SignRenderSettings.Mode.ColorOnly || layer.renderSettings.mode == SignData.SignRenderSettings.Mode.ColorAndMask) ? SignUIStyle.LayerThumbnail : SignUIStyle.MaskThumbnail);
			SignDataManager.Instance.TryApplyRenderingData(signId, 1f, mat, layer, style);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (base.GetBindingValueInternal(ref _value, _bindingName))
		{
			return true;
		}
		if (_bindingName == "showdraganddrop")
		{
			_value = (layer != null).ToString();
			return true;
		}
		return false;
	}
}
