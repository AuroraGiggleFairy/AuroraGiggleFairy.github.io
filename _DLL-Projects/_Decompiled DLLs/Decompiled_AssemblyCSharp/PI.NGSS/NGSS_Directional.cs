using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PI.NGSS;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class NGSS_Directional : MonoBehaviour
{
	public enum ShadowMapResolution
	{
		UseQualitySettings = 0x100,
		VeryLow = 0x200,
		Low = 0x400,
		Med = 0x800,
		High = 0x1000,
		Ultra = 0x2000,
		Mega = 0x4000
	}

	[Header("MAIN SETTINGS")]
	public Shader denoiserShader;

	[Tooltip("If disabled, NGSS Directional shadows replacement will be removed from Graphics settings when OnDisable is called in this component.")]
	public bool NGSS_KEEP_ONDISABLE = true;

	[Tooltip("Check this option if you don't need to update shadows variables at runtime, only once when scene loads.\nUseful to save some CPU cycles.")]
	public bool NGSS_NO_UPDATE_ON_PLAY;

	[Tooltip("Shadows resolution.\nUseQualitySettings = From Quality Settings, SuperLow = 512, Low = 1024, Med = 2048, High = 4096, Ultra = 8192, Mega = 16384.")]
	public ShadowMapResolution NGSS_SHADOWS_RESOLUTION = ShadowMapResolution.UseQualitySettings;

	[Header("BASE SAMPLING")]
	[Tooltip("Used to test blocker search and early bail out algorithms. Keep it as low as possible, might lead to white noise if too low.\nRecommended values: Mobile = 8, Consoles & VR = 16, Desktop = 24")]
	[Range(4f, 32f)]
	public int NGSS_SAMPLING_TEST = 16;

	[Tooltip("Number of samplers per pixel used for PCF and PCSS shadows algorithms.\nRecommended values: Mobile = 16, Consoles & VR = 32, Desktop Med = 48, Desktop High = 64, Desktop Ultra = 128")]
	[Range(8f, 128f)]
	public int NGSS_SAMPLING_FILTER = 48;

	[Tooltip("New optimization that reduces sampling over distance. Interpolates current sampling set (TEST and FILTER) down to 4spp when reaching this distance.")]
	[Range(0f, 500f)]
	public float NGSS_SAMPLING_DISTANCE = 75f;

	[Header("SHADOW SOFTNESS")]
	[Tooltip("Overall shadows softness.")]
	[Range(0f, 3f)]
	public float NGSS_SHADOWS_SOFTNESS = 1f;

	[Header("PCSS")]
	[Tooltip("PCSS Requires inline sampling and SM3.5.\nProvides Area Light soft-shadows.\nDisable it if you are looking for PCF filtering (uniform soft-shadows) which runs with SM3.0.")]
	public bool NGSS_PCSS_ENABLED;

	[Tooltip("How soft shadows are when close to caster.")]
	[Range(0f, 2f)]
	public float NGSS_PCSS_SOFTNESS_NEAR = 0.125f;

	[Tooltip("How soft shadows are when far from caster.")]
	[Range(0f, 2f)]
	public float NGSS_PCSS_SOFTNESS_FAR = 1f;

	[Header("NOISE")]
	[Tooltip("If zero = 100% noise.\nIf one = 100% dithering.\nUseful when fighting banding.")]
	[Range(0f, 1f)]
	public int NGSS_NOISE_TO_DITHERING_SCALE;

	[Tooltip("If you set the noise scale value to something less than 1 you need to input a noise texture.\nRecommended noise textures are blue noise signals.")]
	public Texture2D NGSS_NOISE_TEXTURE;

	[Header("DENOISER")]
	[Tooltip("Separable low pass filter that help fight artifacts and noise in shadows.\nRequires Cascaded Shadows to be enabled in the Editor Graphics Settings.")]
	public bool NGSS_DENOISER_ENABLED = true;

	[Tooltip("How many iterations the Denoiser algorithm should do.")]
	[Range(1f, 4f)]
	public int NGSS_DENOISER_PASSES = 1;

	[Tooltip("Overall Denoiser softness.")]
	[Range(0.01f, 1f)]
	public float NGSS_DENOISER_SOFTNESS = 1f;

	[Tooltip("The amount of shadow edges the Denoiser can tolerate during denoising.")]
	[Range(0.01f, 1f)]
	public float NGSS_DENOISER_EDGE_SOFTNESS = 1f;

	[Header("BIAS")]
	[Tooltip("This estimates receiver slope using derivatives and tries to tilt the filtering kernel along it.\nHowever, when doing it in screenspace from the depth texture can leads to shadow artifacts.\nThus it is disabled by default.")]
	public bool NGSS_RECEIVER_PLANE_BIAS;

	[Header("CASCADES")]
	[Tooltip("Blends cascades at seams intersection.\nAdditional overhead required for this option.")]
	public bool NGSS_CASCADES_BLENDING = true;

	[Tooltip("Tweak this value to adjust the blending transition between cascades.")]
	[Range(0f, 2f)]
	public float NGSS_CASCADES_BLENDING_VALUE = 1f;

	[Range(0f, 1f)]
	[Tooltip("If one, softness across cascades will be matched using splits distribution, resulting in realistic soft-ness over distance.\nIf zero the softness distribution will be based on cascade index, resulting in blurrier shadows over distance thus less realistic.")]
	public float NGSS_CASCADES_SOFTNESS_NORMALIZATION = 1f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssDirSamplingDistanceid = Shader.PropertyToID("NGSS_DIR_SAMPLING_DISTANCE");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssTestSamplersDirid = Shader.PropertyToID("NGSS_TEST_SAMPLERS_DIR");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssFilterSamplersDirid = Shader.PropertyToID("NGSS_FILTER_SAMPLERS_DIR");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssGlobalSoftnessid = Shader.PropertyToID("NGSS_GLOBAL_SOFTNESS");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssGlobalSoftnessOptimizedid = Shader.PropertyToID("NGSS_GLOBAL_SOFTNESS_OPTIMIZED");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssOptimizedIterationsid = Shader.PropertyToID("NGSS_OPTIMIZED_ITERATIONS");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssOptimizedSamplersid = Shader.PropertyToID("NGSS_OPTIMIZED_SAMPLERS");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssNoiseToDitheringScaleDirid = Shader.PropertyToID("NGSS_NOISE_TO_DITHERING_SCALE_DIR");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssPcssFilterDirMinid = Shader.PropertyToID("NGSS_PCSS_FILTER_DIR_MIN");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssPcssFilterDirMaxid = Shader.PropertyToID("NGSS_PCSS_FILTER_DIR_MAX");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssCascadesSoftnessNormalizationid = Shader.PropertyToID("NGSS_CASCADES_SOFTNESS_NORMALIZATION");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssCascadesCountid = Shader.PropertyToID("NGSS_CASCADES_COUNT");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssCascadesSplitsid = Shader.PropertyToID("NGSS_CASCADES_SPLITS");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _ngssCascadeBlendDistanceid = Shader.PropertyToID("NGSS_CASCADE_BLEND_DISTANCE");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _denoiseKernelID = Shader.PropertyToID("DenoiseKernel");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _denoiseSoftnessID = Shader.PropertyToID("DenoiseSoftness");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _denoiseTextureID = Shader.PropertyToID("NGSS_DenoiseTexture");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _denoiseEdgeToleranceID = Shader.PropertyToID("DenoiseEdgeTolerance");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CommandBuffer _computeDenoiseCB;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CommandBuffer _blendDenoiseCB;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _DENOISER_ENABLED;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int _DENOISER_PASSES = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSetup;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInitialized;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGraphicSet;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light _DirLight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material _mMaterial;

	public Light DirLight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (_DirLight == null)
			{
				_DirLight = GetComponent<Light>();
			}
			return _DirLight;
		}
	}

	public Material mMaterial
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if ((bool)_mMaterial)
			{
				return _mMaterial;
			}
			if (denoiserShader == null)
			{
				denoiserShader = Shader.Find("Hidden/NGSS_DenoiseShader");
			}
			_mMaterial = new Material(denoiserShader);
			if ((bool)_mMaterial)
			{
				return _mMaterial;
			}
			Debug.LogWarning("NGSS Warning: can't find NGSS_DenoiseShader shader, make sure it's on your project.", this);
			base.enabled = false;
			return _mMaterial;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			_mMaterial = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		isInitialized = false;
		if (!NGSS_KEEP_ONDISABLE)
		{
			if (isGraphicSet)
			{
				isGraphicSet = false;
				GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/Internal-ScreenSpaceShadows"));
				GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseBuiltin);
			}
			if ((bool)mMaterial)
			{
				UnityEngine.Object.DestroyImmediate(mMaterial);
				mMaterial = null;
			}
			RemoveCommandBuffer();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveCommandBuffer()
	{
		if (_computeDenoiseCB == null || _DirLight == null)
		{
			return;
		}
		CommandBuffer[] commandBuffers = _DirLight.GetCommandBuffers(LightEvent.AfterScreenspaceMask);
		foreach (CommandBuffer commandBuffer in commandBuffers)
		{
			if (!(commandBuffer.name != _computeDenoiseCB.name))
			{
				_DirLight.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer);
				break;
			}
		}
		_computeDenoiseCB = null;
		commandBuffers = _DirLight.GetCommandBuffers(LightEvent.AfterScreenspaceMask);
		foreach (CommandBuffer commandBuffer2 in commandBuffers)
		{
			if (!(commandBuffer2.name != _blendDenoiseCB.name))
			{
				_DirLight.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer2);
				break;
			}
		}
		_blendDenoiseCB = null;
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
	public void Init()
	{
		if (!isInitialized)
		{
			if (!isGraphicSet)
			{
				GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseCustom);
				GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/NGSS_Directional"));
				isGraphicSet = true;
			}
			if (NGSS_NOISE_TEXTURE == null)
			{
				NGSS_NOISE_TEXTURE = Resources.Load<Texture2D>("BlueNoise_R8_8");
			}
			Shader.SetGlobalTexture("_BlueNoiseTextureDir", NGSS_NOISE_TEXTURE);
			bool flag = false;
			if (NGSS_DENOISER_ENABLED && !flag)
			{
				AddCommandBuffer();
			}
			isInitialized = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddCommandBuffer()
	{
		if (_DirLight == null)
		{
			return;
		}
		_computeDenoiseCB = new CommandBuffer();
		_blendDenoiseCB = new CommandBuffer();
		_computeDenoiseCB.name = "NGSS_Directional Denoiser Computation";
		_blendDenoiseCB.name = "NGSS_Directional Denoiser Blending";
		int num = Shader.PropertyToID("NGSS_DenoiseTexture1");
		int num2 = Shader.PropertyToID("NGSS_DenoiseTexture2");
		_blendDenoiseCB.Blit(BuiltinRenderTextureType.None, BuiltinRenderTextureType.CurrentActive, mMaterial, 1);
		_computeDenoiseCB.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		_computeDenoiseCB.GetTemporaryRT(num2, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		for (int i = 1; i <= NGSS_DENOISER_PASSES; i++)
		{
			_computeDenoiseCB.SetGlobalVector(_denoiseKernelID, new Vector2(0f, 1f));
			if (i == 1)
			{
				_computeDenoiseCB.Blit(BuiltinRenderTextureType.CurrentActive, num2, mMaterial, 0);
			}
			else
			{
				_computeDenoiseCB.Blit(num, num2, mMaterial, 0);
			}
			_computeDenoiseCB.SetGlobalVector(_denoiseKernelID, new Vector2(1f, 0f));
			_computeDenoiseCB.Blit(num2, num, mMaterial, 0);
		}
		_computeDenoiseCB.SetGlobalTexture(_denoiseTextureID, num);
		_computeDenoiseCB.ReleaseTemporaryRT(num);
		_computeDenoiseCB.ReleaseTemporaryRT(num2);
		mMaterial.SetFloat(_denoiseEdgeToleranceID, NGSS_DENOISER_EDGE_SOFTNESS);
		mMaterial.SetFloat(_denoiseSoftnessID, NGSS_DENOISER_SOFTNESS);
		bool flag = true;
		CommandBuffer[] commandBuffers = _DirLight.GetCommandBuffers(LightEvent.AfterScreenspaceMask);
		for (int j = 0; j < commandBuffers.Length; j++)
		{
			if (!(commandBuffers[j].name != _computeDenoiseCB.name))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			_DirLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, _computeDenoiseCB);
		}
		flag = true;
		commandBuffers = _DirLight.GetCommandBuffers(LightEvent.AfterScreenspaceMask);
		for (int j = 0; j < commandBuffers.Length; j++)
		{
			if (!(commandBuffers[j].name != _blendDenoiseCB.name))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			_DirLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, _blendDenoiseCB);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsNotSupported()
	{
		return SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if ((Application.isPlaying && NGSS_NO_UPDATE_ON_PLAY && isSetup) || DirLight.shadows == LightShadows.None || DirLight.type != LightType.Directional)
		{
			return;
		}
		Shader.SetGlobalFloat(_ngssDirSamplingDistanceid, NGSS_SAMPLING_DISTANCE);
		NGSS_SAMPLING_TEST = Mathf.Clamp(NGSS_SAMPLING_TEST, 4, NGSS_SAMPLING_FILTER);
		Shader.SetGlobalFloat(_ngssTestSamplersDirid, NGSS_SAMPLING_TEST);
		Shader.SetGlobalFloat(_ngssFilterSamplersDirid, NGSS_SAMPLING_FILTER);
		Shader.SetGlobalFloat(_ngssGlobalSoftnessid, (QualitySettings.shadowProjection == ShadowProjection.CloseFit) ? NGSS_SHADOWS_SOFTNESS : (NGSS_SHADOWS_SOFTNESS * 2f / (QualitySettings.shadowDistance * 0.66f) * ((QualitySettings.shadowCascades == 2) ? 1.5f : ((QualitySettings.shadowCascades == 4) ? 1f : 0.25f))));
		Shader.SetGlobalFloat(_ngssGlobalSoftnessOptimizedid, NGSS_SHADOWS_SOFTNESS / QualitySettings.shadowDistance);
		int num = (int)Mathf.Sqrt(NGSS_SAMPLING_FILTER);
		Shader.SetGlobalInt(_ngssOptimizedIterationsid, (num % 2 == 0) ? (num + 1) : num);
		Shader.SetGlobalInt(_ngssOptimizedSamplersid, NGSS_SAMPLING_FILTER);
		if (_DENOISER_ENABLED != NGSS_DENOISER_ENABLED)
		{
			_DENOISER_ENABLED = NGSS_DENOISER_ENABLED;
			RemoveCommandBuffer();
			if (NGSS_DENOISER_ENABLED)
			{
				AddCommandBuffer();
			}
		}
		if (_DENOISER_PASSES != NGSS_DENOISER_PASSES)
		{
			_DENOISER_PASSES = NGSS_DENOISER_PASSES;
			RemoveCommandBuffer();
			if (NGSS_DENOISER_ENABLED)
			{
				AddCommandBuffer();
			}
		}
		if (NGSS_DENOISER_ENABLED)
		{
			mMaterial.SetFloat(_denoiseEdgeToleranceID, NGSS_DENOISER_EDGE_SOFTNESS);
			mMaterial.SetFloat(_denoiseSoftnessID, NGSS_DENOISER_SOFTNESS);
		}
		if (NGSS_RECEIVER_PLANE_BIAS)
		{
			Shader.EnableKeyword("NGSS_USE_RECEIVER_PLANE_BIAS");
		}
		else
		{
			Shader.DisableKeyword("NGSS_USE_RECEIVER_PLANE_BIAS");
		}
		Shader.SetGlobalFloat(_ngssNoiseToDitheringScaleDirid, NGSS_NOISE_TO_DITHERING_SCALE);
		if (NGSS_PCSS_ENABLED)
		{
			float num2 = NGSS_PCSS_SOFTNESS_NEAR * 0.25f;
			float num3 = NGSS_PCSS_SOFTNESS_FAR * 0.25f;
			Shader.SetGlobalFloat(_ngssPcssFilterDirMinid, (num2 > num3) ? num3 : num2);
			Shader.SetGlobalFloat(_ngssPcssFilterDirMaxid, (num3 < num2) ? num2 : num3);
			Shader.EnableKeyword("NGSS_PCSS_FILTER_DIR");
		}
		else
		{
			Shader.DisableKeyword("NGSS_PCSS_FILTER_DIR");
		}
		if (NGSS_SHADOWS_RESOLUTION != ShadowMapResolution.UseQualitySettings)
		{
			DirLight.shadowCustomResolution = (int)NGSS_SHADOWS_RESOLUTION;
		}
		else
		{
			DirLight.shadowCustomResolution = 0;
			DirLight.shadowResolution = LightShadowResolution.FromQualitySettings;
		}
		if (QualitySettings.shadowCascades > 1)
		{
			Shader.SetGlobalFloat(_ngssCascadesSoftnessNormalizationid, NGSS_CASCADES_SOFTNESS_NORMALIZATION);
			Shader.SetGlobalFloat(_ngssCascadesCountid, QualitySettings.shadowCascades);
			Shader.SetGlobalVector(_ngssCascadesSplitsid, (QualitySettings.shadowCascades == 2) ? new Vector4(QualitySettings.shadowCascade2Split, 1f, 1f, 1f) : new Vector4(QualitySettings.shadowCascade4Split.x, QualitySettings.shadowCascade4Split.y, QualitySettings.shadowCascade4Split.z, 1f));
		}
		if (NGSS_CASCADES_BLENDING && QualitySettings.shadowCascades > 1)
		{
			Shader.EnableKeyword("NGSS_USE_CASCADE_BLENDING");
			Shader.SetGlobalFloat(_ngssCascadeBlendDistanceid, NGSS_CASCADES_BLENDING_VALUE * 0.125f);
		}
		else
		{
			Shader.DisableKeyword("NGSS_USE_CASCADE_BLENDING");
		}
		isSetup = true;
	}
}
