using System;
using UnityEngine;

public class FaceSpriteAtCamera : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera mainCamera;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (GameManager.IsDedicatedServer)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (GameManager.IsDedicatedServer)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
		mainCamera = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		if (mainCamera != null)
		{
			base.transform.LookAt(mainCamera.transform.position, -Vector3.up);
		}
	}
}
