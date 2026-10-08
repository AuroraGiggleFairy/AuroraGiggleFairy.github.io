using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Platform;
using UnityEngine;

namespace MapPlus
{
	/// <summary>
	/// Per-player, per-save list of POI instances entered after this mod was installed.
	/// The host keeps each player's list in the save. A client also writes its own copy.
	/// </summary>
	public static class MapPlusVisits
	{
		public const string IgnoreTags = "part,streettile,navonly,hideui";
		const string FolderName = "AGF-MapPlus";
		const string CVarPrefix = "agfmapplus_";
		const float TickSeconds = 1f;
		const float SaveSeconds = 30f;

		static FastTags<TagGroup.Poi> ignorePoiTags;
		static bool ignorePoiTagsReady;
		static readonly object Sync = new object();
		static readonly Dictionary<string, VisitList> Lists = new Dictionary<string, VisitList>(StringComparer.Ordinal);
		static readonly Dictionary<int, HashSet<string>> Pending = new Dictionary<int, HashSet<string>>();
		static float _nextTick;
		static float _nextSave = -1f;
		static int _localCvarCount = -1;

		sealed class VisitList
		{
			public readonly HashSet<string> Keys = new HashSet<string>(StringComparer.Ordinal);
			public bool Dirty;
			public string Path;
			public bool FileRead;
			public bool LegacyRead;
		}

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
			return pos.x + "," + pos.z + ":" + name;
		}

		public static bool HasVisited(PrefabInstance prefabInstance)
		{
			string key = MakeKey(prefabInstance);
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}

			EntityPlayerLocal local = GameManager.Instance?.World?.GetPrimaryPlayer();
			int count = local?.Buffs?.CVars?.Count ?? 0;
			if (count != _localCvarCount)
			{
				_localCvarCount = count;
				Absorb(local);
			}

			string playerId = PlayerId(local);
			lock (Sync)
			{
				if (!string.IsNullOrEmpty(playerId) && Lists.TryGetValue(playerId, out VisitList list) && list.Keys.Contains(key))
				{
					return true;
				}

				if (local != null && Pending.TryGetValue(local.entityId, out HashSet<string> pending) && pending.Contains(key))
				{
					return true;
				}
			}

