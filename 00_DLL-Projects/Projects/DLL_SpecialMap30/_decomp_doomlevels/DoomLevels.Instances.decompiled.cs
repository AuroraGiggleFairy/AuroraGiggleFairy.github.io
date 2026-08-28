using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace DoomLevels;

public static class Instances
{
	private sealed class Instance
	{
		public int Cell;

		public int PlayerId;

		public Level Level;

		public Vector3i Origin;

		public Stats Stats;

		public List<ChunkObserver> Observers;

		public Vector3i Area;

		public float Waited;

		public List<(Vector3i At, BlockValue Wanted)> Things;
	}

	public const int BaseX = 100000;

	public const int BaseZ = 100000;

	public const int CellSize = 512;

	public const int CellHeight = 64;

	public const int GridWidth = 64;

	private const string InLevelBuff = "doomBuffInLevel";

	private const string TeleportBuff = "doomBuffTeleport";

	private const string ReturnX = "doom_return_x";

	private const string ReturnY = "doom_return_y";

	private const string ReturnZ = "doom_return_z";

	private static readonly List<(float Due, Action Work)> Pending = new List<(float, Action)>();

	private static float _clock;

	private static bool _handover;

	private static readonly Dictionary<int, Instance> ByPlayer = new Dictionary<int, Instance>();

	private static readonly HashSet<int> UsedCells = new HashSet<int>();

	public const int CellCycle = 8;

	private static int _nextCell;

	public const int CellSpacing = 4096;

	public static int Active => ByPlayer.Count;

	private static void Post(Action work, float delay = 0f)
	{
		lock (Pending)
		{
			Pending.Add((_clock + delay, work));
		}
	}

