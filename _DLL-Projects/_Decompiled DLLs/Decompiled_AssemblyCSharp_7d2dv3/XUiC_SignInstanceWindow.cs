using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignInstanceWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureCanvas signTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<SignCanvas.SignBlendMode> comboBlendMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleShowOnImposter;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera renderCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture previewTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SignRenderer> previewSignRenderers = new List<SignRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Light previewLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject decalBackgroundMesh;

	public override void Init()
	{
		base.Init();
		comboBlendMode = GetChildByType<XUiC_ComboBoxEnum<SignCanvas.SignBlendMode>>();
		toggleShowOnImposter = GetChildById("toggleShowOnImposter") as XUiC_ToggleButton;
		cbxRotation = GetChildById("cbxRotation").GetChildByType<XUiC_ComboBoxInt>();
		cbxRotation.OnValueChangedGeneric += HandleRotationChanged;
		previewTexture = (XUiV_Texture)GetChildById("previewMaterial").ViewComponent;
		XUiController childById = GetChildById("previewArea");
		childById.OnDrag += HandlePreviewDragged;
		childById.OnScroll += HandlePreviewScrolled;
		GetChildById("btnToggleLight").OnPress += HandleToggleLight;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty && base.ViewComponent.IsVisible)
		{
			IsDirty = false;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		comboBlendMode.OnValueChanged -= ComboBlendMode_OnValueChanged;
		toggleShowOnImposter.OnValueChanged -= ToggleShowOnImposter_OnValueChanged;
		CleanupPreview();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		comboBlendMode.OnValueChanged -= ComboBlendMode_OnValueChanged;
		comboBlendMode.OnValueChanged += ComboBlendMode_OnValueChanged;
		toggleShowOnImposter.OnValueChanged -= ToggleShowOnImposter_OnValueChanged;
		toggleShowOnImposter.OnValueChanged += ToggleShowOnImposter_OnValueChanged;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboBlendMode_OnValueChanged(XUiController _sender, SignCanvas.SignBlendMode _oldValue, SignCanvas.SignBlendMode _newValue)
	{
		if (signTileEntity?.Canvas != null)
		{
			signTileEntity.Canvas.BlendMode = _newValue;
			RefreshPreviewRenderers();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleShowOnImposter_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (signTileEntity?.Canvas != null)
		{
			signTileEntity.Canvas.ShowOnImposter = _newValue;
			signTileEntity.SetModified();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "blockname":
			value = ((signTileEntity != null) ? signTileEntity.Parent.block.GetBlockName() : "");
			return true;
		case "showblendmode":
			value = (signTileEntity?.Canvas != null && signTileEntity.Canvas.IsDecal).ToString();
			return true;
		case "allowedonimposter":
			value = (signTileEntity?.Canvas != null && signTileEntity.Canvas.AllowedOnImposter).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	public void InitialiseTo(TEFeatureCanvas sign)
	{
		signTileEntity = sign;
		comboBlendMode.OnValueChanged -= ComboBlendMode_OnValueChanged;
		toggleShowOnImposter.OnValueChanged -= ToggleShowOnImposter_OnValueChanged;
		if (signTileEntity?.Canvas != null)
		{
			comboBlendMode.Value = sign.Canvas.BlendMode;
			cbxRotation.Value = (int)sign.Canvas.CanvasRotation * 90;
			toggleShowOnImposter.Value = sign.Canvas.ShowOnImposter;
		}
		else
		{
			comboBlendMode.Value = SignCanvas.SignBlendMode.Cutout;
			cbxRotation.Value = 0L;
			toggleShowOnImposter.Value = false;
		}
		comboBlendMode.OnValueChanged += ComboBlendMode_OnValueChanged;
		toggleShowOnImposter.OnValueChanged += ToggleShowOnImposter_OnValueChanged;
		RefreshBindings();
		SetupPreview(sign);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupPreview(TEFeatureCanvas sign)
	{
		CleanupPreview();
		if (sign == null || !(sign.Parent.block.shape is BlockShapeModelEntity blockShapeModelEntity))
		{
			return;
		}
		GameObject gameObject = new GameObject("SignPreviewPivot");
		renderTextureSystem = new RenderTextureSystem();
		renderTextureSystem.Create("signPreview", gameObject, Vector3.zero, new Vector3(1f, 1.5f, -1f), new Vector2i(previewTexture.Size.x, previewTexture.Size.y), _isAA: true);
		Transform transform = blockShapeModelEntity.CloneModel(sign.Parent.blockValue, gameObject.transform);
		Utils.SetLayerRecursively(gameObject, 11);
		Quaternion rotationStatic = BlockShapeNew.GetRotationStatic(sign.Parent.blockValue.rotation);
		Vector3 vector = rotationStatic * Vector3.forward;
		if (Mathf.Abs(vector.y) < 0.5f)
		{
			vector.y = 0f;
			float angle = Vector3.SignedAngle(vector, Vector3.back, Vector3.up);
			gameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
		}
		else
		{
			gameObject.transform.rotation = Quaternion.AngleAxis(180f, Vector3.up) * Quaternion.Inverse(rotationStatic);
		}
		transform.GetComponentsInChildren(includeInactive: true, previewSignRenderers);
		renderCamera = renderTextureSystem.CameraGO.GetComponent<Camera>();
		renderCamera.fieldOfView = 40f;
		renderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		renderCamera.renderingPath = RenderingPath.DeferredShading;
		renderCamera.farClipPlane = 32f;
		Bounds bounds = ComputeBounds(gameObject);
		Vector3 center = bounds.center;
		float f = renderCamera.fieldOfView * 0.5f * (MathF.PI / 180f);
		float num = (float)previewTexture.Size.x / (float)previewTexture.Size.y;
		float num2 = Mathf.Max(bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z));
		maxDistance = Mathf.Max(num2 / Mathf.Tan(f) * 1.2f, 1.5f);
		minDistance = maxDistance * 0.1f;
		float a = bounds.extents.y / Mathf.Tan(f);
		float b = bounds.extents.x / (Mathf.Tan(f) * num);
		currentDistance = Mathf.Clamp(Mathf.Max(a, b) * 1.15f, minDistance, maxDistance);
		gameObject.transform.position -= center;
		renderCamera.transform.position = new Vector3(0.1f, 0.15f, -1f).normalized * currentDistance;
		renderCamera.transform.LookAt(Vector3.zero);
		if (sign.Canvas.IsDecal)
		{
			if (decalBackgroundMesh == null)
			{
				InitializeDecalBackgroundMesh();
			}
			decalBackgroundMesh.SetActive(value: true);
			decalBackgroundMesh.transform.SetParent(transform.transform);
			decalBackgroundMesh.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			decalBackgroundMesh.transform.localPosition = new Vector3(0f, 2f, -0.5f);
		}
		else
		{
			decalBackgroundMesh?.SetActive(value: false);
		}
		Light component = renderTextureSystem.LightGO.GetComponent<Light>();
		component.type = LightType.Directional;
		component.intensity = 1f;
		component.color = Color.white;
		renderTextureSystem.LightGO.transform.position = renderCamera.transform.position;
		renderTextureSystem.LightGO.transform.rotation = renderCamera.transform.rotation;
		previewLight = component;
		previewTexture.Texture = renderTextureSystem.RenderTex;
		RefreshPreviewRenderers();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeDecalBackgroundMesh()
	{
		decalBackgroundMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
		decalBackgroundMesh.name = "SignInstancePreviewBackgroundMesh";
		decalBackgroundMesh.layer = 11;
		decalBackgroundMesh.transform.localScale = Vector3.one * 32f;
		MeshRenderer component = decalBackgroundMesh.GetComponent<MeshRenderer>();
		component.material = Resources.Load<Material>("Materials/SignPreviewBackground");
		component.material.SetVector("_MainTex_ST", new Vector4(4f, 4f, 0f, 0f));
		decalBackgroundMesh.GetComponent<Collider>().enabled = false;
	}

	public void RefreshPreviewRenderers()
	{
		if (signTileEntity != null && previewSignRenderers.Count != 0)
		{
			SignDataManager.Instance.TryApplyRenderingData(signTileEntity.Canvas.DisplaySignId, [PublicizedFrom(EAccessModifier.Private)] (MaterialPropertyBlock mpb) =>
			{
				mpb.SetInteger(SignShaderIDs._UseTexture, 0);
				mpb.SetTexture(SignShaderIDs._BakedTexture, Texture2D.whiteTexture);
				mpb.SetFloat(SignShaderIDs._CanvasRotation, signTileEntity.Canvas.CanvasRotationRadians);
			}, previewSignRenderers, signTileEntity.Canvas.BlendMode, renderCamera);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Bounds ComputeBounds(GameObject go)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return new Bounds(go.transform.position, Vector3.one);
		}
		Transform transform = go.GetComponentInChildren<TemporaryObject>()?.transform;
		Bounds bounds = componentsInChildren[0].bounds;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			if (!(transform != null) || !componentsInChildren[i].transform.IsChildOf(transform))
			{
				bounds.Encapsulate(componentsInChildren[i].bounds);
			}
		}
		return bounds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CleanupPreview()
	{
		if (decalBackgroundMesh != null)
		{
			decalBackgroundMesh.transform.SetParent(base.ViewComponent.UiTransform);
			decalBackgroundMesh.SetActive(value: false);
		}
		previewSignRenderers.Clear();
		if (renderTextureSystem != null)
		{
			renderTextureSystem.Cleanup();
			renderTextureSystem = null;
		}
		renderCamera = null;
		previewLight = null;
		if (previewTexture != null)
		{
			previewTexture.Texture = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePreviewDragged(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (!(renderCamera == null))
		{
			switch (UICamera.currentKey)
			{
			case KeyCode.Mouse0:
				OrbitCamera(_mousePositionDelta);
				break;
			case KeyCode.Mouse1:
			case KeyCode.Mouse2:
				ZoomCamera(_mousePositionDelta.y * 0.005f);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePreviewScrolled(XUiController _sender, float _delta)
	{
		ZoomCamera(_delta);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OrbitCamera(Vector2 _mousePositionDelta)
	{
		if (Mathf.Abs(_mousePositionDelta.x) > 0.01f)
		{
			renderCamera.transform.RotateAround(Vector3.zero, Vector3.up, _mousePositionDelta.x * 0.5f);
		}
		if (Mathf.Abs(_mousePositionDelta.y) > 0.01f)
		{
			renderCamera.transform.RotateAround(Vector3.zero, renderCamera.transform.right, (0f - _mousePositionDelta.y) * 0.5f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ZoomCamera(float _zoomDelta)
	{
		if (!(renderCamera == null))
		{
			currentDistance = Mathf.Clamp(currentDistance - _zoomDelta * maxDistance, minDistance, maxDistance);
			renderCamera.transform.position = renderCamera.transform.position.normalized * currentDistance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleToggleLight(XUiController _sender, int _mouseButton)
	{
		if (previewLight != null)
		{
			previewLight.enabled = !previewLight.enabled;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRotationChanged(XUiController _sender)
	{
		if (signTileEntity != null)
		{
			signTileEntity.Canvas.CanvasRotation = (CanvasRotationMode)(cbxRotation.Value / 90);
			cbxRotation.Value = (int)signTileEntity.Canvas.CanvasRotation * 90;
			RefreshPreviewRenderers();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}
}
