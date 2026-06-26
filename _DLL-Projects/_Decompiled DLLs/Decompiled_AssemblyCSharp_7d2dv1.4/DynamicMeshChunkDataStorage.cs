#define UNITY_STANDALONE
using System;
using System.Collections.Concurrent;
using System.IO;
using Noemax.GZip;

public class DynamicMeshChunkDataStorage<T> where T : DynamicMeshContainer
{
	public ConcurrentDictionary<long, DynamicMeshChunkDataWrapper> ChunkData = new ConcurrentDictionary<long, DynamicMeshChunkDataWrapper>();

	public ConcurrentStack<DynamicMeshChunkDataWrapper> LoadRequests = new ConcurrentStack<DynamicMeshChunkDataWrapper>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime NextCachePurge = DateTime.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int PurgeInterval = 3;

	public int LiveItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream FileMemoryStream = new MemoryStream();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] Buffer = new byte[2048];

	public int MaxAllowedItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly LoosePool<byte> Pool = new LoosePool<byte>();

	public long BytesAllocated;

	public long BytesReleased;

	public int LargestAllocation;

	public DynamicMeshChunkDataStorage(int purgeInterval)
	{
		PurgeInterval = purgeInterval;
	}

	public void ClearQueues()
	{
		Log.Out("Clearing queues.");
		DynamicMeshChunkDataWrapper result;
		while (LoadRequests.TryPop(out result))
		{
		}
		CleanUpAndSave();
		ChunkData.Clear();
		Log.Out("Cleared queues.");
	}

	public bool IsReadyThreaded()
	{
		if (MaxAllowedItems != 0)
		{
			return LiveItems < MaxAllowedItems - 1;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ForceLoadItem(DynamicMeshChunkDataWrapper wrapper)
	{
		return TryLoadItem(wrapper, releaseLock: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryLoadItem(DynamicMeshChunkDataWrapper wrapper, bool releaseLock)
	{
		lock (_lock)
		{
			string text = wrapper.Path();
			if (!SdFile.Exists(text))
			{
				wrapper.StateInfo |= DynamicMeshStates.FileMissing;
				return false;
			}
			bool num = wrapper.StateInfo.HasFlag(DynamicMeshStates.SaveRequired);
			DynamicMeshChunkData data;
			bool flag = wrapper.TryGetData(out data, "tryLoadItem");
			if (!num && flag)
			{
				using Stream stream = SdFile.OpenRead(text);
				int num2 = 0;
				int num3 = 0;
				FileMemoryStream.Position = 0L;
				FileMemoryStream.SetLength(0L);
				stream.ReadByte();
				stream.ReadByte();
				stream.ReadByte();
				stream.ReadByte();
				using DeflateInputStream deflateInputStream = new DeflateInputStream(stream);
				do
				{
					num3 = deflateInputStream.Read(Buffer, 0, Buffer.Length);
					num2 += num3;
					if (num3 > 0)
					{
						FileMemoryStream.Write(Buffer, 0, num3);
					}
				}
				while (num3 == Buffer.Length);
				DynamicMeshChunkData data2 = DynamicMeshChunkData.LoadFromStream(FileMemoryStream);
				if (DynamicMeshManager.DoLog)
				{
					Log.Out("LOAD FILE SIZE: " + text + " @ " + FileMemoryStream.Length + " OR " + num2);
				}
				ReplaceData(wrapper, data2, "loadItem");
			}
			if (releaseLock)
			{
				wrapper.TryExit("tryLoadItem");
			}
			wrapper.StateInfo &= ~DynamicMeshStates.LoadRequired;
			wrapper.StateInfo &= ~DynamicMeshStates.FileMissing;
			wrapper.StateInfo &= ~DynamicMeshStates.LoadBoosted;
		}
		return true;
	}

	public bool IsUpdating(T item)
	{
		return GetWrapper(item.Key).StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating);
	}

	public void MarkAsUpdating(T item)
	{
		GetWrapper(item.Key).StateInfo |= DynamicMeshStates.ThreadUpdating;
	}

	public void MarkAsUpdated(T item)
	{
		if (item != null)
		{
			GetWrapper(item.Key).StateInfo &= ~DynamicMeshStates.ThreadUpdating;
		}
	}

	public void MarkAsGenerating(T item)
	{
		GetWrapper(item.Key).StateInfo |= DynamicMeshStates.Generating;
	}

	public void MarkAsGenerated(T item)
	{
		if (item != null)
		{
			GetWrapper(item.Key).StateInfo &= ~DynamicMeshStates.Generating;
		}
	}

	public bool SaveNetPackageData(int x, int z, byte[] data, int updateTime)
	{
		long itemKey = DynamicMeshUnity.GetItemKey(x, z);
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return false;
		}
		DynamicMeshChunkDataWrapper wrapper = GetWrapper(itemKey);
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Adding Saving from net " + wrapper.ToDebugLocation() + ":" + ((data != null) ? data.Length : 0));
		}
		string debug = "addSave";
		if (!wrapper.GetLock(debug))
		{
			Log.Warning("Could not get lock on save request: " + wrapper.ToDebugLocation());
			return false;
		}
		ReleaseData(wrapper, "saveRequestNet");
		bool num = data == null || data.Length == 0;
		string itemPath = DynamicMeshUnity.GetItemPath(itemKey);
		if (num)
		{
			SdFile.Delete(itemPath);
		}
		else
		{
			DynamicMeshUnity.EnsureDMDirectoryExists();
			using Stream baseStream = SdFile.Create(itemPath);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			pooledBinaryWriter.Write(data, 0, data.Length);
		}
		DynamicMeshManager instance = DynamicMeshManager.Instance;
		int num2;
		if ((object)instance == null)
		{
			num2 = 0;
		}
		else
		{
			num2 = (instance.IsInLoadableArea(wrapper.Key) ? 1 : 0);
			if (num2 != 0)
			{
				DynamicMeshThread.AddRegionUpdateData(wrapper.X, wrapper.Z, isUrgent: false);
				DynamicMeshThread.ChunkReadyForCollection.Add(new Vector2i(wrapper.X, wrapper.Z));
			}
		}
		ClearLock(wrapper, "_SAVERELEASE_NET_");
		return (byte)num2 != 0;
	}

	public void AddSaveRequest(long key, DynamicMeshChunkData data)
	{
		if (DynamicMeshManager.CONTENT_ENABLED)
		{
			DynamicMeshChunkDataWrapper wrapper = GetWrapper(key);
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Adding Saving " + wrapper.ToDebugLocation());
			}
			string debug = "addSave";
			if (!wrapper.GetLock(debug))
			{
				Log.Warning("Could not get lock on save request: " + wrapper.ToDebugLocation());
				return;
			}
			ReplaceData(wrapper, data, "saveRequest");
			SaveItem(wrapper);
			DynamicMeshThread.ChunkReadyForCollection.Add(new Vector2i(wrapper.X, wrapper.Z));
			ClearLock(wrapper, "_SAVERELEASE_");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveItem(DynamicMeshChunkDataWrapper wrapper)
	{
		wrapper.GetLock("saveItem");
		wrapper.TryGetData(out var data, "saveItemTry");
		wrapper.ClearUnloadMarks();
		wrapper.StateInfo &= ~DynamicMeshStates.SaveRequired;
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Saving " + wrapper.ToDebugLocation() + ":" + data?.GetStreamSize().ToString());
		}
		string path = wrapper.Path();
		if (data == null)
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Deleting null bytes " + wrapper.ToDebugLocation());
			}
			if (SdFile.Exists(path))
			{
				SdFile.Delete(path);
			}
			return;
		}
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Saving to disk " + wrapper.ToDebugLocation());
		}
		int streamSize = data.GetStreamSize();
		int count = 0;
		byte[] fromPool = DynamicMeshThread.ChunkDataQueue.GetFromPool(streamSize);
		using (MemoryStream baseStream = new MemoryStream(fromPool))
		{
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			DynamicMeshChunkData dynamicMeshChunkData = data;
			int updateTime = (data.UpdateTime = (int)(DateTime.UtcNow - DynamicMeshFile.ItemMin).TotalSeconds);
			dynamicMeshChunkData.UpdateTime = updateTime;
			data.Write(pooledBinaryWriter);
			count = (int)pooledBinaryWriter.BaseStream.Position;
		}
		DynamicMeshUnity.EnsureDMDirectoryExists();
		using (Stream stream = SdFile.Create(path))
		{
			using (PooledBinaryWriter pooledBinaryWriter2 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter2.SetBaseStream(stream);
				pooledBinaryWriter2.Write(data.UpdateTime);
			}
			using DeflateOutputStream deflateOutputStream = new DeflateOutputStream(stream, 3, leaveOpen: false);
			deflateOutputStream.Write(fromPool, 0, count);
		}
		wrapper.StateInfo &= ~DynamicMeshStates.FileMissing;
		DynamicMeshThread.ChunkDataQueue.ManuallyReleaseBytes(fromPool);
		ReleaseData(wrapper, "saveItem");
	}

	public void CleanUpAndSave()
	{
	}

	public void MarkForDeletion(long worldPosition)
	{
		GetWrapper(worldPosition).StateInfo |= DynamicMeshStates.MarkedForDelete;
	}

	public void MarkForUnload(long worldPosition)
	{
		DynamicMeshChunkDataWrapper wrapper = GetWrapper(worldPosition);
		wrapper.StateInfo |= DynamicMeshStates.UnloadMark1;
		wrapper.StateInfo |= DynamicMeshStates.UnloadMark2;
		wrapper.StateInfo |= DynamicMeshStates.UnloadMark3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ReplaceData(DynamicMeshChunkDataWrapper wrapper, DynamicMeshChunkData data, string debug)
	{
		while (!ReleaseData(wrapper, "replaceData"))
		{
			Log.Out("Waiting for bytes to be released: " + wrapper.ToDebugLocation());
		}
		wrapper.Data = data;
		return true;
	}

	public bool ClearLock(DynamicMeshChunkDataWrapper wrapper, string debug)
	{
		ReleaseData(wrapper.Key, "CL_" + debug);
		if (!wrapper.TryExit("ClearLock " + debug))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ClearLock(long worldPosition, string debug)
	{
		if (!GetWrapper(worldPosition).TryExit(debug))
		{
			return false;
		}
		return true;
	}

	public void LogMemoryUsage()
	{
	}

	public void FreeMemory()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ReleaseData(DynamicMeshChunkDataWrapper wrapper, string debug)
	{
		if (!wrapper.ThreadHasLock())
		{
			Log.Error("You can not release bytes if you do not have the lock: " + wrapper.ToDebugLocation());
			return false;
		}
		if (wrapper.TryGetData(out var data, "releaseData" + debug))
		{
			if (data != null)
			{
				data.Reset();
				DynamicMeshChunkData.AddToCache(data, "releaseData_" + debug);
				wrapper.Data = null;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ReleaseData(long worldPosition, string debug)
	{
		DynamicMeshChunkDataWrapper wrapper = GetWrapper(worldPosition);
		if (!wrapper.TryTakeLock("releaseDataQueue"))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Out(wrapper.ToDebugLocation() + " Could not clear from queue because item is locked by " + wrapper.lastLock);
			}
			return false;
		}
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Releasing " + wrapper.ToDebugLocation());
		}
		bool result = ReleaseData(wrapper, "RD_" + debug);
		wrapper.Reset();
		if (wrapper.StateInfo.HasFlag(DynamicMeshStates.MarkedForDelete))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Deleting file from disk " + wrapper.ToDebugLocation());
			}
			string path = wrapper.Path();
			if (SdFile.Exists(path))
			{
				SdFile.Delete(path);
			}
		}
		return result;
	}

	public bool CollectBytes(long key, out byte[] data, out int length)
	{
		DynamicMeshChunkDataWrapper wrapper = GetWrapper(key);
		if (!wrapper.TryTakeLock("collectBytes"))
		{
			data = null;
			length = 0;
			return false;
		}
		while (!wrapper.GetLock("collectBytes"))
		{
			Log.Out("failed to get lock on collect");
		}
		string itemPath = DynamicMeshUnity.GetItemPath(key);
		if (!SdFile.Exists(itemPath))
		{
			data = null;
			length = 0;
		}
		else
		{
			length = (int)new SdFileInfo(itemPath).Length;
			data = GetFromPool(length);
			using Stream stream = SdFile.OpenRead(itemPath);
			stream.Read(data, 0, length);
		}
		ClearLock(wrapper, "collectBytes");
		return true;
	}

	public bool CollectItem(long worldPosition, out DynamicMeshChunkDataWrapper wrapper, out string debugMessage)
	{
		debugMessage = string.Empty;
		wrapper = GetWrapper(worldPosition);
		while (!wrapper.GetLock("collectItem"))
		{
			Log.Out("failed to get lock on collect");
		}
		if (wrapper.StateInfo.HasFlag(DynamicMeshStates.MarkedForDelete))
		{
			debugMessage = "toDelete";
			return true;
		}
		if (wrapper.StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating))
		{
			debugMessage = "threadUpdating";
			return false;
		}
		ForceLoadItem(wrapper);
		return true;
	}

	public DynamicMeshChunkDataWrapper GetWrapper(long key)
	{
		if (!ChunkData.TryGetValue(key, out var value))
		{
			value = DynamicMeshChunkDataWrapper.Create(key);
			if (!ChunkData.TryAdd(key, value))
			{
				ChunkData.TryGetValue(key, out value);
				Log.Error("Request failed to add data: " + DynamicMeshUnity.GetDebugPositionKey(key));
			}
		}
		return value;
	}

	public byte[] GetFromPool(int length)
	{
		byte[] array = Pool.Alloc(length);
		BytesAllocated += array.Length;
		if (array.Length > LargestAllocation)
		{
			LargestAllocation = array.Length;
		}
		return array;
	}

	public bool ManuallyReleaseBytes(byte[] bytes)
	{
		if (bytes != null)
		{
			Pool.Free(bytes);
			BytesReleased += bytes.Length;
			return true;
		}
		return false;
	}

	public bool IsReadyToCollect(long worldPosition)
	{
		return GetWrapper(worldPosition).Data != null;
	}
}
