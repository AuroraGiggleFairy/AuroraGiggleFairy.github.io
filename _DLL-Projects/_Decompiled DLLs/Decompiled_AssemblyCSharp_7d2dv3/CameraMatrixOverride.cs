using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraMatrixOverride : MonoBehaviour
{
	public enum ProjectionMode
	{
		Custom,
		Reference,
		ReferenceNonJittered
	}

	public enum UpdateTiming
	{
		LateUpdate,
		OnPreCull,
		OnPreRender,
		None
	}

	[Serializable]
	public class AdvancedSettings
	{
		public bool enableNearClipOverride = true;

		public bool enableChildShadows;

		public bool enableBoundsPadding = true;

		public ProjectionMode projectionMode;

		public UpdateTiming updateTiming = UpdateTiming.OnPreCull;

		[Range(float.Epsilon, 2f)]
		public float depthScaleFactor = 1f;

		public float farClipFactor = 1f;

		public float jitterFactor = 1f;

		public float boundsPadding = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class RendererSettings
	{
		public ShadowCastingMode originalShadowCastingMode;

		public MaterialPropertyBlock originalProperties;

		public MaterialPropertyBlock overriddenProperties;

		public bool boundsDirty;

		public bool shadowModeDirty;
	}

	[Tooltip("The overridden FoV to use when rendering any child Renderers in the hierarchy beneath the Camera this script is attached to.")]
	public float fov = 45f;

	[Range(0.01f, 1f)]
	[Tooltip("The overridden near-clip distance to use when this script is enabled. Note this applies to the Camera as a whole, rather than specifically targeting child Renderers.")]
	public float nearClipOverride = 0.01f;

	[Range(float.Epsilon, 8f)]
	[Tooltip("A value of 1 results in normal rendering behaviour. Higher values effectively squash the depth of child Renderers towards the camera; this reduces the likelihood of clipping into environment geometry, but can distort certain screen effects such as reflections. A value of 2 seems to provide a good balance between reducing clipping and minimising distortion of screen effects.")]
	public float nearClipFactor = 2f;

	[Tooltip("An assortment of parameters left over from earlier prototyping. They remain exposed for debug purposes if ever required; otherwise it is not recommended to change them away from their default values.")]
	public AdvancedSettings advancedSettings = new AdvancedSettings();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera referenceCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Renderer, RendererSettings> rendererSettingsMap = new Dictionary<Renderer, RendererSettings>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<Renderer> overriddenRenderers = new List<Renderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Renderer> renderersToRestore = new HashSet<Renderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float originalNearClip = 0.0751f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (referenceCamera == null && !TryGetComponent<Camera>(out referenceCamera))
		{
			Debug.LogError("Failed to get Camera. The CameraMatrixOverride script must be attached to a GameObject with a Camera component.");
			base.enabled = false;
		}
		else
		{
			originalNearClip = referenceCamera.nearClipPlane;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (!(referenceCamera == null))
		{
			referenceCamera.nearClipPlane = originalNearClip;
			RestoreChildSettings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRendererList()
	{
		bool flag = advancedSettings.enableBoundsPadding && referenceCamera.fieldOfView < fov;
		renderersToRestore.Clear();
		foreach (Renderer overriddenRenderer in overriddenRenderers)
		{
			if (overriddenRenderer != null)
			{
				renderersToRestore.Add(overriddenRenderer);
			}
		}
		overriddenRenderers.Clear();
		GetComponentsInChildren(overriddenRenderers);
		Vector3 vector = new Vector3(advancedSettings.boundsPadding, advancedSettings.boundsPadding, advancedSettings.boundsPadding);
		foreach (Renderer overriddenRenderer2 in overriddenRenderers)
		{
			renderersToRestore.Remove(overriddenRenderer2);
			if (!rendererSettingsMap.TryGetValue(overriddenRenderer2, out var value))
			{
				value = new RendererSettings();
				value.originalShadowCastingMode = overriddenRenderer2.shadowCastingMode;
				value.originalProperties = new MaterialPropertyBlock();
				value.overriddenProperties = new MaterialPropertyBlock();
				rendererSettingsMap[overriddenRenderer2] = value;
			}
			if (advancedSettings.enableChildShadows && value.shadowModeDirty)
			{
				overriddenRenderer2.shadowCastingMode = value.originalShadowCastingMode;
				value.shadowModeDirty = false;
			}
			else if (!advancedSettings.enableChildShadows && !value.shadowModeDirty)
			{
				overriddenRenderer2.shadowCastingMode = ShadowCastingMode.Off;
				value.shadowModeDirty = true;
			}
			if (flag && !(overriddenRenderer2 is ParticleSystemRenderer))
			{
				overriddenRenderer2.ResetBounds();
				Bounds bounds = overriddenRenderer2.bounds;
				bounds.extents += vector;
				overriddenRenderer2.bounds = bounds;
				value.boundsDirty = true;
			}
			else if (value.boundsDirty)
			{
				overriddenRenderer2.ResetBounds();
				value.boundsDirty = false;
			}
		}
		foreach (Renderer item in renderersToRestore)
		{
			if (rendererSettingsMap.TryGetValue(item, out var value2))
			{
				if (value2.shadowModeDirty)
				{
					item.shadowCastingMode = value2.originalShadowCastingMode;
					value2.shadowModeDirty = false;
				}
				if (value2.boundsDirty)
				{
					item.ResetBounds();
					value2.boundsDirty = false;
				}
			}
		}
		renderersToRestore.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (advancedSettings.updateTiming == UpdateTiming.LateUpdate)
		{
			UpdateRendererList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCull()
	{
		if (advancedSettings.updateTiming == UpdateTiming.OnPreCull)
		{
			UpdateRendererList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender()
	{
		if (advancedSettings.updateTiming == UpdateTiming.OnPreRender)
		{
			UpdateRendererList();
		}
		referenceCamera.nearClipPlane = (advancedSettings.enableNearClipOverride ? nearClipOverride : originalNearClip);
		Matrix4x4 projectionMatrix = referenceCamera.projectionMatrix;
		Matrix4x4 matrix4x = Matrix4x4.Perspective(fov, referenceCamera.aspect, referenceCamera.nearClipPlane * nearClipFactor, referenceCamera.farClipPlane * advancedSettings.farClipFactor);
		matrix4x[0, 2] += advancedSettings.jitterFactor * (projectionMatrix[0, 2] - matrix4x[0, 2]);
		matrix4x[1, 2] += advancedSettings.jitterFactor * (projectionMatrix[1, 2] - matrix4x[1, 2]);
		Matrix4x4 matrix4x2 = advancedSettings.projectionMode switch
		{
			ProjectionMode.Custom => matrix4x, 
			ProjectionMode.Reference => projectionMatrix, 
			ProjectionMode.ReferenceNonJittered => referenceCamera.nonJitteredProjectionMatrix, 
			_ => projectionMatrix, 
		};
		if (advancedSettings.depthScaleFactor != 1f)
		{
			Matrix4x4 identity = Matrix4x4.identity;
			identity.m22 = advancedSettings.depthScaleFactor;
			matrix4x2 = identity * matrix4x2;
		}
		matrix4x2 = GL.GetGPUProjectionMatrix(matrix4x2, renderIntoTexture: true);
		Matrix4x4 worldToCameraMatrix = referenceCamera.worldToCameraMatrix;
		Matrix4x4 value = matrix4x2 * worldToCameraMatrix;
		foreach (Renderer overriddenRenderer in overriddenRenderers)
		{
			if (!rendererSettingsMap.TryGetValue(overriddenRenderer, out var value2))
			{
				Debug.LogError("[CMO] Failed to retrieve RendererSettings for overridden renderer");
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
	public void RestoreChildSettings()
	{
		foreach (Renderer overriddenRenderer in overriddenRenderers)
		{
			if (overriddenRenderer != null && rendererSettingsMap.TryGetValue(overriddenRenderer, out var value))
			{
				if (value.shadowModeDirty)
				{
					overriddenRenderer.shadowCastingMode = value.originalShadowCastingMode;
					value.shadowModeDirty = false;
				}
				if (value.boundsDirty)
				{
					overriddenRenderer.ResetBounds();
					value.boundsDirty = false;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPostRender()
	{
		foreach (Renderer overriddenRenderer in overriddenRenderers)
		{
			if (!(overriddenRenderer == null) && rendererSettingsMap.TryGetValue(overriddenRenderer, out var value))
			{
				overriddenRenderer.SetPropertyBlock(value.originalProperties);
			}
		}
	}
}
