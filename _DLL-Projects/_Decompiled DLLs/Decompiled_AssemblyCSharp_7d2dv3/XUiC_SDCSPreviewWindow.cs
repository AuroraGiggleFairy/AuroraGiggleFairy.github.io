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
	public enum eGamepadRotationMode
	{
		Bumper,
		Trigger,
		RightStick
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ZoomStates
	{
		Eyes,
		Head,
		Chest,
		FullBody
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture renderTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera renderCamera;

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

	[XuiBindComponent("zoomButton", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button zoomButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalCamPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalLightPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalCamRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalLightRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int originalPixelLightCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog uiBoneCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public ZoomStates state = ZoomStates.FullBody;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseOrtho = 1f;

	[XuiXmlAttribute("can_zoom", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool CanZoom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlAttribute("gamepad_rotation_mode", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public eGamepadRotationMode GamepadRotationMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		textPreview = (XUiV_Texture)GetChildById("playerPreview").ViewComponent;
		renderTextureSystem = new RenderTextureSystem();
		base.OnScroll += OnScrolled;
		base.OnDrag += OnDragged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnScroll -= OnScrolled;
		base.OnDrag -= OnDragged;
	}

	[XuiBindEvent("OnPress", "zoomButton")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ZoomButton_OnPress(XUiController _sender, int _mouseButton)
	{
		toggleHeadZoom();
	}

	[XuiBindEvent("OnVisiblity", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void visibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			startViz();
		}
		else
		{
			endViz();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startViz()
	{
		originalPixelLightCount = QualitySettings.pixelLightCount;
		QualitySettings.pixelLightCount = 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void endViz()
	{
		SDCSUtils.UnloadViz(previewTransform);
		renderTextureSystem.Cleanup();
		lastProfile = "";
		lastFieldOfView = 54f;
		QualitySettings.pixelLightCount = originalPixelLightCount;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		string text = ProfileSDF.CurrentProfileName();
		if (viewComponent.IsActiveInHierarchy && text != lastProfile)
		{
			Archetype = Archetype.GetArchetype(text);
			if (Archetype == null)
			{
				Archetype = ProfileSDF.CreateTempArchetype(text);
			}
			MakePreview();
			lastProfile = text;
			if (CanZoom)
			{
				state = ZoomStates.Head;
				setToHeadZoom();
			}
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			updateController();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateController()
	{
		if (XUiUtils.HotkeysAllowedFor(viewComponent))
		{
			float num = 0f;
			switch (GamepadRotationMode)
			{
			case eGamepadRotationMode.Bumper:
				num = xui.playerUI.playerInput.GUIActions.BumperAxis.Value;
				break;
			case eGamepadRotationMode.Trigger:
				num = xui.playerUI.playerInput.GUIActions.TriggerAxis.Value;
				break;
			case eGamepadRotationMode.RightStick:
				num = 0f - xui.playerUI.playerInput.GUIActions.Camera.Value.x;
				break;
			}
			if (num != 0f)
			{
				cameraRotate(0f - num);
			}
			if (xui.playerUI.playerInput.GUIActions.HalfStack.WasPressed)
			{
				toggleHeadZoom();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDragged(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		float x = _mousePositionDelta.x;
		if (xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.RightButton))
		{
			cameraRotate(x);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cameraVerticalPan(float _value)
	{
		renderCamera.transform.localPosition -= new Vector3(0f, _value, 0f);
		if (renderCamera.transform.localPosition.y < -1.5f)
		{
			renderCamera.transform.localPosition = new Vector3(renderCamera.transform.localPosition.x, -1.5f, renderCamera.transform.localPosition.z);
		}
		else if (renderCamera.transform.localPosition.y > 0f)
		{
			renderCamera.transform.localPosition = new Vector3(renderCamera.transform.localPosition.x, 0f, renderCamera.transform.localPosition.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cameraRotate(float _value)
	{
		renderCamera.transform.RotateAround(previewTransform.transform.position, Vector3.up, _value);
		renderTextureSystem.LightGO.transform.RotateAround(previewTransform.transform.position, Vector3.up, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrolled(XUiController _sender, float _delta)
	{
	}

	public void MakePreview()
	{
		SDCSUtils.CreateVizUI(Archetype, ref previewTransform, ref uiBoneCatalog);
		previewTransform.GetComponentInChildren<Animator>().Update(0f);
		init();
		previewTransform.transform.parent = renderTextureSystem.TargetGO.transform;
		previewTransform.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		previewTransform.transform.localPosition = new Vector3(0f, -0.9f, 0f);
		characterGazeController = previewTransform.GetComponentInChildren<CharacterGazeController>();
		Utils.SetLayerRecursively(renderTextureSystem.TargetGO, 11);
		textPreview.Texture = renderTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleHeadZoom()
	{
		state = state switch
		{
			ZoomStates.Eyes => ZoomStates.Head, 
			ZoomStates.Head => ZoomStates.Chest, 
			ZoomStates.Chest => ZoomStates.FullBody, 
			ZoomStates.FullBody => ZoomStates.Eyes, 
			_ => state, 
		};
		switch (state)
		{
		case ZoomStates.FullBody:
			setToFullBodyZoom();
			break;
		case ZoomStates.Head:
			setToHeadZoom();
			break;
		case ZoomStates.Chest:
			setToChestZoom();
			break;
		case ZoomStates.Eyes:
			setToEyeZoom();
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToFullBodyZoom()
	{
		renderCamera.transform.SetLocalPositionAndRotation(originalCamPosition, Quaternion.AngleAxis(19f, new Vector3(1f, 0f, 0f)) * originalCamRotation);
		renderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(originalLightPosition, originalLightRotation);
		renderCamera.fieldOfView = 54f;
		setCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToHeadZoom()
	{
		renderCamera.transform.SetLocalPositionAndRotation(originalCamPosition, Quaternion.AngleAxis(1.5f, new Vector3(1f, 0f, 0f)) * originalCamRotation);
		renderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(originalLightPosition, originalLightRotation);
		renderCamera.fieldOfView = 12f;
		renderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.015f, -0.78f, 2.14f);
		setCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToChestZoom()
	{
		renderCamera.transform.SetLocalPositionAndRotation(originalCamPosition, Quaternion.AngleAxis(5f, new Vector3(1f, 0f, 0f)) * originalCamRotation);
		renderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(originalLightPosition, originalLightRotation);
		renderCamera.fieldOfView = 20f;
		renderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.02f, -0.78f, 2.14f);
		setCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToEyeZoom()
	{
		renderCamera.transform.SetLocalPositionAndRotation(originalCamPosition, originalCamRotation);
		renderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(originalLightPosition, originalLightRotation);
		renderCamera.fieldOfView = 6f;
		renderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.015f, -0.78f, 2.14f);
		setCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setCameraInitialRotationOffset()
	{
		renderCamera.transform.RotateAround(previewTransform.transform.position, Vector3.up, -30f);
		renderTextureSystem.LightGO.transform.RotateAround(previewTransform.transform.position, Vector3.up, -30f);
		if (characterGazeController != null)
		{
			characterGazeController.SnapNextUpdate();
		}
	}

	public void ZoomToHead()
	{
		if (state != ZoomStates.Head && (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || xui.playerUI.playerInput.GUIActions.TriggerAxis.Value == 0f))
		{
			state = ZoomStates.Head;
			setToHeadZoom();
		}
	}

	public void ZoomToEye()
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || xui.playerUI.playerInput.GUIActions.TriggerAxis.Value == 0f)
		{
			state = ZoomStates.Eyes;
			setToEyeZoom();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void init()
	{
		if (!(renderTextureSystem.ParentGO != null))
		{
			renderTextureSystem.Create("characterpreview", new GameObject(), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), textPreview.Size, _isAA: true, _orthographic: true);
			renderTextureSystem.TargetGO.transform.localPosition = new Vector3(0f, -0.75f, 2.15f);
			renderTexture = renderTextureSystem.RenderTex;
			renderCamera = renderTextureSystem.CameraGO.GetComponent<Camera>();
			renderCamera.orthographic = false;
			renderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			renderCamera.fieldOfView = 54f;
			renderCamera.renderingPath = RenderingPath.DeferredShading;
			renderCamera.tag = "MainCamera";
			renderCamera.gameObject.AddComponent<StreamingController>();
			renderTextureSystem.CameraGO.AddComponent<NGSS_Local>().NGSS_PCSS_SOFTNESS_NEAR = 0.05f;
			HBAO hBAO = renderTextureSystem.CameraGO.AddComponent<HBAO>();
			hBAO.SetAoPerPixelNormals(HBAO.PerPixelNormals.Reconstruct);
			hBAO.SetAoIntensity(0.5f);
			renderTextureSystem.LightGO.GetComponent<Light>().enabled = false;
			GameObject gameObject = new GameObject("Key Light", typeof(Light));
			gameObject.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
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
			NGSS_FrustumShadows nGSS_FrustumShadows = renderTextureSystem.CameraGO.AddComponent<NGSS_FrustumShadows>();
			nGSS_FrustumShadows.mainShadowsLight = component;
			nGSS_FrustumShadows.m_fastBlur = false;
			nGSS_FrustumShadows.m_shadowsBlur = 1f;
			nGSS_FrustumShadows.m_shadowsBlurIterations = 4;
			nGSS_FrustumShadows.m_rayThickness = 0.025f;
			GameObject gameObject2 = new GameObject("Fill Light", typeof(Light));
			gameObject2.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
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
			gameObject3.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
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
			gameObject4.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
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
			originalCamPosition = renderCamera.transform.localPosition;
			originalCamRotation = renderCamera.transform.localRotation;
			originalLightPosition = renderTextureSystem.LightGO.transform.localPosition;
			originalLightRotation = renderTextureSystem.LightGO.transform.localRotation;
			renderCamera.transform.localPosition = new Vector3(0f, -0.75f, 0f);
		}
	}
}