	private static void PostBackground(Action work)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		ThreadManager.AddSingleTask((TaskFunctionDelegate)delegate
		{
			work();
		}, (object)null, (ExitCallbackTask)null, true);
	}

	private static bool Live(Instance instance)
	{
		if (ByPlayer.TryGetValue(instance.PlayerId, out var value))
		{
			return value == instance;
		}
		return false;
	}

	private static Vector3i AreaToLoad(Vector3i size)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vector3i maxSize = LevelDb.MaxSize;
		return new Vector3i(Mathf.Max(size.x, maxSize.x), Mathf.Max(size.y, maxSize.y), Mathf.Max(size.z, maxSize.z));
	}

	public static void Handover(EntityPlayer player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			Effects.Clear();
			Release(((Entity)player).entityId, log: true, clear: false);
			_handover = true;
			Log.Out("[DoomLevels] handover: player " + ((Entity)player).entityId + " stays in instance space for the next level");
		}
	}

	public static bool IsInstanceSpace(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (position.x >= 99488f)
		{
			return position.z >= 99488f;
		}
		return false;
	}

	public static bool TryGet(int playerId, out Vector3i origin, out Level level, out int cell)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!ByPlayer.TryGetValue(playerId, out var value))
		{
			origin = Vector3i.zero;
			level = null;
			cell = -1;
			return false;
		}
		origin = value.Origin;
		level = value.Level;
		cell = value.Cell;
		return true;
	}

	public static Stats StatsFor(int playerId)
	{
		if (!ByPlayer.TryGetValue(playerId, out var value))
		{
			return null;
		}
		return value.Stats;
	}

	private static bool Inside(Instance instance, Vector3 at)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		Vector3i origin = instance.Origin;
		Vector3i size = instance.Level.Size;
		if (at.x >= (float)origin.x && at.x <= (float)(origin.x + size.x) && at.y >= (float)origin.y && at.y <= (float)(origin.y + size.y) && at.z >= (float)origin.z)
		{
			return at.z <= (float)(origin.z + size.z);
		}
		return false;
	}

	public static bool Inside(int playerId, Vector3 at)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (ByPlayer.TryGetValue(playerId, out var value) && value.Level != null)
		{
			return Inside(value, at);
		}
		return false;
	}

	public static bool Inside(int playerId, Vector3i at)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Instances.Inside(playerId, new Vector3((float)at.x, (float)at.y, (float)at.z));
	}

	public static EntityPlayer PlayerAt(Vector3 position)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		World val = (((Object)(object)GameManager.Instance == (Object)null) ? null : GameManager.Instance.World);
		if (val == null)
		{
			return null;
		}
		foreach (KeyValuePair<int, Instance> item in ByPlayer)
		{
			Instance value = item.Value;
			Vector3i origin = value.Origin;
			Vector3i size = value.Level.Size;
			if (!(position.x < (float)origin.x) && !(position.x > (float)(origin.x + size.x)) && !(position.z < (float)origin.z) && !(position.z > (float)(origin.z + size.z)) && val.Players.dict.TryGetValue(item.Key, out var value2))
			{
				return value2;
			}
		}
		return null;
	}

	public static Stats StatsAt(Vector3 position)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<int, Instance> item in ByPlayer)
		{
			Instance value = item.Value;
			Vector3i origin = value.Origin;
			Vector3i size = value.Level.Size;
			if (position.x >= (float)origin.x && position.x <= (float)(origin.x + size.x) && position.z >= (float)origin.z && position.z <= (float)(origin.z + size.z))
			{
				return value.Stats;
			}
		}
		return null;
	}

	public static Level LevelFor(EntityPlayer player)
	{
		if (!((Object)(object)player != (Object)null) || !ByPlayer.TryGetValue(((Entity)player).entityId, out var value))
		{
			return null;
		}
		return value.Level;
	}

	private static int TakeFreeCell()
	{
		for (int i = 0; i < 8; i++)
		{
			int num = (_nextCell + i) % 8;
			if (UsedCells.Add(num))
			{
				_nextCell = (num + 1) % 8;
				return num;
			}
		}
		int j;
		for (j = 8; !UsedCells.Add(j); j++)
		{
		}
		return j;
	}

	private static int CellAt(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Clamp(Mathf.FloorToInt((position.x - 100000f) / 4096f), 0, 63);
		return Mathf.Max(0, Mathf.FloorToInt((position.z - 100000f) / 4096f)) * 64 + num;
	}

	private static Vector3i OriginForCell(int cell)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		int num = cell % 64;
		int num2 = cell / 64;
		return new Vector3i(Align(100000 + num * 4096), LevelDb.Lift, Align(100000 + num2 * 4096));
	}

	private static int Align(int coord)
	{
		int chunkSize = LevelDb.ChunkSize;
		int num = ((coord + LevelDb.Shell) % chunkSize + chunkSize) % chunkSize;
		return coord - num;
	}

	public static bool IsInside(int playerId)
	{
		return ByPlayer.ContainsKey(playerId);
	}

	public unsafe static void Enter(EntityPlayer player, string map)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return;
		}
		LevelDb.Load();
		Level level = LevelDb.Get(map);
		if (level == null)
		{
			Log.Error("[DoomLevels] no level data for " + map);
			return;
		}
		if (level.Size.x > 512 || level.Size.z > 512)
		{
			Log.Error($"[DoomLevels] {map} is {level.Size} which exceeds the {512} instance cell");
			return;
		}
		if (ByPlayer.ContainsKey(((Entity)player).entityId))
		{
			Log.Warning("[DoomLevels] player " + ((Entity)player).entityId + " is already inside " + MapFor(((Entity)player).entityId) + ", refusing " + map);
			return;
		}
		bool handover = _handover;
		_handover = false;
		bool flag = !handover && IsInstanceSpace(((Entity)player).position);
		if (flag)
		{
			if (new Vector3(((EntityAlive)player).GetCVar("doom_return_x"), ((EntityAlive)player).GetCVar("doom_return_y"), ((EntityAlive)player).GetCVar("doom_return_z")).y <= 0f)
			{
				Log.Warning("[DoomLevels] player " + ((Entity)player).entityId + " is inside an instance but no return position stored, using current");
			}
		}
		else if (!handover)
		{
			SetReturn(player, ((Entity)player).position);
		}
		int num = (flag ? CellAt(((Entity)player).position) : TakeFreeCell());
		UsedCells.Add(num);
		Vector3i val = OriginForCell(num);
		Vector3i val2 = AreaToLoad(level.Size);
		Instance instance = new Instance
		{
			Cell = num,
			PlayerId = ((Entity)player).entityId,
			Level = level,
			Origin = val,
			Area = val2,
			Stats = new Stats(level),
			Observers = Observe(val, val2)
		};
		ByPlayer[((Entity)player).entityId] = instance;
		((EntityAlive)player).Buffs.AddBuff("doomBuffInLevel", -1, true, false, -1f);
		string[] obj = new string[8]
		{
			$"[DoomLevels] {map} instance cell {num} at {val} for player {((Entity)player).entityId}",
			flag ? ", resumed in place" : (handover ? ", handed over" : ""),
			", area ",
			null,
			null,
			null,
			null,
			null
		};
		Vector3i val3 = val2;
		obj[3] = ((object)(*(Vector3i*)(&val3))/*cast due to .constrained prefix*/).ToString();
		obj[4] = ", observers ";
		obj[5] = instance.Observers.Count.ToString();
		obj[6] = ", note lock ";
		obj[7] = ((EntityAlive)player).Buffs.HasBuff("doomBuffInLevel").ToString();
		Log.Out(string.Concat(obj));
		if (flag)
		{
			return;
		}
		PostBackground(delegate
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			Stopwatch stopwatch = Stopwatch.StartNew();
			Prefab prefab = new Prefab();
			bool loaded = prefab.Load(level.Prefab, true, true, false, false);
			Log.Out("[DoomLevels] loaded prefab " + level.Prefab + " in " + stopwatch.ElapsedMilliseconds + "ms");
			Post(delegate
			{
				if (Live(instance))
				{
					if (!loaded)
					{
						Log.Error("[DoomLevels] could not load prefab " + level.Prefab);
						Release(instance.PlayerId, log: true);
					}
					else
					{
						BuildLevel(instance, prefab);
					}
				}
			});
		});
	}

	private static List<ChunkObserver> Observe(Vector3i origin, Vector3i area)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		int num = 12 * LevelDb.ChunkSize * 2;
		int num2 = Mathf.Max(1, Mathf.CeilToInt((float)area.x / (float)num));
		int num3 = Mathf.Max(1, Mathf.CeilToInt((float)area.z / (float)num));
		List<ChunkObserver> list = new List<ChunkObserver>();
		Vector3 val = default(Vector3);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				float num4 = (float)area.x / (float)num2;
				float num5 = (float)area.z / (float)num3;
				((Vector3)(ref val))..ctor((float)origin.x + ((float)i + 0.5f) * num4, (float)origin.y + 2f, (float)origin.z + ((float)j + 0.5f) * num5);
				int num6 = Mathf.Min(12, (int)Mathf.Max(num4, num5) / (2 * LevelDb.ChunkSize) + 2);
				list.Add(GameManager.Instance.AddChunkObserver(val, false, num6, -1));
			}
		}
		return list;
	}

	public static string MapFor(int playerId)
	{
		if (!ByPlayer.TryGetValue(playerId, out var value))
		{
			return null;
		}
		return value.Level.Name;
	}

	public static void Exit(EntityPlayer player, ExitReason reason)
	{
		if (!((Object)(object)player == (Object)null))
		{
			Log.Out("[DoomLevels] exit " + reason.ToString() + " for player " + ((Entity)player).entityId);
			switch (reason)
			{
			case ExitReason.Cleared:
				FinishLevel(player);
				break;
			case ExitReason.Returned:
				ReturnToWorld(player);
				break;
			case ExitReason.Died:
				FailLevel(player);
				break;
			case ExitReason.Respawned:
				ReleaseAfterRespawn(player);
				break;
			}
		}
	}

	private static void FinishLevel(EntityPlayer player)
	{
		if (!ByPlayer.TryGetValue(((Entity)player).entityId, out var value))
		{
			Log.Warning("[DoomLevels] cleared outside an instance, ignored");
			return;
		}
		World val = (((Object)(object)GameManager.Instance == (Object)null) ? null : GameManager.Instance.World);
		if (val != null)
		{
			value.Stats?.Stop();
			ClearEntities(val, value);
			Effects.Clear();
			StripKeys(player);
			((EntityAlive)player).Buffs.RemoveBuff("doomBuffInLevel", -1, true);
			string name = value.Level.Name;
			QuestLink.Cleared(((Entity)player).entityId, name);
			Log.Out("[DoomLevels] " + name + " tally: " + value.Stats);
			XUiC_Intermission.Show(player, value.Level, value.Stats);
		}
	}

	private static void ReturnToWorld(EntityPlayer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Vector3 back = ReturnOf(player);
		Effects.Clear();
		Release(((Entity)player).entityId, log: true, clear: false);
		ClearReturn(player);
		SendToReturn(player, back);
	}

	private static void FailLevel(EntityPlayer player)
	{
		if (ByPlayer.TryGetValue(((Entity)player).entityId, out var value))
		{
			QuestLink.Failed(((Entity)player).entityId, value.Level.Name);
		}
		StripKeys(player);
	}

	private static void ReleaseAfterRespawn(EntityPlayer player)
	{
		if (ByPlayer.TryGetValue(((Entity)player).entityId, out var value))
		{
			value.Stats?.Stop();
		}
		Release(((Entity)player).entityId, log: true);
		ClearReturn(player);
		Effects.Clear();
		((EntityAlive)player).Buffs.RemoveBuff("doomBuffInLevel", -1, true);
	}

	public static void Leave(EntityPlayer player)
	{
		Exit(player, ExitReason.Returned);
	}

	public static Vector3 ReturnOf(EntityPlayer player)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(((EntityAlive)player).GetCVar("doom_return_x"), ((EntityAlive)player).GetCVar("doom_return_y"), ((EntityAlive)player).GetCVar("doom_return_z"));
	}

	public unsafe static void SetReturn(EntityPlayer player, Vector3 at)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && !(at.y <= 0f))
		{
			((EntityAlive)player).SetCVar("doom_return_x", at.x);
			((EntityAlive)player).SetCVar("doom_return_y", at.y);
			((EntityAlive)player).SetCVar("doom_return_z", at.z);
			string text = ((Entity)player).entityId.ToString();
			Vector3 val = at;
			Log.Out("[DoomLevels] return for player " + text + " set to " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString());
		}
	}

	public static void ClearReturn(EntityPlayer player)
	{
		((EntityAlive)player).SetCVar("doom_return_x", -1f);
		((EntityAlive)player).SetCVar("doom_return_y", -1f);
		((EntityAlive)player).SetCVar("doom_return_z", -1f);
		Log.Out("[DoomLevels] cleared return for player " + ((Entity)player).entityId + ", note lock " + ((EntityAlive)player).Buffs.HasBuff("doomBuffInLevel"));
	}

	private static void SendToReturn(EntityPlayer player, Vector3 back)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (back.y <= 0f)
		{
			Log.Warning("[DoomLevels] no return position stored for player " + ((Entity)player).entityId);
			return;
		}
		back.y += 0.5f;
		Teleport(player, back, null);
		Log.Out($"[DoomLevels] player {((Entity)player).entityId} returned to {back}");
	}

	private static void StripKeys(EntityPlayer player)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		string[] array = new string[3] { "doomKeyBlue", "doomKeyYellow", "doomKeyRed" };
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			ItemValue item = ItemClass.GetItem(array[i], false);
			if (item == null || item.IsEmpty())
			{
				continue;
			}
			int num2 = 0;
			if (((Entity)player).bag != null)
			{
				num2 += ((Entity)player).bag.DecItem(item, 1000, false, (IList<ItemStack>)null);
			}
			if (((EntityAlive)player).inventory != null)
			{
				num2 += ((EntityAlive)player).inventory.DecItem(item, 1000, false, (IList<ItemStack>)null);
			}
			if (num2 > 0)
			{
				EntityPlayerLocal val = (EntityPlayerLocal)(object)((player is EntityPlayerLocal) ? player : null);
				if (val != null)
				{
					((Entity)val).AddUIHarvestingItem(new ItemStack(item, -num2), false);
				}
			}
			num += num2;
		}
		if (num > 0)
		{
			Log.Out("[DoomLevels] stripped " + num + " keys from player " + ((Entity)player).entityId);
		}
	}

	public static void Release(int playerId, bool log, bool clear = true)
	{
		if (!ByPlayer.TryGetValue(playerId, out var value))
		{
			return;
		}
		if (clear)
		{
			ClearEntities(((Object)(object)GameManager.Instance == (Object)null) ? null : GameManager.Instance.World, value);
		}
		if (value.Observers != null)
		{
			for (int i = 0; i < value.Observers.Count; i++)
			{
				if (value.Observers[i] != null)
				{
					GameManager.Instance.RemoveChunkObserver(value.Observers[i]);
				}
			}
			value.Observers = null;
		}
		UsedCells.Remove(value.Cell);
		ByPlayer.Remove(playerId);
		if (log)
		{
			Log.Out($"[DoomLevels] released instance cell {value.Cell}");
		}
	}

	public static void Died(int playerId)
	{
		Exit(PlayerOf(playerId), ExitReason.Died);
	}

	public static void Unload(int playerId)
	{
		Exit(PlayerOf(playerId), ExitReason.Respawned);
	}

	private static EntityPlayer PlayerOf(int playerId)
	{
		World val = (((Object)(object)GameManager.Instance == (Object)null) ? null : GameManager.Instance.World);
		if (val != null)
		{
			Entity entity = ((WorldBase)val).GetEntity(playerId);
			return (EntityPlayer)(object)((entity is EntityPlayer) ? entity : null);
		}
		return null;
	}

	private static void ClearEntities(World world, Instance instance)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = instance.Origin.x - 16;
		int num2 = instance.Origin.z - 16;
		int num3 = instance.Origin.x + instance.Level.Size.x + 16;
		int num4 = instance.Origin.z + instance.Level.Size.z + 16;
		SpawnQueue.Drop(num, num2, num3, num4);
		int num5 = 0;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		List<Entity> list = world.Entities.list;
		for (int num6 = list.Count - 1; num6 >= 0; num6--)
		{
			Entity val = list[num6];
			if (!((Object)(object)val == (Object)null) && !(val is EntityPlayer))
			{
				Vector3 position = val.position;
				if (!(position.x < (float)num) && !(position.x >= (float)num3) && !(position.z < (float)num2) && !(position.z >= (float)num4))
				{
					string name = ((object)val).GetType().Name;
					dictionary[name] = ((!dictionary.ContainsKey(name)) ? 1 : (dictionary[name] + 1));
					EntityLootContainer val2 = (EntityLootContainer)(object)((val is EntityLootContainer) ? val : null);
					if ((Object)(object)val2 != (Object)null)
					{
						val2.bRemoved = true;
					}
					((WorldBase)world).RemoveEntity(val.entityId, (EnumRemoveEntityReason)3);
					num5++;
				}
			}
		}
		Log.Out($"[DoomLevels] cleared {num5} entities from instance cell " + $"{instance.Cell} in {stopwatch.ElapsedMilliseconds}ms");
	}

	public static void Reset()
	{
		ByPlayer.Clear();
		UsedCells.Clear();
		lock (Pending)
		{
			Pending.Clear();
		}
	}

	public static void Update()
	{
		_clock = Time.time;
		List<Action> list = null;
		lock (Pending)
		{
			int num = 0;
			while (num < Pending.Count)
			{
				if (Pending[num].Due <= _clock)
				{
					(list ?? (list = new List<Action>())).Add(Pending[num].Work);
					Pending.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i]();
			}
		}
	}

	private static bool ChunksReady(World world, Instance instance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Vector3i origin = instance.Origin;
		Vector3i area = instance.Area;
		int num = 0;
		for (int i = origin.x; i <= origin.x + area.x; i += LevelDb.ChunkSize)
		{
			for (int j = origin.z; j <= origin.z + area.z; j += LevelDb.ChunkSize)
			{
				if (((WorldBase)world).GetChunkFromWorldPos(i, origin.y, j) == null)
				{
					num++;
				}
			}
		}
		if (num > 0 && instance.Waited < 20f)
		{
			return false;
		}
		if (num > 0)
		{
			Log.Warning("[DoomLevels] stamping " + instance.Level.Name + " with " + num + " chunks of the cell still unloaded, leftovers may survive the wipe");
		}
		return true;
	}

	private static void ClearBlocks(World world, Vector3i origin, List<BlockChangeInfo> changes)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		Stopwatch stopwatch = Stopwatch.StartNew();
		int chunkSize = LevelDb.ChunkSize;
		Vector3i val2 = default(Vector3i);
		for (int i = 0; i < 512; i += chunkSize)
		{
			for (int j = 0; j < 512; j += chunkSize)
			{
				IChunk chunkFromWorldPos = ((WorldBase)world).GetChunkFromWorldPos(origin.x + j, origin.y, origin.z + i);
				Chunk val = (Chunk)(object)((chunkFromWorldPos is Chunk) ? chunkFromWorldPos : null);
				if (val == null || val.IsEmpty())
				{
					continue;
				}
				for (int k = 0; k < 64; k++)
				{
					for (int l = 0; l < chunkSize; l++)
					{
						for (int m = 0; m < chunkSize; m++)
						{
							((Vector3i)(ref val2))..ctor(origin.x + j + m, origin.y + k, origin.z + i + l);
							BlockValue block = ((WorldBase)world).GetBlock(val2);
							if (!((BlockValue)(ref block)).isair)
							{
								changes.Add(new BlockChangeInfo(new BlockValueRef(val2), BlockValue.Air, false));
							}
						}
					}
				}
			}
		}
		Log.Out("[DoomLevels] cleared " + changes.Count + " blocks in " + stopwatch.ElapsedMilliseconds + "ms");
	}

	public static void StampPrefab(World world, Prefab prefab, Vector3i origin, List<BlockChangeInfo> changes)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = 0;
		Vector3i size = prefab.size;
		Vector3i val = default(Vector3i);
		for (int i = 0; i < size.y; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				for (int k = 0; k < size.x; k++)
				{
					BlockValue block = prefab.GetBlock(k, i, j);
					if (!((BlockValue)(ref block)).isair)
					{
						((Vector3i)(ref val))..ctor(origin.x + k, origin.y + i, origin.z + j);
						changes.Add(new BlockChangeInfo(new BlockValueRef(val), block, false));
						num++;
					}
				}
			}
		}
		Log.Out("[DoomLevels] stamped " + prefab.PrefabName + " with " + num + " blocks in " + stopwatch.ElapsedMilliseconds + "ms");
	}

	public static int ApplyBlocks(World world, List<BlockChangeInfo> changes)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		if (changes.Count > 0)
		{
			((WorldBase)world).SetBlocksRPC(changes);
		}
		Log.Out("[DoomLevels] applied " + changes.Count + " changes in " + stopwatch.ElapsedMilliseconds + "ms");
		return changes.Count;
	}

	private static void BuildLevel(Instance instance, Prefab prefab)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		if (!Live(instance))
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (!ChunksReady(world, instance))
		{
			instance.Waited += 0.25f;
			if (instance.Waited > 30f)
			{
				Log.Error("[DoomLevels] timed out loading chunks for " + instance.Level.Name);
				Release(instance.PlayerId, log: true);
			}
			else
			{
				Post(delegate
				{
					BuildLevel(instance, prefab);
				}, 0.25f);
			}
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		Level level = instance.Level;
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		ClearEntities(world, instance);
		ClearBlocks(world, instance.Origin, list);
		StampPrefab(world, prefab, instance.Origin, list);
		ApplyBlocks(world, list);
		instance.Things = CollectThings(prefab, instance.Origin);
		RestoreThings(world, instance.Things, "stamp");
		ResetChunkCollisions(world, instance.Origin);
		Log.Out($"[DoomLevels] built level {level.Name} at {instance.Origin} in " + stopwatch.ElapsedMilliseconds + "ms (" + list.Count + " block changes)");
		Post(delegate
		{
			if (Live(instance))
			{
				SpawnAtStart(GameManager.Instance.World, instance.PlayerId, instance);
			}
		}, 0.5f);
		Post(delegate
		{
			EnsureGrounded(instance, 1);
		}, 0.75f);
		Post(delegate
		{
			Verify(instance);
		}, 2f);
	}

	private static void ResetChunkCollisions(World world, Vector3i origin)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = 0;
		for (int i = 0; i < 512; i += LevelDb.ChunkSize)
		{
			for (int j = 0; j < 512; j += LevelDb.ChunkSize)
			{
				IChunk chunkFromWorldPos = ((WorldBase)world).GetChunkFromWorldPos(origin.x + j, origin.y, origin.z + i);
				Chunk val = (Chunk)(object)((chunkFromWorldPos is Chunk) ? chunkFromWorldPos : null);
				if (val != null)
				{
					val.IsCollisionMeshGenerated = false;
					num++;
				}
			}
		}
		Log.Out("[DoomLevels] reset " + num + " chunk collisions in " + stopwatch.ElapsedMilliseconds + "ms");
	}

	private static bool SpawnEntitiesReady(World world, Instance instance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = StartSpawn(instance);
		int chunkSize = LevelDb.ChunkSize;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				IChunk chunkFromWorldPos = ((WorldBase)world).GetChunkFromWorldPos((int)val.x + j * chunkSize, (int)val.y, (int)val.z + i * chunkSize);
				Chunk val2 = (Chunk)(object)((chunkFromWorldPos is Chunk) ? chunkFromWorldPos : null);
				if (val2 == null || val2.blockEntityStubsToRemove.Count > 0)
				{
					return false;
				}
				List<BlockEntityData> list = val2.blockEntityStubs.list;
				for (int k = 0; k < list.Count; k++)
				{
					if (!list[k].bHasTransform)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private static void EnsureGrounded(Instance instance, int attempt)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		if (!Live(instance))
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (!world.Players.dict.TryGetValue(instance.PlayerId, out var value) || (Object)(object)value == (Object)null)
		{
			return;
		}
		bool flag = SpawnEntitiesReady(world, instance);
		bool flag2 = flag && Physics.Raycast(((Entity)value).position - Origin.position + Vector3.up * 0.3f, Vector3.down, 2.6f, -5, (QueryTriggerInteraction)1);
		if (flag && flag2)
		{
			Log.Out("[DoomLevels] player " + instance.PlayerId + " grounded after " + attempt + " polls");
			return;
		}
		if (attempt >= 60)
		{
			Log.Warning("[DoomLevels] player " + instance.PlayerId + " still not grounded after " + attempt + " polls, giving up");
			return;
		}
		Vector3 position = StartSpawn(instance);
		((EntityAlive)value).Buffs.AddBuff("doomBuffTeleport", -1, true, false, -1f);
		TeleportInLevel(value, position, null);
		if (attempt % 8 == 0)
		{
			Log.Out("[DoomLevels] player " + instance.PlayerId + " held at spawn, " + (flag ? "entities ready, not grounded" : "entities pending") + ", poll " + attempt);
		}
		Post(delegate
		{
			EnsureGrounded(instance, attempt + 1);
		}, 0.25f);
	}

	private static void Verify(Instance instance)
	{
		if (Live(instance))
		{
			RestoreThings(GameManager.Instance.World, instance.Things, "post-stamp");
			instance.Things = null;
		}
	}

	private static List<(Vector3i At, BlockValue Wanted)> CollectThings(Prefab prefab, Vector3i origin)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<(Vector3i, BlockValue)> list = new List<(Vector3i, BlockValue)>();
		Vector3i size = prefab.size;
		for (int i = 0; i < size.y; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				for (int k = 0; k < size.x; k++)
				{
					BlockValue block = prefab.GetBlock(k, i, j);
					if (!((BlockValue)(ref block)).isair && !((BlockValue)(ref block)).ischild && IsThing(block))
					{
						list.Add((new Vector3i(origin.x + k, origin.y + i, origin.z + j), block));
					}
				}
			}
		}
		Log.Out("[DoomLevels] collected " + list.Count + " things in " + stopwatch.ElapsedMilliseconds + "ms");
		return list;
	}

	private static bool IsThing(BlockValue value)
	{
		Block block = ((BlockValue)(ref value)).Block;
		if (block == null)
		{
			return false;
		}
		string blockName = block.GetBlockName();
		if (!blockName.StartsWith("doomP", StringComparison.Ordinal) && !blockName.StartsWith("doomM", StringComparison.Ordinal) && !blockName.StartsWith("doomO", StringComparison.Ordinal))
		{
			return blockName.StartsWith("doomB", StringComparison.Ordinal);
		}
		return true;
	}

	private unsafe static void RestoreThings(World world, List<(Vector3i At, BlockValue Wanted)> things, string when)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (things == null || things.Count == 0)
		{
			return;
		}
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		foreach (var thing in things)
		{
			Vector3i item = thing.At;
			BlockValue item2 = thing.Wanted;
			BlockValue block = ((WorldBase)world).GetBlock(item);
			if (((BlockValue)(ref block)).type != ((BlockValue)(ref item2)).type)
			{
				list.Add(new BlockChangeInfo(new BlockValueRef(item), item2, false));
				string[] obj = new string[9]
				{
					"[DoomLevels] ",
					when,
					" lost ",
					((BlockValue)(ref item2)).Block.GetBlockName(),
					" at ",
					null,
					null,
					null,
					null
				};
				Vector3i val = item;
				obj[5] = ((object)(*(Vector3i*)(&val))/*cast due to .constrained prefix*/).ToString();
				obj[6] = " to ";
				obj[7] = (((BlockValue)(ref block)).isair ? "air" : ((BlockValue)(ref block)).Block.GetBlockName());
				obj[8] = ", restored";
				Log.Warning(string.Concat(obj));
			}
		}
		if (list.Count > 0)
		{
			((WorldBase)world).SetBlocksRPC(list);
		}
	}

	private static Vector3 StartSpawn(Instance instance)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		Level level = instance.Level;
		int shell = LevelDb.Shell;
		return new Vector3((float)(instance.Origin.x + shell + level.Start.x) + level.StartOffX, (float)(instance.Origin.y + shell + level.Start.y) + 0.05f, (float)(instance.Origin.z + shell + level.Start.z) + level.StartOffZ);
	}

	private static void SpawnAtStart(World world, int playerId, Instance instance)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (!world.Players.dict.TryGetValue(playerId, out var value) || (Object)(object)value == (Object)null)
		{
			Log.Warning("[DoomLevels] no player " + playerId + " to place in " + instance.Level.Name);
			return;
		}
		Level level = instance.Level;
		Vector3 val = StartSpawn(instance);
		Vector3 value2 = default(Vector3);
		((Vector3)(ref value2))..ctor(0f, (float)level.StartAngle, 0f);
		((EntityAlive)value).Buffs.AddBuff("doomBuffTeleport", -1, true, false, -1f);
		Teleport(value, val, value2);
		Log.Out($"[DoomLevels] placed player {instance.PlayerId} at {val}, " + $"angle {level.StartAngle}");
	}

	private static void DropPlatform(EntityPlayerLocal player)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		vp_FPController vp_FPController = player.vp_FPController;
		if (!((Object)(object)vp_FPController == (Object)null))
		{
			vp_FPController.m_Platform = null;
			vp_FPController.m_PositionOnPlatform = Vector3.zero;
		}
	}

	public static void TeleportInLevel(EntityPlayer player, Vector3 position, Vector3? look)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		EntityPlayerLocal val = (EntityPlayerLocal)(object)((player is EntityPlayerLocal) ? player : null);
		if ((Object)(object)val == (Object)null)
		{
			Teleport(player, position, look);
			return;
		}
		DropPlatform(val);
		((Entity)val).SetPosition(position, true);
		if (look.HasValue)
		{
			((Entity)val).SetRotation(new Vector3(0f, Mathf.Atan2(look.Value.x, look.Value.z) * 57.29578f, 0f));
		}
		((Entity)val).SetVelocity(Vector3.zero);
	}

	public static void Teleport(EntityPlayer player, Vector3 position, Vector3? look)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		EntityPlayerLocal val = (EntityPlayerLocal)(object)((player is EntityPlayerLocal) ? player : null);
		if ((Object)(object)val != (Object)null)
		{
			DropPlatform(val);
			val.TeleportToPosition(position, false, look);
			return;
		}
		ClientInfo val2 = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(((Entity)player).entityId);
		if (val2 != null)
		{
			val2.SendPackage((NetPackage)(object)NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(position, look, false));
		}
	}
}
