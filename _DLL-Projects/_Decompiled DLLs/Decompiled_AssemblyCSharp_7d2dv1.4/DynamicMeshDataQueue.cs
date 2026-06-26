using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ConcurrentCollections;
using Noemax.GZip;

public class DynamicMeshDataQueue<T> where T : DynamicMeshContainer
{
	public ConcurrentDictionary<Vector3i, DynamicMeshData> Cache = new ConcurrentDictionary<Vector3i, DynamicMeshData>();

	public ConcurrentStack<DynamicMeshData> MainThreadLoadRequests = new ConcurrentStack<DynamicMeshData>();

	public ConcurrentStack<DynamicMeshData> LoadRequests = new ConcurrentStack<DynamicMeshData>();

	public ConcurrentHashSet<DynamicMeshData> SaveRequests = new ConcurrentHashSet<DynamicMeshData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly LoosePool<byte> Pool = new LoosePool<byte>();

	public DynamicMeshData ImportantLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	public string MeshFolder;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsRegionQueue;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime NextCachePurge = DateTime.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int PurgeInterval = 3;

	public long BytesAllocated;

	public long BytesReleased;

	public int LiveItems;

	public int LargestAllocation;

	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream FileMemoryStream = new MemoryStream();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] Buffer = new byte[2048];

	public int MaxAllowedItems;

	public long MBytesAllocated => BytesAllocated / 1024 / 1024;

	public long MBytesReleased => BytesReleased / 1024 / 1024;

	public long BytesLive => BytesAllocated - BytesReleased;

	public long MBytesLive => BytesLive / 1024 / 1024;

	public bool HasLoadRequests
	{
		get
		{
			if (LoadRequests.Count > 0 || ImportantLoad != null)
			{
				return IsReadyThreaded();
			}
			return false;
		}
	}

	public DynamicMeshDataQueue(bool isRegion, int purgeInterval)
	{
		MeshFolder = DynamicMeshFile.MeshLocation;
		IsRegionQueue = isRegion;
		PurgeInterval = purgeInterval;
		Pool.EnforceMaxSize = true;
	}

	public string GetQueueType()
	{
		if (!IsRegionQueue)
		{
			return "Item";
		}
		return "Region";
	}

	public byte[] GetFromPool(int length)
	{
		byte[] array = Pool.Alloc(length);
		BytesAllocated += array.Length;
		if (array.Length > LargestAllocation)
		{
			LargestAllocation = array.Length;
		}
		LiveItems++;
		return array;
	}

	public byte[] Clone(byte[] source)
	{
		byte[] fromPool = GetFromPool(source.Length);
		Array.Copy(source, fromPool, source.Length);
		return fromPool;
	}

	public void ClearQueues()
	{
		Log.Out("Clearing queues. IsRegion: " + IsRegionQueue);
		DynamicMeshData result;
		while (LoadRequests.TryPop(out result))
		{
		}
		CleanUpAndSave();
		Cache.Clear();
		Log.Out("Cleared queues. IsRegion: " + IsRegionQueue);
	}

	public bool LoadItem()
	{
		if (ImportantLoad != null)
		{
			if (!TryLoadItem(ImportantLoad))
			{
				ImportantLoad.StateInfo |= DynamicMeshStates.FileMissing;
				Log.Out("Important load FAILED: " + ImportantLoad.ToDebugLocation());
				ImportantLoad = null;
			}
			ImportantLoad = null;
		}
		DynamicMeshData result;
		while (MainThreadLoadRequests.TryPop(out result))
		{
			if (!TryLoadItem(result))
			{
				Log.Out("Failed main thread load " + result.ToDebugLocation());
			}
		}
		if (LiveItems >= MaxAllowedItems)
		{
			return false;
		}
		if (LoadRequests.Count == 0)
		{
			return false;
		}
		if (!LoadRequests.TryPop(out result))
		{
			return false;
		}
		if (result.StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating))
		{
			return false;
		}
		if (!TryLoadItem(result))
		{
			LoadRequests.Push(result);
		}
		return true;
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
	public bool ForceLoadItem(DynamicMeshData data, out byte[] bytes)
	{
		if (!data.GetLock("forceLoad"))
		{
			bytes = null;
			return false;
		}
		TryLoadItem(data);
		data.TryGetBytes(out bytes, "forceTry");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryLoadItem(DynamicMeshData data)
	{
		lock (_lock)
		{
			string text = data.Path(IsRegionQueue);
			if (!SdFile.Exists(text))
			{
				data.StateInfo |= DynamicMeshStates.FileMissing;
				return false;
			}
			bool num = data.StateInfo.HasFlag(DynamicMeshStates.SaveRequired);
			byte[] bytes;
			bool flag = data.TryGetBytes(out bytes, "tryLoadItem");
			if (!num && flag)
			{
				if (DynamicMeshManager.CompressFiles)
				{
					using Stream input = SdFile.OpenRead(text);
					int num2 = 0;
					int num3 = 0;
					FileMemoryStream.Position = 0L;
					FileMemoryStream.SetLength(0L);
					using DeflateInputStream deflateInputStream = new DeflateInputStream(input);
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
					bytes = GetFromPool((int)FileMemoryStream.Length);
					FileMemoryStream.Position = 0L;
					FileMemoryStream.Read(bytes, 0, (int)FileMemoryStream.Length);
					if (DynamicMeshManager.DoLog)
					{
						Log.Out("LOAD FILE SIZE: " + text + " @ " + FileMemoryStream.Length + " OR " + num2);
					}
					ReplaceBytes(data, bytes, "loadItem");
					data.StreamLength = (int)FileMemoryStream.Length;
				}
				else
				{
					using Stream baseStream = SdFile.OpenRead(text);
					using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
					pooledBinaryReader.SetBaseStream(baseStream);
					int num4 = (int)pooledBinaryReader.BaseStream.Length;
					bytes = GetFromPool(num4);
					pooledBinaryReader.Read(bytes, 0, num4);
					ReplaceBytes(data, bytes, "loadUncompressed");
					data.StreamLength = num4;
				}
			}
			data.TryExit("tryLoadItem");
			data.StateInfo &= ~DynamicMeshStates.LoadRequired;
			data.StateInfo &= ~DynamicMeshStates.FileMissing;
			data.StateInfo &= ~DynamicMeshStates.LoadBoosted;
		}
		return true;
	}

	public string GetCacheSize()
	{
		int totalBytes;
		return (IsRegionQueue ? "Region Queue\n" : "Item Queue\n") + Pool.GetSize(out totalBytes);
	}

	public bool IsUpdating(T item)
	{
		return GetData(item.WorldPosition).StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating);
	}

	public void MarkAsUpdating(T item)
	{
		GetData(item.WorldPosition).StateInfo |= DynamicMeshStates.ThreadUpdating;
	}

	public void MarkAsUpdated(T item)
	{
		if (item != null)
		{
			GetData(item.WorldPosition).StateInfo &= ~DynamicMeshStates.ThreadUpdating;
		}
	}

	public void ClearMainThreadTag(T item)
	{
		if (item != null)
		{
			GetData(item.WorldPosition).StateInfo &= ~DynamicMeshStates.MainThreadLoadRequest;
		}
	}

	public void ResetData(T item)
	{
		AddSaveRequest(item, null, 0, requestRegionUpdate: true, unloadImmediately: true, loadInWorld: false);
	}

	public void AddSaveRequest(T item, byte[] bytes, int length, bool requestRegionUpdate, bool unloadImmediately, bool loadInWorld)
	{
		AddSaveRequest(item.WorldPosition, bytes, length, requestRegionUpdate, unloadImmediately, loadInWorld);
	}

	public void AddSaveRequest(Vector3i worldPosition, byte[] bytes, int length, bool requestRegionUpdate, bool unloadImmediately, bool loadInWorld)
	{
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return;
		}
		DynamicMeshData data = GetData(worldPosition);
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Adding Saving " + GetQueueType() + " " + data.ToDebugLocation() + ":" + length);
		}
		string debug = "addSave";
		if (!data.GetLock(debug))
		{
			Log.Warning("Could not get lock on save request: " + data.ToDebugLocation());
			return;
		}
		data.StreamLength = length;
		ReplaceBytes(data, bytes, "saveRequest");
		data.StateInfo |= DynamicMeshStates.SaveRequired;
		data.StateInfo &= ~DynamicMeshStates.MarkedForDelete;
		if (unloadImmediately)
		{
			data.StateInfo |= DynamicMeshStates.UnloadMark1;
			data.StateInfo |= DynamicMeshStates.UnloadMark2;
			data.StateInfo |= DynamicMeshStates.UnloadMark3;
		}
		SaveRequests.Add(data);
		data.TryExit(debug);
		if (IsRegionQueue)
		{
			DynamicMeshManager.Instance.AddUpdateData(data.X, data.Z, isUrgent: false, addToThread: false);
		}
	}

	public void SaveNetworkPackage(Vector3i worldPosition, byte[] bytes, int length)
	{
		try
		{
			string path = DynamicMeshFile.MeshLocation + $"{worldPosition.x},{worldPosition.z}.mesh";
			if (bytes == null)
			{
				if (SdFile.Exists(path))
				{
					SdFile.Delete(path);
				}
				return;
			}
			using Stream output = SdFile.Create(path);
			using DeflateOutputStream deflateOutputStream = new DeflateOutputStream(output, 3, leaveOpen: false);
			deflateOutputStream.Write(bytes, 0, length);
		}
		catch (Exception ex)
		{
			Log.Error("Data queue error: " + ex.Message);
		}
	}

	public bool TrySave(bool forceSave = false)
	{
		if (SaveRequests.Count == 0)
		{
			return false;
		}
		try
		{
			if (!SaveRequests.TryRemoveFirst(out var returnValue))
			{
				return false;
			}
			if (!forceSave && returnValue.StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating))
			{
				Log.Out("Skipping save on updating item " + returnValue.ToDebugLocation());
				SaveRequests.Add(returnValue);
				return false;
			}
			SaveItem(returnValue);
			returnValue.TryExit("saveItem");
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Data queue error: " + ex.Message);
		}
		return false;
	}

	public void SaveItem(DynamicMeshData data)
	{
		data.GetLock("saveItem");
		data.TryGetBytes(out var bytes, "saveItemTry");
		data.ClearUnloadMarks();
		data.StateInfo &= ~DynamicMeshStates.SaveRequired;
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Saving " + GetQueueType() + " " + data.ToDebugLocation() + ":" + bytes?.Length.ToString());
		}
		string path = data.Path(IsRegionQueue);
		if (bytes == null)
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Deleting null bytes " + GetQueueType() + " " + data.ToDebugLocation());
			}
			if (SdFile.Exists(path))
			{
				SdFile.Delete(path);
			}
			return;
		}
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Saving " + GetQueueType() + " to disk " + data.ToDebugLocation() + ": " + data.StreamLength);
		}
		if (DynamicMeshManager.CompressFiles)
		{
			using (Stream output = SdFile.Create(path))
			{
				using DeflateOutputStream deflateOutputStream = new DeflateOutputStream(output, 3, leaveOpen: false);
				deflateOutputStream.Write(bytes, 0, data.StreamLength);
				return;
			}
		}
		throw new NotImplementedException("Compression is not active");
	}

	public void CleanUpAndSave()
	{
		while (SaveRequests.Count > 0)
		{
			TrySave(forceSave: true);
		}
	}

	public void TryRelease()
	{
		if (NextCachePurge > DateTime.Now)
		{
			return;
		}
		NextCachePurge = DateTime.Now.AddSeconds(PurgeInterval);
		foreach (KeyValuePair<Vector3i, DynamicMeshData> item in Cache)
		{
			DynamicMeshData value = item.Value;
			if (value.X == 0 && value.Z == 0)
			{
				continue;
			}
			if (value.StateInfo.HasFlag(DynamicMeshStates.SaveRequired) && SaveRequests.Count > 0)
			{
				if (DynamicMeshManager.DebugReleases)
				{
					Log.Out($"{value.X},{value.Z} save required");
				}
			}
			else if (value.StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating))
			{
				if (DynamicMeshManager.DebugReleases)
				{
					Log.Out($"{value.X},{value.Z} thread updating");
				}
			}
			else if (!value.StateInfo.HasFlag(DynamicMeshStates.UnloadMark1) && !value.StateInfo.HasFlag(DynamicMeshStates.MarkedForDelete))
			{
				value.StateInfo |= DynamicMeshStates.UnloadMark1;
			}
			else
			{
				ReleaseData(item.Key);
			}
		}
	}

	public void MarkForDeletion(Vector3i worldPosition)
	{
		GetData(worldPosition).StateInfo |= DynamicMeshStates.MarkedForDelete;
	}

	public void MarkForUnload(Vector3i worldPosition)
	{
		DynamicMeshData data = GetData(worldPosition);
		data.StateInfo |= DynamicMeshStates.UnloadMark1;
		data.StateInfo |= DynamicMeshStates.UnloadMark2;
		data.StateInfo |= DynamicMeshStates.UnloadMark3;
	}

	public bool ReplaceBytes(DynamicMeshData data, byte[] bytes, string debug)
	{
		if (!data.GetLock("ReplaceBytes"))
		{
			Log.Out("ReplaceBytesLockFailed");
			return false;
		}
		while (!ReleaseBytes(data))
		{
			Log.Out("Waiting for bytes to be released: " + data.ToDebugLocation());
		}
		data.Bytes = bytes;
		return true;
	}

	public bool ClearLock(T item, string debug, bool releasePool)
	{
		if (item == null)
		{
			return true;
		}
		DynamicMeshData data = GetData(item.WorldPosition);
		if (releasePool && !data.StateInfo.HasFlag(DynamicMeshStates.SaveRequired))
		{
			ReleaseData(item.WorldPosition);
		}
		if (!data.TryExit("ClearLock " + debug))
		{
			return false;
		}
		return true;
	}

	public bool ClearLock(Vector3i worldPosition, string debug)
	{
		if (!GetData(worldPosition).TryExit(debug))
		{
			return false;
		}
		return true;
	}

	public void LogMemoryUsage()
	{
		Pool.GetSize(out var totalBytes);
		Log.Out($"{GetQueueType()}   Allocated: {MBytesAllocated}   Released: {MBytesReleased}   {LiveItems}x Live: {MBytesLive}   {Pool.GetTotalItems()}x CacheMb: {totalBytes / 1024 / 1024}   Longest: {LargestAllocation}");
	}

	public void FreeMemory()
	{
		Pool.FreeAll();
	}

	public bool ReleaseBytes(DynamicMeshData data)
	{
		if (!data.ThreadHasLock())
		{
			Log.Error("You can not release bytes if you do not have the lock: " + data.ToDebugLocation());
			return false;
		}
		if (data.TryGetBytes(out var bytes, "releaseBytes"))
		{
			if (bytes != null)
			{
				Pool.Free(bytes);
				BytesReleased += bytes.Length;
				LiveItems--;
				data.Bytes = null;
			}
			return true;
		}
		return false;
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

	public bool ReleaseData(Vector3i worldPosition)
	{
		worldPosition.y = 0;
		DynamicMeshData value = GetData(worldPosition);
		if (!value.TryTakeLock("releaseDataQueue"))
		{
			Log.Out("Could not clear from queue because item is locked by " + value.lastLock);
			return false;
		}
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Releasing " + GetQueueType() + " " + value.ToDebugLocation());
		}
		bool result = ReleaseBytes(value);
		if (!Cache.TryRemove(worldPosition, out value))
		{
			Vector3i vector3i = worldPosition;
			Log.Out("Could not remove item from cache: " + vector3i.ToString());
		}
		if (value.StateInfo.HasFlag(DynamicMeshStates.MarkedForDelete))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Deleting " + GetQueueType() + " file from disk " + value.ToDebugLocation());
			}
			string path = value.Path(IsRegionQueue);
			if (SdFile.Exists(path))
			{
				SdFile.Delete(path);
			}
		}
		return result;
	}

	public bool CollectItem(T item, bool forceLoad, bool mainThreadLoad, bool waitForLock, out byte[] bytes, out int length)
	{
		string debugMessage;
		return CollectItem(item, forceLoad, mainThreadLoad, waitForLock, out bytes, out debugMessage, out length);
	}

	public bool CollectItem(T item, bool forceLoad, bool mainThreadLoad, bool waitForLock, out byte[] bytes, out string debugMessage, out int length)
	{
		if (item == null)
		{
			bytes = null;
			debugMessage = "null";
			length = 0;
			return false;
		}
		return CollectItem(item.WorldPosition, forceLoad, mainThreadLoad, waitForLock, out bytes, out debugMessage, out length);
	}

	public bool CollectItem(Vector3i worldPosition, bool forceLoad, bool mainThreadLoad, bool waitForLock, out byte[] bytes, out string debugMessage, out int length)
	{
		debugMessage = string.Empty;
		DynamicMeshData data = GetData(worldPosition);
		data.ClearUnloadMarks();
		if (data.IsAvailableToLoad())
		{
			while (!data.TryGetBytes(out bytes, "collectItem"))
			{
				if (waitForLock)
				{
					if (DynamicMeshManager.DoLog)
					{
						Log.Out("Waiting for lock on CollectItem " + data.ToDebugLocation() + " (" + debugMessage + ")");
					}
					continue;
				}
				debugMessage = "locked";
				length = 0;
				return false;
			}
			length = data.StreamLength;
			if (bytes != null || !data.Exists(IsRegionQueue))
			{
				return true;
			}
		}
		if (data.StateInfo.HasFlag(DynamicMeshStates.MarkedForDelete))
		{
			debugMessage = "toDelete";
			bytes = null;
			length = 0;
			return true;
		}
		if (data.StateInfo.HasFlag(DynamicMeshStates.ThreadUpdating))
		{
			debugMessage = "threadUpdating";
			bytes = null;
			length = 0;
			return false;
		}
		if (forceLoad)
		{
			ForceLoadItem(data, out bytes);
			length = data.StreamLength;
			return true;
		}
		if (data.StateInfo.HasFlag(DynamicMeshStates.FileMissing))
		{
			debugMessage = DynamicMeshManager.FileMissing;
			bytes = null;
			length = 0;
			return false;
		}
		if (mainThreadLoad)
		{
			ImportantLoad = data;
		}
		else
		{
			LoadRequests.Push(data);
		}
		if (!data.StateInfo.HasFlag(DynamicMeshStates.LoadRequired))
		{
			data.StateInfo |= DynamicMeshStates.LoadRequired;
		}
		else if (data.StateInfo.HasFlag(DynamicMeshStates.LoadRequired) && LoadRequests.Count > 0 && !data.StateInfo.HasFlag(DynamicMeshStates.LoadBoosted))
		{
			data.StateInfo |= DynamicMeshStates.LoadBoosted;
			LoadRequests.Push(data);
		}
		debugMessage = "dunno";
		length = 0;
		bytes = null;
		return false;
	}

	public DynamicMeshData TryGetData(Vector3i worldPosition)
	{
		worldPosition.y = 0;
		DynamicMeshData value = null;
		Cache.TryGetValue(worldPosition, out value);
		return value;
	}

	public DynamicMeshData GetData(Vector3i worldPosition)
	{
		worldPosition.y = 0;
		DynamicMeshData value = null;
		if (!Cache.TryGetValue(worldPosition, out value))
		{
			value = DynamicMeshData.Create(worldPosition.x, worldPosition.z, IsRegionQueue);
			if (!Cache.TryAdd(worldPosition, value))
			{
				Cache.TryGetValue(worldPosition, out value);
				Log.Error("Request failed to add data: " + worldPosition.ToDebugLocation());
			}
		}
		return value;
	}

	public void AddToLoadListMainThread(Vector3i worldPosition)
	{
		DynamicMeshData data = GetData(worldPosition);
		if (data.Exists(isRegionQueue: false))
		{
			data.ClearUnloadMarks();
			data.StateInfo |= DynamicMeshStates.MainThreadLoadRequest;
			if (!data.StateInfo.HasFlag(DynamicMeshStates.LoadRequired))
			{
				data.StateInfo |= DynamicMeshStates.LoadRequired;
				MainThreadLoadRequests.Push(data);
			}
		}
	}

	public bool IsReadyToCollect(Vector3i worldPosition)
	{
		return GetData(worldPosition).Bytes != null;
	}
}
