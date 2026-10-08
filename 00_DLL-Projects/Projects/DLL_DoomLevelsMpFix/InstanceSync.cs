using System.IO;
using DoomLevels;
using UnityEngine.Scripting;

namespace DoomLevelsMpFix
{
	internal static class InstanceSync
	{
		internal struct Record
		{
			public string Map;
			public Vector3i Origin;
			public int Cell;
		}

		public struct Stats
		{
			public int Health;
			public int MaxHealth;
			public float ArmourPct;
			public int Level;
			public int GameStage;
			public int ZombieKills;
			public int PlayerKills;
			public int Deaths;
			public int Ping;
			public bool PartyFull;
			public float X;
			public float Y;
			public float Z;
			public bool HasPos;
		}

		private static readonly System.Collections.Generic.Dictionary<int, Record> Maps = new System.Collections.Generic.Dictionary<int, Record>();
		private static readonly System.Collections.Generic.Dictionary<int, Stats> Last = new System.Collections.Generic.Dictionary<int, Stats>();

		internal static bool TryGet(int playerId, out string map, out Vector3i origin, out int cell)
		{
			if (Maps.TryGetValue(playerId, out Record record))
			{
				map = record.Map;
				origin = record.Origin;
				cell = record.Cell;
				return true;
			}

			map = null;
			origin = Vector3i.zero;
			cell = -1;
			return false;
		}

		internal static void Remember(int playerId, string map, Vector3i origin, int cell, Stats stats, bool inLevel)
		{
			Last[playerId] = stats;
			if (inLevel)
			{
				Maps[playerId] = new Record
				{
					Map = map ?? "",
					Origin = origin,
					Cell = cell
				};
				return;
			}

			Maps.Remove(playerId);
		}

		internal static bool TryVitals(int playerId, out int health, out int maxHealth, out float armourPct)
		{
			if (Last.TryGetValue(playerId, out Stats stats) && stats.MaxHealth > 0)
			{
				health = stats.Health;
				maxHealth = stats.MaxHealth;
				armourPct = stats.ArmourPct;
				return true;
			}

			health = 0;
			maxHealth = 0;
			armourPct = 0f;
			return false;
		}

		internal static bool TryStats(int playerId, out Stats stats)
		{
			if (Last.TryGetValue(playerId, out stats) && stats.Level > 0)
			{
				return true;
			}

			stats = default;
			return false;
		}

		internal static void Forget(int playerId)
		{
			Maps.Remove(playerId);
		}

		internal static void Drop(int playerId)
		{
			Maps.Remove(playerId);
			Last.Remove(playerId);
		}

		internal static bool Known(int playerId)
		{
			return Maps.ContainsKey(playerId);
		}

		internal static bool IsSyncedOnline(int playerId)
		{
			return Known(playerId) || TryStats(playerId, out _);
		}

		internal static bool PartyIsFull(int playerId)
		{
			return Last.TryGetValue(playerId, out Stats stats) && stats.PartyFull;
		}

		internal static bool TryPos(int playerId, out UnityEngine.Vector3 pos)
		{
			if (Last.TryGetValue(playerId, out Stats stats) && stats.HasPos)
			{
				pos = new UnityEngine.Vector3(stats.X, stats.Y, stats.Z);
				return true;
			}

			pos = UnityEngine.Vector3.zero;
			return false;
		}

		internal static void CollectOnline(System.Collections.Generic.List<int> into)
		{
			if (into == null || Last.Count == 0)
			{
				return;
			}

			int[] keys = new int[Last.Count];
			Last.Keys.CopyTo(keys, 0);
			for (int i = 0; i < keys.Length; i++)
			{
				if (IsSyncedOnline(keys[i]))
				{
					into.Add(keys[i]);
				}
			}
		}

		internal static void Clear()
		{
			Maps.Clear();
			Last.Clear();
			_nextPulse = 0f;
			Handoff.PlayerId = 0;
		}

		internal static void Watch()
		{
			ConnectionManager cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (cm == null || cm.IsClient)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null)
			{
				return;
			}

			System.Collections.Generic.HashSet<int> alive = new System.Collections.Generic.HashSet<int>();
			for (int i = 0; i < world.Players.list.Count; i++)
			{
				EntityPlayer player = world.Players.list[i];
				if (player == null)
				{
					continue;
				}

				alive.Add(player.entityId);
				bool inside = Instances.IsInside(player.entityId);
				if (inside && !Known(player.entityId))
				{
					SendEnter(player);
				}
				else if (!inside && Known(player.entityId) && Handoff.PlayerId != player.entityId)
				{
					SendLeave(player.entityId);
				}
			}

