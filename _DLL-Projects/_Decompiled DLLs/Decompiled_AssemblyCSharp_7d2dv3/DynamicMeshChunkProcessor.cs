using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class DynamicMeshChunkProcessor
{
	public static DyMeshData RegionMeshData = DyMeshData.Create(1048000, 63000);

	public static bool DebugOnMainThread = false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string ThreadName;

	public string Error;

	public bool StopRequested;

	public DynamicMeshBuilderStatus Status;

	public DyMeshData ChunkData;

	public ExportMeshResult Result = ExportMeshResult.Missing;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Chunk chunk = new Chunk();

	public DynamicMeshItem Item;

	public DynamicMeshThread.ThreadRegion Region;

	public bool IsPrimaryQueue;

	[PublicizedFrom(EAccessModifier.Private)]
	public VoxelMeshLayer MeshLayer = MemoryPools.poolVML.AllocSync(_bReset: true);

	public int yOffset;

	public int EndY;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ChunkCacheNeighborChunks cacheNeighbourChunks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ChunkCacheNeighborBlocks cacheBlocks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Chunk[] neighbours = new Chunk[9];

	[PublicizedFrom(EAccessModifier.Protected)]
	public MeshGeneratorMC2 meshGen;

	public double MeshDataTime;

	public double ExportTime;

	public int MinTerrainHeight;

	[PublicizedFrom(EAccessModifier.Protected)]
	public DateTime LastActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkPreviewData PreviewData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[] FakeChunks = new Chunk[8]
	{
		new Chunk(),
		new Chunk(),
		new Chunk(),
		new Chunk(),
		new Chunk(),
		new Chunk(),
		new Chunk(),
		new Chunk()
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Thread thread;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> ChunkKeys = new List<long>(20);

	public static World world => GameManager.Instance.World;

	public double InactiveTime => (DateTime.Now - LastActive).TotalSeconds;

	public static bool IsServer => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

	public static bool IsChunkLoaded(Chunk c)
	{
		if (c == null || c.InProgressUnloading || c.InProgressSaving || c.InProgressDecorating || c.InProgressCopying || c.InProgressLighting || c.InProgressNetworking || c.InProgressRegeneration)
		{
			return false;
		}
		if (c == null || c.IsLocked || c.IsLockedExceptUnloading)
		{
			return false;
		}
		return true;
	}

	public int AddNewItem(DynamicMeshItem item, bool isPrimary)
	{
		if (Status != DynamicMeshBuilderStatus.Ready)
		{
			Log.Warning("Builder thread tried to start on item " + item.ToDebugLocation() + " when not ready. Current Status: " + Status);
			return 0;
		}
		if (GameManager.IsDedicatedServer)
		{
			DyMeshData fromCache = DyMeshData.GetFromCache();
			if (fromCache == null)
			{
				return -2;
			}
			ChunkData = fromCache;
		}
		Item = item;
		IsPrimaryQueue = isPrimary;
		LastActive = DateTime.Now;
		DynamicMeshThread.ChunkDataQueue.MarkAsUpdating(Item);
		Status = DynamicMeshBuilderStatus.StartingExport;
		return 1;
	}

	public int AddRegenerateRegion(DynamicMeshThread.ThreadRegion region)
	{
		if (Status != DynamicMeshBuilderStatus.Ready)
		{
			Log.Warning("Builder thread tried to start on item " + region.ToDebugLocation() + " when not ready. Current Status: " + Status);
			return 0;
		}
		DyMeshData fromCache = DyMeshData.GetFromCache();
		if (fromCache == null)
		{
			return -2;
		}
		ChunkData = fromCache;
		Region = region;
		IsPrimaryQueue = false;
		LastActive = DateTime.Now;
		Status = DynamicMeshBuilderStatus.StartingRegionRegen;
		return 1;
	}

	public int AddItemForMeshPreview(DynamicMeshItem item, ChunkPreviewData previewData)
	{
		if (Status != DynamicMeshBuilderStatus.Ready)
		{
			Log.Warning("Builder thread tried to start preview when not ready. Current Status: " + Status);
			return 0;
		}
		if (Item != null)
		{
			Log.Error("Item was not null on chunk mesh thread: " + Item.ToDebugLocation());
		}
		DyMeshData fromCache = DyMeshData.GetFromCache();
		if (fromCache == null)
		{
			return -2;
		}
		ChunkData = fromCache;
		Item = item;
		PreviewData = previewData;
		IsPrimaryQueue = false;
		LastActive = DateTime.Now;
		Status = DynamicMeshBuilderStatus.StartingPreview;
		return 1;
	}

	public int AddItemForMeshGeneration(DynamicMeshItem item, bool isPrimary)
	{
		if (Status != DynamicMeshBuilderStatus.Ready)
		{
			Log.Warning("Builder thread tried to start on item " + item.ToDebugLocation() + " when not ready. Current Status: " + Status);
			return 0;
		}
		if (Item != null)
		{
			Log.Error("Item was not null on chunk mesh thread: " + Item.ToDebugLocation());
		}
		DyMeshData fromCache = DyMeshData.GetFromCache();
		if (fromCache == null)
		{
			return -2;
		}
		ChunkData = fromCache;
		Item = item;
		IsPrimaryQueue = isPrimary;
		LastActive = DateTime.Now;
		DynamicMeshThread.ChunkDataQueue.MarkAsGenerating(Item);
		Status = DynamicMeshBuilderStatus.StartingGeneration;
		return 1;
	}

	public void CleanUp()
	{
		MeshLayer.Cleanup();
		MeshLayer = null;
	}

	public void RequestStop(bool forceStop = false)
	{
		StopRequested = true;
		if (forceStop)
		{
			try
			{
				thread?.Abort();
			}
			catch (Exception)
			{
			}
		}
		Status = DynamicMeshBuilderStatus.Stopped;
	}

	public void StartThread()
	{
		if (DebugOnMainThread)
		{
			return;
		}
		thread = new Thread([PublicizedFrom(EAccessModifier.Private)] () =>
		{
			try
			{
				while (!(GameManager.Instance == null) && GameManager.Instance.World != null)
				{
					if ((Item == null && Region == null) || Status == DynamicMeshBuilderStatus.Ready || Status == DynamicMeshBuilderStatus.Complete)
					{
						if (StopRequested)
						{
							break;
						}
						Thread.Sleep(100);
					}
					else
					{
						RunJob();
					}
				}
			}
			catch (Exception ex)
			{
				Error = "Builder error: " + ex.Message + "\n" + ex.StackTrace;
				if (!StopRequested)
				{
					Log.Error(Error);
				}
			}
			Status = DynamicMeshBuilderStatus.Stopped;
		});
		thread.Name = ThreadName;
		thread.Priority = System.Threading.ThreadPriority.Lowest;
		thread.Start();
	}

	public void RunJob()
	{
		if (Status != DynamicMeshBuilderStatus.Ready && Status != DynamicMeshBuilderStatus.Complete && Status != DynamicMeshBuilderStatus.PreviewComplete && Status != DynamicMeshBuilderStatus.Error)
		{
			if (Status != DynamicMeshBuilderStatus.StartingExport && Status != DynamicMeshBuilderStatus.StartingGeneration && Status != DynamicMeshBuilderStatus.StartingPreview && Status != DynamicMeshBuilderStatus.StartingRegionRegen)
			{
				Status = DynamicMeshBuilderStatus.Error;
			}
			else if (Status == DynamicMeshBuilderStatus.StartingExport)
			{
				ExportChunk(Item);
			}
			else if (Status == DynamicMeshBuilderStatus.StartingGeneration)
			{
				CreateMesh(Item);
			}
			else if (Status == DynamicMeshBuilderStatus.StartingPreview)
			{
				CreatePreviewMeshJob();
			}
			else if (Status == DynamicMeshBuilderStatus.StartingRegionRegen)
			{
				RegenerateRegion(Region);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ExportMeshResult SetResultPreview(ExportMeshResult result)
	{
		Result = result;
		ChunkData = DyMeshData.AddToCache(ChunkData);
		Status = DynamicMeshBuilderStatus.PreviewComplete;
		LastActive = DateTime.Now;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ExportMeshResult SetResult(ExportMeshResult result)
	{
		Result = result;
		Status = DynamicMeshBuilderStatus.Complete;
		LastActive = DateTime.Now;
		if (Item != null)
		{
			DynamicMeshThread.ChunkDataQueue.MarkAsUpdated(Item);
			DynamicMeshThread.ChunkDataQueue.MarkAsGenerated(Item);
		}
		return result;
	}

	public void Init(int id)
	{
		LastActive = DateTime.Now;
		cacheNeighbourChunks = new ChunkCacheNeighborChunks(GameManager.Instance.World.ChunkCache);
		cacheBlocks = new ChunkCacheNeighborBlocks(cacheNeighbourChunks);
		meshGen = new MeshGeneratorMC2(cacheBlocks, cacheNeighbourChunks);
		ThreadName = "DymeshThread" + id;
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 256; k++)
				{
					Chunk[] fakeChunks = FakeChunks;
					for (int l = 0; l < fakeChunks.Length; l++)
					{
						fakeChunks[l].SetLight(i, k, j, 15, Chunk.LIGHT_TYPE.SUN);
					}
				}
			}
		}
	}

	public ExportMeshResult CreatePreviewMeshJob()
	{
		ExportMeshResult resultPreview = CreatePreviewMesh();
		return SetResultPreview(resultPreview);
	}

	public ExportMeshResult CreateMesh(DynamicMeshItem item)
	{
		ExportMeshResult result = CreateMesh(item.Key, isRegionRegen: false);
		return SetResult(result);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderChunkMeshToData(Vector3i worldPosition, Chunk chunk, DyMeshData data, bool fullHeight)
	{
		chunk.NeedsRegeneration = true;
		bool flag = false;
		for (int i = 0; i < 16; i++)
		{
			if ((chunk.NeedsRegenerationAt & (1 << i)) != 0)
			{
				if (meshGen.IsLayerEmpty(i))
				{
					chunk.ClearNeedsRegenerationAt(i);
					MeshLayer.idx = -1;
				}
				else
				{
					MeshLayer.idx = i;
				}
			}
			if (MeshLayer.idx != -1)
			{
				chunk.ClearNeedsRegenerationAt(MeshLayer.idx);
				meshGen.GenerateMesh(worldPosition, MeshLayer.idx, MeshLayer.meshes);
				VoxelMesh voxelMesh = MeshLayer.meshes[0];
				if (voxelMesh.Vertices.Count != 0)
				{
					flag = true;
					LoadChunkIntoRegion(worldPosition.x, worldPosition.z, worldPosition, null, voxelMesh, ChunkData.OpaqueMesh, ChunkData.TerrainMesh, 0);
				}
				VoxelMesh voxelMesh2 = MeshLayer.meshes[5];
				if (fullHeight || (flag && voxelMesh2.Vertices.Count != 0 && MeshLayer.idx * 16 + 16 >= MinTerrainHeight))
				{
					LoadChunkIntoRegion(worldPosition.x, worldPosition.z, worldPosition, null, voxelMesh2, ChunkData.OpaqueMesh, ChunkData.TerrainMesh, 0);
				}
				DyMeshData.ResetMeshLayer(MeshLayer);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ExportMeshResult CreateMesh(long key, bool isRegionRegen)
	{
		Vector3i worldPosFromKey = DynamicMeshUnity.GetWorldPosFromKey(key);
		if (DynamicMeshThread.NoProcessing)
		{
			return ExportMeshResult.Success;
		}
		DateTime now = DateTime.Now;
		int num = 0;
		DynamicMeshChunkDataWrapper wrapper;
		string debugMessage;
		while (!DynamicMeshThread.ChunkDataQueue.CollectItem(key, out wrapper, out debugMessage) && num++ <= 5)
		{
			Log.Out(DynamicMeshUnity.GetWorldPosFromKey(key).ToString() + " Waiting for chunk: " + debugMessage);
			Thread.Sleep(100);
		}
		DynamicMeshChunkData data = wrapper.Data;
		if (data == null)
		{
			DynamicMeshThread.ChunkDataQueue.ClearLock(wrapper, "_CREATEDATAMISSING_");
			if (!isRegionRegen)
			{
				ChunkData = DyMeshData.AddToCache(ChunkData);
			}
			return ExportMeshResult.ChunkMissing;
		}
		ChunkCacheNeighborChunks chunkCacheNeighborChunks = cacheNeighbourChunks;
		Chunk obj = chunk;
		IChunk[] fakeChunks = FakeChunks;
		chunkCacheNeighborChunks.Init(obj, fakeChunks);
		data.ApplyToChunk(chunk, cacheNeighbourChunks);
		_ = data.MainBiome;
		MinTerrainHeight = data.MinTerrainHeight;
		yOffset = data.OffsetY;
		EndY = data.EndY;
		DynamicMeshThread.ChunkDataQueue.ClearLock(wrapper, "_CREATEMESHRELEASE_");
		RenderChunkMeshToData(worldPosFromKey, chunk, ChunkData, fullHeight: false);
		cacheNeighbourChunks.Clear();
		if (ChunkData.OpaqueMesh.Vertices.Count == 0 && !isRegionRegen)
		{
			string itemPath = DynamicMeshUnity.GetItemPath(Item.Key);
			DynamicMeshThread.ChunkDataQueue.MarkForDeletion(key);
			num = 0;
			while (!DynamicMeshThread.ChunkDataQueue.CollectItem(key, out wrapper, out debugMessage))
			{
				if (num++ > 5)
				{
					Log.Warning("Failed to take lock for chunk deletion");
					break;
				}
				Log.Out(DynamicMeshUnity.GetWorldPosFromKey(key).ToString() + " Waiting for chunk deletion: " + debugMessage);
				Thread.Sleep(100);
			}
			if (SdFile.Exists(itemPath))
			{
				SdFile.Delete(itemPath);
			}
			DynamicMeshThread.ChunkDataQueue.ClearLock(wrapper, "_CreateMeshDeleteFile");
			Item.DestroyMesh();
			if (IsServer)
			{
				DynamicMeshServer.SendToAllClients(Item, isDelete: true);
			}
			DynamicMeshManager.Instance.ArrangeChunkRemoval(worldPosFromKey.x, worldPosFromKey.z);
			DynamicMeshThread.RemoveRegionChunk(worldPosFromKey.x, worldPosFromKey.z, Item.Key);
			ChunkData = DyMeshData.AddToCache(ChunkData);
			return ExportMeshResult.SuccessNoLoad;
		}
		if (GameManager.IsDedicatedServer)
		{
			return ExportMeshResult.Success;
		}
		if (ChunkData.OpaqueMesh.Vertices.Count > 0 && ChunkData.OpaqueMesh.Normals.Count == 0)
		{
			MeshCalculations.RecalculateNormals(ChunkData.OpaqueMesh.Vertices, ChunkData.OpaqueMesh.Indices, ChunkData.OpaqueMesh.Normals);
		}
		if (ChunkData.OpaqueMesh.Vertices.Count > 0 && ChunkData.OpaqueMesh.Tangents.Count == 0)
		{
			MeshCalculations.CalculateMeshTangents(ChunkData.OpaqueMesh.Vertices, ChunkData.OpaqueMesh.Indices, ChunkData.OpaqueMesh.Normals, ChunkData.OpaqueMesh.Uvs, ChunkData.OpaqueMesh.Tangents);
		}
		if (ChunkData.TerrainMesh.Vertices.Count > 0)
		{
			_ = ChunkData.TerrainMesh.Normals.Count;
		}
		ExportTime = (DateTime.Now - now).TotalMilliseconds;
		if (!isRegionRegen)
		{
			DynamicMeshVoxelLoad loadData = DynamicMeshVoxelLoad.Create(Item, ChunkData);
			ChunkData = null;
			DynamicMeshManager.Instance.AddChunkLoadData(loadData);
		}
		return ExportMeshResult.Success;
	}

	public ExportMeshResult CreatePreviewMesh()
	{
		World obj = GameManager.Instance.World;
		_ = DateTime.Now;
		Vector3i worldPosition = Item.WorldPosition;
		Chunk chunk = (Chunk)obj.GetChunkFromWorldPos(worldPosition);
		if (chunk == null)
		{
			return ExportMeshResult.PreviewMissing;
		}
		if (!GameManager.Instance.World.ChunkCache.GetNeighborChunks(chunk, neighbours))
		{
			DynamicMeshThread.SetNextChunks(chunk.Key);
			return ExportMeshResult.PreviewDelay;
		}
		this.chunk.X = chunk.X;
		this.chunk.Z = chunk.Z;
		for (int i = 0; i < FakeChunks.Length; i++)
		{
			CopyPreviewChunk(neighbours[i], FakeChunks[i], isNeighbour: true);
		}
		ChunkCacheNeighborChunks chunkCacheNeighborChunks = cacheNeighbourChunks;
		IChunk[] fakeChunks = FakeChunks;
		chunkCacheNeighborChunks.Init(chunk, fakeChunks);
		Chunk chunk2 = CopyPreviewChunk(chunk, this.chunk, isNeighbour: false);
		ChunkCacheNeighborChunks chunkCacheNeighborChunks2 = cacheNeighbourChunks;
		fakeChunks = FakeChunks;
		chunkCacheNeighborChunks2.Init(chunk2, fakeChunks);
		for (int j = -1; j < 2; j++)
		{
			for (int k = -1; k < 2; k++)
			{
				if (cacheNeighbourChunks[j, k] == null)
				{
					if (!IsChunkLoaded((Chunk)cacheNeighbourChunks[j, k]))
					{
						return ExportMeshResult.PreviewDelay;
					}
					return ExportMeshResult.PreviewMissing;
				}
				if (!IsChunkLoaded((Chunk)cacheNeighbourChunks[j, k]))
				{
					return ExportMeshResult.PreviewDelay;
				}
			}
		}
		RenderChunkMeshToData(worldPosition, this.chunk, ChunkData, fullHeight: true);
		if (ChunkData.OpaqueMesh.Vertices.Count > 0 && ChunkData.OpaqueMesh.Normals.Count == 0)
		{
			MeshCalculations.RecalculateNormals(ChunkData.OpaqueMesh.Vertices, ChunkData.OpaqueMesh.Indices, ChunkData.OpaqueMesh.Normals);
		}
		if (ChunkData.OpaqueMesh.Vertices.Count > 0 && ChunkData.OpaqueMesh.Tangents.Count == 0)
		{
			MeshCalculations.CalculateMeshTangents(ChunkData.OpaqueMesh.Vertices, ChunkData.OpaqueMesh.Indices, ChunkData.OpaqueMesh.Normals, ChunkData.OpaqueMesh.Uvs, ChunkData.OpaqueMesh.Tangents);
		}
		if (ChunkData.TerrainMesh.Vertices.Count > 0)
		{
			_ = ChunkData.TerrainMesh.Normals.Count;
		}
		DynamicMeshVoxelLoad loadData = DynamicMeshVoxelLoad.Create(Item, ChunkData);
		ChunkData = null;
		ChunkPreviewManager.Instance?.AddChunkPreviewLoadData(loadData);
		return ExportMeshResult.PreviewSuccess;
	}

	public ExportMeshResult ExportChunk(DynamicMeshItem item)
	{
		World obj = GameManager.Instance.World;
		DateTime now = DateTime.Now;
		Vector3i worldPosition = item.WorldPosition;
		Chunk chunk = (Chunk)obj.GetChunkFromWorldPos(worldPosition);
		if (chunk == null)
		{
			return SetResult(ExportMeshResult.Missing);
		}
		if (!GameManager.Instance.World.ChunkCache.GetNeighborChunks(chunk, neighbours))
		{
			DynamicMeshThread.SetNextChunks(chunk.Key);
			return SetResult(ExportMeshResult.Delay);
		}
		ChunkCacheNeighborChunks chunkCacheNeighborChunks = cacheNeighbourChunks;
		IChunk[] chunkArr = neighbours;
		chunkCacheNeighborChunks.Init(chunk, chunkArr);
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (cacheNeighbourChunks[i, j] == null)
				{
					if (!IsChunkLoaded((Chunk)cacheNeighbourChunks[i, j]))
					{
						return SetResult(ExportMeshResult.Delay);
					}
					return SetResult(ExportMeshResult.Missing);
				}
				if (!IsChunkLoaded((Chunk)cacheNeighbourChunks[i, j]))
				{
					return SetResult(ExportMeshResult.Delay);
				}
			}
		}
		DynamicMeshManager.ThreadDistance = item.DistanceToPlayer();
		DynamicMeshChunkData fromCache = DynamicMeshChunkData.GetFromCache("_EXPORT_");
		bool num = CopyChunkFromWorld(chunk, fromCache, fullCopy: false);
		MeshDataTime = (DateTime.Now - now).TotalMilliseconds;
		if (!num || fromCache.BlockRaw.Count == 0 || fromCache.Height.Count == 0)
		{
			string path = DynamicMeshFile.MeshLocation + item.Key + ".update";
			DynamicMeshThread.ChunkDataQueue.MarkForDeletion(item.Key);
			if (SdFile.Exists(path))
			{
				SdFile.Delete(path);
			}
			item.DestroyMesh();
			if (IsServer)
			{
				DynamicMeshServer.SendToAllClients(item, isDelete: true);
			}
			DynamicMeshManager.Instance.ArrangeChunkRemoval(item.WorldPosition.x, item.WorldPosition.z);
			DynamicMeshChunkData.AddToCache(fromCache, "_noDataRelease_");
			return SetResult(ExportMeshResult.SuccessNoLoad);
		}
		DynamicMeshThread.ChunkDataQueue.AddSaveRequest(item.Key, fromCache);
		DynamicMeshThread.AddRegionUpdateData(item.WorldPosition.x, item.WorldPosition.z, isUrgent: false);
		ExportMeshResult exportMeshResult = (GameManager.IsDedicatedServer ? CreateMesh(item) : ExportMeshResult.Success);
		if (IsServer && exportMeshResult == ExportMeshResult.Success)
		{
			DynamicMeshServer.SendToAllClients(item, isDelete: false);
		}
		return SetResult(ExportMeshResult.Success);
	}

	public Chunk CopyPreviewChunk(Chunk chunkInWorld, Chunk previewChunk, bool isNeighbour)
	{
		Vector3i worldPos = chunkInWorld.GetWorldPos();
		Vector3i worldPosition = PreviewData.WorldPosition;
		Prefab prefabData = PreviewData.PrefabData;
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					if (!isNeighbour || j == 0 || j == 1 || j == 14 || j == 15 || k == 0 || k == 1 || k == 14 || k == 15)
					{
						int num = worldPos.x + j - worldPosition.x;
						int num2 = i - worldPosition.y;
						int num3 = worldPos.z + k - worldPosition.z;
						BlockValue block;
						WaterValue water;
						sbyte density;
						TextureFullArray texturefullArray;
						if (num >= 0 && num2 >= 0 && num3 >= 0 && num < prefabData.size.x && num3 < prefabData.size.z && num2 < prefabData.size.y)
						{
							block = prefabData.GetBlock(num, num2, num3);
							water = prefabData.GetWater(num, num2, num3);
							density = prefabData.GetDensity(num, num2, num3);
							texturefullArray = prefabData.GetTexture(num, num2, num3);
						}
						else
						{
							block = chunkInWorld.GetBlock(j, i, k);
							water = chunkInWorld.GetWater(j, i, k);
							density = chunkInWorld.GetDensity(j, i, k);
							texturefullArray = chunkInWorld.GetTextureFullArray(j, i, k);
						}
						previewChunk.SetBlockRaw(j, i, k, block);
						previewChunk.SetWater(j, i, k, water);
						previewChunk.SetDensity(j, i, k, density);
						previewChunk.GetSetTextureFullArray(j, i, k, texturefullArray);
					}
				}
			}
		}
		return previewChunk;
	}

	public bool CopyChunkFromWorld(Chunk chunkInWorld, DynamicMeshChunkData data, bool fullCopy)
	{
		data.Reset();
		bool flag = false;
		bool flag2 = false;
		MinTerrainHeight = 500;
		data.X = chunkInWorld.X * 16;
		data.Z = chunkInWorld.Z * 16;
		data.MainBiome = chunkInWorld.DominantBiome;
		data.SetTopSoil(chunkInWorld.GetTopSoil());
		for (int i = 0; i < 256; i++)
		{
			data.RecordCounts();
			bool flag3 = false;
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					if (i == 0)
					{
						byte terrainHeight = chunkInWorld.GetTerrainHeight(j, k);
						byte height = chunkInWorld.GetHeight(j, k);
						byte b = Math.Min(terrainHeight, height);
						data.TerrainHeight.Add(b);
						data.Height.Add(b);
						MinTerrainHeight = (data.MinTerrainHeight = Math.Min(MinTerrainHeight, b));
					}
					bool flag4 = i > MinTerrainHeight;
					switch (j)
					{
					case 0:
					{
						DynamicMeshChunkData.ChunkNeighbourData neighbourData4 = data.GetNeighbourData(-1, 0);
						IChunk chunk2 = cacheNeighbourChunks[-1, 0];
						BlockValue blockValue2 = (flag4 ? chunk2.GetBlock(15, i, k) : BlockValue.Air);
						neighbourData4.SetData(density: (sbyte)(flag4 ? chunk2.GetDensity(15, i, k) : 0), texture: flag4 ? chunk2.GetTextureFull(15, i, k) : 0, blockraw: blockValue2.rawData);
						if (k == 0)
						{
							DynamicMeshChunkData.ChunkNeighbourData neighbourData5 = data.GetNeighbourData(-1, -1);
							IChunk obj3 = cacheNeighbourChunks[-1, -1];
							BlockValue block3 = obj3.GetBlock(15, i, 15);
							neighbourData5.SetData(density: obj3.GetDensity(15, i, 15), texture: obj3.GetTextureFull(15, i, 15), blockraw: block3.rawData);
						}
						if (k == 15)
						{
							DynamicMeshChunkData.ChunkNeighbourData neighbourData6 = data.GetNeighbourData(-1, 1);
							IChunk obj4 = cacheNeighbourChunks[-1, 1];
							BlockValue block4 = obj4.GetBlock(15, i, 0);
							neighbourData6.SetData(density: obj4.GetDensity(15, i, 0), texture: obj4.GetTextureFull(15, i, 0), blockraw: block4.rawData);
						}
						break;
					}
					case 15:
					{
						DynamicMeshChunkData.ChunkNeighbourData neighbourData = data.GetNeighbourData(1, 0);
						IChunk chunk = cacheNeighbourChunks[1, 0];
						BlockValue blockValue = (flag4 ? chunk.GetBlock(0, i, k) : BlockValue.Air);
						neighbourData.SetData(density: (sbyte)(flag4 ? chunk.GetDensity(0, i, k) : 0), texture: flag4 ? chunk.GetTextureFull(0, i, k) : 0, blockraw: blockValue.rawData);
						if (k == 0)
						{
							DynamicMeshChunkData.ChunkNeighbourData neighbourData2 = data.GetNeighbourData(1, -1);
							IChunk obj = cacheNeighbourChunks[1, -1];
							BlockValue block = obj.GetBlock(0, i, 0);
							neighbourData2.SetData(density: obj.GetDensity(0, i, 0), texture: obj.GetTextureFull(0, i, 0), blockraw: block.rawData);
						}
						if (k == 15)
						{
							DynamicMeshChunkData.ChunkNeighbourData neighbourData3 = data.GetNeighbourData(1, 1);
							IChunk obj2 = cacheNeighbourChunks[1, 1];
							BlockValue block2 = obj2.GetBlock(0, i, 0);
							neighbourData3.SetData(density: obj2.GetDensity(0, i, 0), texture: obj2.GetTextureFull(0, i, 0), blockraw: block2.rawData);
						}
						break;
					}
					}
					switch (k)
					{
					case 0:
					{
						DynamicMeshChunkData.ChunkNeighbourData neighbourData8 = data.GetNeighbourData(0, -1);
						IChunk chunk4 = cacheNeighbourChunks[0, -1];
						BlockValue blockValue4 = (flag4 ? chunk4.GetBlock(j, i, 15) : BlockValue.Air);
						neighbourData8.SetData(density: (sbyte)(flag4 ? chunk4.GetDensity(j, i, 15) : 0), texture: flag4 ? chunk4.GetTextureFull(j, i, 15) : 0, blockraw: blockValue4.rawData);
						break;
					}
					case 15:
					{
						DynamicMeshChunkData.ChunkNeighbourData neighbourData7 = data.GetNeighbourData(0, 1);
						IChunk chunk3 = cacheNeighbourChunks[0, 1];
						BlockValue blockValue3 = (flag4 ? chunk3.GetBlock(j, i, 0) : BlockValue.Air);
						neighbourData7.SetData(density: (sbyte)(flag4 ? chunk3.GetDensity(j, i, 0) : 0), texture: flag4 ? chunk3.GetTextureFull(j, i, 0) : 0, blockraw: blockValue3.rawData);
						break;
					}
					}
					BlockValue block5 = chunkInWorld.GetBlock(j, i, k);
					Block block6 = Block.list[block5.type];
					bool flag5 = fullCopy || DynamicMeshBlockSwap.OpaqueBlocks.Contains(block5.type);
					bool flag6 = DynamicMeshBlockSwap.TerrainBlocks.Contains(block5.type);
					flag2 = flag2 || flag5;
					bool flag7 = DynamicMeshBlockSwap.DoorBlocks.Contains(block5.type);
					bool flag8 = flag5 || flag7 || (flag2 && flag6);
					if (flag8)
					{
						EndY = i + 1;
						if (block5.type == 0)
						{
							data.BlockRaw.Add(0u);
						}
						else if (flag7)
						{
							data.BlockRaw.Add(DynamicMeshBlockSwap.DoorReplacement.rawData);
							data.Densities.Add(MarchingCubes.DensityAir);
							data.Textures.Add(0L);
						}
						else
						{
							long item = (block6.shape.IsTerrain() ? 0 : chunkInWorld.GetTextureFull(j, i, k));
							sbyte item2 = (sbyte)(block6.shape.IsTerrain() ? chunkInWorld.GetDensity(j, i, k) : 0);
							if (block5.type == 0)
							{
								data.BlockRaw.Add(0u);
							}
							else
							{
								data.BlockRaw.Add(block5.rawData);
								data.Densities.Add(item2);
								data.Textures.Add(item);
							}
						}
						if (!flag && flag5)
						{
							yOffset = i;
							flag = true;
						}
					}
					else
					{
						data.BlockRaw.Add(0u);
					}
					flag3 = flag3 || flag8;
				}
			}
			if (!flag2)
			{
				data.ClearPreviousLayers();
			}
		}
		data.OffsetY = (yOffset = Math.Max(0, yOffset));
		data.EndY = EndY;
		return flag2;
	}

	public ExportMeshResult RegenerateRegion(DynamicMeshThread.ThreadRegion region)
	{
		DateTime now = DateTime.Now;
		Vector3i worldPosition = region.ToWorldPosition();
		if (RegionMeshData.OpaqueMesh.Vertices.Count > 0)
		{
			Log.Warning("Region object was already in use. Only one region can be regenerated at a time");
		}
		DyMeshData regionMeshData = RegionMeshData;
		region.CopyLoadedChunks(ChunkKeys);
		foreach (long chunkKey in ChunkKeys)
		{
			Vector3i worldPosFromKey = DynamicMeshUnity.GetWorldPosFromKey(chunkKey);
			ChunkData.Reset();
			if (CreateMesh(chunkKey, isRegionRegen: true) == ExportMeshResult.Success)
			{
				LoadChunkIntoRegion(region.X, region.Z, worldPosFromKey, region, ChunkData.OpaqueMesh, regionMeshData.OpaqueMesh, regionMeshData.TerrainMesh, 128);
				LoadChunkIntoRegion(region.X, region.Z, worldPosFromKey, region, ChunkData.TerrainMesh, regionMeshData.OpaqueMesh, regionMeshData.TerrainMesh, 128);
			}
		}
		MeshCalculations.RecalculateNormals(regionMeshData.TerrainMesh.Vertices, regionMeshData.TerrainMesh.Indices, regionMeshData.TerrainMesh.Normals);
		DynamicMeshThread.RegionStorage.SaveRegion(region, worldPosition, regionMeshData.OpaqueMesh, regionMeshData.TerrainMesh);
		DynamicMeshManager.AddRegionLoadMeshes(region.Key);
		regionMeshData.Reset();
		ChunkData = DyMeshData.AddToCache(ChunkData);
		DynamicMeshThread.RegionUpdatesDebug = region.ToDebugLocation() + " with " + region.LoadedChunkCount + " took " + (int)(DateTime.Now - now).TotalMilliseconds + "ms";
		return SetResult(ExportMeshResult.Success);
	}

	public static void LoadChunkIntoRegion(int regionX, int regionZ, Vector3i chunkPos, DynamicMeshThread.ThreadRegion region, VoxelMesh voxelMeshToCopyFrom, VoxelMesh opaqueMesh, VoxelMeshTerrain terrainMesh, byte biomeId)
	{
		if (voxelMeshToCopyFrom == null || voxelMeshToCopyFrom.Vertices.Count == 0)
		{
			return;
		}
		int num = chunkPos.x - regionX;
		int num2 = 0;
		int num3 = chunkPos.z - regionZ;
		Vector3 vector = new Vector3(num, num2, num3);
		if (voxelMeshToCopyFrom is VoxelMeshTerrain)
		{
			VoxelMeshTerrain voxelMeshTerrain = voxelMeshToCopyFrom as VoxelMeshTerrain;
			int count = terrainMesh.Vertices.Count;
			terrainMesh.Vertices.Grow(count + voxelMeshTerrain.Vertices.Count);
			for (int i = 0; i < voxelMeshTerrain.Vertices.Count; i++)
			{
				terrainMesh.Vertices.Add(voxelMeshTerrain.Vertices[i] + vector);
			}
			terrainMesh.Uvs.AddRange(voxelMeshTerrain.Uvs.Items, 0, voxelMeshTerrain.Uvs.Count);
			terrainMesh.UvsCrack.AddRange(voxelMeshTerrain.UvsCrack.Items, 0, voxelMeshTerrain.UvsCrack.Count);
			terrainMesh.Uvs3.AddRange(voxelMeshTerrain.Uvs3.Items, 0, voxelMeshTerrain.Uvs3.Count);
			terrainMesh.Uvs4.AddRange(voxelMeshTerrain.Uvs4.Items, 0, voxelMeshTerrain.Uvs4.Count);
			terrainMesh.ColorVertices.AddRange(voxelMeshTerrain.ColorVertices.Items, 0, voxelMeshTerrain.ColorVertices.Count);
			terrainMesh.Normals.AddRange(voxelMeshTerrain.Normals.Items, 0, voxelMeshTerrain.Normals.Count);
			terrainMesh.Tangents.AddRange(voxelMeshTerrain.Tangents.Items, 0, voxelMeshTerrain.Tangents.Count);
			if (voxelMeshTerrain.Indices.Count <= 0)
			{
				terrainMesh.Indices.Grow(terrainMesh.Indices.Count + voxelMeshTerrain.submeshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (TerrainSubMesh d) => d.triangles.Count));
				{
					foreach (TerrainSubMesh submesh in voxelMeshTerrain.submeshes)
					{
						for (int num4 = 0; num4 < submesh.triangles.Count; num4++)
						{
							terrainMesh.Indices.Add(submesh.triangles[num4] + count);
						}
					}
					return;
				}
			}
			terrainMesh.Indices.Grow(terrainMesh.Indices.Count + voxelMeshTerrain.Indices.Count);
			for (int num5 = 0; num5 < voxelMeshTerrain.Indices.Count; num5++)
			{
				terrainMesh.Indices.Add(voxelMeshTerrain.Indices[num5] + count);
			}
		}
		else
		{
			int count2 = opaqueMesh.Vertices.Count;
			opaqueMesh.Vertices.Grow(count2 + voxelMeshToCopyFrom.Vertices.Count);
			for (int num6 = 0; num6 < voxelMeshToCopyFrom.Vertices.Count; num6++)
			{
				opaqueMesh.Vertices.Add(voxelMeshToCopyFrom.Vertices[num6] + vector);
			}
			opaqueMesh.Uvs.AddRange(voxelMeshToCopyFrom.Uvs.Items, 0, voxelMeshToCopyFrom.Uvs.Count);
			opaqueMesh.UvsCrack.AddRange(voxelMeshToCopyFrom.UvsCrack.Items, 0, voxelMeshToCopyFrom.UvsCrack.Count);
			opaqueMesh.ColorVertices.AddRange(voxelMeshToCopyFrom.ColorVertices.Items, 0, voxelMeshToCopyFrom.ColorVertices.Count);
			opaqueMesh.Normals.AddRange(voxelMeshToCopyFrom.Normals.Items, 0, voxelMeshToCopyFrom.Normals.Count);
			opaqueMesh.Tangents.AddRange(voxelMeshToCopyFrom.Tangents.Items, 0, voxelMeshToCopyFrom.Tangents.Count);
			opaqueMesh.Indices.Grow(opaqueMesh.Indices.Count + voxelMeshToCopyFrom.Indices.Count);
			for (int num7 = 0; num7 < voxelMeshToCopyFrom.Indices.Count; num7++)
			{
				opaqueMesh.Indices.Add(voxelMeshToCopyFrom.Indices[num7] + count2);
			}
		}
	}

	public void ResetAfterJob()
	{
		Item = null;
		Region = null;
		Status = DynamicMeshBuilderStatus.Ready;
	}
}
