using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PI.NGSS;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
public class NGSS_FrustumShadows_7DTD : MonoBehaviour
{
	[Header("REFERENCES")]
	public Light mainShadowsLight;

	public Shader frustumShadowsShader;

	[Header("SHADOWS SETTINGS")]
	[Tooltip("Poisson Noise. Randomize samples to remove repeated patterns.")]
	public bool m_dithering;

	[Tooltip("If enabled a faster separable blur will be used.\nIf disabled a slower depth aware blur will be used.")]
	public bool m_fastBlur = true;

	[Tooltip("If enabled, backfaced lit fragments will be skipped increasing performance. Requires GBuffer normals.")]
	public bool m_deferredBackfaceOptimization;

	[Range(0f, 1f)]
	[Tooltip("Set how backfaced lit fragments are shaded. Requires DeferredBackfaceOptimization to be enabled.")]
	public float m_deferredBackfaceTranslucency;

	[Tooltip("Tweak this value to remove soft-shadows leaking around edges.")]
	[Range(0.01f, 1f)]
	public float m_shadowsEdgeBlur = 0.25f;

	[Tooltip("Overall softness of the shadows.")]
	[Range(0.01f, 1f)]
	public float m_shadowsBlur = 0.5f;

	[Tooltip("Overall softness of the shadows. Higher values than 1 wont work well if FastBlur is enabled.")]
	[Range(1f, 4f)]
	public int m_shadowsBlurIterations = 1;

	[Tooltip("Rising this value will make shadows more blurry but also lower in resolution.")]
	[Range(1f, 4f)]
	public int m_shadowsDownGrade = 1;

	[Tooltip("Tweak this value if your objects display backface shadows.")]
	[Range(0f, 1f)]
	public float m_shadowsBias = 0.05f;

	[Tooltip("The distance in metters from camera where shadows start to shown.")]
	public float m_shadowsDistanceStart;

	[Header("RAY SETTINGS")]
	[Tooltip("If enabled the ray length will be scaled at screen space instead of world space. Keep it enabled for an infinite view shadows coverage. Disable it for a ContactShadows like effect. Adjust the Ray Scale property accordingly.")]
	public bool m_rayScreenScale = true;

	[Tooltip("Number of samplers between each step. The higher values produces less gaps between shadows but is more costly.")]
	[Range(16f, 128f)]
	public int m_raySamples = 64;

	[Tooltip("The higher the value, the larger the shadows ray will be.")]
	[Range(0.01f, 1f)]
	public float m_rayScale = 0.25f;

	[Tooltip("The higher the value, the ticker the shadows will look.")]
	[Range(0f, 1f)]
	public float m_rayThickness = 0.01f;

	[Header("TEMPORAL SETTINGS")]
	[Tooltip("Enable this option if you use temporal anti-aliasing in your project. Works better when Dithering is enabled.")]
	public bool m_Temporal;

	[Range(0f, 1f)]
	public float m_JitterScale = 0.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_temporalJitter;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _iterations = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _downGrade = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _width;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _height;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public RenderingPath _currentRenderingPath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CommandBuffer computeShadowsCB;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _isInit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Camera _mCamera;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material _mMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh _fullScreenTriangle;

