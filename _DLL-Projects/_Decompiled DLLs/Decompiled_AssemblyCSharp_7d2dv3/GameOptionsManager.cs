using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public static class GameOptionsManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenExclusiveCheckDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cScreenExclusiveFrameWait = 10;

	public static readonly EnumDictionary<EnumGamePrefs, List<object>> QualityPresets;

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
		QualityPresets = new EnumDictionary<EnumGamePrefs, List<object>>();
		uiScaleLimits = new(double, double)[6]
		{
			(1.2, 0.65),
			(1.26, 0.7),
			(1.34, 0.75),
			(1.51, 0.85),
			(1.61, 0.9),
			(1000.0, 1.0)
		};
		initQualityPresets();
	}

	public static void ValidateGamePrefs()
	{
		GamePrefs.OnGamePrefChanged += OnGamePrefChanged;
		ValidateFoV();
		ValidateFoV3P();
		ValidateTreeDistance();
		ValidateHudSize();
		ValidateShadowDistance();
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
		case EnumGamePrefs.OptionsOverallAudioVolumeLevel:
			AudioListener.volume = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel);
			break;
		case EnumGamePrefs.OptionsGfxShadowDistance:
			ValidateShadowDistance();
			break;
		case EnumGamePrefs.OptionsGfxWaterPtlLimiter:
			WaterSplashCubes.particleLimiter = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateShadowDistance()
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		if (num >= 5 && num < 20)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 20);
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
		SignTextureManager.Instance.SetQuality((SignTextureManager.SignTextureQuality)GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSignQuality));
		SignTextureManager.Instance.SetTileSizeForCurrentQuality();
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
				int num = (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) ? GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV) : Constants.cDefaultCameraFieldOfView);
				camera.fieldOfView = num;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public static void initQualityPresets()
	{
		QualityPresets[EnumGamePrefs.OptionsGfxAA] = new List<object> { 0, 1, 2, 4, 4, null, 3, null, 3 };
		QualityPresets[EnumGamePrefs.OptionsGfxAASharpness] = new List<object> { 0f, 0f, 0f, 0.5f, 0.6f, null, 0.85f, null, 0.85f };
		QualityPresets[EnumGamePrefs.OptionsGfxMotionBlur] = new List<object> { 0, 0, 1, 1, 2, null, 2, null, 2 };
		QualityPresets[EnumGamePrefs.OptionsGfxTexQuality] = new List<object>
		{
			3,
			2,
			1,
			0,
			0,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 1 : 0,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 1 : 0
		};
		QualityPresets[EnumGamePrefs.OptionsGfxTexFilter] = new List<object> { 0, 0, 1, 2, 3, null, 3, null, 3 };
		QualityPresets[EnumGamePrefs.OptionsGfxReflectQuality] = new List<object> { 0, 0, 1, 2, 3, null, 3, null, 3 };
		QualityPresets[EnumGamePrefs.OptionsGfxReflectShadows] = new List<object> { false, false, false, false, true, null, true, null, true };
		QualityPresets[EnumGamePrefs.OptionsGfxShadowDistance] = new List<object> { 0, 0, 1, 2, 3, null, 1, null, 3 };
		QualityPresets[EnumGamePrefs.OptionsGfxShadowQuality] = new List<object>
		{
			0,
			1,
			2,
			3,
			4,
			null,
			2,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 2 : 3
		};
		QualityPresets[EnumGamePrefs.OptionsGfxLODDistance] = new List<object> { 0f, 0.25f, 0.5f, 0.75f, 1f, null, 0.75f, null, 0.75f };
		QualityPresets[EnumGamePrefs.OptionsGfxTerrainQuality] = new List<object> { 0, 1, 2, 3, 4, null, 2, null, 4 };
		QualityPresets[EnumGamePrefs.OptionsGfxObjQuality] = new List<object>
		{
			0,
			1,
			2,
			3,
			4,
			null,
			2,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 3 : 4
		};
		QualityPresets[EnumGamePrefs.OptionsGfxGrassDistance] = new List<object> { 0, 1, 2, 3, 3, null, 2, null, 3 };
		QualityPresets[EnumGamePrefs.OptionsGfxBloom] = new List<object> { false, false, true, true, true, null, true, null, true };
		QualityPresets[EnumGamePrefs.OptionsGfxDOF] = new List<object> { false, false, false, false, false, null, false, null, false };
		QualityPresets[EnumGamePrefs.OptionsGfxSSAO] = new List<object> { false, false, true, true, true, null, true, null, true };
		QualityPresets[EnumGamePrefs.OptionsGfxSSReflections] = new List<object> { 0, 1, 1, 2, 3, null, 1, null, 1 };
		QualityPresets[EnumGamePrefs.OptionsGfxSunShafts] = new List<object> { false, false, true, true, true, null, true, null, true };
		QualityPresets[EnumGamePrefs.OptionsGfxWaterQuality] = new List<object> { 0, 0, 1, 1, 1, null, 1, null, 1 };
		QualityPresets[EnumGamePrefs.OptionsGfxWaterPtlLimiter] = new List<object> { 0f, 0.2f, 0.5f, 0.75f, 1f, null, 0.75f, null, 0.75f };
		QualityPresets[EnumGamePrefs.OptionsGfxSignQuality] = new List<object> { 0, 1, 2, 3, 4, null, 2, null, 2 };
		QualityPresets[EnumGamePrefs.OptionsGfxOcclusion] = new List<object> { false, true, true, true, true, null, true, null, true };
		QualityPresets[EnumGamePrefs.OptionsGfxViewDistance] = new List<object> { 5, 5, 6, 6, 7, null, 6, null, 6 };
		QualityPresets[EnumGamePrefs.OptionsGfxDynamicScale] = new List<object>
		{
			null,
			null,
			null,
			null,
			null,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 0.6f : 0.5f,
			null,
			0.75f
		};
		QualityPresets[EnumGamePrefs.OptionsGfxFSRPreset] = new List<object> { null, null, null, null, null, null, 0, null, 2 };
	}

	public static void SetGraphicsQuality()
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset);
		if (num < 0)
		{
			Log.Warning($"SetGraphicsQuality: Selected preset is negative ({num})");
			return;
		}
		foreach (var (enumGamePrefs2, list2) in QualityPresets)
		{
			if (num >= list2.Count)
			{
				Log.Warning($"SetGraphicsQuality: Skipping GamePref {enumGamePrefs2.ToStringCached()}, selected preset ({num}) outside of defined values ({list2.Count})");
				continue;
			}
			object obj = list2[num];
			if (obj == null)
			{
				Log.Warning($"SetGraphicsQuality: Skipping GamePref {enumGamePrefs2.ToStringCached()}, selected preset ({num}) does not have a value defined for this setting");
			}
			else
			{
				GamePrefs.SetObject(enumGamePrefs2, obj);
			}
		}
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
