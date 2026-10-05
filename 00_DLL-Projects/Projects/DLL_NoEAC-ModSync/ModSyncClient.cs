using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ModSync
{
	/// <summary>
	/// Client half. Compares the server manifest against the local Mods folder, pulls
	/// what is missing into a staging folder, then restarts the game with the new mods.
	/// </summary>
	public static class ModSyncClient
	{
		private enum State
		{
			Idle,
			WaitingForManifest,
			Downloading,
			Finishing
		}

		private const int MaxAttempts = 3;

		private static State state = State.Idle;
		private static byte[][] manifestChunks;
		private static int manifestChunksIn;
		private static List<ManifestEntry> manifest;
		private static int manifestVersion;
		private static int rechecks;
		private static List<int> needed;
		private static List<string> extraMods;
		private static List<string> staleFiles;
		private static int neededPos;
		private static long bytesWanted;
		private static long bytesDone;
		private static int chunksReceived;
		private static ModSyncHashCache hashes;
		private static DateTime lastProgress = DateTime.MinValue;
		private static DateTime downloadStarted = DateTime.UtcNow;
		private static FileStream current;
		private static int currentFileIndex = -1;
		private static bool triedBackup;

		public static bool Active => state != State.Idle;

		/// <summary>Called from the handshake patch once package mappings are in place.</summary>
		public static void Begin()
		{
			triedBackup = false;
			Reset();
			state = State.WaitingForManifest;
			Progress("Checking mods with the server...");
			ModSyncCommon.Info("Asking the server for its mod list.");
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
				NetPackageManager.GetPackage<NetPackageModSyncHello>().Setup(), true);
		}

		private static void Reset()
		{
			CloseCurrent();
			manifestChunks = null;
			manifestChunksIn = 0;
			manifest = null;
			needed = null;
			extraMods = null;
			staleFiles = null;
			neededPos = 0;
			bytesWanted = 0;
			bytesDone = 0;
			chunksReceived = 0;
			currentFileIndex = -1;
			state = State.Idle;
		}

		private static void Progress(string text)
		{
			try
			{
				LocalPlayerUI ui = LocalPlayerUI.primaryUI;
				if (ui == null)
				{
					return;
				}

				if (XUiC_ProgressWindow.IsWindowOpen())
				{
					XUiC_ProgressWindow.SetText(ui, text);
				}
				else
				{
					XUiC_ProgressWindow.Open(ui, text);
				}
			}
			catch (Exception)
			{
			}
		}

		private static void Fail(string reason)
		{
			ModSyncCommon.Error(reason);
			Reset();
			try
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
				GameManager.Instance.ShowMessageServerAuthFailed("Mod sync failed:\n" + reason);
			}
			catch (Exception)
			{
			}
		}

		public static void OnManifestChunk(int version, int chunkIndex, int chunkCount, byte[] data)
		{
			if (state != State.WaitingForManifest)
			{
				return;
			}

			manifestVersion = version;

			if (manifestChunks == null || manifestChunks.Length != chunkCount)
			{
				manifestChunks = new byte[chunkCount][];
				manifestChunksIn = 0;
			}

			if (chunkIndex < 0 || chunkIndex >= chunkCount || manifestChunks[chunkIndex] != null)
			{
				return;
			}

			manifestChunks[chunkIndex] = data;
			manifestChunksIn++;
			if (manifestChunksIn < chunkCount)
			{
				return;
			}

			int total = 0;
			for (int i = 0; i < manifestChunks.Length; i++)
			{
				total += manifestChunks[i].Length;
			}

			byte[] packed = new byte[total];
			int offset = 0;
			for (int i = 0; i < manifestChunks.Length; i++)
			{
				Array.Copy(manifestChunks[i], 0, packed, offset, manifestChunks[i].Length);
				offset += manifestChunks[i].Length;
			}

			try
			{
				manifest = ModSyncCommon.DeserializeManifest(packed);
			}
			catch (Exception ex)
			{
				Fail("Could not read the server mod list: " + ex.Message);
				return;
			}

			CompareAndRequest();
		}

		/// <summary>
		/// Works out what this game has that the server does not run, split into the two cases
		/// that need different handling:
		///
		///   extraMods   whole mod folders the server does not run at all. These get set aside
		///               as a unit, which is what "removing a mod" means.
		///   staleFiles  leftovers sitting inside a mod the server does keep, usually from an
		///               older version of that same mod. The mod stays; the odd file goes.
		///
		/// A file the server has a different version of is neither. That is an overwrite, and it
		/// is handled by downloading over the top of it, never by removal.
		///
		/// Only ever collected on an install ModSync made for a server, so a player's own game
		/// never loses their personal mods.
		/// </summary>
		private static void FindUnwanted(string modsDir)
		{
			extraMods = new List<string>();
			staleFiles = new List<string>();

			if (!ModSyncCommon.IsManagedInstall || !Directory.Exists(modsDir))
			{
				return;
			}

			HashSet<string> onServer = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> serverMods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < manifest.Count; i++)
			{
				string rel = manifest[i].RelPath;
				onServer.Add(rel);

				int slash = rel.IndexOf('/');
				if (slash > 0)
				{
					serverMods.Add(rel.Substring(0, slash));
				}
			}

			string[] dirs = Directory.GetDirectories(modsDir);
			for (int i = 0; i < dirs.Length; i++)
			{
				string name = Path.GetFileName(dirs[i]);
				if (!serverMods.Contains(name) && !ModSyncCommon.IsProtectedFolder(name))
				{
					extraMods.Add(dirs[i]);
				}
			}

			string[] local = Directory.GetFiles(modsDir, "*", SearchOption.AllDirectories);
			for (int i = 0; i < local.Length; i++)
			{
				string rel = ModSyncCommon.NormalizeRel(local[i].Substring(modsDir.Length));
				if (onServer.Contains(rel) || ModSyncCommon.IsProtected(rel))
				{
					continue;
				}

				// Already covered by moving its whole folder.
				int slash = rel.IndexOf('/');
				if (slash > 0 && !serverMods.Contains(rel.Substring(0, slash)))
				{
					continue;
				}

				staleFiles.Add(local[i]);
			}
		}

		private static int UnwantedCount
		{
			get
			{
				return (extraMods != null ? extraMods.Count : 0)
					+ (staleFiles != null ? staleFiles.Count : 0);
			}
		}

		private static string SyncedMarkerPath
		{
			get { return Path.Combine(ModSyncCommon.GameDir, "ModSync-Synced.txt"); }
		}

		private static bool IsFirstJoin()
		{
			return !File.Exists(SyncedMarkerPath);
		}

		private static void MarkSynced()
		{
			try
			{
				File.WriteAllText(SyncedMarkerPath,
					"# This game has finished a mod check for this server.\r\n");
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		/// First join only. Copy same-named server mods out of Mods - Backup into Mods,
		/// and leave the backup copies where they are. Newest dated folder wins.
		/// </summary>
		private static int CopyUsefulFromBackup()
		{
			string backupRoot = Path.Combine(ModSyncCommon.GameDir, "Mods - Backup");
			if (!Directory.Exists(backupRoot) || manifest == null)
			{
				return 0;
			}

			HashSet<string> wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < manifest.Count; i++)
			{
				string rel = manifest[i].RelPath;
				int slash = rel.IndexOf('/');
				if (slash > 0)
				{
					wanted.Add(rel.Substring(0, slash));
				}
			}

			string[] dated = Directory.GetDirectories(backupRoot);
			Array.Sort(dated, StringComparer.OrdinalIgnoreCase);
			int copied = 0;
			for (int d = 0; d < dated.Length; d++)
			{
				string[] mods = Directory.GetDirectories(dated[d]);
				for (int m = 0; m < mods.Length; m++)
				{
					string name = Path.GetFileName(mods[m]);
					if (!wanted.Contains(name) || ModSyncCommon.IsProtectedFolder(name))
					{
						continue;
					}

					CopyDirectory(mods[m], Path.Combine(ModSyncCommon.ModsDir, name));
					copied++;
				}
			}

			return copied;
		}

		private static void CopyDirectory(string from, string to)
		{
			Directory.CreateDirectory(to);
			string[] files = Directory.GetFiles(from);
			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i], Path.Combine(to, Path.GetFileName(files[i])), true);
			}

			string[] dirs = Directory.GetDirectories(from);
			for (int i = 0; i < dirs.Length; i++)
			{
				CopyDirectory(dirs[i], Path.Combine(to, Path.GetFileName(dirs[i])));
			}
		}

		/// <summary>Size first because it is free; the hash only runs when the size already agrees.</summary>
		private static bool Matches(string file, ManifestEntry entry)
		{
			try
			{
				FileInfo fi = new FileInfo(file);
				if (!fi.Exists || fi.Length != entry.Size)
				{
					return false;
				}
				return hashes.Get(file) == entry.Hash;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static void CompareAndRequest()
		{
			string modsDir = ModSyncCommon.ModsDir;
			needed = new List<int>();
			bytesWanted = 0;

			if (hashes == null)
			{
				hashes = new ModSyncHashCache(Path.Combine(ModSyncCommon.GameDir, "ModSync-Hashes.txt"));
			}

			for (int i = 0; i < manifest.Count; i++)
			{
				ManifestEntry e = manifest[i];

				// Never pull Harmony or this mod over itself. The syncer swapping its own DLL
				// mid-run would downgrade the client whenever the server lags a version behind.
				if (ModSyncCommon.IsProtected(e.RelPath))
				{
					continue;
				}

				string rel = e.RelPath.Replace('/', Path.DirectorySeparatorChar);
				bool have = Matches(Path.Combine(modsDir, rel), e);

				// A file already downloaded but not yet moved in still counts, so an
				// interrupted sync picks up where it left off instead of starting over.
				if (!have)
				{
					have = Matches(Path.Combine(ModSyncCommon.StagingDir, rel), e);
				}

				if (!have)
				{
					needed.Add(i);
					bytesWanted += e.Size;
				}
			}

			if (needed.Count > 0 && IsFirstJoin() && !triedBackup)
			{
				triedBackup = true;
				int copied = CopyUsefulFromBackup();
				if (copied > 0)
				{
					ModSyncCommon.Info("Copied " + copied + " folder(s) from Mods - Backup. Checking again.");
					Progress("Found saved mods. Checking again...");
					CompareAndRequest();
					return;
				}
			}

			FindUnwanted(modsDir);
			hashes.Save();

			ModSyncCommon.Info("Server has " + manifest.Count + " mod file(s); this game needs "
				+ needed.Count + " (" + ModSyncCommon.HumanSize(bytesWanted) + "), has "
				+ extraMods.Count + " mod(s) the server does not run and "
				+ staleFiles.Count + " leftover file(s) inside mods it keeps.");

			if (needed.Count == 0 && UnwantedCount == 0)
			{
				ModSyncCommon.Info("Mods already match. Continuing to login.");
				MarkSynced();
				ClearAttempts();
				Reset();
				Progress("Joining server...");
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendLogin();
				return;
			}

			// Fail early rather than halfway through a 10 GB download.
			long free = ModSyncCommon.FreeSpace;
			if (free >= 0L && free < bytesWanted + 512L * 1024L * 1024L)
			{
				Fail("Not enough free space for the server's mods.\n"
					+ "Need about " + ModSyncCommon.HumanSize(bytesWanted) + " plus room to spare, but only "
					+ ModSyncCommon.HumanSize(free) + " is free.");
				return;
			}

			// Counts only attempts that made no headway, so a repeatedly interrupted big
			// download is not mistaken for a stuck loop.
			if (BumpAttempts(bytesWanted) > MaxAttempts)
			{
				Fail("Tried " + MaxAttempts + " times without getting any further.\n"
					+ "Check that the game folder is not read-only and that the disk is not full.");
				return;
			}

			try
			{
				// Kept between attempts on purpose. Files already verified above are skipped,
				// so a dropped 1 GB download resumes rather than restarting.
				Directory.CreateDirectory(ModSyncCommon.StagingDir);
			}
			catch (Exception ex)
			{
				Fail("Could not prepare the download folder: " + ex.Message);
				return;
			}

			state = State.Downloading;
			neededPos = 0;
			bytesDone = 0;
			chunksReceived = 0;
			downloadStarted = DateTime.UtcNow;
			lastProgress = DateTime.MinValue;

			// Nothing to fetch, only mods to set aside.
			if (needed.Count == 0)
			{
				OnTransferDone(true, "Removing mods the server does not run");
				return;
			}

			Progress(BuildProgressText());

			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
				NetPackageManager.GetPackage<NetPackageModSyncRequest>().Setup(manifestVersion, needed.ToArray()), true);
		}

		/// <summary>Server -> client queue notice. Nothing is wrong, this player is just in line.</summary>
		public static void OnQueued(int place, int total)
		{
			if (state != State.Downloading)
			{
				return;
			}

			Progress("Waiting for the server to reach you\n"
				+ "You are " + place + " of " + total + " in line\n"
				+ ModSyncCommon.HumanSize(bytesWanted) + " to download when your turn comes");
		}

		private static string BuildProgressText()
		{
			string line = "Downloading server mods\n"
				+ "File " + Math.Min(neededPos + 1, needed.Count) + " of " + needed.Count + "\n"
				+ ModSyncCommon.HumanSize(bytesDone) + " of " + ModSyncCommon.HumanSize(bytesWanted);

			double seconds = (DateTime.UtcNow - downloadStarted).TotalSeconds;
			if (seconds > 5.0 && bytesDone > 0L)
			{
				double rate = bytesDone / seconds;
				line += "  (" + ModSyncCommon.HumanSize((long)rate) + "/s)";

				long left = bytesWanted - bytesDone;
				if (left > 0L && rate > 1.0)
				{
					line += "\nAbout " + HumanTime(left / rate) + " left";
				}
			}

			return line;
		}

		private static string HumanTime(double seconds)
		{
			if (seconds < 90.0)
			{
				return Math.Max(1, (int)seconds) + " seconds";
			}
			if (seconds < 5400.0)
			{
				return Math.Max(1, (int)(seconds / 60.0)) + " minutes";
			}
			return (seconds / 3600.0).ToString("0.#") + " hours";
		}

		public static void OnFileChunk(int fileIndex, int chunkIndex, int chunkCount, byte[] data)
		{
			if (state != State.Downloading)
			{
				return;
			}

			try
			{
				if (currentFileIndex != fileIndex)
				{
					CloseCurrent();
					if (fileIndex < 0 || manifest == null || fileIndex >= manifest.Count)
					{
						return;
					}

					string rel = manifest[fileIndex].RelPath.Replace('/', Path.DirectorySeparatorChar);
					string dest = Path.Combine(ModSyncCommon.StagingDir, rel);
					Directory.CreateDirectory(Path.GetDirectoryName(dest));
					current = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None);
					currentFileIndex = fileIndex;
				}

				if (data != null && data.Length > 0)
				{
					current.Write(data, 0, data.Length);
					bytesDone += data.Length;
				}

				if (chunkIndex + 1 >= chunkCount)
				{
					CloseCurrent();
					neededPos++;
				}

				chunksReceived++;
				if (chunksReceived % ModSyncCommon.AckEvery == 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
						NetPackageManager.GetPackage<NetPackageModSyncAck>().Setup(chunksReceived), true);

					// Redrawing on every ack would be thousands of UI updates over a big pack.
					DateTime now = DateTime.UtcNow;
					if ((now - lastProgress).TotalSeconds >= 0.5)
					{
						lastProgress = now;
						Progress(BuildProgressText());
					}
				}
			}
			catch (Exception ex)
			{
				Fail("Could not save a mod file: " + ex.Message);
			}
		}

		private static void CloseCurrent()
		{
			if (current != null)
			{
				try
				{
					current.Dispose();
				}
				catch (Exception)
				{
				}
				current = null;
			}
			currentFileIndex = -1;
		}

		public static void OnTransferDone(bool ok, string message)
		{
			if (state != State.Downloading)
			{
				return;
			}

			CloseCurrent();

			if (!ok)
			{
				// The admin changed a mod mid-check. Read the list again instead of giving up.
				if (message == ModSyncCommon.ManifestChangedMessage && rechecks < 2)
				{
					rechecks++;
					ModSyncCommon.Info("Server mod list changed. Checking again.");
					Begin();
					return;
				}

				Fail(message);
				return;
			}

			state = State.Finishing;
			PreseedCache();
			ModSyncCommon.Info("Download complete. Restarting to apply mods.");
			Progress("Mods downloaded.\nRestarting the game to finish...");

			try
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
			}
			catch (Exception)
			{
			}

			try
			{
				LaunchApplier();
			}
			catch (Exception ex)
			{
				Fail("Could not restart to apply mods: " + ex.Message);
				return;
			}

			UnityEngine.Application.Quit();
		}

		/// <summary>
		/// The server already told us the hash of everything just downloaded, and robocopy /MOVE
		/// keeps the write time, so the next launch can trust these instead of reading it all back.
		/// </summary>
		private static void PreseedCache()
		{
			try
			{
				if (hashes == null || manifest == null || needed == null)
				{
					return;
				}

				string staging = ModSyncCommon.StagingDir;
				string modsDir = ModSyncCommon.ModsDir;

				for (int i = 0; i < needed.Count; i++)
				{
					ManifestEntry e = manifest[needed[i]];
					string rel = e.RelPath.Replace('/', Path.DirectorySeparatorChar);
					FileInfo fi = new FileInfo(Path.Combine(staging, rel));
					if (fi.Exists && fi.Length == e.Size)
					{
						hashes.Put(Path.Combine(modsDir, rel), fi.Length, fi.LastWriteTimeUtc.Ticks, e.Hash);
					}
				}

				hashes.Save();
			}
			catch (Exception ex)
			{
				ModSyncCommon.Warn("Could not save hashes for the new mods: " + ex.Message);
			}
		}

		/// <summary>
		/// Mod DLLs are locked while the game runs, so a helper script waits for exit,
		/// moves staging into Mods, and starts the game again.
		/// </summary>
		private static void LaunchApplier()
		{
			string gameDir = ModSyncCommon.GameDir;
			string exe = Path.Combine(gameDir, "7DaysToDie.exe");
			int pid = Process.GetCurrentProcess().Id;

			string[] argv = Environment.GetCommandLineArgs();
			StringBuilder args = new StringBuilder();
			for (int i = 1; i < argv.Length; i++)
			{
				if (args.Length > 0)
				{
					args.Append(' ');
				}
				args.Append(argv[i].IndexOf(' ') >= 0 ? "\"" + argv[i] + "\"" : argv[i]);
			}

			// Kept beside the game rather than in TEMP: a user folder with accented characters
			// mangles inside a batch file and the whole script silently does nothing.
			string bat = Path.Combine(gameDir, "ModSync-Apply.bat");
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("@echo off");
			sb.AppendLine("title ModSync - installing server mods");
			sb.AppendLine("echo.");
			sb.AppendLine("echo   Installing the mods you just downloaded...");
			sb.AppendLine("echo   The game will start again by itself. Please wait.");
			sb.AppendLine("echo.");
			sb.AppendLine(":waitloop");
			sb.AppendLine("tasklist /fi \"PID eq " + pid + "\" 2>nul | find \"" + pid + "\" >nul");
			sb.AppendLine("if not errorlevel 1 (");
			sb.AppendLine("  timeout /t 1 /nobreak >nul");
			sb.AppendLine("  goto waitloop");
			sb.AppendLine(")");
			sb.AppendLine("timeout /t 2 /nobreak >nul");

			string modsDir = ModSyncCommon.ModsDir;
			string removedDir = Path.Combine(ModSyncCommon.GameDir, "ModSync-Removed");

			// Set aside rather than deleted, so a wrong call is always recoverable.
			if (extraMods != null && extraMods.Count > 0)
			{
				sb.AppendLine("echo   Setting aside " + extraMods.Count + " mod(s) this server does not run...");
				sb.AppendLine("if not exist \"" + removedDir + "\" md \"" + removedDir + "\" >nul 2>&1");

				for (int i = 0; i < extraMods.Count; i++)
				{
					string name = Path.GetFileName(extraMods[i]);
					string dest = Path.Combine(removedDir, name);

					ModSyncCommon.Info("Setting aside mod the server does not run: " + name);
					sb.AppendLine("echo     - " + EscapeForEcho(name));
					sb.AppendLine("if exist \"" + extraMods[i] + "\" (");
					// A whole folder on the same drive is one rename, so even a multi-GB mod is instant.
					sb.AppendLine("  if not exist \"" + dest + "\" (");
					sb.AppendLine("    move /y \"" + extraMods[i] + "\" \"" + removedDir + "\\\" >nul 2>&1");
					sb.AppendLine("  ) else (");
					// Something of that name is already set aside, so merge into it instead.
					sb.AppendLine("    robocopy \"" + extraMods[i] + "\" \"" + dest + "\" /E /MOVE /R:1 /W:1 /NFL /NDL /NJH /NJS /NP >nul");
					sb.AppendLine("    rd /s /q \"" + extraMods[i] + "\" >nul 2>&1");
					sb.AppendLine("  )");
					sb.AppendLine(")");
				}
			}

			if (staleFiles != null && staleFiles.Count > 0)
			{
				string[] pairs = new string[staleFiles.Count];
				for (int i = 0; i < staleFiles.Count; i++)
				{
					string rel = staleFiles[i].Substring(modsDir.Length).TrimStart('\\', '/');
					pairs[i] = staleFiles[i] + "|" + Path.Combine(removedDir, rel);
				}

				string listFile = Path.Combine(gameDir, "ModSync-Remove.txt");
				File.WriteAllLines(listFile, pairs, BatEncoding);

				sb.AppendLine("echo   Clearing " + staleFiles.Count + " old file(s) out of mods the server keeps...");
				sb.AppendLine("for /f \"usebackq tokens=1,2 delims=|\" %%A in (\"" + listFile + "\") do (");
				sb.AppendLine("  if exist \"%%~A\" (");
				sb.AppendLine("    if not exist \"%%~dpB\" md \"%%~dpB\" >nul 2>&1");
				sb.AppendLine("    move /y \"%%~A\" \"%%~B\" >nul 2>&1");
				sb.AppendLine("  )");
				sb.AppendLine(")");
				// Pulling files out leaves empty folders behind; rd without /s only removes empty ones.
				sb.AppendLine("for /f \"usebackq delims=\" %%D in (`dir \"" + modsDir + "\" /ad /b /s ^| sort /r`) do rd \"%%~D\" >nul 2>&1");
				sb.AppendLine("del /f /q \"" + listFile + "\" >nul 2>&1");
			}

			if (Directory.Exists(ModSyncCommon.StagingDir))
			{
				// /MOVE, not a copy. Staging sits on the same drive as Mods, so this is a rename
				// per file: seconds instead of writing another 10 GB, and no second copy of the
				// pack has to fit on the disk.
				sb.AppendLine("echo   Putting the mods in place...");
				sb.AppendLine("robocopy \"" + ModSyncCommon.StagingDir + "\" \"" + ModSyncCommon.ModsDir + "\" /E /IS /MOVE /R:2 /W:2 /NFL /NDL /NJH /NJS /NP >nul");
				sb.AppendLine("rmdir /s /q \"" + ModSyncCommon.StagingDir + "\" >nul 2>&1");
			}
			sb.AppendLine("start \"\" \"" + exe + "\" " + args);
			sb.AppendLine("del /f /q \"%~f0\" >nul 2>&1");
			sb.AppendLine("exit");

			File.WriteAllText(bat, sb.ToString(), BatEncoding);

			ProcessStartInfo psi = new ProcessStartInfo(bat);
			psi.UseShellExecute = true;
			psi.WorkingDirectory = gameDir;
			Process.Start(psi);
		}

		/// <summary>A mod folder named with &amp; or | would otherwise run as a command.</summary>
		private static string EscapeForEcho(string text)
		{
			StringBuilder sb = new StringBuilder(text.Length);
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '&' || c == '|' || c == '<' || c == '>' || c == '^' || c == '(' || c == ')')
				{
					sb.Append('^');
				}
				sb.Append(c);
			}
			return sb.ToString();
		}

		/// <summary>cmd reads a batch file in the console codepage, not the ANSI one.</summary>
		private static Encoding BatEncoding
		{
			get
			{
				try
				{
					return Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
				}
				catch (Exception)
				{
					return Encoding.Default;
				}
			}
		}

		private static string AttemptsFile
		{
			get { return Path.Combine(ModSyncCommon.GameDir, "ModSync-Attempts.txt"); }
		}

		/// <summary>
		/// Counts consecutive attempts that got nowhere. A big download that keeps being cut off
		/// still shrinks the remaining bytes each time, and that counts as progress, so only a
		/// genuinely stuck install (read-only folder, full disk) ever hits the limit.
		/// </summary>
		private static int BumpAttempts(long remaining)
		{
			int n = 0;
			long last = long.MaxValue;

			try
			{
				if (File.Exists(AttemptsFile))
				{
					string[] parts = File.ReadAllText(AttemptsFile).Trim().Split('|');
					if (parts.Length > 0)
					{
						int.TryParse(parts[0], out n);
					}
					if (parts.Length < 2 || !long.TryParse(parts[1], out last))
					{
						last = long.MaxValue;
					}
				}

				n = remaining < last ? 1 : n + 1;
				File.WriteAllText(AttemptsFile, n + "|" + remaining);
			}
			catch (Exception)
			{
			}

			return n;
		}

		private static void ClearAttempts()
		{
			try
			{
				if (File.Exists(AttemptsFile))
				{
					File.Delete(AttemptsFile);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
