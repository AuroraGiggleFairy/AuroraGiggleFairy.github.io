using System;
using System.Collections.Generic;
using System.IO;

public class EventPrefabs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabCache prefabCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator dpd;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager rfm;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> prefabs = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadedFileWriterQueue saveWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool needsSaving;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSaveVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cSaveFilename = "eventprefabs.dat";

	public EventPrefabs(World world, DynamicPrefabDecorator prefabDecorator, RegionFileManager regionFileManager)
	{
		this.world = world;
		prefabCache = world.m_PrefabCache;
		dpd = prefabDecorator;
		rfm = regionFileManager;
		saveWriter = new ThreadedFileWriterQueue("EventPrefabs.Save", Path.Combine(GameIO.GetSaveGameDir(), "eventprefabs.dat"));
	}

	public void GetPrefabs(List<PrefabInstance> prefabs)
	{
		prefabs.AddRange(this.prefabs);
	}

	public List<PrefabInstance.Serializable> GetPrefabsSerialized()
	{
		List<PrefabInstance.Serializable> list = new List<PrefabInstance.Serializable>();
		foreach (PrefabInstance prefab in prefabs)
		{
			list.Add(prefab.GetSerializable());
		}
		return list;
	}

	public PrefabInstance TryPlaceAt(string prefabName, byte rotation, Vector3i position, bool yIsGroundLevel = true)
	{
		Prefab prefabRotated = prefabCache.GetPrefabRotated(prefabName, rotation);
		if (prefabRotated == null)
		{
			Log.Error("[EventPrefabs] cannot place " + prefabName + ", prefab not found");
			return null;
		}
		if (yIsGroundLevel)
		{
			position.y += prefabRotated.yOffset;
		}
		PrefabInstance prefabInstance = new PrefabInstance(dpd.GetNextId(), prefabRotated.location, position, rotation, prefabRotated, 0);
		HashSetLong occupiedChunks = prefabInstance.GetOccupiedChunks();
		if (!rfm.TryResetChunks(occupiedChunks, ChunkProtectionLevel.All, out var _protectionLevels))
		{
			Log.Error($"[EventPrefabs] cannot place {prefabName} at ({position}), {rotation}. Chunks could not be reset, protection level: {_protectionLevels}");
			return null;
		}
		DecoManager.Instance.ClearDecoObjectsInArea(prefabInstance.boundingBoxPosition, prefabInstance.boundingBoxSize);
		Log.Out($"[EventPrefabs] placing {prefabName} at ({position}), {rotation}");
		dpd.AddEventPrefab(prefabInstance);
		prefabs.Add(prefabInstance);
		rfm.AddGroupedChunks(occupiedChunks);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEventPrefab>().Setup(NetPackageEventPrefab.Operation.Add, prefabInstance));
		needsSaving = true;
		return prefabInstance;
	}

	public bool Remove(PrefabInstance pi)
	{
		HashSetLong occupiedChunks = pi.GetOccupiedChunks();
		if (!rfm.TryResetChunks(occupiedChunks, ChunkProtectionLevel.All, out var _protectionLevels))
		{
			Log.Error($"[EventPrefabs] cannot remove {pi}. Chunks could not be reset, protection level: {_protectionLevels}");
			return false;
		}
		if (!prefabs.Remove(pi))
		{
			Log.Error($"[EventPrefabs] cannot remove {pi}. Not an event prefab");
			return false;
		}
		world.RemoveTriggerVolumesFor(pi);
		world.RemoveSleeperVolumesFor(pi);
		world.RemoveWallVolumesFor(pi);
		dpd.RemoveEventPrefab(pi);
		rfm.RemoveGroupedChunks(occupiedChunks);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEventPrefab>().Setup(NetPackageEventPrefab.Operation.Remove, pi));
		needsSaving = true;
		return true;
	}

	public void Save(bool waitForComplete = false)
	{
		if (!needsSaving)
		{
			return;
		}
		needsSaving = false;
		PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
			pooledBinaryWriter.Write(1);
			pooledBinaryWriter.Write(prefabs.Count);
			foreach (PrefabInstance prefab in prefabs)
			{
				pooledBinaryWriter.Write(prefab.prefab.PrefabName);
				StreamUtils.Write(pooledBinaryWriter, prefab.boundingBoxPosition);
				pooledBinaryWriter.Write(prefab.rotation);
			}
		}
		pooledExpandableMemoryStream.Position = 0L;
		saveWriter.Write(pooledExpandableMemoryStream, waitForComplete);
	}

	public void Load()
	{
		string path = Path.Combine(GameIO.GetSaveGameDir(), "eventprefabs.dat");
		if (!SdFile.Exists(path))
		{
			return;
		}
		try
		{
			Log.Out("[EventPrefabs] loading prefab list");
			using Stream baseStream = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			pooledBinaryReader.ReadInt32();
			int num = pooledBinaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string name = pooledBinaryReader.ReadString();
				Vector3i position = StreamUtils.ReadVector3i(pooledBinaryReader);
				byte rotation = pooledBinaryReader.ReadByte();
				Prefab prefabRotated = prefabCache.GetPrefabRotated(name, rotation);
				PrefabInstance item = new PrefabInstance(dpd.GetNextId(), prefabRotated.location, position, rotation, prefabRotated, 0);
				prefabs.Add(item);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[EventPrefabs] error while reading save data " + ex.Message);
			Log.Exception(ex);
		}
		foreach (PrefabInstance prefab in prefabs)
		{
			DecoManager.Instance.ClearDecoObjectsInArea(prefab.boundingBoxPosition, prefab.boundingBoxSize);
			dpd.AddEventPrefab(prefab);
			rfm.AddGroupedChunks(prefab.GetOccupiedChunks());
		}
	}
}
