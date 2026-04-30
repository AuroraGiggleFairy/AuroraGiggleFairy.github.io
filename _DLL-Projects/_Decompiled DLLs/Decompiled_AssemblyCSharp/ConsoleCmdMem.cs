using System;
using System.Collections;
using System.Collections.Generic;
using SystemInformation;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdMem : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<byte[]> permanentAllocs = new List<byte[]>();

	public static string[] Stats = new string[12];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loggingEnabed;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "mem" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Prints memory information and unloads resources or changes garbage collector";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\nclean - cleanup memory pools\npools - list memory pools\ngc - show GC info\ngc alloc <value> - allocate k value of temp memory\ngc perm <value> - allocate k value of permanent memory\ngc clearperm - clear permanent allocations list\ngc c - run collection\ngc enable <value> - enable GC (0 or 1)\ngc inc <value> - run incremental collect for value ms\ngc inctime <value> - set incremental collect time in ms\nobj [mode] - list object pool (active or all)\nobjs - shrink object pool\nlog [interval] - start/stop logging of performance data to a file";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count >= 1 && _params[0] == "gc")
		{
			if (_params.Count == 1)
			{
				string text = "editor";
				text = GarbageCollector.GCMode.ToString();
				string line = $"gc {text}, mem {GC.GetTotalMemory(forceFullCollection: false) / 1024}k, count {GC.CollectionCount(0)}, isInc {GarbageCollector.isIncremental}, inc time {GarbageCollector.incrementalTimeSliceNanoseconds / 1000000}ms";
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(line);
			}
			if (_params.Count >= 2)
			{
				int result = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out result);
				}
				switch (_params[1])
				{
				case "alloc":
					_ = new byte[result * 1024];
					break;
				case "perm":
					permanentAllocs.Add(new byte[result * 1024]);
					break;
				case "clearperm":
					permanentAllocs.Clear();
					break;
				case "c":
					GC.Collect();
					break;
				case "force":
					GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
					break;
				case "inc":
					GarbageCollector.CollectIncremental((ulong)result * 1000000uL);
					break;
				case "inctime":
					GarbageCollector.incrementalTimeSliceNanoseconds = (ulong)result * 1000000uL;
					break;
				case "enable":
					GarbageCollector.GCMode = ((result != 0) ? GarbageCollector.Mode.Enabled : GarbageCollector.Mode.Disabled);
					break;
				default:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown gc command");
					break;
				}
			}
			return;
		}
		if (_params.Count >= 1)
		{
			if (_params[0] == "obj")
			{
				GameObjectPool.Instance.CmdList((_params.Count >= 2) ? _params[1] : null);
				return;
			}
			if (_params[0] == "objs")
			{
				GameObjectPool.Instance.CmdShrink();
				return;
			}
			if (_params[0].EqualsCaseInsensitive("log"))
			{
				if (loggingEnabed)
				{
					loggingEnabed = false;
					return;
				}
				float _result = 2f;
				if (_params.Count > 1 && !StringParsers.TryParseFloat(_params[1], out _result))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Interval argument is not a valid float");
					return;
				}
				DateTime now = DateTime.Now;
				string text2 = GameIO.GetGamePath() + $"/perflog_{now.Year:0000}-{now.Month:00}-{now.Day:00}__{now.Hour:00}-{now.Minute:00}-{now.Second:00}.txt";
				loggingEnabed = true;
				ThreadManager.StartCoroutine(logCoroutine(text2, _result));
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Started performance logging to " + text2);
				return;
			}
			if (_params[0] == "prefab")
			{
				GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().CalculateStats(out var basePrefabCount, out var rotatedPrefabsCount, out var activePrefabCount, out var basePrefabBytes, out var rotatedPrefabBytes, out var activePrefabBytes);
				Log.Out("\nBase Prefabs - Count: {0}, Memory: {1:F2} MB\nRotated Prefabs - Count: {2}, Memory: {3:F2} MB\nActive Prefabs - Count: {4}, Memory: {5:F2} MB\n", basePrefabCount, (double)basePrefabBytes * 9.5367431640625E-07, rotatedPrefabsCount, (double)rotatedPrefabBytes * 9.5367431640625E-07, activePrefabCount, (double)activePrefabBytes * 9.5367431640625E-07);
			}
		}
		Resources.UnloadUnusedAssets();
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetStats(_bDoGc: true, GameManager.Instance));
		World world = GameManager.Instance.World;
		if (world != null && world.m_ChunkManager.m_ObservedEntities.Count > 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Observers");
			List<ChunkManager.ChunkObserver> observedEntities = world.m_ChunkManager.m_ObservedEntities;
			for (int i = 0; i < observedEntities.Count; i++)
			{
				ChunkManager.ChunkObserver chunkObserver = observedEntities[i];
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" id=" + chunkObserver.id);
			}
		}
		if (_params.Count > 0 && _params[0] == "clear")
		{
			MemoryPools.Cleanup();
		}
		if (_params.Count > 0 && _params[0] == "pools")
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(MemoryPools.GetDebugInfo());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(MemoryPools.GetDebugInfoEx());
		}
		if (_params.Count > 0 && _params[0] == "arraypools")
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(MemoryPools.GetDebugInfoArrays());
		}
	}

	public static string GetStats(bool _bDoGc, GameManager _gm)
	{
		FillStats(_bDoGc, _gm);
		World world = _gm.World;
		return "Time: " + Stats[0] + "m FPS: " + Stats[1] + " Heap: " + Stats[2] + "MB Max: " + Stats[3] + "MB Chunks: " + Stats[4] + " CGO: " + Stats[5] + " Ply: " + Stats[6] + " Zom: " + Stats[7] + " Ent: " + Stats[8] + " (" + Stats[9] + ") Items: " + Stats[10] + " CO: " + (world?.m_ChunkManager.m_ObservedEntities.Count ?? 0) + " RSS: " + Stats[11] + "MB";
	}

	public static void FillStats(bool _bDoGc, GameManager _gm)
	{
		World world = _gm.World;
		long totalMemory = GC.GetTotalMemory(_bDoGc);
		Stats[0] = (Time.timeSinceLevelLoad / 60f).ToCultureInvariantString("F2");
		Stats[1] = _gm.fps.Counter.ToCultureInvariantString("F2");
		Stats[2] = ((float)totalMemory / 1048576f).ToCultureInvariantString("0.0");
		Stats[3] = ((float)GameManager.MaxMemoryConsumption / 1048576f).ToCultureInvariantString("0.0");
		Stats[4] = Chunk.InstanceCount.ToString();
		Stats[5] = ((world != null) ? world.m_ChunkManager.GetDisplayedChunkGameObjectsCount().ToString() : "");
		Stats[6] = ((world != null) ? world.Players.list.Count.ToString() : "");
		Stats[7] = GameStats.GetInt(EnumGameStats.EnemyCount).ToString();
		Stats[8] = ((world != null) ? world.Entities.Count.ToString() : "");
		Stats[9] = Entity.InstanceCount.ToString();
		Stats[10] = EntityItem.ItemInstanceCount.ToString();
		Stats[11] = ((float)GetRSS.GetCurrentRSS() / 1024f / 1024f).ToCultureInvariantString("0.0");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator logCoroutine(string _filename, float _interval)
	{
		WaitForSeconds wait = new WaitForSeconds(_interval);
		while (loggingEnabed)
		{
			DateTime now = DateTime.Now;
			string contents = $"{now.Year:0000}-{now.Month:00}-{now.Day:00}T{now.Hour:00}:{now.Minute:00}:{now.Second:00} 0.000 INF {GetStats(_bDoGc: false, GameManager.Instance)}\r\n";
			SdFile.AppendAllText(_filename, contents);
			yield return wait;
		}
		Log.Out("Stopped performance logging");
	}
}