			return false;
		}

		public static void RecordIfEntered(PrefabInstance prefabInstance)
		{
			RecordIfEntered(GameManager.Instance?.World?.GetPrimaryPlayer(), prefabInstance);
		}

		public static void RecordIfEntered(EntityPlayer player, PrefabInstance prefabInstance)
		{
			if (player == null || !IsNameablePrefab(prefabInstance))
			{
				return;
			}

			if (!(player is EntityPlayerLocal))
			{
				ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
				if (net == null || !net.IsServer)
				{
					return;
				}
			}

			string key = MakeKey(prefabInstance);
			if (string.IsNullOrEmpty(key))
			{
				return;
			}

			string playerId = PlayerId(player);
			if (string.IsNullOrEmpty(playerId))
			{
				Hold(player.entityId, key);
				return;
			}

			VisitList list = Ensure(playerId);
			bool added;
			lock (Sync)
			{
				added = list.Keys.Add(key);
				if (added)
				{
					list.Dirty = true;
				}
			}

			if (added)
			{
				SetVisitCVar(player, key);
			}
		}

		public static void Tick()
		{
			if (Time.time < _nextTick)
			{
				return;
			}

			_nextTick = Time.time + TickSeconds;
			try
			{
				BindPending();
				List<EntityPlayer> players = GameManager.Instance?.World?.Players?.list;
				if (players != null)
				{
					ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
					bool server = net != null && net.IsServer;
					for (int i = 0; i < players.Count; i++)
					{
						EntityPlayer player = players[i];
						if (player == null || (!server && !(player is EntityPlayerLocal)))
						{
							continue;
						}

						Absorb(player);
					}
				}

				if (_nextSave < 0f)
				{
					_nextSave = Time.time + SaveSeconds;
				}

				if (Time.time >= _nextSave)
				{
					_nextSave = Time.time + SaveSeconds;
					Save();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("MapPlus: Visit tick failed: " + ex.Message);
			}
		}

		public static void OnWorldDown()
		{
			try
			{
				Save();
			}
			catch (Exception ex)
			{
				Console.WriteLine("MapPlus: Visit save on leave failed: " + ex.Message);
			}

			lock (Sync)
			{
				Lists.Clear();
				Pending.Clear();
			}

			_localCvarCount = -1;
			_nextSave = -1f;
		}

		public static void Save()
		{
			BindPending();
			List<VisitList> lists;
			lock (Sync)
			{
				lists = new List<VisitList>(Lists.Values);
			}

			for (int i = 0; i < lists.Count; i++)
			{
				WriteList(lists[i]);
			}
		}

		static void Absorb(EntityPlayer player)
		{
			if (player?.Buffs?.CVars == null)
			{
				return;
			}

			string playerId = PlayerId(player);
			if (string.IsNullOrEmpty(playerId))
			{
				return;
			}

			VisitList list = Ensure(playerId);
			foreach (KeyValuePair<string, float> pair in player.Buffs.CVars)
			{
				if (pair.Value <= 0f || !TryParseCVar(pair.Key, out string key))
				{
					continue;
				}

				lock (Sync)
				{
					if (list.Keys.Add(key))
					{
						list.Dirty = true;
					}
				}
			}

			string[] keys;
			lock (Sync)
			{
				keys = new string[list.Keys.Count];
				list.Keys.CopyTo(keys);
			}

			for (int i = 0; i < keys.Length; i++)
			{
				SetVisitCVar(player, keys[i]);
			}
		}

		static void BindPending()
		{
			if (Pending.Count == 0)
			{
				return;
			}

			List<int> ready = null;
			foreach (KeyValuePair<int, HashSet<string>> pair in Pending)
			{
				EntityPlayer player = GameManager.Instance?.World?.GetEntity(pair.Key) as EntityPlayer;
				string playerId = PlayerId(player);
				if (string.IsNullOrEmpty(playerId))
				{
					continue;
				}

				VisitList list = Ensure(playerId);
				lock (Sync)
				{
					foreach (string key in pair.Value)
					{
						if (list.Keys.Add(key))
						{
							list.Dirty = true;
						}
					}
				}

				foreach (string key in pair.Value)
				{
					SetVisitCVar(player, key);
				}

				if (ready == null)
				{
					ready = new List<int>();
				}

				ready.Add(pair.Key);
			}

			if (ready == null)
			{
				return;
			}

			lock (Sync)
			{
				for (int i = 0; i < ready.Count; i++)
				{
					Pending.Remove(ready[i]);
				}
			}
		}

		static void Hold(int entityId, string key)
		{
			lock (Sync)
			{
				if (!Pending.TryGetValue(entityId, out HashSet<string> pending))
				{
					pending = new HashSet<string>(StringComparer.Ordinal);
					Pending[entityId] = pending;
				}

				pending.Add(key);
			}
		}

		static VisitList Ensure(string playerId)
		{
			string path = PathFor(playerId);
			VisitList list;
			string flushPath = null;
			List<string> flushLines = null;
			bool readFile;
			bool readLegacy;
			lock (Sync)
			{
				if (!Lists.TryGetValue(playerId, out list))
				{
					list = new VisitList();
					Lists[playerId] = list;
				}

				if (!string.IsNullOrEmpty(list.Path) && !string.Equals(list.Path, path, StringComparison.OrdinalIgnoreCase))
				{
					if (list.Dirty && list.Keys.Count > 0 && !string.IsNullOrEmpty(list.Path))
					{
						flushPath = list.Path;
						flushLines = new List<string>(list.Keys);
						list.Dirty = false;
					}

					list.Path = path;
					list.FileRead = false;
				}

				if (string.IsNullOrEmpty(list.Path))
				{
					list.Path = path;
				}

				readFile = !list.FileRead && !string.IsNullOrEmpty(path);
				readLegacy = !list.LegacyRead;
				if (readFile)
				{
					list.FileRead = true;
				}

				if (readLegacy)
				{
					list.LegacyRead = true;
				}
			}

			if (flushLines != null && !WriteVisitedFile(flushPath, flushLines))
			{
				lock (Sync)
				{
					list.Dirty = true;
				}
			}

			if (readFile)
			{
				MergeFile(list, path);
			}

			if (readLegacy)
			{
				MergeLegacy(list, playerId, path);
			}

			return list;
		}

		static void MergeLegacy(VisitList list, string playerId, string canonicalPath)
		{
			string platformId = null;
			try
			{
				platformId = PlatformManager.MultiPlatform?.User?.PlatformUserId?.CombinedString;
			}
			catch
			{
			}

			foreach (string root in SaveRoots())
			{
				MergeFile(list, FilePath(root, "local"));
				if (!string.IsNullOrEmpty(platformId) && !string.Equals(platformId, playerId, StringComparison.Ordinal))
				{
					MergeFile(list, FilePath(root, platformId));
				}

				string alt = FilePath(root, playerId);
				if (!string.Equals(alt, canonicalPath, StringComparison.OrdinalIgnoreCase))
				{
					MergeFile(list, alt);
				}
			}
		}

		static void MergeFile(VisitList list, string path)
		{
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return;
			}

			string[] lines;
			try
			{
				lines = File.ReadAllLines(path, Encoding.UTF8);
			}
			catch (Exception ex)
			{
				Console.WriteLine("MapPlus: Failed to load visited POIs: " + ex.Message);
				return;
			}

			for (int i = 0; i < lines.Length; i++)
			{
				string raw = lines[i];
				if (string.IsNullOrWhiteSpace(raw))
				{
					continue;
				}

				raw = raw.Trim();
				string key = Normalize(raw);
				if (string.IsNullOrEmpty(key))
				{
					continue;
				}

				if (list.Keys.Add(key) || !string.Equals(raw, key, StringComparison.Ordinal))
				{
					list.Dirty = true;
				}
			}
		}

		static void WriteList(VisitList list)
		{
			string path;
			List<string> lines;
			lock (Sync)
			{
				if (list == null || !list.Dirty || string.IsNullOrEmpty(list.Path) || list.Keys.Count == 0)
				{
					return;
				}

				path = list.Path;
				lines = new List<string>(list.Keys);
				list.Dirty = false;
			}

			if (!WriteVisitedFile(path, lines))
			{
				lock (Sync)
				{
					list.Dirty = true;
				}
			}
		}

		static bool WriteVisitedFile(string path, List<string> lines)
		{
			try
			{
				string directory = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				File.WriteAllLines(path, lines, Encoding.UTF8);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("MapPlus: Failed to save visited POIs: " + ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Height is ignored. Older lines may be x,y,z:name or id:x,y,z:name.
		/// </summary>
		static string Normalize(string line)
		{
			int nameAt = line.LastIndexOf(':');
			if (nameAt <= 0 || nameAt >= line.Length - 1)
			{
				return null;
			}

			string name = line.Substring(nameAt + 1);
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			string left = line.Substring(0, nameAt);
			int coordAt = left.LastIndexOf(':');
			string coords = coordAt >= 0 ? left.Substring(coordAt + 1) : left;
			string[] parts = coords.Split(',');
			if (parts.Length < 2)
			{
				return null;
			}

			if (!int.TryParse(parts[0], out int x))
			{
				return null;
			}

			if (!int.TryParse(parts[parts.Length - 1], out int z))
			{
				return null;
			}

			return x + "," + z + ":" + name;
		}

		static bool TryCVarName(string key, out string cvar)
		{
			cvar = null;
			int comma = key.IndexOf(',');
			int colon = key.IndexOf(':');
			if (comma <= 0 || colon <= comma + 1 || colon >= key.Length - 1)
			{
				return false;
			}

			string name = key.Substring(colon + 1);
			for (int i = 0; i < name.Length; i++)
			{
				char c = name[i];
				if (!char.IsLetterOrDigit(c) && c != '_')
				{
					return false;
				}
			}

			cvar = CVarPrefix + key.Substring(0, comma) + "_" + key.Substring(comma + 1, colon - comma - 1) + "_" + name;
			return true;
		}

		static bool TryParseCVar(string cvar, out string key)
		{
			key = null;
			if (string.IsNullOrEmpty(cvar) || !cvar.StartsWith(CVarPrefix, StringComparison.Ordinal))
			{
				return false;
			}

			string rest = cvar.Substring(CVarPrefix.Length);
			int i = 0;
			if (!ReadInt(rest, ref i, out int x) || i >= rest.Length || rest[i] != '_')
			{
				return false;
			}

			i++;
			if (!ReadInt(rest, ref i, out int z) || i >= rest.Length || rest[i] != '_')
			{
				return false;
			}

			i++;
			string name = rest.Substring(i);
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}

			key = x + "," + z + ":" + name;
			return true;
		}

		static bool ReadInt(string text, ref int i, out int value)
		{
			value = 0;
			int start = i;
			if (i < text.Length && text[i] == '-')
			{
				i++;
			}

			int digits = i;
			while (i < text.Length && text[i] >= '0' && text[i] <= '9')
			{
				i++;
			}

			if (i == digits)
			{
				i = start;
				return false;
			}

			return int.TryParse(text.Substring(start, i - start), out value);
		}

		static void SetVisitCVar(EntityPlayer player, string key)
		{
			if (player?.Buffs == null || !TryCVarName(key, out string cvar))
			{
				return;
			}

			if (player.Buffs.GetCustomVar(cvar) == 1f)
			{
				return;
			}

			player.Buffs.SetCustomVar(cvar, 1f, true, CVarOperation.set, true);
		}

		static string PlayerId(EntityPlayer player)
		{
			if (player == null)
			{
				return null;
			}

			try
			{
				string combined = GameManager.Instance?.persistentPlayers?.GetPlayerDataFromEntityID(player.entityId)?.PrimaryId?.CombinedString;
				if (!string.IsNullOrEmpty(combined))
				{
					return combined;
				}
			}
			catch
			{
			}

			if (player is EntityPlayerLocal)
			{
				try
				{
					string combined = PlatformManager.InternalLocalUserIdentifier?.CombinedString;
					if (!string.IsNullOrEmpty(combined))
					{
						return combined;
					}
				}
				catch
				{
				}
			}

			return null;
		}

		static string PathFor(string playerId)
		{
			string root = SaveRoot();
			if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(playerId))
			{
				return null;
			}

			return FilePath(root, playerId);
		}

		static string FilePath(string root, string playerId)
		{
			if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(playerId))
			{
				return null;
			}

			return Path.Combine(root, FolderName, SanitizeFileName(playerId) + ".visited.txt");
		}

		static string SaveRoot()
		{
			try
			{
				ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
				if (net != null && !net.IsServer)
				{
					string localDir = GameIO.GetSaveGameLocalDir();
					if (!string.IsNullOrEmpty(localDir))
					{
						return localDir;
					}
				}
			}
			catch
			{
			}

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

		static IEnumerable<string> SaveRoots()
		{
			string primary = SaveRoot();
			if (!string.IsNullOrEmpty(primary))
			{
				yield return primary;
			}

			string saveDir = null;
			string localDir = null;
			try
			{
				saveDir = GameIO.GetSaveGameDir();
			}
			catch
			{
			}

			try
			{
				localDir = GameIO.GetSaveGameLocalDir();
			}
			catch
			{
			}

			if (!string.IsNullOrEmpty(saveDir) && !string.Equals(saveDir, primary, StringComparison.OrdinalIgnoreCase))
			{
				yield return saveDir;
			}

			if (!string.IsNullOrEmpty(localDir) && !string.Equals(localDir, primary, StringComparison.OrdinalIgnoreCase) && !string.Equals(localDir, saveDir, StringComparison.OrdinalIgnoreCase))
			{
				yield return localDir;
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

		static string SanitizeFileName(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return "player";
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
