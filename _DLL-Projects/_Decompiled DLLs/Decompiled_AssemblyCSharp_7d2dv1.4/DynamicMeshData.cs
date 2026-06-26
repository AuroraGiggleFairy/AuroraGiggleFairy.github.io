using System;
using System.Diagnostics;
using System.Threading;

[DebuggerDisplay("{X},{Z} {StateInfo}")]
public class DynamicMeshData
{
	public byte[] Bytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	public string lastLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsRegion;

	public DynamicMeshStates StateInfo;

	public int X;

	public int Z;

	public int StreamLength;

	public string GetByteLengthString()
	{
		if (Bytes == null)
		{
			return "null";
		}
		return Bytes.Length.ToString();
	}

	public string Path(bool isRegionQueue)
	{
		return DynamicMeshFile.MeshLocation + string.Format("{0},{1}.{2}", X, Z, isRegionQueue ? "region" : "mesh");
	}

	public bool Exists(bool isRegionQueue)
	{
		return SdFile.Exists(DynamicMeshFile.MeshLocation + string.Format("{0},{1}.{2}", X, Z, isRegionQueue ? "region" : "mesh"));
	}

	public bool GetLock(string debug)
	{
		DateTime now = DateTime.Now;
		bool flag = false;
		while (!TryTakeLock(debug))
		{
			if (DynamicMeshThread.RequestThreadStop)
			{
				Log.Out(ToDebugLocation() + " World is unloading so lock attempt failed " + debug);
				return false;
			}
			double totalSeconds = (DateTime.Now - now).TotalSeconds;
			if (!flag && totalSeconds > 5.0)
			{
				flag = true;
				if (DynamicMeshManager.DoLog)
				{
					Log.Out(ToDebugLocation() + " Waiting for lock to release: " + lastLock);
				}
			}
			if (totalSeconds > 60.0 && Monitor.IsEntered(_lock))
			{
				Log.Warning("Forcing lock release to " + debug + " from " + lastLock + " after " + totalSeconds + " seconds");
				ReleaseLock();
			}
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

	public bool TryGetBytes(out byte[] bytes, string debug)
	{
		if (TryTakeLock(debug))
		{
			bytes = Bytes;
			return true;
		}
		bytes = null;
		return false;
	}

	public bool TryExit(string debug)
	{
		if (!Monitor.IsEntered(_lock))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Warning(ToDebugLocation() + " Tried to release lock when not owner " + debug);
			}
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
		if (Bytes != null && !StateInfo.HasFlag(DynamicMeshStates.MarkedForDelete) && !StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating))
		{
			return !StateInfo.HasFlag(DynamicMeshStates.LoadRequired);
		}
		return false;
	}

	public string ToDebugLocation()
	{
		return string.Format("{0}:{1},{2}", IsRegion ? "R" : "C", X, Z);
	}

	public void ClearUnloadMarks()
	{
		StateInfo &= ~DynamicMeshStates.UnloadMark1;
		StateInfo &= ~DynamicMeshStates.UnloadMark2;
		StateInfo &= ~DynamicMeshStates.UnloadMark3;
	}

	public static DynamicMeshData Create(int x, int z, bool isRegion)
	{
		return new DynamicMeshData
		{
			X = x,
			Z = z,
			IsRegion = isRegion
		};
	}
}
