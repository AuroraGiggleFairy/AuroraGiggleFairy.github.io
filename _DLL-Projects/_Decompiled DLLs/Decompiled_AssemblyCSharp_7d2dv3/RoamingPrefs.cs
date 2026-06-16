using System.IO;
using Platform;

public static class RoamingPrefs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly EnumGamePrefs[] s_gamePrefs = new EnumGamePrefs[59]
	{
		EnumGamePrefs.OptionsAmbientVolumeLevel,
		EnumGamePrefs.OptionsDynamicMusicEnabled,
		EnumGamePrefs.OptionsDynamicMusicDailyTime,
		EnumGamePrefs.OptionsMusicVolumeLevel,
		EnumGamePrefs.OptionsMenuMusicVolumeLevel,
		EnumGamePrefs.OptionsOverallAudioVolumeLevel,
		EnumGamePrefs.OptionsSubtitlesEnabled,
		EnumGamePrefs.OptionsFilterProfanity,
		EnumGamePrefs.OptionsLookSensitivity,
		EnumGamePrefs.OptionsZoomSensitivity,
		EnumGamePrefs.OptionsZoomAccel,
		EnumGamePrefs.OptionsVehicleLookSensitivity,
		EnumGamePrefs.OptionsWeaponAiming,
		EnumGamePrefs.OptionsInvertMouse,
		EnumGamePrefs.OptionsControlsSprintLock,
		EnumGamePrefs.OptionsControlsDefaultQuickAction,
		EnumGamePrefs.OptionsControllerIconStyle,
		EnumGamePrefs.OptionsControllerJoystickLayout,
		EnumGamePrefs.OptionsControllerSensitivityX,
		EnumGamePrefs.OptionsControllerSensitivityY,
		EnumGamePrefs.OptionsControllerZoomSensitivity,
		EnumGamePrefs.OptionsControllerVehicleSensitivity,
		EnumGamePrefs.OptionsControllerLookInvert,
		EnumGamePrefs.OptionsControllerLookAcceleration,
		EnumGamePrefs.OptionsControllerLookAxisDeadzone,
		EnumGamePrefs.OptionsControllerMoveAxisDeadzone,
		EnumGamePrefs.OptionsControllerAimAssists,
		EnumGamePrefs.OptionsControllerWeaponAiming,
		EnumGamePrefs.OptionsControllerVibration,
		EnumGamePrefs.OptionsControllerVibrationStrength,
		EnumGamePrefs.OptionsControllerTriggerEffects,
		EnumGamePrefs.OptionsControllerCursorHoverSensitivity,
		EnumGamePrefs.OptionsControllerCursorSnap,
		EnumGamePrefs.ConnectToServerIP,
		EnumGamePrefs.ConnectToServerPort,
		EnumGamePrefs.LanguageBrowser,
		EnumGamePrefs.Region,
		EnumGamePrefs.ServerPasswordCache,
		EnumGamePrefs.FavoriteServersList,
		EnumGamePrefs.ServerHistoryCache,
		EnumGamePrefs.Language,
		EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections,
		EnumGamePrefs.OptionsTempCelsius,
		EnumGamePrefs.OptionsDisableXmlEvents,
		EnumGamePrefs.OptionsQuestsAutoShare,
		EnumGamePrefs.OptionsQuestsAutoAccept,
		EnumGamePrefs.OptionsAutoPartyWithFriends,
		EnumGamePrefs.OptionsChatCommunication,
		EnumGamePrefs.OptionsCrossplay,
		EnumGamePrefs.OptionsShowConsoleButton,
		EnumGamePrefs.OptionsCrosshairEnabled,
		EnumGamePrefs.OptionsCrosshairScale,
		EnumGamePrefs.OptionsCrosshairColor,
		EnumGamePrefs.OptionsCrosshairOpacity,
		EnumGamePrefs.OptionsCrosshairADS,
		EnumGamePrefs.OptionsCrosshairDot3P,
		EnumGamePrefs.OptionsCrosshairThickness,
		EnumGamePrefs.OptionsCrosshairRangedEnabled,
		EnumGamePrefs.EulaVersionAccepted
	};

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static PrefVersionStore Store
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static void Init()
	{
		if (!PlatformManager.MultiPlatform.UserDataRoaming.IsSupported || !PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
		{
			return;
		}
		Store = new PrefVersionStore(Path.Combine(GameIO.GetRoamingUserGameDataDir(), "RoamingPlayerPrefs"));
		EnumGamePrefs[] array = s_gamePrefs;
		foreach (EnumGamePrefs enumGamePrefs in array)
		{
			if (GameOptionsReset.TryGetPrefType(enumGamePrefs, out var prefType))
			{
				string groupId = GameOptionsReset.GetGroupId(enumGamePrefs);
				Store.AddPref(groupId, enumGamePrefs.ToStringCached(), prefType);
			}
		}
		foreach (var actionSetPref in GameOptionsControls.ActionSetPrefs)
		{
			string item = actionSetPref.Item2;
			Store.AddPref(GameOptionsReset.Bindings.VersionId, item, PrefType.String);
		}
		Store.AddPref(GameOptionsReset.Bindings.VersionId, "ActionSetsSaved", PrefType.Int);
		Store.AddPref(GameOptionsReset.Bindings.VersionId, "Controls", PrefType.String);
		Log.Out("[RoamingPrefs] initialized");
	}

	public static void Destroy()
	{
		Store = null;
	}
}