			if (Maps.Count > 0 || Last.Count > 0)
			{
				int[] keys = new int[Last.Count];
				Last.Keys.CopyTo(keys, 0);
				for (int i = 0; i < keys.Length; i++)
				{
					if (!alive.Contains(keys[i]))
					{
						Drop(keys[i]);
						Broadcast(keys[i], "", Vector3i.zero, -1, active: false, default);
						PartyHud.RequestRefresh();
					}
				}
			}

			if (UnityEngine.Time.time < _nextPulse)
			{
				return;
			}

			_nextPulse = UnityEngine.Time.time + 1f;
			for (int i = 0; i < world.Players.list.Count; i++)
			{
				EntityPlayer player = world.Players.list[i];
				if (player == null)
				{
					continue;
				}

				bool inside = Instances.IsInside(player.entityId);
				string map = "";
				Vector3i origin = Vector3i.zero;
				int cell = -1;
				if (inside)
				{
					Instances.TryGet(player.entityId, out origin, out Level level, out cell);
					map = level != null ? level.Name : "";
				}

				Stats stats = ReadStats(player);
				Remember(player.entityId, map, origin, cell, stats, inside);
				Broadcast(player.entityId, map, origin, cell, inside, stats);
			}
		}

		private static float _nextPulse;

		internal static void SendEnter(EntityPlayer player)
		{
			if (player == null || SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return;
			}

			if (!Instances.TryGet(player.entityId, out Vector3i origin, out Level level, out int cell) || level == null)
			{
				return;
			}

			Stats stats = ReadStats(player);
			Remember(player.entityId, level.Name, origin, cell, stats, inLevel: true);
			Broadcast(player.entityId, level.Name, origin, cell, active: true, stats);
			PartyHud.RequestRefresh();
		}

