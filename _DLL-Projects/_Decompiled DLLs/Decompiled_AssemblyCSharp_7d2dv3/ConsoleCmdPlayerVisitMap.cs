using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPlayerVisitMap : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine traverseCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public int x1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int z1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int x2;

	[PublicizedFrom(EAccessModifier.Private)]
	public int z2;

	[PublicizedFrom(EAccessModifier.Private)]
	public int height = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public int radius = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string logFilePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int stepsPerLog = 1;

	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "playervisitmap", "pvm" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Teleports the player through a rectangular area with optional memory logging";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "<x1> <z1> <x2> <z2> : start teleporting through the area defined by the coorindates, should be such that x1 < x2 and z1 < z2\nstop : stop moving\nheight <int> : set the height off the ground to move the player to\nstepradius <int> : set distance between teleports in chunks, default is half the player's view dimension\nlogfile <prefix> : sets the file for the memory log to a temporary file with this prefix\nstepsperlog <int> : count of teleports between logs\ndumplog : dump the current log file to the game log";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!GameManager.Instance.World.GetPrimaryPlayer())
		{
			Log.Out("No local player! (Are you in-game?)");
		}
		else
		{
			if (_params.Count == 0)
			{
				return;
			}
			if (_params.Count == 4)
			{
				if (!int.TryParse(_params[0], out x1))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given x1 coordinate is not a valid integer");
				}
				else if (!int.TryParse(_params[1], out z1))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given z1 coordinate is not a valid integer");
				}
				else if (!int.TryParse(_params[2], out x2))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given x2 coordinate is not a valid integer");
				}
				else if (!int.TryParse(_params[3], out z2))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given z2 coordinate is not a valid integer");
				}
				else
				{
					StartTraversing();
				}
				return;
			}
			switch (_params[0].ToLowerInvariant())
			{
			case "stop":
				Stop();
				break;
			case "height":
			{
				if (_params.Count > 1 && int.TryParse(_params[1], out var result))
				{
					height = Math.Max(result, 0);
				}
				Log.Out("Height above ground: {0}", height);
				break;
			}
			case "stepradius":
			{
				if (_params.Count > 1 && int.TryParse(_params[1], out var result2))
				{
					radius = result2;
				}
				if (radius >= 0)
				{
					Log.Out("Step Radius (Chunks): {0}", radius);
				}
				else
				{
					Log.Out("Step Radius (Chunks): player view distance");
				}
				break;
			}
			case "logfile":
			{
				string prefix = string.Empty;
				if (_params.Count > 1)
				{
					prefix = _params[1];
				}
				logFilePath = PlatformManager.NativePlatform.Utils.GetTempFileName(prefix, ".log");
				Log.Out($"Setting logging for player visit map. Path: {logFilePath}, Steps per log: {stepsPerLog}");
				break;
			}
			case "stepsperlog":
			{
				if (_params.Count > 1 && int.TryParse(_params[1], out var result3))
				{
					if (result3 > 0)
					{
						stepsPerLog = result3;
						Log.Out($"Setting logging for player visit map. Path: {logFilePath}, Steps per log: {stepsPerLog}");
					}
					else
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Steps per log must be at least 1");
					}
				}
				break;
			}
			case "dumplog":
				if (logFilePath == null)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No log file set");
				}
				else
				{
					Log.Out(logFilePath + "\n" + File.ReadAllText(logFilePath));
				}
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartTraversing()
	{
		if (traverseCoroutine == null)
		{
			Coroutine coroutine = ThreadManager.StartCoroutine(CoroutineTraverse());
			if (isRunning)
			{
				traverseCoroutine = coroutine;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Stop()
	{
		if (traverseCoroutine != null)
		{
			isRunning = false;
			ThreadManager.StopCoroutine(traverseCoroutine);
			traverseCoroutine = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CoroutineTraverse()
	{
		if (!ProfilerGameUtils.TryGetFlyingPlayer(out var player))
		{
			Log.Error("Could not get player, cancelling");
			yield break;
		}
		isRunning = true;
		int radiusChunks = radius;
		if (radiusChunks < 0)
		{
			radiusChunks = Math.Max(player.ChunkObserver.viewDim - 2, 0);
		}
		Vector3i chunkPos1 = new Vector3i(World.toChunkXZ((x1 <= x2) ? x1 : x2), 0, World.toChunkXZ((z1 <= z2) ? z1 : z2));
		Vector3i chunkPos2 = new Vector3i(World.toChunkXZ((x1 <= x2) ? x2 : x1), 0, World.toChunkXZ((z1 <= z2) ? z2 : z1));
		_ = chunkPos2.x - chunkPos1.x + 1;
		_ = chunkPos2.z - chunkPos1.z + 1;
		_ = Time.time;
		int curChunkX = Math.Min(chunkPos1.x + radiusChunks, chunkPos2.x);
		int curChunkZ = Math.Min(chunkPos1.z + radiusChunks, chunkPos2.z);
		ProfilingMetricCapture memoryMetrics = ProfilerCaptureUtils.CreateMemoryProfiler();
		if (logFilePath != null)
		{
			using FileStream stream = File.Open(logFilePath, FileMode.OpenOrCreate);
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write("WorldX,WorldY,WorldZ,");
			streamWriter.Write(memoryMetrics.GetCsvHeader());
			streamWriter.WriteLine();
		}
		Log.Out("Running Player Visit Map. Block Rect: ({0},{1}), ({2},{3}), Step Radius: {4}, Chunk Rect: ({5},{6}), ({7},{8})", x1, z1, x2, z2, radiusChunks, chunkPos1.x, chunkPos1.z, chunkPos2.x, chunkPos2.z);
		yield return null;
		int stepCount = 0;
		while (curChunkX - radiusChunks <= chunkPos2.x && curChunkZ - radiusChunks <= chunkPos2.z)
		{
			if (player.world != GameManager.Instance.World || player != GameManager.Instance.World.GetPrimaryPlayer())
			{
				Stop();
				yield break;
			}
			if (!isRunning)
			{
				yield break;
			}
			Vector3i blockPos = chunkPosToBlockPos(curChunkX, curChunkZ);
			player.SetPosition(blockPos);
			yield return new WaitForSeconds(0.5f);
			yield return ProfilerGameUtils.WaitForChunksAroundObserverToLoad(player.ChunkObserver, ChunkConditions.Displayed);
			curChunkX += radiusChunks * 2 + 1;
			if (curChunkX - radiusChunks > chunkPos2.x)
			{
				curChunkX = Math.Min(chunkPos1.x + radiusChunks, chunkPos2.x);
				curChunkZ += radiusChunks * 2 + 1;
			}
			if (logFilePath != null && stepCount % stepsPerLog == 0)
			{
				using FileStream stream2 = File.Open(logFilePath, FileMode.Append);
				using StreamWriter streamWriter2 = new StreamWriter(stream2);
				streamWriter2.Write("{0},{1},{2},", blockPos.x, blockPos.y, blockPos.z);
				streamWriter2.Write(memoryMetrics.GetLastValueCsv());
				streamWriter2.WriteLine();
			}
			stepCount++;
		}
		Stop();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i chunkPosToBlockPos(int _x, int _z)
	{
		int num = (_x << 4) + 8;
		int num2 = (_z << 4) + 8;
		return new Vector3i(num, GameManager.Instance.World.GetHeightAt(num, num2) + (float)height, num2);
	}
}
