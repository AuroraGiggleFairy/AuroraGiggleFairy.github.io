using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Level kills, items, secrets, and time live on the server instance. A restart
	/// makes a new instance. The dedicated server writes the player file from the
	/// client upload, so cvars set only on the server were dropped. The tally is
	/// stored in the world save and put back when that same level is entered again.
	/// Cleared spawner, item, and secret spots are stored with that tally and put
	/// back onto a rebuilt level. A logout while the server stays up keeps the
	/// loaded copy and rebinds it when that player returns.
	/// </summary>
	internal static class RunStats
	{
		private static readonly FieldInfo Started = AccessTools.Field(typeof(Stats), "_started");
		private static readonly Dictionary<string, Record> Runs = new Dictionary<string, Record>();
		private static readonly Dictionary<int, string> PersistentByEntity = new Dictionary<int, string>();
		private static readonly Dictionary<int, Vector3i> SpawnedAt = new Dictionary<int, Vector3i>();
		private static readonly Dictionary<string, Parked> Parks = new Dictionary<string, Parked>();
		private static readonly Dictionary<int, float> ApplyUntil = new Dictionary<int, float>();
		private static readonly Dictionary<int, Claim> GrantClaims = new Dictionary<int, Claim>();
		private static readonly Dictionary<int, Claim> PackageClaims = new Dictionary<int, Claim>();
		private static readonly HashSet<int> Pending = new HashSet<int>();
		private static readonly HashSet<int> PendingRejoin = new HashSet<int>();
		private static FieldInfo _byPlayer;
		private static FieldInfo _instancePlayerId;
		private static bool _loaded;
		private static bool _host;
		private static float _next;

		internal static bool PreserveOnRelease;

		private struct Parked
		{
			public int EntityId;
			public Vector3 Position;
		}

		private struct Claim
		{
			public int Count;
			public float Until;
		}

		private struct Record
		{
			public string Map;
			public int Kills;
			public int Items;
			public int Secrets;
			public float Seconds;
			public HashSet<Vector3i> Spawners;
			public HashSet<Vector3i> ItemsTaken;
			public HashSet<Vector3i> SecretsFound;
			public HashSet<Vector3i> Triggers;
			public HashSet<Vector3i> Movers;
			public HashSet<Vector3i> MapChunks;
			public HashSet<int> SeenLines;
		}

		private static readonly Dictionary<string, string> LastMap = new Dictionary<string, string>();

		internal static void Tick()
		{
			if (!Authority())
			{
				return;
			}

			if (Time.time < _next)
			{
				return;
			}

			_next = Time.time + 2f;
			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null)
			{
				return;
			}

			for (int i = 0; i < world.Players.list.Count; i++)
			{
				EntityPlayer player = world.Players.list[i];
				if (player != null && Instances.IsInside(player.entityId))
				{
					if (Pending.Contains(player.entityId))
					{
						Restore(player);
					}
					else
					{
						Save(player);
					}

					ApplyDue(player.entityId);
				}
			}

			RetryRejoin(world);
			SweepClaims();
		}

		internal static void Flush()
		{
			if (!Authority())
			{
				return;
			}

			World world = GameManager.Instance?.World;
			if (world?.Players?.list != null)
			{
				for (int i = 0; i < world.Players.list.Count; i++)
				{
					EntityPlayer player = world.Players.list[i];
					if (player != null && Instances.IsInside(player.entityId))
					{
						Save(player);
					}
				}
			}

			Write();
		}

		internal static void Save(EntityPlayer player)
		{
			if (player == null || !Instances.IsInside(player.entityId))
			{
				return;
			}

			Stats stats = Instances.StatsFor(player.entityId);
			string map = Instances.MapFor(player.entityId);
			string id = IdOf(player);
			if (stats == null || string.IsNullOrEmpty(map) || string.IsNullOrEmpty(id))
			{
				return;
			}

			EnsureLoaded();
			PersistentByEntity[player.entityId] = id;
			HashSet<Vector3i> spawners;
			HashSet<Vector3i> taken;
			HashSet<Vector3i> secrets;
			string rowKey = RowKey(id, map);
			HashSet<Vector3i> triggers;
			HashSet<Vector3i> movers;
			HashSet<Vector3i> chunks;
			HashSet<int> seenLines;
			if (Runs.TryGetValue(rowKey, out Record existing))
			{
				spawners = existing.Spawners ?? new HashSet<Vector3i>();
				taken = existing.ItemsTaken ?? new HashSet<Vector3i>();
				secrets = existing.SecretsFound ?? new HashSet<Vector3i>();
				triggers = existing.Triggers ?? new HashSet<Vector3i>();
				movers = existing.Movers ?? new HashSet<Vector3i>();
				chunks = existing.MapChunks ?? new HashSet<Vector3i>();
				seenLines = existing.SeenLines ?? new HashSet<int>();
			}
			else
			{
				spawners = new HashSet<Vector3i>();
				taken = new HashSet<Vector3i>();
				secrets = new HashSet<Vector3i>();
				triggers = new HashSet<Vector3i>();
				movers = new HashSet<Vector3i>();
				chunks = new HashSet<Vector3i>();
				seenLines = new HashSet<int>();
			}

			LastMap[id] = map;
			Runs[rowKey] = new Record
			{
				Map = map,
				Kills = stats.Kills,
				Items = stats.Items,
				Secrets = stats.Secrets,
				Seconds = stats.Seconds,
				Spawners = spawners,
				ItemsTaken = taken,
				SecretsFound = secrets,
				Triggers = triggers,
				Movers = movers,
				MapChunks = chunks,
				SeenLines = seenLines
			};
			Write();
		}

		internal static void Clear(EntityPlayer player)
		{
			if (player == null)
			{
				return;
			}

			string id = IdOf(player);
			if (string.IsNullOrEmpty(id))
			{
				return;
			}

			EnsureLoaded();
			string map = Instances.MapFor(player.entityId);
			if (string.IsNullOrEmpty(map) || !Runs.Remove(RowKey(id, map)))
			{
				return;
			}

			string last;
			if (LastMap.TryGetValue(id, out last) && string.Equals(last, map, StringComparison.OrdinalIgnoreCase))
			{
				LastMap.Remove(id);
			}

			Write();
		}

		private static void Restore(EntityPlayer player)
		{
			if (player == null || !Instances.IsInside(player.entityId))
			{
				return;
			}

			Stats stats = Instances.StatsFor(player.entityId);
			string map = Instances.MapFor(player.entityId);
			string id = IdOf(player);
			if (stats == null || string.IsNullOrEmpty(map))
			{
				return;
			}

			if (string.IsNullOrEmpty(id))
			{
				if (stats.Kills == 0 && stats.Items == 0 && stats.Secrets == 0 && stats.Seconds < 2f)
				{
					Pending.Add(player.entityId);
				}

				return;
			}

			Pending.Remove(player.entityId);
			EnsureLoaded();
			bool untouched = stats.Kills == 0 && stats.Items == 0 && stats.Secrets == 0;
			if (!Runs.TryGetValue(RowKey(id, map), out Record saved))
			{
				if (untouched)
				{
					Save(player);
				}

				return;
			}

			if (!untouched)
			{
				return;
			}

			stats.Kills = saved.Kills;
			stats.Items = saved.Items;
			stats.Secrets = saved.Secrets;
			if (Started != null)
			{
				Started.SetValue(stats, Time.time - Mathf.Max(0f, saved.Seconds));
			}

			Debug.Log("[DoomMultiplayer] restored " + map + " tally for " + id +
				" kills " + saved.Kills + " items " + saved.Items + " secrets " + saved.Secrets +
				" time " + Mathf.FloorToInt(saved.Seconds));
		}

		private static string IdOf(EntityPlayer player)
		{
			PersistentPlayerData data = GameManager.Instance?.persistentPlayers?.GetPlayerDataFromEntityID(player.entityId);
			string id = data?.PrimaryId?.CombinedString;
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}

			return id.Replace("|", "_").Replace("\r", "").Replace("\n", "");
		}

		private static string FilePath()
		{
			try
			{
				string dir = GameIO.GetSaveGameDir();
				if (string.IsNullOrEmpty(dir))
				{
					return null;
				}

				return Path.Combine(dir, "doom-run-stats.txt");
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static void EnsureLoaded()
		{
			if (_loaded)
			{
				return;
			}

			string path = FilePath();
			if (path == null)
			{
				return;
			}

			_loaded = true;
			if (!File.Exists(path))
			{
				return;
			}

			string[] lines;
			try
			{
				lines = File.ReadAllLines(path);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[DoomMultiplayer] could not read run stats: " + e.Message);
				return;
			}

			for (int i = 0; i < lines.Length; i++)
			{
				string[] parts = lines[i].Split('|');
				if (parts.Length < 3 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
				{
					continue;
				}

				if (parts[1] == "*")
				{
					LastMap[parts[0]] = parts[2];
					continue;
				}

				if (parts.Length < 6)
				{
					continue;
				}

				LastMap[parts[0]] = parts[1];
				Runs[RowKey(parts[0], parts[1])] = new Record
				{
					Map = parts[1],
					Kills = ParseInt(parts[2]),
					Items = ParseInt(parts[3]),
					Secrets = ParseInt(parts[4]),
					Seconds = ParseFloat(parts[5]),
					Spawners = ParseSpots(parts.Length > 6 ? parts[6] : ""),
					ItemsTaken = ParseSpots(parts.Length > 7 ? parts[7] : ""),
					SecretsFound = ParseSpots(parts.Length > 8 ? parts[8] : ""),
					Triggers = ParseSpots(parts.Length > 9 ? parts[9] : ""),
					Movers = ParseSpots(parts.Length > 10 ? parts[10] : ""),
					MapChunks = ParseSpots(parts.Length > 11 ? parts[11] : ""),
					SeenLines = ParseLines(parts.Length > 12 ? parts[12] : "")
				};
			}
		}

		private static void Write()
		{
			if (!Authority())
			{
				return;
			}

			string path = FilePath();
			if (path == null)
			{
				return;
			}

			try
			{
				List<string> lines = new List<string>(Runs.Count + LastMap.Count);
				foreach (KeyValuePair<string, string> last in LastMap)
				{
					lines.Add(last.Key + "|*|" + last.Value);
				}

				foreach (KeyValuePair<string, Record> pair in Runs)
				{
					Record row = pair.Value;
					int split = pair.Key.IndexOf('\t');
					string playerId = split < 0 ? pair.Key : pair.Key.Substring(0, split);
					lines.Add(string.Concat(
						playerId, "|",
						row.Map, "|",
						row.Kills.ToString(CultureInfo.InvariantCulture), "|",
						row.Items.ToString(CultureInfo.InvariantCulture), "|",
						row.Secrets.ToString(CultureInfo.InvariantCulture), "|",
						row.Seconds.ToString("0.###", CultureInfo.InvariantCulture), "|",
						FormatSpots(row.Spawners), "|",
						FormatSpots(row.ItemsTaken), "|",
						FormatSpots(row.SecretsFound), "|",
						FormatSpots(row.Triggers), "|",
						FormatSpots(row.Movers), "|",
						FormatSpots(row.MapChunks), "|",
						FormatLines(row.SeenLines)));
				}

				File.WriteAllLines(path, lines);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[DoomMultiplayer] could not write run stats: " + e.Message);
			}
		}

		private static int ParseInt(string text)
		{
			int value;
			return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
		}

		private static float ParseFloat(string text)
		{
			float value;
			return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0f;
		}

		private static bool Authority()
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net != null)
			{
				_host = net.IsServer;
				return _host;
			}

			return _host;
		}

		internal static void NoteSpawn(int entityId, Vector3i blockPos)
		{
			if (entityId > 0)
			{
				SpawnedAt[entityId] = blockPos;
			}
		}

		internal static void MonsterKilled(Entity killed)
		{
			if (killed == null || !Authority() || !SpawnedAt.TryGetValue(killed.entityId, out Vector3i blockPos))
			{
				return;
			}

			SpawnedAt.Remove(killed.entityId);
			if (!Instances.IsInstanceSpace(killed.position))
			{
				return;
			}

			Mark(blockPos, Spot.Spawner);
		}

		internal static void ItemTaken(Vector3i blockPos)
		{
			if (Authority())
			{
				Mark(blockPos, Spot.Item);
			}
		}

		internal static void SecretFound(Vector3i blockPos)
		{
			if (Authority())
			{
				Mark(blockPos, Spot.Secret);
			}
		}

		internal static bool SpawnerCleared(Vector3i worldPos)
		{
			if (!Authority() || !TryRow(worldPos, out Record row, out Vector3i local))
			{
				return false;
			}

			return row.Spawners != null && row.Spawners.Contains(local);
		}

		internal static void ApplyLoaded()
		{
			if (!Authority())
			{
				return;
			}

			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null)
			{
				return;
			}

			for (int i = 0; i < world.Players.list.Count; i++)
			{
				EntityPlayer player = world.Players.list[i];
				if (player != null && Instances.IsInside(player.entityId))
				{
					ApplyDue(player.entityId);
				}
			}
		}

		internal static void Park(ClientInfo client)
		{
			if (client == null || client.entityId < 0 || !Authority() || !Instances.IsInside(client.entityId))
			{
				return;
			}

			World world = GameManager.Instance?.World;
			EntityPlayer player = world == null ? null : world.GetEntity(client.entityId) as EntityPlayer;
			if (player == null)
			{
				return;
			}

			Save(player);
			string id = IdOf(player);
			if (string.IsNullOrEmpty(id))
			{
				return;
			}

			Parks[id] = new Parked
			{
				EntityId = player.entityId,
				Position = player.position
			};
			Debug.Log("[DoomMultiplayer] parked level for " + id + " at " + player.position);
		}

		internal static void Rejoin(EntityPlayer player)
		{
			if (player == null || !Authority())
			{
				return;
			}

			string id = IdOf(player);
			if (string.IsNullOrEmpty(id))
			{
				PendingRejoin.Add(player.entityId);
				return;
			}

			PendingRejoin.Remove(player.entityId);
			if (Instances.IsInside(player.entityId))
			{
				ReleaseOrphan(id, player.entityId);
				WatchApply(player.entityId);
				return;
			}

			if (Parks.TryGetValue(id, out Parked parked) && Rekey(parked.EntityId, player.entityId))
			{
				Parks.Remove(id);
				if (!Instances.IsInstanceSpace(player.position))
				{
					Instances.Teleport(player, parked.Position, null);
				}

				if (player.Buffs != null && !player.Buffs.HasBuff("doomBuffInLevel"))
				{
					player.Buffs.AddBuff("doomBuffInLevel");
				}

				WatchApply(player.entityId);
				Debug.Log("[DoomMultiplayer] rebound " + id + " to the loaded level");
				return;
			}

			Parks.Remove(id);
			if (!Instances.IsInstanceSpace(player.position))
			{
				return;
			}

			EnsureLoaded();
			string resumed;
			if (!LastMap.TryGetValue(id, out resumed) ||
				!Runs.TryGetValue(RowKey(id, resumed), out Record saved) ||
				string.IsNullOrEmpty(saved.Map))
			{
				return;
			}

			Instances.Enter(player, saved.Map);
		}

		internal static bool AllowGrant(int playerId)
		{
			if (TakeClaim(PackageClaims, playerId))
			{
				return false;
			}

			AddClaim(GrantClaims, playerId);
			return true;
		}

		internal static bool AllowPackage(int playerId)
		{
			if (TakeClaim(GrantClaims, playerId))
			{
				return false;
			}

			AddClaim(PackageClaims, playerId);
			return true;
		}

		private enum Spot
		{
			Spawner,
			Item,
			Secret,
			Trigger,
			Mover,
			Chunk
		}

		private static string RowKey(string playerId, string map)
		{
			return playerId + "\t" + (map ?? "");
		}

		private static void Mark(Vector3i worldPos, Spot spot)
		{
			if (!TryOwner(worldPos, out int playerId, out Vector3i origin, out string persistentId, out Record row))
			{
				return;
			}

			Vector3i local = new Vector3i(worldPos.x - origin.x, worldPos.y - origin.y, worldPos.z - origin.z);
			HashSet<Vector3i> set = SetFor(row, spot);
			if (set == null || !set.Add(local))
			{
				return;
			}

			row = PutSet(row, spot, set);
			Runs[RowKey(persistentId, row.Map)] = row;
			Write();
			EntityPlayer player = GameManager.Instance?.World?.GetEntity(playerId) as EntityPlayer;
			if (player != null)
			{
				Save(player);
			}
		}

		private static void WatchApply(int playerId)
		{
			ApplyUntil[playerId] = Time.time + 20f;
			ApplyDue(playerId);
		}

		private static void ApplyDue(int playerId)
		{
			if (!ApplyUntil.TryGetValue(playerId, out float until))
			{
				return;
			}

			if (Time.time > until || !Instances.IsInside(playerId))
			{
				ApplyUntil.Remove(playerId);
				return;
			}

			ApplyPlayer(playerId);
		}

		private static void ApplyPlayer(int playerId)
		{
			World world = GameManager.Instance?.World;
			if (world == null || !Instances.TryGet(playerId, out Vector3i origin, out Level _, out int _))
			{
				return;
			}

			if (!PersistentByEntity.TryGetValue(playerId, out string id))
			{
				EntityPlayer player = world.GetEntity(playerId) as EntityPlayer;
				id = player == null ? null : IdOf(player);
			}

			if (string.IsNullOrEmpty(id))
			{
				return;
			}

			string map = Instances.MapFor(playerId);
			EnsureLoaded();
			if (string.IsNullOrEmpty(map) || !Runs.TryGetValue(RowKey(id, map), out Record row))
			{
				return;
			}

			int changed = 0;
			changed += ApplySpawners(world, origin, row.Spawners);
			changed += ApplyAir(world, origin, row.ItemsTaken);
			changed += ApplySecrets(origin, row.SecretsFound);
			changed += ApplyBits(origin, row.Triggers, BlockState.TriggerSpentBit);
			changed += ApplyBits(origin, row.Movers, BlockState.MoverMovedBit);
			if (changed > 0)
			{
				Debug.Log("[DoomMultiplayer] kept " + map + " cleared spots, spawners " +
					(row.Spawners == null ? 0 : row.Spawners.Count) + " items " +
					(row.ItemsTaken == null ? 0 : row.ItemsTaken.Count) + " secrets " +
					(row.SecretsFound == null ? 0 : row.SecretsFound.Count));
			}
		}

		private static int ApplySpawners(World world, Vector3i origin, HashSet<Vector3i> spots)
		{
			if (spots == null)
			{
				return 0;
			}

			int changed = 0;
			foreach (Vector3i local in spots)
			{
				Vector3i at = new Vector3i(origin.x + local.x, origin.y + local.y, origin.z + local.z);
				BlockValue state = world.GetBlock(at);
				if (!(state.Block is BlockSpawner) || (state.rawData & Spawner.SpentBit) != 0)
				{
					continue;
				}

				state.rawData |= Spawner.SpentBit;
				world.SetBlockRPC(new BlockValueRef(at), state);
				changed++;
			}

			return changed;
		}

		private static int ApplyAir(World world, Vector3i origin, HashSet<Vector3i> spots)
		{
			if (spots == null)
			{
				return 0;
			}

			int changed = 0;
			foreach (Vector3i local in spots)
			{
				Vector3i at = new Vector3i(origin.x + local.x, origin.y + local.y, origin.z + local.z);
				BlockValue state = world.GetBlock(at);
				if (state.isair)
				{
					continue;
				}

				world.SetBlockRPC(new BlockValueRef(at), BlockValue.Air);
				changed++;
			}

			return changed;
		}

		private static int ApplySecrets(Vector3i origin, HashSet<Vector3i> spots)
		{
			if (spots == null)
			{
				return 0;
			}

			int changed = 0;
			foreach (Vector3i local in spots)
			{
				Vector3i at = new Vector3i(origin.x + local.x, origin.y + local.y, origin.z + local.z);
				if (BlockState.Get(at, BlockState.TriggerSpentBit))
				{
					continue;
				}

				BlockState.Set(at, BlockState.TriggerSpentBit, true);
				if (BlockState.Get(at, BlockState.TriggerSpentBit))
				{
					changed++;
				}
			}

			return changed;
		}

		private static bool TryRow(Vector3i worldPos, out Record row, out Vector3i local)
		{
			row = default;
			local = Vector3i.zero;
			if (!TryOwner(worldPos, out int _, out Vector3i origin, out string _, out row))
			{
				return false;
			}

			local = new Vector3i(worldPos.x - origin.x, worldPos.y - origin.y, worldPos.z - origin.z);
			return true;
		}

		private static bool TryOwner(Vector3i worldPos, out int playerId, out Vector3i origin, out string persistentId, out Record row)
		{
			playerId = -1;
			origin = Vector3i.zero;
			persistentId = null;
			row = default;
			EnsureFields();
			if (_byPlayer == null)
			{
				return false;
			}

			IDictionary map = _byPlayer.GetValue(null) as IDictionary;
			if (map == null)
			{
				return false;
			}

			foreach (DictionaryEntry entry in map)
			{
				object instance = entry.Value;
				if (instance == null)
				{
					continue;
				}

				Vector3i at = (Vector3i)OriginField(instance);
				Level level = LevelField(instance);
				if (level == null || worldPos.x < at.x || worldPos.x > at.x + level.Size.x ||
					worldPos.z < at.z || worldPos.z > at.z + level.Size.z)
				{
					continue;
				}

				playerId = (int)entry.Key;
				origin = at;
				if (!PersistentByEntity.TryGetValue(playerId, out persistentId))
				{
					EntityPlayer player = GameManager.Instance?.World?.GetEntity(playerId) as EntityPlayer;
					persistentId = player == null ? null : IdOf(player);
					if (!string.IsNullOrEmpty(persistentId))
					{
						PersistentByEntity[playerId] = persistentId;
					}
				}

				if (string.IsNullOrEmpty(persistentId))
				{
					return false;
				}

				string mapName = level.Name;
				string rowKey = RowKey(persistentId, mapName);
				EnsureLoaded();
				if (!Runs.TryGetValue(rowKey, out row))
				{
					EntityPlayer player = GameManager.Instance?.World?.GetEntity(playerId) as EntityPlayer;
					if (player != null)
					{
						Save(player);
					}

					if (!Runs.TryGetValue(rowKey, out row))
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}

		private static FieldInfo _originField;
		private static FieldInfo _levelField;

		private static Vector3i OriginField(object instance)
		{
			if (_originField == null)
			{
				_originField = instance.GetType().GetField("Origin");
			}

			return (Vector3i)_originField.GetValue(instance);
		}

		private static Level LevelField(object instance)
		{
			if (_levelField == null)
			{
				_levelField = instance.GetType().GetField("Level");
			}

			return _levelField.GetValue(instance) as Level;
		}

		private static HashSet<Vector3i> SetFor(Record row, Spot spot)
		{
			if (spot == Spot.Spawner)
			{
				return row.Spawners ?? new HashSet<Vector3i>();
			}

			if (spot == Spot.Item)
			{
				return row.ItemsTaken ?? new HashSet<Vector3i>();
			}

			if (spot == Spot.Secret)
			{
				return row.SecretsFound ?? new HashSet<Vector3i>();
			}

			if (spot == Spot.Trigger)
			{
				return row.Triggers ?? new HashSet<Vector3i>();
			}

			if (spot == Spot.Mover)
			{
				return row.Movers ?? new HashSet<Vector3i>();
			}

			return row.MapChunks ?? new HashSet<Vector3i>();
		}

		private static Record PutSet(Record row, Spot spot, HashSet<Vector3i> set)
		{
			if (spot == Spot.Spawner)
			{
				row.Spawners = set;
			}
			else if (spot == Spot.Item)
			{
				row.ItemsTaken = set;
			}
			else if (spot == Spot.Secret)
			{
				row.SecretsFound = set;
			}
			else if (spot == Spot.Trigger)
			{
				row.Triggers = set;
			}
			else if (spot == Spot.Mover)
			{
				row.Movers = set;
			}
			else
			{
				row.MapChunks = set;
			}

			return row;
		}

		internal static void NoteTrigger(Vector3i worldPos)
		{
			if (Authority())
			{
				Mark(worldPos, Spot.Trigger);
			}
		}

		internal static void NoteMover(Vector3i worldPos)
		{
			if (Authority())
			{
				Mark(worldPos, Spot.Mover);
			}
		}

		internal static void RememberChunks(int playerId, IList<Vector3i> localChunks)
		{
			if (!Authority() || localChunks == null || localChunks.Count == 0)
			{
				return;
			}

			string id;
			if (!PersistentByEntity.TryGetValue(playerId, out id))
			{
				EntityPlayer player = GameManager.Instance?.World?.GetEntity(playerId) as EntityPlayer;
				id = player == null ? null : IdOf(player);
			}

			string map = Instances.MapFor(playerId);
			if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(map))
			{
				return;
			}

			EnsureLoaded();
			string rowKey = RowKey(id, map);
			if (!Runs.TryGetValue(rowKey, out Record row))
			{
				return;
			}

			HashSet<Vector3i> chunks = row.MapChunks ?? new HashSet<Vector3i>();
			bool added = false;
			for (int i = 0; i < localChunks.Count; i++)
			{
				added |= chunks.Add(localChunks[i]);
			}

			if (!added)
			{
				return;
			}

			row.MapChunks = chunks;
			Runs[rowKey] = row;
			Write();
		}

		internal static void RememberLines(int playerId, IList<int> lines)
		{
			if (!Authority() || lines == null || lines.Count == 0)
			{
				return;
			}

			string id;
			if (!PersistentByEntity.TryGetValue(playerId, out id))
			{
				EntityPlayer player = GameManager.Instance?.World?.GetEntity(playerId) as EntityPlayer;
				id = player == null ? null : IdOf(player);
			}

			string map = Instances.MapFor(playerId);
			if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(map))
			{
				return;
			}

			EnsureLoaded();
			PersistentByEntity[playerId] = id;
			string rowKey = RowKey(id, map);
			if (!Runs.TryGetValue(rowKey, out Record row))
			{
				row = new Record
				{
					Map = map,
					SeenLines = new HashSet<int>()
				};
			}

			HashSet<int> seen = row.SeenLines ?? new HashSet<int>();
			bool added = false;
			for (int i = 0; i < lines.Count; i++)
			{
				added |= seen.Add(lines[i]);
			}

			if (!added)
			{
				return;
			}

			row.Map = map;
			row.SeenLines = seen;
			LastMap[id] = map;
			Runs[rowKey] = row;
			Write();
		}

		internal static List<int> LinesFor(int playerId)
		{
			var list = new List<int>();
			string id;
			if (!PersistentByEntity.TryGetValue(playerId, out id))
			{
				EntityPlayer player = GameManager.Instance?.World?.GetEntity(playerId) as EntityPlayer;
				id = player == null ? null : IdOf(player);
			}

			string map = Instances.MapFor(playerId);
			if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(map))
			{
				return list;
			}

			EnsureLoaded();
			PersistentByEntity[playerId] = id;
			Record row;
			if (Runs.TryGetValue(RowKey(id, map), out row) && row.SeenLines != null)
			{
				list.AddRange(row.SeenLines);
			}

			return list;
		}

		internal static bool TriggerCleared(Vector3i worldPos)
		{
			return SpotCleared(worldPos, Spot.Trigger);
		}

		internal static bool MoverCleared(Vector3i worldPos)
		{
			return SpotCleared(worldPos, Spot.Mover);
		}

		private static bool SpotCleared(Vector3i worldPos, Spot spot)
		{
			Record row;
			Vector3i local;
			if (!Authority() || !TryRow(worldPos, out row, out local))
			{
				return false;
			}

			HashSet<Vector3i> set = SetFor(row, spot);
			return set != null && set.Contains(local);
		}

		private static int ApplyBits(Vector3i origin, HashSet<Vector3i> spots, byte bit)
		{
			if (spots == null)
			{
				return 0;
			}

			int changed = 0;
			foreach (Vector3i local in spots)
			{
				Vector3i at = new Vector3i(origin.x + local.x, origin.y + local.y, origin.z + local.z);
				if (BlockState.Get(at, bit))
				{
					continue;
				}

				BlockState.Set(at, bit, true);
				if (BlockState.Get(at, bit))
				{
					changed++;
				}
			}

			return changed;
		}

		private static void EnsureFields()
		{
			if (_byPlayer != null)
			{
				return;
			}

			_byPlayer = AccessTools.Field(typeof(Instances), "ByPlayer");
			Type instanceType = typeof(Instances).GetNestedType("Instance", BindingFlags.NonPublic);
			_instancePlayerId = instanceType == null ? null : instanceType.GetField("PlayerId");
		}

		private static bool Rekey(int oldId, int newId)
		{
			if (oldId == newId)
			{
				return Instances.IsInside(newId);
			}

			EnsureFields();
			if (_byPlayer == null || _instancePlayerId == null)
			{
				return false;
			}

			IDictionary map = _byPlayer.GetValue(null) as IDictionary;
			if (map == null || !map.Contains(oldId) || map.Contains(newId))
			{
				return false;
			}

			object instance = map[oldId];
			map.Remove(oldId);
			_instancePlayerId.SetValue(instance, newId);
			map.Add(newId, instance);
			if (PersistentByEntity.TryGetValue(oldId, out string persistentId))
			{
				PersistentByEntity.Remove(oldId);
				PersistentByEntity[newId] = persistentId;
			}

			return true;
		}

		private static void ReleaseOrphan(string persistentId, int currentId)
		{
			if (!Parks.TryGetValue(persistentId, out Parked parked) || parked.EntityId == currentId)
			{
				Parks.Remove(persistentId);
				return;
			}

			Parks.Remove(persistentId);
			if (!Instances.IsInside(parked.EntityId))
			{
				return;
			}

			PreserveOnRelease = true;
			try
			{
				Instances.Release(parked.EntityId, true, true);
			}
			finally
			{
				PreserveOnRelease = false;
			}
		}

		private static void RetryRejoin(World world)
		{
			if (PendingRejoin.Count == 0 || world == null)
			{
				return;
			}

			List<int> ids = new List<int>(PendingRejoin);
			for (int i = 0; i < ids.Count; i++)
			{
				EntityPlayer player = world.GetEntity(ids[i]) as EntityPlayer;
				if (player == null)
				{
					PendingRejoin.Remove(ids[i]);
					continue;
				}

				Rejoin(player);
			}
		}

		private static void AddClaim(Dictionary<int, Claim> claims, int playerId)
		{
			claims.TryGetValue(playerId, out Claim claim);
			if (Time.time > claim.Until)
			{
				claim.Count = 0;
			}

			claim.Count++;
			claim.Until = Time.time + 3f;
			claims[playerId] = claim;
		}

		private static bool TakeClaim(Dictionary<int, Claim> claims, int playerId)
		{
			if (!claims.TryGetValue(playerId, out Claim claim) || claim.Count <= 0 || Time.time > claim.Until)
			{
				claims.Remove(playerId);
				return false;
			}

			claim.Count--;
			if (claim.Count <= 0)
			{
				claims.Remove(playerId);
			}
			else
			{
				claims[playerId] = claim;
			}

			return true;
		}

		private static void SweepClaims()
		{
			Sweep(GrantClaims);
			Sweep(PackageClaims);
		}

		private static void Sweep(Dictionary<int, Claim> claims)
		{
			if (claims.Count == 0)
			{
				return;
			}

			List<int> drop = null;
			foreach (KeyValuePair<int, Claim> pair in claims)
			{
				if (Time.time > pair.Value.Until)
				{
					if (drop == null)
					{
						drop = new List<int>();
					}

					drop.Add(pair.Key);
				}
			}

			if (drop == null)
			{
				return;
			}

			for (int i = 0; i < drop.Count; i++)
			{
				claims.Remove(drop[i]);
			}
		}

		private static HashSet<Vector3i> ParseSpots(string text)
		{
			HashSet<Vector3i> spots = new HashSet<Vector3i>();
			if (string.IsNullOrEmpty(text))
			{
				return spots;
			}

			string[] entries = text.Split(';');
			for (int i = 0; i < entries.Length; i++)
			{
				string[] axes = entries[i].Split(',');
				if (axes.Length != 3)
				{
					continue;
				}

				int x;
				int y;
				int z;
				if (int.TryParse(axes[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x) &&
					int.TryParse(axes[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y) &&
					int.TryParse(axes[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out z))
				{
					spots.Add(new Vector3i(x, y, z));
				}
			}

			return spots;
		}

		private static string FormatSpots(HashSet<Vector3i> spots)
		{
			if (spots == null || spots.Count == 0)
			{
				return "";
			}

			List<string> parts = new List<string>(spots.Count);
			foreach (Vector3i spot in spots)
			{
				parts.Add(string.Concat(
					spot.x.ToString(CultureInfo.InvariantCulture), ",",
					spot.y.ToString(CultureInfo.InvariantCulture), ",",
					spot.z.ToString(CultureInfo.InvariantCulture)));
			}

			return string.Join(";", parts);
		}

		private static HashSet<int> ParseLines(string text)
		{
			HashSet<int> lines = new HashSet<int>();
			if (string.IsNullOrEmpty(text))
			{
				return lines;
			}

			string[] entries = text.Split(',');
			for (int i = 0; i < entries.Length; i++)
			{
				int index;
				if (int.TryParse(entries[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out index) && index >= 0)
				{
					lines.Add(index);
				}
			}

			return lines;
		}

		private static string FormatLines(HashSet<int> lines)
		{
			if (lines == null || lines.Count == 0)
			{
				return "";
			}

			List<string> parts = new List<string>(lines.Count);
			foreach (int line in lines)
			{
				parts.Add(line.ToString(CultureInfo.InvariantCulture));
			}

			return string.Join(",", parts);
		}

		[HarmonyPatch(typeof(Instances), nameof(Instances.Enter))]
		private static class Patch_Enter
		{
			[HarmonyPriority(Priority.First)]
			private static void Postfix(EntityPlayer player)
			{
				Restore(player);
				if (player != null)
				{
					WatchApply(player.entityId);
				}
			}
		}

		[HarmonyPatch(typeof(Instances), nameof(Instances.Exit))]
		private static class Patch_Exit
		{
			private static void Prefix(EntityPlayer player)
			{
				Save(player);
			}
		}

		[HarmonyPatch(typeof(Instances), nameof(Instances.Release))]
		private static class Patch_Release
		{
			private static void Prefix(int playerId)
			{
				if (PreserveOnRelease || !Instances.IsInside(playerId))
				{
					return;
				}

				World world = GameManager.Instance?.World;
				EntityPlayer player = world == null ? null : world.GetEntity(playerId) as EntityPlayer;
				Clear(player);
			}
		}
	}
}