		internal static void SendLeave(int playerId)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return;
			}

			Stats stats = default;
			EntityPlayer player = PartyMembers.Find(GameManager.Instance?.World, playerId);
			if (player != null)
			{
				stats = ReadStats(player);
			}
			else
			{
				Last.TryGetValue(playerId, out stats);
			}

			Remember(playerId, "", Vector3i.zero, -1, stats, inLevel: false);
			Broadcast(playerId, "", Vector3i.zero, -1, active: false, stats);
			PartyHud.RequestRefresh();
		}

		internal static void SendAllTo(int entityId)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return;
			}

			ClientInfo client = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients?.ForEntityId(entityId);
			if (client == null)
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
				if (player == null)
				{
					continue;
				}

				bool inside = Instances.IsInside(player.entityId);
				string map = "";
				Vector3i origin = Vector3i.zero;
				int cell = -1;
				if (inside && Instances.TryGet(player.entityId, out origin, out Level level, out cell) && level != null)
				{
					map = level.Name;
				}

				client.SendPackage(Pack(player.entityId, map, origin, cell, inside, ReadStats(player)));
			}
		}

		internal static float ArmourPercent(EntityPlayer player)
		{
			if (player?.Stats?.Stamina == null)
			{
				return 0f;
			}

			float max = player.Stats.Stamina.ModifiedMax;
			if (max <= 0.01f)
			{
				return 0f;
			}

			return UnityEngine.Mathf.Clamp01(player.GetCVar("doomArmour") / max);
		}

		internal static Stats ReadStats(EntityPlayer player)
		{
			Stats stats = default;
			if (player == null)
			{
				return stats;
			}

			try
			{
				stats.Health = player.Health;
				stats.MaxHealth = player.GetMaxHealth();
				if (player.Stats?.Health != null)
				{
					if (stats.Health <= 0)
					{
						stats.Health = (int)player.Stats.Health.Value;
					}

					if (stats.MaxHealth <= 0)
					{
						stats.MaxHealth = (int)player.Stats.Health.ModifiedMax;
					}
				}

				stats.ArmourPct = ArmourPercent(player);

				if (player.Progression != null)
				{
					stats.Level = player.Progression.GetLevel();
				}

				stats.GameStage = player.gameStage;
				stats.ZombieKills = player.KilledZombies;
				stats.PlayerKills = player.KilledPlayers;
				stats.Deaths = player.Died;
				stats.Ping = player.pingToServer;
				stats.PartyFull = player.Party != null && player.Party.IsFull();
				stats.X = player.position.x;
				stats.Y = player.position.y;
				stats.Z = player.position.z;
				stats.HasPos = true;
			}
			catch
			{
			}

			if (!player.IsDead() && stats.Health <= 0 && Last.TryGetValue(player.entityId, out Stats prev) && prev.Health > 0)
			{
				stats.Health = prev.Health;
				if (stats.MaxHealth <= 0)
				{
					stats.MaxHealth = prev.MaxHealth;
				}
			}

			return stats;
		}

		private static void Broadcast(int playerId, string map, Vector3i origin, int cell, bool active, Stats stats)
		{
			ConnectionManager cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (cm == null)
			{
				return;
			}

			NetPackageDoomInstance package = Pack(playerId, map, origin, cell, active, stats);
			cm.SendPackage(package, _onlyClientsAttachedToAnEntity: true, -1, -1, -1);
		}

		private static NetPackageDoomInstance Pack(int playerId, string map, Vector3i origin, int cell, bool active, Stats stats)
		{
			return PackageEmit.Take<NetPackageDoomInstance>().Setup(playerId, map, origin, cell, active, stats);
		}
	}

	[Preserve]
	public abstract class NetPackageDoomInstance : NetPackage
	{
		private int _playerId;
		private string _map = "";
		private int _ox;
		private int _oy;
		private int _oz;
		private int _cell;
		private bool _active;
		private InstanceSync.Stats _stats;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		internal NetPackageDoomInstance Setup(int playerId, string map, Vector3i origin, int cell, bool active, InstanceSync.Stats stats)
		{
			_playerId = playerId;
			_map = map ?? "";
			_ox = origin.x;
			_oy = origin.y;
			_oz = origin.z;
			_cell = cell;
			_active = active;
			_stats = stats;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			_playerId = _br.ReadInt32();
			_map = _br.ReadString();
			_ox = _br.ReadInt32();
			_oy = _br.ReadInt32();
			_oz = _br.ReadInt32();
			_cell = _br.ReadInt32();
			_active = _br.ReadBoolean();
			_stats.Health = _br.ReadInt32();
			_stats.MaxHealth = _br.ReadInt32();
			_stats.ArmourPct = _br.ReadSingle();
			_stats.Level = _br.ReadInt32();
			_stats.GameStage = _br.ReadInt32();
			_stats.ZombieKills = _br.ReadInt32();
			_stats.PlayerKills = _br.ReadInt32();
			_stats.Deaths = _br.ReadInt32();
			_stats.Ping = _br.ReadInt32();
			_stats.PartyFull = _br.ReadBoolean();
			_stats.X = _br.ReadSingle();
			_stats.Y = _br.ReadSingle();
			_stats.Z = _br.ReadSingle();
			_stats.HasPos = _br.ReadBoolean();
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			((BinaryWriter)_bw).Write(_playerId);
			((BinaryWriter)_bw).Write(_map);
			((BinaryWriter)_bw).Write(_ox);
			((BinaryWriter)_bw).Write(_oy);
			((BinaryWriter)_bw).Write(_oz);
			((BinaryWriter)_bw).Write(_cell);
			((BinaryWriter)_bw).Write(_active);
			((BinaryWriter)_bw).Write(_stats.Health);
			((BinaryWriter)_bw).Write(_stats.MaxHealth);
			((BinaryWriter)_bw).Write(_stats.ArmourPct);
			((BinaryWriter)_bw).Write(_stats.Level);
			((BinaryWriter)_bw).Write(_stats.GameStage);
			((BinaryWriter)_bw).Write(_stats.ZombieKills);
			((BinaryWriter)_bw).Write(_stats.PlayerKills);
			((BinaryWriter)_bw).Write(_stats.Deaths);
			((BinaryWriter)_bw).Write(_stats.Ping);
			((BinaryWriter)_bw).Write(_stats.PartyFull);
			((BinaryWriter)_bw).Write(_stats.X);
			((BinaryWriter)_bw).Write(_stats.Y);
			((BinaryWriter)_bw).Write(_stats.Z);
			((BinaryWriter)_bw).Write(_stats.HasPos);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			InstanceSync.Remember(_playerId, _map, new Vector3i(_ox, _oy, _oz), _cell, _stats, _active);
			PartyHud.RequestRefresh();
		}

		public int Length()
		{
			return 90 + _map.Length * 2;
		}
	}
}
