using System;
using InControl;
using Platform;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public static class GameOptionsManager
{
	public enum ResetType
	{
		All,
		Graphics,
		Audio,
		Controls,
		Controller,
		Bindings
	}

	public const string cPrefFullscreen = "Screenmanager Fullscreen mode";

	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenExclusiveCheckDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cScreenExclusiveFrameWait = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cActionSetSavePrefix = "ActionSet_";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly (double aspectLimit, double scaleLimit)[] uiScaleLimits;

	public static Vector2i CurrentScreenSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return new Vector2i(Screen.width, Screen.height);
		}
	}

	public static event Action<int, int> ResolutionChanged;

	public static event Action<int> TextureQualityChanged;

	public static event Action<int> TextureFilterChanged;

	public static event Action<int> ShadowDistanceChanged;

	public static event Action OnGameOptionsApplied;

	[PublicizedFrom(EAccessModifier.Private)]
	static GameOptionsManager()
	{
		uiScaleLimits = new(double, double)[6]
		{
			(1.2, 0.65),
			(1.26, 0.7),
			(1.34, 0.75),
			(1.51, 0.85),
			(1.61, 0.9),
			(1000.0, 1.0)
		};
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
		ValidateFoV();
		ValidateFoV3P();
		ValidateTreeDistance();
		ValidateHudSize();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		switch (_pref)
		{
		case EnumGamePrefs.OptionsGfxFOV:
			ValidateFoV();
			break;
		case EnumGamePrefs.OptionsGfxFOV3P:
			ValidateFoV3P();
			break;
		case EnumGamePrefs.OptionsGfxTreeDistance:
			ValidateTreeDistance();
			break;
		case EnumGamePrefs.OptionsHudSize:
			ValidateHudSize();
			break;
		case EnumGamePrefs.OptionsMumblePositionalAudioSupport:
			UpdateMumblePositionalAudioState();
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateFoV()
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
		if (num < Constants.cMinCameraFieldOfView)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cMinCameraFieldOfView);
		}
		else if (num > Constants.cMaxCameraFieldOfView)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cMaxCameraFieldOfView);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateFoV3P()
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV3P);
		if (num < Constants.cMinCameraFieldOfView)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cMinCameraFieldOfView);
		}
		else if (num > Constants.cMaxCameraFieldOfView)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cMaxCameraFieldOfView);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateTreeDistance()
	{
		if (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTreeDistance) < 2)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTreeDistance, 2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateHudSize()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
		if ((double)num < 0.01)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsHudSize, 1f);
		}
		else if ((double)num < 0.5)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsHudSize, 0.5f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateMumblePositionalAudioState()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsMumblePositionalAudioSupport))
		{
			MumblePositionalAudio.Init();
		}
		else
		{
			MumblePositionalAudio.Destroy();
		}
	}

	public static void ApplyAllOptions(LocalPlayerUI _playerUi)
	{
		QualitySettings.antiAliasing = 0;
		float num = (QualitySettings.streamingMipmapsMemoryBudget = GameOptionsPlatforms.GetStreamingMipmapBudget());
		Log.Out("ApplyAllOptions streaming budget {0} MB", num);
		QualitySettings.softParticles = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter) >= 0.51f;
		ApplyScreenResolution();
		AudioListener.volume = Math.Min(GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel), 1f);
		ApplyShadowQuality();
		ApplyTextureQuality();
		ApplyTextureFilter();
		ApplyCameraOptions();
		Shader.globalMaximumLOD = 400 + GamePrefs.GetInt(EnumGamePrefs.OptionsGfxObjQuality) * 100;
		QualitySettings.lodBias = GetLODBias();
		ApplyTerrainOptions();
		MeshDescription.SetGrassQuality();
		MeshDescription.SetWaterQuality();
		GameOptionsManager.OnGameOptionsApplied?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyScreenResolution()
	{
		(int width, int height, FullScreenMode fullScreenMode) screenOptions = PlatformApplicationManager.Application.ScreenOptions;
		int item = screenOptions.width;
		int item2 = screenOptions.height;
		FullScreenMode item3 = screenOptions.fullScreenMode;
		Resolution currentResolution = PlatformApplicationManager.Application.GetCurrentResolution();
		Log.Out("ApplyAllOptions current screen {0} x {1}, {2}hz, window {3} x {4}, mode {5}", currentResolution.width, currentResolution.height, currentResolution.refreshRateRatio.value, Screen.width, Screen.height, Screen.fullScreenMode);
		SetResolution(item, item2, item3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyShadowQuality()
	{
		Vector3 vector = new Vector3(0.06f, 0.15f, 0.35f);
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		switch (num)
		{
		case 0:
			QualitySettings.shadowDistance = 35f;
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowCascade2Split = 0.33f;
			break;
		case 1:
			QualitySettings.shadowDistance = 80f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowCascade4Split = vector;
			break;
		case 2:
			QualitySettings.shadowDistance = 120f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowCascade4Split = vector;
			break;
		case 3:
			QualitySettings.shadowDistance = 200f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowCascade4Split = vector * 0.8f;
			break;
		default:
			QualitySettings.shadowDistance = 300f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowCascade4Split = vector * 0.6f;
			break;
		}
		if (GameOptionsManager.ShadowDistanceChanged != null)
		{
			GameOptionsManager.ShadowDistanceChanged(num);
		}
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality))
		{
		case 0:
			QualitySettings.shadows = ShadowQuality.Disable;
			break;
		case 1:
			QualitySettings.shadows = ShadowQuality.HardOnly;
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			break;
		case 2:
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			break;
		case 3:
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = ShadowResolution.High;
			break;
		case 4:
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = ShadowResolution.High;
			break;
		case 5:
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
			break;
		}
	}

	public static float GetLODBias()
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxObjQuality);
		return num switch
		{
			0 => 0.5f, 
			1 => 0.65f, 
			2 => 0.8f, 
			3 => 1.2f, 
			4 => 1.7f, 
			_ => (float)num / 100f, 
		};
	}

	public static void CheckResolution()
	{
		if (screenExclusiveCheckDelay > 0 && --screenExclusiveCheckDelay == 0)
		{
			screenExclusiveCheckDelay = 10;
			if (Screen.width != screenWidth || Screen.height != screenHeight)
			{
				Log.Warning("Fullscreen Exclusive failed! Reverting to {0} x {1}", screenWidth, screenHeight);
				SetResolution(screenWidth, screenHeight);
			}
		}
	}

	public static void SetResolution(int _width, int _height, FullScreenMode _fullscreen = FullScreenMode.Windowed)
	{
		if (Screen.width != _width || Screen.height != _height || Screen.fullScreenMode != _fullscreen)
		{
			Resolution currentResolution = PlatformApplicationManager.Application.GetCurrentResolution();
			Log.Out("SetResolution was screen {0} x {1}, {2}hz, window {3} x {4}, mode {5}", currentResolution.width, currentResolution.height, currentResolution.refreshRateRatio.value, Screen.width, Screen.height, Screen.fullScreenMode);
			Log.Out("SetResolution to {0} x {1}, mode {2}", _width, _height, _fullscreen);
			screenWidth = _width;
			screenHeight = _height;
			screenExclusiveCheckDelay = ((_fullscreen == FullScreenMode.ExclusiveFullScreen) ? 10 : 0);
			PlatformApplicationManager.Application.SetResolution(_width, _height, _fullscreen);
			if (GameOptionsManager.ResolutionChanged != null)
			{
				GameOptionsManager.ResolutionChanged(_width, _height);
			}
		}
	}

	public static int GetTextureQuality(int _overrideValue = -1)
	{
		if (_overrideValue != -1)
		{
			return _overrideValue;
		}
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexQuality);
		if (Constants.Is32BitOs && num < 2)
		{
			num = 2;
		}
		return Utils.FastMax(num, CalcTextureQualityMin());
	}

	public static int CalcTextureQualityMin()
	{
		return GameOptionsPlatforms.CalcTextureQualityMin();
	}

	public static void ApplyTextureQuality(int _overrideValue = -1)
	{
		int textureQuality = GetTextureQuality(_overrideValue);
		QualitySettings.streamingMipmapsActive = true;
		QualitySettings.streamingMipmapsMaxLevelReduction = Math.Max(3, GameRenderManager.TextureMipmapLimit);
		GameRenderManager.TextureMipmapLimit = textureQuality;
		float value = 0.6776996f;
		switch (textureQuality)
		{
		case 1:
			value = 0.6f;
			break;
		case 2:
			value = 0.5f;
			break;
		case 3:
			value = 0.4f;
			break;
		}
		Shader.SetGlobalFloat("_MipSlope", value);
		if (GameOptionsManager.TextureQualityChanged != null)
		{
			GameOptionsManager.TextureQualityChanged(textureQuality);
		}
		Log.Out("Texture quality is set to " + GameRenderManager.TextureMipmapLimit);
	}

	public static int GetTextureFilter()
	{
		return GameOptionsPlatforms.ApplyTextureFilterLimit(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexFilter));
	}

	public static void ApplyTextureFilter()
	{
		int textureFilter = GetTextureFilter();
		QualitySettings.anisotropicFiltering = ((textureFilter != 0) ? ((textureFilter <= 3) ? AnisotropicFiltering.Enable : AnisotropicFiltering.ForceEnable) : AnisotropicFiltering.Disable);
		if (GameOptionsManager.TextureFilterChanged != null)
		{
			GameOptionsManager.TextureFilterChanged(textureFilter);
		}
		Log.Out("ApplyTextureFilter {0}, AF {1}", textureFilter, QualitySettings.anisotropicFiltering);
	}

	public static void ApplyCameraOptions(EntityPlayerLocal _playerLocal = null)
	{
		bool enabled = false;
		float num = 9.5f;
		int num2 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance);
		num = (((uint)(num2 - 8) <= 4u) ? 9.5f : 0f);
		Shader.SetGlobalFloat("_MinDistantMip", num);
		GameRenderManager.ApplyCameraOptions(_playerLocal);
		Camera[] array = ((!_playerLocal) ? Camera.allCameras : _playerLocal.GetComponentsInChildren<Camera>());
		foreach (Camera camera in array)
		{
			if ((camera.cullingMask & 0x1000) == 0)
			{
				if (camera.TryGetComponent<DepthOfField>(out var component))
				{
					component.enabled = enabled;
				}
				camera.allowHDR = true;
				int num3 = (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) ? GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV) : Constants.cDefaultCameraFieldOfView);
				camera.fieldOfView = num3;
				camera.renderingPath = RenderingPath.DeferredShading;
				if (QualitySettings.antiAliasing != 0 && camera.actualRenderingPath == RenderingPath.DeferredShading)
				{
					Log.Warning("QualitySettings antialiasing has been enabled but the rendering path is set to deferred. This is incompatible and wastes memory and so will be disabled");
					QualitySettings.antiAliasing = 0;
				}
				camera.farClipPlane = 2000f;
			}
		}
	}

	public static void ApplyTerrainOptions()
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality);
		if (num <= 1)
		{
			Shader.EnableKeyword("GAME_TERRAINLOWQ");
		}
		else
		{
			Shader.DisableKeyword("GAME_TERRAINLOWQ");
		}
		if (num == 0)
		{
			Shader.DisableKeyword("_MAX3LAYER");
			Shader.EnableKeyword("_MAX2LAYER");
		}
		else if (num <= 1)
		{
			Shader.DisableKeyword("_MAX2LAYER");
			Shader.EnableKeyword("_MAX3LAYER");
		}
		else
		{
			Shader.DisableKeyword("_MAX2LAYER");
			Shader.DisableKeyword("_MAX3LAYER");
		}
		Log.Out("ApplyTerrainOptions {0}", num);
	}

	public static void SetGraphicsQuality()
	{
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset))
		{
		case 0:
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAASharpness, 0f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlur, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cDefaultCameraFieldOfView - 7);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cDefaultCameraFieldOfView - 7);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexQuality, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexFilter, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectShadows, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxLODDistance, 0f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxBloom, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSReflections, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSunShafts, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterPtlLimiter, 0f);
			if (GameManager.Instance.World == null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, 5);
				GamePrefs.Set(EnumGamePrefs.OptionsGfxOcclusion, _value: false);
			}
			break;
		case 1:
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAASharpness, 0f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlur, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cDefaultCameraFieldOfView - 4);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cDefaultCameraFieldOfView - 4);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexQuality, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexFilter, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectShadows, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxLODDistance, 0.25f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxBloom, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSReflections, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSunShafts, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterPtlLimiter, 0.2f);
			if (GameManager.Instance.World == null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, 5);
				GamePrefs.Set(EnumGamePrefs.OptionsGfxOcclusion, _value: true);
			}
			break;
		case 2:
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAASharpness, 0f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlur, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cDefaultCameraFieldOfView);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cDefaultCameraFieldOfView);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexQuality, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexFilter, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectQuality, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectShadows, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxLODDistance, 0.5f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxBloom, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSReflections, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSunShafts, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterQuality, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterPtlLimiter, 0.5f);
			if (GameManager.Instance.World == null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, 6);
				GamePrefs.Set(EnumGamePrefs.OptionsGfxOcclusion, _value: true);
			}
			break;
		case 3:
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, 4);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAASharpness, 0.5f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlur, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cDefaultCameraFieldOfView);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cDefaultCameraFieldOfView);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexFilter, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectQuality, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectShadows, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxLODDistance, 0.75f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxBloom, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSReflections, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSunShafts, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterQuality, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterPtlLimiter, 0.75f);
			if (GameManager.Instance.World == null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, 6);
				GamePrefs.Set(EnumGamePrefs.OptionsGfxOcclusion, _value: true);
			}
			break;
		case 4:
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, 4);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAASharpness, 0.6f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlur, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cDefaultCameraFieldOfView);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cDefaultCameraFieldOfView);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexQuality, 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexFilter, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectQuality, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectShadows, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, 4);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxLODDistance, 1f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, 4);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, 4);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxBloom, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSReflections, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSunShafts, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterQuality, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterPtlLimiter, 1f);
			if (GameManager.Instance.World == null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, 7);
				GamePrefs.Set(EnumGamePrefs.OptionsGfxOcclusion, _value: true);
			}
			break;
		case 6:
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicScale, DeviceFlag.XBoxSeriesS.IsCurrent() ? 0.6f : 0.5f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFSRPreset, 0);
			ApplyConsoleCommonGfxOptions();
			ApplyConsolePerformanceGfxOptions();
			break;
		case 8:
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicScale, 0.75f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFSRPreset, 2);
			ApplyConsoleCommonGfxOptions();
			ApplyConsoleQualityGfxOptions();
			break;
		case 5:
		case 7:
			break;
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void ApplyConsoleCommonGfxOptions()
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxOcclusion, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxBloom, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, _value: false);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlur, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSunShafts, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxAASharpness, 0.85f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexQuality, DeviceFlag.XBoxSeriesS.IsCurrent() ? 1 : 0);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTexFilter, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectQuality, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectShadows, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterPtlLimiter, 0.75f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, 6);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxLODDistance, 0.75f);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterQuality, 1);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void ApplyConsolePerformanceGfxOptions()
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 1);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, 2);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, 2);
		}
		[PublicizedFrom(EAccessModifier.Internal)]
		static void ApplyConsoleQualityGfxOptions()
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, DeviceFlag.XBoxSeriesS.IsCurrent() ? 2 : 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, 4);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, 3);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, DeviceFlag.XBoxSeriesS.IsCurrent() ? 3 : 4);
		}
	}

	public static bool ResetGameOptions(ResetType _resetType)
	{
		if (_resetType == ResetType.Audio)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsAmbientVolumeLevel, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsAmbientVolumeLevel));
			GamePrefs.Set(EnumGamePrefs.OptionsMusicVolumeLevel, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsMusicVolumeLevel));
			GamePrefs.Set(EnumGamePrefs.OptionsMenuMusicVolumeLevel, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsMenuMusicVolumeLevel));
			GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicEnabled, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsDynamicMusicEnabled));
			GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicDailyTime, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsDynamicMusicDailyTime));
			GamePrefs.Set(EnumGamePrefs.OptionsMicVolumeLevel, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsMicVolumeLevel));
			GamePrefs.Set(EnumGamePrefs.OptionsVoiceVolumeLevel, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceVolumeLevel));
			GamePrefs.Set(EnumGamePrefs.OptionsOverallAudioVolumeLevel, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsOverallAudioVolumeLevel));
			GamePrefs.Set(EnumGamePrefs.OptionsVoiceChatEnabled, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceChatEnabled));
			GamePrefs.Set(EnumGamePrefs.OptionsAudioOcclusion, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsAudioOcclusion));
			GamePrefs.Set(EnumGamePrefs.OptionsSubtitlesEnabled, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsSubtitlesEnabled));
		}
		if (_resetType == ResetType.Graphics)
		{
			ResetGraphicsOptions();
		}
		if (_resetType == ResetType.Controls)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsLookSensitivity, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsLookSensitivity));
			GamePrefs.Set(EnumGamePrefs.OptionsZoomSensitivity, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsZoomSensitivity));
			GamePrefs.Set(EnumGamePrefs.OptionsZoomAccel, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsZoomAccel));
			GamePrefs.Set(EnumGamePrefs.OptionsInvertMouse, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsInvertMouse));
			GamePrefs.Set(EnumGamePrefs.OptionsVehicleLookSensitivity, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsVehicleLookSensitivity));
			GamePrefs.Set(EnumGamePrefs.OptionsControlsSprintLock, (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsControlsSprintLock));
			foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
			{
				actionSet.Reset();
			}
			SaveControls();
		}
		if (_resetType == ResetType.Controller)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsAllowController, _value: true);
			GamePrefs.Set(EnumGamePrefs.OptionsControllerVibration, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerVibration));
			GamePrefs.Set(EnumGamePrefs.OptionsInterfaceSensitivity, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsInterfaceSensitivity));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerSensitivityX, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerSensitivityX));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerSensitivityY, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerSensitivityY));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerLookInvert, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerLookInvert));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerJoystickLayout, (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerJoystickLayout));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerLookAcceleration, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerLookAcceleration));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerZoomSensitivity, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerZoomSensitivity));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerLookAxisDeadzone, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerLookAxisDeadzone));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerMoveAxisDeadzone, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerMoveAxisDeadzone));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerCursorSnap, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerCursorSnap));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerCursorHoverSensitivity, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerCursorHoverSensitivity));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerVehicleSensitivity, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerVehicleSensitivity));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerAimAssists, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerAimAssists));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerWeaponAiming, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerWeaponAiming));
			GamePrefs.Set(EnumGamePrefs.OptionsControlsSprintLock, (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsControlsSprintLock));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerTriggerEffects, (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerTriggerEffects));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerIconStyle, (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsControllerIconStyle));
			foreach (PlayerActionsBase actionSet2 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				actionSet2.Reset();
			}
			SaveControls();
		}
		if (_resetType == ResetType.Bindings)
		{
			foreach (PlayerActionsBase actionSet3 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				actionSet3.Reset();
			}
			SaveControls();
		}
		if (_resetType == ResetType.All)
		{
			GamePrefs.PropertyDecl[] propertyList = GamePrefs.GetPropertyList();
			for (int i = 0; i < propertyList.Length; i++)
			{
				switch (propertyList[i].type)
				{
				case GamePrefs.EnumType.Float:
					GamePrefs.Set(propertyList[i].name, (float)GamePrefs.GetDefault(propertyList[i].name));
					break;
				case GamePrefs.EnumType.Int:
					GamePrefs.Set(propertyList[i].name, (int)GamePrefs.GetDefault(propertyList[i].name));
					break;
				case GamePrefs.EnumType.String:
					GamePrefs.Set(propertyList[i].name, (string)GamePrefs.GetDefault(propertyList[i].name));
					break;
				case GamePrefs.EnumType.Binary:
					GamePrefs.Set(propertyList[i].name, (string)GamePrefs.GetDefault(propertyList[i].name));
					break;
				case GamePrefs.EnumType.Bool:
					GamePrefs.Set(propertyList[i].name, (bool)GamePrefs.GetDefault(propertyList[i].name));
					break;
				}
			}
			foreach (PlayerActionsBase actionSet4 in PlatformManager.NativePlatform.Input.ActionSets)
			{
				actionSet4.Reset();
			}
			SaveControls();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ResetGraphicsOptions()
	{
		GamePrefs.Set(EnumGamePrefs.OptionsGfxResolution, (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxResolution));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicMode, (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxDynamicMode));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxUpscalerMode));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxFSRPreset, (int)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxFSRPreset));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicScale, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxDynamicScale));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxBrightness, (float)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxBrightness));
		EnumGamePrefs vSyncCountPref = PlatformApplicationManager.Application.VSyncCountPref;
		GamePrefs.Set(vSyncCountPref, (int)GamePrefs.GetDefault(vSyncCountPref));
		int value = GameOptionsPlatforms.CalcDefaultGfxPreset();
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, value);
		int defaultUpscalerMode = GameOptionsPlatforms.DefaultUpscalerMode;
		GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, defaultUpscalerMode);
		SetGraphicsQuality();
		GamePrefs.Set(EnumGamePrefs.DynamicMeshEnabled, (bool)GamePrefs.GetDefault(EnumGamePrefs.DynamicMeshEnabled));
		GamePrefs.Set(EnumGamePrefs.DynamicMeshDistance, (int)GamePrefs.GetDefault(EnumGamePrefs.DynamicMeshDistance));
		GamePrefs.Set(EnumGamePrefs.NoGraphicsMode, (bool)GamePrefs.GetDefault(EnumGamePrefs.NoGraphicsMode));
	}

	public static void SaveControls()
	{
		foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
		{
			SdPlayerPrefs.SetString("ActionSet_" + actionSet.Name, actionSet.Save());
		}
		SdPlayerPrefs.SetString("Controls", ExportControls());
		SdPlayerPrefs.SetInt("ActionSetsSaved", 0);
		ApplyAllowControllerOption();
	}

	public static void LoadControls()
	{
		bool flag = SdPlayerPrefs.HasKey("ActionSetsSaved");
		if (!flag && SdPlayerPrefs.HasKey("Controls"))
		{
			string text = SdPlayerPrefs.GetString("Controls", string.Empty);
			PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(text.Split(';'));
			SaveControls();
			ApplyAllowControllerOption();
			RestoreNonBindableControllerActionsToDefaults();
			Log.Out("Legacy controls data converted");
			return;
		}
		if (flag)
		{
			foreach (PlayerActionsBase actionSet in PlatformManager.NativePlatform.Input.ActionSets)
			{
				if (!string.IsNullOrEmpty(SdPlayerPrefs.GetString("ActionSet_" + actionSet.Name, string.Empty)))
				{
					actionSet.Load(SdPlayerPrefs.GetString("ActionSet_" + actionSet.Name));
				}
				else
				{
					Log.Warning("Loading controls: No data for action set " + actionSet.Name);
				}
			}
		}
		ApplyAllowControllerOption();
		RestoreNonBindableControllerActionsToDefaults();
	}

	public static string ExportControls()
	{
		string[] array = new string[PlatformManager.NativePlatform.Input.ActionSets.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = PlatformManager.NativePlatform.Input.ActionSets[i].Save();
		}
		return string.Join(";", array);
	}

	public static void ImportControls(string importString)
	{
		PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(importString.Split(';'));
		ApplyAllowControllerOption();
		RestoreNonBindableControllerActionsToDefaults();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyAllowControllerOption()
	{
		bool flag = GamePrefs.GetBool(EnumGamePrefs.OptionsAllowController);
		for (int i = 0; i < PlatformManager.NativePlatform.Input.ActionSets.Count; i++)
		{
			PlatformManager.NativePlatform.Input.ActionSets[i].Device = (flag ? null : InputDevice.Null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RestoreNonBindableControllerActionsToDefaults()
	{
		PlatformManager.NativePlatform.Input.GetActionSetForName("gui").ResetControllerBindings();
		PlatformManager.NativePlatform.Input.GetActionSetForName("permanent").ResetControllerBindings();
	}

	public static double GetUiSizeLimit(double _aspectRation)
	{
		int i;
		for (i = 0; _aspectRation > uiScaleLimits[i].aspectLimit; i++)
		{
		}
		return uiScaleLimits[i].scaleLimit;
	}

	public static double GetUiSizeLimit()
	{
		Vector2i currentScreenSize = CurrentScreenSize;
		return GetUiSizeLimit((double)currentScreenSize.x / (double)currentScreenSize.y);
	}

	public static float GetActiveUiScale()
	{
		float v = (float)GetUiSizeLimit();
		float v2 = GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
		return Utils.FastMin(v, v2);
	}
}
