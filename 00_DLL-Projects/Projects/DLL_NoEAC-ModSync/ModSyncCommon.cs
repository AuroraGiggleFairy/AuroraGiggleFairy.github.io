using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace ModSync
{
	/// <summary>One file the server offers to clients.</summary>
	public class ManifestEntry
	{
		public string RelPath;
		public long Size;
		public string Hash;
	}

	/// <summary>
	/// Remembers file hashes between runs, keyed by path, size and write time. Without this a
	/// 10 GB mod set would be re-hashed from scratch every launch and every server restart,
	/// which costs minutes of stall for no benefit.
	/// </summary>
	public class ModSyncHashCache
	{
		private readonly string file;
		private readonly Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> pending = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public ModSyncHashCache(string cacheFile)
		{
			file = cacheFile;
			Load();
		}

		private static string Key(string full, long size, long ticks)
		{
			return full + "|" + size + "|" + ticks;
		}

		private void Load()
		{
			try
			{
				if (!File.Exists(file))
				{
					return;
				}

				foreach (string line in File.ReadAllLines(file))
				{
					int cut = line.LastIndexOf('|');
					if (cut > 0 && cut < line.Length - 1)
					{
						map[line.Substring(0, cut)] = line.Substring(cut + 1);
					}
				}
			}
			catch (Exception)
			{
				map.Clear();
			}
		}

		/// <summary>Hash of the file, computed only if it is not already known at this size and time.</summary>
		public string Get(string fullPath)
		{
			FileInfo fi = new FileInfo(fullPath);
			if (!fi.Exists)
			{
				return null;
			}

			string key = Key(fullPath, fi.Length, fi.LastWriteTimeUtc.Ticks);
			string hash;
			if (map.TryGetValue(key, out hash))
			{
				return hash;
			}

			hash = ModSyncCommon.Md5(fullPath);
			map[key] = hash;
			return hash;
		}

		/// <summary>
		/// Records a hash for a file that is not in place yet. Used for downloads still sitting in
		/// staging, so the launch right after an install does not re-hash everything it just got.
		/// </summary>
		public void Put(string fullPath, long size, long writeTimeTicks, string hash)
		{
			string key = Key(fullPath, size, writeTimeTicks);
			map[key] = hash;
			pending.Add(key);
		}

		/// <summary>Writes back only entries that still describe a file on disk, so it self-prunes.</summary>
		public void Save()
		{
			try
			{
				List<string> lines = new List<string>(map.Count);
				foreach (KeyValuePair<string, string> kv in map)
				{
					string[] parts = kv.Key.Split('|');
					if (parts.Length < 3)
					{
						continue;
					}

					// Not on disk yet by design; keep it for the run after the restart.
					if (pending.Contains(kv.Key))
					{
						lines.Add(kv.Key + "|" + kv.Value);
						continue;
					}

					try
					{
						FileInfo fi = new FileInfo(parts[0]);
						if (fi.Exists && fi.Length.ToString() == parts[1]
							&& fi.LastWriteTimeUtc.Ticks.ToString() == parts[2])
						{
							lines.Add(kv.Key + "|" + kv.Value);
						}
					}
					catch (Exception)
					{
					}
				}

				File.WriteAllLines(file, lines.ToArray());
			}
			catch (Exception ex)
			{
				ModSyncCommon.Warn("Could not save the hash cache: " + ex.Message);
			}
		}
	}

	public static class ModSyncCommon
	{
		public const string LogPrefix = "[ModSync] ";

		/// <summary>Signals the client to read the mod list again rather than fail.</summary>
		public const string ManifestChangedMessage = "MODSYNC_LIST_CHANGED";

		/// <summary>Bytes per NetPackage payload. Kept well under the MTU fragmentation limit.</summary>
		public const int ChunkSize = 16384;

		/// <summary>Chunks the server will push before it needs an ack from the client.</summary>
		public const int MaxChunksInFlight = 48;

		/// <summary>How many chunks the client receives between acks.</summary>
		public const int AckEvery = 16;

		public static void Info(string msg)
		{
			Log.Out(LogPrefix + msg);
		}

		public static void Warn(string msg)
		{
			Log.Warning(LogPrefix + msg);
		}

		public static void Error(string msg)
		{
			Log.Error(LogPrefix + msg);
		}

		public static string GameDir
		{
			get { return Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..")); }
		}

		/// <summary>
		/// Where mods go on this client: the game folder, never the shared AppData folder.
		/// ModManager.ModsBasePath points at the per-user AppData path, which every install on
		/// the machine loads from, so syncing there would push one server's mods into all of them.
		/// </summary>
		public static string ModsDir
		{
			get { return Path.Combine(GameDir, "Mods"); }
		}

		/// <summary>
		/// Where this server's mods actually live. A host may use either location, so prefer
		/// whichever one really holds mods.
		/// </summary>
		public static string ServerModsDir
		{
			get
			{
				string gameFolder = Path.Combine(GameDir, "Mods");
				if (HasMods(gameFolder))
				{
					return gameFolder;
				}

				string userFolder = ModManager.ModsBasePath;
				if (HasMods(userFolder))
				{
					return userFolder;
				}

				return gameFolder;
			}
		}

		private static bool HasMods(string dir)
		{
			try
			{
				return Directory.Exists(dir) && Directory.GetDirectories(dir).Length > 0;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static string StagingDir
		{
			get { return Path.Combine(GameDir, "ModSync-Staging"); }
		}

		/// <summary>Set by the installer only on a game folder it made for one server.</summary>
		public static bool IsManagedInstall
		{
			get { return File.Exists(Path.Combine(GameDir, "ModSync-Managed.txt")); }
		}

		/// <summary>Folder name of this mod, so a mirror pass never deletes the syncer itself.</summary>
		public static string OwnModFolder = "";

		/// <summary>
		/// Files a mirror pass must leave alone even when the server does not list them:
		/// Harmony, this mod, and a leftover join file. The shortcut owns the address;
		/// a join file inside Mods would be loaded by every install that has this mod.
		/// </summary>
		public static bool IsProtected(string relPath)
		{
			string rel = relPath.ToLowerInvariant();
			if (rel.StartsWith("0_tfp_harmony/"))
			{
				return true;
			}

			if (Path.GetFileName(rel) == "modsync-join.txt")
			{
				return true;
			}

			if (OwnModFolder.Length > 0 && rel.StartsWith(OwnModFolder.ToLowerInvariant() + "/"))
			{
				return true;
			}

			return false;
		}

		/// <summary>Top-level mod folders a mirror pass must never move, whatever the server lists.</summary>
		public static bool IsProtectedFolder(string folderName)
		{
			string name = folderName.ToLowerInvariant();
			return name == "0_tfp_harmony"
				|| (OwnModFolder.Length > 0 && name == OwnModFolder.ToLowerInvariant());
		}

		public static string NormalizeRel(string path)
		{
			return path.Replace('\\', '/').TrimStart('/');
		}

		public static string Md5(string file)
		{
			using (MD5 md5 = MD5.Create())
			using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				byte[] hash = md5.ComputeHash(fs);
				StringBuilder sb = new StringBuilder(32);
				for (int i = 0; i < hash.Length; i++)
				{
					sb.Append(hash[i].ToString("x2"));
				}
				return sb.ToString();
			}
		}

		public static byte[] Gzip(byte[] raw)
		{
			using (MemoryStream outStream = new MemoryStream())
			{
				using (GZipStream gz = new GZipStream(outStream, CompressionMode.Compress, true))
				{
					gz.Write(raw, 0, raw.Length);
				}
				return outStream.ToArray();
			}
		}

		public static byte[] Gunzip(byte[] packed)
		{
			using (MemoryStream inStream = new MemoryStream(packed))
			using (GZipStream gz = new GZipStream(inStream, CompressionMode.Decompress))
			using (MemoryStream outStream = new MemoryStream())
			{
				byte[] buffer = new byte[8192];
				int read;
				while ((read = gz.Read(buffer, 0, buffer.Length)) > 0)
				{
					outStream.Write(buffer, 0, read);
				}
				return outStream.ToArray();
			}
		}

		public static byte[] SerializeManifest(List<ManifestEntry> entries)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("MODSYNC1\n");
			for (int i = 0; i < entries.Count; i++)
			{
				ManifestEntry e = entries[i];
				sb.Append(e.RelPath).Append('\t').Append(e.Size).Append('\t').Append(e.Hash).Append('\n');
			}
			return Gzip(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		public static List<ManifestEntry> DeserializeManifest(byte[] packed)
		{
			List<ManifestEntry> entries = new List<ManifestEntry>();
			string text = Encoding.UTF8.GetString(Gunzip(packed));
			string[] lines = text.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				if (line.Length == 0 || i == 0)
				{
					continue;
				}

				string[] parts = line.Split('\t');
				if (parts.Length != 3)
				{
					continue;
				}

				long size;
				if (!long.TryParse(parts[1], out size))
				{
					continue;
				}

				entries.Add(new ManifestEntry { RelPath = parts[0], Size = size, Hash = parts[2] });
			}
			return entries;
		}

		/// <summary>Free bytes on the volume holding the game, or -1 when it cannot be read.</summary>
		public static long FreeSpace
		{
			get
			{
				try
				{
					return new DriveInfo(Path.GetPathRoot(Path.GetFullPath(GameDir))).AvailableFreeSpace;
				}
				catch (Exception)
				{
					return -1L;
				}
			}
		}

		public static string HumanSize(long bytes)
		{
			if (bytes >= 1024L * 1024L * 1024L)
			{
				return (bytes / 1073741824.0).ToString("0.0") + " GB";
			}
			if (bytes >= 1024L * 1024L)
			{
				return (bytes / 1048576.0).ToString("0.0") + " MB";
			}
			if (bytes >= 1024L)
			{
				return (bytes / 1024.0).ToString("0") + " KB";
			}
			return bytes + " B";
		}
	}
}
