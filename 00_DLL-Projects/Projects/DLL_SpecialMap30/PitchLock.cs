using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SpecialMap30
{
	internal static class PitchLock
	{
		public static string ModPath;

		static string _mapName = "MAP30";
		static int _minX = 39;
		static int _maxX = 44;
		static int _minZ = 57;
		static int _maxZ = 61;

		static bool _loggedInZone;
		static bool _doomMissingLogged;
		static bool _instancesMissingLogged;

		static MethodInfo _tryGet;
		static MethodInfo _mapFor;
		static PropertyInfo _shellProp;
		static FieldInfo _levelNameField;
		static PropertyInfo _levelNameProp;
		static bool _resolved;

		public static string DescribeZone()
		{
			return $"{_mapName} x={_minX}..{_maxX} z={_minZ}..{_maxZ} (any Y)";
		}

		public static void LoadZone()
		{
			string path = ModPath == null ? null : Path.Combine(ModPath, "Config", "zone.txt");
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
				return;

			foreach (var raw in File.ReadAllLines(path))
			{
				var line = raw.Trim();
				if (line.Length == 0 || line.StartsWith("#"))
					continue;
				int eq = line.IndexOf('=');
				if (eq <= 0)
					continue;
				string key = line.Substring(0, eq).Trim();
				string val = line.Substring(eq + 1).Trim();
				if (key.Equals("map", StringComparison.OrdinalIgnoreCase))
					_mapName = val;
				else if (key.Equals("minX", StringComparison.OrdinalIgnoreCase))
					int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out _minX);
				else if (key.Equals("maxX", StringComparison.OrdinalIgnoreCase))
					int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out _maxX);
				else if (key.Equals("minZ", StringComparison.OrdinalIgnoreCase))
					int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out _minZ);
				else if (key.Equals("maxZ", StringComparison.OrdinalIgnoreCase))
					int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out _maxZ);
			}

			if (_minX > _maxX)
			{
				int t = _minX;
				_minX = _maxX;
				_maxX = t;
			}
			if (_minZ > _maxZ)
			{
				int t = _minZ;
				_minZ = _maxZ;
				_maxZ = t;
			}
		}

		public static void Apply(vp_FPCamera cam)
		{
			if (cam == null)
				return;
			if (!InLockZone(cam, out int localX, out int localZ))
			{
				if (_loggedInZone)
				{
					_loggedInZone = false;
					Log("left pitch-lock zone");
				}
				return;
			}

			cam.Pitch = 0f;
			if (!_loggedInZone)
			{
				_loggedInZone = true;
				Log($"entered pitch-lock zone at local {localX},y,{localZ}");
			}
		}

		static bool InLockZone(vp_FPCamera cam, out int localX, out int localZ)
		{
			localX = 0;
			localZ = 0;

			var player = PlayerForCamera(cam);
			if (player == null)
				return false;

			Vector3 pos = player.position;
			// DoomLevels instances sit around 100000,100000 (see Instances.IsInstanceSpace).
			if (pos.x < 99488f || pos.z < 99488f)
				return false;

			if (!TryGetInstance(player.entityId, out Vector3i origin, out string mapName, out int shell))
				return false;
			if (!string.Equals(mapName, _mapName, StringComparison.OrdinalIgnoreCase))
				return false;

			Vector3i block = World.worldToBlockPos(pos);
			localX = block.x - origin.x - shell;
			localZ = block.z - origin.z - shell;
			return localX >= _minX && localX <= _maxX && localZ >= _minZ && localZ <= _maxZ;
		}

		static EntityPlayerLocal PlayerForCamera(vp_FPCamera cam)
		{
			var world = GameManager.Instance?.World;
			if (world == null)
				return null;

			var primary = world.GetPrimaryPlayer();
			if (primary != null && primary.vp_FPCamera == cam)
				return primary;

			var list = world.Players?.list;
			if (list == null)
				return null;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is EntityPlayerLocal local && local.vp_FPCamera == cam)
					return local;
			}
			return null;
		}

		static bool TryGetInstance(int playerId, out Vector3i origin, out string mapName, out int shell)
		{
			origin = Vector3i.zero;
			mapName = null;
			shell = 2;

			ResolveDoomLevels();
			if (_tryGet == null)
				return false;

			try
			{
				object[] args = { playerId, Vector3i.zero, null, 0 };
				object ok = _tryGet.Invoke(null, args);
				if (!(ok is bool found) || !found)
					return false;

				origin = args[1] is Vector3i o ? o : Vector3i.zero;
				mapName = ReadLevelName(args[2]);
				if (_shellProp != null)
					shell = Convert.ToInt32(_shellProp.GetValue(null, null));
				if (string.IsNullOrEmpty(mapName) && _mapFor != null)
					mapName = _mapFor.Invoke(null, new object[] { playerId }) as string;
				return !string.IsNullOrEmpty(mapName);
			}
			catch (Exception ex)
			{
				if (!_doomMissingLogged)
				{
					_doomMissingLogged = true;
					Log("DoomLevels.Instances.TryGet failed: " + ex.Message);
				}
				return false;
			}
		}

		static string ReadLevelName(object level)
		{
			if (level == null)
				return null;
			if (_levelNameField != null)
				return _levelNameField.GetValue(level) as string;
			if (_levelNameProp != null)
				return _levelNameProp.GetValue(level, null) as string;

			var t = level.GetType();
			_levelNameField = t.GetField("Name", BindingFlags.Public | BindingFlags.Instance);
			_levelNameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
			return _levelNameField?.GetValue(level) as string
				?? _levelNameProp?.GetValue(level, null) as string;
		}

		static void ResolveDoomLevels()
		{
			if (_resolved)
				return;
			_resolved = true;

			var instances = AccessTools.TypeByName("DoomLevels.Instances");
			var levelDb = AccessTools.TypeByName("DoomLevels.LevelDb");
			if (instances == null)
			{
				if (!_instancesMissingLogged)
				{
					_instancesMissingLogged = true;
					Log("DoomLevels.Instances not loaded yet (is DoomClassicMaps enabled?)");
				}
				_resolved = false;
				return;
			}

			_tryGet = AccessTools.Method(instances, "TryGet");
			_mapFor = AccessTools.Method(instances, "MapFor", new[] { typeof(int) });
			_shellProp = levelDb == null ? null : AccessTools.Property(levelDb, "Shell");
			if (_tryGet == null)
				Log("DoomLevels.Instances.TryGet not found");
		}

		static void Log(string msg)
		{
			var line = "[SpecialMap30] " + msg;
			Console.WriteLine(line);
			try { Debug.Log(line); } catch { /* ignore */ }
		}
	}

	[HarmonyPatch(typeof(vp_FPCamera), "UpdateInput")]
	static class Patch_FPCamera_UpdateInput
	{
		static void Postfix(vp_FPCamera __instance)
		{
			PitchLock.Apply(__instance);
		}
	}

	[HarmonyPatch(typeof(vp_FPCamera), "LateUpdate")]
	static class Patch_FPCamera_LateUpdate
	{
		static void Prefix(vp_FPCamera __instance)
		{
			PitchLock.Apply(__instance);
		}
	}
}
