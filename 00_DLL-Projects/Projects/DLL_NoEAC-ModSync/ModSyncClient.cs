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
		private static HashSet<int> verified;
		private static bool selfUpdate;
		private static string selfFolder = "";
		private static ModSyncHashCache hashes;
		private static DateTime lastProgress = DateTime.MinValue;
		private static DateTime downloadStarted = DateTime.UtcNow;
		private static FileStream current;
		private static int currentFileIndex = -1;

		public static bool Active => state != State.Idle;

		/// <summary>Called from the handshake patch once package mappings are in place.</summary>
		public static void Begin()
		{
			Reset();
			state = State.WaitingForManifest;
			Progress("Checking mods with the server...");
			ModSyncCommon.Info("Asking the server for its mod list.");
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
				NetPackageManager.GetPackage<NetPackageModSyncHello>().Setup(ModSyncCommon.ReadOwnVersion()), true);
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
			verified = null;
			selfUpdate = false;
			selfFolder = "";
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

			selfUpdate = manifestVersion == ModSyncCommon.SelfUpdateManifestVersion;
			if (selfUpdate)
			{
				selfFolder = "";
				if (manifest != null && manifest.Count > 0 && !string.IsNullOrEmpty(manifest[0].RelPath))
				{
					int slash = manifest[0].RelPath.IndexOf('/');
					selfFolder = slash > 0 ? manifest[0].RelPath.Substring(0, slash) : manifest[0].RelPath;
				}

				if (selfFolder.Length == 0)
				{
					Fail(ModSyncCommon.TooOldMessage);
					return;
				}
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
		/// A file the server has a different version of is neither. The previous copy is moved
		/// into the same dated ModSync-Removed folder before the new one takes its place.
		///
		/// Collected on every join. The dated folder is the copy a player can put back.
		/// </summary>
		private static void FindUnwanted(string modsDir)
		{
			extraMods = new List<string>();
			staleFiles = new List<string>();

			if (!Directory.Exists(modsDir))
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
				return string.Equals(hashes.Get(file), entry.Hash, StringComparison.OrdinalIgnoreCase);
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

				// Never pull Harmony or this mod over itself during a normal check. A self-update
				// is the exception: the server is newer, and those files are the update.
				if (!selfUpdate && ModSyncCommon.IsProtected(e.RelPath))
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

			if (!selfUpdate)
			{
				FindUnwanted(modsDir);
			}
			else
			{
				extraMods = new List<string>();
				staleFiles = new List<string>();
			}

			hashes.Save();

			ModSyncCommon.Info("Server has " + manifest.Count + " mod file(s); this game needs "
				+ needed.Count + " (" + ModSyncCommon.HumanSize(bytesWanted) + "), has "
				+ extraMods.Count + " mod(s) the server does not run and "
				+ staleFiles.Count + " leftover file(s) inside mods it keeps.");

			if (selfUpdate && needed.Count == 0)
			{
				BeginSelfUpdateApply();
				return;
			}

			if (needed.Count == 0 && UnwantedCount == 0)
			{
				if (!ModSyncCommon.UserModsNeedSetAside())
				{
					ModSyncCommon.Info("Mods already match. Continuing to login.");
					ClearAttempts();
					Reset();
					Progress("Joining server...");
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendLogin();
					return;
				}

				ModSyncCommon.Info("User Mods folder has mods. Restarting to set them aside.");
				Progress("Setting aside mods from your user folder.\nRestarting the game to finish...");
				ClearAttempts();
				state = State.Finishing;
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
			verified = new HashSet<int>();
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
					int finished = currentFileIndex;
					CloseCurrent();
					if (finished < 0 || manifest == null || finished >= manifest.Count)
					{
						Fail("The download ended on a file the server did not list.\nNothing was changed.");
						return;
					}

					ManifestEntry entry = manifest[finished];
					string rel = entry.RelPath.Replace('/', Path.DirectorySeparatorChar);
					string staged = Path.Combine(ModSyncCommon.StagingDir, rel);
					if (!StagedMatches(entry, staged))
					{
						try
						{
							if (File.Exists(staged))
							{
								File.Delete(staged);
							}
						}
						catch (Exception)
						{
						}

						Fail(MismatchMessage(entry.RelPath));
						return;
					}

					if (verified != null)
					{
						verified.Add(finished);
					}
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

			string unverified = FirstUnverified();
			if (unverified != null)
			{
				Fail(unverified);
				return;
			}

			state = State.Finishing;
			PreseedCache();
			if (selfUpdate)
			{
				BeginSelfUpdateApply();
				return;
			}

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
		/// Size is not enough. A same-size edit, or a short write that still lands on the right
		/// length, used to be stored under the server's hash and then treated as a match.
		/// </summary>
		private static bool StagedMatches(ManifestEntry entry, string stagedPath)
		{
			try
			{
				FileInfo fi = new FileInfo(stagedPath);
				if (!fi.Exists || fi.Length != entry.Size || string.IsNullOrEmpty(entry.Hash))
				{
					return false;
				}

				return string.Equals(ModSyncCommon.Md5(stagedPath), entry.Hash, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static string MismatchMessage(string relPath)
		{
			string shown = "Mods\\" + (relPath ?? string.Empty).Replace('/', '\\');
			return "A downloaded file does not match the server:\n\n  " + shown
				+ "\n\nNothing was changed.\n"
				+ "Restart the server if its mods were edited after it started, then log in again.";
		}

		/// <summary>Done is only success when every requested file was hashed and matched.</summary>
		private static string FirstUnverified()
		{
			if (needed == null)
			{
				return null;
			}

			for (int i = 0; i < needed.Count; i++)
			{
				int index = needed[i];
				if (verified != null && verified.Contains(index))
				{
					continue;
				}

				string rel = manifest != null && index >= 0 && index < manifest.Count
					? manifest[index].RelPath
					: "unknown";
				return MismatchMessage(rel);
			}

			return null;
		}

		/// <summary>
		/// The staged file has already been hashed against the server. robocopy /MOVE keeps the
		/// write time, so the next launch can trust these instead of reading it all back.
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
		/// Installs only the newer ModSync, sets the previous copy aside, and starts the
		/// same shortcut again. The next join does the normal mod check.
		/// </summary>
		private static void BeginSelfUpdateApply()
		{
			if (selfFolder.Length == 0)
			{
				Fail(ModSyncCommon.TooOldMessage);
				return;
			}

			ModSyncCommon.Info("ModSync on the server is newer. Restarting to install it.");
			Progress("Updating ModSync.\nThe game will close and start again.");
			state = State.Finishing;

			try
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
			}
			catch (Exception)
			{
			}

			try
			{
				LaunchSelfUpdateApplier();
			}
			catch (Exception ex)
			{
				Fail("Could not restart to update ModSync: " + ex.Message);
				return;
			}

			UnityEngine.Application.Quit();
		}

		private static void LaunchSelfUpdateApplier()
		{
			string gameDir = ModSyncCommon.GameDir;
			string exe = Path.Combine(gameDir, "7DaysToDie.exe");
			string modsDir = ModSyncCommon.ModsDir;
			int pid = Process.GetCurrentProcess().Id;
			string stamp = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
			string removedDir = Path.Combine(gameDir, "ModSync-Removed", stamp);
			string stagingFolder = Path.Combine(ModSyncCommon.StagingDir, selfFolder);
			bool haveStaging = Directory.Exists(stagingFolder);

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

			List<string> move = new List<string>();
			if (Directory.Exists(modsDir))
			{
				string[] dirs = Directory.GetDirectories(modsDir);
				for (int i = 0; i < dirs.Length; i++)
				{
					string name = Path.GetFileName(dirs[i]);
					if (!name.StartsWith(ModSyncCommon.ModFolderPrefix, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					if (!haveStaging && name.Equals(selfFolder, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					move.Add(dirs[i]);
				}
			}

			List<string> lockFiles = new List<string>();
			for (int i = 0; i < move.Count; i++)
			{
				if (Directory.Exists(move[i]))
				{
					lockFiles.AddRange(Directory.GetFiles(move[i], "*", SearchOption.AllDirectories));
				}
			}

			if (Directory.Exists(ModSyncCommon.StagingDir))
			{
				lockFiles.AddRange(Directory.GetFiles(ModSyncCommon.StagingDir, "*", SearchOption.AllDirectories));
			}

			string bat = Path.Combine(gameDir, "ModSync-Apply.bat");
			string lockList = Path.Combine(gameDir, "ModSync-LockList.txt");
			string lockScript = Path.Combine(gameDir, "ModSync-LockCheck.ps1");
			string lockResult = Path.Combine(gameDir, "ModSync-Locked.txt");
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("@echo off");
			sb.AppendLine("title ModSync - updating ModSync");
			sb.AppendLine("echo.");
			sb.AppendLine("echo   Updating ModSync...");
			sb.AppendLine("echo   The game will start again by itself. Please wait.");
			sb.AppendLine("echo.");
			sb.AppendLine(":waitloop");
			sb.AppendLine("tasklist /fi \"PID eq " + pid + "\" 2>nul | find \"" + pid + "\" >nul");
			sb.AppendLine("if not errorlevel 1 (");
			sb.AppendLine("  timeout /t 1 /nobreak >nul");
			sb.AppendLine("  goto waitloop");
			sb.AppendLine(")");
			sb.AppendLine("timeout /t 2 /nobreak >nul");

			if (lockFiles.Count > 0)
			{
				File.WriteAllLines(lockList, lockFiles.ToArray(), new UTF8Encoding(true));
				File.WriteAllText(lockScript, LockCheckScript, new UTF8Encoding(true));
				sb.AppendLine("if not exist \"" + lockList + "\" goto :nolock");
				sb.AppendLine("del /f /q \"" + lockResult + "\" >nul 2>&1");
				sb.AppendLine("powershell -NoProfile -ExecutionPolicy Bypass -File \"" + lockScript + "\" -ListFile \"" + lockList + "\" -ModsDir \"" + modsDir + "\" -ResultFile \"" + lockResult + "\"");
				sb.AppendLine("if exist \"" + lockResult + "\" goto :fileopen");
				sb.AppendLine("del /f /q \"" + lockScript + "\" \"" + lockList + "\" >nul 2>&1");
				sb.AppendLine(":nolock");
			}

			if (move.Count > 0)
			{
				sb.AppendLine("echo   Keeping the previous ModSync in ModSync-Removed\\" + Path.GetFileName(removedDir));
				sb.AppendLine("if not exist \"" + removedDir + "\" md \"" + removedDir + "\" >nul 2>&1");
				for (int i = 0; i < move.Count; i++)
				{
					sb.AppendLine("if exist \"" + move[i] + "\" move /y \"" + move[i] + "\" \"" + removedDir + "\\\" >nul 2>&1");
				}
			}

			if (haveStaging)
			{
				sb.AppendLine("echo   Putting ModSync in place...");
				sb.AppendLine("robocopy \"" + ModSyncCommon.StagingDir + "\" \"" + modsDir + "\" /E /IS /MOVE /R:2 /W:2 /NFL /NDL /NJH /NJS /NP >nul");
				sb.AppendLine("rmdir /s /q \"" + ModSyncCommon.StagingDir + "\" >nul 2>&1");
			}

			sb.AppendLine("start \"\" \"" + exe + "\" " + args);
			sb.AppendLine("del /f /q \"%~f0\" >nul 2>&1");
			sb.AppendLine("exit");
			if (lockFiles.Count > 0)
			{
				sb.AppendLine(":fileopen");
				sb.AppendLine("echo.");
				sb.AppendLine("echo   Nothing was changed.");
				sb.AppendLine("rmdir /s /q \"" + ModSyncCommon.StagingDir + "\" >nul 2>&1");
				sb.AppendLine("del /f /q \"" + lockScript + "\" \"" + lockList + "\" \"" + lockResult + "\" >nul 2>&1");
				sb.AppendLine("echo.");
				sb.AppendLine("pause");
				sb.AppendLine("del /f /q \"%~f0\" >nul 2>&1");
				sb.AppendLine("exit");
			}

			File.WriteAllText(bat, sb.ToString(), BatEncoding);
			ProcessStartInfo psi = new ProcessStartInfo(bat);
			psi.UseShellExecute = true;
			psi.WorkingDirectory = gameDir;
			Process.Start(psi);
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
			sb.AppendLine("echo   Finishing the mod check...");
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
			// One folder per join, so a later sync cannot mix with an earlier rescue.
			string stamp = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
			string removedDir = Path.Combine(gameDir, "ModSync-Removed", stamp);
			List<string> replaced = ListFilesStagingWillReplace(modsDir, removedDir);
			string userMods = ModSyncCommon.UserModsDir;
			string userBackup = ModSyncCommon.UserModsNeedSetAside() ? ModSyncCommon.UserModsBackupDir(stamp) : null;
			string lockList = Path.Combine(gameDir, "ModSync-LockList.txt");
			string lockScript = Path.Combine(gameDir, "ModSync-LockCheck.ps1");
			string lockResult = Path.Combine(gameDir, "ModSync-Locked.txt");
			List<string> lockFiles = FilesTheApplyWillTouch(modsDir, replaced);
			if (!string.IsNullOrEmpty(userMods) && Directory.Exists(userMods))
			{
				lockFiles.AddRange(Directory.GetFiles(userMods, "*", SearchOption.AllDirectories));
			}
			if (lockFiles.Count > 0)
			{
				File.WriteAllLines(lockList, lockFiles.ToArray(), new UTF8Encoding(true));
				File.WriteAllText(lockScript, LockCheckScript, new UTF8Encoding(true));
				sb.AppendLine("if not exist \"" + lockList + "\" goto :nolock");
				sb.AppendLine("del /f /q \"" + lockResult + "\" >nul 2>&1");
				sb.AppendLine("powershell -NoProfile -ExecutionPolicy Bypass -File \"" + lockScript + "\" -ListFile \"" + lockList + "\" -ModsDir \"" + modsDir + "\" -ResultFile \"" + lockResult + "\"");
				sb.AppendLine("if exist \"" + lockResult + "\" goto :fileopen");
				sb.AppendLine("del /f /q \"" + lockScript + "\" \"" + lockList + "\" >nul 2>&1");
				sb.AppendLine(":nolock");
			}

			if (userBackup != null)
			{
				ModSyncCommon.Info("Setting aside user Mods folder into " + userBackup);
				sb.AppendLine("echo   Moving mods out of the user Mods folder.");
				sb.AppendLine("echo   Keeping them in " + EscapeForEcho(userBackup));
				sb.AppendLine("if not exist \"" + userBackup + "\" md \"" + userBackup + "\" >nul 2>&1");
				sb.AppendLine("robocopy \"" + userMods + "\" \"" + userBackup + "\" /E /MOVE /R:1 /W:1 /NFL /NDL /NJH /NJS /NP >nul");
			}

			bool keeping = (extraMods != null && extraMods.Count > 0)
				|| (staleFiles != null && staleFiles.Count > 0)
				|| replaced.Count > 0;
			if (keeping)
			{
				sb.AppendLine("echo   Keeping the previous copies in ModSync-Removed\\" + Path.GetFileName(removedDir));
			}

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

			if (replaced.Count > 0)
			{
				string listFile = Path.Combine(gameDir, "ModSync-Replace.txt");
				File.WriteAllLines(listFile, replaced.ToArray(), BatEncoding);

				sb.AppendLine("echo   Saving " + replaced.Count + " file(s) the server is about to replace...");
				sb.AppendLine("for /f \"usebackq tokens=1,2 delims=|\" %%A in (\"" + listFile + "\") do (");
				sb.AppendLine("  if exist \"%%~A\" (");
				sb.AppendLine("    if not exist \"%%~dpB\" md \"%%~dpB\" >nul 2>&1");
				sb.AppendLine("    move /y \"%%~A\" \"%%~B\" >nul 2>&1");
				sb.AppendLine("  )");
				sb.AppendLine(")");
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
			if (lockFiles.Count > 0)
			{
				string hashFile = Path.Combine(gameDir, "ModSync-Hashes.txt");
				sb.AppendLine(":fileopen");
				sb.AppendLine("echo.");
				sb.AppendLine("echo   Nothing was changed.");
				sb.AppendLine("rmdir /s /q \"" + ModSyncCommon.StagingDir + "\" >nul 2>&1");
				sb.AppendLine("del /f /q \"" + hashFile + "\" >nul 2>&1");
				sb.AppendLine("del /f /q \"" + Path.Combine(gameDir, "ModSync-Remove.txt") + "\" >nul 2>&1");
				sb.AppendLine("del /f /q \"" + Path.Combine(gameDir, "ModSync-Replace.txt") + "\" >nul 2>&1");
				sb.AppendLine("del /f /q \"" + lockScript + "\" \"" + lockList + "\" \"" + lockResult + "\" >nul 2>&1");
				sb.AppendLine("echo.");
				sb.AppendLine("pause");
				sb.AppendLine("del /f /q \"%~f0\" >nul 2>&1");
				sb.AppendLine("exit");
			}

			File.WriteAllText(bat, sb.ToString(), BatEncoding);

			ProcessStartInfo psi = new ProcessStartInfo(bat);
			psi.UseShellExecute = true;
			psi.WorkingDirectory = gameDir;
			Process.Start(psi);
		}

		/// <summary>
		/// Files this apply will move. Checked after the game has closed, so a lock means
		/// some other program still has the file open.
		/// </summary>
		private static List<string> FilesTheApplyWillTouch(string modsDir, List<string> replacedPairs)
		{
			List<string> files = new List<string>();
			if (extraMods != null)
			{
				for (int i = 0; i < extraMods.Count; i++)
				{
					if (!Directory.Exists(extraMods[i]))
					{
						continue;
					}

					files.AddRange(Directory.GetFiles(extraMods[i], "*", SearchOption.AllDirectories));
				}
			}

			if (staleFiles != null)
			{
				files.AddRange(staleFiles);
			}

			if (replacedPairs != null)
			{
				for (int i = 0; i < replacedPairs.Count; i++)
				{
					int cut = replacedPairs[i].IndexOf('|');
					if (cut > 0)
					{
						files.Add(replacedPairs[i].Substring(0, cut));
					}
				}
			}

			return files;
		}

		private const string LockCheckScript =
			"param([string]$ListFile, [string]$ModsDir, [string]$ResultFile)\r\n" +
			"$modsFull = [System.IO.Path]::GetFullPath($ModsDir).TrimEnd('\\')\r\n" +
			"$locked = New-Object System.Collections.Generic.List[string]\r\n" +
			"foreach ($line in [System.IO.File]::ReadAllLines($ListFile)) {\r\n" +
			"  $p = $line.Trim()\r\n" +
			"  if (-not $p) { continue }\r\n" +
			"  if (-not [System.IO.File]::Exists($p)) { continue }\r\n" +
			"  $stream = $null\r\n" +
			"  try {\r\n" +
			"    $stream = [System.IO.File]::Open($p, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)\r\n" +
			"  } catch [System.IO.IOException] {\r\n" +
			"    $code = $_.Exception.HResult -band 65535\r\n" +
			"    if ($code -eq 32 -or $code -eq 33) {\r\n" +
			"      $full = [System.IO.Path]::GetFullPath($p)\r\n" +
			"      $shown = $full\r\n" +
			"      if ($full.StartsWith($modsFull, [System.StringComparison]::OrdinalIgnoreCase)) {\r\n" +
			"        $rel = $full.Substring($modsFull.Length).TrimStart('\\')\r\n" +
			"        $shown = 'Mods\\' + $rel\r\n" +
			"      }\r\n" +
			"      [void]$locked.Add($shown)\r\n" +
			"    }\r\n" +
			"  } finally {\r\n" +
			"    if ($stream) { $stream.Dispose() }\r\n" +
			"  }\r\n" +
			"}\r\n" +
			"if ($locked.Count -eq 0) { exit 0 }\r\n" +
			"Write-Host ''\r\n" +
			"if ($locked.Count -eq 1) {\r\n" +
			"  Write-Host '  This file is open in another program, so it cannot be updated:'\r\n" +
			"} else {\r\n" +
			"  Write-Host '  These files are open in another program, so they cannot be updated:'\r\n" +
			"}\r\n" +
			"Write-Host ''\r\n" +
			"foreach ($shown in $locked) { Write-Host ('  ' + $shown) }\r\n" +
			"Write-Host ''\r\n" +
			"if ($locked.Count -eq 1) {\r\n" +
			"  Write-Host '  Close that program, then log in again.'\r\n" +
			"} else {\r\n" +
			"  Write-Host '  Close the programs that have them open, then log in again.'\r\n" +
			"}\r\n" +
			"[System.IO.File]::WriteAllLines($ResultFile, $locked.ToArray())\r\n" +
			"exit 1\r\n";

		/// <summary>
		/// Files already in Mods that this download will replace. The previous copy is listed
		/// so the apply script can move it, with the same path, into this join's removed folder.
		/// </summary>
		private static List<string> ListFilesStagingWillReplace(string modsDir, string removedDir)
		{
			List<string> pairs = new List<string>();
			string staging = ModSyncCommon.StagingDir;
			if (!Directory.Exists(staging))
			{
				return pairs;
			}

			string[] staged = Directory.GetFiles(staging, "*", SearchOption.AllDirectories);
			for (int i = 0; i < staged.Length; i++)
			{
				string rel = staged[i].Substring(staging.Length).TrimStart('\\', '/');
				string current = Path.Combine(modsDir, rel);
				if (!File.Exists(current))
				{
					continue;
				}

				pairs.Add(current + "|" + Path.Combine(removedDir, rel));
			}

			return pairs;
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
