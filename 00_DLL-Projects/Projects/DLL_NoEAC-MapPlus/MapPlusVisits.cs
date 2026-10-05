using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Platform;

namespace MapPlus
{
	/// <summary>
	/// Per-player, per-save list of POI instances entered after this mod was installed.
	/// </summary>
	public static class MapPlusVisits
	{
		public const string IgnoreTags = "part,streettile,navonly,hideui";

		static FastTags<TagGroup.Poi> ignorePoiTags;
		static bool ignorePoiTagsReady;
		static readonly HashSet<string> VisitedKeys = new HashSet<string>(StringComparer.Ordinal);
		static readonly object Sync = new object();

		static string loadedPath;
		static bool dirty;

		public static bool IsNameablePrefab(PrefabInstance prefabInstance)
		{
			if (prefabInstance?.prefab == null)
			{
				return false;
			}

			Prefab prefab = prefabInstance.prefab;
			if (prefab.Tags.Test_AnySet(GetIgnoreTags()))
			{
				return false;
			}

			string prefabName = prefab.PrefabName;
			if (string.IsNullOrEmpty(prefabName))
			{
				return false;
			}

			if (prefabName.IndexOf("rwg_tile", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return false;
			}

			if (prefabName.StartsWith("part_", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return Localization.Exists(prefabName);
		}

		public static string MakeKey(PrefabInstance prefabInstance)
		{
			if (prefabInstance?.prefab == null)
			{
				return null;
			}

			string name = prefabInstance.prefab.PrefabName;
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			Vector3i pos = prefabInstance.boundingBoxPosition;
			return pos.x + "," + pos.y + "," + pos.z + ":" + name;
		}

		public static bool HasVisited(PrefabInstance prefabInstance)
		{
			string key = MakeKey(prefabInstance);
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}

			EnsureLoaded();
			lock (Sync)
			{
				if (VisitedKeys.Contains(key))
				{
					return true;
				}

				// Older saves put a load-order id in front of the same position and name.
				string suffix = ":" + key;
				foreach (string line in VisitedKeys)
				{
					if (line != null && line.EndsWith(suffix, StringComparison.Ordinal))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static void RecordIfEntered(PrefabInstance prefabInstance)
		{
			if (!IsNameablePrefab(prefabInstance))
			{
				return;
			}

			string key = MakeKey(prefabInstance);
			if (string.IsNullOrEmpty(key))
			{
				return;
			}

			EnsureLoaded();
			lock (Sync)
			{
				if (!VisitedKeys.Add(key))
				{
					return;
				}

				dirty = true;
			}
		}

		static FastTags<TagGroup.Poi> GetIgnoreTags()
		{
			if (!ignorePoiTagsReady)
			{
				ignorePoiTags = FastTags<TagGroup.Poi>.Parse(IgnoreTags);
				ignorePoiTagsReady = true;
			}

			return ignorePoiTags;
		}

		public static void Save()
		{
			EnsureLoaded();

			string path = GetSavePath();
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			List<string> lines;
			lock (Sync)
			{
				if (!dirty)
				{
					return;
				}

				lines = new List<string>(VisitedKeys);
				dirty = false;
				loadedPath = path;
			}

			WriteVisitedFile(path, lines);
		}

		static void WriteVisitedFile(string path, List<string> lines)
		{
			try
			{
				string directory = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				File.WriteAllLines(path, lines, Encoding.UTF8);
			}
			catch (Exception ex)
			{
				Console.WriteLine("MapPlus: Failed to save visited POIs: " + ex.Message);
			}
		}

		static void EnsureLoaded()
		{
			string path = GetSavePath();
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			lock (Sync)
			{
				if (string.Equals(path, loadedPath, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				VisitedKeys.Clear();
				loadedPath = path;
				dirty = false;
			}

			if (!File.Exists(path))
			{
				return;
			}

			try
			{
				string[] lines = File.ReadAllLines(path, Encoding.UTF8);
				lock (Sync)
				{
					for (int i = 0; i < lines.Length; i++)
					{
						string line = lines[i];
						if (!string.IsNullOrWhiteSpace(line))
						{
							VisitedKeys.Add(line.Trim());
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("MapPlus: Failed to load visited POIs: " + ex.Message);
			}
		}

		static string GetSavePath()
		{
			string saveDir = GetSaveDirectory();
			if (string.IsNullOrEmpty(saveDir))
			{
				return null;
			}

			return Path.Combine(saveDir, "AGF-NoEAC-MapPlus", SanitizeFileName(GetPlayerKey()) + ".visited.txt");
		}

		static string GetSaveDirectory()
		{
			try
			{
				string saveDir = GameIO.GetSaveGameDir();
				if (!string.IsNullOrEmpty(saveDir))
				{
					return saveDir;
				}
			}
			catch
			{
			}

			try
			{
				return GameIO.GetSaveGameLocalDir();
			}
			catch
			{
				return null;
			}
		}

		static string GetPlayerKey()
		{
			try
			{
				string combined = GameManager.Instance?.persistentLocalPlayer?.PrimaryId?.CombinedString;
				if (!string.IsNullOrEmpty(combined))
				{
					return combined;
				}
			}
			catch
			{
			}

			try
			{
				string combined = PlatformManager.MultiPlatform?.User?.PlatformUserId?.CombinedString;
				if (!string.IsNullOrEmpty(combined))
				{
					return combined;
				}
			}
			catch
			{
			}

			return "local";
		}

		static string SanitizeFileName(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return "local";
			}

			char[] invalid = Path.GetInvalidFileNameChars();
			StringBuilder builder = new StringBuilder(value.Length);
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				bool bad = c == ':' || c == '/' || c == '\\';
				if (!bad)
				{
					for (int j = 0; j < invalid.Length; j++)
					{
						if (c == invalid[j])
						{
							bad = true;
							break;
						}
					}
				}

				builder.Append(bad ? '_' : c);
			}

			return builder.ToString();
		}
	}
}
