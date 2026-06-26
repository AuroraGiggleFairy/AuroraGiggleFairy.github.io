using System.Collections.Generic;
using FidelityFX.FSR3;
using HorizonBasedAmbientOcclusion;
using PI.NGSS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class GameRenderManager
{
	public class FSRCallback : IFsr3UpscalerCallbacks
	{
		public virtual void ApplyMipmapBias(float biasOffset)
		{
		}

		public virtual void UndoMipmapBias()
		{
		}
	}

	public GameGraphManager graphManager;

	public GameLightManager lightManager;

	public ReflectionManager reflectionManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicUpdateDelay = 18f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicChangeSeconds = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicFPSMin = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicFPSMax = 64f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicFPSVSyncMin = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicFPSVSyncMax = 55f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicScaleMin = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDynamicScaleThreshold = 0.049f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDynamicRTCount = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float[] dynamicScales = new float[5] { 1f, 0.75f, 0.62f, 0.5f, 0f };

	public static bool dynamicIsEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicUpdateDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicFPSTargetMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicFPSTargetMax = 64f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicFPS = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicScaleTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dynamicScaleOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dynamicScreenW;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture dynamicRT;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture[] dynamicRTs;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fsrEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public SuperResolution fsr;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dlssEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float mipmapDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong mipmapTextureMem;

	public static int TextureMipmapLimit
	{
		get
		{
			return QualitySettings.globalTextureMipmapLimit;
		}
		set
		{
			QualitySettings.globalTextureMipmapLimit = value;
		}
	}

	public static GameRenderManager Create(EntityPlayerLocal player)
	{
		GameRenderManager gameRenderManager = new GameRenderManager();
		gameRenderManager.player = player;
		gameRenderManager.Init();
		return gameRenderManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		graphManager = GameGraphManager.Create(player);
		lightManager = GameLightManager.Create(player);
		reflectionManager = ReflectionManager.Create(player);
		PostProcessInit();
		DynamicResolutionInit();
	}

	public void Destroy()
	{
		lightManager.Destroy();
		lightManager = null;
		reflectionManager.Destroy();
		reflectionManager = null;
		DynamicResolutionDestroyRT();
	}

	public void FrameUpdate()
	{
		lightManager.FrameUpdate();
		reflectionManager.FrameUpdate();
		DynamicResolutionUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PostProcessInit()
	{
		_ = player.playerCamera;
	}

	public static void ApplyCameraOptions(EntityPlayerLocal player)
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if ((bool)player)
		{
			player.renderManager.ApplyCameraOptions();
			return;
		}
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		for (int i = 0; i < localPlayers.Count; i++)
		{
			localPlayers[i].renderManager.ApplyCameraOptions();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyCameraOptions()
	{
		Camera playerCamera = player.playerCamera;
		PostProcessLayer component = playerCamera.GetComponent<PostProcessLayer>();
		playerCamera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
		NGSS_FrustumShadows_7DTD component2 = playerCamera.GetComponent<NGSS_FrustumShadows_7DTD>();
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality))
		{
		case 0:
		case 1:
		case 2:
			component2.enabled = false;
			break;
		case 3:
			component2.enabled = true;
			component2.m_shadowsBlurIterations = 1;
			component2.m_raySamples = 32;
			break;
		case 4:
			component2.enabled = true;
			component2.m_shadowsBlurIterations = 2;
			component2.m_raySamples = 48;
			break;
		case 5:
			component2.enabled = true;
			component2.m_shadowsBlurIterations = 4;
			component2.m_raySamples = 64;
			break;
		}
		int aaQuality = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxAA);
		float sharpness = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxAASharpness);
		bool x = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxBloom);
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
		bool enabled = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSSAO);
		bool x2 = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSunShafts);
		int num2 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxMotionBlur);
		if (!GamePrefs.GetBool(EnumGamePrefs.OptionsGfxMotionBlurEnabled))
		{
			num2 = 0;
		}
		PostProcessVolume component3 = playerCamera.GetComponent<PostProcessVolume>();
		if ((bool)component3)
		{
			PostProcessProfile profile = component3.profile;
			if ((bool)profile)
			{
				component3.enabled = false;
				ScreenSpaceReflections setting = profile.GetSetting<ScreenSpaceReflections>();
				if ((bool)setting)
				{
					switch (num)
					{
					case 1:
						setting.maximumIterationCount.Override(200);
						setting.resolution.Override(ScreenSpaceReflectionResolution.Downsampled);
						break;
					case 2:
						setting.maximumIterationCount.Override(120);
						setting.resolution.Override(ScreenSpaceReflectionResolution.FullSize);
						break;
					case 3:
						setting.maximumIterationCount.Override(250);
						setting.resolution.Override(ScreenSpaceReflectionResolution.FullSize);
						break;
					}
					setting.enabled.Override(num > 0);
				}
				MotionBlur setting2 = profile.GetSetting<MotionBlur>();
				setting2.enabled.Override(num2 != 0);
				switch (num2)
				{
				case 1:
					setting2.shutterAngle.Override(135f);
					setting2.sampleCount.Override(5);
					break;
				case 2:
					setting2.shutterAngle.Override(270f);
					setting2.sampleCount.Override(10);
					break;
				}
				profile.GetSetting<Bloom>().enabled.Override(x);
				ColorGrading setting3 = profile.GetSetting<ColorGrading>();
				float num3 = 0.5f - GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
				num3 = ((!(num3 < 0f)) ? 0f : (num3 * 0.4f));
				setting3.toneCurveGamma.Override(1f + num3);
				if (profile.TryGetSettings<SunShaftsEffect>(out var outSetting))
				{
					outSetting.enabled.Override(x2);
				}
				component3.enabled = true;
			}
		}
		HBAO component4 = playerCamera.GetComponent<HBAO>();
		if ((bool)component4)
		{
			switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset))
			{
			case 0:
			case 1:
			case 2:
				component4.SetQuality(HBAO.Quality.Low);
				break;
			case 3:
				component4.SetQuality(HBAO.Quality.Medium);
				break;
			case 4:
				component4.SetQuality(HBAO.Quality.High);
				break;
			}
			component4.enabled = enabled;
		}
		int num4 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMode);
		int num5 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMinFPS);
		float scale = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxDynamicScale);
		if (num4 == 0)
		{
			scale = -1f;
		}
		if (num4 == 1)
		{
			scale = 0f;
		}
		SetDynamicResolution(scale, num5, -1f);
		if (dlssEnabled)
		{
			aaQuality = 0;
		}
		FSRInit(component.superResolution);
		if ((bool)component)
		{
			SetAntialiasing(aaQuality, sharpness, component);
			Rect rect = playerCamera.rect;
			rect.x = ((component.antialiasingMode == PostProcessLayer.Antialiasing.SuperResolution) ? 1E-07f : 0f);
			playerCamera.rect = rect;
		}
		reflectionManager.ApplyCameraOptions(playerCamera);
	}

	public void SetAntialiasing(int aaQuality, float sharpness, PostProcessLayer mainLayer)
	{
		if (aaQuality == 0)
		{
			mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
			FSRSetQuality(-1);
			return;
		}
		if (aaQuality <= 3)
		{
			switch (aaQuality)
			{
			case 1:
				mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
				mainLayer.fastApproximateAntialiasing.fastMode = false;
				break;
			case 2:
				mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
				mainLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Medium;
				break;
			default:
				mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
				mainLayer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.High;
				break;
			}
		}
		else if (aaQuality == 4 || !mainLayer.superResolution.IsSupported())
		{
			mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
			mainLayer.temporalAntialiasing.jitterSpread = 0.35f;
			mainLayer.temporalAntialiasing.stationaryBlending = 0.8f;
			mainLayer.temporalAntialiasing.motionBlending = 0.75f;
			mainLayer.temporalAntialiasing.sharpness = sharpness * 0.2f;
		}
		else
		{
			mainLayer.antialiasingMode = PostProcessLayer.Antialiasing.SuperResolution;
			mainLayer.superResolution.sharpness = sharpness;
		}
		FSRSetQuality(aaQuality - 5);
	}

	public void DynamicResolutionInit()
	{
		DynamicResolutionDestroyRT();
		Camera playerCamera = player.playerCamera;
		Camera finalCamera = player.finalCamera;
		bool flag = finalCamera != playerCamera;
		if (!dynamicIsEnabled)
		{
			if (flag)
			{
				Object.Destroy(finalCamera.gameObject);
			}
			player.finalCamera = playerCamera;
		}
		else
		{
			if (!flag)
			{
				AddFinalCameraToPlayer();
			}
			DynamicResolutionAllocRTs();
		}
		DLSSInit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddFinalCameraToPlayer()
	{
		GameObject gameObject = new GameObject("FinalCamera");
		gameObject.transform.SetParent(player.cameraTransform, worldPositionStays: false);
		Camera camera = gameObject.AddComponent<Camera>();
		player.finalCamera = camera;
		camera.clearFlags = CameraClearFlags.Nothing;
		camera.cullingMask = 0;
		camera.depth = -0.1f;
		gameObject.AddComponent<LocalPlayerFinalCamera>().entityPlayerLocal = player;
	}

	public void DynamicResolutionUpdate()
	{
		if (GameManager.Instance.World == null || !dynamicIsEnabled)
		{
			return;
		}
		if (Screen.width != dynamicScreenW)
		{
			DynamicResolutionAllocRTs();
		}
		else
		{
			if (dynamicScaleOverride > 0f)
			{
				return;
			}
			if (dynamicUpdateDelay > 0f)
			{
				dynamicUpdateDelay -= Time.deltaTime;
				return;
			}
			float num = Time.deltaTime + 0.001f;
			dynamicFPS = dynamicFPS * 0.5f + 0.5f / num;
			float num2 = 0.1f * num;
			if (dynamicFPS < dynamicFPSTargetMin)
			{
				dynamicScaleTarget -= num2;
				if (dynamicScaleTarget < 0.4f)
				{
					dynamicScaleTarget = 0.4f;
				}
			}
			else
			{
				dynamicScaleTarget += num2 * 0.2f;
				if (dynamicFPS > dynamicFPSTargetMax)
				{
					dynamicScaleTarget += num2;
				}
				if (dynamicScaleTarget > 1f)
				{
					dynamicScaleTarget = 1f;
				}
			}
			if (!(dynamicScaleTarget >= 1f) || !(dynamicScale < 1f))
			{
				float num3 = dynamicScaleTarget - dynamicScale;
				if (num3 > -0.049f && num3 < 0.049f)
				{
					return;
				}
			}
			dynamicScale = dynamicScaleTarget;
			RenderTexture renderTexture = null;
			for (int i = 0; i < dynamicRTs.Length; i++)
			{
				renderTexture = dynamicRTs[i];
				float num4 = (dynamicScales[i] + dynamicScales[i + 1]) * 0.5f;
				if (dynamicScale >= num4)
				{
					break;
				}
			}
			if (!(dynamicRT == renderTexture))
			{
				dynamicRT = renderTexture;
			}
		}
	}

	public bool DynamicResolutionUpdateGraph(ref float value)
	{
		if (dynamicRT != null)
		{
			float num = (float)dynamicRT.width / (float)Screen.width;
			if (num != value)
			{
				value = num;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DynamicResolutionAllocRTs()
	{
		DynamicResolutionDestroyRT();
		dynamicScreenW = Screen.width;
		int num = ((dynamicScaleOverride > 0f) ? 1 : 4);
		dynamicRTs = new RenderTexture[num];
		for (int i = 0; i < num; i++)
		{
			float scale = dynamicScales[i];
			if (dynamicScaleOverride > 0f)
			{
				scale = dynamicScaleOverride;
			}
			RenderTexture renderTexture = DynamicResolutionAllocRT(scale);
			dynamicRTs[i] = renderTexture;
		}
		dynamicRT = dynamicRTs[0];
		dynamicScale = 1f;
		dynamicScaleTarget = 1f;
		dynamicUpdateDelay = 18f;
	}

	public RenderTexture DynamicResolutionAllocRT(float scale)
	{
		int num = (int)((float)Screen.width * scale);
		int num2 = (int)((float)Screen.height * scale);
		RenderTexture renderTexture = new RenderTexture(num, num2, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		renderTexture.name = $"DynRT{num}x{num2}";
		Log.Out("DynamicResolutionAllocRT scale {0}, Tex {1}x{2}", scale, num, num2);
		return renderTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DynamicResolutionDestroyRT()
	{
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		for (int i = 0; i < localPlayers.Count; i++)
		{
			localPlayers[i].playerCamera.targetTexture = null;
		}
		if (dynamicRTs != null)
		{
			for (int j = 0; j < dynamicRTs.Length; j++)
			{
				dynamicRTs[j].Release();
				Object.Destroy(dynamicRTs[j]);
			}
			dynamicRTs = null;
		}
		dynamicRT = null;
	}

	public void SetDynamicResolution(float scale, float fpsMin, float fpsMax)
	{
		dynamicIsEnabled = scale >= 0f;
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxVsync);
		dynamicFPSTargetMin = fpsMin;
		if (fpsMin < 0f)
		{
			dynamicFPSTargetMin = 30f;
		}
		if (num > 0)
		{
			dynamicFPSTargetMin = Utils.FastMin(30f, dynamicFPSTargetMin);
		}
		if (num > 1)
		{
			dynamicFPSTargetMin = Utils.FastMin(18f, dynamicFPSTargetMin);
		}
		dynamicFPSTargetMax = fpsMax;
		if (fpsMax < 0f)
		{
			dynamicFPSTargetMax = 64f;
			if (num > 0)
			{
				dynamicFPSTargetMax = 55f;
			}
			if (num > 1)
			{
				dynamicFPSTargetMax = 25f;
			}
		}
		dynamicScaleOverride = scale;
		if (dynamicScaleOverride > 0f)
		{
			dynamicScaleOverride = Mathf.Clamp(dynamicScaleOverride, 0.1f, 1f);
			dynamicScale = dynamicScaleOverride;
		}
		DynamicResolutionInit();
	}

	public RenderTexture GetDynamicRenderTexture()
	{
		return dynamicRT;
	}

	public void DynamicResolutionRender()
	{
		if (!dlssEnabled)
		{
			Graphics.Blit((Texture)GetDynamicRenderTexture(), (RenderTexture)null);
		}
		else
		{
			DLSSRender();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FSRInit(SuperResolution _sr)
	{
		fsr = _sr;
		fsr.callbacksFactory = [PublicizedFrom(EAccessModifier.Internal)] (PostProcessRenderContext context) => new FSRCallback();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FSRSetQuality(int _quality)
	{
		if (_quality < 0)
		{
			if (fsrEnabled)
			{
				fsrEnabled = false;
				SetMipmapBias(0f);
			}
			return;
		}
		fsrEnabled = true;
		mipmapTextureMem = 0uL;
		SuperResolution superResolution = fsr;
		superResolution.qualityMode = _quality switch
		{
			0 => Fsr3Upscaler.QualityMode.UltraPerformance, 
			1 => Fsr3Upscaler.QualityMode.Performance, 
			2 => Fsr3Upscaler.QualityMode.Balanced, 
			3 => Fsr3Upscaler.QualityMode.Quality, 
			4 => Fsr3Upscaler.QualityMode.UltraQuality, 
			_ => Fsr3Upscaler.QualityMode.NativeAA, 
		};
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceVendor.ToLower().Contains("nvidia"))
		{
			fsr.exposureSource = SuperResolution.ExposureSource.Default;
		}
	}

	public void FSRPreCull()
	{
		if (fsrEnabled)
		{
			UpdateMipmaps((float)fsr.renderSize.x / (float)Screen.width);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DLSSInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsDLSSSupported()
	{
		return false;
	}

	public void DLSSPreCull()
	{
	}

	public void DLSSSetupCommandBuffer()
	{
	}

	public void DLSSRender()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMipmaps(float _renderToScreenRatio)
	{
		mipmapDelay -= Time.deltaTime;
		if (mipmapDelay <= 0f)
		{
			mipmapDelay = 2f;
			ulong currentTextureMemory = Texture.currentTextureMemory;
			if (mipmapTextureMem != currentTextureMemory)
			{
				mipmapTextureMem = currentTextureMemory;
				float num = 1f;
				num *= Mathf.Log(_renderToScreenRatio, 2f) - 1f;
				SetMipmapBias(num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMipmapBias(float _bias)
	{
		Texture2D[] array = Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].mipMapBias = _bias;
		}
		Texture2DArray[] array2 = Resources.FindObjectsOfTypeAll(typeof(Texture2DArray)) as Texture2DArray[];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].mipMapBias = _bias;
		}
	}

	public void OnGUI()
	{
		graphManager.Draw();
	}

	public bool FPSUpdateGraph(ref float value)
	{
		value = 1f / (Time.deltaTime + 0.001f);
		return true;
	}

	public bool SPFUpdateGraph(ref float value)
	{
		value = (Time.deltaTime + 0.0001f) * 1000f;
		return true;
	}
}
