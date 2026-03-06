using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LocalPlayerCamera : MonoBehaviour
{
	public enum CameraType
	{
		None,
		Main,
		Weapon,
		UI
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float splitScreenFOVFactors = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera camera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CameraType cameraType;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerUI playerUI;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal entityPlayerLocal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayer localPlayer;

	public int uiChildIndex;

	public event Action<LocalPlayerCamera> PreCull;

	public event Action<LocalPlayerCamera> PreRender;

	public static LocalPlayerCamera AddToCamera(Camera camera, CameraType camType)
	{
		LocalPlayerCamera localPlayerCamera = camera.gameObject.AddMissingComponent<LocalPlayerCamera>();
		if (camType != CameraType.UI)
		{
			localPlayerCamera.Init(camType);
		}
		return localPlayerCamera;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(CameraType camType)
	{
		camera = GetComponent<Camera>();
		cameraType = camType;
		if (camType != CameraType.UI)
		{
			camera.allowDynamicResolution = true;
		}
		entityPlayerLocal = GetComponentInParent<EntityPlayerLocal>();
		localPlayer = GetComponentInParent<LocalPlayer>();
	}

	public void SetUI(LocalPlayerUI ui)
	{
		playerUI = ui;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		playerUI = GetComponentInChildren<LocalPlayerUI>();
		if ((bool)playerUI)
		{
			Init(CameraType.UI);
			playerUI.UpdateChildCameraIndices();
		}
		LocalPlayerManager.OnLocalPlayersChanged += HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		LocalPlayerManager.OnLocalPlayersChanged -= HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAttachedToLocalPlayer()
	{
		return localPlayer != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLocalPlayersChanged()
	{
		ModifyCameraProperties();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ModifyCameraProperties()
	{
		camera.enabled = true;
		if (IsAttachedToLocalPlayer())
		{
			camera.fieldOfView = (float)Constants.cDefaultCameraFieldOfView * splitScreenFOVFactors;
			return;
		}
		UIRect[] componentsInChildren = GetComponentsInChildren<UIRect>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateAnchors();
		}
		SetCameraDepth();
	}

	public void SetCameraDepth()
	{
		camera.depth = 1.01f + (float)playerUI.playerIndex * 0.01f + (float)uiChildIndex * 0.001f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCull()
	{
		if (cameraType == CameraType.Main)
		{
			OcclusionManager.Instance.LocalPlayerOnPreCull();
		}
		if (this.PreCull != null)
		{
			this.PreCull(this);
		}
		if (GameRenderManager.dynamicIsEnabled && (cameraType == CameraType.Main || cameraType == CameraType.Weapon))
		{
			camera.targetTexture = entityPlayerLocal.renderManager.GetDynamicRenderTexture();
		}
		if (cameraType == CameraType.Main)
		{
			entityPlayerLocal.renderManager.UpscalingPreCull();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender()
	{
		if (this.PreRender != null)
		{
			this.PreRender(this);
		}
	}
}
