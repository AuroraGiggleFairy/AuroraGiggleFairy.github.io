#define DEBUG_CHUNK_PROFILE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Platform;
using UnityEngine;

public class Chunk : IChunk, IBlockAccess, IMemoryPoolableObject
{
	public enum LIGHT_TYPE
	{
		BLOCK,
		SUN
	}

	public enum DisplayState
	{
		Start,
		BlockEntities,
		Done
	}

	public struct DensityMismatchInformation(int _x, int _y, int _z, sbyte _density, int _bvType, bool _isTerrain)
	{
		public int x = _x;

		public int y = _y;

		public int z = _z;

		public sbyte density = _density;

		public int bvType = _bvType;

		public bool isTerrain = _isTerrain;

		public string ToJsonString()
		{
			return $"{{\"x\":{x}, \"y\":{y}, \"z\":{z}, \"density\":{density}, \"bvtype\":{bvType}, \"terrain\":{isTerrain.ToString().ToLower()}}}";
		}

		public override string ToString()
		{
			return $"DENSITYMISMATCH;{x};{y};{z};{density};{isTerrain};{bvType}";
		}
	}

	public static uint CurrentSaveVersion = 47u;

	public const int cAreaMasterSizeChunks = 5;

	public const int cAreaMasterSizeBlocks = 80;

	public const int cTextureChannelCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockLayer[] m_BlockLayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnStability;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel[] chnTextures;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnWater;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_X;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_Y;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_Z;

	public Vector3i worldPosIMin;

	public Vector3i worldPosIMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double cEntityListHeight = 16.0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEntityListCount = 16;

	public List<Entity>[] entityLists = new List<Entity>[16];

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<Vector3i, TileEntity> tileEntities = new DictionaryList<Vector3i, TileEntity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> sleeperVolumes = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> triggerVolumes = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> wallVolumes = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_HeightMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_bTopSoilBroken;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_Biomes;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_BiomeIntensities;

	public byte DominantBiome;

	public byte AreaMasterDominantBiome = byte.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_NormalX;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_NormalY;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_NormalZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_TerrainHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityCreationData> entityStubs = new List<EntityCreationData>();

	public DictionaryKeyValueList<string, ChunkCustomData> ChunkCustomData = new DictionaryKeyValueList<string, ChunkCustomData>();

	public ulong SavedInWorldTicks;

	public ulong LastTimeRandomTicked;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3b> insideDevices = new List<Vector3b>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> insideDevicesHashSet = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<Vector3i, BlockTrigger> triggerData = new DictionaryList<Vector3i, BlockTrigger>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<ulong, BlockEntityData> blockEntityStubs = new DictionaryList<ulong, BlockEntityData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockEntityData> blockEntityStubsToRemove = new List<BlockEntityData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkAreaBiomeSpawnData biomeSpawnData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<int> m_layerIndexQueue = new Queue<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public VoxelMeshLayer[] m_meshLayers = new VoxelMeshLayer[16];

	public volatile bool hasEntities;

	public bool isModified;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds boundingBox;

	public DictionarySave<string, List<Vector3i>> IndexedBlocks = new DictionarySave<string, List<Vector3i>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int m_NeedsRegenerationAtY;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDecoAllowed[] m_DecoBiomeArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[] mapColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMapDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEmptyDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryKeyList<Vector3i, int> tickedBlocks = new DictionaryKeyList<Vector3i, int>();

	public bool IsInternalBlocksCulled;

	public bool StopStabilityCalculation;

	public OcclusionManager.OccludeeZone occludeeZone;

	public readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterSimulationNative.ChunkHandle waterSimHandle;

	public static int InstanceCount;

	public int TotalMemory;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalTris;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[][] trisInMesh = new int[16][];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[][] sizeOfMesh = new int[16][];

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterDebugManager.RendererHandle waterDebugHandle;

	public readonly int ClrIdx;

	public volatile bool InProgressCopying;

	public volatile bool InProgressDecorating;

	public volatile bool InProgressLighting;

	public volatile bool InProgressRegeneration;

	public volatile bool InProgressUnloading;

	public volatile bool InProgressSaving;

	public volatile bool InProgressNetworking;

	public volatile bool InProgressWaterSim;

	public volatile bool IsDisplayed;

	public volatile bool IsCollisionMeshGenerated;

	public volatile bool NeedsOnlyCollisionMesh;

	public int NeedsRegenerationDebug;

	public volatile bool NeedsDecoration;

	public volatile bool NeedsLightDecoration;

	public volatile bool NeedsLightCalculation;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue bvPOIFiller;

	public static bool IgnorePaintTextures = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawnedBiomeParticles;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> biomeParticles;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Transform> occlusionTs = new List<Transform>(200);

	public DisplayState displayState;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockEntitiesIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MeshRenderer> tempMeshRenderers = new List<MeshRenderer>();

