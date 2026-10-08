using System;
using System.Reflection;
using HarmonyLib;

namespace AGF.Compat.DoomSurvival
{
	/// <summary>
	/// In a Doom level, MapPlus header shows loc doom_*_name (e.g. E1M1: Hangar)
	/// and POI bounds stay hidden. Reflection only — loads without MapPlus or DoomLevels.
	/// </summary>
	[HarmonyPatch]
	public static class Patch_MapPlus_DoomLevelHover
	{
		static Type _hover;
		static Type _gameMap;
		static Type _automap;
		static PropertyInfo _inLevel;
		static PropertyInfo _automapLevel;
		static FieldInfo _levelName;
		static MethodInfo _bindViews;
		static MethodInfo _applyLabel;
		static MethodInfo _applyBounds;
		static MethodInfo _currentNameSet;
		static bool _doomTypesChecked;

		static bool Prepare()
		{
			_hover = AccessTools.TypeByName("MapPlus.MapPlusHover");
			if (_hover == null)
			{
				return false;
			}

			_bindViews = AccessTools.Method(_hover, "BindViews");
			_applyLabel = AccessTools.Method(_hover, "ApplyLabel");
			_applyBounds = AccessTools.Method(_hover, "ApplyBounds");
			_currentNameSet = AccessTools.PropertySetter(_hover, "CurrentDisplayName");
			return _bindViews != null && _applyLabel != null && _applyBounds != null && _currentNameSet != null;
		}

		static MethodBase TargetMethod()
		{
			return AccessTools.Method(_hover, "RefreshFromMap");
		}

		static bool Prefix(XUiC_MapArea mapArea)
		{
			try
			{
				if (!InDoomLevel())
				{
					return true;
				}

				string mapName = GetDoomMapName();
				_currentNameSet.Invoke(null, new object[] { mapName });
				_bindViews.Invoke(null, new object[] { mapArea });
				_applyLabel.Invoke(null, new object[] { mapName });
				_applyBounds.Invoke(null, new object[] { mapArea, null });
				mapArea?.RefreshBindings();
				return false;
			}
			catch
			{
				return true;
			}
		}

		static bool InDoomLevel()
		{
			EnsureDoomTypes();
			if (_inLevel == null)
			{
				return false;
			}

			object value = _inLevel.GetValue(null);
			return value is bool inLevel && inLevel;
		}

		static string GetDoomMapName()
		{
			object level = _automapLevel?.GetValue(null);
			if (level == null)
			{
				return string.Empty;
			}

			if (_levelName == null || _levelName.DeclaringType != level.GetType())
			{
				_levelName = AccessTools.Field(level.GetType(), "Name");
			}

			string code = _levelName?.GetValue(level) as string ?? string.Empty;
			if (string.IsNullOrEmpty(code))
			{
				return string.Empty;
			}

			string locKey = "doom_" + code.ToLowerInvariant() + "_name";
			if (Localization.Exists(locKey))
			{
				string localized = Localization.Get(locKey);
				if (!string.IsNullOrEmpty(localized) && localized != locKey)
				{
					return localized;
				}
			}

			return code;
		}

		static void EnsureDoomTypes()
		{
			if (_doomTypesChecked)
			{
				return;
			}

			_doomTypesChecked = true;
			_gameMap = FindType("DoomLevels.GameMap");
			_automap = FindType("DoomLevels.Automap");
			_inLevel = _gameMap == null ? null : AccessTools.Property(_gameMap, "InLevel");
			_automapLevel = _automap == null ? null : AccessTools.Property(_automap, "Level");
		}

		static Type FindType(string fullName)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(fullName, false);
				if (type != null)
				{
					return type;
				}
			}

			return null;
		}
	}
}
