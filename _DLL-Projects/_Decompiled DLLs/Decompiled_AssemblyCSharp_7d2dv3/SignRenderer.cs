using System;
using UnityEngine;
using UnityEngine.Rendering;

public class SignRenderer : MonoBehaviour
{
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer renderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera targetCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SignCanvas.SignBlendMode signBlendMode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CommandBuffer decalCommandBuffer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material signMaterial;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDecal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAlphaBlendedSignMaterialPath = "@:Entities/Crafting/Materials/sign_decal.mat";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera alphaBlendCamera;

	public Renderer Renderer => renderer;

	public bool IsDecal => isDecal;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (isDecal && signBlendMode == SignCanvas.SignBlendMode.AlphaBlend && InitializeAlphaBlendDecal())
		{
			BuildDecalCommandBuffer();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (isDecal && signBlendMode == SignCanvas.SignBlendMode.AlphaBlend)
		{
			TeardownDecalIfNeeded();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InitializeAlphaBlendDecal()
	{
		targetCamera = ((!(alphaBlendCamera == null)) ? alphaBlendCamera : GameManager.Instance.World.GetPrimaryPlayer()?.playerCamera);
		if (signMaterial == null)
		{
			signMaterial = DataLoader.LoadAsset<Material>("@:Entities/Crafting/Materials/sign_decal.mat");
		}
		if (targetCamera == null || renderer == null || signMaterial == null)
		{
			Debug.LogError("[SignRenderer] Alpha blended rendering mode enabled but references are missing.", this);
			return false;
		}
		renderer.enabled = false;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TeardownDecalIfNeeded()
	{
		renderer.enabled = true;
		RemoveDecalCommandBuffer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildDecalCommandBuffer()
	{
		decalCommandBuffer = new CommandBuffer
		{
			name = "SignAlphaBlendedDecal: " + base.gameObject.name
		};
		decalCommandBuffer.DrawRenderer(renderer, signMaterial, 0, 0);
		targetCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, decalCommandBuffer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveDecalCommandBuffer()
	{
		if (targetCamera != null && decalCommandBuffer != null)
		{
			targetCamera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, decalCommandBuffer);
		}
		decalCommandBuffer?.Dispose();
		decalCommandBuffer = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRenderingOn()
	{
		if (isDecal && signBlendMode == SignCanvas.SignBlendMode.AlphaBlend)
		{
			renderer.enabled = false;
		}
	}

	public void SetRenderParameters(MaterialPropertyBlock mpb, SignCanvas.SignBlendMode blendMode = SignCanvas.SignBlendMode.Cutout, Camera alphaBlendCamera = null)
	{
		renderer.SetPropertyBlock(mpb);
		if (!IsDecal)
		{
			return;
		}
		signBlendMode = blendMode;
		this.alphaBlendCamera = alphaBlendCamera;
		switch (blendMode)
		{
		case SignCanvas.SignBlendMode.Cutout:
			TeardownDecalIfNeeded();
			break;
		case SignCanvas.SignBlendMode.AlphaBlend:
			if (InitializeAlphaBlendDecal())
			{
				RemoveDecalCommandBuffer();
				BuildDecalCommandBuffer();
			}
			break;
		}
	}
}
