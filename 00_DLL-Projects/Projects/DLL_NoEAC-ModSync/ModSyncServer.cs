using System;
using System.Collections.Generic;
using System.IO;

namespace ModSync
{
	/// <summary>
	/// Server half. Publishes a manifest of everything in the Mods folder and streams
	/// requested files to clients that have not logged in yet.
	/// </summary>
	public static class ModSyncServer
	{
		private class Transfer
		{
			public ClientInfo Client;
			public List<int> Files = new List<int>();
			public int FilePos;
			public int ChunkIndex;
			public int ChunkCount;
			public FileStream Stream;
			public int ChunksSent;
			public int ChunksAcked;
		}

		/// <summary>Across all downloaders, per tick. Keeps sending from eating a whole frame.</summary>
		private const int MaxChunksPerTick = 24;

		private static double baseBytesPerSecond = 10.0 * 1024 * 1024;
		private static bool capSetByAdmin;
		private static int maxActive = 6;
		private static int maxActiveSetByAdmin;
		private static int windowChunks = ModSyncCommon.MaxChunksInFlight;

		private static double sendTokens;
		private static DateTime lastRefill = DateTime.UtcNow;
		private static DateTime lastQueueNotice = DateTime.MinValue;
		private static int roundRobin;

		private static int playersOnline;
		private static DateTime playersCountedAt = DateTime.MinValue;

		private static readonly List<ManifestEntry> manifest = new List<ManifestEntry>();
		private static ModSyncHashCache hashes;
		private static readonly List<Transfer> transfers = new List<Transfer>();
		private static readonly List<string> excludes = new List<string>();

		private static bool manifestBuilt;
		private static int manifestVersion;
		private static DateTime manifestBuiltAt = DateTime.MinValue;
		private static byte[][] manifestChunks;
		private static bool registered;

		public static void Init(string modPath)
		{
			ReadServerConfig(modPath);
			if (!registered)
			{
				registered = true;
				ModEvents.GameUpdate.RegisterHandler(OnGameUpdate);
				// Hash the mods now, while nobody is waiting on it.
				ModEvents.GameStartDone.RegisterHandler(OnGameStartDone);
			}
			ModSyncCommon.Info("Server half ready. Serving mods from " + ModSyncCommon.ServerModsDir);
		}

		private static void ReadServerConfig(string modPath)
		{
			excludes.Clear();
			if (string.IsNullOrEmpty(modPath))
			{
				return;
			}

			string file = Path.Combine(modPath, "modsync-server.txt");
			if (!File.Exists(file))
			{
				return;
			}

			foreach (string raw in File.ReadAllLines(file))
			{
				string line = raw.Trim();
				if (line.Length == 0 || line.StartsWith("#"))
				{
					continue;
				}

				int eq = line.IndexOf('=');
				if (eq < 1)
				{
					continue;
				}

				string key = line.Substring(0, eq).Trim().ToUpperInvariant();
				string val = line.Substring(eq + 1).Trim();
				if (key == "EXCLUDE" && val.Length > 0)
				{
					excludes.Add(val.ToLowerInvariant());
				}
				else if (key == "MAXMBPERSEC")
				{
					double mb;
					if (double.TryParse(val, out mb) && mb > 0.0)
					{
						baseBytesPerSecond = mb * 1024 * 1024;
						capSetByAdmin = true;
						ModSyncCommon.Info("Mod sending capped at " + mb + " MB/s for all clients together.");
					}
				}
				else if (key == "MAXDOWNLOADERS")
				{
					int n;
					if (int.TryParse(val, out n) && n > 0)
					{
						maxActiveSetByAdmin = n;
						ModSyncCommon.Info("Serving at most " + n + " downloader(s) at a time.");
					}
				}
			}

			if (excludes.Count > 0)
			{
				ModSyncCommon.Info("Excluding " + excludes.Count + " mod folder(s) from sync.");
			}
		}

		private static bool IsExcluded(string relPath)
		{
			if (excludes.Count == 0)
			{
				return false;
			}

			int slash = relPath.IndexOf('/');
			string folder = (slash > 0 ? relPath.Substring(0, slash) : relPath).ToLowerInvariant();
			return excludes.Contains(folder);
		}