	public Camera mCamera
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (_mCamera == null)
			{
				_mCamera = GetComponent<Camera>();
				if (_mCamera == null)
				{
					_mCamera = Camera.main;
				}
				if (_mCamera == null)
				{
					Debug.LogError("NGSS Error: No MainCamera found, please provide one.", this);
					base.enabled = false;
				}
			}
			return _mCamera;
		}
	}

	public Material mMaterial
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (_mMaterial == null)
			{
				if (frustumShadowsShader == null)
				{
					frustumShadowsShader = Shader.Find("Hidden/NGSS_FrustumShadows");
				}
				_mMaterial = new Material(frustumShadowsShader);
				if (_mMaterial == null)
				{
					Debug.LogWarning("NGSS Warning: can't find NGSS_FrustumShadows shader, make sure it's on your project.", this);
					base.enabled = false;
				}
			}
			return _mMaterial;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			_mMaterial = value;
		}
	}

	public Mesh FullScreenTriangle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if ((bool)_fullScreenTriangle)
			{
				return _fullScreenTriangle;
			}
			_fullScreenTriangle = new Mesh
			{
				name = "Full-Screen Triangle",
				vertices = new Vector3[3]
				{
					new Vector3(-1f, -1f, 0f),
					new Vector3(-1f, 3f, 0f),
					new Vector3(3f, -1f, 0f)
				},
				triangles = new int[3] { 0, 1, 2 }
			};
			_fullScreenTriangle.UploadMeshData(markNoLongerReadable: true);
			return _fullScreenTriangle;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsNotSupported()
	{
		return SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddCommandBuffers()
	{
		if (computeShadowsCB == null)
		{
			computeShadowsCB = new CommandBuffer
			{
				name = "NGSS FrustumShadows: Compute"
			};
		}
		else
		{
			computeShadowsCB.Clear();
		}
		bool flag = true;
		if (!mCamera)
		{
			return;
		}
		CommandBuffer[] commandBuffers = mCamera.GetCommandBuffers((mCamera.actualRenderingPath != RenderingPath.DeferredShading) ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting);
		for (int i = 0; i < commandBuffers.Length; i++)
		{
			if (!(commandBuffers[i].name != computeShadowsCB.name))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			mCamera.AddCommandBuffer((mCamera.actualRenderingPath != RenderingPath.DeferredShading) ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting, computeShadowsCB);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveCommandBuffers()
	{
		_mMaterial = null;
		if ((bool)mCamera)
		{
			mCamera.RemoveCommandBuffer(CameraEvent.BeforeLighting, computeShadowsCB);
			mCamera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, computeShadowsCB);
		}
		_isInit = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		int scaledPixelWidth = mCamera.scaledPixelWidth;
		int scaledPixelHeight = mCamera.scaledPixelHeight;
		m_shadowsBlurIterations = (m_fastBlur ? 1 : m_shadowsBlurIterations);
		if (_iterations == m_shadowsBlurIterations && _downGrade == m_shadowsDownGrade && _width == scaledPixelWidth && _height == scaledPixelHeight && (_isInit || mainShadowsLight == null))
		{
			return;
		}
		if (mCamera.actualRenderingPath == RenderingPath.VertexLit)
		{
			Debug.LogWarning("Vertex Lit Rendering Path is not supported by NGSS Contact Shadows. Please set the Rendering Path in your game camera or Graphics Settings to something else than Vertex Lit.", this);
			base.enabled = false;
			return;
		}
		if (mCamera.actualRenderingPath == RenderingPath.Forward)
		{
			mCamera.depthTextureMode |= DepthTextureMode.Depth;
		}
		AddCommandBuffers();
		_width = scaledPixelWidth;
		_height = scaledPixelHeight;
		_downGrade = m_shadowsDownGrade;
		int num = Shader.PropertyToID("NGSS_ContactShadowRT1");
		int num2 = Shader.PropertyToID("NGSS_ContactShadowRT2");
		computeShadowsCB.GetTemporaryRT(num, scaledPixelWidth / _downGrade, scaledPixelHeight / _downGrade, 0, FilterMode.Bilinear, RenderTextureFormat.RG16);
		computeShadowsCB.GetTemporaryRT(num2, scaledPixelWidth / _downGrade, scaledPixelHeight / _downGrade, 0, FilterMode.Bilinear, RenderTextureFormat.RG16);
		computeShadowsCB.Blit(null, num, mMaterial, 0);
		_iterations = m_shadowsBlurIterations;
		for (int i = 1; i <= _iterations; i++)
		{
			computeShadowsCB.SetGlobalVector("ShadowsKernel", new Vector2(0f, i));
			computeShadowsCB.Blit(num, num2, mMaterial, 1);
			computeShadowsCB.SetGlobalVector("ShadowsKernel", new Vector2(i, 0f));
			computeShadowsCB.Blit(num2, num, mMaterial, 1);
		}
		computeShadowsCB.SetGlobalTexture("NGSS_FrustumShadowsTexture", num);
		computeShadowsCB.ReleaseTemporaryRT(num);
		computeShadowsCB.ReleaseTemporaryRT(num2);
		_isInit = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (IsNotSupported())
		{
			Debug.LogWarning("Unsupported graphics API, NGSS requires at least SM3.0 or higher and DX9 is not supported.", this);
			base.enabled = false;
		}
		else
		{
			Init();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		Shader.SetGlobalFloat("NGSS_FRUSTUM_SHADOWS_ENABLED", 0f);
		if (_isInit)
		{
			RemoveCommandBuffers();
		}
		if (mMaterial != null)
		{
			UnityEngine.Object.DestroyImmediate(mMaterial);
			mMaterial = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationQuit()
	{
		if (_isInit)
		{
			RemoveCommandBuffers();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender()
	{
		if (mainShadowsLight == null && SkyManager.SunLightT != null)
		{
			mainShadowsLight = SkyManager.SunLightT.GetComponent<Light>();
		}
		Init();
		if (_isInit && !(mainShadowsLight == null))
		{
			if (_currentRenderingPath != mCamera.actualRenderingPath)
			{
				_currentRenderingPath = mCamera.actualRenderingPath;
				RemoveCommandBuffers();
				AddCommandBuffers();
			}
			Shader.SetGlobalFloat("NGSS_FRUSTUM_SHADOWS_ENABLED", 1f);
			Shader.SetGlobalFloat("NGSS_FRUSTUM_SHADOWS_OPACITY", 1f - mainShadowsLight.shadowStrength);
			if (m_Temporal)
			{
				m_temporalJitter = (m_temporalJitter + 1) % 8;
				mMaterial.SetFloat("TemporalJitter", (float)m_temporalJitter * m_JitterScale * 0.0002f);
			}
			else
			{
				mMaterial.SetFloat("TemporalJitter", 0f);
			}
			if (QualitySettings.shadowProjection == ShadowProjection.StableFit)
			{
				mMaterial.EnableKeyword("SHADOWS_SPLIT_SPHERES");
			}
			else
			{
				mMaterial.DisableKeyword("SHADOWS_SPLIT_SPHERES");
			}
			mMaterial.SetMatrix("WorldToView", mCamera.worldToCameraMatrix);
			mMaterial.SetVector("LightDir", mCamera.transform.InverseTransformDirection(-mainShadowsLight.transform.forward));
			mMaterial.SetVector("LightPosRange", new Vector4(mainShadowsLight.transform.position.x, mainShadowsLight.transform.position.y, mainShadowsLight.transform.position.z, mainShadowsLight.range * mainShadowsLight.range));
			mMaterial.SetVector("LightDirWorld", -mainShadowsLight.transform.forward);
			mMaterial.SetFloat("ShadowsEdgeTolerance", m_shadowsEdgeBlur);
			mMaterial.SetFloat("ShadowsSoftness", m_shadowsBlur);
			mMaterial.SetFloat("RayScale", m_rayScale);
			mMaterial.SetFloat("ShadowsBias", m_shadowsBias * 0.02f);
			mMaterial.SetFloat("ShadowsDistanceStart", m_shadowsDistanceStart - 10f);
			mMaterial.SetFloat("RayThickness", m_rayThickness);
			mMaterial.SetFloat("RaySamples", m_raySamples);
			if (m_deferredBackfaceOptimization && mCamera.actualRenderingPath == RenderingPath.DeferredShading)
			{
				mMaterial.EnableKeyword("NGSS_DEFERRED_OPTIMIZATION");
				mMaterial.SetFloat("BackfaceOpacity", m_deferredBackfaceTranslucency);
			}
			else
			{
				mMaterial.DisableKeyword("NGSS_DEFERRED_OPTIMIZATION");
			}
			if (m_dithering)
			{
				mMaterial.EnableKeyword("NGSS_USE_DITHERING");
			}
			else
			{
				mMaterial.DisableKeyword("NGSS_USE_DITHERING");
			}
			if (m_fastBlur)
			{
				mMaterial.EnableKeyword("NGSS_FAST_BLUR");
			}
			else
			{
				mMaterial.DisableKeyword("NGSS_FAST_BLUR");
			}
			if (mainShadowsLight.type != LightType.Directional)
			{
				mMaterial.EnableKeyword("NGSS_USE_LOCAL_SHADOWS");
			}
			else
			{
				mMaterial.DisableKeyword("NGSS_USE_LOCAL_SHADOWS");
			}
			mMaterial.SetFloat("RayScreenScale", m_rayScreenScale ? 1f : 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPostRender()
	{
		Shader.SetGlobalFloat("NGSS_FRUSTUM_SHADOWS_ENABLED", 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlitXR(CommandBuffer cmd, RenderTargetIdentifier src, RenderTargetIdentifier dest, Material mat, int pass)
	{
		cmd.SetRenderTarget(dest, 0, CubemapFace.Unknown, -1);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		cmd.DrawMesh(FullScreenTriangle, Matrix4x4.identity, mat, pass);
	}
}
