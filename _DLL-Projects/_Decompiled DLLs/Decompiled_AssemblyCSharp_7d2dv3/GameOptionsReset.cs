using System;
using System.Collections.Generic;

public static class GameOptionsReset
{
	public interface IGroup
	{
		string VersionId { get; }

		bool HasPref(string prefName);

		bool NeedsReset();

		void Reset(PrefVersionStore versionedPrefs);
	}

	public abstract class EnumGamePrefGroup : IGroup
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<EnumGamePrefs> EnumPrefs = new HashSet<EnumGamePrefs>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<string> PrefNames = new HashSet<string>();

		public abstract string VersionId { get; }

		[PublicizedFrom(EAccessModifier.Protected)]
		public EnumGamePrefGroup(params EnumGamePrefs[] enumGamePrefs)
		{
			foreach (EnumGamePrefs enumGamePref in enumGamePrefs)
			{
				AddEnumGamePref(enumGamePref);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void AddEnumGamePref(EnumGamePrefs enumGamePref)
		{
			if (EnumPrefs.Add(enumGamePref))
			{
				PrefNames.Add(enumGamePref.ToStringCached());
			}
		}

		public virtual bool HasPref(string prefName)
		{
			return PrefNames.Contains(prefName);
		}

		public virtual bool NeedsReset()
		{
			return false;
		}

		public virtual void Reset(PrefVersionStore versionedPrefs)
		{
			foreach (EnumGamePrefs enumPref in EnumPrefs)
			{
				ResetPref(enumPref, versionedPrefs);
			}
		}
	}

	public class AudioGroup : EnumGamePrefGroup
	{
		public override string VersionId => $"Audio_{13}_0";

		public AudioGroup()
			: base(EnumGamePrefs.OptionsAmbientVolumeLevel, EnumGamePrefs.OptionsMusicVolumeLevel, EnumGamePrefs.OptionsMenuMusicVolumeLevel, EnumGamePrefs.OptionsDynamicMusicEnabled, EnumGamePrefs.OptionsDynamicMusicDailyTime, EnumGamePrefs.OptionsMicVolumeLevel, EnumGamePrefs.OptionsVoiceVolumeLevel, EnumGamePrefs.OptionsOverallAudioVolumeLevel, EnumGamePrefs.OptionsVoiceChatEnabled, EnumGamePrefs.OptionsAudioOcclusion, EnumGamePrefs.OptionsSubtitlesEnabled)
		{
		}

		public override void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options AudioGroup");
			base.Reset(versionedPrefs);
		}
	}

	public class GraphicsGroup : EnumGamePrefGroup
	{
		public override string VersionId => $"Graphics_{13}_{4}";

		public GraphicsGroup()
			: base(EnumGamePrefs.OptionsGfxResolution, EnumGamePrefs.OptionsGfxVsync, EnumGamePrefs.OptionsGfxDynamicMode, EnumGamePrefs.OptionsGfxUpscalerMode, EnumGamePrefs.OptionsGfxFSRPreset, EnumGamePrefs.OptionsGfxDynamicScale, EnumGamePrefs.OptionsGfxBrightness, EnumGamePrefs.OptionsGfxSignQuality, EnumGamePrefs.DynamicMeshEnabled, EnumGamePrefs.DynamicMeshDistance, EnumGamePrefs.NoGraphicsMode)
		{
			AddEnumGamePref(EnumGamePrefs.OptionsGfxQualityPreset);
			foreach (EnumGamePrefs key in GameOptionsManager.QualityPresets.Keys)
			{
				AddEnumGamePref(key);
			}
		}

		public override bool NeedsReset()
		{
			return 4 != GamePrefs.GetInt(EnumGamePrefs.OptionsGfxResetRevision);
		}

