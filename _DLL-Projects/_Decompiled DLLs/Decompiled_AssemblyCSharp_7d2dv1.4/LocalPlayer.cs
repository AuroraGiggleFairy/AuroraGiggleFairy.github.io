using System;
using System.Collections;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AvatarLocalPlayerController avatarController;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal entityPlayerLocal
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public LocalPlayerUI playerUI
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		avatarController = GetComponentInChildren<AvatarLocalPlayerController>();
		entityPlayerLocal = GetComponent<EntityPlayerLocal>();
		playerUI = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		LocalPlayerCamera.CameraType camType = LocalPlayerCamera.CameraType.Main;
		Camera[] componentsInChildren = GetComponentsInChildren<Camera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Camera camera = componentsInChildren[i];
			if (i > 0)
			{
				camType = LocalPlayerCamera.CameraType.Weapon;
			}
			if (camera.name != "FinalCamera")
			{
				LocalPlayerCamera.AddToCamera(camera, camType).SetUI(playerUI);
			}
		}
		Transform transform = entityPlayerLocal.playerCamera.transform;
		Transform transform2 = transform.Find("ScreenEffectsWithDepth");
		if (transform2 != null)
		{
			SetupLocalPlayerVisual(transform2.Find("UnderwaterHaze"));
		}
		Transform transform3 = transform.Find("effect_refract_plane");
		if (transform3 != null)
		{
			transform3.GetComponent<MeshRenderer>().material.SetInt("_ZTest", 8);
			SetupLocalPlayerVisual(transform3);
		}
		SetupLocalPlayerVisual(transform.Find("effect_underwater_debris"));
		SetupLocalPlayerVisual(transform.Find("effect_dropletsParticle"));
		SetupLocalPlayerVisual(transform.Find("effect_water_fade"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupLocalPlayerVisual(Transform _transform)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Start()
	{
		DispatchLocalPlayersChanged();
		while (avatarController == null)
		{
			yield return null;
			avatarController = GetComponentInChildren<AvatarLocalPlayerController>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		LocalPlayerManager.OnLocalPlayersChanged += HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		LocalPlayerManager.OnLocalPlayersChanged -= HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DispatchLocalPlayersChanged()
	{
		LocalPlayerManager.LocalPlayersChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLocalPlayersChanged()
	{
		int num = 0;
		Camera[] componentsInChildren = GetComponentsInChildren<Camera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth = -1f + (float)(playerUI.playerIndex * 2 + num++) * 0.01f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		DispatchLocalPlayersChanged();
	}
}
