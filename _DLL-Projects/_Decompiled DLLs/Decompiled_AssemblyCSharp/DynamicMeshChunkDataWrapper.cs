using System.Diagnostics;
using System.Threading;

[DebuggerDisplay("{X},{Z} {StateInfo}")]
public class DynamicMeshChunkDataWrapper
{
	public DynamicMeshChunkData Data;

	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	public string lastLock;

	public DynamicMeshStates StateInfo;

	public int X;

	public int Z;

	public long Key;

	public void Reset()
	{
		StateInfo = DynamicMeshStates.None;
	}

	public bool IsReadyForRelease()
	{
		if (StateInfo.HasFlag(DynamicMeshStates.SaveRequired))
		{
			if (DynamicMeshManager.DebugReleases)
			{
				Log.Out($"{X},{Z} save required");
			}
			return false;
		}
		if (StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating) || StateInfo.HasFlag(DynamicMeshStates.Generating))
		{
			if (DynamicMeshManager.DebugReleases)
			{
				Log.Out($"{X},{Z} thread updating");
			}
			return false;
		}
		return true;
	}

	public string Path()
	{
		return DynamicMeshFile.MeshLocation + $"{WorldChunkCache.MakeChunkKey(World.toChunkXZ(X), World.toChunkXZ(Z))}.update";
	}

	public string RawPath()
	{
		return DynamicMeshFile.MeshLocation + $"{WorldChunkCache.MakeChunkKey(World.toChunkXZ(X), World.toChunkXZ(Z))}.raw";
	}

	public bool Exists()
	{
		return SdFile.Exists(Path());
	}

	public bool GetLock(string debug)
	{
		int num = 0;
		while (!TryTakeLock(debug))
		{
			if (DynamicMeshThread.RequestThreadStop)
			{
				Log.Out(ToDebugLocation() + " World is unloading so lock attempt failed " + debug);
				return false;
			}
			if (num == 0 || ++num % 10 == 0)
			{
				Log.Out(ToDebugLocation() + " Waiting for lock to release: " + lastLock);
			}
			if (num > 600 && Monitor.IsEntered(_lock))
			{
				Log.Warning("Forcing lock release to " + debug + " from " + lastLock + " after 60 seconds");
				ReleaseLock();
			}
			Thread.Sleep(100);
		}
		return true;
	}

	public bool ReleaseLock()
	{
		return TryExit("releaseLock");
	}

	public bool TryTakeLock(string debug)
	{
		bool lockTaken = false;
		if (Monitor.IsEntered(_lock))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Warning(ToDebugLocation() + " Lock kept by " + debug);
			}
			lastLock = debug;
			return true;
		}
		Monitor.TryEnter(_lock, ref lockTaken);
		if (lockTaken)
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Warning(ToDebugLocation() + " Lock taken by " + debug);
			}
			lastLock = debug;
		}
		else if (DynamicMeshManager.DoLog)
		{
			Log.Warning(ToDebugLocation() + " Lock failed on " + debug + " : " + lastLock);
		}
		return lockTaken;
	}

	public bool ThreadHasLock()
	{
		return Monitor.IsEntered(_lock);
	}

	public bool TryGetData(out DynamicMeshChunkData data, string debug)
	{
		if (TryTakeLock(debug))
		{
			data = Data;
			return true;
		}
		data = null;
		return false;
	}

	public bool TryExit(string debug)
	{
		if (!Monitor.IsEntered(_lock))
		{
			Log.Warning(ToDebugLocation() + " Tried to release lock when not owner " + debug);
			return false;
		}
		while (Monitor.IsEntered(_lock))
		{
			Monitor.Exit(_lock);
		}
		if (DynamicMeshManager.DoLog)
		{
			Log.Out(ToDebugLocation() + " Lock released " + debug);
		}
		return true;
	}

	public bool IsAvailableToLoad()
	{
		if (Data != null && !StateInfo.HasFlag(DynamicMeshStates.MarkedForDelete) && !StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating))
		{
			return !StateInfo.HasFlag(DynamicMeshStates.LoadRequired);
		}
		return false;
	}

	public string ToDebugLocation()
	{
		return $"{X},{Z}";
	}

	public void ClearUnloadMarks()
	{
		StateInfo &= ~DynamicMeshStates.UnloadMark1;
		StateInfo &= ~DynamicMeshStates.UnloadMark2;
		StateInfo &= ~DynamicMeshStates.UnloadMark3;
	}

	public static DynamicMeshChunkDataWrapper Create(long key)
	{
		return new DynamicMeshChunkDataWrapper
		{
			X = DynamicMeshUnity.GetWorldXFromKey(key),
			Z = DynamicMeshUnity.GetWorldZFromKey(key),
			Key = key
		};
	}
}