		private static void BuildManifest()
		{
			// Built once. The server loads its mods at startup, so this snapshot is what the
			// server is actually running. Rescanning live would sync clients to files the
			// server has not loaded and make them mismatch instead of match.
			if (manifestBuilt)
			{
				return;
			}

			manifestBuiltAt = DateTime.UtcNow;
			manifestVersion++;

			if (hashes == null)
			{
				hashes = new ModSyncHashCache(Path.Combine(ModSyncCommon.GameDir, "ModSync-ServerHashes.txt"));
			}

			DateTime started = DateTime.UtcNow;

			manifest.Clear();
			string root = ModSyncCommon.ServerModsDir;
			if (!Directory.Exists(root))
			{
				ModSyncCommon.Warn("No Mods folder at " + root);
				manifestBuilt = true;
				manifestChunks = SliceManifest();
				return;
			}

			long total = 0;
			string[] files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
			ModSyncCommon.Info("Checking " + files.Length + " mod file(s). The first run after a mod "
				+ "change reads them all, which can take a few minutes on a large pack.");
			for (int i = 0; i < files.Length; i++)
			{
				string full = files[i];
				string rel = ModSyncCommon.NormalizeRel(full.Substring(root.Length));
				if (IsExcluded(rel))
				{
					continue;
				}

				FileInfo fi = new FileInfo(full);
				string hash;
				try
				{
					hash = hashes.Get(full);
				}
				catch (Exception ex)
				{
					ModSyncCommon.Warn("Could not hash " + rel + ": " + ex.Message);
					continue;
				}

				if (hash == null)
				{
					continue;
				}

				manifest.Add(new ManifestEntry { RelPath = rel, Size = fi.Length, Hash = hash });
				total += fi.Length;
			}

			manifestBuilt = true;
			manifestChunks = SliceManifest();
			hashes.Save();
			ChooseProfile(total);
			ModSyncCommon.Info("Manifest built: " + manifest.Count + " files, " + ModSyncCommon.HumanSize(total)
				+ " in " + manifestChunks.Length + " chunk(s), took "
				+ (DateTime.UtcNow - started).TotalSeconds.ToString("0.0") + "s.");
		}

		/// <summary>
		/// Picks how hard to push based on how big the mod set actually is. A 40 MB pack can be
		/// handed to everyone at once and is gone in seconds. A 10 GB pack is served a few
		/// players at a time: three finishing in twenty minutes each beats ten crawling for two
		/// hours, and a queued player costs the server nothing while it waits.
		/// </summary>
		private static void ChooseProfile(long packBytes)
		{
			const long Mb = 1024L * 1024L;
			int autoActive;
			double autoMbPerSec;

			if (packBytes < 512L * Mb)
			{
				autoActive = 12;
				windowChunks = 48;
				autoMbPerSec = 20.0;
			}
			else if (packBytes < 4096L * Mb)
			{
				autoActive = 6;
				windowChunks = 64;
				autoMbPerSec = 15.0;
			}
			else
			{
				autoActive = 3;
				// A longer leash keeps a distant player's connection full between acks.
				windowChunks = 96;
				autoMbPerSec = 10.0;
			}

			maxActive = maxActiveSetByAdmin > 0 ? maxActiveSetByAdmin : autoActive;
			if (!capSetByAdmin)
			{
				baseBytesPerSecond = autoMbPerSec * Mb;
			}

			ModSyncCommon.Info("Mod set is " + ModSyncCommon.HumanSize(packBytes) + ": up to " + maxActive
				+ " downloader(s) at once, " + (baseBytesPerSecond / Mb).ToString("0.#") + " MB/s shared.");
		}

