using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCluster : WorldChunkCache, IChunkAccess
{
	public delegate void OnBlockDamagedDelegate(Vector3i _blockPos, BlockValue _blockValue, int _damage, int _attackerEntityId);

	public delegate void OnChunksFinishedLoadingDelegate();

	public delegate void OnChunksFinishedDisplayingDelegate();

	public delegate void OnChunkVisibleDelegate(long _key, bool _isDisplayed);

	public delegate void OnBlockChangedDelegate(Vector3i pos, BlockValue bvOld, sbyte densOld, TextureFullArray texOld, BlockValue bvNew);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFinishedLoadingDelegateCalled;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkKeysNeedLoading;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFinishedDisplayingDelegateCalled;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkKeysNeedDisplaying;

	public string Name;

	public bool IsFixedSize;

	public readonly int ClusterIdx;

	public int LayerMappingId;

	public Dictionary<string, int> LayerMappingTable;

	public DictionarySave<long, ChunkGameObject> DisplayedChunkGameObjects = new DictionarySave<long, ChunkGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentDictionary<long, (int, DateTime)> chunkRegenerationStartTimestamps = new ConcurrentDictionary<long, (int, DateTime)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentDictionary<long, (int, DateTime)> chunkRegenerationEndTimestamps = new ConcurrentDictionary<long, (int, DateTime)>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static long currentChunkKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int currentChunkVMLIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string currentChunkRegenState;

	public Vector3 Position;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILightProcessor m_LightProcessorMainThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILightProcessor m_LightProcessorLightingThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public StabilityCalculator stabilityCalcMainThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public StabilityInitializer stabilityCalcLightingThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCacheNeighborChunks nChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCacheNeighborBlocks nBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshGenerator meshGenerator;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	public IChunkProvider ChunkProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SBlockPosValue> multiBlockList = new List<SBlockPosValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInNotify;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunksNeedingRegThisCall = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Chunk, int> delayedRegenChunks = new Dictionary<Chunk, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int delayedRegenCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Bounds> aabbList = new List<Bounds>();

	public event OnBlockDamagedDelegate OnBlockDamagedDelegates;

	public event OnChunksFinishedLoadingDelegate OnChunksFinishedLoadingDelegates;

	public event OnChunksFinishedDisplayingDelegate OnChunksFinishedDisplayingDelegates;

	public event OnChunkVisibleDelegate OnChunkVisibleDelegates;

	public event OnBlockChangedDelegate OnBlockChangedDelegates;

	public ChunkCluster(World _world, string _name, Dictionary<string, int> _layerMappingTable)
	{
		Name = _name;
		world = _world;
		LayerMappingTable = _layerMappingTable;
	}

	public IEnumerator Init(EnumChunkProviderId _providerId)
	{
		nChunks = new ChunkCacheNeighborChunks(this);
		nBlocks = new ChunkCacheNeighborBlocks(nChunks);
		meshGenerator = new MeshGeneratorMC2(nBlocks, nChunks);
		stabilityCalcMainThread = new StabilityCalculator();
		stabilityCalcLightingThread = new StabilityInitializer(world);
		m_LightProcessorMainThread = new LightProcessor(this);
		m_LightProcessorLightingThread = new LightProcessor(this);
		if (world.GetGameManager() != null)
		{
			stabilityCalcMainThread.Init(world);
		}
		ChunkProvider = null;
		switch (_providerId)
		{
		case EnumChunkProviderId.Disc:
			ChunkProvider = new ChunkProviderDisc(this, Name);
			break;
		case EnumChunkProviderId.NetworkClient:
			if (!IsFixedSize)
			{
				ChunkProvider = new ChunkProviderGenerateWorldFromRaw(this, Name, _bClientMode: true, _bFixedWaterLevel: true);
			}
			else
			{
				ChunkProvider = new ChunkProviderDummy();
			}
			break;
		case EnumChunkProviderId.GenerateFromDtm:
			ChunkProvider = new ChunkProviderGenerateWorldFromImage(this, Name);
			break;
		case EnumChunkProviderId.ChunkDataDriven:
			ChunkProvider = new ChunkProviderGenerateWorldFromRaw(this, Name);
			break;
		case EnumChunkProviderId.FlatWorld:
			ChunkProvider = new ChunkProviderGenerateFlat(this, Name);
			break;
		}
		yield return null;
		if (ChunkProvider != null)
		{
			yield return ChunkProvider.Init(world);
		}
	}

	public void Cleanup()
	{
		ChunkManager chunkManager = world.m_ChunkManager;
		if (ChunkProvider != null)
		{
			ChunkProvider.StopUpdate();
		}
		List<Chunk> chunkArrayCopySync = GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			RemoveChunk(chunk);
			UnloadChunk(chunk);
		}
		chunkManager.ClearChunksForAllObservers(this);
		if (ChunkProvider != null)
		{
			ChunkProvider.Cleanup();
			ChunkProvider = null;
		}
		lock (DisplayedChunkGameObjects)
		{
			long[] array = new long[DisplayedChunkGameObjects.Count];
			DisplayedChunkGameObjects.Dict.CopyKeysTo(array);
			foreach (long num in array)
			{
				_ = DisplayedChunkGameObjects[num];
				chunkManager.FreeChunkGameObject(this, num);
			}
			DisplayedChunkGameObjects.Clear();
		}
		if (stabilityCalcMainThread != null)
		{
			stabilityCalcMainThread.Cleanup();
			stabilityCalcMainThread = null;
		}
		stabilityCalcLightingThread = null;
		this.OnBlockDamagedDelegates = null;
		this.OnChunksFinishedLoadingDelegates = null;
		this.OnChunksFinishedDisplayingDelegates = null;
		this.OnChunkVisibleDelegates = null;
		this.OnBlockChangedDelegates = null;
		chunkRegenerationStartTimestamps.Clear();
		chunkRegenerationEndTimestamps.Clear();
	}

	public World GetWorld()
	{
		return world;
	}

	public List<Vector3i> GetIndexedBlocks(string _name)
	{
		List<Vector3i> list = new List<Vector3i>();
		List<Chunk> chunkArrayCopySync = GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			lock (chunkArrayCopySync[i])
			{
				if (chunkArrayCopySync[i].InProgressUnloading)
				{
					continue;
				}
				List<Vector3i> list2 = chunkArrayCopySync[i].IndexedBlocks[_name];
				if (list2 != null)
				{
					for (int j = 0; j < list2.Count; j++)
					{
						Vector3i pos = list2[j];
						list.Add(chunkArrayCopySync[i].ToWorldPos(pos));
					}
				}
			}
		}
		return list;
	}

	public IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return GetChunkSync(chunkX, chunkZ);
	}

	public IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return GetChunkSync(World.toChunkXZ(x), World.toChunkXZ(z));
	}

	public IChunk GetChunkFromWorldPos(Vector3i _blockPos)
	{
		return GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkXZ(_blockPos.z));
	}

	public override bool AddChunkSync(Chunk _chunk, bool _bOmitCallbacks = false)
	{
		bool result = base.AddChunkSync(_chunk, _bOmitCallbacks);
		if (!_bOmitCallbacks && IsFixedSize && !bFinishedLoadingDelegateCalled)
		{
			if (chunkKeysNeedLoading == null)
			{
				chunkKeysNeedLoading = new HashSetLong();
				for (int i = ChunkMinPos.x; i <= ChunkMaxPos.x; i++)
				{
					for (int j = ChunkMinPos.y; j <= ChunkMaxPos.y; j++)
					{
						long item = WorldChunkCache.MakeChunkKey(i, j);
						chunkKeysNeedLoading.Add(item);
					}
				}
			}
			if (chunkKeysNeedDisplaying == null)
			{
				chunkKeysNeedDisplaying = new HashSetLong();
				for (int k = ChunkMinPos.x + 2; k <= ChunkMaxPos.x - 2; k++)
				{
					for (int l = ChunkMinPos.y + 2; l <= ChunkMaxPos.y - 2; l++)
					{
						long item2 = WorldChunkCache.MakeChunkKey(k, l);
						chunkKeysNeedDisplaying.Add(item2);
					}
				}
			}
			chunkKeysNeedLoading.Remove(_chunk.Key);
			if (chunkKeysNeedDisplaying.Count > 0 && _chunk.IsEmpty())
			{
				chunkKeysNeedDisplaying.Remove(_chunk.Key);
			}
			if (chunkKeysNeedLoading.Count == 0)
			{
				NotifyOnChunksFinishedLoading();
			}
		}
		return result;
	}

	public void NotifyOnChunksFinishedLoading()
	{
		if (this.OnChunksFinishedLoadingDelegates != null)
		{
			this.OnChunksFinishedLoadingDelegates();
			bFinishedLoadingDelegateCalled = true;
		}
	}

	public void RemoveChunk(Chunk _chunk)
	{
		_chunk.OnUnload(world);
		RemoveChunkSync(_chunk.Key);
	}

	public void UnloadChunk(Chunk _chunk)
	{
		if (ChunkProvider != null)
		{
			_chunk.NeedsRegeneration = true;
			ChunkProvider.UnloadChunk(_chunk);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addDistantDecorationBlocks(Chunk _chunk)
	{
		multiBlockList.Clear();
		World world = GameManager.Instance.World;
		DecoManager.Instance.GetDecorationsOnChunk(_chunk.X, _chunk.Z, multiBlockList);
		for (int num = multiBlockList.Count - 1; num >= 0; num--)
		{
			SBlockPosValue sBlockPosValue = multiBlockList[num];
			int x = World.toBlockXZ(sBlockPosValue.blockPos.x);
			int y = sBlockPosValue.blockPos.y;
			int z = World.toBlockXZ(sBlockPosValue.blockPos.z);
			if (!_chunk.GetBlock(x, y, z).Block.isMultiBlock)
			{
				Block block = sBlockPosValue.blockValue.Block;
				if (block.isMultiBlock)
				{
					for (int num2 = block.multiBlockPos.Length - 1; num2 >= 0; num2--)
					{
						Vector3i vector3i = block.multiBlockPos.Get(num2, sBlockPosValue.blockValue.type, sBlockPosValue.blockValue.rotation);
						Vector3i vector3i2 = sBlockPosValue.blockPos + vector3i;
						BlockValue blockValue = sBlockPosValue.blockValue;
						if (vector3i.x != 0 || vector3i.y != 0 || vector3i.z != 0)
						{
							blockValue.ischild = true;
							blockValue.parentx = -vector3i.x;
							blockValue.parenty = -vector3i.y;
							blockValue.parentz = -vector3i.z;
						}
						sbyte density = GetDensity(vector3i2);
						SetBlockRaw(vector3i2, blockValue);
						SetDensityRaw(vector3i2, density);
						SetStability(vector3i2, 15);
					}
				}
				else if (world.GetBlock(sBlockPosValue.blockPos).isair)
				{
					SetBlockRaw(sBlockPosValue.blockPos, sBlockPosValue.blockValue);
				}
			}
		}
	}

	public void LightChunk(Chunk chunk, Chunk[] _neighbours)
	{
		addDistantDecorationBlocks(chunk);
		if (m_LightProcessorLightingThread != null)
		{
			m_LightProcessorLightingThread.LightChunk(chunk);
			chunk.CheckSameLight();
		}
		CalcStability(chunk);
		chunk.CalcBiomeIntensity(_neighbours);
		chunk.CalcDominantBiome();
	}

	public void CalcStability(Chunk chunk)
	{
		if (stabilityCalcLightingThread != null)
		{
			stabilityCalcLightingThread.DistributeStability(chunk);
			chunk.CheckSameStability();
		}
	}

	public void RegenerateChunk(Chunk _chunk, Chunk[] _neighbours)
	{
		ChunkCacheNeighborChunks chunkCacheNeighborChunks = nChunks;
		IChunk[] chunkArr = _neighbours;
		chunkCacheNeighborChunks.Init(_chunk, chunkArr);
		while (_chunk.NeedsRegeneration)
		{
			VoxelMeshLayer voxelMeshLayer = null;
			for (int i = 0; i < 16; i++)
			{
				if ((_chunk.NeedsRegenerationAt & (1 << i)) != 0)
				{
					if (!meshGenerator.IsLayerEmpty(i))
					{
						voxelMeshLayer = MemoryPools.poolVML.AllocSync(_bReset: true);
						voxelMeshLayer.idx = i;
						voxelMeshLayer.SizeToChunkDefaults();
						break;
					}
					_chunk.ClearNeedsRegenerationAt(i);
				}
			}
			if (voxelMeshLayer == null)
			{
				nChunks.Clear();
				return;
			}
			currentChunkKey = _chunk.Key;
			currentChunkVMLIndex = voxelMeshLayer.idx;
			currentChunkRegenState = "start";
			chunkRegenerationStartTimestamps[_chunk.Key] = (voxelMeshLayer.idx, DateTime.UtcNow);
			_chunk.ClearNeedsRegenerationAt(voxelMeshLayer.idx);
			ChunkCacheNeighborChunks chunkCacheNeighborChunks2 = nChunks;
			chunkArr = _neighbours;
			chunkCacheNeighborChunks2.Init(_chunk, chunkArr);
			currentChunkRegenState = "after init";
			Vector3i chunkPos = new Vector3i(_chunk.X << 4, _chunk.Y << 8 - voxelMeshLayer.idx * 16, _chunk.Z << 4);
			meshGenerator.GenerateMesh(chunkPos, voxelMeshLayer.idx, voxelMeshLayer.meshes);
			currentChunkRegenState = "after generatemesh";
			_chunk.AddMeshLayer(voxelMeshLayer);
			currentChunkRegenState = "finish";
			chunkRegenerationEndTimestamps[_chunk.Key] = (voxelMeshLayer.idx, DateTime.UtcNow);
		}
		nBlocks.Clear();
		nChunks.Clear();
	}

	public bool IsOnBorder(Chunk _c)
	{
		if (IsFixedSize)
		{
			if (_c.X != ChunkMinPos.x && _c.X != ChunkMaxPos.x && _c.Z != ChunkMinPos.y)
			{
				return _c.Z == ChunkMaxPos.y;
			}
			return true;
		}
		return false;
	}

	public sbyte GetDensity(Vector3i _worldPos)
	{
		Chunk chunkSync = GetChunkSync(World.toChunkXZ(_worldPos.x), World.toChunkXZ(_worldPos.z));
		if (chunkSync == null)
		{
			return MarchingCubes.DensityAir;
		}
		Vector3i vector3i = World.toBlock(_worldPos);
		return chunkSync.GetDensity(vector3i.x, vector3i.y, vector3i.z);
	}

	public void SetDensity(Vector3i _pos, sbyte _density, bool _isForceDensity = false)
	{
		SetBlock(_pos, _isChangeBV: false, BlockValue.Air, _isChangeDensity: true, _density, _isNotify: false, _isUpdateLight: false, _isForceDensity);
	}

	public void SetDensityRaw(Vector3i _pos, sbyte _density)
	{
		Chunk chunk = (Chunk)GetChunkFromWorldPos(_pos);
		if (chunk != null)
		{
			int x = World.toBlockXZ(_pos.x);
			int z = World.toBlockXZ(_pos.z);
			int y = World.toBlockY(_pos.y);
			chunk.SetDensity(x, y, z, _density);
		}
	}

	public void SetStability(Vector3i _pos, byte _v)
	{
		Chunk chunk = (Chunk)GetChunkFromWorldPos(_pos);
		if (chunk != null)
		{
			int x = World.toBlockXZ(_pos.x);
			int z = World.toBlockXZ(_pos.z);
			int y = World.toBlockY(_pos.y);
			chunk.SetStability(x, y, z, _v);
		}
	}

	public WaterValue GetWater(Vector3i _pos)
	{
		if ((uint)_pos.y < 256u)
		{
			IChunk chunkFromWorldPos = GetChunkFromWorldPos(_pos);
			if (chunkFromWorldPos != null)
			{
				return chunkFromWorldPos.GetWater(World.toBlockXZ(_pos.x), _pos.y, World.toBlockXZ(_pos.z));
			}
		}
		return WaterValue.Empty;
	}

	public void SetWater(Vector3i _pos, WaterValue _waterData)
	{
		Chunk chunk = (Chunk)GetChunkFromWorldPos(_pos);
		if (chunk != null)
		{
			int num = World.toBlockXZ(_pos.x);
			int num2 = World.toBlockXZ(_pos.z);
			int num3 = World.toBlockY(_pos.y);
			chunk.SetWater(num, num3, num2, _waterData);
			chunkPosNeedsRegeneration(chunk, num, num3, num2, _bTerrainBlockChanged: false);
		}
	}

	public BlockValue GetBlock(Vector3i _pos)
	{
		if ((uint)_pos.y < 256u)
		{
			IChunk chunkFromWorldPos = GetChunkFromWorldPos(_pos);
			if (chunkFromWorldPos != null)
			{
				return chunkFromWorldPos.GetBlock(World.toBlockXZ(_pos.x), _pos.y, World.toBlockXZ(_pos.z));
			}
		}
		return BlockValue.Air;
	}

	public BlockValue SetBlock(Vector3i _pos, BlockValue _bv, bool _isNotify, bool _isUpdateLight)
	{
		return SetBlock(_pos, _isChangeBV: true, _bv, _isChangeDensity: false, 0, _isNotify, _isUpdateLight);
	}

	public BlockValue SetBlock(Vector3i _pos, bool _isChangeBV, BlockValue _bv, bool _isChangeDensity, sbyte _density, bool _isNotify, bool _isUpdateLight, bool _isForceDensity = false, bool _wasChild = false, int _changedByEntityId = -1)
	{
		if (_pos.y <= 0 || _pos.y >= 255)
		{
			return BlockValue.Air;
		}
		Block block = _bv.Block;
		if (block == null)
		{
			return BlockValue.Air;
		}
		int num = World.toChunkXZ(_pos.x);
		int num2 = World.toChunkXZ(_pos.z);
		if (IsFixedSize && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (num <= ChunkMinPos.x + 2)
			{
				int x = ChunkMinPos.x;
				ChunkMinPos.x = num - 3;
				for (int i = ChunkMinPos.x; i < x; i++)
				{
					for (int j = ChunkMinPos.y; j <= ChunkMaxPos.y; j++)
					{
						AddChunkSync(new Chunk(i, j));
					}
				}
			}
			if (num >= ChunkMaxPos.x - 2)
			{
				int x2 = ChunkMaxPos.x;
				ChunkMaxPos.x = num + 3;
				for (int k = x2 + 1; k <= ChunkMaxPos.x; k++)
				{
					for (int l = ChunkMinPos.y; l <= ChunkMaxPos.y; l++)
					{
						AddChunkSync(new Chunk(k, l));
					}
				}
			}
			if (num2 <= ChunkMinPos.y + 2)
			{
				int y = ChunkMinPos.y;
				ChunkMinPos.y = num2 - 3;
				for (int m = ChunkMinPos.y; m < y; m++)
				{
					for (int n = ChunkMinPos.x; n <= ChunkMaxPos.x; n++)
					{
						AddChunkSync(new Chunk(n, m));
					}
				}
			}
			if (num2 >= ChunkMaxPos.y - 2)
			{
				int y2 = ChunkMaxPos.y;
				ChunkMaxPos.y = num2 + 3;
				for (int num3 = y2 + 1; num3 <= ChunkMaxPos.y; num3++)
				{
					for (int num4 = ChunkMinPos.x; num4 <= ChunkMaxPos.x; num4++)
					{
						AddChunkSync(new Chunk(num4, num3));
					}
				}
			}
		}
		Chunk chunkSync = GetChunkSync(num, num2);
		if (chunkSync == null)
		{
			if (_bv.isair)
			{
				DecoManager.Instance.SetBlock(world, _pos, BlockValue.Air);
			}
			else if (block.IsDistantDecoration)
			{
				DecoManager.Instance.SetBlock(world, _pos, _bv);
			}
			return BlockValue.Air;
		}
		int num5 = World.toBlockXZ(_pos.x);
		int blockY = World.toBlockY(_pos.y);
		int num6 = World.toBlockXZ(_pos.z);
		BlockValue blockValue = BlockValue.Air;
		if (_isChangeBV)
		{
			blockValue = chunkSync.SetBlock(world, num5, _pos.y, num6, _bv, _notifyAddChange: true, !_wasChild, _fromReset: false, _poiOwned: false, _changedByEntityId);
		}
		blockValue.Block.CheckUpdate(blockValue, _bv, out var bUpdateMesh, out var bUpdateNotify, out var bUpdateLight);
		if (_isNotify && !bUpdateNotify)
		{
			_isNotify = false;
		}
		if (_isUpdateLight && !bUpdateLight)
		{
			_isUpdateLight = false;
		}
		if (!_isChangeBV)
		{
			_bv = chunkSync.GetBlock(num5, _pos.y, num6);
			block = _bv.Block;
		}
		sbyte density = chunkSync.GetDensity(num5, _pos.y, num6);
		if (!_isChangeDensity)
		{
			_density = density;
		}
		if (!_isForceDensity)
		{
			if (block.shape.IsTerrain())
			{
				if (_density > MarchingCubes.DensityAirHi)
				{
					_density = MarchingCubes.DensityAirHi;
					_isChangeDensity = true;
				}
			}
			else if (_density < MarchingCubes.DensityTerrainHi)
			{
				_density = MarchingCubes.DensityTerrainHi;
				_isChangeDensity = true;
			}
		}
		if (_isChangeDensity)
		{
			chunkSync.SetDensity(num5, _pos.y, num6, _density);
		}
		TextureFullArray textureFullArray = chunkSync.GetTextureFullArray(num5, _pos.y, num6);
		if (_isChangeBV && _isNotify && !isInNotify && (!blockValue.Equals(_bv) || blockValue.damage != _bv.damage))
		{
			isInNotify = true;
			if (!world.IsRemote())
			{
				notifyBlocksOfNeighborChange(_pos, _bv, blockValue);
			}
			if (GameManager.bPhysicsActive && !chunkSync.StopStabilityCalculation && !blockValue.Equals(_bv))
			{
				bool isair = _bv.isair;
				if (!isair && !block.blockMaterial.IsLiquid)
				{
					bool stabilityFull = _bv.Block.StabilityFull;
					stabilityCalcMainThread.BlockPlacedAt(_pos, stabilityFull);
					if (block.isMultiBlock)
					{
						for (int num7 = block.multiBlockPos.Length - 1; num7 >= 0; num7--)
						{
							Vector3i pos = _pos + block.multiBlockPos.Get(num7, _bv.type, _bv.rotation);
							if (pos.x != 0 || pos.y != 0 || pos.z != 0)
							{
								stabilityCalcMainThread.BlockPlacedAt(pos, stabilityFull);
							}
						}
					}
				}
				else if (isair && !blockValue.Block.blockMaterial.IsLiquid)
				{
					stabilityCalcMainThread.BlockRemovedAt(_pos);
					Block block2 = blockValue.Block;
					if (block2.isMultiBlock)
					{
						for (int num8 = block2.multiBlockPos.Length - 1; num8 >= 0; num8--)
						{
							Vector3i vector3i = block2.multiBlockPos.Get(num8, blockValue.type, blockValue.rotation);
							if (vector3i.x != 0 || vector3i.y != 0 || vector3i.z != 0)
							{
								Vector3i pos2 = _pos + vector3i;
								stabilityCalcMainThread.BlockRemovedAt(pos2);
							}
						}
					}
				}
				if (MeshDescription.bDebugStability)
				{
					for (int num9 = num2 - 1; num9 <= num2 + 1; num9++)
					{
						for (int num10 = num - 1; num10 <= num + 1; num10++)
						{
							Chunk chunkSync2 = GetChunkSync(num10, num9);
							if (chunkSync2 != null)
							{
								chunkSync2.NeedsRegeneration = true;
							}
						}
					}
				}
			}
			isInNotify = false;
		}
		if (bUpdateMesh)
		{
			chunkPosNeedsRegeneration(chunkSync, num5, blockY, num6, _isChangeDensity || blockValue.Block.shape.IsTerrain() || block.shape.IsTerrain());
		}
		if (_isChangeBV && _isUpdateLight)
		{
			m_LightProcessorMainThread.RefreshSunlightAtLocalPos(chunkSync, num5, num6, _isSpread: true);
			byte light = chunkSync.GetLight(num5, _pos.y, num6, Chunk.LIGHT_TYPE.BLOCK);
			byte b;
			if (!blockValue.isair && _bv.isair)
			{
				chunkSync.SetLight(num5, _pos.y, num6, 0, Chunk.LIGHT_TYPE.BLOCK);
				m_LightProcessorMainThread.RefreshLightAtLocalPos(chunkSync, num5, _pos.y, num6, Chunk.LIGHT_TYPE.BLOCK);
				b = chunkSync.GetLight(num5, _pos.y, num6, Chunk.LIGHT_TYPE.BLOCK);
			}
			else
			{
				b = block.GetLightValue(_bv);
				chunkSync.SetLight(num5, _pos.y, num6, b, Chunk.LIGHT_TYPE.BLOCK);
			}
			if (b > light)
			{
				m_LightProcessorMainThread.SpreadLight(chunkSync, num5, _pos.y, num6, b, Chunk.LIGHT_TYPE.BLOCK);
			}
			else if (b < light)
			{
				m_LightProcessorMainThread.UnspreadLight(chunkSync, num5, _pos.y, num6, light, Chunk.LIGHT_TYPE.BLOCK);
			}
		}
		if (chunkSync.GetTextureFull(num5, _pos.y, num6) != 0 && !blockValue.isair && _bv.isair)
		{
			chunkSync.SetTextureFull(num5, _pos.y, num6, 0L);
		}
		if (this.OnBlockChangedDelegates != null)
		{
			this.OnBlockChangedDelegates(_pos, blockValue, density, textureFullArray, _bv);
		}
		return blockValue;
	}

	public void SetBlockRaw(Vector3i _worldBlockPos, BlockValue _blockValue)
	{
		GetChunkSync(World.toChunkXZ(_worldBlockPos.x), World.toChunkXZ(_worldBlockPos.z))?.SetBlockRaw(World.toBlockXZ(_worldBlockPos.x), _worldBlockPos.y, World.toBlockXZ(_worldBlockPos.z), _blockValue);
	}

	public byte GetLight(Vector3i _blockPos, Chunk.LIGHT_TYPE type)
	{
		return GetChunkFromWorldPos(_blockPos)?.GetLight(World.toBlockXZ(_blockPos.x), World.toBlockY(_blockPos.y), World.toBlockXZ(_blockPos.z), type) ?? 0;
	}

	public ChunkGameObject RemoveDisplayedChunkGameObject(long _key)
	{
		lock (DisplayedChunkGameObjects)
		{
			ChunkGameObject result = DisplayedChunkGameObjects[_key];
			DisplayedChunkGameObjects.Remove(_key);
			return result;
		}
	}

	public void SetDisplayedChunkGameObject(long _key, ChunkGameObject _cgo)
	{
		lock (DisplayedChunkGameObjects)
		{
			DisplayedChunkGameObjects[_key] = _cgo;
		}
	}

	public void ChunkPosNeedsRegeneration_DelayedStart()
	{
		delayedRegenCount++;
		if (delayedRegenCount == 1)
		{
			lock (delayedRegenChunks)
			{
				delayedRegenChunks.Clear();
			}
		}
	}

	public void ChunkPosNeedsRegeneration_DelayedStop()
	{
		delayedRegenCount--;
		if (delayedRegenCount != 0)
		{
			return;
		}
		lock (delayedRegenChunks)
		{
			foreach (KeyValuePair<Chunk, int> delayedRegenChunk in delayedRegenChunks)
			{
				delayedRegenChunk.Key.SetNeedsRegenerationRaw(delayedRegenChunk.Value);
			}
			delayedRegenChunks.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void chunkRegenerateAt(Chunk _c, int _yPos)
	{
		if (delayedRegenCount == 0)
		{
			_c.NeedsRegenerationAt = _yPos;
			return;
		}
		lock (delayedRegenChunks)
		{
			if (!delayedRegenChunks.TryGetValue(_c, out var value))
			{
				value = _c.NeedsRegenerationAt;
				delayedRegenChunks.Add(_c, 0);
			}
			delayedRegenChunks[_c] = value | (1 << _yPos / 16);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void chunkPosNeedsRegeneration(Chunk _chunk, int _blockX, int _blockY, int _blockZ, bool _bTerrainBlockChanged)
	{
		chunksNeedingRegThisCall.Clear();
		chunkRegenerateAt(_chunk, _blockY);
		chunksNeedingRegThisCall.Add(_chunk);
		if (_blockY > 0 && _blockY % 16 == 0)
		{
			chunkRegenerateAt(_chunk, _blockY - 1);
		}
		else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
		{
			chunkRegenerateAt(_chunk, _blockY + 1);
		}
		switch (_blockX)
		{
		case 15:
		{
			Chunk chunkSync2 = GetChunkSync(_chunk.X + 1, _chunk.Z);
			if (chunkSync2 != null)
			{
				chunkRegenerateAt(chunkSync2, _blockY);
				chunksNeedingRegThisCall.Add(chunkSync2);
				if (_blockY > 0 && _blockY % 16 == 0)
				{
					chunkRegenerateAt(chunkSync2, _blockY - 1);
				}
				else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
				{
					chunkRegenerateAt(chunkSync2, _blockY + 1);
				}
			}
			break;
		}
		case 0:
		{
			Chunk chunkSync = GetChunkSync(_chunk.X - 1, _chunk.Z);
			if (chunkSync != null)
			{
				chunkRegenerateAt(chunkSync, _blockY);
				chunksNeedingRegThisCall.Add(chunkSync);
				if (_blockY > 0 && _blockY % 16 == 0)
				{
					chunkRegenerateAt(chunkSync, _blockY - 1);
				}
				else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
				{
					chunkRegenerateAt(chunkSync, _blockY + 1);
				}
			}
			break;
		}
		}
		switch (_blockZ)
		{
		case 0:
		{
			Chunk chunkSync4 = GetChunkSync(_chunk.X, _chunk.Z - 1);
			if (chunkSync4 != null)
			{
				chunkRegenerateAt(chunkSync4, _blockY);
				chunksNeedingRegThisCall.Add(chunkSync4);
				if (_blockY > 0 && _blockY % 16 == 0)
				{
					chunkRegenerateAt(chunkSync4, _blockY - 1);
				}
				else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
				{
					chunkRegenerateAt(chunkSync4, _blockY + 1);
				}
			}
			break;
		}
		case 15:
		{
			Chunk chunkSync3 = GetChunkSync(_chunk.X, _chunk.Z + 1);
			if (chunkSync3 != null)
			{
				chunkRegenerateAt(chunkSync3, _blockY);
				chunksNeedingRegThisCall.Add(chunkSync3);
				if (_blockY > 0 && _blockY % 16 == 0)
				{
					chunkRegenerateAt(chunkSync3, _blockY - 1);
				}
				else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
				{
					chunkRegenerateAt(chunkSync3, _blockY + 1);
				}
			}
			break;
		}
		}
		if (_bTerrainBlockChanged)
		{
			if (_blockX == 0 && _blockZ == 0)
			{
				Chunk chunkSync5 = GetChunkSync(_chunk.X - 1, _chunk.Z - 1);
				if (chunkSync5 != null)
				{
					chunkRegenerateAt(chunkSync5, _blockY);
					chunksNeedingRegThisCall.Add(chunkSync5);
					if (_blockY > 0 && _blockY % 16 == 0)
					{
						chunkRegenerateAt(chunkSync5, _blockY - 1);
					}
					else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
					{
						chunkRegenerateAt(chunkSync5, _blockY + 1);
					}
				}
			}
			if (_blockX == 0 && _blockZ == 15)
			{
				Chunk chunkSync6 = GetChunkSync(_chunk.X - 1, _chunk.Z + 1);
				if (chunkSync6 != null)
				{
					chunkRegenerateAt(chunkSync6, _blockY);
					chunksNeedingRegThisCall.Add(chunkSync6);
					if (_blockY > 0 && _blockY % 16 == 0)
					{
						chunkRegenerateAt(chunkSync6, _blockY - 1);
					}
					else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
					{
						chunkRegenerateAt(chunkSync6, _blockY + 1);
					}
				}
			}
			if (_blockX == 15 && _blockZ == 0)
			{
				Chunk chunkSync7 = GetChunkSync(_chunk.X + 1, _chunk.Z - 1);
				if (chunkSync7 != null)
				{
					chunkRegenerateAt(chunkSync7, _blockY);
					chunksNeedingRegThisCall.Add(chunkSync7);
					if (_blockY > 0 && _blockY % 16 == 0)
					{
						chunkRegenerateAt(chunkSync7, _blockY - 1);
					}
					else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
					{
						chunkRegenerateAt(chunkSync7, _blockY + 1);
					}
				}
			}
			if (_blockX == 15 && _blockZ == 15)
			{
				Chunk chunkSync8 = GetChunkSync(_chunk.X + 1, _chunk.Z + 1);
				if (chunkSync8 != null)
				{
					chunkRegenerateAt(chunkSync8, _blockY);
					chunksNeedingRegThisCall.Add(chunkSync8);
					if (_blockY > 0 && _blockY % 16 == 0)
					{
						chunkRegenerateAt(chunkSync8, _blockY - 1);
					}
					else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
					{
						chunkRegenerateAt(chunkSync8, _blockY + 1);
					}
				}
			}
		}
		EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
		if (primaryPlayer != null && chunksNeedingRegThisCall.Count > 0)
		{
			Vector3i vector3i = World.worldToBlockPos(ToLocalPosition(primaryPlayer.GetPosition()));
			int num = World.toChunkXZ(vector3i.x);
			int num2 = World.toChunkXZ(vector3i.z);
			ChunkManager chunkManager = world.m_ChunkManager;
			chunkManager.ResetChunksToCopyInOneFrame();
			for (int i = 0; i < chunksNeedingRegThisCall.Count; i++)
			{
				Chunk chunk = chunksNeedingRegThisCall[i];
				int num3 = num - chunk.X;
				int num4 = num2 - chunk.Z;
				if (num3 < 0)
				{
					num3 = -num3;
				}
				if (num4 < 0)
				{
					num4 = -num4;
				}
				if (num3 <= 1 && num4 <= 1)
				{
					chunkManager.ChunksToCopyInOneFrame.Add(chunk);
				}
			}
		}
		chunksNeedingRegThisCall.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifyBlocksOfNeighborChange(Vector3i _worldBlockPos, BlockValue _newBlockValue, BlockValue _oldBlockValue)
	{
		for (int i = 0; i < Vector3i.AllDirections.Length; i++)
		{
			notifyBlockOfNeighborChange(_worldBlockPos + Vector3i.AllDirections[i], _newBlockValue, _oldBlockValue, _worldBlockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifyBlockOfNeighborChange(Vector3i _myBlockPos, BlockValue _newBlockValue, BlockValue _oldBlockValue, Vector3i _blockPosThatChanged)
	{
		if (!world.IsRemote())
		{
			BlockValue block = world.GetBlock(_myBlockPos);
			if (!block.isair)
			{
				block.Block.OnNeighborBlockChange(world, 0, _myBlockPos, block, _blockPosThatChanged, _newBlockValue, _oldBlockValue);
			}
		}
	}

	public Vector3 ToWorldPosition(Vector3 _localPos)
	{
		return _localPos + Position;
	}

	public Vector3 ToLocalPosition(Vector3 _worldPos)
	{
		_worldPos.x -= Position.x;
		_worldPos.y -= Position.y;
		_worldPos.z -= Position.z;
		return _worldPos;
	}

	public Vector3 ToLocalVector(Vector3 _vector)
	{
		return _vector;
	}

	public long ToLocalKey(long _key)
	{
		int num = World.toChunkXZ(Mathf.FloorToInt(Position.x));
		int num2 = World.toChunkXZ(Mathf.FloorToInt(Position.z));
		int x = WorldChunkCache.extractX(_key) - num;
		int y = WorldChunkCache.extractZ(_key) - num2;
		return WorldChunkCache.MakeChunkKey(x, y);
	}

	public List<BlockEntityData> GetBlockEntities(string _indexedBlockKey)
	{
		List<BlockEntityData> list = new List<BlockEntityData>();
		List<Chunk> chunkArrayCopySync = GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			List<Vector3i> list2 = chunkArrayCopySync[i].IndexedBlocks[_indexedBlockKey];
			if (list2 == null)
			{
				continue;
			}
			for (int j = 0; j < list2.Count; j++)
			{
				Vector3i pos = list2[j];
				Vector3i worldPos = chunkArrayCopySync[i].ToWorldPos(pos);
				BlockEntityData blockEntity = chunkArrayCopySync[i].GetBlockEntity(worldPos);
				if (blockEntity != null)
				{
					list.Add(blockEntity);
				}
			}
		}
		return list;
	}

	public BlockEntityData GetBlockEntity(Vector3i _blockLocalPos)
	{
		return GetChunkFromWorldPos(_blockLocalPos)?.GetBlockEntity(_blockLocalPos);
	}

	public void DebugOnGUI(float middleX, float middleY, float size)
	{
		List<Chunk> chunkArrayCopySync = GetChunkArrayCopySync();
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			vector.x = Utils.FastMin(chunk.X, vector.x);
			vector.y = Utils.FastMin(chunk.Z, vector.y);
			vector2.x = Utils.FastMax(chunk.X, vector2.x);
			vector2.y = Utils.FastMax(chunk.Z, vector2.y);
		}
		vector *= size;
		vector2 *= size;
		float num = middleX + vector.x;
		if (num < 0f)
		{
			middleX -= num;
		}
		float num2 = middleY + vector.y;
		if (num2 < 0f)
		{
			middleY += num2;
		}
		num = middleX - vector2.x;
		if (num < 0f)
		{
			middleX += num;
		}
		num2 = middleY - vector2.y;
		if (num2 < 0f)
		{
			middleY -= num2;
		}
		for (int j = 0; j < chunkArrayCopySync.Count; j++)
		{
			Chunk chunk2 = chunkArrayCopySync[j];
			bool num3 = DisplayedChunkGameObjects.ContainsKey(chunk2.Key);
			Color colFill = new Color(0.1f, 0.5f, 0.1f);
			Color colBorder = Color.black;
			if (num3)
			{
				colBorder = new Color(0.6f, 0.6f, 0.6f);
			}
			if (chunk2.NeedsDecoration)
			{
				colFill = Color.red;
			}
			else if (chunk2.NeedsLightCalculation)
			{
				colFill = new Color(0.7f, 0.7f, 0f);
			}
			else if (chunk2.NeedsRegeneration)
			{
				colFill = new Color(0.1f, 0.1f, 0.7f);
			}
			else if (chunk2.NeedsCopying)
			{
				colFill = new Color(0.7f, 0.1f, 0.7f);
			}
			else if (chunk2.NeedsOnlyCollisionMesh)
			{
				colFill = Color.gray;
			}
			colFill.a = 0.7f;
			GUIUtils.DrawFilledRect(new Rect(middleX + (float)chunk2.X * size - size * 0.5f, middleY - (float)chunk2.Z * size - size * 0.5f, size, size), colFill, _bDrawBorder: true, colBorder);
		}
	}

	public void SnapTerrainToPositionAroundLocal(Vector3i _worldPos)
	{
		SnapTerrainToPositionAroundRPC(null, _worldPos);
	}

	public void SnapTerrainToPositionAroundRPC(WorldBase _world, Vector3i _worldPos)
	{
		if (GetBlock(_worldPos).Block.shape.IsTerrain())
		{
			snapTerrainToPosition(_world, _worldPos, _bLiftUpTerrainByOneIfNeeded: false, _bUseHalfTerrainDensity: false);
			snapTerrainToPosition(_world, _worldPos + Vector3i.right, _bLiftUpTerrainByOneIfNeeded: false, _bUseHalfTerrainDensity: true);
			snapTerrainToPosition(_world, _worldPos - Vector3i.right, _bLiftUpTerrainByOneIfNeeded: false, _bUseHalfTerrainDensity: true);
			snapTerrainToPosition(_world, _worldPos + Vector3i.forward, _bLiftUpTerrainByOneIfNeeded: false, _bUseHalfTerrainDensity: true);
			snapTerrainToPosition(_world, _worldPos - Vector3i.forward, _bLiftUpTerrainByOneIfNeeded: false, _bUseHalfTerrainDensity: true);
		}
	}

	public void SnapTerrainToPositionAtLocal(Vector3i _worldPos, bool _bLiftUpTerrainByOneIfNeeded, bool _bUseHalfTerrainDensity)
	{
		snapTerrainToPosition(null, _worldPos, _bLiftUpTerrainByOneIfNeeded, _bUseHalfTerrainDensity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void snapTerrainToPosition(WorldBase _world, Vector3i _worldPos, bool _bLiftUpTerrainByOneIfNeeded, bool _bUseHalfTerrainDensity)
	{
		if (_worldPos.y < 1)
		{
			return;
		}
		BlockValue block = GetBlock(_worldPos);
		if (!block.Block.shape.IsTerrain())
		{
			if (!_bLiftUpTerrainByOneIfNeeded || !block.isair)
			{
				return;
			}
			BlockValue block2 = GetBlock(_worldPos - Vector3i.up);
			if (!block2.Block.shape.IsTerrain())
			{
				return;
			}
			sbyte density = GetDensity(_worldPos);
			sbyte b = (sbyte)(_bUseHalfTerrainDensity ? (MarchingCubes.DensityTerrain / 2) : MarchingCubes.DensityTerrain);
			if (_world == null)
			{
				SetBlockRaw(_worldPos, block2);
				if (density > b)
				{
					SetDensityRaw(_worldPos, b);
				}
			}
			else if (density > b)
			{
				_world.SetBlockRPC(_worldPos, block2, b);
			}
			else
			{
				_world.SetBlockRPC(_worldPos, block2);
			}
		}
		else
		{
			if (GetBlock(_worldPos + Vector3i.up).Block.IsTerrainDecoration)
			{
				return;
			}
			sbyte density2 = GetDensity(_worldPos);
			sbyte b2 = (sbyte)(_bUseHalfTerrainDensity ? (MarchingCubes.DensityTerrain / 2) : MarchingCubes.DensityTerrain);
			if (density2 > b2)
			{
				if (_world == null)
				{
					SetDensityRaw(_worldPos, b2);
				}
				else
				{
					_world.SetBlockRPC(_worldPos, b2);
				}
			}
		}
	}

	public void InvokeOnBlockDamagedDelegates(Vector3i _blockPos, BlockValue _blockValue, int _damage, int _attackerEntityId)
	{
		if (this.OnBlockDamagedDelegates != null)
		{
			this.OnBlockDamagedDelegates(_blockPos, _blockValue, _damage, _attackerEntityId);
		}
	}

	public bool Overlaps(Bounds _boundsInWorldCoord)
	{
		return true;
	}

	public void OnChunkDisplayed(long _key, bool _isDisplayed)
	{
		if (this.OnChunkVisibleDelegates != null)
		{
			this.OnChunkVisibleDelegates(_key, _isDisplayed);
		}
		if (bFinishedDisplayingDelegateCalled || chunkKeysNeedDisplaying == null)
		{
			return;
		}
		chunkKeysNeedDisplaying.Remove(_key);
		if (chunkKeysNeedDisplaying.Count == 0)
		{
			bFinishedDisplayingDelegateCalled = true;
			if (this.OnChunksFinishedDisplayingDelegates != null)
			{
				this.OnChunksFinishedDisplayingDelegates();
			}
		}
	}

	public void CheckCollisionWithBlocks(Entity _entity)
	{
		Vector3 min = ToLocalPosition(_entity.boundingBox.min + new Vector3(0.001f, 0.001f, 0.001f));
		Vector3 max = ToLocalPosition(_entity.boundingBox.max - new Vector3(0.001f, 0.001f, 0.001f));
		int num = Utils.Fastfloor(min.x - 0.5f);
		int num2 = Utils.Fastfloor(min.y - 0.5f);
		int num3 = Utils.Fastfloor(min.z - 0.5f);
		int num4 = Utils.Fastfloor(max.x + 0.5f);
		int num5 = Utils.Fastfloor(max.y + 0.5f);
		int num6 = Utils.Fastfloor(max.z + 0.5f);
		Bounds aabb = default(Bounds);
		aabb.SetMinMax(min, max);
		aabb.Expand(new Vector3(0.05f, 0.05f, 0.05f));
		float num7 = _entity.m_characterController?.GetSkinWidth() ?? 0.08f;
		aabb.min = new Vector3(aabb.min.x, aabb.min.y - num7, aabb.min.z);
		if (num2 <= 0)
		{
			num2 = 1;
		}
		if (num5 >= 256)
		{
			num5 = 255;
		}
		IChunk chunk = null;
		for (int i = num; i <= num4; i++)
		{
			for (int j = num3; j <= num6; j++)
			{
				int num8 = World.toChunkXZ(i);
				int num9 = World.toChunkXZ(j);
				if (chunk == null || chunk.X != num8 || chunk.Z != num9)
				{
					chunk = GetChunkSync(num8, num9);
					if (chunk == null)
					{
						continue;
					}
				}
				int x = World.toBlockXZ(i);
				int z = World.toBlockXZ(j);
				for (int k = num2; k <= num5; k++)
				{
					BlockValue block = chunk.GetBlock(x, k, z);
					if (block.isair)
					{
						continue;
					}
					Block block2 = block.Block;
					if (block2.IsCheckCollideWithEntity)
					{
						Vector3i vector3i = new Vector3i(i, k, j);
						if (block2.isMultiBlock && block.ischild)
						{
							Vector3i parentPos = block2.multiBlockPos.GetParentPos(vector3i, block);
							block = world.GetBlock(parentPos);
							vector3i = parentPos;
						}
						if (block2.HasCollidingAABB(block, vector3i.x, vector3i.y, vector3i.z, 0f, aabb))
						{
							block2.OnEntityCollidedWithBlock(world, 0, vector3i, block, _entity);
						}
					}
				}
			}
		}
	}

	public void Save()
	{
		if (ChunkProvider != null)
		{
			ChunkProvider.SaveAll();
		}
	}

	public int GetBlockFaceTexture(Vector3i _blockPos, BlockFace _blockFace, int _channel)
	{
		return ((Chunk)GetChunkFromWorldPos(_blockPos))?.GetBlockFaceTexture(World.toBlockXZ(_blockPos.x), World.toBlockY(_blockPos.y), World.toBlockXZ(_blockPos.z), _blockFace, _channel) ?? 0;
	}

	public void SetBlockFaceTexture(Vector3i _blockPos, BlockFace _blockFace, int _textureIdx, int _channel)
	{
		Chunk chunk = (Chunk)GetChunkFromWorldPos(_blockPos);
		if (chunk != null)
		{
			int num = World.toBlockXZ(_blockPos.x);
			int num2 = World.toBlockY(_blockPos.y);
			int num3 = World.toBlockXZ(_blockPos.z);
			TextureFullArray textureFullArray = chunk.GetTextureFullArray(num, num2, num3, applyIgnore: false);
			chunk.SetBlockFaceTexture(num, num2, num3, _blockFace, _textureIdx, _channel);
			chunkPosNeedsRegeneration(chunk, num, num2, num3, _bTerrainBlockChanged: false);
			if (this.OnBlockChangedDelegates != null)
			{
				BlockValue block = GetBlock(_blockPos);
				this.OnBlockChangedDelegates(_blockPos, block, GetDensity(_blockPos), textureFullArray, block);
			}
		}
	}

	public void SetTextureFull(Vector3i _blockPos, long _textureFull, int channel)
	{
		Chunk chunk = (Chunk)GetChunkFromWorldPos(_blockPos);
		if (chunk != null)
		{
			int num = World.toBlockXZ(_blockPos.x);
			int num2 = World.toBlockY(_blockPos.y);
			int num3 = World.toBlockXZ(_blockPos.z);
			TextureFullArray textureFullArray = chunk.GetTextureFullArray(num, num2, num3, applyIgnore: false);
			chunk.SetTextureFull(num, num2, num3, _textureFull, channel);
			chunkPosNeedsRegeneration(chunk, num, num2, num3, _bTerrainBlockChanged: false);
			if (this.OnBlockChangedDelegates != null)
			{
				BlockValue block = GetBlock(_blockPos);
				this.OnBlockChangedDelegates(_blockPos, block, GetDensity(_blockPos), textureFullArray, block);
			}
		}
	}

	public void SetTextureFullArray(Vector3i _blockPos, TextureFullArray _textureFull)
	{
		Chunk chunk = (Chunk)GetChunkFromWorldPos(_blockPos);
		if (chunk != null)
		{
			int num = World.toBlockXZ(_blockPos.x);
			int num2 = World.toBlockY(_blockPos.y);
			int num3 = World.toBlockXZ(_blockPos.z);
			TextureFullArray setTextureFullArray = chunk.GetSetTextureFullArray(num, num2, num3, _textureFull);
			chunkPosNeedsRegeneration(chunk, num, num2, num3, _bTerrainBlockChanged: false);
			if (this.OnBlockChangedDelegates != null)
			{
				BlockValue block = GetBlock(_blockPos);
				this.OnBlockChangedDelegates(_blockPos, block, GetDensity(_blockPos), setTextureFullArray, block);
			}
		}
	}

	public long GetTextureFull(Vector3i _blockPos)
	{
		return ((Chunk)GetChunkFromWorldPos(_blockPos))?.GetTextureFull(World.toBlockXZ(_blockPos.x), World.toBlockY(_blockPos.y), World.toBlockXZ(_blockPos.z)) ?? 0;
	}

	public TextureFullArray GetTextureFullArray(Vector3i _blockPos)
	{
		return ((Chunk)GetChunkFromWorldPos(_blockPos))?.GetTextureFullArray(World.toBlockXZ(_blockPos.x), World.toBlockY(_blockPos.y), World.toBlockXZ(_blockPos.z)) ?? TextureFullArray.Default;
	}

	public void MGTest()
	{
		meshGenerator.Test();
	}

	public static (int, double) SecondsSinceChunkStartedRegeneration(long chunkKey)
	{
		if (chunkRegenerationStartTimestamps.TryGetValue(chunkKey, out var value))
		{
			return (value.Item1, (DateTime.UtcNow - value.Item2).TotalSeconds);
		}
		return (-1, -1.0);
	}

	public static (int, double) SecondsSinceChunkEndedRegeneration(long chunkKey)
	{
		if (chunkRegenerationEndTimestamps.TryGetValue(chunkKey, out var value))
		{
			return (value.Item1, (DateTime.UtcNow - value.Item2).TotalSeconds);
		}
		return (-1, -1.0);
	}

	public static void LogCurrentChunkRegenerationState()
	{
		Log.Out($"[FELLTHROUGHWORLD] ChunkCluster Current Regeneration State -  Chunk: {currentChunkKey}, VML Index: {currentChunkVMLIndex}, Regeneration State: {currentChunkRegenState}");
	}
}
