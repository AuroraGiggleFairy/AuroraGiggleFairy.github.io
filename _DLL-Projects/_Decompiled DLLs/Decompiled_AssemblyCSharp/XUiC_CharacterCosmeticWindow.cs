using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController previewFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer ep;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera cam;

	public RuntimeAnimatorController animationController;

	public float atlasResolutionScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RenderTextureSystem renderTextureSystem = new RenderTextureSystem();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPreviewDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform characterPivot;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float DragRotateSpeed = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ControllerRotateSpeed = 200f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMouseOverPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 lastMousePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RotationStickDeadzone = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const bool InvertRotationStick = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cursorLockActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 lockedCursorPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool prevCursorHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewSDCSObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog transformCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera renderCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public override void Init()
	{
		base.Init();
		lblName = (XUiV_Label)GetChildById("characterName").ViewComponent;
		textPreview = (XUiV_Texture)GetChildById("playerPreviewSDCS").ViewComponent;
		isDirty = true;
		XUiM_PlayerEquipment.HandleRefreshEquipment += XUiM_PlayerEquipment_HandleRefreshEquipment;
		base.xui.playerUI.OnUIShutdown += HandleUIShutdown;
		base.xui.OnShutdown += HandleUIShutdown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleUIShutdown()
	{
		base.xui.playerUI.OnUIShutdown -= HandleUIShutdown;
		base.xui.OnShutdown -= HandleUIShutdown;
		XUiM_PlayerEquipment.HandleRefreshEquipment -= XUiM_PlayerEquipment_HandleRefreshEquipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnHover(XUiController _sender, bool _isOver)
	{
		isMouseOverPreview = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnPress(XUiController _sender, int _mouseButton)
	{
		if (base.xui.dragAndDrop.CurrentStack != ItemStack.Empty)
		{
			ItemStack itemStack = base.xui.PlayerEquipment.EquipItem(base.xui.dragAndDrop.CurrentStack);
			if (base.xui.dragAndDrop.CurrentStack != itemStack)
			{
				base.xui.dragAndDrop.CurrentStack = itemStack;
				base.xui.dragAndDrop.PickUpType = XUiC_ItemStack.StackLocationTypes.Equipment;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		if (ep == null)
		{
			ep = base.xui.playerUI.entityPlayer;
		}
		if (isDirty)
		{
			if (player == null)
			{
				return;
			}
			lblName.Text = player.PlayerDisplayName;
			isDirty = false;
			RefreshBindings();
		}
		if (isPreviewDirty)
		{
			MakePreview();
		}
		textPreview.Texture = renderTextureSystem.RenderTex;
		if (characterPivot != null)
		{
			PlayerActionsGUI playerActionsGUI = base.xui?.playerUI?.playerInput?.GUIActions;
			if (playerActionsGUI != null)
			{
				bool num = PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard;
				base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
				bool flag = false;
				if (num)
				{
					flag = true;
					float x = playerActionsGUI.Camera.Value.x;
					if (Mathf.Abs(x) > 0.15f)
					{
						float num2 = -1f;
						characterPivot.Rotate(Vector3.up, num2 * x * 200f * _dt, Space.World);
					}
				}
				if (cursorLockActive)
				{
					if (flag)
					{
						MaintainCursorLock();
					}
					else
					{
						EndCursorLock();
					}
				}
			}
			bool mouseButton = base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			bool mouseButton2 = base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (mouseButton || mouseButton2)
			{
				if (!isDragging && isMouseOverPreview)
				{
					isDragging = true;
					lastMousePos = Input.mousePosition;
				}
				if (isDragging)
				{
					Vector2 vector = Input.mousePosition;
					Vector2 vector2 = vector - lastMousePos;
					lastMousePos = vector;
					if (Mathf.Abs(vector2.x) > 0.01f)
					{
						float num3 = vector2.x * 0.4f;
						characterPivot.Rotate(Vector3.up, 0f - num3, Space.World);
					}
				}
			}
			else
			{
				isDragging = false;
			}
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isDirty = true;
		isPreviewDirty = true;
		player = base.xui.playerUI.entityPlayer;
		previewFrame = GetChildById("previewFrameSDCS");
		if (previewFrame != null)
		{
			previewFrame.OnPress += PreviewFrame_OnPress;
			previewFrame.OnHover += PreviewFrame_OnHover;
		}
		textPreview = (XUiV_Texture)GetChildById("playerPreviewSDCS").ViewComponent;
		if (renderTextureSystem.ParentGO == null)
		{
			renderTextureSystem.Create("playerpreview", new GameObject(), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), textPreview.Size, _isAA: true);
		}
		renderCamera = renderTextureSystem.CameraGO.GetComponent<Camera>();
		renderCamera.transform.localPosition = new Vector3(0f, 1.5f, 0f);
		renderCamera.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
		renderCamera.orthographic = false;
		renderCamera.fieldOfView = 54f;
		renderCamera.renderingPath = RenderingPath.DeferredShading;
		if (characterPivot == null)
		{
			GameObject gameObject = new GameObject("CharacterPivot");
			characterPivot = gameObject.transform;
			characterPivot.SetParent(renderTextureSystem.ParentGO.transform, worldPositionStays: false);
			characterPivot.localPosition = new Vector3(0f, 0f, 2.15f);
			characterPivot.localRotation = Quaternion.AngleAxis(-30f, Vector3.up);
		}
		renderTextureSystem.LightGO.GetComponent<Light>().enabled = false;
		GameObject gameObject2 = new GameObject("Key Light", typeof(Light));
		gameObject2.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
		gameObject2.transform.SetPositionAndRotation(new Vector3(1.5f, 2.5f, -1.5f), Quaternion.Euler(20f, -20f, 0f));
		Light component = gameObject2.GetComponent<Light>();
		component.color = new Color(0.9f, 0.8f, 0.7f, 1f);
		component.type = LightType.Spot;
		component.range = 20f;
		component.spotAngle = 60f;
		component.intensity = 1.5f;
		component.shadows = LightShadows.Hard;
		component.shadowStrength = 0.2f;
		component.shadowBias = 0.005f;
		component.cullingMask = 2048;
		GameObject gameObject3 = new GameObject("Fill Light", typeof(Light));
		gameObject3.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
		gameObject3.transform.SetPositionAndRotation(new Vector3(-2f, 3f, 0f), Quaternion.Euler(35f, 45f, 0f));
		Light component2 = gameObject3.GetComponent<Light>();
		component2.color = new Color(1f, 1f, 1f, 1f);
		component2.type = LightType.Spot;
		component2.range = 20f;
		component2.spotAngle = 60f;
		component2.intensity = 0.5f;
		component2.shadows = LightShadows.Hard;
		component2.shadowStrength = 0.2f;
		component2.shadowBias = 0.005f;
		component2.cullingMask = 2048;
		GameObject gameObject4 = new GameObject("Fill 2 Light", typeof(Light));
		gameObject4.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
		gameObject4.transform.SetPositionAndRotation(new Vector3(0f, -0.5f, -1.5f), Quaternion.Euler(-20f, 0f, 0f));
		Light component3 = gameObject4.GetComponent<Light>();
		component3.color = new Color(1f, 1f, 1f, 1f);
		component3.type = LightType.Spot;
		component3.range = 20f;
		component3.spotAngle = 60f;
		component3.intensity = 0.5f;
		component3.shadows = LightShadows.Hard;
		component3.shadowStrength = 0.2f;
		component3.shadowBias = 0.005f;
		component3.cullingMask = 2048;
		GameObject gameObject5 = new GameObject("Back Light", typeof(Light));
		gameObject5.transform.SetParent(renderTextureSystem.LightGO.transform, worldPositionStays: false);
		gameObject5.transform.SetPositionAndRotation(new Vector3(-2f, 5f, 2f), Quaternion.Euler(60f, 105f, 0f));
		Light component4 = gameObject5.GetComponent<Light>();
		component4.color = new Color(0.4f, 0.75f, 1f, 1f);
		component4.type = LightType.Spot;
		component4.spotAngle = 60f;
		component4.range = 20f;
		component4.intensity = 1.5f;
		component4.shadows = LightShadows.Hard;
		component4.shadowStrength = 0.2f;
		component4.shadowBias = 0.005f;
		component4.cullingMask = 2048;
		if (player as EntityPlayerLocal != null && player.emodel as EModelSDCS != null)
		{
			XUiM_PlayerEquipment.HandleRefreshEquipment += XUiM_PlayerEquipment_HandleRefreshEquipment1;
		}
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment1(XUiM_PlayerEquipment playerEquipment)
	{
		if (base.IsOpen)
		{
			MakePreview();
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_PlayerEquipment.HandleRefreshEquipment -= XUiM_PlayerEquipment_HandleRefreshEquipment1;
		SDCSUtils.DestroyViz(previewSDCSObj);
		renderTextureSystem.Cleanup();
		EndCursorLock();
		characterPivot = null;
		isMouseOverPreview = false;
		isDragging = false;
		if (previewFrame != null)
		{
			previewFrame.OnPress -= PreviewFrame_OnPress;
			previewFrame.OnHover -= PreviewFrame_OnHover;
			previewFrame = null;
		}
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview);
	}

	public void MakePreview()
	{
		if (!(ep == null) && !(ep.emodel == null) && ep.emodel is EModelSDCS eModelSDCS)
		{
			isPreviewDirty = false;
			SDCSUtils.CreateVizUI(eModelSDCS.Archetype, ref previewSDCSObj, ref transformCatalog, ep, useTempCosmetics: true);
			Utils.SetLayerRecursively(previewSDCSObj, 11);
			Transform transform = previewSDCSObj.transform;
			transform.SetParent((characterPivot != null) ? characterPivot : renderTextureSystem.ParentGO.transform, worldPositionStays: false);
			transform.localPosition = Vector3.zero;
			transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			CharacterGazeController componentInChildren = previewSDCSObj.GetComponentInChildren<CharacterGazeController>();
			if ((bool)componentInChildren && renderCamera != null)
			{
				componentInChildren.LookAtTransformOverride = renderCamera.transform;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BeginCursorLock()
	{
		if (!cursorLockActive)
		{
			cursorLockActive = true;
			lockedCursorPos = base.xui.playerUI.CursorController.GetScreenPosition();
			prevCursorHidden = base.xui.playerUI.CursorController.VirtualCursorHidden;
			base.xui.playerUI.CursorController.VirtualCursorHidden = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MaintainCursorLock()
	{
		base.xui.playerUI.CursorController.SetScreenPosition(lockedCursorPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EndCursorLock()
	{
		if (cursorLockActive)
		{
			cursorLockActive = false;
			base.xui.playerUI.CursorController.VirtualCursorHidden = prevCursorHidden;
		}
	}
}
