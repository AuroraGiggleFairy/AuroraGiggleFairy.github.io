using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;
using WorldGenerationEngineFinal;

[Preserve]
public class XUiC_WorldGenerationPreview : XUiController
{
	public class PrefabNameHandler : MonoBehaviour
	{
		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public TextMesh textMesh;

		[PublicizedFrom(EAccessModifier.Private)]
		public void Awake()
		{
			textMesh = base.transform.GetComponent<TextMesh>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnMouseOver(bool isOver)
		{
			if (textMesh != null)
			{
				if (isOver)
				{
					base.gameObject.layer = 11;
				}
				else
				{
					base.gameObject.layer = 0;
				}
			}
		}
	}

	public static XUiC_WorldGenerationPreview Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject terrainPreviewRootObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture UIPreviewTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorldGenerationWindowGroup UIWinGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraRotX;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraRotY;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float currentFlySpeed = 150f;

	public static WorldBuilder worldBuilder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_WorldGenerationWindowGroup.Instance?.worldBuilder;
		}
	}

	public override void Init()
	{
		base.Init();
		Instance = this;
		UIPreviewTexture = (XUiV_Texture)base.ViewComponent;
		UIWinGroup = base.WindowGroup.Controller as XUiC_WorldGenerationWindowGroup;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void worldPreview_OnPress(XUiController _sender, int _button)
	{
		ResetCamera();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void worldPreview_OnDrag(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (renderTextureSystem != null && Input.GetMouseButton(1))
		{
			Transform transform = renderTextureSystem.CameraGO.transform;
			cameraRotX += _mousePositionDelta.y * -0.2f;
			cameraRotY += _mousePositionDelta.x * 0.2f;
			transform.rotation = Quaternion.Euler(cameraRotX, cameraRotY, 0f);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (renderTextureSystem != null)
		{
			Vector3 vector = ((PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard) ? UpdateMovementController(_dt) : UpdateMovementKeyboard());
			vector *= 150f * Time.deltaTime;
			Transform transform = renderTextureSystem.CameraGO.transform;
			Vector3 position = transform.position;
			position += transform.forward * vector.z;
			position += transform.right * vector.x;
			position += transform.up * vector.y;
			position.y = Utils.FastMax(40f, position.y);
			transform.position = position;
		}
	}

	public Vector3 UpdateMovementKeyboard()
	{
		Vector3 zero = Vector3.zero;
		if (!UICamera.inputHasFocus)
		{
			if (Input.GetKey(KeyCode.W))
			{
				zero.z = 1f;
			}
			else if (Input.GetKey(KeyCode.S))
			{
				zero.z = -1f;
			}
			if (Input.GetKey(KeyCode.A))
			{
				zero.x = -1f;
			}
			else if (Input.GetKey(KeyCode.D))
			{
				zero.x = 1f;
			}
			if (Input.GetKey(KeyCode.Space))
			{
				zero.y = 1f;
			}
			else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
			{
				zero.y = -1f;
			}
			if (InputUtils.ShiftKeyPressed)
			{
				zero *= 10f;
			}
		}
		return zero;
	}

	public Vector3 UpdateMovementController(float _dt)
	{
		Vector3 result = Vector3.zero;
		if (base.xui.playerUI.playerInput.GUIActions.PageUp.IsPressed)
		{
			base.xui.playerUI.CursorController.Locked = true;
			base.xui.playerUI.CursorController.VirtualCursorHidden = true;
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGEditor);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGCamera);
			result = new Vector3(base.xui.playerUI.playerInput.GUIActions.Look.X, 0f, base.xui.playerUI.playerInput.GUIActions.Look.Y);
			if (base.xui.playerUI.playerInput.GUIActions.PageDown.IsPressed)
			{
				result *= 10f;
			}
			Transform transform = renderTextureSystem.CameraGO.transform;
			cameraRotX += base.xui.playerUI.playerInput.GUIActions.Camera.Y * _dt * -165f;
			cameraRotY += base.xui.playerUI.playerInput.GUIActions.Camera.X * _dt * 165f;
			transform.rotation = Quaternion.Euler(cameraRotX, cameraRotY, 0f);
		}
		else
		{
			base.xui.playerUI.CursorController.Locked = false;
			base.xui.playerUI.CursorController.VirtualCursorHidden = false;
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGEditor);
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGCamera);
		}
		return result;
	}

	public Vector3 GetCameraPosition()
	{
		return renderTextureSystem.CameraGO.transform.position;
	}

	public override void OnOpen()
	{
		UIPreviewTexture = (XUiV_Texture)base.ViewComponent;
		UIWinGroup = base.WindowGroup.Controller as XUiC_WorldGenerationWindowGroup;
		UIPreviewTexture.Controller.OnPress += worldPreview_OnPress;
		UIPreviewTexture.Controller.OnDrag += worldPreview_OnDrag;
		initRenderTextureSystem();
		base.OnOpen();
	}

	public override void OnClose()
	{
		renderTextureSystem.SetEnabled(_b: false);
		destroyRenderTextureSystem();
		CleanupTerrainMesh();
		destroyPOIPreviews();
		UIPreviewTexture.Controller.OnPress -= worldPreview_OnPress;
		UIPreviewTexture.Controller.OnDrag -= worldPreview_OnDrag;
		UIPreviewTexture = null;
		base.xui.playerUI.CursorController.Locked = false;
		base.xui.playerUI.CursorController.VirtualCursorHidden = false;
		base.OnClose();
	}

	public void GeneratePreview()
	{
		CleanupTerrainMesh();
		if ((bool)terrainPreviewRootObj && worldBuilder != null)
		{
			WorldPreviewTerrain.GenerateTerrain(terrainPreviewRootObj.transform);
			ResetCamera();
		}
	}

	public void CleanupTerrainMesh()
	{
		if ((bool)terrainPreviewRootObj)
		{
			WorldPreviewTerrain.Cleanup(terrainPreviewRootObj);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void destroyPOIPreviews()
	{
		if (UIWinGroup != null && UIWinGroup.prefabPreviewManager != null)
		{
			UIWinGroup.prefabPreviewManager.Cleanup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initRenderTextureSystem()
	{
		if (renderTextureSystem == null)
		{
			renderTextureSystem = new RenderTextureSystem();
			terrainPreviewRootObj = new GameObject("TerrainMesh");
			renderTextureSystem.Create("worldpreview", terrainPreviewRootObj, new Vector3(0f, 0f, 0f), new Vector3(0f, 4000f, 0f), UIPreviewTexture.Size, _isAA: false);
			Camera component = renderTextureSystem.CameraGO.transform.GetComponent<Camera>();
			component.nearClipPlane = 0.1f;
			component.farClipPlane = 20000f;
			ResetCamera();
			Transform transform = renderTextureSystem.LightGO.transform;
			transform.localPosition = new Vector3(0f, 2000f, 0f);
			transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
			transform.GetComponent<Light>().type = LightType.Directional;
			transform.GetComponent<Light>().intensity = 1.2f;
			terrainPreviewRootObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			terrainPreviewRootObj.transform.position = new Vector3(-10240f, 0f, -10240f);
			UIPreviewTexture.Texture = renderTextureSystem.RenderTex;
		}
		renderTextureSystem.SetEnabled(_b: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void destroyRenderTextureSystem()
	{
		UnityEngine.Object.Destroy(renderTextureSystem.ParentGO);
		renderTextureSystem = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetCamera()
	{
		if (renderTextureSystem != null && worldBuilder != null)
		{
			cameraRotX = 90f;
			cameraRotY = 0f;
			Transform transform = renderTextureSystem.CameraGO.transform;
			transform.localPosition = new Vector3(0f, (float)worldBuilder.WorldSize * 0.8745f, 0f);
			transform.localRotation = Quaternion.Euler(cameraRotX, cameraRotY, 0f);
		}
	}
}
