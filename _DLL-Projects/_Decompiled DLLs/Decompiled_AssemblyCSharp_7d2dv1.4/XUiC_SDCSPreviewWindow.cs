using HorizonBasedAmbientOcclusion;
using PI.NGSS;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class XUiC_SDCSPreviewWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum ZoomStates
	{
		Eyes,
		Head,
		Chest,
		FullBody
	}

	public RenderTextureSystem RenderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	public RenderTexture RenderTexture;

	public Camera RenderCamera;

	public Transform TargetTransform;

	public GameObject RotateTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastFieldOfView;

	public Archetype Archetype;

	[PublicizedFrom(EAccessModifier.Private)]
	public CharacterGazeController characterGazeController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController zoomButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalCamPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalLightPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalCamRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalLightRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canZoom = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int originalPixelLightCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog uiBoneCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public ZoomStates state = ZoomStates.FullBody;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseOrtho = 1f;

	public override void Init()
	{
		base.Init();
		textPreview = (XUiV_Texture)GetChildById("playerPreview").ViewComponent;
		textPreview.UpdateData();
		RenderTextureSystem = new RenderTextureSystem();
		zoomButton = GetChildById("zoomButton");
		if (zoomButton != null)
		{
			zoomButton.OnPress += ZoomButton_OnPress;
		}
		RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ZoomButton_OnPress(XUiController _sender, int _mouseButton)
	{
		toggleHeadZoom();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		originalPixelLightCount = QualitySettings.pixelLightCount;
		QualitySettings.pixelLightCount = 4;
		if (zoomButton != null)
		{
			zoomButton.ViewComponent.IsVisible = PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		SDCSUtils.DestroyViz(previewTransform);
		RenderTextureSystem.Cleanup();
		lastProfile = "";
		lastFieldOfView = 54f;
		QualitySettings.pixelLightCount = originalPixelLightCount;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		string text = ProfileSDF.CurrentProfileName();
		if (text != lastProfile)
		{
			Archetype = Archetype.GetArchetype(text);
			if (Archetype == null)
			{
				Archetype = ProfileSDF.CreateTempArchetype(text);
			}
			MakePreview();
			lastProfile = text;
			if (canZoom)
			{
				state = ZoomStates.Head;
				SetToHeadZoom();
			}
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			UpdateController();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		if (zoomButton != null)
		{
			zoomButton.ViewComponent.IsVisible = _newStyle == PlayerInputManager.InputStyle.Keyboard;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateController()
	{
		float value = base.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value;
		if (value != 0f)
		{
			CameraRotate(0f - value);
		}
		if (base.xui.playerUI.playerInput.GUIActions.HalfStack.WasPressed)
		{
			toggleHeadZoom();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		base.OnDragged(_dragType, _mousePositionDelta);
		float x = _mousePositionDelta.x;
		if (base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.RightButton))
		{
			CameraRotate(x);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraVerticalPan(float _value)
	{
		RenderCamera.transform.localPosition -= new Vector3(0f, _value, 0f);
		if (RenderCamera.transform.localPosition.y < -1.5f)
		{
			RenderCamera.transform.localPosition = new Vector3(RenderCamera.transform.localPosition.x, -1.5f, RenderCamera.transform.localPosition.z);
		}
		else if (RenderCamera.transform.localPosition.y > 0f)
		{
			RenderCamera.transform.localPosition = new Vector3(RenderCamera.transform.localPosition.x, 0f, RenderCamera.transform.localPosition.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraRotate(float _value)
	{
		RenderCamera.transform.RotateAround(previewTransform.transform.position, Vector3.up, _value);
		RenderTextureSystem.LightGO.transform.RotateAround(previewTransform.transform.position, Vector3.up, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnScrolled(float _delta)
	{
		base.OnScrolled(_delta);
	}

	public void MakePreview()
	{
		SDCSUtils.CreateVizUI(Archetype, ref previewTransform, ref uiBoneCatalog);
		previewTransform.GetComponentInChildren<Animator>().Update(0f);
		init();
		previewTransform.transform.parent = RenderTextureSystem.TargetGO.transform;
		previewTransform.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		previewTransform.transform.localPosition = new Vector3(0f, -0.9f, 0f);
		characterGazeController = previewTransform.GetComponentInChildren<CharacterGazeController>();
		Utils.SetLayerRecursively(RenderTextureSystem.TargetGO, 11);
		textPreview.Texture = RenderTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleHeadZoom()
	{
		switch (state)
		{
		case ZoomStates.Eyes:
			state = ZoomStates.Head;
			break;
		case ZoomStates.Head:
			state = ZoomStates.Chest;
			break;
		case ZoomStates.Chest:
			state = ZoomStates.FullBody;
			break;
		case ZoomStates.FullBody:
			state = ZoomStates.Eyes;
			break;
		}
		switch (state)
		{
		case ZoomStates.FullBody:
			SetToFullBodyZoom();
			break;
		case ZoomStates.Head:
			SetToHeadZoom();
			break;
		case ZoomStates.Chest:
			SetToChestZoom();
			break;
		case ZoomStates.Eyes:
			SetToEyeZoom();
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToFullBodyZoom()
	{
		RenderCamera.transform.SetLocalPositionAndRotation(originalCamPosition, Quaternion.AngleAxis(19f, new Vector3(1f, 0f, 0f)) * originalCamRotation);
		RenderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(originalLightPosition, originalLightRotation);
		RenderCamera.fieldOfView = 54f;
		SetCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToHeadZoom()
	{
		RenderCamera.transform.SetLocalPositionAndRotation(originalCamPosition, Quaternion.AngleAxis(1.5f, new Vector3(1f, 0f, 0f)) * originalCamRotation);
		RenderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(originalLightPosition, originalLightRotation);
		RenderCamera.fieldOfView = 12f;
		RenderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.015f, -0.78f, 2.14f);
		SetCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToChestZoom()
	{
		RenderCamera.transform.SetLocalPositionAndRotation(originalCamPosition, Quaternion.AngleAxis(5f, new Vector3(1f, 0f, 0f)) * originalCamRotation);
		RenderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(originalLightPosition, originalLightRotation);
		RenderCamera.fieldOfView = 20f;
		RenderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.02f, -0.78f, 2.14f);
		SetCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToEyeZoom()
	{
		RenderCamera.transform.SetLocalPositionAndRotation(originalCamPosition, originalCamRotation);
		RenderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(originalLightPosition, originalLightRotation);
		RenderCamera.fieldOfView = 6f;
		RenderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.015f, -0.78f, 2.14f);
		SetCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCameraInitialRotationOffset()
	{
		RenderCamera.transform.RotateAround(previewTransform.transform.position, Vector3.up, -30f);
		RenderTextureSystem.LightGO.transform.RotateAround(previewTransform.transform.position, Vector3.up, -30f);
		if (characterGazeController != null)
		{
			characterGazeController.SnapNextUpdate();
		}
	}

	public void ZoomToHead()
	{
		if (state != ZoomStates.Head && (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || base.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value == 0f))
		{
			state = ZoomStates.Head;
			SetToHeadZoom();
		}
	}

	public void ZoomToEye()
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || base.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value == 0f)
		{
			state = ZoomStates.Eyes;
			SetToEyeZoom();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void init()
	{
		if (RenderTextureSystem.ParentGO == null)
		{
			RenderTextureSystem.Create("characterpreview", new GameObject(), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), textPreview.Size, _isAA: true, _orthographic: true);
			RenderTextureSystem.TargetGO.transform.localPosition = new Vector3(0f, -0.75f, 2.15f);
			RenderTexture = RenderTextureSystem.RenderTex;
			RenderCamera = RenderTextureSystem.CameraGO.GetComponent<Camera>();
			RenderCamera.orthographic = false;
			RenderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			RenderCamera.fieldOfView = 54f;
			RenderCamera.renderingPath = RenderingPath.DeferredShading;
			RenderCamera.tag = "MainCamera";
			RenderCamera.gameObject.AddComponent<StreamingController>();
			RenderTextureSystem.CameraGO.AddComponent<NGSS_Local>().NGSS_PCSS_SOFTNESS_NEAR = 0.05f;
			HBAO hBAO = RenderTextureSystem.CameraGO.AddComponent<HBAO>();
			hBAO.SetAoPerPixelNormals(HBAO.PerPixelNormals.Reconstruct);
			hBAO.SetAoIntensity(0.5f);
			RenderTextureSystem.LightGO.GetComponent<Light>().enabled = false;
			GameObject gameObject = new GameObject("Key Light", typeof(Light));
			gameObject.transform.SetParent(RenderTextureSystem.LightGO.transform, worldPositionStays: false);
			gameObject.transform.SetPositionAndRotation(new Vector3(0.25f, 0.475f, 0.62f), Quaternion.Euler(33f, -8f, 0f));
			gameObject.AddComponent<NGSS_Directional>().NGSS_PCSS_ENABLED = true;
			Light component = gameObject.GetComponent<Light>();
			component.color = new Color(0.9f, 0.8f, 0.7f, 1f);
			component.type = LightType.Spot;
			component.range = 20f;
			component.spotAngle = 60f;
			component.intensity = 1.5f;
			component.shadows = LightShadows.Hard;
			component.shadowStrength = 0.2f;
			component.shadowBias = 0.005f;
			NGSS_FrustumShadows nGSS_FrustumShadows = RenderTextureSystem.CameraGO.AddComponent<NGSS_FrustumShadows>();
			nGSS_FrustumShadows.mainShadowsLight = component;
			nGSS_FrustumShadows.m_fastBlur = false;
			nGSS_FrustumShadows.m_shadowsBlur = 1f;
			nGSS_FrustumShadows.m_shadowsBlurIterations = 4;
			nGSS_FrustumShadows.m_rayThickness = 0.025f;
			GameObject gameObject2 = new GameObject("Fill Light", typeof(Light));
			gameObject2.transform.SetParent(RenderTextureSystem.LightGO.transform, worldPositionStays: false);
			gameObject2.transform.SetPositionAndRotation(new Vector3(-1.15f, 1.4f, 1f), Quaternion.Euler(50f, 45f, 0f));
			Light component2 = gameObject2.GetComponent<Light>();
			component2.color = new Color(1f, 1f, 1f, 1f);
			component2.type = LightType.Spot;
			component2.range = 20f;
			component2.spotAngle = 60f;
			component2.intensity = 0.5f;
			component2.shadows = LightShadows.Hard;
			component2.shadowStrength = 0.2f;
			component2.shadowBias = 0.005f;
			GameObject gameObject3 = new GameObject("Fill 2 Light", typeof(Light));
			gameObject3.transform.SetParent(RenderTextureSystem.LightGO.transform, worldPositionStays: false);
			gameObject3.transform.SetPositionAndRotation(new Vector3(0f, -1.5f, -0.5f), Quaternion.Euler(-15f, 0f, 0f));
			Light component3 = gameObject3.GetComponent<Light>();
			component3.color = new Color(1f, 1f, 1f, 1f);
			component3.type = LightType.Spot;
			component3.range = 20f;
			component3.spotAngle = 60f;
			component3.intensity = 0.5f;
			component3.shadows = LightShadows.Hard;
			component3.shadowStrength = 0.2f;
			component3.shadowBias = 0.005f;
			GameObject gameObject4 = new GameObject("Back Light", typeof(Light));
			gameObject4.transform.SetParent(RenderTextureSystem.LightGO.transform, worldPositionStays: false);
			gameObject4.transform.SetPositionAndRotation(new Vector3(-0.6f, 0.75f, 2.6f), Quaternion.Euler(55f, 133f, 0f));
			Light component4 = gameObject4.GetComponent<Light>();
			component4.color = new Color(0.4f, 0.75f, 1f, 1f);
			component4.type = LightType.Spot;
			component4.spotAngle = 60f;
			component4.range = 20f;
			component4.intensity = 1.5f;
			component4.shadows = LightShadows.Hard;
			component4.shadowStrength = 0.2f;
			component4.shadowBias = 0.005f;
			originalCamPosition = RenderCamera.transform.localPosition;
			originalCamRotation = RenderCamera.transform.localRotation;
			originalLightPosition = RenderTextureSystem.LightGO.transform.localPosition;
			originalLightRotation = RenderTextureSystem.LightGO.transform.localRotation;
			RenderCamera.transform.localPosition = new Vector3(0f, -0.75f, 0f);
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (name == "can_zoom")
			{
				canZoom = StringParsers.ParseBool(value);
				return true;
			}
			return false;
		}
		return flag;
	}
}
