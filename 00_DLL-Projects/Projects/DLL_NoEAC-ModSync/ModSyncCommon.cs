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

		/// <summary>Hello protocol of a client that can replace its own copy.</summary>
		public const int Protocol = 2;

		/// <summary>Manifest version that only asks an old client to open a transfer, then fails it.</summary>
		public const int TooOldManifestVersion = 0;

		/// <summary>Manifest version that carries only this mod, so the client can replace itself and rejoin.</summary>
		public const int SelfUpdateManifestVersion = -1;

		public const string TooOldMessage = "ModSync needs an update before you can join this server.";

		public const string ModFolderPrefix = "AGF-ModSync";

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
		/// The other Mods folder the game loads, usually under AppData. Null when it is the
		/// same folder as <see cref="ModsDir"/>, so a join never moves the game folder aside.
		/// </summary>
		public static string UserModsDir
		{
			get
			{
				try
				{
					string user = ModManager.ModsBasePath;
					if (string.IsNullOrEmpty(user))
					{
						return null;
					}

					user = Path.GetFullPath(user);
					string game = Path.GetFullPath(ModsDir);
					if (string.Equals(user, game, StringComparison.OrdinalIgnoreCase))
					{
						return null;
					}

					return user;
				}
				catch (Exception)
				{
					return null;
				}
			}
		}

		/// <summary>True when the user Mods folder has anything in it for this join to set aside.</summary>
		public static bool UserModsNeedSetAside()
		{
			string dir = UserModsDir;
			if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
			{
				return false;
			}

			try
			{
				return Directory.GetFileSystemEntries(dir).Length > 0;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>Dated folder beside the user Mods folder. The Mods folder itself stays.</summary>
		public static string UserModsBackupDir(string stamp)
		{
			string dir = UserModsDir;
			return Path.Combine(Path.GetDirectoryName(dir), "Mods - Backup", stamp);
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

		/// <summary>Folder name of this mod, so a mirror pass never deletes the syncer itself.</summary>
		public static string OwnModFolder = "";

		/// <summary>Version from this copy's ModInfo, or 0.0.0 when it cannot be read.</summary>
		public static string ReadOwnVersion()
		{
			try
			{
				if (OwnModFolder.Length == 0)
				{
					return "0.0.0";
				}

				return ReadVersion(Path.Combine(ModsDir, OwnModFolder, "ModInfo.xml"));
			}
			catch (Exception)
			{
				return "0.0.0";
			}
		}

		public static string ReadVersion(string modInfoPath)
		{
			try
			{
				if (!File.Exists(modInfoPath))
				{
					return "0.0.0";
				}

				string text = File.ReadAllText(modInfoPath);
				int at = text.IndexOf("<Version", StringComparison.OrdinalIgnoreCase);
				if (at < 0)
				{
					return "0.0.0";
				}

				int value = text.IndexOf("value=\"", at, StringComparison.OrdinalIgnoreCase);
				if (value < 0)
				{
					return "0.0.0";
				}

				value += "value=\"".Length;
				int end = text.IndexOf('"', value);
				if (end <= value)
				{
					return "0.0.0";
				}

				return text.Substring(value, end - value).Trim();
			}
			catch (Exception)
			{
				return "0.0.0";
			}
		}

		/// <summary>Negative when <paramref name="left"/> is older.</summary>
		public static int CompareVersions(string left, string right)
		{
			int[] a = VersionParts(left);
			int[] b = VersionParts(right);
			int n = a.Length > b.Length ? a.Length : b.Length;
			for (int i = 0; i < n; i++)
			{
				int av = i < a.Length ? a[i] : 0;
				int bv = i < b.Length ? b[i] : 0;
				if (av != bv)
				{
					return av < bv ? -1 : 1;
				}
			}

			return 0;
		}

		private static int[] VersionParts(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return new int[0];
			}

			string[] bits = version.Split('.');
			int[] parts = new int[bits.Length];
			for (int i = 0; i < bits.Length; i++)
			{
				int n;
				parts[i] = int.TryParse(bits[i], out n) ? n : 0;
			}

			return parts;
		}

		/// <summary>
		/// Vanilla mods that are already on every dedicated server. They are not part of
		/// the client sync: the server does not send them, and a client does not delete
		/// its own copies just because the server left them out.
		/// </summary>
		public static bool IsVanillaServerMod(string folderName)
		{
			string name = folderName.ToLowerInvariant();
			return name == "tfp_commandextensions" || name == "xample_markersmod";
		}

		/// <summary>
		/// Files a mirror pass must leave alone even when the server does not list them:
		/// Harmony, this mod, the vanilla dedicated-server mods, and a leftover join file.
		/// The shortcut owns the address; a join file inside Mods would be loaded by every
		/// install that has this mod.
		/// </summary>
		public static bool IsProtected(string relPath)
		{
			string rel = relPath.ToLowerInvariant();
			if (rel.StartsWith("0_tfp_harmony/"))
			{
				return true;
			}

			int slash = rel.IndexOf('/');
			string folder = slash > 0 ? rel.Substring(0, slash) : rel;
			if (IsVanillaServerMod(folder))
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
				|| IsVanillaServerMod(name)
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