		/// <summary>
		/// People already in the world come first. The more of them there are, the less of the
		/// uplink new arrivals are allowed to take.
		/// </summary>
		private static double LoadFactor()
		{
			DateTime now = DateTime.UtcNow;
			if ((now - playersCountedAt).TotalSeconds >= 1.0)
			{
				playersCountedAt = now;
				playersOnline = 0;
				try
				{
					ConnectionManager cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
					if (cm != null && cm.Clients != null && cm.Clients.List != null)
					{
						System.Collections.ObjectModel.ReadOnlyCollection<ClientInfo> list = cm.Clients.List;
						for (int i = 0; i < list.Count; i++)
						{
							if (list[i] != null && list[i].loginDone)
							{
								playersOnline++;
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}

			if (playersOnline == 0)
			{
				return 1.0;
			}
			if (playersOnline <= 3)
			{
				return 0.75;
			}
			return playersOnline <= 7 ? 0.55 : 0.4;
		}

		private static byte[][] SliceManifest()
		{
			byte[] packed = ModSyncCommon.SerializeManifest(manifest);
			int count = Math.Max(1, (packed.Length + ModSyncCommon.ChunkSize - 1) / ModSyncCommon.ChunkSize);
			byte[][] chunks = new byte[count][];
			for (int i = 0; i < count; i++)
			{
				int offset = i * ModSyncCommon.ChunkSize;
				int len = Math.Min(ModSyncCommon.ChunkSize, packed.Length - offset);
				byte[] slice = new byte[len];
				Array.Copy(packed, offset, slice, 0, len);
				chunks[i] = slice;
			}
			return chunks;
		}

		/// <summary>Drop the cached manifest so the next client picks up mod changes.</summary>
		public static void Invalidate()
		{
			manifestBuilt = false;
		}

		public static void OnHello(ClientInfo client, int protocol)
		{
			if (client == null)
			{
				return;
			}

			BuildManifest();
			ModSyncCommon.Info("Client " + Describe(client) + " asked for the mod manifest (protocol " + protocol + ").");
			for (int i = 0; i < manifestChunks.Length; i++)
			{
				client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncManifest>()
					.Setup(manifestVersion, i, manifestChunks.Length, manifestChunks[i]));
			}
		}

		public static void OnRequest(ClientInfo client, int version, int[] indices)
		{
			if (client == null || indices == null)
			{
				return;
			}

			// The mod list was rebuilt after this client read it, so its indices no longer line up.
			if (version != manifestVersion)
			{
				ModSyncCommon.Info("Mod list changed while " + Describe(client) + " was comparing. Asking it to recheck.");
				client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncDone>()
					.Setup(false, ModSyncCommon.ManifestChangedMessage));
				return;
			}

			Abort(client);

			if (indices.Length == 0)
			{
				client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncDone>().Setup(true, "Nothing to send"));
				return;
			}

			Transfer t = new Transfer { Client = client };
			long bytes = 0;
			for (int i = 0; i < indices.Length; i++)
			{
				int idx = indices[i];
				if (idx >= 0 && idx < manifest.Count)
				{
					t.Files.Add(idx);
					bytes += manifest[idx].Size;
				}
			}

			if (t.Files.Count == 0)
			{
				client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncDone>().Setup(true, "Nothing to send"));
				return;
			}

			transfers.Add(t);

			int place = transfers.Count - maxActive;
			if (place > 0)
			{
				ModSyncCommon.Info("Queued " + Describe(client) + " at place " + place + " for "
					+ ModSyncCommon.HumanSize(bytes) + ".");
				client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncWait>().Setup(place, place));
				return;
			}

			ModSyncCommon.Info("Sending " + t.Files.Count + " file(s), " + ModSyncCommon.HumanSize(bytes)
				+ " to " + Describe(client));
		}

		public static void OnAck(ClientInfo client, int received)
		{
			Transfer t = Find(client);
			if (t != null)
			{
				t.ChunksAcked = received;
			}
		}

		private static Transfer Find(ClientInfo client)
		{
			for (int i = 0; i < transfers.Count; i++)
			{
				if (transfers[i].Client == client)
				{
					return transfers[i];
				}
			}
			return null;
		}

		private static void Abort(ClientInfo client)
		{
			Transfer t = Find(client);
			if (t == null)
			{
				return;
			}
			CloseStream(t);
			transfers.Remove(t);
		}

		private static void CloseStream(Transfer t)
		{
			if (t.Stream != null)
			{
				try
				{
					t.Stream.Dispose();
				}
				catch (Exception)
				{
				}
				t.Stream = null;
			}
		}

		private static string Describe(ClientInfo client)
		{
			try
			{
				return client.ip ?? "client";
			}
			catch (Exception)
			{
				return "client";
			}
		}

