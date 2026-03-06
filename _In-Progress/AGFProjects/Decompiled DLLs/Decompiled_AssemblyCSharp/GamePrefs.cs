using System;
using System.Collections.Generic;
using Platform;
using SDF;

public class GamePrefs
{
	public enum EnumType
	{
		Int,
		Float,
		String,
		Bool,
		Binary
	}

	public struct PropertyDecl
	{
		public EnumGamePrefs name;

		public EnumType type;

		public object defaultValue;

		public DeviceFlag bPersistent;

		public object minStockValue;

		public object maxStockValue;

		public bool IsPersistent => bPersistent.HasFlag(DeviceFlag.StandaloneWindows);

		public PropertyDecl(EnumGamePrefs _name, DeviceFlag _bPersistent, EnumType _type, object _defaultValue, object _minStockValue, object _maxStockValue, Dictionary<DeviceFlag, object> _deviceDefaults = null)
		{
			name = _name;
			type = _type;
			bPersistent = _bPersistent;
			minStockValue = _minStockValue;
			maxStockValue = _maxStockValue;
			if (_deviceDefaults != null && _deviceDefaults.ContainsKey(DeviceFlag.StandaloneWindows))
			{
				defaultValue = _deviceDefaults[DeviceFlag.StandaloneWindows];
			}
			else
			{
				defaultValue = _defaultValue;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PropertyDecl[] s_propertyList;

	[PublicizedFrom(EAccessModifier.Private)]
	public object[] propertyValues = new object[285];

	[PublicizedFrom(EAccessModifier.Private)]
	public static GamePrefs m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<IGamePrefsChangedListener> listeners = new List<IGamePrefsChangedListener>();

	public static GamePrefs Instance
	{
		get
		{
			if (m_Instance == null)
			{
				throw new InvalidOperationException("GamePrefs is being accessed before it is ready.");
			}
			return m_Instance;
		}
	}

	public static event Action<EnumGamePrefs> OnGamePrefChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void initPropertyDecl()
	{
		int num = 131072;
		if (!GameManager.IsDedicatedServer)
		{
			num = 524288;
		}
		s_propertyList = new PropertyDecl[262]
		{
			new PropertyDecl(EnumGamePrefs.OptionsAmbientVolumeLevel, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsDynamicMusicEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsDynamicMusicDailyTime, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.45f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsPlayChanceFrequency, DeviceFlag.None, EnumType.Float, 3f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsPlayChanceProbability, DeviceFlag.None, EnumType.Float, 0.983f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsMusicVolumeLevel, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.6f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsMenuMusicVolumeLevel, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.7f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsMicVolumeLevel, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.75f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsVoiceVolumeLevel, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.75f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsOverallAudioVolumeLevel, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsVoiceChatEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsVoiceInputDevice, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.OptionsVoiceOutputDevice, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.OptionsMumblePositionalAudioSupport, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsAudioOcclusion, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxResetRevision, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControlsResetRevision, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxWaterQuality, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxViewDistance, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 6, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxShadowDistance, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxResolution, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxDynamicMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxDynamicMinFPS, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 30, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxDynamicScale, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxVsync, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxAA, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxAASharpness, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Float, 0f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxLODDistance, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxFOV, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, Constants.cDefaultCameraFieldOfView, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxTexQuality, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxTexFilter, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxReflectQuality, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxStreamMipmaps, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxTerrainQuality, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 3, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxObjQuality, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 3, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxGrassDistance, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxQualityPreset, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 2, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxOcclusion, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxBloom, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxDOF, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxMotionBlur, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxSSAO, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxSSReflections, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxSunShafts, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxReflectShadows, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsHudSize, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsHudOpacity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsShowCrosshair, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsShowCompass, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsBackgroundGlobalOpacity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.95f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsForegroundGlobalOpacity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxBrightness, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxWaterPtlLimiter, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsPOICulling, DeviceFlag.None, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsDisableChunkLODs, DeviceFlag.None, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsPlayerModel, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "playerMale", null, null),
			new PropertyDecl(EnumGamePrefs.OptionsPlayerModelTexture, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "Player/Male/Player_male", null, null),
			new PropertyDecl(EnumGamePrefs.OptionsLookSensitivity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsZoomSensitivity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.3f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsZoomAccel, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsVehicleLookSensitivity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsWeaponAiming, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsInvertMouse, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsAllowController, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsStabSpawnBlocksOnGround, DeviceFlag.None, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.GameName, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "My Game", null, null),
			new PropertyDecl(EnumGamePrefs.GameNameClient, DeviceFlag.None, EnumType.String, "My Game", null, null),
			new PropertyDecl(EnumGamePrefs.GameMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, GameModeSurvival.TypeName, null, null),
			new PropertyDecl(EnumGamePrefs.GameDifficulty, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.GameWorld, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, null, null, null),
			new PropertyDecl(EnumGamePrefs.GameVersion, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, Constants.cVersionInformation.LongStringNoBuild, null, null),
			new PropertyDecl(EnumGamePrefs.ServerIP, DeviceFlag.None, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.ServerPort, DeviceFlag.None, EnumType.Int, Constants.cDefaultPort, null, null),
			new PropertyDecl(EnumGamePrefs.ServerMaxPlayerCount, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 8, null, null, new Dictionary<DeviceFlag, object>
			{
				{
					DeviceFlag.PS5,
					4
				},
				{
					DeviceFlag.XBoxSeriesX,
					4
				},
				{
					DeviceFlag.XBoxSeriesS,
					2
				}
			}),
			new PropertyDecl(EnumGamePrefs.ServerPasswordCache, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Binary, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.ServerIsPublic, DeviceFlag.None, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.ServerPassword, DeviceFlag.None, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.ServerName, DeviceFlag.None, EnumType.String, "Default Server", null, null),
			new PropertyDecl(EnumGamePrefs.ServerDescription, DeviceFlag.None, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.ServerWebsiteURL, DeviceFlag.None, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.ServerMaxWorldTransferSpeedKiBs, DeviceFlag.None, EnumType.Int, 512, null, null),
			new PropertyDecl(EnumGamePrefs.ServerMaxAllowedViewDistance, DeviceFlag.None, EnumType.Int, 12, null, null),
			new PropertyDecl(EnumGamePrefs.ServerAllowCrossplay, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.ConnectToServerIP, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "127.0.0.1", null, null),
			new PropertyDecl(EnumGamePrefs.ConnectToServerPort, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, Constants.cDefaultPort, null, null),
			new PropertyDecl(EnumGamePrefs.FavoriteServersList, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.UNUSED_ControlPanelPort, DeviceFlag.None, EnumType.Int, 8080, null, null),
			new PropertyDecl(EnumGamePrefs.UNUSED_ControlPanelPassword, DeviceFlag.None, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.CreateLevelName, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "My Level", null, null),
			new PropertyDecl(EnumGamePrefs.CreateLevelDim, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "4096", null, null),
			new PropertyDecl(EnumGamePrefs.DebugMenuShowTasks, DeviceFlag.None, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.DebugMenuEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.DebugStopEnemiesMoving, DeviceFlag.None, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.CreativeMenuEnabled, DeviceFlag.None, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.PlayerName, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "Player", null, null),
			new PropertyDecl(EnumGamePrefs.UNUSED_PlayerId, DeviceFlag.None, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.PlayerPassword, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Binary, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.PlayerAutologin, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.PlayerToken, DeviceFlag.None, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.PlayerSafeZoneHours, DeviceFlag.None, EnumType.Int, 7, null, null),
			new PropertyDecl(EnumGamePrefs.PlayerSafeZoneLevel, DeviceFlag.None, EnumType.Int, 5, null, null),
			new PropertyDecl(EnumGamePrefs.DynamicSpawner, DeviceFlag.None, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.PlayerKillingMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, EnumPlayerKillingMode.KillStrangersOnly, null, null),
			new PropertyDecl(EnumGamePrefs.MatchLength, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 10, null, null),
			new PropertyDecl(EnumGamePrefs.FragLimit, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 20, null, null),
			new PropertyDecl(EnumGamePrefs.RebuildMap, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.JoiningOptions, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.ZombiePlayers, DeviceFlag.None, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.DayCount, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 3, null, null),
			new PropertyDecl(EnumGamePrefs.DayNightLength, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 60, 60, null),
			new PropertyDecl(EnumGamePrefs.DayLightLength, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 18, 12, 18),
			new PropertyDecl(EnumGamePrefs.BloodMoonFrequency, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 7, null, null),
			new PropertyDecl(EnumGamePrefs.BloodMoonRange, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.BloodMoonWarning, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 8, null, null),
			new PropertyDecl(EnumGamePrefs.ShowFriendPlayerOnMap, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.AdminFileName, DeviceFlag.None, EnumType.String, "serveradmin.xml", null, null),
			new PropertyDecl(EnumGamePrefs.UNUSED_ControlPanelEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.TelnetEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.TelnetPort, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 25003, null, null),
			new PropertyDecl(EnumGamePrefs.TelnetPassword, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.ZombieFeralSense, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.ZombieMove, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.ZombieMoveNight, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 3, null, null),
			new PropertyDecl(EnumGamePrefs.ZombieFeralMove, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 3, null, null),
			new PropertyDecl(EnumGamePrefs.ZombieBMMove, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 3, null, null),
			new PropertyDecl(EnumGamePrefs.DeathPenalty, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 1, 1, null),
			new PropertyDecl(EnumGamePrefs.DropOnDeath, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 1, 1, null),
			new PropertyDecl(EnumGamePrefs.DropOnQuit, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, 0, 1),
			new PropertyDecl(EnumGamePrefs.BloodMoonEnemyCount, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 8, 8, null),
			new PropertyDecl(EnumGamePrefs.EnemySpawnMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, true, null),
			new PropertyDecl(EnumGamePrefs.EnemyDifficulty, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.BlockDamagePlayer, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 100, 100, null),
			new PropertyDecl(EnumGamePrefs.BlockDamageAI, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 100, 100, null),
			new PropertyDecl(EnumGamePrefs.BlockDamageAIBM, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 100, 100, null),
			new PropertyDecl(EnumGamePrefs.LootRespawnDays, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 7, 7, null),
			new PropertyDecl(EnumGamePrefs.LootAbundance, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 100, 100, null),
			new PropertyDecl(EnumGamePrefs.LandClaimCount, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 5, null, null),
			new PropertyDecl(EnumGamePrefs.LandClaimSize, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 41, null, null),
			new PropertyDecl(EnumGamePrefs.LandClaimDeadZone, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 30, null, null),
			new PropertyDecl(EnumGamePrefs.LandClaimExpiryTime, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 7, null, null),
			new PropertyDecl(EnumGamePrefs.LandClaimDecayMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.LandClaimOnlineDurabilityModifier, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 4, null, null),
			new PropertyDecl(EnumGamePrefs.LandClaimOfflineDurabilityModifier, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 4, null, null),
			new PropertyDecl(EnumGamePrefs.LandClaimOfflineDelay, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.AirDropFrequency, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 72, 72, null),
			new PropertyDecl(EnumGamePrefs.AirDropMarker, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.PartySharedKillRange, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 100, null, null),
			new PropertyDecl(EnumGamePrefs.MaxSpawnedZombies, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 64, null, null),
			new PropertyDecl(EnumGamePrefs.MaxSpawnedAnimals, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 50, null, null),
			new PropertyDecl(EnumGamePrefs.AutopilotMode, DeviceFlag.None, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.SelectionOperationMode, DeviceFlag.None, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.SelectionContextMode, DeviceFlag.None, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.EACEnabled, DeviceFlag.None, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.BuildCreate, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, false, null),
			new PropertyDecl(EnumGamePrefs.PersistentPlayerProfiles, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.XPMultiplier, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 100, 100, null),
			new PropertyDecl(EnumGamePrefs.LastGameResetRevision, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.NoGraphicsMode, DeviceFlag.None, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsTempCelsius, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsDisableXmlEvents, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.ServerDisabledNetworkProtocols, DeviceFlag.None, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.OptionsScreenBoundsValue, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsUiFpsScaling, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsInterfaceSensitivity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerVibration, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerTriggerEffects, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, TriggerEffectManager.SettingDefaultValue(), null, null),
			new PropertyDecl(EnumGamePrefs.HideCommandExecutionLog, DeviceFlag.None, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.MaxUncoveredMapChunksPerPlayer, DeviceFlag.None, EnumType.Int, num, null, null),
			new PropertyDecl(EnumGamePrefs.ServerReservedSlots, DeviceFlag.None, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.ServerReservedSlotsPermission, DeviceFlag.None, EnumType.Int, 100, null, null),
			new PropertyDecl(EnumGamePrefs.ServerAdminSlots, DeviceFlag.None, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.ServerAdminSlotsPermission, DeviceFlag.None, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.GameGuidClient, DeviceFlag.None, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.BedrollDeadZoneSize, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 15, null, null),
			new PropertyDecl(EnumGamePrefs.BedrollExpiryTime, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 45, null, null),
			new PropertyDecl(EnumGamePrefs.LastLoadedPrefab, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsJournalPopup, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsFilterProfanity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsQuestsAutoShare, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsQuestsAutoAccept, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsAutoPartyWithFriends, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.TelnetFailedLoginLimit, DeviceFlag.None, EnumType.Int, 10, null, null),
			new PropertyDecl(EnumGamePrefs.TelnetFailedLoginsBlocktime, DeviceFlag.None, EnumType.Int, 10, null, null),
			new PropertyDecl(EnumGamePrefs.TerminalWindowEnabled, DeviceFlag.None, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.ServerEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.ServerVisibility, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 2, null, null),
			new PropertyDecl(EnumGamePrefs.ServerLoginConfirmationText, DeviceFlag.None, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.WorldGenSeed, DeviceFlag.None, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.WorldGenSize, DeviceFlag.None, EnumType.Int, 8192, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxTreeDistance, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 4, null, null),
			new PropertyDecl(EnumGamePrefs.LastLoadingTipRead, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, -1, null, null),
			new PropertyDecl(EnumGamePrefs.DynamicMeshEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.DynamicMeshDistance, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 1000, 100, 3000),
			new PropertyDecl(EnumGamePrefs.DynamicMeshLandClaimOnly, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.DynamicMeshLandClaimBuffer, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 3, 1, 5),
			new PropertyDecl(EnumGamePrefs.DynamicMeshUseImposters, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.DynamicMeshMaxRegionCache, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 1, 1, 3),
			new PropertyDecl(EnumGamePrefs.DynamicMeshMaxItemCache, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 3, 1, 6),
			new PropertyDecl(EnumGamePrefs.TwitchServerPermission, DeviceFlag.None, EnumType.Int, 90, null, null),
			new PropertyDecl(EnumGamePrefs.TwitchBloodMoonAllowed, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsSelectionBoxAlphaMultiplier, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.4f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsPoiVolumesSkipDeleteConfirmation, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.PlaytestBiome, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 3, null, null),
			new PropertyDecl(EnumGamePrefs.Language, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.LanguageBrowser, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.Region, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.ServerHistoryCache, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Binary, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.MaxChunkAge, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, -1, null, null),
			new PropertyDecl(EnumGamePrefs.SaveDataLimit, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, -1, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsSubtitlesEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsIntroMovieEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.AllowSpawnNearBackpack, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.AllowSpawnNearFriend, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 2, null, null),
			new PropertyDecl(EnumGamePrefs.WebDashboardEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.WebDashboardPort, DeviceFlag.None, EnumType.Int, 8080, null, null),
			new PropertyDecl(EnumGamePrefs.WebDashboardUrl, DeviceFlag.None, EnumType.String, "", null, null),
			new PropertyDecl(EnumGamePrefs.EnableMapRendering, DeviceFlag.None, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.MaxQueuedMeshLayers, DeviceFlag.None, EnumType.Int, 40, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerSensitivityX, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.35f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerSensitivityY, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.25f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerLookInvert, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerJoystickLayout, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerLookAcceleration, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 4f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerZoomSensitivity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerLookAxisDeadzone, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerMoveAxisDeadzone, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerCursorSnap, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerCursorHoverSensitivity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.5f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerVehicleSensitivity, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 1f, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerWeaponAiming, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerAimAssists, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsChatCommunication, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsCrossplay, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControlsSprintLock, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.DebugPanelsEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, "-", null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControllerVibrationStrength, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 2, null, null),
			new PropertyDecl(EnumGamePrefs.EulaVersionAccepted, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, -1, null, null),
			new PropertyDecl(EnumGamePrefs.EulaLatestVersion, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxMotionBlurEnabled, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.IgnoreEOSSanctions, DeviceFlag.None, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.SkipSpawnButton, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxShadowQuality, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 1, null, null),
			new PropertyDecl(EnumGamePrefs.QuestProgressionDailyLimit, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 4, -1, 8),
			new PropertyDecl(EnumGamePrefs.OptionsControllerIconStyle, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsShowConsoleButton, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, false, null, null),
			new PropertyDecl(EnumGamePrefs.SaveDataLimitType, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.String, SaveDataLimitType.Unlimited.ToStringCached(), null, null),
			new PropertyDecl(EnumGamePrefs.ServerEACPeerToPeer, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.BiomeProgression, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.StormFreq, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 100, null, null),
			new PropertyDecl(EnumGamePrefs.JarRefund, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.ServerMatchmakingGroup, DeviceFlag.None, EnumType.String, string.Empty, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxUpscalerMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, GameOptionsPlatforms.DefaultUpscalerMode, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxFSRPreset, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 2, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxFOV3P, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, Constants.cDefaultCameraFieldOfView, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxDefaultFirstPersonCamera, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Bool, true, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfx3PCameraMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.CameraRestrictionMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsGfxCameraDistance3P, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Float, 0.25f, null, null),
			new PropertyDecl(EnumGamePrefs.AISmellMode, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 3, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsControlsDefaultQuickAction, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null),
			new PropertyDecl(EnumGamePrefs.OptionsBindingsResetRevision, DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5, EnumType.Int, 0, null, null)
		};
	}

	public static void InitPropertyDeclarations()
	{
		if (s_propertyList != null)
		{
			throw new InvalidOperationException("GamePrefs' property declarations should only be initialized once.");
		}
		initPropertyDecl();
	}

	public static void InitPrefs()
	{
		if (m_Instance != null)
		{
			throw new InvalidOperationException("GamePrefs should only be initialized and loaded once.");
		}
		m_Instance = new GamePrefs();
		m_Instance.Load();
	}

	public static PropertyDecl[] GetPropertyList()
	{
		return s_propertyList;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Load()
	{
		PropertyDecl[] array = s_propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			string key = propertyDecl.name.ToStringCached();
			_ = propertyDecl.name;
			if (!propertyDecl.IsPersistent || !SdPlayerPrefs.HasKey(key))
			{
				SetObjectInternal(propertyDecl.name, propertyDecl.defaultValue);
				continue;
			}
			switch (propertyDecl.type)
			{
			case EnumType.Float:
				SetObjectInternal(propertyDecl.name, SdPlayerPrefs.GetFloat(key));
				break;
			case EnumType.Int:
				SetObjectInternal(propertyDecl.name, SdPlayerPrefs.GetInt(key));
				break;
			case EnumType.String:
			{
				string text2 = SdPlayerPrefs.GetString(key);
				SetObjectInternal(propertyDecl.name, (text2 != null) ? text2 : propertyDecl.defaultValue);
				break;
			}
			case EnumType.Binary:
			{
				string text = SdPlayerPrefs.GetString(key);
				SetObjectInternal(propertyDecl.name, (text != null) ? Utils.FromBase64(text) : propertyDecl.defaultValue);
				break;
			}
			case EnumType.Bool:
				SetObjectInternal(propertyDecl.name, SdPlayerPrefs.GetInt(key) != 0);
				break;
			}
		}
	}

	public void Load(string sdfFileName)
	{
		try
		{
			SdfFile sdfFile = new SdfFile();
			sdfFile.Open(sdfFileName);
			string[] storedGamePrefs = sdfFile.GetStoredGamePrefs();
			foreach (string text in storedGamePrefs)
			{
				EnumGamePrefs enumGamePrefs = EnumGamePrefs.Last;
				try
				{
					enumGamePrefs = EnumUtils.Parse<EnumGamePrefs>(text);
				}
				catch (ArgumentException)
				{
					Log.Warning("Savegame contains unknown option '{0}'. Probably an outdated savegame, ignoring this option!", text);
					continue;
				}
				int num = find(enumGamePrefs);
				if (num == -1)
				{
					return;
				}
				switch (s_propertyList[num].type)
				{
				case EnumType.Float:
				{
					float? num2 = sdfFile.GetFloat(enumGamePrefs.ToStringCached());
					if (num2.HasValue)
					{
						Set(enumGamePrefs, num2.Value);
					}
					break;
				}
				case EnumType.Int:
				{
					int? num3 = sdfFile.GetInt(enumGamePrefs.ToStringCached());
					if (num3.HasValue)
					{
						Set(enumGamePrefs, num3.Value);
					}
					break;
				}
				case EnumType.String:
				{
					string text3 = sdfFile.GetString(enumGamePrefs.ToStringCached());
					if (text3 != null)
					{
						Set(enumGamePrefs, text3);
					}
					break;
				}
				case EnumType.Binary:
				{
					string text2 = sdfFile.GetString(enumGamePrefs.ToStringCached(), isBinary: true);
					if (text2 != null)
					{
						Set(enumGamePrefs, text2);
					}
					break;
				}
				case EnumType.Bool:
				{
					bool? flag = sdfFile.GetBool(enumGamePrefs.ToStringCached());
					if (flag.HasValue)
					{
						Set(enumGamePrefs, flag.Value);
					}
					break;
				}
				}
			}
			sdfFile.Close();
			if (GetInt(EnumGamePrefs.MaxChunkAge) == 0)
			{
				Set(EnumGamePrefs.MaxChunkAge, (int)GetDefault(EnumGamePrefs.MaxChunkAge));
			}
		}
		catch (Exception ex2)
		{
			Log.Error(ex2.Message + "\n" + ex2.StackTrace);
		}
	}

	public void Save(string sdfFileName)
	{
		List<EnumGamePrefs> list = new List<EnumGamePrefs>();
		PropertyDecl[] array = s_propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			list.Add(propertyDecl.name);
		}
		Save(sdfFileName, list);
	}

	public void Save(string sdfFileName, List<EnumGamePrefs> prefsToSave)
	{
		try
		{
			SdfFile sdfFile = new SdfFile();
			sdfFile.Open(sdfFileName);
			PropertyDecl[] array = s_propertyList;
			for (int i = 0; i < array.Length; i++)
			{
				PropertyDecl propertyDecl = array[i];
				if (prefsToSave.Contains(propertyDecl.name))
				{
					switch (propertyDecl.type)
					{
					case EnumType.Float:
						sdfFile.Set(propertyDecl.name.ToStringCached(), GetFloat(propertyDecl.name));
						break;
					case EnumType.Int:
						sdfFile.Set(propertyDecl.name.ToStringCached(), GetInt(propertyDecl.name));
						break;
					case EnumType.String:
						sdfFile.Set(propertyDecl.name.ToStringCached(), GetString(propertyDecl.name));
						break;
					case EnumType.Binary:
						sdfFile.Set(propertyDecl.name.ToStringCached(), GetString(propertyDecl.name), isBinary: true);
						break;
					case EnumType.Bool:
						sdfFile.Set(propertyDecl.name.ToStringCached(), GetBool(propertyDecl.name));
						break;
					}
				}
			}
			sdfFile.Close();
		}
		catch (Exception ex)
		{
			Log.Error(ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void Save()
	{
		PropertyDecl[] array = s_propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			if (propertyDecl.IsPersistent)
			{
				switch (propertyDecl.type)
				{
				case EnumType.Float:
					SdPlayerPrefs.SetFloat(propertyDecl.name.ToStringCached(), GetFloat(propertyDecl.name));
					break;
				case EnumType.Int:
					SdPlayerPrefs.SetInt(propertyDecl.name.ToStringCached(), GetInt(propertyDecl.name));
					break;
				case EnumType.String:
					SdPlayerPrefs.SetString(propertyDecl.name.ToStringCached(), GetString(propertyDecl.name));
					break;
				case EnumType.Binary:
					SdPlayerPrefs.SetString(propertyDecl.name.ToStringCached(), Utils.ToBase64(GetString(propertyDecl.name)));
					break;
				case EnumType.Bool:
					SdPlayerPrefs.SetInt(propertyDecl.name.ToStringCached(), GetBool(propertyDecl.name) ? 1 : 0);
					break;
				}
			}
		}
		SdPlayerPrefs.Save();
		SaveDataUtils.SaveDataManager.CommitAsync();
		Log.Out("Persistent GamePrefs saved");
	}

	public static object Parse(EnumGamePrefs _enum, string _val)
	{
		int num = find(_enum);
		if (num == -1)
		{
			return null;
		}
		switch (s_propertyList[num].type)
		{
		case EnumType.Float:
			return StringParsers.ParseFloat(_val);
		case EnumType.Int:
		{
			if (!int.TryParse(_val, out var result))
			{
				result = 0;
			}
			return result;
		}
		case EnumType.String:
			return _val;
		case EnumType.Binary:
			return _val;
		case EnumType.Bool:
			return StringParsers.ParseBool(_val);
		default:
			return null;
		}
	}

	public static string GetString(EnumGamePrefs _eProperty)
	{
		try
		{
			return (string)GetObject(_eProperty);
		}
		catch (InvalidCastException e)
		{
			Log.Error("GetString: InvalidCastException " + _eProperty.ToStringCached());
			Log.Exception(e);
			return string.Empty;
		}
	}

	public static float GetFloat(EnumGamePrefs _eProperty)
	{
		try
		{
			object obj = GetObject(_eProperty);
			if (obj != null)
			{
				return (float)obj;
			}
			obj = GetDefault(_eProperty);
			if (obj != null)
			{
				return (float)obj;
			}
			Log.Error("GetFloat: GamePref {0}/{1} does not have a value/default", (int)_eProperty, _eProperty.ToStringCached());
			return 0f;
		}
		catch (InvalidCastException e)
		{
			Log.Error("GetFloat: InvalidCastException " + _eProperty.ToStringCached());
			Log.Exception(e);
			return (float)GetDefault(_eProperty);
		}
	}

	public static int GetInt(EnumGamePrefs _eProperty)
	{
		try
		{
			object obj = GetObject(_eProperty);
			if (obj != null)
			{
				return (int)obj;
			}
			obj = GetDefault(_eProperty);
			if (obj != null)
			{
				return (int)obj;
			}
			Log.Error("GetInt: GamePref {0}/{1} does not have a value/default", (int)_eProperty, _eProperty.ToStringCached());
			return 0;
		}
		catch (InvalidCastException e)
		{
			Log.Error("GetInt: InvalidCastException " + _eProperty.ToStringCached());
			Log.Exception(e);
			return 0;
		}
	}

	public static bool GetBool(EnumGamePrefs _eProperty)
	{
		try
		{
			object obj = GetObject(_eProperty);
			if (obj != null)
			{
				return (bool)obj;
			}
			obj = GetDefault(_eProperty);
			if (obj != null)
			{
				return (bool)obj;
			}
			Log.Error("GetBool: GamePref {0}/{1} does not have a value/default", (int)_eProperty, _eProperty.ToStringCached());
			return false;
		}
		catch (InvalidCastException e)
		{
			Log.Error("GetBool: InvalidCastException " + _eProperty.ToStringCached());
			Log.Exception(e);
			return false;
		}
	}

	public static object GetObject(EnumGamePrefs _eProperty)
	{
		int num = (int)_eProperty;
		if (num >= Instance.propertyValues.Length)
		{
			Log.Error("GamePrefs: Trying to access non-existing pref " + num);
			return null;
		}
		return Instance.propertyValues[(int)_eProperty];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int find(EnumGamePrefs _eProperty)
	{
		for (int i = 0; i < s_propertyList.Length; i++)
		{
			if (s_propertyList[i].name == _eProperty)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Exists(EnumGamePrefs _eProperty)
	{
		return find(_eProperty) != -1;
	}

	public static void SetPersistent(EnumGamePrefs _eProperty, bool _bPersistent)
	{
		int num = find(_eProperty);
		if (num == -1)
		{
			Log.Error("Property value " + _eProperty.ToStringCached() + " not found!");
		}
		else if (_bPersistent)
		{
			s_propertyList[num].bPersistent = DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
		else
		{
			s_propertyList[num].bPersistent = DeviceFlag.None;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetObjectInternal(EnumGamePrefs _eProperty, object _value)
	{
		int num = (int)_eProperty;
		if (num >= propertyValues.Length)
		{
			Log.Error("GamePrefs: Trying to set non-existing pref " + num);
		}
		else if ((propertyValues[num] != null || _value != null) && !object.Equals(propertyValues[num], _value))
		{
			propertyValues[num] = _value;
			notifyListeners(_eProperty);
		}
	}

	public static void SetObject(EnumGamePrefs _eProperty, object _value)
	{
		Instance.SetObjectInternal(_eProperty, _value);
	}

	public static void Set(EnumGamePrefs _eProperty, int _value)
	{
		SetObject(_eProperty, _value);
	}

	public static void Set(EnumGamePrefs _eProperty, float _value)
	{
		SetObject(_eProperty, _value);
	}

	public static void Set(EnumGamePrefs _eProperty, string _value)
	{
		SetObject(_eProperty, _value);
	}

	public static void Set(EnumGamePrefs _eProperty, bool _value)
	{
		SetObject(_eProperty, _value);
	}

	public static object[] GetSettingsCopy()
	{
		object[] array = new object[Instance.propertyValues.Length];
		Array.Copy(Instance.propertyValues, array, array.Length);
		return array;
	}

	public static void ApplySettingsCopy(object[] _settings)
	{
		Array.Copy(_settings, Instance.propertyValues, _settings.Length);
	}

	public static bool IsDefault(EnumGamePrefs _eProperty)
	{
		int num = (int)_eProperty;
		if (num >= Instance.propertyValues.Length)
		{
			Log.Error("GamePrefs: Trying to get default of non-existing pref " + num);
			return true;
		}
		if (Instance.propertyValues[num] != null)
		{
			return Instance.propertyValues[num].Equals(GetDefault(_eProperty));
		}
		return false;
	}

	public static object GetDefault(EnumGamePrefs _eProperty)
	{
		PropertyDecl[] array = s_propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			if (propertyDecl.name == _eProperty)
			{
				return propertyDecl.defaultValue;
			}
		}
		return null;
	}

	public static EnumType? GetPrefType(EnumGamePrefs _eProperty)
	{
		PropertyDecl[] array = s_propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl propertyDecl = array[i];
			if (propertyDecl.name == _eProperty)
			{
				return propertyDecl.type;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PrintNonStockWarning(PropertyDecl prop, object curVal, object allowedMin, object allowedMax = null)
	{
		if (allowedMax != null)
		{
			Log.Out($"Setting for '{prop.name.ToStringCached()}' not within the default range (server will go to the modded category): current = {curVal.ToString()}, default = {allowedMin.ToString()} - {allowedMax.ToString()}");
		}
		else
		{
			Log.Out($"Setting for '{prop.name.ToStringCached()}' does not match the default (server will go to the modded category): current = {curVal.ToString()}, default = {allowedMin.ToString()}");
		}
	}

	public static bool HasStockSettings()
	{
		bool result = true;
		PropertyDecl[] array = s_propertyList;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyDecl prop = array[i];
			if (prop.minStockValue == null)
			{
				continue;
			}
			switch (prop.type)
			{
			case EnumType.Float:
			{
				float num4 = GetFloat(prop.name);
				float num5 = (float)prop.minStockValue;
				if (prop.maxStockValue == null)
				{
					if (num4 != num5)
					{
						PrintNonStockWarning(prop, num4, num5);
						result = false;
					}
					break;
				}
				float num6 = (float)prop.maxStockValue;
				if (num4 < num5 || num4 > num6)
				{
					PrintNonStockWarning(prop, num4, num5, num6);
					result = false;
				}
				break;
			}
			case EnumType.Int:
			{
				int num = GetInt(prop.name);
				int num2 = (int)prop.minStockValue;
				if (prop.maxStockValue == null)
				{
					if (num != num2)
					{
						PrintNonStockWarning(prop, num, num2);
						result = false;
					}
					break;
				}
				int num3 = (int)prop.maxStockValue;
				if (num < num2 || num > num3)
				{
					PrintNonStockWarning(prop, num, num2, num3);
					result = false;
				}
				break;
			}
			case EnumType.String:
			case EnumType.Binary:
			{
				string text = GetString(prop.name);
				string text2 = (string)prop.minStockValue;
				if (!text.Equals(text2))
				{
					PrintNonStockWarning(prop, text, text2);
					result = false;
				}
				break;
			}
			case EnumType.Bool:
			{
				bool flag = GetBool(prop.name);
				bool flag2 = (bool)prop.minStockValue;
				if (flag != flag2)
				{
					PrintNonStockWarning(prop, flag, flag2);
					result = false;
				}
				break;
			}
			}
		}
		return result;
	}

	public static void AddChangeListener(IGamePrefsChangedListener _listener)
	{
		listeners.Add(_listener);
	}

	public static void RemoveChangeListener(IGamePrefsChangedListener _listener)
	{
		listeners.Remove(_listener);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void notifyListeners(EnumGamePrefs _enum)
	{
		for (int i = 0; i < listeners.Count; i++)
		{
			listeners[i].OnGamePrefChanged(_enum);
		}
		GamePrefs.OnGamePrefChanged?.Invoke(_enum);
	}
}