		public override void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options GraphicsGroup");
			base.Reset(versionedPrefs);
			GameOptionsPlatforms.GfxPreset value = GameOptionsPlatforms.CalcDefaultGfxPreset();
			GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, (int)value);
			GameOptionsManager.SetGraphicsQuality();
			GamePrefs.Set(EnumGamePrefs.OptionsGfxResetRevision, 4);
		}
	}

	public class ControlsGroup : EnumGamePrefGroup
	{
		public override string VersionId => $"Controls_{13}_{7}";

		public ControlsGroup()
			: base(EnumGamePrefs.OptionsLookSensitivity, EnumGamePrefs.OptionsZoomSensitivity, EnumGamePrefs.OptionsZoomAccel, EnumGamePrefs.OptionsInvertMouse, EnumGamePrefs.OptionsVehicleLookSensitivity, EnumGamePrefs.OptionsControlsSprintLock)
		{
		}

		public override bool NeedsReset()
		{
			return 7 != GamePrefs.GetInt(EnumGamePrefs.OptionsControlsResetRevision);
		}

		public override void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options ControlsGroup");
			base.Reset(versionedPrefs);
			GamePrefs.Set(EnumGamePrefs.OptionsControlsResetRevision, 7);
		}
	}

	public class ControllerGroup : EnumGamePrefGroup
	{
		public override string VersionId => $"Controller_{13}_0";

		public ControllerGroup()
			: base(EnumGamePrefs.OptionsAllowController, EnumGamePrefs.OptionsControllerVibration, EnumGamePrefs.OptionsInterfaceSensitivity, EnumGamePrefs.OptionsControllerSensitivityX, EnumGamePrefs.OptionsControllerSensitivityY, EnumGamePrefs.OptionsControllerLookInvert, EnumGamePrefs.OptionsControllerJoystickLayout, EnumGamePrefs.OptionsControllerLookAcceleration, EnumGamePrefs.OptionsControllerZoomSensitivity, EnumGamePrefs.OptionsControllerLookAxisDeadzone, EnumGamePrefs.OptionsControllerMoveAxisDeadzone, EnumGamePrefs.OptionsControllerCursorSnap, EnumGamePrefs.OptionsControllerCursorHoverSensitivity, EnumGamePrefs.OptionsControllerVehicleSensitivity, EnumGamePrefs.OptionsControllerAimAssists, EnumGamePrefs.OptionsControllerWeaponAiming, EnumGamePrefs.OptionsControlsSprintLock, EnumGamePrefs.OptionsControllerTriggerEffects, EnumGamePrefs.OptionsControllerIconStyle)
		{
		}

		public override void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options ControllerGroup");
			base.Reset(versionedPrefs);
		}
	}

	public class BindingsGroup : IGroup
	{
		public string VersionId => $"Bindings_{13}_{1}";

		public bool HasPref(string prefName)
		{
			if (prefName.Equals("Controls"))
			{
				return true;
			}
			if (prefName.Equals("ActionSetsSaved"))
			{
				return true;
			}
			if (!prefName.StartsWith("ActionSet_"))
			{
				return false;
			}
			foreach (var actionSetPref in GameOptionsControls.ActionSetPrefs)
			{
				string item = actionSetPref.Item2;
				if (prefName.Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		public bool NeedsReset()
		{
			return 1 != GamePrefs.GetInt(EnumGamePrefs.OptionsBindingsResetRevision);
		}

		public void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options BindingsGroup");
			foreach (var (playerActionsBase, key) in GameOptionsControls.ActionSetPrefs)
			{
				if (versionedPrefs != null && versionedPrefs.TryGetString(key, out var value) && !string.IsNullOrEmpty(value))
				{
					playerActionsBase.Load(value);
				}
				else
				{
					playerActionsBase.Reset();
				}
			}
			GameOptionsControls.Save();
			GamePrefs.Set(EnumGamePrefs.OptionsBindingsResetRevision, 1);
		}
	}

	public static AudioGroup Audio;

	public static GraphicsGroup Graphics;

	public static ControlsGroup Controls;

	public static ControllerGroup Controller;

	public static BindingsGroup Bindings;

	public static List<IGroup> groups;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultGroupId = $"DefaultGroup_{13}";

	public static void Init()
	{
		Audio = new AudioGroup();
		Graphics = new GraphicsGroup();
		Controls = new ControlsGroup();
		Controller = new ControllerGroup();
		Bindings = new BindingsGroup();
		groups = new List<IGroup> { Audio, Graphics, Controls, Controller, Bindings };
	}

	public static string GetGroupId(EnumGamePrefs enumGamePref)
	{
		return GetGroupId(enumGamePref.ToStringCached());
	}

	public static string GetGroupId(string prefName)
	{
		foreach (IGroup group in groups)
		{
			if (group.HasPref(prefName))
			{
				return group.VersionId;
			}
		}
		return defaultGroupId;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefType EnumTypeToPrefType(GamePrefs.EnumType enumType)
	{
		return enumType switch
		{
			GamePrefs.EnumType.Int => PrefType.Int, 
			GamePrefs.EnumType.Float => PrefType.Float, 
			GamePrefs.EnumType.String => PrefType.String, 
			GamePrefs.EnumType.Bool => PrefType.Int, 
			GamePrefs.EnumType.Binary => PrefType.String, 
			_ => throw new Exception(string.Format("Unmapped {0} for {1}: {2}", "PrefType", "EnumType", enumType)), 
		};
	}

	public static bool TryGetPrefType(EnumGamePrefs enumGamePref, out PrefType prefType)
	{
		GamePrefs.EnumType? prefType2 = GamePrefs.GetPrefType(enumGamePref);
		if (!prefType2.HasValue)
		{
			Log.Error($"Unknown enum type for {enumGamePref}");
			prefType = PrefType.Float;
			return false;
		}
		prefType = EnumTypeToPrefType(prefType2.Value);
		return true;
	}

	public static bool NeedsResetGame()
	{
		return 13 != GamePrefs.GetInt(EnumGamePrefs.LastGameResetRevision);
	}

	public static void ResetGame(PrefVersionStore versionedPrefs)
	{
		GamePrefs.PropertyDecl[] propertyList = GamePrefs.GetPropertyList();
		for (int i = 0; i < propertyList.Length; i++)
		{
			ResetPref(propertyList[i].name, versionedPrefs);
		}
		Graphics.Reset(versionedPrefs);
		Controls.Reset(versionedPrefs);
		Bindings.Reset(versionedPrefs);
		GamePrefs.Set(EnumGamePrefs.LastGameResetRevision, 13);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ResetPref(EnumGamePrefs enumGamePref, PrefVersionStore versionedPrefs)
	{
		if (versionedPrefs == null || !versionedPrefs.TryGetGamePref(enumGamePref, out var value))
		{
			value = GamePrefs.GetDefault(enumGamePref);
		}
		GamePrefs.SetObject(enumGamePref, value);
	}
}
