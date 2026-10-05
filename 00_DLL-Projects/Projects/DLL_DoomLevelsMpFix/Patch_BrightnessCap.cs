using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// While this Doom process is running, brightness stays at 60% or below.
	/// A higher saved value is remembered and written back when prefs are saved,
	/// so the regular game still opens at that higher setting.
	/// </summary>
	internal static class BrightnessCap
	{
		internal const float Max = 0.6f;

		private const float Epsilon = 0.001f;

		private static readonly FieldInfo ComboField = AccessTools.Field(typeof(XUiC_OptionEntryGamePrefFloat), "combo");

		private static bool _active;

		private static bool _writing;

		private static bool _saving;

		private static bool _announcedHold;

		private static float? _savedAboveMax;

		internal static void Apply()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}

			_active = true;
			float current;
			try
			{
				current = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[DoomMultiplayer] brightness read: " + e.Message);
				return;
			}

			if (current > Max + Epsilon)
			{
				if (!_savedAboveMax.HasValue)
				{
					_savedAboveMax = current;
				}

				Write(Max);
				Refresh();
				if (!_announcedHold)
				{
					_announcedHold = true;
					int saved = Mathf.RoundToInt(_savedAboveMax.Value * 100f);
					Debug.Log("[DoomMultiplayer] brightness held at 60% until Doom closes (saved " + saved + "%)");
				}

			}
		}

		internal static void NoteWrite(ref float value)
		{
			if (!_active || _writing)
			{
				return;
			}

			if (value > Max + Epsilon)
			{
				if (!_savedAboveMax.HasValue)
				{
					_savedAboveMax = value;
					if (!_announcedHold)
					{
						_announcedHold = true;
						int saved = Mathf.RoundToInt(value * 100f);
						Debug.Log("[DoomMultiplayer] brightness held at 60% until Doom closes (saved " + saved + "%)");
					}
				}

				value = Max;
				return;
			}

			if (value < Max - Epsilon)
			{
				_savedAboveMax = null;
			}
		}

		internal static void BeginSave()
		{
			if (_active && _savedAboveMax.HasValue)
			{
				_saving = true;
			}
		}

		internal static void EndSave()
		{
			_saving = false;
		}

		internal static void NoteSavedFloat(string key, ref float value)
		{
			if (!_saving || !_savedAboveMax.HasValue || !IsBrightnessKey(key))
			{
				return;
			}

			value = _savedAboveMax.Value;
		}

		internal static void CapSlider(XUiC_OptionEntryGamePrefFloat entry)
		{
			if (!_active || entry == null || entry.GamePref != EnumGamePrefs.OptionsGfxBrightness)
			{
				return;
			}

			if (!(ComboField?.GetValue(entry) is XUiC_ComboBoxFloat combo))
			{
				return;
			}

			combo.Max = Max;
			if (combo.Value > Max)
			{
				combo.Value = Max;
			}
		}

		internal static void Refresh()
		{
			if (!_active || GameManager.IsDedicatedServer || GameManager.Instance == null)
			{
				return;
			}

			try
			{
				GameRenderManager.ApplyCameraOptions(null);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[DoomMultiplayer] brightness refresh: " + e.Message);
			}
		}

		private static void Write(float value)
		{
			_writing = true;
			try
			{
				GamePrefs.Set(EnumGamePrefs.OptionsGfxBrightness, value);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[DoomMultiplayer] brightness write: " + e.Message);
			}
			finally
			{
				_writing = false;
			}
		}

		private static bool IsBrightnessKey(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}

			return key == "OptionsGfxBrightness"
				|| key == EnumGamePrefs.OptionsGfxBrightness.ToStringCached();
		}
	}

	[HarmonyPatch(typeof(GamePrefs), nameof(GamePrefs.Set), new[] { typeof(EnumGamePrefs), typeof(float) })]
	internal static class Patch_BrightnessSet
	{
		private static void Prefix(EnumGamePrefs _eProperty, ref float _value)
		{
			if (_eProperty == EnumGamePrefs.OptionsGfxBrightness)
			{
				BrightnessCap.NoteWrite(ref _value);
			}
		}
	}

	[HarmonyPatch(typeof(GamePrefs), nameof(GamePrefs.SetObject))]
	internal static class Patch_BrightnessSetObject
	{
		private static void Prefix(EnumGamePrefs _eProperty, ref object _value)
		{
			if (_eProperty != EnumGamePrefs.OptionsGfxBrightness || !(_value is float value))
			{
				return;
			}

			BrightnessCap.NoteWrite(ref value);
			_value = value;
		}
	}

	[HarmonyPatch(typeof(GamePrefs), nameof(GamePrefs.Save), new Type[] { })]
	internal static class Patch_BrightnessSave
	{
		private static void Prefix()
		{
			BrightnessCap.BeginSave();
		}

		private static void Postfix()
		{
			BrightnessCap.EndSave();
		}

		private static Exception Finalizer()
		{
			BrightnessCap.EndSave();
			return null;
		}
	}

	[HarmonyPatch(typeof(SdPlayerPrefs), nameof(SdPlayerPrefs.SetFloat))]
	internal static class Patch_BrightnessPrefsWrite
	{
		private static void Prefix(string key, ref float value)
		{
			BrightnessCap.NoteSavedFloat(key, ref value);
		}
	}

	[HarmonyPatch(typeof(XUiC_OptionEntryGamePrefFloat), "initCurrentValue")]
	internal static class Patch_BrightnessSlider
	{
		private static void Postfix(XUiC_OptionEntryGamePrefFloat __instance)
		{
			BrightnessCap.CapSlider(__instance);
		}
	}
}
