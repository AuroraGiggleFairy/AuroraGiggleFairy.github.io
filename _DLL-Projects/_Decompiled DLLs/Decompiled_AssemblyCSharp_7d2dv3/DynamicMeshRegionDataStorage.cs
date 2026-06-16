#define UNITY_STANDALONE
using System;
using System.Collections.Concurrent;
using System.IO;
using Noemax.GZip;

public class DynamicMeshRegionDataStorage
{
	public ConcurrentDictionary<long, DynamicMeshRegionDataWrapper> ChunkData = new ConcurrentDictionary<long, DynamicMeshRegionDataWrapper>();

	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream FileMemoryStream = new MemoryStream();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] Buffer = new byte[2048];

	public int MaxAllowedItems;

	public void ClearQueues()
	{
		Log.Out("Clearing queues.");
		CleanUpAndSave();
		ChunkData.Clear();
		Log.Out("Cleared queues.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryLoadItem(DynamicMeshRegionDataWrapper wrapper, bool releaseLock)
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
			bool flag = false;
			if (!num && flag)
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
				DynamicMeshChunkData.LoadFromStream(FileMemoryStream);
				if (DynamicMeshManager.DoLog)
				{
					Log.Out("LOAD FILE SIZE: " + text + " @ " + FileMemoryStream.Length + " OR " + num2);
				}
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

	public void AddSaveRequest(long key, DynamicMeshChunkData data, int length, bool requestRegionUpdate, bool unloadImmediately, bool loadInWorld)
	{
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return;
		}
		DynamicMeshRegionDataWrapper wrapper = GetWrapper(key);
		if (DynamicMeshManager.DoLog)
		{
			Log.Out("Adding Saving " + wrapper.ToDebugLocation() + ":" + length);
		}
		string debug = "addSave";
		if (!wrapper.GetLock(debug))
		{
			Log.Warning("Could not get lock on save request: " + wrapper.ToDebugLocation());
			return;
		}
		SaveItem(wrapper);
		if (requestRegionUpdate)
		{
			DynamicMeshThread.AddRegionUpdateData(wrapper.X, wrapper.Z, isUrgent: false);
		}
		if (loadInWorld)
		{
			DynamicMeshThread.ChunkReadyForCollection.Add(new Vector2i(wrapper.X, wrapper.Z));
		}
		ClearLock(wrapper, "_SAVERELEASE_");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveItem(DynamicMeshRegionDataWrapper wrapper)
	{
		DynamicMeshChunkData dynamicMeshChunkData = null;
		wrapper.GetLock("saveItem");
		wrapper.ClearUnloadMarks();
		wrapper.StateInfo &= ~DynamicMeshStates.SaveRequired;
		string path = wrapper.Path();
		if (dynamicMeshChunkData == null)
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
		int streamSize = dynamicMeshChunkData.GetStreamSize();
		int count = 0;
		byte[] fromPool = DynamicMeshThread.ChunkDataQueue.GetFromPool(streamSize);
		using (MemoryStream baseStream = new MemoryStream(fromPool))
		{
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			int updateTime = (dynamicMeshChunkData.UpdateTime = (int)(DateTime.UtcNow - DynamicMeshFile.ItemMin).TotalSeconds);
			dynamicMeshChunkData.UpdateTime = updateTime;
			dynamicMeshChunkData.Write(pooledBinaryWriter);
			count = (int)pooledBinaryWriter.BaseStream.Position;
		}
		DynamicMeshUnity.EnsureDMDirectoryExists();
		using (Stream output = SdFile.Create(path))
		{
			using DeflateOutputStream deflateOutputStream = new DeflateOutputStream(output, 3, leaveOpen: false);
			deflateOutputStream.Write(fromPool, 0, count);
		}
		wrapper.StateInfo &= ~DynamicMeshStates.FileMissing;
		DynamicMeshThread.ChunkDataQueue.ManuallyReleaseBytes(fromPool);
	}

	public void CleanUpAndSave()
	{
	}

	public bool ClearLock(DynamicMeshRegionDataWrapper wrapper, string debug)
	{
		if (!wrapper.TryExit("ClearLock " + debug))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ClearLock(long worldPosition, string debug)
	{
		DynamicMeshRegionDataWrapper wrapper = GetWrapper(worldPosition);
		return ClearLock(wrapper, debug);
	}

	public DynamicMeshRegionDataWrapper GetWrapper(long key)
	{
		if (!ChunkData.TryGetValue(key, out var value))
		{
			value = DynamicMeshRegionDataWrapper.Create(key);
			if (!ChunkData.TryAdd(key, value))
			{
				ChunkData.TryGetValue(key, out value);
				Log.Error("Request failed to add data: " + DynamicMeshUnity.GetDebugPositionKey(key));
			}
		}
		return value;
	}

	public void LoadRegion(DyMeshRegionLoadRequest load)
	{
		DynamicMeshRegionDataWrapper wrapper = GetWrapper(load.Key);
		wrapper.GetLock("loadRegion");
		string text = wrapper.Path();
		if (SdFile.Exists(text))
		{
			try
			{
				using Stream input = SdFile.OpenRead(text);
				int num = 0;
				int num2 = 0;
				FileMemoryStream.Position = 0L;
				FileMemoryStream.SetLength(0L);
				using DeflateInputStream deflateInputStream = new DeflateInputStream(input);
				do
				{
					num2 = deflateInputStream.Read(Buffer, 0, Buffer.Length);
					num += num2;
					if (num2 > 0)
					{
						FileMemoryStream.Write(Buffer, 0, num2);
					}
				}
				while (num2 == Buffer.Length);
				FileMemoryStream.Position = 0L;
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
				{
					pooledBinaryReader.SetBaseStream(FileMemoryStream);
					_ = (DateTime.UtcNow - DynamicMeshFile.ItemMin).TotalSeconds;
					DynamicMeshVoxelRegionLoad.LoadRegionFromFile(pooledBinaryReader, load);
				}
				if (DynamicMeshManager.DoLog)
				{
					Log.Out("LOAD FILE SIZE: " + text + " @ " + FileMemoryStream.Length + " OR " + num);
				}
			}
			catch (Exception ex)
			{
				Log.Out("Read file error on dymesh: " + ex.Message + ". Deleting corrupted file.");
				try
				{
					SdFile.Delete(text);
				}
				catch (Exception)
				{
					Log.Out("Unable to delete dymesh file. You should manually delete: " + text);
				}
			}
		}
		ClearLock(wrapper, "loadRegionRelease");
	}

	public void SaveRegion(DynamicMeshThread.ThreadRegion region, Vector3i worldPosition, VoxelMesh opaque, VoxelMeshTerrain terrain)
	{
		long key = region.Key;
		DynamicMeshRegionDataWrapper wrapper = GetWrapper(key);
		wrapper.GetLock("saveRegion");
		DynamicMeshUnity.EnsureDMDirectoryExists();
		using (Stream output = SdFile.Open(wrapper.Path(), FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			using DeflateOutputStream baseStream = new DeflateOutputStream(output, 3, leaveOpen: false);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			int updateTime = (int)(DateTime.UtcNow - DynamicMeshFile.ItemMin).TotalSeconds;
			DynamicMeshVoxelRegionLoad.SaveRegionToFile(pooledBinaryWriter, opaque, terrain, worldPosition, updateTime, null, null);
		}
		ClearLock(wrapper, "saveRegionRelease");
	}
}
