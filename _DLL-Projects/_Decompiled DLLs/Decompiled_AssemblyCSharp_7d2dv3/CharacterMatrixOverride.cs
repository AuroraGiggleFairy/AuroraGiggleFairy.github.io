using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMatrixOverride : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class RendererSettings
	{
		public MaterialPropertyBlock originalProperties;

		public MaterialPropertyBlock overriddenProperties;
	}

	public bool Active;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera referenceCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Renderer> overriddenRenderers = new List<Renderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Renderer, RendererSettings> rendererSettingsMap = new Dictionary<Renderer, RendererSettings>();

	public void Init(EntityPlayerLocal epl)
	{
		referenceCamera = epl.playerCamera;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(OnPreCullCallback));
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(OnPreCullCallback));
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRenderCallback));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCallback));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCullCallback(Camera camera)
	{
		if (Active && camera == referenceCamera)
		{
			UpdateRendererList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRendererList()
	{
		overriddenRenderers.Clear();
		GetComponentsInChildren(overriddenRenderers);
		foreach (Renderer overriddenRenderer in overriddenRenderers)
		{
			if (!rendererSettingsMap.TryGetValue(overriddenRenderer, out var value))
			{
				value = new RendererSettings();
				value.originalProperties = new MaterialPropertyBlock();
				value.overriddenProperties = new MaterialPropertyBlock();
				rendererSettingsMap[overriddenRenderer] = value;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRenderCallback(Camera camera)
	{
		if (!Active || camera != referenceCamera)
		{
			return;
		}
		Matrix4x4 value = Matrix4x4.Perspective(90f, 1f, 10000f, 10000.1f) * referenceCamera.worldToCameraMatrix;
		foreach (Renderer overriddenRenderer in overriddenRenderers)
		{
			if (!rendererSettingsMap.TryGetValue(overriddenRenderer, out var value2))
			{
				Log.Error("[CharacterMatrixOverride] Failed to retrieve RendererSettings for overridden renderer");
				continue;
			}
			overriddenRenderer.GetPropertyBlock(value2.originalProperties);
			overriddenRenderer.GetPropertyBlock(value2.overriddenProperties);
			MaterialPropertyBlock overriddenProperties = value2.overriddenProperties;
			overriddenProperties.SetMatrix("unity_MatrixVP", value);
			overriddenRenderer.SetPropertyBlock(overriddenProperties);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPostRenderCallback(Camera camera)
	{
		if (camera != referenceCamera)
		{
			return;
		}
		foreach (Renderer overriddenRenderer in overriddenRenderers)
		{
			if (!(overriddenRenderer == null) && rendererSettingsMap.TryGetValue(overriddenRenderer, out var value))
			{
				overriddenRenderer.SetPropertyBlock(value.originalProperties);
			}
		}
	}
}
