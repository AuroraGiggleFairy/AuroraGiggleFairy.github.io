using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CameraWindow : XUiController
{
	public TileEntityPowered TileEntity;

	public bool UseEdgeDetection = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture cameraView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView cameraDrag;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView cameraClick;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPowerSystemCamera cameraController;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform cameraParentTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera sensorCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture myRenderTexture;

	public static bool hackyIsOpeningMaximizedWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool firstPass = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextModifiedTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBuried;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool maximizedWindow;

	public static string lastWindowGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDraggingCamera;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiController Owner { get; set; }

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("camera");
		if (childById != null)
		{
			cameraView = (XUiV_Texture)childById.ViewComponent;
		}
		XUiController childById2 = GetChildById("cameraDrag");
		if (childById2 != null)
		{
			cameraDrag = childById2.ViewComponent;
		}
		XUiController childById3 = GetChildById("cameraClick");
		if (childById3 != null)
		{
			cameraClick = childById3.ViewComponent;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (TileEntity == null)
		{
			return;
		}
		_ = xui.playerUI.localPlayer.entityPlayerLocal;
		maximizedWindow = hackyIsOpeningMaximizedWindow;
		hackyIsOpeningMaximizedWindow = false;
		if (!maximizedWindow && firstPass)
		{
			firstPass = false;
		}
		if (cameraClick != null)
		{
			cameraClick.Controller.OnPress += OnPreviewClicked;
		}
		if (TileEntity.BlockTransform != null)
		{
			cameraController = TileEntity.BlockTransform.GetComponent<IPowerSystemCamera>();
			if (cameraController == null)
			{
				OnClose();
				return;
			}
			GameManager.Instance.StartCoroutine(xui.playerUI.localPlayer.entityPlayerLocal.CancelInventoryActions([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Color white = Color.white;
				white.a = 0f;
				cameraController.SetConeColor(white);
				cameraController.SetConeActive(_active: true);
				cameraController.SetLaserActive(_active: true);
				if (cameraDrag != null)
				{
					TileEntity.SetUserAccessing(_bUserAccessing: true);
					TileEntity.SetModified();
					xui.playerUI.CursorController.SetCursorHidden(_hidden: true);
				}
				EntityPlayerLocal entityPlayerLocal = xui.playerUI.localPlayer.entityPlayerLocal;
				entityPlayerLocal.SetFirstPersonView(_bFirstPersonView: false, _bLerpPosition: true);
				cameraParentTransform = TileEntity.BlockTransform.FindInChilds("camera");
				myRenderTexture = new RenderTexture(cameraView.Size.x, cameraView.Size.y, 24);
				cameraController.SetUserAccessing(entityPlayerLocal.entityId);
				OcclusionManager.Instance.SetMultipleCameras(isMultiple: true);
			}, holsterWeapon: false));
		}
		else
		{
			OnClose();
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		OcclusionManager.Instance.SetMultipleCameras(isMultiple: false);
		if (!hackyIsOpeningMaximizedWindow && !maximizedWindow)
		{
			firstPass = false;
			if (xui != null && xui.playerUI != null && xui.playerUI.entityPlayer != null && xui.playerUI.entityPlayer.emodel != null)
			{
				xui.playerUI.entityPlayer.SwitchToPreferredCameraMode(_lerpPosition: true);
			}
		}
		if (cameraController != null)
		{
			cameraController.SetConeActive(_active: false);
			cameraController.SetLaserActive(_active: false);
			cameraController.SetConeColor(cameraController.GetOriginalConeColor());
			cameraController.SetUserAccessing(-1);
			cameraController = null;
		}
		DestroyCamera();
		myRenderTexture?.Release();
		myRenderTexture = null;
		cameraView.Texture = null;
		if (cameraClick != null)
		{
			cameraClick.Controller.OnPress -= OnPreviewClicked;
		}
		if (TileEntity == null)
		{
			return;
		}
		if (cameraDrag != null)
		{
			TileEntity.SetUserAccessing(_bUserAccessing: false);
			TileEntity.SetModified();
			if (xui != null && xui.playerUI != null && xui.playerUI.uiCamera != null && xui.playerUI.CursorController != null)
			{
				xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
			}
		}
		if (WireManager.Instance != null)
		{
			WireManager.Instance.RefreshPulseObjects();
		}
		xui.playerUI.CursorController.Locked = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (cameraParentTransform == null && TileEntity != null)
		{
			if (TileEntity.BlockTransform == null)
			{
				xui.playerUI.windowManager.CloseAllOpenModalWindows();
				return;
			}
			cameraParentTransform = TileEntity.BlockTransform.FindInChilds("camera");
		}
		if (sensorCamera == null && cameraParentTransform != null)
		{
			CreateCamera();
		}
		if (sensorCamera != null)
		{
			sensorCamera.backgroundColor = GameManager.Instance.World.m_WorldEnvironment.GetAmbientColor();
		}
		Vector3i pos = TileEntity.ToWorldPos();
		pos.y++;
		isBuried = GameManager.Instance.World.GetBlock(pos).Block.shape.IsTerrain();
		if (sensorCamera != null)
		{
			bool flag = TileEntity.IsPowered && !isBuried;
			sensorCamera.gameObject.SetActive(flag);
			cameraView.IsVisible = flag;
		}
		if (cameraDrag != null)
		{
			Vector2 vector = xui.playerUI.entityPlayer.MoveController.GetCameraInputSensitivity() * 2f;
			float x;
			float y;
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				x = xui.playerUI.playerInput.Look.X;
				y = xui.playerUI.playerInput.Look.Y;
			}
			else
			{
				x = xui.playerUI.playerInput.GUIActions.Look.X;
				y = xui.playerUI.playerInput.GUIActions.Look.Y;
			}
			x *= vector.x;
			y *= -1f * vector.y;
			if (cameraController is MotionSensorController || cameraController is SpotlightController)
			{
				TileEntity.CenteredYaw = Mathf.Clamp(TileEntity.CenteredYaw + x, -90f, 90f);
				TileEntity.CenteredPitch = Mathf.Clamp(TileEntity.CenteredPitch + y, -80f, 80f);
			}
			else
			{
				TileEntity.CenteredYaw += x;
				TileEntity.CenteredPitch = Mathf.Clamp(TileEntity.CenteredPitch + y, -80f, 80f);
			}
			PlayerActionsLocal playerInput = xui.playerUI.playerInput;
			PlayerActionsGUI gUIActions = playerInput.GUIActions;
			if (cameraController is AutoTurretController autoTurretController)
			{
				autoTurretController.FireController.PlayerFire(gUIActions.Submit.IsPressed || playerInput.Primary.IsPressed);
			}
		}
		if (Time.realtimeSinceStartup > nextModifiedTime)
		{
			TileEntity.SetModified();
			nextModifiedTime = Time.realtimeSinceStartup + 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateCamera()
	{
		GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("Prefabs/ElectricityCamera"), cameraParentTransform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		sensorCamera = gameObject.GetComponent<Camera>();
		sensorCamera.nearClipPlane = 0.01f;
		sensorCamera.depth = -10f;
		sensorCamera.farClipPlane = 1000f;
		sensorCamera.fieldOfView = 80f;
		sensorCamera.cullingMask &= -513;
		sensorCamera.renderingPath = RenderingPath.DeferredShading;
		sensorCamera.clearFlags = CameraClearFlags.Color;
		sensorCamera.targetTexture = myRenderTexture;
		if (SystemInfo.graphicsDeviceVersion.Contains("OpenGL"))
		{
			cameraView.Flip = UIBasicSprite.Flip.Vertically;
		}
		cameraView.Texture = sensorCamera.targetTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyCamera()
	{
		if (sensorCamera != null)
		{
			Object.DestroyImmediate(sensorCamera.gameObject);
		}
	}

	public void OnPreviewClicked(XUiController _sender, int _mouseButton)
	{
		if (TileEntity.IsPowered && !isBuried)
		{
			hackyIsOpeningMaximizedWindow = true;
			lastWindowGroup = windowGroup.Id;
			xui.playerUI.windowManager.Close(windowGroup);
			XUiC_PowerCameraWindowGroup obj = (XUiC_PowerCameraWindowGroup)((XUiWindowGroup)xui.playerUI.windowManager.GetWindow("powercamera")).Controller;
			obj.TileEntity = TileEntity;
			obj.UseEdgeDetection = UseEdgeDetection;
			xui.playerUI.windowManager.Open("powercamera", _bModal: true);
			xui.playerUI.entityPlayer.PlayOneShot("motion_sensor_trigger");
		}
		else
		{
			EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
			string text = (TileEntity.IsPowered ? "ttTurretIsBuried" : "ttRequiresPowerForCamera");
			GameManager.ShowTooltip(entityPlayer, text, string.Empty, "ui_denied");
		}
	}
}