		private static void OnGameStartDone(ref ModEvents.SGameStartDoneData _)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance != null
				&& SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				BuildManifest();
			}
		}

		/// <summary>
		/// Refills the shared send allowance. Paced by elapsed time rather than per tick so the
		/// cap means the same thing whatever frame rate the server runs at.
		/// </summary>
		private static void RefillTokens()
		{
			DateTime now = DateTime.UtcNow;
			double elapsed = (now - lastRefill).TotalSeconds;
			lastRefill = now;

			if (elapsed <= 0.0)
			{
				return;
			}

			double budget = baseBytesPerSecond * LoadFactor();
			sendTokens += elapsed * budget;
			if (sendTokens > budget)
			{
				sendTokens = budget;
			}
		}

		private static void OnGameUpdate(ref ModEvents.SGameUpdateData _)
		{
			if (transfers.Count == 0)
			{
				lastRefill = DateTime.UtcNow;
				return;
			}

			// Drop anyone who left first, queued or not, so a slot is never held by a dead connection.
			for (int i = transfers.Count - 1; i >= 0; i--)
			{
				if (Gone(transfers[i]))
				{
					CloseStream(transfers[i]);
					transfers.RemoveAt(i);
				}
			}

			if (transfers.Count == 0)
			{
				lastRefill = DateTime.UtcNow;
				return;
			}

			RefillTokens();

			// Only the front of the queue is served. The rest wait their turn.
			int active = Math.Min(maxActive, transfers.Count);
			int share = Math.Max(1, MaxChunksPerTick / active);

			// Rotate the order so the same client is not always first at the allowance.
			roundRobin = (roundRobin + 1) % active;

			List<Transfer> finished = new List<Transfer>();
			for (int n = 0; n < active; n++)
			{
				Transfer t = transfers[(n + roundRobin) % active];
				try
				{
					if (!Pump(t, share))
					{
						finished.Add(t);
					}
				}
				catch (Exception ex)
				{
					ModSyncCommon.Error("Transfer failed: " + ex);
					finished.Add(t);
					try
					{
						t.Client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncDone>()
							.Setup(false, "Server error: " + ex.Message));
					}
					catch (Exception)
					{
					}
				}
			}

			for (int n = 0; n < finished.Count; n++)
			{
				CloseStream(finished[n]);
				transfers.Remove(finished[n]);
			}

			NotifyQueued();
		}

		private static bool Gone(Transfer t)
		{
			try
			{
				return t.Client == null || t.Client.netConnection == null
					|| t.Client.netConnection[0] == null || t.Client.netConnection[0].IsDisconnected();
			}
			catch (Exception)
			{
				return true;
			}
		}

		/// <summary>Keeps waiting players told where they are in line instead of staring at nothing.</summary>
		private static void NotifyQueued()
		{
			int active = Math.Min(maxActive, transfers.Count);
			int waiting = transfers.Count - active;
			if (waiting <= 0)
			{
				return;
			}

			DateTime now = DateTime.UtcNow;
			if ((now - lastQueueNotice).TotalSeconds < 3.0)
			{
				return;
			}
			lastQueueNotice = now;

			for (int i = active; i < transfers.Count; i++)
			{
				try
				{
					transfers[i].Client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncWait>()
						.Setup(i - active + 1, waiting));
				}
				catch (Exception)
				{
				}
			}
		}

		/// <summary>Sends up to this client's share of chunks. Returns false when the transfer is finished.</summary>
		private static bool Pump(Transfer t, int maxChunks)
		{
			int budget = maxChunks;
			byte[] buffer = new byte[ModSyncCommon.ChunkSize];

			while (budget > 0 && sendTokens >= ModSyncCommon.ChunkSize
				&& t.ChunksSent - t.ChunksAcked < windowChunks)
			{
				if (t.Stream == null)
				{
					if (t.FilePos >= t.Files.Count)
					{
						t.Client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncDone>()
							.Setup(true, "All files sent"));
						ModSyncCommon.Info("Finished sending to " + Describe(t.Client));
						return false;
					}

					ManifestEntry entry = manifest[t.Files[t.FilePos]];
					string full = Path.Combine(ModSyncCommon.ServerModsDir, entry.RelPath.Replace('/', Path.DirectorySeparatorChar));
					if (!File.Exists(full))
					{
						ModSyncCommon.Warn("Manifest file vanished: " + entry.RelPath);
						t.FilePos++;
						continue;
					}

					t.Stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					t.ChunkIndex = 0;
					t.ChunkCount = (int)Math.Max(1, (t.Stream.Length + ModSyncCommon.ChunkSize - 1) / ModSyncCommon.ChunkSize);
				}

				int read = t.Stream.Read(buffer, 0, buffer.Length);
				byte[] payload;
				if (read == buffer.Length)
				{
					payload = (byte[])buffer.Clone();
				}
				else
				{
					payload = new byte[read];
					Array.Copy(buffer, payload, read);
				}

				t.Client.SendPackage(NetPackageManager.GetPackage<NetPackageModSyncData>()
					.Setup(t.Files[t.FilePos], t.ChunkIndex, t.ChunkCount, payload));

				t.ChunkIndex++;
				t.ChunksSent++;
				budget--;
				sendTokens -= payload.Length;

				if (t.ChunkIndex >= t.ChunkCount || read == 0)
				{
					CloseStream(t);
					t.FilePos++;
				}
			}

			return true;
		}
	}
}