	public int MeshLayerCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] biomeCnt = new int[50];

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedToString;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int dbChunkX = 136;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int dbChunkZ = 25;

	public int X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_X;
		}
		set
		{
			cachedToString = null;
			m_X = value;
			updateBounds();
		}
	}

	public int Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Y;
		}
	}

	public int Z
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Z;
		}
		set
		{
			cachedToString = null;
			m_Z = value;
			updateBounds();
		}
	}

	public Vector3i ChunkPos
	{
		get
		{
			return new Vector3i(m_X, m_Y, m_Z);
		}
		set
		{
			cachedToString = null;
			m_X = value.x;
			m_Z = value.z;
			updateBounds();
		}
	}

	public long Key => WorldChunkCache.MakeChunkKey(m_X, m_Z);

	public bool IsLocked
	{
		get
		{
			if (!InProgressCopying && !InProgressDecorating && !InProgressLighting && !InProgressRegeneration && !InProgressUnloading && !InProgressSaving && !InProgressNetworking)
			{
				return InProgressWaterSim;
			}
			return true;
		}
	}

	public bool IsLockedExceptUnloading
	{
		get
		{
			if (!InProgressCopying && !InProgressDecorating && !InProgressLighting && !InProgressRegeneration && !InProgressSaving && !InProgressNetworking)
			{
				return InProgressWaterSim;
			}
			return true;
		}
	}

	public bool IsInitialized
	{
		get
		{
			if (!NeedsLightCalculation && !InProgressDecorating)
			{
				return !InProgressUnloading;
			}
			return false;
		}
	}

	public bool NeedsRegeneration
	{
		get
		{
			lock (this)
			{
				return m_NeedsRegenerationAtY != 0;
			}
		}
		set
		{
			lock (m_layerIndexQueue)
			{
				MeshLayerCount = 0;
				m_layerIndexQueue.Clear();
				MemoryPools.poolVML.FreeSync(m_meshLayers);
			}
			lock (this)
			{
				if (value)
				{
					m_NeedsRegenerationAtY = 65535;
				}
				else
				{
					m_NeedsRegenerationAtY = 0;
				}
			}
			NeedsRegenerationDebug = m_NeedsRegenerationAtY;
		}
	}

	public bool NeedsCopying => HasMeshLayer();

	public int NeedsRegenerationAt
	{
		get
		{
			lock (this)
			{
				return m_NeedsRegenerationAtY;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			lock (this)
			{
				m_NeedsRegenerationAtY |= 1 << (value >> 4);
			}
		}
	}

	public bool NeedsSaving
	{
		get
		{
			if (!isModified && !hasEntities && tileEntities.Count <= 0)
			{
				return triggerData.Count > 0;
			}
			return true;
		}
	}

	public bool NeedsTicking
	{
		get
		{
			if (tileEntities.Count <= 0)
			{
				return sleeperVolumes.Count > 0;
			}
			return true;
		}
	}

	public void AssignWaterSimHandle(WaterSimulationNative.ChunkHandle handle)
	{
		waterSimHandle = handle;
	}

	public void ResetWaterSimHandle()
	{
		waterSimHandle.Reset();
	}

	public void AssignWaterDebugRenderer(WaterDebugManager.RendererHandle handle)
	{
		waterDebugHandle = handle;
	}

	public void ResetWaterDebugHandle()
	{
	}

	public byte[] GetTopSoil()
	{
		return m_bTopSoilBroken;
	}

	public void SetTopSoil(IList<byte> soil)
	{
		for (int i = 0; i < m_bTopSoilBroken.Length; i++)
		{
			m_bTopSoilBroken[i] = soil[i];
		}
	}

	public Chunk()
	{
		m_X = 0;
		m_Y = 0;
		Z = 0;
		for (int i = 0; i < trisInMesh.GetLength(0); i++)
		{
			trisInMesh[i] = new int[MeshDescription.meshes.Length];
			sizeOfMesh[i] = new int[MeshDescription.meshes.Length];
		}
		for (int j = 0; j < 16; j++)
		{
			entityLists[j] = new List<Entity>();
		}
		NeedsLightCalculation = true;
		NeedsDecoration = true;
		hasEntities = false;
		isModified = false;
		m_BlockLayers = new ChunkBlockLayer[64];
		chnLight = new ChunkBlockChannel(0L);
		chnDensity = new ChunkBlockChannel((byte)MarchingCubes.DensityAir);
		chnStability = new ChunkBlockChannel(0L);
		chnDamage = new ChunkBlockChannel(0L, 2);
		chnTextures = new ChunkBlockChannel[1];
		for (int k = 0; k < 1; k++)
		{
			chnTextures[k] = new ChunkBlockChannel(0L, 6);
		}
		chnWater = new ChunkBlockChannel(0L, 2);
		m_HeightMap = new byte[256];
		m_TerrainHeight = new byte[256];
		m_bTopSoilBroken = new byte[32];
		m_Biomes = new byte[256];
		m_BiomeIntensities = new byte[1536];
		m_NormalX = new byte[256];
		m_NormalY = new byte[256];
		m_NormalZ = new byte[256];
		InstanceCount++;
	}

	public Chunk(int _x, int _z)
		: this()
	{
		m_X = _x;
		m_Y = 0;
		m_Z = _z;
		ResetStability();
		RefreshSunlight();
		NeedsLightCalculation = true;
		NeedsDecoration = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	~Chunk()
	{
		InstanceCount--;
	}

	public void ResetLights(byte _lightValue = 0)
	{
		chnLight.Clear(_lightValue);
	}

	public void Reset()
	{
		if (InProgressSaving)
		{
			Log.Warning("Unloading: chunk while saving " + this);
		}
		cachedToString = null;
		m_X = 0;
		m_Y = 0;
		Z = 0;
		MeshLayerCount = 0;
		for (int i = 0; i < 16; i++)
		{
			entityLists[i].Clear();
		}
		entityStubs.Clear();
		blockEntityStubs.Clear();
		sleeperVolumes.Clear();
		triggerVolumes.Clear();
		tileEntities.Clear();
		IndexedBlocks.Clear();
		triggerData.Clear();
		insideDevices.Clear();
		insideDevicesHashSet.Clear();
		NeedsRegeneration = false;
		NeedsDecoration = true;
		NeedsLightDecoration = false;
		NeedsLightCalculation = true;
		hasEntities = false;
		isModified = false;
		InProgressRegeneration = false;
		InProgressSaving = false;
		InProgressCopying = false;
		InProgressDecorating = false;
		InProgressLighting = false;
		InProgressUnloading = false;
		NeedsOnlyCollisionMesh = false;
		IsCollisionMeshGenerated = false;
		SavedInWorldTicks = 0uL;
		MemoryPools.poolCBL.FreeSync(m_BlockLayers);
		chnDensity.FreeLayers();
		chnStability.FreeLayers();
		chnLight.FreeLayers();
		chnDamage.FreeLayers();
		for (int j = 0; j < 1; j++)
		{
			chnTextures[j].FreeLayers();
		}
		chnWater.FreeLayers();
		ResetLights(0);
		Array.Clear(m_HeightMap, 0, m_HeightMap.GetLength(0));
		Array.Clear(m_TerrainHeight, 0, m_TerrainHeight.GetLength(0));
		Array.Clear(m_bTopSoilBroken, 0, m_bTopSoilBroken.GetLength(0));
		Array.Clear(m_Biomes, 0, m_Biomes.GetLength(0));
		Array.Clear(m_NormalX, 0, m_NormalX.GetLength(0));
		Array.Clear(m_NormalY, 0, m_NormalY.GetLength(0));
		Array.Clear(m_NormalZ, 0, m_NormalZ.GetLength(0));
		ResetBiomeIntensity(BiomeIntensity.Default);
		DominantBiome = 0;
		AreaMasterDominantBiome = byte.MaxValue;
		biomeSpawnData = null;
		if (m_DecoBiomeArray != null)
		{
			Array.Clear(m_DecoBiomeArray, 0, m_DecoBiomeArray.GetLength(0));
		}
		ChunkCustomData.Clear();
		bMapDirty = true;
		lock (tickedBlocks)
		{
			tickedBlocks.Clear();
		}
		bEmptyDirty = true;
		StopStabilityCalculation = true;
		waterSimHandle.Reset();
	}

	public void Cleanup()
	{
		waterSimHandle.Reset();
	}

	public bool GetAvailable()
	{
		return IsCollisionMeshGenerated;
	}

	public void ClearNeedsRegenerationAt(int _idx)
	{
		lock (this)
		{
			m_NeedsRegenerationAtY &= ~(1 << _idx);
			NeedsRegenerationDebug = m_NeedsRegenerationAtY;
		}
	}

	public void SetNeedsRegenerationRaw(int _v)
	{
		m_NeedsRegenerationAtY = _v;
	}

	public void load(PooledBinaryReader stream, uint _version)
	{
		read(stream, _version, _bNetworkRead: false);
		isModified = false;
	}

	public void read(PooledBinaryReader stream, uint _version)
	{
		read(stream, _version, _bNetworkRead: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _br, uint _version, bool _bNetworkRead)
	{
		cachedToString = null;
		m_X = _br.ReadInt32();
		m_Y = _br.ReadInt32();
		Z = _br.ReadInt32();
		if (_version > 30)
		{
			SavedInWorldTicks = _br.ReadUInt64();
		}
		LastTimeRandomTicked = SavedInWorldTicks;
		MemoryPools.poolCBL.FreeSync(m_BlockLayers);
		Array.Clear(m_HeightMap, 0, 256);
		if (_version < 28)
		{
			throw new Exception("Chunk version " + _version + " not supported any more!");
		}
		for (int i = 0; i < 64; i++)
		{
			if (_br.ReadBoolean())
			{
				ChunkBlockLayer chunkBlockLayer = MemoryPools.poolCBL.AllocSync(_bReset: false);
				chunkBlockLayer.Read(_br, _version, _bNetworkRead);
				m_BlockLayers[i] = chunkBlockLayer;
				bEmptyDirty = true;
			}
		}
		if (_version < 28)
		{
			ChunkBlockLayerLegacy[] blockLayers = new ChunkBlockLayerLegacy[256];
			chnStability.Convert(blockLayers);
		}
		else if (!_bNetworkRead)
		{
			chnStability.Read(_br, _version, _bNetworkRead);
		}
		_br.Flush();
		recalcIndexedBlocks();
		BinaryFormatter binaryFormatter = null;
		if (_version < 10)
		{
			binaryFormatter = new BinaryFormatter();
			byte[,] array = (byte[,])binaryFormatter.Deserialize(_br.BaseStream);
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					m_HeightMap[j + k * 16] = array[j, k];
				}
			}
		}
		else
		{
			_br.Read(m_HeightMap, 0, 256);
		}
		if (_version >= 7 && _version < 8)
		{
			if (binaryFormatter == null)
			{
				binaryFormatter = new BinaryFormatter();
			}
			byte[,] array2 = (byte[,])binaryFormatter.Deserialize(_br.BaseStream);
			m_TerrainHeight = new byte[array2.GetLength(0) * array2.GetLength(1)];
			for (int l = 0; l < array2.GetLength(0); l++)
			{
				for (int m = 0; m < array2.GetLength(1); m++)
				{
					SetTerrainHeight(l, m, array2[l, m]);
				}
			}
		}
		else if (_version > 21)
		{
			_br.Read(m_TerrainHeight, 0, m_TerrainHeight.Length);
		}
		if (_version > 41)
		{
			_br.Read(m_bTopSoilBroken, 0, 32);
		}
		if (_version > 8 && _version < 15)
		{
			if (binaryFormatter == null)
			{
				binaryFormatter = new BinaryFormatter();
			}
			byte[,] array3 = (byte[,])binaryFormatter.Deserialize(_br.BaseStream);
			m_Biomes = new byte[array3.GetLength(0) * array3.GetLength(1)];
			for (int n = 0; n < array3.GetLength(0); n++)
			{
				for (int num = 0; num < array3.GetLength(1); num++)
				{
					SetBiomeId(n, num, array3[n, num]);
				}
			}
		}
		else
		{
			_br.Read(m_Biomes, 0, 256);
		}
		if (_version > 19)
		{
			_br.Read(m_BiomeIntensities, 0, 1536);
		}
		else
		{
			for (int num2 = 0; num2 < m_BiomeIntensities.Length; num2 += 6)
			{
				BiomeIntensity.Default.ToArray(m_BiomeIntensities, num2);
			}
		}
		if (_version > 23)
		{
			DominantBiome = _br.ReadByte();
		}
		if (_version > 24)
		{
			AreaMasterDominantBiome = _br.ReadByte();
		}
		if (_version > 25)
		{
			int num3 = _br.ReadUInt16();
			ChunkCustomData.Clear();
			for (int num4 = 0; num4 < num3; num4++)
			{
				ChunkCustomData chunkCustomData = new ChunkCustomData();
				chunkCustomData.Read(_br);
				ChunkCustomData.Set(chunkCustomData.key, chunkCustomData);
			}
		}
		if (_version > 22)
		{
			_br.Read(m_NormalX, 0, 256);
		}
		if (_version > 20)
		{
			_br.Read(m_NormalY, 0, 256);
		}
		if (_version > 22)
		{
			_br.Read(m_NormalZ, 0, 256);
		}
		if (_version > 12 && _version < 27)
		{
			throw new Exception("Chunk version " + _version + " not supported any more!");
		}
		chnDensity.Read(_br, _version, _bNetworkRead);
		if (_version < 27)
		{
			SmartArray smartArray = new SmartArray(4, 8, 4);
			smartArray.read(_br);
			SmartArray smartArray2 = new SmartArray(4, 8, 4);
			smartArray2.read(_br);
			chnLight.Convert(smartArray, 0);
			chnLight.Convert(smartArray2, 4);
		}
		else
		{
			chnLight.Read(_br, _version, _bNetworkRead);
		}
		if (_version >= 33 && _version < 36)
		{
			ChunkBlockChannel chunkBlockChannel = new ChunkBlockChannel(0L);
			chunkBlockChannel.Read(_br, _version, _bNetworkRead);
			chunkBlockChannel.Read(_br, _version, _bNetworkRead);
		}
		if (_version >= 36)
		{
			chnDamage.Read(_br, _version, _bNetworkRead);
		}
		switch (_version)
		{
		default:
		{
			for (int num5 = 0; num5 < 1; num5++)
			{
				chnTextures[num5].Read(_br, _version, _bNetworkRead);
			}
			break;
		}
		case 35u:
		case 36u:
		case 37u:
		case 38u:
		case 39u:
		case 40u:
		case 41u:
		case 42u:
		case 43u:
		case 44u:
		case 45u:
		case 46u:
			chnTextures[0].Read(_br, _version, _bNetworkRead);
			break;
		case 0u:
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
		case 8u:
		case 9u:
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 15u:
		case 16u:
		case 17u:
		case 18u:
		case 19u:
		case 20u:
		case 21u:
		case 22u:
		case 23u:
		case 24u:
		case 25u:
		case 26u:
		case 27u:
		case 28u:
		case 29u:
		case 30u:
		case 31u:
		case 32u:
		case 33u:
		case 34u:
			break;
		}
		if (_version >= 46)
		{
			chnWater.Read(_br, _version, _bNetworkRead);
		}
		else if (WaterSimulationNative.Instance.IsInitialized)
		{
			throw new Exception("Serialized data incompatible with new water simulation");
		}
		NeedsDecoration = false;
		NeedsLightCalculation = false;
		if (_version >= 6)
		{
			NeedsLightCalculation = _br.ReadBoolean();
		}
		int num6 = _br.ReadInt32();
		for (int num7 = 0; num7 < 16; num7++)
		{
			entityLists[num7].Clear();
		}
		entityStubs.Clear();
		for (int num8 = 0; num8 < num6; num8++)
		{
			EntityCreationData entityCreationData = new EntityCreationData();
			entityCreationData.read(_br, _bNetworkRead);
			entityStubs.Add(entityCreationData);
		}
		hasEntities = entityStubs.Count > 0;
		if (_version > 13 && _version < 32)
		{
			num6 = _br.ReadInt32();
		}
		num6 = _br.ReadInt32();
		tileEntities.Clear();
		for (int num9 = 0; num9 < num6; num9++)
		{
			TileEntity tileEntity = TileEntity.Instantiate((TileEntityType)_br.ReadInt32(), this);
			if (tileEntity != null)
			{
				tileEntity.read(_br, _bNetworkRead ? TileEntity.StreamModeRead.FromServer : TileEntity.StreamModeRead.Persistency);
				tileEntity.OnReadComplete();
				tileEntities.Set(tileEntity.localChunkPos, tileEntity);
			}
		}
		if (_version > 10 && _version < 43 && !_bNetworkRead)
		{
			_br.ReadUInt16();
			_br.ReadByte();
		}
		if (_version > 33 && _br.ReadBoolean())
		{
			for (int num10 = 0; num10 < 16; num10++)
			{
				_br.ReadUInt16();
			}
		}
		if (!_bNetworkRead && _version == 37)
		{
			byte b = _br.ReadByte();
			for (int num11 = 0; num11 < b; num11++)
			{
				SleeperVolume.Read(_br);
			}
		}
		if (!_bNetworkRead && _version > 37)
		{
			sleeperVolumes.Clear();
			int num12 = _br.ReadByte();
			for (int num13 = 0; num13 < num12; num13++)
			{
				int num14 = _br.ReadInt32();
				if (num14 < 0)
				{
					Log.Error("chunk sleeper volumeId invalid {0}", num14);
				}
				else
				{
					AddSleeperVolumeId(num14);
				}
			}
		}
		if (!_bNetworkRead && _version >= 44)
		{
			triggerVolumes.Clear();
			int num15 = _br.ReadByte();
			for (int num16 = 0; num16 < num15; num16++)
			{
				int num17 = _br.ReadInt32();
				if (num17 < 0)
				{
					Log.Error("chunk trigger volumeId invalid {0}", num17);
				}
				else
				{
					AddTriggerVolumeId(num17);
				}
			}
		}
		if (_version >= 45)
		{
			wallVolumes.Clear();
			int num18 = _br.ReadByte();
			for (int num19 = 0; num19 < num18; num19++)
			{
				int num20 = _br.ReadInt32();
				if (num20 < 0)
				{
					Log.Error("chunk wall volumeId invalid {0}", num20);
				}
				else
				{
					AddWallVolumeId(num20);
				}
			}
		}
		if (_bNetworkRead)
		{
			_br.ReadBoolean();
		}
		lock (tickedBlocks)
		{
			tickedBlocks.Clear();
			for (int num21 = 0; num21 < 64; num21++)
			{
				ChunkBlockLayer chunkBlockLayer2 = m_BlockLayers[num21];
				if (chunkBlockLayer2 == null)
				{
					continue;
				}
				for (int num22 = 0; num22 < 1024; num22++)
				{
					int idAt = chunkBlockLayer2.GetIdAt(num22);
					if (idAt != 0 && Block.BlocksLoaded && idAt < Block.list.Length && Block.list[idAt] != null && Block.list[idAt].IsRandomlyTick && !chunkBlockLayer2.GetAt(num22).ischild)
					{
						int x = num22 % 256 % 16;
						int y = num21 * 4 + num22 / 256;
						int z = num22 % 256 / 16;
						tickedBlocks.Add(ToWorldPos(x, y, z), 0);
					}
				}
			}
		}
		insideDevices.Clear();
		if (_version > 39)
		{
			int num23 = _br.ReadInt16();
			insideDevices.Capacity = num23;
			byte x2 = 0;
			byte z2 = 0;
			int num24 = 0;
			for (int num25 = 0; num25 < num23; num25++)
			{
				if (num24 == 0)
				{
					x2 = _br.ReadByte();
					z2 = _br.ReadByte();
					num24 = _br.ReadByte();
				}
				Vector3b item = new Vector3b(x2, _br.ReadByte(), z2);
				insideDevices.Add(item);
				insideDevicesHashSet.Add(item.GetHashCode());
				num24--;
			}
		}
		if (_version > 40)
		{
			IsInternalBlocksCulled = _br.ReadBoolean();
		}
		if (_version > 42 && !_bNetworkRead)
		{
			triggerData.Clear();
			int num26 = _br.ReadInt16();
			for (int num27 = 0; num27 < num26; num27++)
			{
				Vector3i vector3i = StreamUtils.ReadVector3i(_br);
				BlockTrigger blockTrigger = new BlockTrigger(this);
				blockTrigger.LocalChunkPos = vector3i;
				blockTrigger.Read(_br);
				triggerData.Add(vector3i, blockTrigger);
			}
		}
		if (_bNetworkRead)
		{
			ResetStabilityToBottomMost();
			NeedsLightCalculation = true;
		}
		bMapDirty = true;
		StopStabilityCalculation = false;
	}

	public void save(PooledBinaryWriter stream)
	{
		saveBlockIds();
		write(stream, _bNetworkWrite: false);
		isModified = false;
		SavedInWorldTicks = GameTimer.Instance.ticks;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveBlockIds()
	{
		if (Block.nameIdMapping == null)
		{
			return;
		}
		NameIdMapping nameIdMapping = Block.nameIdMapping;
		lock (nameIdMapping)
		{
			for (int i = 0; i < 256; i += 4)
			{
				int num = i >> 2;
				ChunkBlockLayer chunkBlockLayer = m_BlockLayers[num];
				if (chunkBlockLayer == null)
				{
					Block block = BlockValue.Air.Block;
					nameIdMapping.AddMapping(block.blockID, block.GetBlockName());
				}
				else
				{
					chunkBlockLayer.SaveBlockMappings(nameIdMapping);
				}
			}
		}
	}

	public void write(PooledBinaryWriter stream)
	{
		write(stream, _bNetworkWrite: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(PooledBinaryWriter _bw, bool _bNetworkWrite)
	{
		byte[] array = MemoryPools.poolByte.Alloc(256);
		_bw.Write(m_X);
		_bw.Write(m_Y);
		_bw.Write(m_Z);
		_bw.Write(SavedInWorldTicks);
		for (int i = 0; i < 64; i++)
		{
			bool flag = m_BlockLayers[i] != null;
			_bw.Write(flag);
			if (flag)
			{
				m_BlockLayers[i].Write(_bw, _bNetworkWrite);
			}
		}
		if (!_bNetworkWrite)
		{
			chnStability.Write(_bw, _bNetworkWrite, array);
		}
		_bw.Write(m_HeightMap);
		_bw.Write(m_TerrainHeight);
		_bw.Write(m_bTopSoilBroken);
		_bw.Write(m_Biomes);
		_bw.Write(m_BiomeIntensities);
		_bw.Write(DominantBiome);
		_bw.Write(AreaMasterDominantBiome);
		int num = 0;
		if (_bNetworkWrite)
		{
			for (int j = 0; j < ChunkCustomData.valueList.Count; j++)
			{
				if (ChunkCustomData.valueList[j].isSavedToNetwork)
				{
					num++;
				}
			}
		}
		else
		{
			num = ChunkCustomData.valueList.Count;
		}
		_bw.Write((ushort)num);
		for (int k = 0; k < ChunkCustomData.valueList.Count; k++)
		{
			if (!_bNetworkWrite || ChunkCustomData.valueList[k].isSavedToNetwork)
			{
				ChunkCustomData.valueList[k].Write(_bw);
			}
		}
		_bw.Write(m_NormalX);
		_bw.Write(m_NormalY);
		_bw.Write(m_NormalZ);
		chnDensity.Write(_bw, _bNetworkWrite, array);
		chnLight.Write(_bw, _bNetworkWrite, array);
		chnDamage.Write(_bw, _bNetworkWrite, array);
		for (int l = 0; l < 1; l++)
		{
			chnTextures[l].Write(_bw, _bNetworkWrite, array);
		}
		chnWater.Write(_bw, _bNetworkWrite, array);
		_bw.Write(NeedsLightCalculation);
		int num2 = 0;
		for (int m = 0; m < 16; m++)
		{
			List<Entity> list = entityLists[m];
			for (int n = 0; n < list.Count; n++)
			{
				Entity entity = list[n];
				if (!(entity is EntityVehicle) && !(entity is EntityDrone) && ((!_bNetworkWrite && entity.IsSavedToFile()) || (_bNetworkWrite && entity.IsSavedToNetwork())))
				{
					num2++;
				}
			}
		}
		_bw.Write(num2);
		for (int num3 = 0; num3 < 16; num3++)
		{
			List<Entity> list2 = entityLists[num3];
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				Entity entity2 = list2[num4];
				if (!(entity2 is EntityVehicle) && !(entity2 is EntityDrone) && ((!_bNetworkWrite && entity2.IsSavedToFile()) || (_bNetworkWrite && entity2.IsSavedToNetwork())))
				{
					new EntityCreationData(entity2).write(_bw, _bNetworkWrite);
				}
			}
		}
		_bw.Write(tileEntities.Count);
		for (int num5 = 0; num5 < tileEntities.list.Count; num5++)
		{
			_bw.Write((int)tileEntities.list[num5].GetTileEntityType());
			tileEntities.list[num5].write(_bw, _bNetworkWrite ? TileEntity.StreamModeWrite.ToClient : TileEntity.StreamModeWrite.Persistency);
		}
		_bw.Write(false);
		if (!_bNetworkWrite)
		{
			int count = sleeperVolumes.Count;
			_bw.Write((byte)count);
			for (int num6 = 0; num6 < count; num6++)
			{
				_bw.Write(sleeperVolumes[num6]);
			}
		}
		if (!_bNetworkWrite)
		{
			int count2 = triggerVolumes.Count;
			_bw.Write((byte)count2);
			for (int num7 = 0; num7 < count2; num7++)
			{
				_bw.Write(triggerVolumes[num7]);
			}
		}
		int count3 = wallVolumes.Count;
		_bw.Write((byte)count3);
		for (int num8 = 0; num8 < count3; num8++)
		{
			_bw.Write(wallVolumes[num8]);
		}
		if (_bNetworkWrite)
		{
			_bw.Write(false);
		}
		List<byte> list3 = new List<byte>();
		int num9 = int.MaxValue;
		int num10 = int.MaxValue;
		_bw.Write((short)insideDevices.Count);
		foreach (Vector3b insideDevice in insideDevices)
		{
			if (list3.Count > 254 || num9 != insideDevice.x || num10 != insideDevice.z)
			{
				if (list3.Count > 0)
				{
					_bw.Write((byte)num9);
					_bw.Write((byte)num10);
					_bw.Write((byte)list3.Count);
					for (int num11 = 0; num11 < list3.Count; num11++)
					{
						_bw.Write(list3[num11]);
					}
					list3.Clear();
				}
				num9 = insideDevice.x;
				num10 = insideDevice.z;
			}
			list3.Add(insideDevice.y);
		}
		if (list3.Count > 0)
		{
			_bw.Write((byte)num9);
			_bw.Write((byte)num10);
			_bw.Write((byte)list3.Count);
			for (int num12 = 0; num12 < list3.Count; num12++)
			{
				_bw.Write(list3[num12]);
			}
		}
		_bw.Write(IsInternalBlocksCulled);
		if (!_bNetworkWrite)
		{
			int count4 = triggerData.Count;
			_bw.Write((short)count4);
			for (int num13 = 0; num13 < count4; num13++)
			{
				StreamUtils.Write(_bw, triggerData.list[num13].LocalChunkPos);
				triggerData.list[num13].Write(_bw);
			}
		}
		MemoryPools.poolByte.Free(array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recalcIndexedBlocks()
	{
		IndexedBlocks.Clear();
		for (int i = 0; i < 64; i++)
		{
			m_BlockLayers[i]?.AddIndexedBlocks(i, IndexedBlocks);
		}
	}

	public void AddEntityStub(EntityCreationData _ecd)
	{
		entityStubs.Add(_ecd);
	}

	public BlockEntityData GetBlockEntity(Vector3i _worldPos)
	{
		blockEntityStubs.dict.TryGetValue(GameUtils.Vector3iToUInt64(_worldPos), out var value);
		return value;
	}

	public BlockEntityData GetBlockEntity(Transform _transform)
	{
		for (int i = 0; i < blockEntityStubs.list.Count; i++)
		{
			if (blockEntityStubs.list[i].transform == _transform)
			{
				return blockEntityStubs.list[i];
			}
		}
		return null;
	}

	public void AddEntityBlockStub(BlockEntityData _ecd)
	{
		ulong key = GameUtils.Vector3iToUInt64(_ecd.pos);
		if (blockEntityStubs.dict.TryGetValue(key, out var value))
		{
			blockEntityStubsToRemove.Add(value);
		}
		blockEntityStubs.Set(key, _ecd);
	}

	public void RemoveEntityBlockStub(Vector3i _pos)
	{
		ulong key = GameUtils.Vector3iToUInt64(_pos);
		if (blockEntityStubs.dict.TryGetValue(key, out var value))
		{
			blockEntityStubsToRemove.Add(value);
			blockEntityStubs.Remove(key);
		}
		else
		{
			Vector3i vector3i = _pos;
			Log.Warning("Entity block on pos " + vector3i.ToString() + " not found!");
		}
	}

	public int EnableEntityBlocks(bool _on, string _name)
	{
		_name = _name.ToLower();
		int num = 0;
		for (int i = 0; i < blockEntityStubs.list.Count; i++)
		{
			BlockEntityData blockEntityData = blockEntityStubs.list[i];
			if ((bool)blockEntityData.transform)
			{
				string text = blockEntityData.transform.name.ToLower();
				if (_name.Length == 0 || text.Contains(_name))
				{
					blockEntityData.transform.gameObject.SetActive(_on);
					num++;
				}
			}
		}
		return num;
	}

	public void AddInsideDevicePosition(int _blockX, int _blockY, int _blockZ, BlockValue _bv)
	{
		Vector3b item = new Vector3b(_blockX, _blockY, _blockZ);
		insideDevices.Add(item);
		insideDevicesHashSet.Add(item.GetHashCode());
		IsInternalBlocksCulled = true;
	}

	public int EnableInsideBlockEntities(bool _bOn)
	{
		int num = 0;
		foreach (Vector3b insideDevice in insideDevices)
		{
			ulong key = GameUtils.Vector3iToUInt64(ToWorldPos(insideDevice.ToVector3i()));
			if (blockEntityStubs.dict.TryGetValue(key, out var value) && value.bHasTransform)
			{
				value.transform.gameObject.SetActive(_bOn);
				num++;
			}
		}
		return num;
	}

	public void ResetStability()
	{
		chnStability.Clear(-1L);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 256; k++)
				{
					int blockId = GetBlockId(i, k, j);
					if (blockId == 0)
					{
						break;
					}
					if (!Block.list[blockId].StabilitySupport)
					{
						chnStability.Set(i, k, j, 1L);
						break;
					}
					chnStability.Set(i, k, j, 15L);
				}
			}
		}
	}

	public void ResetStabilityToBottomMost()
	{
		chnStability.Clear(-1L);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int k;
				for (k = 0; k < 256; k++)
				{
					int blockId = GetBlockId(j, k, i);
					if (blockId != 0 && Block.list[blockId].StabilitySupport)
					{
						break;
					}
				}
				for (; k < 256; k++)
				{
					int blockId2 = GetBlockId(j, k, i);
					if (blockId2 == 0)
					{
						break;
					}
					if (!Block.list[blockId2].StabilitySupport)
					{
						chnStability.Set(j, k, i, 1L);
						break;
					}
					chnStability.Set(j, k, i, 15L);
				}
			}
		}
	}

	public void RefreshSunlight()
	{
		chnLight.SetHalf(_bSetUpperHalf: false, 15);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int num = 15;
				bool flag = true;
				int num2;
				for (num2 = 255; num2 >= 0; num2--)
				{
					int blockId = GetBlockId(i, num2, j);
					if (flag)
					{
						if (blockId == 0)
						{
							continue;
						}
						flag = false;
					}
					Block block = Block.list[blockId];
					bool flag2 = block.shape.IsTerrain();
					if (!flag2)
					{
						num -= block.lightOpacity;
						if (num <= 0)
						{
							break;
						}
					}
					chnLight.Set(i, num2, j, (byte)num);
					if (flag2)
					{
						num -= block.lightOpacity;
						if (num <= 0)
						{
							break;
						}
					}
				}
				for (num2--; num2 >= 0; num2--)
				{
					chnLight.Set(i, num2, j, 0L);
				}
			}
		}
		isModified = true;
	}

	public void SetFullSunlight()
	{
		chnLight.SetHalf(_bSetUpperHalf: false, 15);
	}

	public void CopyLightsFrom(Chunk _other)
	{
		chnLight.CopyFrom(_other.chnLight);
		isModified = true;
	}

	public bool CanMobsSpawnAtPos(int _x, int _y, int _z, bool _ignoreCanMobsSpawnOn = false, bool _checkWater = true)
	{
		if (_y < 2 || _y > 251)
		{
			return false;
		}
		if (IsTraderArea(_x, _z))
		{
			return false;
		}
		if (_checkWater || !IsWater(_x, _y - 1, _z))
		{
			Block block = GetBlockNoDamage(_x, _y - 1, _z).Block;
			if (!_ignoreCanMobsSpawnOn && !block.CanMobsSpawnOn)
			{
				return false;
			}
			if (!block.IsCollideMovement)
			{
				return false;
			}
		}
		Block block2 = GetBlockNoDamage(_x, _y, _z).Block;
		if (!block2.IsCollideMovement || !block2.shape.IsSolidSpace)
		{
			Block block3 = GetBlockNoDamage(_x, _y + 1, _z).Block;
			if ((!block3.IsCollideMovement || !block3.shape.IsSolidSpace) && (!_checkWater || !IsWater(_x, _y, _z)))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanSleeperSpawnAtPos(int _x, int _y, int _z, bool _checkBelow)
	{
		if (_checkBelow && !GetBlockNoDamage(_x, _y - 1, _z).Block.IsCollideMovement)
		{
			return false;
		}
		Block block = GetBlockNoDamage(_x, _y, _z).Block;
		if (block.IsCollideMovement || block.shape.IsSolidSpace)
		{
			return false;
		}
		return true;
	}

	public bool CanPlayersSpawnAtPos(int _x, int _y, int _z, bool _allowOnAirPos = false)
	{
		if (_y < 2 || _y > 251)
		{
			return false;
		}
		Block block = GetBlockNoDamage(_x, _y - 1, _z).Block;
		if (!block.CanPlayersSpawnOn)
		{
			return false;
		}
		Block block2 = GetBlockNoDamage(_x, _y, _z).Block;
		Block block3 = GetBlockNoDamage(_x, _y + 1, _z).Block;
		if (((_allowOnAirPos && block.blockID == 0) || block.IsCollideMovement) && (!block2.IsCollideMovement || !block2.shape.IsSolidSpace) && !IsWater(_x, _y, _z) && (!block3.IsCollideMovement || !block3.shape.IsSolidSpace))
		{
			return true;
		}
		return false;
	}

	public bool IsPositionOnTerrain(int _x, int _y, int _z)
	{
		if (_y < 1)
		{
			return false;
		}
		return GetBlockNoDamage(_x, _y - 1, _z).Block.shape.IsTerrain();
	}

	public bool FindRandomTopSoilPoint(World _world, out int x, out int y, out int z, int numTrys)
	{
		x = 0;
		y = 0;
		z = 0;
		while (numTrys-- > 0)
		{
			x = _world.GetGameRandom().RandomRange(15);
			z = _world.GetGameRandom().RandomRange(15);
			y = GetHeight(x, z);
			if (y >= 2 && CanMobsSpawnAtPos(x, y, z))
			{
				x += m_X * 16;
				y++;
				z += m_Z * 16;
				return true;
			}
		}
		return false;
	}

	public bool FindRandomCavePoint(World _world, out int x, out int y, out int z, int numTrys, int relMinY)
	{
		x = 0;
		y = 0;
		z = 0;
		while (numTrys-- > 0)
		{
			x = _world.GetGameRandom().RandomRange(15);
			z = _world.GetGameRandom().RandomRange(15);
			int num = (y = GetHeight(x, z));
			while (y > num - relMinY && y > 2)
			{
				if (CanMobsSpawnAtPos(x, y, z))
				{
					x += m_X * 16;
					y++;
					z += m_Z * 16;
					return true;
				}
				y--;
			}
		}
		return false;
	}

	public bool FindSpawnPointAtXZ(int x, int z, out int y, int _maxLightV, int _darknessV, int startY, int endY, bool _bIgnoreCanMobsSpawnOn = false)
	{
		endY = Utils.FastClamp(endY, 1, 255);
		startY = Utils.FastClamp(startY - 1, 1, 255);
		y = endY;
		while (y > startY)
		{
			if (GetLightValue(x, y, z, _darknessV) <= _maxLightV)
			{
				if (CanMobsSpawnAtPos(x, y, z, _bIgnoreCanMobsSpawnOn))
				{
					y++;
					return true;
				}
				y--;
			}
		}
		return false;
	}

	public float GetLightBrightness(int x, int y, int z, int _ss)
	{
		return (float)GetLightValue(x, y, z, _ss) / 15f;
	}

	public int GetLightValue(int x, int y, int z, int _darknessValue)
	{
		int light = GetLight(x, y, z, LIGHT_TYPE.SUN);
		light -= _darknessValue;
		if (light == 15)
		{
			return light;
		}
		int light2 = GetLight(x, y, z, LIGHT_TYPE.BLOCK);
		if (light > light2)
		{
			return light;
		}
		return light2;
	}

	public byte GetLight(int x, int y, int z, LIGHT_TYPE type)
	{
		x &= 0xF;
		z &= 0xF;
		int num = chnLight.GetByte(x, y, z);
		if (type == LIGHT_TYPE.SUN)
		{
			return (byte)(num & 0xF);
		}
		return (byte)(num >> 4);
	}

	public void SetLight(int x, int y, int z, byte intensity, LIGHT_TYPE type)
	{
		x &= 0xF;
		z &= 0xF;
		int num = chnLight.GetByte(x, y, z);
		int num2 = intensity;
		switch (type)
		{
		case LIGHT_TYPE.SUN:
			num2 |= num & 0xF0;
			break;
		case LIGHT_TYPE.BLOCK:
			num2 = (num2 << 4) | (num & 0xF);
			break;
		}
		if (num2 != num)
		{
			chnLight.Set(x, y, z, (byte)num2);
			NeedsRegenerationAt = y;
		}
		isModified = true;
	}

	public void CheckSameLight()
	{
		chnLight.CheckSameValue();
	}

	public void CheckSameStability()
	{
		chnStability.CheckSameValue();
	}

	public static bool IsNeighbourChunksDecorated(Chunk[] _neighbours)
	{
		foreach (Chunk chunk in _neighbours)
		{
			if (chunk == null || chunk.NeedsDecoration)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsNeighbourChunksLit(Chunk[] _neighbours)
	{
		foreach (Chunk chunk in _neighbours)
		{
			if (chunk == null || chunk.NeedsLightCalculation)
			{
				return false;
			}
		}
		return true;
	}

	public Vector3i GetWorldPos()
	{
		return new Vector3i(m_X << 4, m_Y << 8, m_Z << 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetBlockWorldPosX(int _x)
	{
		return (m_X << 4) + _x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetBlockWorldPosZ(int _z)
	{
		return (m_Z << 4) + _z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetHeight(int _x, int _z)
	{
		return m_HeightMap[_x + _z * 16];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetHeight(int _x, int _z, byte _h)
	{
		m_HeightMap[_x + _z * 16] = _h;
	}

	public byte GetMaxHeight()
	{
		byte b = 0;
		for (int num = m_HeightMap.Length - 1; num >= 0; num--)
		{
			byte b2 = m_HeightMap[num];
			if (b2 > b)
			{
				b = b2;
			}
		}
		return b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetTerrainHeight(int _x, int _z)
	{
		return m_TerrainHeight[_x + _z * 16];
	}

	public void SetTerrainHeight(int _x, int _z, byte _h)
	{
		m_TerrainHeight[_x + _z * 16] = _h;
	}

	public byte GetTopMostTerrainHeight()
	{
		byte b = 0;
		for (int i = 0; i < m_TerrainHeight.Length; i++)
		{
			if (m_TerrainHeight[i] > b)
			{
				b = m_TerrainHeight[i];
			}
		}
		return b;
	}

	public bool IsTopSoil(int _x, int _z)
	{
		int num = (_x + _z * 16) / 8;
		int num2 = (_x + _z * 16) % 8;
		return (m_bTopSoilBroken[num] & (1 << num2)) == 0;
	}

	public void SetTopSoilBroken(int _x, int _z)
	{
		int num = (_x + _z * 16) / 8;
		int num2 = (_x + _z * 16) % 8;
		int num3 = m_bTopSoilBroken[num];
		num3 |= 1 << num2;
		m_bTopSoilBroken[num] = (byte)num3;
	}

	public BlockValue GetBlock(Vector3i _pos)
	{
		BlockValue result = BlockValue.Air;
		try
		{
			ChunkBlockLayer chunkBlockLayer = m_BlockLayers[_pos.y >> 2];
			if (chunkBlockLayer != null)
			{
				result = chunkBlockLayer.GetAt(_pos.x, _pos.y, _pos.z);
			}
		}
		catch (IndexOutOfRangeException)
		{
			Log.Error("GetBlock failed: _y = " + _pos.y + ", len = " + m_BlockLayers.Length + " (chunk " + m_X + "/" + m_Z + ")");
			throw;
		}
		result.damage = GetDamage(_pos.x, _pos.y, _pos.z);
		return result;
	}

	public BlockValue GetBlock(int _x, int _y, int _z)
	{
		if (IsInternalBlocksCulled && isInside(_x, _y, _z))
		{
			if (bvPOIFiller.isair)
			{
				bvPOIFiller = new BlockValue((uint)Block.GetBlockByName(Constants.cPOIFillerBlock).blockID);
			}
			return bvPOIFiller;
		}
		BlockValue result = BlockValue.Air;
		try
		{
			ChunkBlockLayer chunkBlockLayer = m_BlockLayers[_y >> 2];
			if (chunkBlockLayer != null)
			{
				result = chunkBlockLayer.GetAt(_x, _y, _z);
			}
		}
		catch (IndexOutOfRangeException)
		{
			Log.Error("GetBlock failed: _y = " + _y + ", len = " + m_BlockLayers.Length + " (chunk " + m_X + "/" + m_Z + ")");
			throw;
		}
		result.damage = GetDamage(_x, _y, _z);
		return result;
	}

	public BlockValue GetBlockNoDamage(int _x, int _y, int _z)
	{
		BlockValue result = BlockValue.Air;
		try
		{
			ChunkBlockLayer chunkBlockLayer = m_BlockLayers[_y >> 2];
			if (chunkBlockLayer != null)
			{
				result = chunkBlockLayer.GetAt(_x, _y, _z);
			}
		}
		catch (IndexOutOfRangeException)
		{
			Log.Error("GetBlockNoDamage failed: _y = " + _y + ", len = " + m_BlockLayers.Length + " (chunk " + m_X + "/" + m_Z + ")");
			throw;
		}
		return result;
	}

	public void GetBlockColumn(int _x, int _y, int _z, BlockValue[] _blocks)
	{
		try
		{
			int num = _blocks.Length;
			for (int i = 0; i < num; i++)
			{
				BlockValue blockValue = BlockValue.Air;
				ChunkBlockLayer chunkBlockLayer = m_BlockLayers[_y >> 2];
				if (chunkBlockLayer != null)
				{
					blockValue = chunkBlockLayer.GetAt(_x, _y, _z);
					blockValue.damage = GetDamage(_x, _y, _z);
				}
				_blocks[i] = blockValue;
				_y++;
			}
		}
		catch (IndexOutOfRangeException)
		{
			Log.Error("GetBlockColumn failed: _y = " + _y + ", len = " + m_BlockLayers.Length + " (chunk " + m_X + "/" + m_Z + ")");
			throw;
		}
	}

	public int GetBlockId(int _x, int _y, int _z)
	{
		return m_BlockLayers[_y >> 2]?.GetIdAt(_x, _y, _z) ?? 0;
	}

	public void CopyMeshDataFrom(Chunk _other)
	{
		for (int i = 0; i < m_BlockLayers.Length; i++)
		{
			if (_other.m_BlockLayers[i] == null)
			{
				if (m_BlockLayers[i] != null)
				{
					MemoryPools.poolCBL.FreeSync(m_BlockLayers[i]);
					m_BlockLayers[i] = null;
				}
			}
			else
			{
				if (m_BlockLayers[i] == null)
				{
					m_BlockLayers[i] = MemoryPools.poolCBL.AllocSync(_bReset: true);
				}
				m_BlockLayers[i].CopyFrom(_other.m_BlockLayers[i]);
			}
		}
		chnDensity.CopyFrom(_other.chnDensity);
		chnDamage.CopyFrom(_other.chnDamage);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetBiomeId(int _x, int _z)
	{
		return m_Biomes[_x + _z * 16];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBiomeId(int _x, int _z, byte _biomeId)
	{
		m_Biomes[_x + _z * 16] = _biomeId;
	}

	public void FillBiomeId(byte _biomeId)
	{
		for (int i = 0; i < m_Biomes.Length; i++)
		{
			m_Biomes[i] = _biomeId;
		}
	}

	public BiomeIntensity GetBiomeIntensity(int _x, int _z)
	{
		if (m_BiomeIntensities == null)
		{
			return BiomeIntensity.Default;
		}
		return new BiomeIntensity(m_BiomeIntensities, (_x + _z * 16) * 6);
	}

	public void CalcBiomeIntensity(Chunk[] _neighbours)
	{
		int[] array = new int[50];
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				Array.Clear(array, 0, array.Length);
				for (int k = -16; k < 16; k++)
				{
					int num = i + k;
					int num2 = j + k;
					Chunk chunk = this;
					if (num < 0)
					{
						chunk = ((num2 < 0) ? _neighbours[5] : ((num2 < 16) ? _neighbours[1] : _neighbours[6]));
					}
					else if (num >= 16)
					{
						chunk = ((num2 < 0) ? _neighbours[3] : ((num2 < 16) ? _neighbours[0] : _neighbours[4]));
					}
					else if (num2 >= 16)
					{
						chunk = _neighbours[2];
					}
					else if (num2 < 0)
					{
						chunk = _neighbours[3];
					}
					int biomeId = chunk.GetBiomeId(World.toBlockXZ(num), World.toBlockXZ(num2));
					if (biomeId >= 0 && biomeId < array.Length)
					{
						array[biomeId]++;
					}
				}
				BiomeIntensity.FromArray(array).ToArray(m_BiomeIntensities, (i + j * 16) * 6);
			}
		}
	}

	public void CalcDominantBiome()
	{
		int[] array = new int[50];
		for (int i = 0; i < m_Biomes.Length; i++)
		{
			array[m_Biomes[i]]++;
		}
		int num = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > num)
			{
				DominantBiome = (byte)j;
				num = array[j];
			}
		}
	}

	public void ResetBiomeIntensity(BiomeIntensity _v)
	{
		for (int i = 0; i < m_BiomeIntensities.Length; i += 6)
		{
			_v.ToArray(m_BiomeIntensities, i);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetStability(int _x, int _y, int _z)
	{
		return (byte)chnStability.Get(_x, _y, _z);
	}

	public void SetStability(int _x, int _y, int _z, byte _v)
	{
		chnStability.Set(_x, _y, _z, _v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDensity(int _x, int _y, int _z, sbyte _density)
	{
		chnDensity.Set(_x, _y, _z, (byte)_density);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public sbyte GetDensity(int _x, int _y, int _z)
	{
		return (sbyte)chnDensity.Get(_x, _y, _z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasSameDensityValue(int _y)
	{
		return chnDensity.HasSameValue(_y);
	}

	public sbyte GetSameDensityValue(int _y)
	{
		if (_y < 0)
		{
			return MarchingCubes.DensityTerrain;
		}
		if (_y >= 256)
		{
			return MarchingCubes.DensityAir;
		}
		return (sbyte)chnDensity.GetSameValue(_y);
	}

	public void CheckSameDensity()
	{
		chnDensity.CheckSameValue();
	}

	public bool IsOnlyTerrain(int _y)
	{
		int idx = _y >> 2;
		return IsOnlyTerrainLayer(idx);
	}

	public bool IsOnlyTerrainLayer(int _idx)
	{
		if (_idx < 0 || _idx >= m_BlockLayers.Length)
		{
			return true;
		}
		if (m_BlockLayers[_idx] != null)
		{
			return m_BlockLayers[_idx].IsOnlyTerrain();
		}
		return false;
	}

	public void CheckOnlyTerrain()
	{
		for (int i = 0; i < m_BlockLayers.Length; i++)
		{
			m_BlockLayers[i]?.CheckOnlyTerrain();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long GetTextureFull(int _x, int _y, int _z, int channel = 0)
	{
		if (!IgnorePaintTextures)
		{
			return chnTextures[channel].Get(_x, _y, _z);
		}
		return 0L;
	}

	public TextureFullArray GetTextureFullArray(int _x, int _y, int _z, bool applyIgnore = true)
	{
		TextureFullArray result = default(TextureFullArray);
		for (int i = 0; i < 1; i++)
		{
			result[i] = ((applyIgnore && IgnorePaintTextures) ? 0 : chnTextures[i].Get(_x, _y, _z));
		}
		return result;
	}

	public void SetTextureFull(int _x, int _y, int _z, long _texturefull, int channel = 0)
	{
		chnTextures[channel].Set(_x, _y, _z, _texturefull);
		isModified = true;
	}

	public TextureFullArray GetSetTextureFullArray(int _x, int _y, int _z, TextureFullArray _texturefullArray)
	{
		TextureFullArray result = default(TextureFullArray);
		for (int i = 0; i < 1; i++)
		{
			result[i] = chnTextures[i].GetSet(_x, _y, _z, _texturefullArray[i]);
		}
		isModified = true;
		return result;
	}

	public int GetBlockFaceTexture(int _x, int _y, int _z, BlockFace _face, int channel)
	{
		return (int)((chnTextures[channel].Get(_x, _y, _z) >> (int)_face * 8) & 0xFF);
	}

	public long SetBlockFaceTexture(int _x, int _y, int _z, BlockFace _face, int _texture, int channel = 0)
	{
		long num;
		long result = (num = chnTextures[channel].Get(_x, _y, _z));
		int num2 = (int)_face * 8;
		num &= ~(255L << num2);
		num |= (long)(_texture & 0xFF) << num2;
		chnTextures[channel].Set(_x, _y, _z, num);
		isModified = true;
		return result;
	}

	public static int Value64FullToIndex(long _valueFull, BlockFace _blockFace)
	{
		return (int)((_valueFull >> (int)_blockFace * 8) & 0xFF);
	}

	public static long TextureIdxToTextureFullValue64(int _idx)
	{
		long num = _idx;
		return ((num & 0xFF) << 40) | ((num & 0xFF) << 32) | ((num & 0xFF) << 24) | ((num & 0xFF) << 16) | ((num & 0xFF) << 8) | (num & 0xFF);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDamage(int _x, int _y, int _z, int _damage)
	{
		chnDamage.Set(_x, _y, _z, _damage);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetDamage(int _x, int _y, int _z)
	{
		return (int)chnDamage.Get(_x, _y, _z);
	}

	public bool IsAir(int _x, int _y, int _z)
	{
		if (!IsWater(_x, _y, _z))
		{
			return GetBlockNoDamage(_x, _y, _z).isair;
		}
		return false;
	}

	public void ClearWater()
	{
		chnWater.Clear(0L);
	}

	public bool IsWater(int _x, int _y, int _z)
	{
		return GetWater(_x, _y, _z).HasMass();
	}

	public WaterValue GetWater(int _x, int _y, int _z)
	{
		return WaterValue.FromRawData(chnWater.Get(_x, _y, _z));
	}

	public void SetWater(int _x, int _y, int _z, WaterValue _data)
	{
		SetWaterRaw(_x, _y, _z, _data);
		waterSimHandle.WakeNeighbours(_x, _y, _z);
	}

	public void SetWaterRaw(int _x, int _y, int _z, WaterValue _data)
	{
		if (!WaterUtils.CanWaterFlowThrough(GetBlockNoDamage(_x, _y, _z)))
		{
			_data.SetMass(0);
		}
		chnWater.Set(_x, _y, _z, _data.RawData);
		bEmptyDirty = true;
		bMapDirty = true;
		isModified = true;
		waterSimHandle.SetWaterMass(_x, _y, _z, _data.GetMass());
		if (_data.HasMass())
		{
			int num = ChunkBlockLayerLegacy.CalcOffset(_x, _z);
			if (m_HeightMap[num] < _y)
			{
				m_HeightMap[num] = (byte)_y;
			}
		}
	}

	public void SetWaterSimUpdate(int _x, int _y, int _z, WaterValue _data, out WaterValue _lastData)
	{
		if (!WaterUtils.CanWaterFlowThrough(GetBlockNoDamage(_x, _y, _z)))
		{
			_lastData = WaterValue.FromRawData(chnWater.Get(_x, _y, _z));
			return;
		}
		long set = chnWater.GetSet(_x, _y, _z, _data.RawData);
		_lastData = WaterValue.FromRawData(set);
		if (_lastData.GetMass() == _data.GetMass())
		{
			return;
		}
		GameManager.Instance.World.HandleWaterLevelChanged(ToWorldPos(_x, _y, _z), _data.GetMassPercent());
		bEmptyDirty = true;
		bMapDirty = true;
		isModified = true;
		if (_data.HasMass())
		{
			int num = ChunkBlockLayerLegacy.CalcOffset(_x, _z);
			if (m_HeightMap[num] < _y)
			{
				m_HeightMap[num] = (byte)_y;
			}
		}
	}

	public bool IsEmpty()
	{
		if (bEmptyDirty)
		{
			bEmpty = true;
			for (int i = 0; i < m_BlockLayers.Length; i++)
			{
				if (m_BlockLayers[i] != null)
				{
					bEmpty = false;
					break;
				}
			}
			if (bEmpty)
			{
				bEmpty = chnWater.IsDefault();
			}
			bEmptyDirty = false;
		}
		return bEmpty;
	}

	public bool IsEmpty(int _y)
	{
		int idx = _y >> 2;
		return IsEmptyLayer(idx);
	}

	public bool IsEmptyLayer(int _idx)
	{
		if ((uint)_idx >= m_BlockLayers.Length)
		{
			return true;
		}
		if (m_BlockLayers[_idx] == null)
		{
			return chnWater.IsDefaultLayer(_idx);
		}
		return false;
	}

	public int RecalcHeights()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int num2 = ChunkBlockLayerLegacy.CalcOffset(j, i);
				m_HeightMap[num2] = 0;
				for (int num3 = 255; num3 >= 0; num3--)
				{
					ChunkBlockLayer chunkBlockLayer = m_BlockLayers[num3 >> 2];
					if ((chunkBlockLayer != null && !chunkBlockLayer.GetAt(j, num3, i).isair) || IsWater(j, num3, i))
					{
						m_HeightMap[num2] = (byte)num3;
						num = Utils.FastMax(num, num3);
						break;
					}
				}
			}
		}
		return num;
	}

	public byte RecalcHeightAt(int _x, int _yMaxStart, int _z)
	{
		int num = ChunkBlockLayerLegacy.CalcOffset(_x, _z);
		for (int num2 = _yMaxStart; num2 >= 0; num2--)
		{
			ChunkBlockLayer chunkBlockLayer = m_BlockLayers[num2 >> 2];
			if ((chunkBlockLayer != null && !chunkBlockLayer.GetAt(_x, num2, _z).isair) || IsWater(_x, num2, _z))
			{
				m_HeightMap[num] = (byte)num2;
				return (byte)num2;
			}
		}
		return 0;
	}

	public BlockValue SetBlock(WorldBase _world, int x, int y, int z, BlockValue _blockValue, bool _notifyAddChange = true, bool _notifyRemove = true, bool _fromReset = false, bool _poiOwned = false, int _changedByEntityId = -1)
	{
		Vector3i vector3i = new Vector3i((m_X << 4) + x, y, (m_Z << 4) + z);
		BlockValue blockValue = SetBlockRaw(x, y, z, _blockValue);
		bool flag = !blockValue.isair && _blockValue.isair;
		if (flag)
		{
			waterSimHandle.WakeNeighbours(x, y, z);
			if (blockValue.Block.StabilitySupport)
			{
				MultiBlockManager.Instance.SetOversizedStabilityDirty(vector3i);
			}
		}
		if (!_blockValue.ischild)
		{
			MultiBlockManager.Instance.UpdateTrackedBlockData(vector3i, _blockValue, _poiOwned);
		}
		_blockValue = GetBlock(x, y, z);
		if (_notifyRemove && !blockValue.isair && blockValue.type != _blockValue.type)
		{
			blockValue.Block?.OnBlockRemoved(_world, this, vector3i, blockValue);
		}
		if (_notifyAddChange)
		{
			Block block = _blockValue.Block;
			if (block != null)
			{
				if (blockValue.type != _blockValue.type)
				{
					if (!_blockValue.isair)
					{
						PlatformUserIdentifierAbs addedByPlayer = null;
						if (_changedByEntityId != -1)
						{
							addedByPlayer = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_changedByEntityId).PrimaryId;
						}
						block.OnBlockAdded(_world, this, vector3i, _blockValue, addedByPlayer);
					}
				}
				else
				{
					block.OnBlockValueChanged(_world, this, 0, vector3i, blockValue, _blockValue);
					if (_fromReset)
					{
						block.OnBlockReset(_world, this, vector3i, _blockValue);
					}
				}
			}
		}
		if (flag)
		{
			RemoveBlockTrigger(new Vector3i(x, y, z));
			GameEventManager.Current.BlockRemoved(vector3i);
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !GameManager.Instance.IsEditMode() && BlockLimitTracker.instance != null && !blockValue.Equals(_blockValue))
		{
			BlockLimitTracker.instance.TryRemoveOrReplaceBlock(blockValue, _blockValue, vector3i);
			if (!flag)
			{
				BlockLimitTracker.instance.TryAddTrackedBlock(_blockValue, vector3i, _changedByEntityId);
			}
			BlockLimitTracker.instance.ServerUpdateClients();
		}
		return blockValue;
	}

	public BlockValue SetBlockRaw(int _x, int _y, int _z, BlockValue _blockValue)
	{
		if ((uint)_y >= 255u)
		{
			return BlockValue.Air;
		}
		Block block = _blockValue.Block;
		if (block == null)
		{
			return BlockValue.Air;
		}
		if (_blockValue.isWater)
		{
			BlockValue obj = m_BlockLayers[_y >> 2]?.GetAt(_x, _y, _z) ?? BlockValue.Air;
			if (!WaterUtils.CanWaterFlowThrough(obj))
			{
				SetBlockRaw(_x, _y, _z, BlockValue.Air);
			}
			SetWater(_x, _y, _z, WaterValue.Full);
			return obj;
		}
		if (!WaterUtils.CanWaterFlowThrough(_blockValue))
		{
			SetWater(_x, _y, _z, WaterValue.Empty);
		}
		waterSimHandle.SetVoxelSolid(_x, _y, _z, BlockFaceFlags.RotateFlags(block.WaterFlowMask, _blockValue.rotation));
		BlockValue result = BlockValue.Air;
		int num = _y >> 2;
		ChunkBlockLayer chunkBlockLayer = m_BlockLayers[num];
		if (chunkBlockLayer != null)
		{
			int offs = ChunkBlockLayer.CalcOffset(_x, _y, _z);
			result = chunkBlockLayer.GetAt(offs);
			chunkBlockLayer.SetAt(offs, _blockValue.rawData);
			if (!result.ischild)
			{
				result.damage = GetDamage(_x, _y, _z);
			}
		}
		else if (!_blockValue.isair)
		{
			chunkBlockLayer = MemoryPools.poolCBL.AllocSync(_bReset: true);
			m_BlockLayers[num] = chunkBlockLayer;
			chunkBlockLayer.SetAt(_x, _y, _z, _blockValue.rawData);
		}
		if (!_blockValue.ischild)
		{
			SetDamage(_x, _y, _z, _blockValue.damage);
		}
		Block block2 = result.Block;
		if (result.type != _blockValue.type)
		{
			if (!result.ischild && block2.IndexName != null && IndexedBlocks.ContainsKey(block2.IndexName))
			{
				IndexedBlocks[block2.IndexName].Remove(new Vector3i(_x, _y, _z));
				if (IndexedBlocks[block2.IndexName].Count == 0)
				{
					IndexedBlocks.Remove(block2.IndexName);
				}
			}
			if (!_blockValue.ischild && block.IndexName != null && block.FilterIndexType(_blockValue))
			{
				if (!IndexedBlocks.ContainsKey(block.IndexName))
				{
					IndexedBlocks[block.IndexName] = new List<Vector3i>();
				}
				IndexedBlocks[block.IndexName].Add(new Vector3i(_x, _y, _z));
			}
		}
		int num2 = ChunkBlockLayerLegacy.CalcOffset(_x, _z);
		if (_blockValue.isair)
		{
			if (m_HeightMap[num2] == _y)
			{
				RecalcHeightAt(_x, _y - 1, _z);
			}
		}
		else if (m_HeightMap[num2] < _y)
		{
			m_HeightMap[num2] = (byte)_y;
		}
		if (result.isair && !_blockValue.isair && !_blockValue.ischild)
		{
			if (block.IsRandomlyTick)
			{
				lock (tickedBlocks)
				{
					tickedBlocks.Replace(ToWorldPos(_x, _y, _z), 0);
				}
			}
		}
		else if (!result.isair && _blockValue.isair && !result.ischild)
		{
			if (block2.IsRandomlyTick)
			{
				lock (tickedBlocks)
				{
					tickedBlocks.Remove(ToWorldPos(_x, _y, _z));
				}
			}
		}
		else if (block2.IsRandomlyTick && !block.IsRandomlyTick && !result.ischild)
		{
			lock (tickedBlocks)
			{
				tickedBlocks.Remove(ToWorldPos(_x, _y, _z));
			}
		}
		else if (!block2.IsRandomlyTick && block.IsRandomlyTick && !_blockValue.ischild)
		{
			lock (tickedBlocks)
			{
				tickedBlocks.Replace(ToWorldPos(_x, _y, _z), 0);
			}
		}
		bMapDirty = true;
		isModified = true;
		bEmptyDirty = true;
		return result;
	}

	public bool FillBlockRaw(int _heightIncl, BlockValue _blockValue)
	{
		if (_heightIncl >= 255)
		{
			return false;
		}
		if (_blockValue.isair || _blockValue.ischild)
		{
			return false;
		}
		Block block = _blockValue.Block;
		if (block == null)
		{
			return false;
		}
		if (_blockValue.isWater)
		{
			return false;
		}
		if (!IsEmpty())
		{
			return false;
		}
		uint rawData = _blockValue.rawData;
		sbyte density = (block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir);
		int damage = _blockValue.damage;
		int i;
		for (i = 0; i <= _heightIncl - 4; i += 4)
		{
			int num = i >> 2;
			if (m_BlockLayers[num] == null)
			{
				m_BlockLayers[num] = MemoryPools.poolCBL.AllocSync(_bReset: true);
			}
			m_BlockLayers[num].Fill(rawData);
		}
		for (; i <= _heightIncl; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num2 = i >> 2;
					if (m_BlockLayers[num2] == null)
					{
						m_BlockLayers[num2] = MemoryPools.poolCBL.AllocSync(_bReset: true);
					}
					m_BlockLayers[num2].SetAt(j, i, k, rawData);
				}
			}
		}
		List<Vector3i> list = null;
		if (block.IndexName != null)
		{
			if (!IndexedBlocks.ContainsKey(block.IndexName))
			{
				IndexedBlocks[block.IndexName] = new List<Vector3i>();
			}
			list = IndexedBlocks[block.IndexName];
			list.Clear();
		}
		lock (tickedBlocks)
		{
			tickedBlocks.Clear();
			for (i = 0; i <= _heightIncl; i++)
			{
				for (int l = 0; l < 16; l++)
				{
					for (int m = 0; m < 16; m++)
					{
						SetDensity(l, i, m, density);
						SetDamage(l, i, m, damage);
						list?.Add(new Vector3i(l, i, m));
						if (block.IsRandomlyTick)
						{
							tickedBlocks.Replace(ToWorldPos(l, i, m), 0);
						}
					}
				}
			}
		}
		for (int n = 0; n < 16; n++)
		{
			for (int num3 = 0; num3 < 16; num3++)
			{
				int num4 = ChunkBlockLayerLegacy.CalcOffset(n, num3);
				m_HeightMap[num4] = (byte)_heightIncl;
			}
		}
		bMapDirty = true;
		isModified = true;
		bEmptyDirty = true;
		return true;
	}

	public DictionaryKeyList<Vector3i, int> GetTickedBlocks()
	{
		return tickedBlocks;
	}

	public void RemoveTileEntityAt<T>(World world, Vector3i _posInChunk)
	{
		if (tileEntities.dict.TryGetValue(_posInChunk, out var value) && value is T)
		{
			value.IsRemoving = true;
			value.OnRemove(world);
			tileEntities.Remove(_posInChunk);
			value.IsRemoving = false;
		}
		isModified = true;
	}

	public void RemoveAllTileEntities()
	{
		isModified = tileEntities.Count > 0;
		tileEntities.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetHeight(int _blockOffset)
	{
		return m_HeightMap[_blockOffset];
	}

	public void AddTileEntity(TileEntity _te)
	{
		tileEntities.Set(_te.localChunkPos, _te);
	}

	public void RemoveTileEntity(World world, TileEntity _te)
	{
		if (tileEntities.dict.TryGetValue(_te.localChunkPos, out var value) && value != null)
		{
			value.IsRemoving = true;
			value.OnRemove(world);
			tileEntities.Remove(_te.localChunkPos);
			value.IsRemoving = false;
			isModified = true;
		}
	}

	public TileEntity GetTileEntity(Vector3i _blockPosInChunk)
	{
		if (!tileEntities.dict.TryGetValue(_blockPosInChunk, out var value))
		{
			return null;
		}
		return value;
	}

	public DictionaryList<Vector3i, TileEntity> GetTileEntities()
	{
		return tileEntities;
	}

	public void AddSleeperVolumeId(int id)
	{
		if (!sleeperVolumes.Contains(id))
		{
			if (sleeperVolumes.Count < 255)
			{
				sleeperVolumes.Add(id);
			}
			else
			{
				Log.Error("Chunk AddSleeperVolumeId at max");
			}
		}
	}

	public List<int> GetSleeperVolumes()
	{
		return sleeperVolumes;
	}

	public void AddTriggerVolumeId(int id)
	{
		if (!triggerVolumes.Contains(id))
		{
			if (triggerVolumes.Count < 255)
			{
				triggerVolumes.Add(id);
			}
			else
			{
				Log.Error("Chunk AddTriggerVolumeId at max");
			}
		}
	}

	public List<int> GetTriggerVolumes()
	{
		return triggerVolumes;
	}

	public void AddWallVolumeId(int id)
	{
		if (!wallVolumes.Contains(id))
		{
			if (wallVolumes.Count < 255)
			{
				wallVolumes.Add(id);
			}
			else
			{
				Log.Error("Chunk AddWallVolume at max");
			}
		}
	}

	public List<int> GetWallVolumes()
	{
		return wallVolumes;
	}

	public int GetTickRefCount(int _layerIdx)
	{
		if (m_BlockLayers[_layerIdx] == null)
		{
			return 0;
		}
		return m_BlockLayers[_layerIdx].GetTickRefCount();
	}

	public DictionaryList<Vector3i, BlockTrigger> GetBlockTriggers()
	{
		return triggerData;
	}

	public void AddBlockTrigger(BlockTrigger _td)
	{
		triggerData.Set(_td.LocalChunkPos, _td);
		isModified = true;
	}

	public void RemoveBlockTrigger(BlockTrigger _td)
	{
		if (triggerData.dict.TryGetValue(_td.LocalChunkPos, out var value) && value != null)
		{
			triggerData.Remove(_td.LocalChunkPos);
			isModified = true;
		}
	}

	public void RemoveBlockTrigger(Vector3i _blockPos)
	{
		if (triggerData.dict.ContainsKey(_blockPos))
		{
			triggerData.Remove(_blockPos);
			isModified = true;
		}
	}

	public BlockTrigger GetBlockTrigger(Vector3i _blockPosInChunk)
	{
		triggerData.dict.TryGetValue(_blockPosInChunk, out var value);
		return value;
	}

	public void UpdateTick(World _world, bool _bSpawnEnemies)
	{
		ProfilerBegin("TeTick");
		for (int i = 0; i < tileEntities.list.Count; i++)
		{
			tileEntities.list[i].UpdateTick(_world);
		}
		ProfilerEnd();
	}

	public bool IsOpenSkyAbove(int _x, int _y, int _z)
	{
		return _y >= GetHeight(_x, _z);
	}

	public void GetLivingEntitiesInBounds(EntityAlive _excludeEntity, Bounds _aabb, List<EntityAlive> _entityOutputList)
	{
		int num = Utils.Fastfloor((double)(_aabb.min.y - 5f) / 16.0);
		int num2 = Utils.Fastfloor((double)(_aabb.max.y + 5f) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				EntityAlive entityAlive = list[j] as EntityAlive;
				if (!(entityAlive == null) && !(entityAlive == _excludeEntity) && !entityAlive.IsDead() && entityAlive.boundingBox.Intersects(_aabb) && (!(_excludeEntity != null) || _excludeEntity.CanCollideWith(entityAlive)))
				{
					_entityOutputList.Add(entityAlive);
				}
			}
		}
	}

	public void GetEntitiesInBounds(Entity _excludeEntity, Bounds _aabb, List<Entity> _entityOutputList, bool isAlive)
	{
		int num = Utils.Fastfloor((double)(_aabb.min.y - 5f) / 16.0);
		int num2 = Utils.Fastfloor((double)(_aabb.max.y + 5f) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if (!(entity == _excludeEntity) && isAlive == entity.IsAlive() && entity.boundingBox.Intersects(_aabb) && (!(_excludeEntity != null) || _excludeEntity.CanCollideWith(entity)))
				{
					_entityOutputList.Add(entity);
				}
			}
		}
	}

	public void GetEntitiesInBounds(FastTags<TagGroup.Global> _tags, Bounds _bb, List<Entity> _list)
	{
		int num = Utils.Fastfloor((double)(_bb.min.y - 5f) / 16.0);
		int num2 = Utils.Fastfloor((double)(_bb.max.y + 5f) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		else if (num >= 16)
		{
			num = 15;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if (entity.HasAnyTags(_tags) && entity.boundingBox.Intersects(_bb))
				{
					_list.Add(entity);
				}
			}
		}
	}

	public void GetEntitiesInBounds(Type _class, Bounds _bb, List<Entity> _list)
	{
		int num = Utils.Fastfloor((double)(_bb.min.y - 5f) / 16.0);
		int num2 = Utils.Fastfloor((double)(_bb.max.y + 5f) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		else if (num >= 16)
		{
			num = 15;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if (_class.IsAssignableFrom(entity.GetType()) && entity.boundingBox.Intersects(_bb))
				{
					_list.Add(entity);
				}
			}
		}
	}

	public void GetEntitiesAround(EntityFlags _mask, Vector3 _pos, float _radius, List<Entity> _list)
	{
		int num = Utils.Fastfloor((double)(_pos.y - _radius) / 16.0);
		int num2 = Utils.Fastfloor((double)(_pos.y + _radius) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		else if (num >= 16)
		{
			num = 15;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		float num3 = _radius * _radius;
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if ((entity.entityFlags & _mask) != EntityFlags.None && (entity.position - _pos).sqrMagnitude <= num3)
				{
					_list.Add(entity);
				}
			}
		}
	}

	public void GetEntitiesAround(EntityFlags _flags, EntityFlags _mask, Vector3 _pos, float _radius, List<Entity> _list)
	{
		int num = Utils.Fastfloor((double)(_pos.y - _radius) / 16.0);
		int num2 = Utils.Fastfloor((double)(_pos.y + _radius) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		else if (num >= 16)
		{
			num = 15;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		float num3 = _radius * _radius;
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if ((entity.entityFlags & _mask) == _flags && (entity.position - _pos).sqrMagnitude <= num3)
				{
					_list.Add(entity);
				}
			}
		}
	}

	public void RemoveEntityFromChunk(Entity _entity)
	{
		int y = _entity.chunkPosAddedEntityTo.y;
		entityLists[y].Remove(_entity);
		isModified = true;
		bool flag = false;
		for (int i = 0; i < 16; i++)
		{
			if (entityLists[i].Count > 0)
			{
				flag = true;
				break;
			}
		}
		hasEntities = flag;
	}

	public void AddEntityToChunk(Entity _entity)
	{
		hasEntities = true;
		int num = World.toChunkXZ(Utils.Fastfloor(_entity.position.x));
		int num2 = World.toChunkXZ(Utils.Fastfloor(_entity.position.z));
		if (num != m_X || num2 != m_Z)
		{
			Log.Error("Wrong entity chunk position! " + _entity?.ToString() + " x=" + num + " z=" + num2 + "/" + this);
		}
		int num3 = Utils.Fastfloor((double)_entity.position.y / 16.0);
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num3 >= 16)
		{
			num3 = 15;
		}
		_entity.addedToChunk = true;
		_entity.chunkPosAddedEntityTo.x = m_X;
		_entity.chunkPosAddedEntityTo.y = num3;
		_entity.chunkPosAddedEntityTo.z = m_Z;
		entityLists[num3].Add(_entity);
	}

	public void AdJustEntityTracking(Entity _entity)
	{
		if (_entity.addedToChunk)
		{
			int num = Utils.Fastfloor((double)_entity.position.y / 16.0);
			if (num < 0)
			{
				num = 0;
			}
			if (num >= 16)
			{
				num = 15;
			}
			if (_entity.chunkPosAddedEntityTo.y != num)
			{
				entityLists[_entity.chunkPosAddedEntityTo.y].Remove(_entity);
				_entity.chunkPosAddedEntityTo.y = num;
				entityLists[num].Add(_entity);
				isModified = true;
			}
		}
	}

	public Bounds GetAABB()
	{
		return boundingBox;
	}

	public static Bounds CalculateAABB(int _chunkX, int _chunkY, int _chunkZ)
	{
		return BoundsUtils.BoundsForMinMax(_chunkX * 16, _chunkY * 256, _chunkZ * 16, _chunkX * 16 + 16, _chunkY * 256 + 256, _chunkZ * 16 + 16);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBounds()
	{
		boundingBox = CalculateAABB(m_X, m_Y, m_Z);
		worldPosIMin.x = m_X << 4;
		worldPosIMin.y = m_Y << 8;
		worldPosIMin.z = m_Z << 4;
		worldPosIMax.x = worldPosIMin.x + 15;
		worldPosIMax.y = worldPosIMin.y + 255;
		worldPosIMax.z = worldPosIMin.z + 15;
	}

	public int GetTris()
	{
		return totalTris;
	}

	public int GetTrisInMesh(int _idx)
	{
		int num = 0;
		for (int i = 0; i < trisInMesh.GetLength(0); i++)
		{
			num += trisInMesh[i][_idx];
		}
		return num;
	}

	public int GetSizeOfMesh(int _idx)
	{
		int num = 0;
		for (int i = 0; i < trisInMesh.GetLength(0); i++)
		{
			num += sizeOfMesh[i][_idx];
		}
		return num;
	}

	public int GetUsedMem()
	{
		TotalMemory = 0;
		for (int i = 0; i < m_BlockLayers.Length; i++)
		{
			TotalMemory += ((m_BlockLayers[i] != null) ? m_BlockLayers[i].GetUsedMem() : 0);
		}
		TotalMemory += 12;
		TotalMemory += m_TerrainHeight.Length;
		TotalMemory += m_HeightMap.Length;
		TotalMemory += m_Biomes.Length;
		TotalMemory += m_BiomeIntensities.Length;
		TotalMemory += m_NormalX.Length;
		TotalMemory += m_NormalY.Length;
		TotalMemory += m_NormalZ.Length;
		TotalMemory += chnStability.GetUsedMem();
		TotalMemory += chnLight.GetUsedMem();
		TotalMemory += chnDensity.GetUsedMem();
		TotalMemory += chnDamage.GetUsedMem();
		for (int j = 0; j < 1; j++)
		{
			TotalMemory += chnTextures[j].GetUsedMem();
		}
		TotalMemory += chnWater.GetUsedMem();
		return TotalMemory;
	}

	public void GetTextureChannelMemory(out int[] texMem)
	{
		texMem = new int[1];
		for (int i = 0; i < 1; i++)
		{
			texMem[i] = chnTextures[i].GetUsedMem();
		}
	}

	public void OnLoadedFromCache()
	{
		NeedsRegeneration = true;
		isModified = true;
		InProgressRegeneration = false;
		InProgressSaving = false;
		InProgressCopying = false;
		InProgressDecorating = false;
		InProgressLighting = false;
		InProgressUnloading = false;
		NeedsOnlyCollisionMesh = false;
		IsCollisionMeshGenerated = false;
		entityStubs.Clear();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < entityLists[i].Count; j++)
			{
				if (entityLists[i][j].IsSavedToFile())
				{
					entityStubs.Add(new EntityCreationData(entityLists[i][j]));
				}
			}
			entityLists[i].Clear();
		}
	}

	public void OnLoad(World _world)
	{
		if (!_world.IsRemote())
		{
			for (int i = 0; i < entityStubs.Count; i++)
			{
				EntityCreationData entityCreationData = entityStubs[i];
				if (!(_world.GetEntity(entityCreationData.id) != null))
				{
					_world.SpawnEntityInWorld(EntityFactory.CreateEntity(entityCreationData));
				}
			}
			removeExpiredCustomChunkDataEntries(_world.GetWorldTime());
		}
		if (!_world.IsEditor())
		{
			GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
		}
		for (int j = 0; j < m_BlockLayers.Length; j++)
		{
			if (m_BlockLayers[j] != null)
			{
				m_BlockLayers[j].OnLoad(_world, 0, X * 16, j * 4, Z * 16);
			}
		}
	}

	public void OnUnload(WorldBase _world)
	{
		ProfilerBegin("Chunk OnUnload");
		InProgressUnloading = true;
		if (biomeParticles != null)
		{
			ProfilerBegin("biome particles");
			for (int i = 0; i < biomeParticles.Count; i++)
			{
				UnityEngine.Object.Destroy(biomeParticles[i]);
			}
			biomeParticles = null;
			ProfilerEnd();
		}
		spawnedBiomeParticles = false;
		if (!_world.IsRemote())
		{
			ProfilerBegin("enities");
			for (int j = 0; j < 16; j++)
			{
				if (entityLists[j].Count != 0)
				{
					_world.UnloadEntities(entityLists[j]);
				}
			}
			ProfilerEnd();
			removeExpiredCustomChunkDataEntries(_world.GetWorldTime());
		}
		ProfilerBegin("tile entities");
		for (int k = 0; k < tileEntities.list.Count; k++)
		{
			tileEntities.list[k].OnUnload(GameManager.Instance.World);
		}
		ProfilerEnd();
		RemoveBlockEntityTransforms();
		ProfilerBegin("block layers");
		for (int l = 0; l < m_BlockLayers.Length; l++)
		{
			if (m_BlockLayers[l] != null)
			{
				m_BlockLayers[l].OnUnload(_world, 0, X * 16, l * 4, Z * 16);
			}
		}
		ProfilerEnd();
		ProfilerBegin("water");
		waterSimHandle.Reset();
		ProfilerEnd();
		ProfilerEnd();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnBiomeParticles(Transform _parentForEntityBlocks)
	{
		if (!spawnedBiomeParticles)
		{
			biomeParticles = BiomeParticleManager.SpawnParticles(this, _parentForEntityBlocks);
			spawnedBiomeParticles = true;
		}
	}

	public void OnDisplay(World _world, Transform _entityBlocksParentT, ChunkCluster _chunkCluster)
	{
		ProfilerBegin("Chunk OnDisplay");
		SpawnBiomeParticles(_entityBlocksParentT);
		displayState = DisplayState.BlockEntities;
		blockEntitiesIndex = 0;
		blockEntityStubs.list.Sort([PublicizedFrom(EAccessModifier.Internal)] (BlockEntityData a, BlockEntityData b) => a.pos.y.CompareTo(b.pos.y));
		ProfilerEnd();
	}

	public void OnDisplayBlockEntities(World _world, Transform _entityBlocksParentT, ChunkCluster _chunkCluster)
	{
		ProfilerBegin("Chunk OnDisplayBlockEntities");
		Vector3 vector = new Vector3(X * 16, 0f, Z * 16);
		int num = _chunkCluster.LayerMappingTable["nocollision"];
		int num2 = _chunkCluster.LayerMappingTable["terraincollision"];
		int num3 = 0;
		int num4 = Utils.FastMax(50, blockEntityStubs.list.Count / 3 + 8);
		while (blockEntitiesIndex < blockEntityStubs.list.Count)
		{
			BlockEntityData blockEntityData = blockEntityStubs.list[blockEntitiesIndex];
			if (blockEntityData.bHasTransform)
			{
				if (!NeedsOnlyCollisionMesh && !blockEntityData.bRenderingOn)
				{
					SetBlockEntityRendering(blockEntityData, _bOn: true);
				}
			}
			else
			{
				if (++num3 > num4)
				{
					ProfilerEnd();
					return;
				}
				BlockValue block = _chunkCluster.GetBlock(blockEntityData.pos);
				if (!IsInternalBlocksCulled || block.type == blockEntityData.blockValue.type)
				{
					Block block2 = blockEntityData.blockValue.Block;
					if (!(block2.shape is BlockShapeModelEntity blockShapeModelEntity))
					{
						RemoveEntityBlockStub(blockEntityData.pos);
					}
					else
					{
						float num5 = 0f;
						if (block2.IsTerrainDecoration && _world.GetBlock(blockEntityData.pos - Vector3i.up).Block.shape.IsTerrain())
						{
							num5 = _world.GetDecorationOffsetY(blockEntityData.pos);
						}
						Quaternion rotation = blockShapeModelEntity.GetRotation(block);
						Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block2, rotation);
						rotatedOffset.x += 0.5f;
						rotatedOffset.z += 0.5f;
						rotatedOffset.y += num5;
						Vector3 vector2 = blockEntityData.pos.ToVector3() + rotatedOffset;
						GameObject objectForType = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity.modelName, out block2.defaultTintColor);
						if ((bool)objectForType)
						{
							ProfilerBegin("BE setup");
							Transform transform = (blockEntityData.transform = objectForType.transform);
							blockEntityData.bHasTransform = true;
							transform.SetParent(_entityBlocksParentT, worldPositionStays: false);
							transform.localScale = Vector3.one;
							transform.SetLocalPositionAndRotation(vector2 - vector, rotation);
							bool isCollideMovement = block2.IsCollideMovement;
							int newLayer = num;
							if (isCollideMovement)
							{
								switch (objectForType.layer)
								{
								case 30:
									newLayer = _chunkCluster.LayerMappingTable["Glass"];
									break;
								default:
									newLayer = num2;
									break;
								case 4:
									break;
								}
							}
							Utils.SetColliderLayerRecursively(objectForType, newLayer);
							Vector3i vector3i = ToLocalPosition(blockEntityData.pos);
							ProfilerBegin("BE TBA");
							block2.OnBlockEntityTransformBeforeActivated(_world, blockEntityData.pos, GetBlock(vector3i.x, vector3i.y, vector3i.z), blockEntityData);
							ProfilerEnd();
							objectForType.SetActive(value: true);
							ProfilerBegin("BE TAA");
							block2.OnBlockEntityTransformAfterActivated(_world, blockEntityData.pos, 0, GetBlock(vector3i.x, vector3i.y, vector3i.z), blockEntityData);
							ProfilerEnd();
							if (NeedsOnlyCollisionMesh)
							{
								SetBlockEntityRendering(blockEntityData, _bOn: false);
							}
							else
							{
								occlusionTs.Add(blockEntityData.transform);
							}
							ProfilerEnd();
						}
					}
				}
			}
			blockEntitiesIndex++;
		}
		if (occlusionTs.Count > 0)
		{
			if (OcclusionManager.Instance.cullChunkEntities)
			{
				ProfilerBegin("BE occlusion");
				OcclusionManager.Instance.AddChunkTransforms(this, occlusionTs);
				ProfilerEnd();
			}
			occlusionTs.Clear();
		}
		removeBlockEntitesMarkedForRemoval();
		AstarManager.AddBoundsToUpdate(boundingBox);
		displayState = DisplayState.Done;
		DynamicMeshThread.AddChunkGameObject(this);
		ProfilerEnd();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i ToLocalPosition(Vector3i _pos)
	{
		_pos.x &= 15;
		_pos.y &= 255;
		_pos.z &= 15;
		return _pos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBlockEntitesMarkedForRemoval()
	{
		if (OcclusionManager.Instance.cullChunkEntities)
		{
			for (int i = 0; i < blockEntityStubsToRemove.Count; i++)
			{
				BlockEntityData blockEntityData = blockEntityStubsToRemove[i];
				if (blockEntityData.bHasTransform)
				{
					occlusionTs.Add(blockEntityData.transform);
				}
			}
			if (occlusionTs.Count > 0)
			{
				OcclusionManager.Instance.RemoveChunkTransforms(this, occlusionTs);
				occlusionTs.Clear();
			}
		}
		for (int j = 0; j < blockEntityStubsToRemove.Count; j++)
		{
			BlockEntityData blockEntityData2 = blockEntityStubsToRemove[j];
			blockEntityData2.Cleanup();
			if (blockEntityData2.bHasTransform)
			{
				poolBlockEntityTransform(blockEntityData2);
			}
		}
		blockEntityStubsToRemove.Clear();
	}

	public void OnHide()
	{
		RemoveBlockEntityTransforms();
		AstarManager.AddBoundsToUpdate(boundingBox);
	}

	public void RemoveBlockEntityTransforms()
	{
		ProfilerBegin("RemoveBlockEntityTransforms");
		if (OcclusionManager.Instance.cullChunkEntities)
		{
			ProfilerBegin("OcclusionManager RemoveChunk");
			OcclusionManager.Instance.RemoveChunk(this);
			ProfilerEnd();
		}
		for (int i = 0; i < blockEntityStubs.list.Count; i++)
		{
			BlockEntityData blockEntityData = blockEntityStubs.list[i];
			if (blockEntityData.bHasTransform)
			{
				poolBlockEntityTransform(blockEntityData);
			}
		}
		ProfilerEnd();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void poolBlockEntityTransform(BlockEntityData _bed)
	{
		if (!_bed.bRenderingOn)
		{
			SetBlockEntityRendering(_bed, _bOn: true);
		}
		if ((bool)_bed.transform)
		{
			GameObjectPool.Instance.PoolObject(_bed.transform.gameObject);
		}
		else
		{
			Log.Error("BlockEntity {0} at pos {1} null transform!", _bed.ToString(), _bed.pos);
		}
		_bed.bHasTransform = false;
		_bed.transform = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBlockEntityRendering(BlockEntityData _bed, bool _bOn)
	{
		_bed.bRenderingOn = _bOn;
		if (!_bed.transform)
		{
			Log.Error($"2: {_bed.ToString()} on pos {_bed.pos} with empty transform/gameobject!");
			return;
		}
		ProfilerBegin("SetBlockEntityRendering set enable");
		_bed.transform.GetComponentsInChildren(tempMeshRenderers);
		for (int i = 0; i < tempMeshRenderers.Count; i++)
		{
			tempMeshRenderers[i].enabled = _bOn;
		}
		tempMeshRenderers.Clear();
		ProfilerEnd();
		ProfilerBegin("SetBlockEntityRendering BroadcastMessage");
		if (_bOn)
		{
			_bed.transform.BroadcastMessage("SetRenderingOn", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			_bed.transform.BroadcastMessage("SetRenderingOff", SendMessageOptions.DontRequireReceiver);
		}
		ProfilerEnd();
	}

	public static void ToTerrain(Chunk _chunk, Chunk _terrainChunk)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				byte height = _chunk.GetHeight(i, j);
				for (int k = 0; k <= height; k++)
				{
					if (!_chunk.GetBlock(i, k, j).isair)
					{
						_terrainChunk.SetBlockRaw(i, k, j, Constants.cTerrainBlockValue);
					}
				}
				for (int l = 0; l < 256; l++)
				{
					_terrainChunk.SetDensity(i, l, j, _chunk.GetDensity(i, l, j));
				}
				_terrainChunk.SetHeight(i, j, height);
				_terrainChunk.SetTerrainHeight(i, j, height);
			}
		}
		_terrainChunk.CopyLightsFrom(_chunk);
		_terrainChunk.isModified = true;
		_terrainChunk.NeedsLightCalculation = false;
	}

	public void AddMeshLayer(VoxelMeshLayer _vml)
	{
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			trisInMesh[_vml.idx][i] = _vml.GetTrisInMesh(i);
			sizeOfMesh[_vml.idx][i] = _vml.GetSizeOfMesh(i);
		}
		totalTris = 0;
		for (int j = 0; j < trisInMesh.GetLength(0); j++)
		{
			for (int k = 0; k < MeshDescription.meshes.Length; k++)
			{
				totalTris += trisInMesh[j][k];
			}
		}
		lock (m_layerIndexQueue)
		{
			VoxelMeshLayer voxelMeshLayer = m_meshLayers[_vml.idx];
			if (voxelMeshLayer == null)
			{
				MeshLayerCount++;
				m_layerIndexQueue.Enqueue(_vml.idx);
			}
			else
			{
				MemoryPools.poolVML.FreeSync(voxelMeshLayer);
			}
			m_meshLayers[_vml.idx] = _vml;
		}
	}

	public bool HasMeshLayer()
	{
		lock (m_layerIndexQueue)
		{
			return m_layerIndexQueue.Count > 0;
		}
	}

	public VoxelMeshLayer GetMeshLayer()
	{
		lock (m_layerIndexQueue)
		{
			if (m_layerIndexQueue.Count > 0)
			{
				MeshLayerCount--;
				int num = m_layerIndexQueue.Dequeue();
				VoxelMeshLayer result = m_meshLayers[num];
				m_meshLayers[num] = null;
				return result;
			}
			return null;
		}
	}

	public EnumDecoAllowed GetDecoAllowedAt(int x, int z)
	{
		EnumDecoAllowed enumDecoAllowed = EnumDecoAllowed.Everything;
		if (m_DecoBiomeArray != null)
		{
			enumDecoAllowed = m_DecoBiomeArray[x + z * 16];
		}
		if ((int)enumDecoAllowed.GetSize() < 1)
		{
			EnumDecoOccupied decoOccupiedAt = DecoManager.Instance.GetDecoOccupiedAt(x + m_X * 16, z + m_Z * 16);
			if ((int)decoOccupiedAt > 3 && decoOccupiedAt != EnumDecoOccupied.POI)
			{
				enumDecoAllowed = enumDecoAllowed.WithSize(EnumDecoAllowedSize.NoBigNoSmall);
			}
		}
		return enumDecoAllowed;
	}

	public EnumDecoAllowedSlope GetDecoAllowedSlopeAt(int x, int z)
	{
		return GetDecoAllowedAt(x, z).GetSlope();
	}

	public EnumDecoAllowedSize GetDecoAllowedSizeAt(int x, int z)
	{
		return GetDecoAllowedAt(x, z).GetSize();
	}

	public bool GetDecoAllowedStreetOnlyAt(int x, int z)
	{
		return GetDecoAllowedAt(x, z).GetStreetOnly();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnsureDecoBiomeArray()
	{
		if (m_DecoBiomeArray == null)
		{
			m_DecoBiomeArray = new EnumDecoAllowed[256];
		}
	}

	public void SetDecoAllowedAt(int x, int z, EnumDecoAllowed _newVal)
	{
		EnsureDecoBiomeArray();
		int num = x + z * 16;
		EnumDecoAllowed decoAllowed = m_DecoBiomeArray[num];
		EnumDecoAllowedSlope slope = decoAllowed.GetSlope();
		if ((int)slope > (int)_newVal.GetSlope())
		{
			_newVal = _newVal.WithSlope(slope);
		}
		EnumDecoAllowedSize size = decoAllowed.GetSize();
		if ((int)size > (int)_newVal.GetSize())
		{
			_newVal = _newVal.WithSize(size);
		}
		if (decoAllowed.GetStreetOnly() && !_newVal.GetStreetOnly())
		{
			_newVal = _newVal.WithStreetOnly(streetOnly: true);
		}
		m_DecoBiomeArray[num] = _newVal;
	}

	public void SetDecoAllowedSlopeAt(int x, int z, EnumDecoAllowedSlope _newVal)
	{
		EnsureDecoBiomeArray();
		int num = x + z * 16;
		SetDecoAllowedAt(x, z, m_DecoBiomeArray[num].WithSlope(_newVal));
	}

	public void SetDecoAllowedSizeAt(int x, int z, EnumDecoAllowedSize _newVal)
	{
		EnsureDecoBiomeArray();
		int num = x + z * 16;
		SetDecoAllowedAt(x, z, m_DecoBiomeArray[num].WithSize(_newVal));
	}

	public void SetDecoAllowedStreetOnlyAt(int x, int z, bool _newVal)
	{
		EnsureDecoBiomeArray();
		int num = x + z * 16;
		SetDecoAllowedAt(x, z, m_DecoBiomeArray[num].WithStreetOnly(_newVal));
	}

	public Vector3 GetTerrainNormal(int _x, int _z)
	{
		int num = _x + _z * 16;
		Vector3 result = default(Vector3);
		result.x = (float)(sbyte)m_NormalX[num] / 127f;
		result.y = (float)(sbyte)m_NormalY[num] / 127f;
		result.z = (float)(sbyte)m_NormalZ[num] / 127f;
		return result;
	}

	public float GetTerrainNormalY(int _x, int _z)
	{
		int num = _x + _z * 16;
		return (float)(sbyte)m_NormalY[num] / 127f;
	}

	public void SetTerrainNormal(int x, int z, Vector3 _v)
	{
		int num = x + z * 16;
		m_NormalX[num] = (byte)Utils.FastClamp(_v.x * 127f, -128f, 127f);
		m_NormalY[num] = (byte)Utils.FastClamp(_v.y * 127f, -128f, 127f);
		m_NormalZ[num] = (byte)Utils.FastClamp(_v.z * 127f, -128f, 127f);
	}

	public Vector3i ToWorldPos()
	{
		return new Vector3i(m_X * 16, m_Y * 256, m_Z * 16);
	}

	public Vector3i ToWorldPos(int _x, int _y, int _z)
	{
		return new Vector3i(m_X * 16 + _x, m_Y * 256 + _y, m_Z * 16 + _z);
	}

	public Vector3i ToWorldPos(Vector3i _pos)
	{
		return new Vector3i(m_X * 16, m_Y * 256, m_Z * 16) + _pos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFullMap()
	{
		if (mapColors == null)
		{
			mapColors = new ushort[256];
		}
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int num = i + j * 16;
				int num2 = m_HeightMap[num];
				int num3 = num2 >> 2;
				BlockValue blockValue = ((m_BlockLayers[num3] != null) ? m_BlockLayers[num3].GetAt(i, num2, j) : BlockValue.Air);
				WaterValue water = GetWater(i, num2, j);
				while (num2 > 0 && (blockValue.isair || blockValue.Block.IsTerrainDecoration) && !water.HasMass())
				{
					num2--;
					blockValue = ((m_BlockLayers[num3] != null) ? m_BlockLayers[num3].GetAt(i, num2, j) : BlockValue.Air);
					water = GetWater(i, num2, j);
				}
				Color col = BlockLiquidv2.Color;
				if (!water.HasMass())
				{
					float x = (float)(sbyte)m_NormalX[num] / 127f;
					float y = (float)(sbyte)m_NormalY[num] / 127f;
					float z = (float)(sbyte)m_NormalZ[num] / 127f;
					col = blockValue.Block.GetMapColor(blockValue, new Vector3(x, y, z), num2);
				}
				mapColors[num] = Utils.ToColor5(col);
			}
		}
		bMapDirty = false;
		ModEvents.SCalcChunkColorsDoneData _data = new ModEvents.SCalcChunkColorsDoneData(this);
		ModEvents.CalcChunkColorsDone.Invoke(ref _data);
	}

	public ushort[] GetMapColors()
	{
		if (mapColors == null || bMapDirty)
		{
			updateFullMap();
		}
		return mapColors;
	}

	public void OnDecorated()
	{
		CheckSameDensity();
		CheckOnlyTerrain();
	}

	public bool IsAreaMaster()
	{
		if (m_X % 5 == 0)
		{
			return m_Z % 5 == 0;
		}
		return false;
	}

	public bool IsAreaMasterCornerChunksLoaded(ChunkCluster _cc)
	{
		if (_cc.GetChunkSync(m_X - 2, m_Z) != null && _cc.GetChunkSync(m_X, m_Z + 2) != null && _cc.GetChunkSync(m_X + 2, m_Z + 2) != null)
		{
			return _cc.GetChunkSync(m_X - 2, m_Z - 2) != null;
		}
		return false;
	}

	public static Vector3i ToAreaMasterChunkPos(Vector3i _worldBlockPos)
	{
		return new Vector3i(World.toChunkXZ(_worldBlockPos.x) / 5 * 5, World.toChunkY(_worldBlockPos.y), World.toChunkXZ(_worldBlockPos.z) / 5 * 5);
	}

	public bool IsAreaMasterDominantBiomeInitialized(ChunkCluster _cc)
	{
		if (AreaMasterDominantBiome != byte.MaxValue)
		{
			return true;
		}
		if (_cc == null)
		{
			return false;
		}
		for (int i = 0; i < 50; i++)
		{
			biomeCnt[i] = 0;
		}
		for (int j = m_X - 2; j < m_X + 2; j++)
		{
			for (int k = m_Z - 2; k < m_Z + 2; k++)
			{
				Chunk chunkSync = _cc.GetChunkSync(j, k);
				if (chunkSync == null)
				{
					return false;
				}
				if (chunkSync.DominantBiome > 0)
				{
					biomeCnt[chunkSync.DominantBiome]++;
				}
			}
		}
		int num = 0;
		for (int l = 1; l < biomeCnt.Length; l++)
		{
			if (biomeCnt[l] > num)
			{
				AreaMasterDominantBiome = (byte)l;
				num = biomeCnt[l];
			}
		}
		return true;
	}

	public ChunkAreaBiomeSpawnData GetChunkBiomeSpawnData()
	{
		if (AreaMasterDominantBiome == byte.MaxValue)
		{
			return null;
		}
		if (biomeSpawnData == null)
		{
			if (!ChunkCustomData.dict.TryGetValue("bspd.main", out var value) || value == null)
			{
				value = new ChunkCustomData("bspd.main", ulong.MaxValue, _isSavedToNetwork: false);
				ChunkCustomData.Set(value.key, value);
			}
			biomeSpawnData = new ChunkAreaBiomeSpawnData(this, AreaMasterDominantBiome, value);
		}
		return biomeSpawnData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeExpiredCustomChunkDataEntries(ulong _worldTime)
	{
		List<string> list = null;
		for (int i = 0; i < ChunkCustomData.valueList.Count; i++)
		{
			if (ChunkCustomData.valueList[i].expiresInWorldTime <= _worldTime)
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(ChunkCustomData.keyList[i]);
				ChunkCustomData.valueList[i].OnRemove(this);
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				ChunkCustomData.Remove(list[j]);
			}
		}
	}

	public bool IsTraderArea(int _x, int _z)
	{
		Vector3i worldBlockPos = worldPosIMin;
		worldBlockPos.x += _x;
		worldBlockPos.z += _z;
		return GameManager.Instance.World.IsWithinTraderArea(worldBlockPos);
	}

	public override int GetHashCode()
	{
		return 31 * m_X + m_Z;
	}

	public void EnterReadLock()
	{
		sync.EnterReadLock();
	}

	public void EnterWriteLock()
	{
		sync.EnterWriteLock();
	}

	public void ExitReadLock()
	{
		sync.ExitReadLock();
	}

	public void ExitWriteLock()
	{
		sync.ExitWriteLock();
	}

	public override bool Equals(object obj)
	{
		if (base.Equals(obj))
		{
			return obj.GetHashCode() == GetHashCode();
		}
		return false;
	}

	public override string ToString()
	{
		if (cachedToString == null)
		{
			cachedToString = $"Chunk_{m_X},{m_Z}";
		}
		return cachedToString;
	}

	public List<DensityMismatchInformation> CheckDensities(bool _logAllMismatches = false)
	{
		Vector3i vector3i = new Vector3i(0, 0, 0);
		Vector3i vector3i2 = new Vector3i(16, 256, 16);
		int num = m_X << 4;
		int num2 = m_Y << 8;
		int num3 = m_Z << 4;
		bool flag = true;
		List<DensityMismatchInformation> list = new List<DensityMismatchInformation>();
		for (int i = vector3i.x; i < vector3i2.x; i++)
		{
			for (int j = vector3i.z; j < vector3i2.z; j++)
			{
				for (int k = vector3i.y; k < vector3i2.y; k++)
				{
					sbyte density = GetDensity(i, k, j);
					BlockValue block = GetBlock(i, k, j);
					bool flag2 = block.Block.shape.IsTerrain();
					bool flag3 = true;
					if (!((!flag2) ? (density >= 0) : (density < 0)))
					{
						DensityMismatchInformation item = new DensityMismatchInformation(num + i, num2 + k, num3 + j, density, block.type, flag2);
						list.Add(item);
						if (flag || _logAllMismatches)
						{
							Log.Warning(item.ToString());
							flag = false;
						}
					}
				}
			}
		}
		return list;
	}

	public bool RepairDensities()
	{
		Vector3i vector3i = new Vector3i(0, 0, 0);
		Vector3i vector3i2 = new Vector3i(16, 256, 16);
		bool result = false;
		for (int i = vector3i.x; i < vector3i2.x; i++)
		{
			for (int j = vector3i.z; j < vector3i2.z; j++)
			{
				for (int k = vector3i.y; k < vector3i2.y; k++)
				{
					Block block = GetBlock(i, k, j).Block;
					sbyte density = GetDensity(i, k, j);
					if (block.shape.IsTerrain())
					{
						if (density >= 0)
						{
							SetDensity(i, k, j, -1);
							result = true;
						}
					}
					else if (density < 0)
					{
						SetDensity(i, k, j, 1);
						result = true;
					}
				}
			}
		}
		return result;
	}

	public void LoopOverAllBlocks(ChunkBlockLayer.LoopBlocksDelegate _delegate, bool _bIncludeChilds = false, bool _bIncludeAirBlocks = false)
	{
		for (int i = 0; i < m_BlockLayers.Length; i++)
		{
			m_BlockLayers[i]?.LoopOverAllBlocks(this, i << 2, _delegate, _bIncludeChilds, _bIncludeAirBlocks);
		}
	}

	public IEnumerator LoopOverAllBlocksCoroutine(ChunkBlockLayer.LoopBlocksDelegate _delegate, bool _bIncludeChilds = false, bool _bIncludeAirBlocks = false)
	{
		for (int i = 0; i < m_BlockLayers.Length; i++)
		{
			ChunkBlockLayer chunkBlockLayer = m_BlockLayers[i];
			if (chunkBlockLayer != null)
			{
				chunkBlockLayer.LoopOverAllBlocks(this, i << 2, _delegate, _bIncludeChilds, _bIncludeAirBlocks);
				yield return null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInside(int _x, int _y, int _z)
	{
		Vector3b vector3b = new Vector3b(_x, _y, _z);
		return insideDevicesHashSet.Contains(vector3b.GetHashCode());
	}

	public BlockFaceFlag RestoreCulledBlocks(World _world)
	{
		BlockFaceFlag blockFaceFlag = BlockFaceFlag.None;
		for (int num = insideDevices.Count - 1; num >= 0; num--)
		{
			Vector3b vector3b = insideDevices[num];
			if (vector3b.x == 0)
			{
				blockFaceFlag |= BlockFaceFlag.West;
			}
			else if (vector3b.x == 15)
			{
				blockFaceFlag |= BlockFaceFlag.East;
			}
			if (vector3b.z == 0)
			{
				blockFaceFlag |= BlockFaceFlag.North;
			}
			else if (vector3b.z == 15)
			{
				blockFaceFlag |= BlockFaceFlag.South;
			}
		}
		IsInternalBlocksCulled = false;
		return blockFaceFlag;
	}

	public bool HasFallingBlocks()
	{
		List<Entity>[] array = entityLists;
		foreach (List<Entity> list in array)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] is EntityFallingBlock)
				{
					return true;
				}
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG_CHUNK_PROFILE")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ProfilerBegin(string _name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG_CHUNK_PROFILE")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ProfilerEnd()
	{
	}

	[Conditional("DEBUG_CHUNK_RWCHECK")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void RWCheck(PooledBinaryReader stream)
	{
		if (stream.ReadInt32() != 1431655765)
		{
			Log.Error("Chunk !RWCheck");
		}
	}

	[Conditional("DEBUG_CHUNK_RWCHECK")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void RWCheck(PooledBinaryWriter stream)
	{
		stream.Write(1431655765);
	}

	[Conditional("DEBUG_CHUNK_TRIGGERLOG")]
	public void LogTrigger(string _format = "", params object[] _args)
	{
		_format = $"{GameManager.frameCount} Chunk {ChunkPos} trigger {_format}";
		Log.Warning(_format, _args);
	}

	[Conditional("DEBUG_CHUNK_CHUNK")]
	public static void LogChunk(long _key, string _format = "", params object[] _args)
	{
		int num = WorldChunkCache.extractX(_key);
		int num2 = WorldChunkCache.extractZ(_key);
		if (num == 136 && num2 == 25)
		{
			_format = $"{GameManager.frameCount} Chunk pos {num} {num2}, {_format}";
			Log.Warning(_format, _args);
		}
	}

	[Conditional("DEBUG_CHUNK_ENTITY")]
	public void LogEntity(string _format = "", params object[] _args)
	{
		if (m_X == 136 && m_Z == 25)
		{
			_format = $"{GameManager.frameCount} Chunk {ChunkPos} entity {_format}";
			Log.Warning(_format, _args);
		}
	}

	public void LogChunkState()
	{
		Log.Out($"[FELLTHROUGHWORLD] Chunk {Key} State\n" + $"  Displayed: {IsDisplayed}\n" + $"  IsCollisionMeshGenerated: {IsCollisionMeshGenerated}\n" + $"  NeedsDecoration: {NeedsDecoration}\n" + $"  NeedsLightDecoration: {NeedsLightDecoration}\n" + $"  NeedsLightCalculation: {NeedsLightCalculation}\n" + $"  NeedsRegeneration: {NeedsRegeneration} {FormatRegenerationLayers(m_NeedsRegenerationAtY)}\n" + $"  NeedsCopying: {NeedsCopying} (layers: {m_layerIndexQueue.Count})");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string FormatRegenerationLayers(int mask)
	{
		switch (mask)
		{
		case 0:
			return "(none)";
		case 65535:
			return "(all layers)";
		default:
		{
			List<int> list = new List<int>();
			for (int i = 0; i < 16; i++)
			{
				if ((mask & (1 << i)) != 0)
				{
					list.Add(i);
				}
			}
			return "(Y layers: " + string.Join(", ", list) + ")";
		}
		}
	}
}
