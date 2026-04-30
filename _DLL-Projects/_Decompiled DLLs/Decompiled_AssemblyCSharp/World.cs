using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Audio;
using DynamicMusic;
using DynamicMusic.Factories;
using GamePath;
using Platform;
using Twitch;
using UnityEngine;

public class World : WorldBase, IBlockAccess, IChunkAccess, IChunkCallback
{
	public delegate void OnEntityLoadedDelegate(Entity _entity);

	public delegate void OnEntityUnloadedDelegate(Entity _entity, EnumRemoveEntityReason _reason);

	public delegate void OnWorldChangedEvent(string _sWorldName);

	[PublicizedFrom(EAccessModifier.Private)]
	public class ClipBlock
	{
		public const int kMaxBlocks = 32;

		public BlockValue value;

		public Vector3 pos;

		public Block block;

		public Vector3 bmins;

		public Vector3 bmaxs;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int _storageIndex = 0;

		[PublicizedFrom(EAccessModifier.Private)]
		public static ClipBlock[] _storage = new ClipBlock[32];

		public static void ResetStorage()
		{
			_storageIndex = 0;
		}

		public static ClipBlock New(BlockValue _value, Block _block, float _yDistort, Vector3 _blockPos, Bounds _bounds)
		{
			ClipBlock clipBlock = _storage[_storageIndex];
			if (clipBlock == null)
			{
				clipBlock = new ClipBlock();
				_storage[_storageIndex] = clipBlock;
			}
			clipBlock.Init(_value, _block, _yDistort, _blockPos, _bounds);
			_storageIndex++;
			return clipBlock;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Init(BlockValue _value, Block _block, float _yDistort, Vector3 _blockPos, Bounds _bounds)
		{
			value = _value;
			block = _block;
			pos = _blockPos;
			Bounds bounds = _bounds;
			bounds.center -= _blockPos;
			bounds.min -= new Vector3(0f, _yDistort, 0f);
			bmins = bounds.min;
			bmaxs = bounds.max;
		}
	}

	public enum WorldEvent
	{
		BloodMoon
	}

	public const int cCollisionBlocks = 5;

	public ulong worldTime;

	public int DawnHour;

	public int DuskHour;

	public float Gravity = 0.08f;

	public DictionaryList<int, Entity> Entities = new DictionaryList<int, Entity>();

	public DictionaryList<int, EntityPlayer> Players = new DictionaryList<int, EntityPlayer>();

	public List<EntityAlive> EntityAlives = new List<EntityAlive>();

	public NetEntityDistribution entityDistributer;

	public AIDirector aiDirector;

	public Manager audioManager;

	public Conductor dmsConductor;

	public IGameManager gameManager;

	public int Seed;

	public WorldBiomes Biomes;

	public SpawnManagerBiomes biomeSpawnManager;

	public BiomeIntensity LocalPlayerBiomeIntensityStandingOn = BiomeIntensity.Default;

	public WorldCreationData wcd;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldState worldState;

	public ChunkManager m_ChunkManager;

	public SharedChunkObserverCache m_SharedChunkObserverCache;

	public WorldEnvironment m_WorldEnvironment;

	public BiomeAtmosphereEffects BiomeAtmosphereEffects;

	public FlatAreaManager FlatAreaManager;

	public static bool IsSplatMapAvailable;

	public List<SSpawnedEntity> Last4Spawned = new List<SSpawnedEntity>();

	public int playerEntityUpdateCount;

	public int clientLastEntityId;

	public Transform EntitiesTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom rand;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnityTerrainConfigured;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayerLocal> m_LocalPlayerEntities = new List<EntityPlayerLocal>();

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal m_LocalPlayerEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entitiesWithinAABBExcludingEntity = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> livingEntitiesWithinAABBExcludingEntity = new List<EntityAlive>();

	[PublicizedFrom(EAccessModifier.Private)]
	public MapObjectManager objectsOnMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnManagerDynamic dynamicSpawnManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBlockTicker worldBlockTicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SleeperVolume> sleeperVolumes = new List<SleeperVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, List<int>> sleeperVolumeMap = new Dictionary<Vector3i, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TriggerVolume> triggerVolumes = new List<TriggerVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, List<int>> triggerVolumeMap = new Dictionary<Vector3i, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<WallVolume> wallVolumes = new List<WallVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, List<int>> wallVolumeMap = new Dictionary<Vector3i, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, object> blockData = new Dictionary<Vector3i, object>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> newlyLoadedChunksThisUpdate = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, ulong> areaMasterChunksToLock = new Dictionary<long, ulong>();

	public TriggerManager triggerManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[][] supportOrder = new int[8][]
	{
		new int[8] { 0, 7, 1, 6, 2, 4, 3, 5 },
		new int[8] { 0, 2, 1, 7, 3, 4, 6, 5 },
		new int[8] { 2, 1, 3, 0, 4, 6, 5, 7 },
		new int[8] { 2, 4, 3, 1, 5, 6, 0, 7 },
		new int[8] { 4, 3, 5, 2, 6, 0, 7, 1 },
		new int[8] { 4, 6, 5, 3, 7, 0, 2, 1 },
		new int[8] { 6, 5, 7, 4, 0, 2, 1, 3 },
		new int[8] { 6, 0, 7, 5, 1, 2, 4, 3 }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] supportOffsets = new int[16]
	{
		0, 1, 1, 1, 1, 0, 1, -1, 0, -1,
		-1, -1, -1, 0, -1, 1
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch msUnculling = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<Chunk> chunksToUncull = new HashSetList<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<Chunk> chunksToRegenerate = new HashSetList<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ClipBlock[] _clipBlocks = new ClipBlock[32];

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds[] _clipBounds = new Bounds[16];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollCacheSize = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[,,] collBlockCache = new BlockValue[50, 50, 50];

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[,,] collDensityCache = new sbyte[50, 50, 50];

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickEntityFrameCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tickEntityFrameCountAverage = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> tickEntityList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float tickEntityPartialTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickEntityIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickEntitySliceCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Queue<Vector3i> fallingBlocks = new Queue<Vector3i>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<Vector3i, float> fallingBlocksMap = new Dictionary<Vector3i, float>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> m_lpChunkList = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWorldNavExtent = 2900;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWorldRWGBorder = 90;

	public const float cEdgeHard = 50f;

	public const float cEdgeSoft = 80f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEdgeMinWorldSize = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<TraderArea> traderAreas;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTraderPlacingProtection = 2;

	public bool isEventBloodMoon;

	public ulong eventWorldTime;

	public int WorldDay;

	public int WorldHour;

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<Vector3i> pendingUpgradeDowngradeBlocks = new HashSet<Vector3i>();

	public string Guid
	{
		get
		{
			if (worldState != null)
			{
				return worldState.Guid;
			}
			return null;
		}
	}

	public Dictionary<Vector3i, float> fallingBlocksHashSet => fallingBlocksMap;

	public List<TraderArea> TraderAreas => ChunkCache.ChunkProvider.GetDynamicPrefabDecorator()?.GetTraderAreas();

	public event OnEntityLoadedDelegate EntityLoadedDelegates;

	public event OnEntityUnloadedDelegate EntityUnloadedDelegates;

	public event OnWorldChangedEvent OnWorldChanged;

	public virtual void Init(IGameManager _gameManager, WorldBiomes _biomes)
	{
		gameManager = _gameManager;
		m_ChunkManager = new ChunkManager();
		m_ChunkManager.Init(this);
		m_SharedChunkObserverCache = new SharedChunkObserverCache(m_ChunkManager, 3, new NoThreadingSemantics());
		LightManager.Init();
		triggerManager = new TriggerManager();
		Biomes = _biomes;
		if (_biomes != null)
		{
			biomeSpawnManager = new SpawnManagerBiomes(this);
		}
		audioManager = Manager.Instance;
		BiomeAtmosphereEffects = new BiomeAtmosphereEffects();
		BiomeAtmosphereEffects.Init(this);
	}

	public IEnumerator LoadWorld(string _sWorldName, bool _fixedSizeCC = false)
	{
		Log.Out("World.Load: " + _sWorldName);
		GamePrefs.Set(EnumGamePrefs.GameWorld, _sWorldName);
		IsSplatMapAvailable = GameManager.IsSplatMapAvailable();
		DuskDawnInit();
		wcd = new WorldCreationData(GameIO.GetWorldDir());
		worldState = new WorldState();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _sWorldName != null)
		{
			string text;
			if (IsEditor())
			{
				text = PathAbstractions.WorldsSearchPaths.GetLocation(_sWorldName).FullPath + "/main.ttw";
			}
			else
			{
				text = GameIO.GetSaveGameDir() + "/main.ttw";
				if (!SdFile.Exists(text))
				{
					if (!SdDirectory.Exists(GameIO.GetSaveGameDir()))
					{
						SdDirectory.CreateDirectory(GameIO.GetSaveGameDir());
					}
					Log.Out("Loading base world file header...");
					worldState.Load(GameIO.GetWorldDir() + "/main.ttw", _warnOnDifferentVersion: false, _infOnDiferentVersion: true);
					worldState.GenerateNewGuid();
					Seed = GamePrefs.GetString(EnumGamePrefs.GameName).GetHashCode();
					worldState.SetFrom(this, worldState.providerId);
					worldState.worldTime = 7000uL;
					worldState.saveDataLimit = SaveDataLimit.GetLimitFromPref();
					worldState.Save(text);
				}
			}
			if (!worldState.Load(text, _warnOnDifferentVersion: true, _infOnDiferentVersion: false, !IsEditor()))
			{
				Log.Error("Could not load file '" + text + "'!");
			}
			else
			{
				Seed = worldState.seed;
			}
		}
		wcd.Apply(this, worldState);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			Seed = GamePrefs.GetString(EnumGamePrefs.GameNameClient).GetHashCode();
		}
		GameRandomManager.Instance.SetBaseSeed(Seed);
		rand = GameRandomManager.Instance.CreateGameRandom();
		rand.SetLock();
		worldTime = ((!IsEditor()) ? worldState.worldTime : 12000);
		GameTimer.Instance.ticks = worldState.timeInTicks;
		EntityFactory.nextEntityID = worldState.nextEntityID;
		if (PlatformOptimizations.LimitedSaveData && worldState.saveDataLimit < 0)
		{
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(PathAbstractions.WorldsSearchPaths.GetLocation(_sWorldName));
			SaveDataLimit.SetLimitToPref(SaveDataLimitType.VeryLong.CalculateTotalSize(worldInfo.WorldSize));
		}
		else
		{
			SaveDataLimit.SetLimitToPref(worldState.saveDataLimit);
		}
		clientLastEntityId = -2;
		if (_sWorldName != null)
		{
			EntitiesTransform = GameObject.Find("/Entities").transform;
			EntityFactory.Init(EntitiesTransform);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			dynamicSpawnManager = new SpawnManagerDynamic(this, null);
			if (worldState.dynamicSpawnerState != null && worldState.dynamicSpawnerState.Length > 0)
			{
				using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader.SetBaseStream(worldState.dynamicSpawnerState);
				dynamicSpawnManager.Read(pooledBinaryReader);
			}
			entityDistributer = new NetEntityDistribution(this, 0);
			worldBlockTicker = new WorldBlockTicker(this);
			aiDirector = new AIDirector(this);
			if (worldState.aiDirectorState != null)
			{
				using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader2.SetBaseStream(worldState.aiDirectorState);
				aiDirector.Load(pooledBinaryReader2);
			}
			if (worldState.sleeperVolumeState != null)
			{
				using PooledBinaryReader pooledBinaryReader3 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader3.SetBaseStream(worldState.sleeperVolumeState);
				ReadSleeperVolumes(pooledBinaryReader3);
			}
			else
			{
				sleeperVolumes.Clear();
			}
			if (worldState.triggerVolumeState != null)
			{
				using PooledBinaryReader pooledBinaryReader4 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader4.SetBaseStream(worldState.triggerVolumeState);
				ReadTriggerVolumes(pooledBinaryReader4);
			}
			else
			{
				triggerVolumes.Clear();
			}
			if (worldState.wallVolumeState != null)
			{
				using PooledBinaryReader pooledBinaryReader5 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
				pooledBinaryReader5.SetBaseStream(worldState.wallVolumeState);
				ReadWallVolumes(pooledBinaryReader5);
			}
			else
			{
				wallVolumes.Clear();
			}
			SleeperVolume.WorldInit();
		}
		DecoManager.Instance.IsEnabled = _sWorldName != "Empty";
		yield return null;
		ChunkCluster cc = null;
		yield return CreateChunkCluster(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? worldState.providerId : EnumChunkProviderId.NetworkClient, GamePrefs.GetString(EnumGamePrefs.GameWorld), 0, _fixedSizeCC, [PublicizedFrom(EAccessModifier.Internal)] (ChunkCluster _cluster) =>
		{
			cc = _cluster;
		});
		yield return null;
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadWorldEnvironment"));
		string typeName = "WorldEnvironment";
		if (wcd.Properties.Values.ContainsKey("WorldEnvironment.Class"))
		{
			typeName = wcd.Properties.Values["WorldEnvironment.Class"];
		}
		GameObject gameObject = new GameObject("WorldEnvironment");
		m_WorldEnvironment = gameObject.AddComponent(Type.GetType(typeName)) as WorldEnvironment;
		m_WorldEnvironment.Init(wcd, this);
		DynamicPrefabDecorator dynamicPrefabDecorator = cc.ChunkProvider.GetDynamicPrefabDecorator();
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetCallback(dynamicPrefabDecorator);
		if (GameManager.Instance.IsEditMode() && !PrefabEditModeManager.Instance.IsActive())
		{
			SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetVisible(_bVisible: true);
		}
		if (DecoManager.Instance.IsEnabled)
		{
			IChunkProvider chunkProvider = ChunkCache.ChunkProvider;
			yield return DecoManager.Instance.OnWorldLoaded(chunkProvider.GetWorldSize().x, chunkProvider.GetWorldSize().y, this, chunkProvider);
			m_WorldEnvironment.CreateUnityTerrain();
		}
		if (!IsEditor())
		{
			(dmsConductor = Factory.CreateConductor()).Init(ReadyImmediate: true);
			if (!GameManager.IsDedicatedServer)
			{
				yield return dmsConductor.PreloadRoutine();
			}
		}
		if (!GameManager.IsDedicatedServer)
		{
			foreach (IEntitlementValidator entitlementValidator in PlatformManager.MultiPlatform.EntitlementValidators)
			{
				if (entitlementValidator is TwitchEntitlementManager twitchEntitlementManager)
				{
					twitchEntitlementManager.Init();
				}
			}
		}
		SetupTraders();
		SetupSleeperVolumes();
		SetupTriggerVolumes();
		SetupWallVolumes();
		if (!GameManager.IsDedicatedServer && GameManager.IsSplatMapAvailable())
		{
			if (UnityDistantTerrainTest.Instance == null)
			{
				UnityDistantTerrainTest.Create();
			}
			if (!isUnityTerrainConfigured)
			{
				isUnityTerrainConfigured = true;
				ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw = ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
				UnityDistantTerrainTest instance = UnityDistantTerrainTest.Instance;
				if (chunkProviderGenerateWorldFromRaw != null)
				{
					instance.HeightMap = chunkProviderGenerateWorldFromRaw.heightData;
					instance.hmWidth = chunkProviderGenerateWorldFromRaw.GetWorldSize().x;
					instance.hmHeight = chunkProviderGenerateWorldFromRaw.GetWorldSize().y;
					instance.TerrainMaterial = MeshDescription.meshes[5].materialDistant;
					instance.TerrainMaterial.renderQueue = 2490;
					instance.WaterMaterial = MeshDescription.meshes[1].materialDistant;
					instance.WaterMaterial.SetVector("_WorldDim", new Vector4(chunkProviderGenerateWorldFromRaw.GetWorldSize().x, chunkProviderGenerateWorldFromRaw.GetWorldSize().y, 0f, 0f));
					chunkProviderGenerateWorldFromRaw.GetWaterChunks16x16(out instance.WaterChunks16x16Width, out instance.WaterChunks16x16);
					instance.LoadTerrain();
				}
			}
		}
		if (this.OnWorldChanged != null)
		{
			this.OnWorldChanged(_sWorldName);
		}
	}

	public void Save()
	{
		for (int i = 0; i < ChunkClusters.Count; i++)
		{
			ChunkClusters[i]?.Save();
		}
		worldState.SetFrom(this, ChunkCache.ChunkProvider.GetProviderId());
		if (IsEditor())
		{
			worldState.ResetDynamicData();
			worldState.nextEntityID = 171;
			worldState.Save(PathAbstractions.WorldsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.GameWorld)).FullPath + "/main.ttw");
		}
		else
		{
			worldState.Save(GameIO.GetSaveGameDir() + "/main.ttw");
		}
		SaveDecorations();
		SaveDataUtils.SaveDataManager.CommitAsync();
	}

	public void SaveDecorations()
	{
		DecoManager.Instance.Save();
	}

	public void SaveWorldState()
	{
		worldState.SetFrom(this, ChunkCache.ChunkProvider.GetProviderId());
		worldState.Save(GameIO.GetSaveGameDir() + "/main.ttw");
	}

	public virtual void UnloadWorld(bool _bUnloadRespawnableEntities)
	{
		Log.Out("World.Unload");
		if (m_WorldEnvironment != null)
		{
			m_WorldEnvironment.Cleanup();
			UnityEngine.Object.Destroy(m_WorldEnvironment.gameObject);
			m_WorldEnvironment = null;
		}
		ChunkCache = null;
		ChunkClusters.Cleanup();
		UnloadEntities(Entities.list, _forceUnload: true);
		EntityFactory.Cleanup();
		if (BlockToolSelection.Instance != null)
		{
			BlockToolSelection.Instance.SelectionActive = false;
		}
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").Clear();
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").Clear();
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").Clear();
		DecoManager.Instance.OnWorldUnloaded();
		Block.OnWorldUnloaded();
		if (UnityDistantTerrainTest.Instance != null)
		{
			UnityDistantTerrainTest.Instance.Cleanup();
			isUnityTerrainConfigured = false;
		}
	}

	public virtual void Cleanup()
	{
		Log.Out("World.Cleanup");
		if (m_ChunkManager != null)
		{
			m_ChunkManager.Cleanup();
			m_ChunkManager = null;
		}
		if (audioManager != null)
		{
			audioManager.Dispose();
			Manager.CleanUp();
			audioManager = null;
		}
		if (dmsConductor != null)
		{
			dmsConductor.CleanUp();
			dmsConductor.OnWorldExit();
			dmsConductor = null;
		}
		LightManager.Dispose();
		for (int i = 0; i < Entities.list.Count; i++)
		{
			UnityEngine.Object.Destroy(Entities.list[i].RootTransform.gameObject);
		}
		Entities.Clear();
		EntityAlives.Clear();
		if (Biomes != null)
		{
			Biomes.Cleanup();
		}
		if (entityDistributer != null)
		{
			entityDistributer.Cleanup();
			entityDistributer = null;
		}
		if (biomeSpawnManager != null)
		{
			biomeSpawnManager.Cleanup();
			biomeSpawnManager = null;
		}
		dynamicSpawnManager = null;
		if (worldBlockTicker != null)
		{
			worldBlockTicker.Cleanup();
			worldBlockTicker = null;
		}
		BlockShapeNew.Cleanup();
		Biomes = null;
		if (objectsOnMap != null)
		{
			objectsOnMap.Clear();
		}
		m_LocalPlayerEntity = null;
		aiDirector = null;
		PathFinderThread.Instance = null;
		wcd = null;
		worldState = null;
		BiomeAtmosphereEffects = null;
		DynamicMeshUnity.ClearCachedDynamicMeshChunksList();
		if (FlatAreaManager != null)
		{
			FlatAreaManager.Cleanup();
		}
	}

	public void ClearCaches()
	{
		m_ChunkManager.FreePools();
		PathPoint.CompactPool();
		for (int i = 0; i < ChunkClusters.Count; i++)
		{
			ChunkClusters[i]?.ChunkProvider.ClearCaches();
		}
	}

	public long GetNextChunkToProvide()
	{
		return m_ChunkManager.GetNextChunkToProvide();
	}

	public virtual IEnumerator CreateChunkCluster(EnumChunkProviderId _chunkProviderId, string _clusterName, int _forceClrIdx, bool _bFixedSize, Action<ChunkCluster> _resultHandler)
	{
		ChunkCluster cc = new ChunkCluster(this, _clusterName, ChunkClusters.LayerMappingTable[0]);
		if (_forceClrIdx != -1)
		{
			ChunkClusters.AddFixed(cc, _forceClrIdx);
			ChunkCache = ChunkClusters.Cluster0;
		}
		cc.IsFixedSize = _bFixedSize;
		cc.AddChunkCallback(this);
		WaterSimulationNative.Instance.Init(cc);
		yield return cc.Init(_chunkProviderId);
		_resultHandler(cc);
	}

	public override void AddLocalPlayer(EntityPlayerLocal _localPlayer)
	{
		if (!m_LocalPlayerEntities.Contains(_localPlayer))
		{
			m_LocalPlayerEntities.Add(_localPlayer);
		}
		if (objectsOnMap == null)
		{
			objectsOnMap = new MapObjectManager();
		}
	}

	public override void RemoveLocalPlayer(EntityPlayerLocal _localPlayer)
	{
		m_LocalPlayerEntities.Remove(_localPlayer);
	}

	public override List<EntityPlayerLocal> GetLocalPlayers()
	{
		return m_LocalPlayerEntities;
	}

	public override bool IsLocalPlayer(int _playerId)
	{
		Entity entity = GetEntity(_playerId);
		if (entity != null && entity is EntityPlayerLocal)
		{
			return true;
		}
		return false;
	}

	public override EntityPlayerLocal GetLocalPlayerFromID(int _playerId)
	{
		return GetEntity(_playerId) as EntityPlayerLocal;
	}

	public override EntityPlayerLocal GetClosestLocalPlayer(Vector3 _position)
	{
		EntityPlayerLocal result = GetPrimaryPlayer();
		if (m_LocalPlayerEntities.Count > 1)
		{
			float num = float.MaxValue;
			for (int i = 0; i < m_LocalPlayerEntities.Count; i++)
			{
				float sqrMagnitude = (m_LocalPlayerEntities[i].GetPosition() - _position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = m_LocalPlayerEntities[i];
				}
			}
		}
		return result;
	}

	public override Vector3 GetVectorToClosestLocalPlayer(Vector3 _position)
	{
		return GetClosestLocalPlayer(_position).GetPosition() - _position;
	}

	public override float GetSquaredDistanceToClosestLocalPlayer(Vector3 _position)
	{
		return GetVectorToClosestLocalPlayer(_position).sqrMagnitude;
	}

	public override float GetDistanceToClosestLocalPlayer(Vector3 _position)
	{
		return GetVectorToClosestLocalPlayer(_position).magnitude;
	}

	public void SetLocalPlayer(EntityPlayerLocal _thePlayer)
	{
		m_LocalPlayerEntity = _thePlayer;
		audioManager.AttachLocalPlayer(_thePlayer, this);
		LightManager.AttachLocalPlayer(_thePlayer, this);
		OcclusionManager.Instance.SetSourceDepthCamera(_thePlayer.playerCamera);
	}

	public override EntityPlayerLocal GetPrimaryPlayer()
	{
		return m_LocalPlayerEntity;
	}

	public int GetPrimaryPlayerId()
	{
		if (!(m_LocalPlayerEntity != null))
		{
			return -1;
		}
		return m_LocalPlayerEntity.entityId;
	}

	public override List<EntityPlayer> GetPlayers()
	{
		return Players.list;
	}

	public void GetSunAndBlockColors(Vector3i _worldBlockPos, out byte sunLight, out byte blockLight)
	{
		sunLight = 0;
		blockLight = 0;
		IChunk chunkFromWorldPos = GetChunkFromWorldPos(_worldBlockPos);
		if (chunkFromWorldPos != null)
		{
			int x = toBlockXZ(_worldBlockPos.x);
			int y = toBlockY(_worldBlockPos.y);
			int z = toBlockXZ(_worldBlockPos.z);
			sunLight = chunkFromWorldPos.GetLight(x, y, z, Chunk.LIGHT_TYPE.SUN);
			blockLight = chunkFromWorldPos.GetLight(x, y, z, Chunk.LIGHT_TYPE.BLOCK);
		}
	}

	public override float GetLightBrightness(Vector3i blockPos)
	{
		IChunk chunkFromWorldPos = GetChunkFromWorldPos(blockPos);
		if (chunkFromWorldPos != null)
		{
			int x = toBlockXZ(blockPos.x);
			int y = toBlockY(blockPos.y);
			int z = toBlockXZ(blockPos.z);
			return chunkFromWorldPos.GetLightBrightness(x, y, z, 0);
		}
		if (!IsDaytime())
		{
			return 0.1f;
		}
		return 0.65f;
	}

	public override int GetBlockLightValue(int _clrIdx, Vector3i blockPos)
	{
		ChunkCluster chunkCluster = ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return MarchingCubes.DensityAir;
		}
		IChunk chunkFromWorldPos = chunkCluster.GetChunkFromWorldPos(blockPos);
		if (chunkFromWorldPos != null)
		{
			int x = toBlockXZ(blockPos.x);
			int y = toBlockY(blockPos.y);
			int z = toBlockXZ(blockPos.z);
			return chunkFromWorldPos.GetLightValue(x, y, z, 0);
		}
		return 0;
	}

	public override sbyte GetDensity(int _clrIdx, Vector3i _blockPos)
	{
		return GetDensity(_clrIdx, _blockPos.x, _blockPos.y, _blockPos.z);
	}

	public override sbyte GetDensity(int _clrIdx, int _x, int _y, int _z)
	{
		ChunkCluster chunkCluster = ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return MarchingCubes.DensityAir;
		}
		return chunkCluster.GetChunkFromWorldPos(_x, _y, _z)?.GetDensity(toBlockXZ(_x), toBlockY(_y), toBlockXZ(_z)) ?? MarchingCubes.DensityAir;
	}

	public void SetDensity(int _clrIdx, Vector3i _pos, sbyte _density, bool _bFoceDensity = false)
	{
		ChunkClusters[_clrIdx]?.SetDensity(_pos, _density, _bFoceDensity);
	}

	public long GetTexture(int _x, int _y, int _z, int channel = 0)
	{
		return ((Chunk)ChunkCache.GetChunkFromWorldPos(_x, _y, _z))?.GetTextureFull(toBlockXZ(_x), toBlockY(_y), toBlockXZ(_z), channel) ?? 0;
	}

	public TextureFullArray GetTextureFullArray(int _x, int _y, int _z)
	{
		return ((Chunk)ChunkCache.GetChunkFromWorldPos(_x, _y, _z))?.GetTextureFullArray(toBlockXZ(_x), toBlockY(_y), toBlockXZ(_z)) ?? new TextureFullArray(0L);
	}

	public void SetTexture(int _clrIdx, int _x, int _y, int _z, long _tex, int channel = 0)
	{
		ChunkClusters[_clrIdx].SetTextureFull(new Vector3i(_x, _y, _z), _tex, channel);
	}

	public override byte GetStability(int worldX, int worldY, int worldZ)
	{
		return GetChunkSync(toChunkXZ(worldX), toChunkXZ(worldZ))?.GetStability(toBlockXZ(worldX), toBlockY(worldY), toBlockXZ(worldZ)) ?? 0;
	}

	public override byte GetStability(Vector3i _pos)
	{
		return GetStability(_pos.x, _pos.y, _pos.z);
	}

	public override void SetStability(int worldX, int worldY, int worldZ, byte stab)
	{
		GetChunkSync(toChunkXZ(worldX), toChunkXZ(worldZ))?.SetStability(toBlockXZ(worldX), toBlockY(worldY), toBlockXZ(worldZ), stab);
	}

	public override void SetStability(Vector3i _pos, byte stab)
	{
		SetStability(_pos.x, _pos.y, _pos.z, stab);
	}

	public override byte GetHeight(int worldX, int worldZ)
	{
		return GetChunkSync(toChunkXZ(worldX), toChunkXZ(worldZ))?.GetHeight(toBlockXZ(worldX), toBlockXZ(worldZ)) ?? 0;
	}

	public byte GetTerrainHeight(int worldX, int worldZ)
	{
		return ((Chunk)GetChunkSync(toChunkXZ(worldX), toChunkXZ(worldZ)))?.GetTerrainHeight(toBlockXZ(worldX), toBlockXZ(worldZ)) ?? 0;
	}

	public float GetHeightAt(float worldX, float worldZ)
	{
		return (ChunkCache.ChunkProvider?.GetTerrainGenerator())?.GetTerrainHeightAt((int)worldX, (int)worldZ) ?? 0f;
	}

	public bool GetWaterAt(float worldX, float worldZ)
	{
		if (!(ChunkCache.ChunkProvider is ChunkProviderGenerateWorldFromRaw { poiFromImage: var poiFromImage }))
		{
			return false;
		}
		if (poiFromImage == null)
		{
			return false;
		}
		if (!poiFromImage.m_Poi.Contains((int)worldX, (int)worldZ))
		{
			return false;
		}
		byte data = poiFromImage.m_Poi.GetData((int)worldX, (int)worldZ);
		if (data == 0)
		{
			return false;
		}
		PoiMapElement poiForColor = Biomes.getPoiForColor(data);
		if (poiForColor == null)
		{
			return false;
		}
		return poiForColor.m_BlockValue.type == 240;
	}

	public override bool IsWater(int _x, int _y, int _z)
	{
		if ((uint)_y < 256u)
		{
			IChunk chunkFromWorldPos = GetChunkFromWorldPos(_x, _y, _z);
			if (chunkFromWorldPos != null)
			{
				_x &= 0xF;
				_y &= 0xFF;
				_z &= 0xF;
				return chunkFromWorldPos.IsWater(_x, _y, _z);
			}
		}
		return false;
	}

	public override bool IsWater(Vector3i _pos)
	{
		return IsWater(_pos.x, _pos.y, _pos.z);
	}

	public override bool IsWater(Vector3 _pos)
	{
		return IsWater(worldToBlockPos(_pos));
	}

	public override bool IsAir(int _x, int _y, int _z)
	{
		if ((uint)_y < 256u)
		{
			IChunk chunkFromWorldPos = GetChunkFromWorldPos(_x, _y, _z);
			if (chunkFromWorldPos != null)
			{
				_x &= 0xF;
				_y &= 0xFF;
				_z &= 0xF;
				return chunkFromWorldPos.IsAir(_x, _y, _z);
			}
		}
		return true;
	}

	public bool CheckForLevelNearbyHeights(float worldX, float worldZ, int distance)
	{
		ITerrainGenerator terrainGenerator = ChunkCache.ChunkProvider?.GetTerrainGenerator();
		float num = -999f;
		float num2 = 999f;
		if (terrainGenerator != null)
		{
			float terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX, (int)worldZ);
			num = terrainHeightAt;
			num2 = terrainHeightAt;
			terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX + distance, (int)worldZ);
			if (terrainHeightAt < num)
			{
				num = terrainHeightAt;
			}
			else if (terrainHeightAt > num2)
			{
				num2 = terrainHeightAt;
			}
			terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX - distance, (int)worldZ);
			if (terrainHeightAt < num)
			{
				num = terrainHeightAt;
			}
			else if (terrainHeightAt > num2)
			{
				num2 = terrainHeightAt;
			}
			terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX, (int)worldZ + distance);
			if (terrainHeightAt < num)
			{
				num = terrainHeightAt;
			}
			else if (terrainHeightAt > num2)
			{
				num2 = terrainHeightAt;
			}
			terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX, (int)worldZ - distance);
			if (terrainHeightAt < num)
			{
				num = terrainHeightAt;
			}
			else if (terrainHeightAt > num2)
			{
				num2 = terrainHeightAt;
			}
			return Mathf.Abs(num2 - num) <= 2f;
		}
		return false;
	}

	public bool FindRandomSpawnPointNearRandomPlayer(int maxLightValue, out int x, out int y, out int z)
	{
		if (Players.list.Count == 0)
		{
			x = (y = (z = 0));
			return false;
		}
		Entity entityPlayer = null;
		int num = GetGameRandom().RandomRange(Players.list.Count);
		for (int i = 0; i < Players.list.Count; i++)
		{
			entityPlayer = Players.list[i];
			if (num-- == 0)
			{
				break;
			}
		}
		return FindRandomSpawnPointNearPlayer(entityPlayer, maxLightValue, out x, out y, out z, 32);
	}

	public bool FindRandomSpawnPointNearPlayer(Entity _entityPlayer, int maxLightValue, out int x, out int y, out int z, int maxDistance)
	{
		return FindRandomSpawnPointNearPosition(_entityPlayer.GetPosition(), maxLightValue, out x, out y, out z, new Vector3(maxDistance, maxDistance, maxDistance), _bOnGround: true);
	}

	public bool FindRandomSpawnPointNearPositionUnderground(Vector3 _pos, int maxLightValue, out int x, out int y, out int z, Vector3 maxDistance)
	{
		x = (y = (z = 0));
		for (int i = 0; i < 5; i++)
		{
			x = Utils.Fastfloor(_pos.x + RandomRange((0f - maxDistance.x) / 2f, maxDistance.x / 2f));
			z = Utils.Fastfloor(_pos.z + RandomRange((0f - maxDistance.z) / 2f, maxDistance.z / 2f));
			Chunk chunk = (Chunk)GetChunkFromWorldPos(x, z);
			if (chunk != null && IsInPlayfield(chunk))
			{
				int x2 = toBlockXZ(x);
				int z2 = toBlockXZ(z);
				int num = Utils.Fastfloor(_pos.y - maxDistance.y / 2f);
				int num2 = Utils.Fastfloor(_pos.y + maxDistance.y / 2f);
				int num3 = (int)_pos.y;
				if (num3 >= num && num3 <= num2 && chunk.CanMobsSpawnAtPos(x2, num3, z2))
				{
					y = num3;
					return true;
				}
				if (chunk.FindSpawnPointAtXZ(x2, z2, out y, maxLightValue, 0, num, num2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool FindRandomSpawnPointNearPosition(Vector3 _pos, int maxLightValue, out int x, out int y, out int z, Vector3 maxDistance, bool _bOnGround, bool _bIgnoreCanMobsSpawnOn = false)
	{
		x = (y = (z = 0));
		for (int i = 0; i < 5; i++)
		{
			x = Utils.Fastfloor(_pos.x + RandomRange((0f - maxDistance.x) / 2f, maxDistance.x / 2f));
			z = Utils.Fastfloor(_pos.z + RandomRange((0f - maxDistance.z) / 2f, maxDistance.z / 2f));
			Chunk chunk = (Chunk)GetChunkFromWorldPos(x, z);
			if (chunk != null && IsInPlayfield(chunk))
			{
				if (!_bOnGround)
				{
					y = Utils.Fastfloor(_pos.y + RandomRange((0f - maxDistance.y) / 2f, maxDistance.y / 2f));
					return true;
				}
				int x2 = toBlockXZ(x);
				int z2 = toBlockXZ(z);
				int num = Utils.Fastfloor(_pos.y - maxDistance.y / 2f);
				int num2 = Utils.Fastfloor(_pos.y + maxDistance.y / 2f);
				int num3 = chunk.GetHeight(x2, z2) + 1;
				if (num3 >= num && num3 <= num2 && chunk.CanMobsSpawnAtPos(x2, num3, z2, _bIgnoreCanMobsSpawnOn))
				{
					y = num3;
					return true;
				}
				if (chunk.FindSpawnPointAtXZ(x2, z2, out y, maxLightValue, 0, num, num2, _bIgnoreCanMobsSpawnOn))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPositionInRangeOfBedrolls(Vector3 _position)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize);
		num *= num;
		for (int i = 0; i < Players.list.Count; i++)
		{
			EntityBedrollPositionList spawnPoints = Players.list[i].SpawnPoints;
			int count = spawnPoints.Count;
			for (int j = 0; j < count; j++)
			{
				if ((spawnPoints[j].ToVector3() - _position).sqrMagnitude < (float)num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool GetRandomSpawnPositionMinMaxToRandomPlayer(int _minRange, int _maxRange, bool _bConsiderBedrolls, out EntityPlayer _player, out Vector3 _position)
	{
		_position = Vector3.zero;
		_player = null;
		if (Players.list.Count == 0)
		{
			return false;
		}
		if (_maxRange - _minRange <= 0)
		{
			return false;
		}
		int num = rand.RandomRange(Players.list.Count);
		for (int i = 0; i < Players.list.Count; i++)
		{
			if (num-- == 0)
			{
				_player = Players.list[i];
				break;
			}
		}
		int num2 = _minRange * _minRange;
		for (int j = 0; j < 10; j++)
		{
			Vector2 zero = Vector2.zero;
			do
			{
				zero = rand.RandomInsideUnitCircle * (_maxRange - _minRange);
			}
			while ((double)zero.sqrMagnitude < 0.01);
			zero += zero * ((float)_minRange / zero.magnitude);
			_position = _player.GetPosition() + new Vector3(zero.x, 0f, zero.y);
			Vector3i blockPos = worldToBlockPos(_position);
			Chunk chunk = (Chunk)GetChunkFromWorldPos(blockPos);
			if (chunk == null)
			{
				continue;
			}
			int x = toBlockXZ(blockPos.x);
			int z = toBlockXZ(blockPos.z);
			blockPos.y = chunk.GetHeight(x, z) + 1;
			_position.y = blockPos.y;
			if ((_bConsiderBedrolls && isPositionInRangeOfBedrolls(blockPos.ToVector3())) || !chunk.CanMobsSpawnAtPos(x, Utils.Fastfloor(_position.y), z))
			{
				continue;
			}
			bool flag = true;
			for (int k = 0; k < Players.list.Count; k++)
			{
				EntityPlayer entityPlayer = Players.list[k];
				if (entityPlayer.GetDistanceSq(_position) < (float)num2)
				{
					flag = false;
					break;
				}
				if (entityPlayer.CanSee(_position))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				_position = blockPos.ToVector3() + new Vector3(0.5f, GetTerrainOffset(0, blockPos), 0.5f);
				return true;
			}
		}
		return false;
	}

	public bool GetMobRandomSpawnPosWithWater(Vector3 _targetPos, int _minRange, int _maxRange, int _minPlayerRange, bool _checkBedrolls, out Vector3 _position)
	{
		if (GetRandomSpawnPositionMinMaxToPosition(_targetPos, _minRange, _maxRange, _minPlayerRange, _checkBedrolls, out _position, -1, _checkWater: true, 20))
		{
			return true;
		}
		if (GetRandomSpawnPositionMinMaxToPosition(_targetPos, _minRange, _maxRange, _minPlayerRange, _checkBedrolls, out _position, -1, _checkWater: false, 20))
		{
			return true;
		}
		return false;
	}

	public bool GetRandomSpawnPositionMinMaxToPosition(Vector3 _targetPos, int _minRange, int _maxRange, int _minPlayerRange, bool _checkBedrolls, out Vector3 _position, int _forPlayerEntityId = -1, bool _checkWater = true, int _retryCount = 50, bool _checkLandClaim = false, EnumLandClaimOwner _maxLandClaimType = EnumLandClaimOwner.None, bool _useSquareRadius = false)
	{
		_position = Vector3.zero;
		int num = _maxRange - _minRange;
		if (num <= 0)
		{
			return false;
		}
		PersistentPlayerData lpRelative = (_checkLandClaim ? GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_forPlayerEntityId) : null);
		for (int i = 0; i < _retryCount; i++)
		{
			if (_useSquareRadius)
			{
				int num2;
				int num3;
				do
				{
					num2 = rand.RandomRange(-_maxRange, _maxRange + 1);
					num3 = rand.RandomRange(-_maxRange, _maxRange + 1);
				}
				while (Mathf.Abs(num2) < _minRange && Mathf.Abs(num3) < _minRange);
				_position.x = _targetPos.x + (float)num2;
				_position.y = _targetPos.y;
				_position.z = _targetPos.z + (float)num3;
			}
			else
			{
				Vector2 vector;
				do
				{
					vector = rand.RandomInsideUnitCircle * num;
				}
				while (vector.sqrMagnitude < 0.01f);
				vector += vector * ((float)_minRange / vector.magnitude);
				_position.x = _targetPos.x + vector.x;
				_position.y = _targetPos.y;
				_position.z = _targetPos.z + vector.y;
			}
			Vector3i vector3i = worldToBlockPos(_position);
			Chunk chunk = (Chunk)GetChunkFromWorldPos(vector3i);
			if (chunk == null)
			{
				continue;
			}
			int x = toBlockXZ(vector3i.x);
			int z = toBlockXZ(vector3i.z);
			vector3i.y = chunk.GetHeight(x, z) + 1;
			_position.y = vector3i.y;
			if (_checkBedrolls && isPositionInRangeOfBedrolls(vector3i.ToVector3()))
			{
				continue;
			}
			if (_forPlayerEntityId == -1)
			{
				if (!chunk.CanMobsSpawnAtPos(x, Utils.Fastfloor(_position.y), z, _ignoreCanMobsSpawnOn: false, _checkWater))
				{
					continue;
				}
			}
			else if (!chunk.CanPlayersSpawnAtPos(x, Utils.Fastfloor(_position.y), z) || !chunk.IsPositionOnTerrain(x, vector3i.y, z) || GetPOIAtPosition(_position) != null || (_checkWater && chunk.IsWater(x, vector3i.y - 1, z)) || (_checkLandClaim && GetLandClaimOwner(vector3i, lpRelative) > _maxLandClaimType))
			{
				continue;
			}
			if (isPositionFarFromPlayers(_position, _minPlayerRange))
			{
				_position = vector3i.ToVector3() + new Vector3(0.5f, GetTerrainOffset(0, vector3i), 0.5f);
				return true;
			}
		}
		_position = Vector3.zero;
		return false;
	}

	public bool GetRandomSpawnPositionInAreaMinMaxToPlayers(Rect _area, int _minDistance, int UNUSED_maxDistance, bool _checkBedrolls, out Vector3 _position)
	{
		_position = Vector3.zero;
		if (Players.list.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < 10; i++)
		{
			_position.x = _area.x + RandomRange(0f, _area.width - 1f);
			_position.y = 0f;
			_position.z = _area.y + RandomRange(0f, _area.height - 1f);
			Vector3i blockPos = worldToBlockPos(_position);
			Chunk chunk = (Chunk)GetChunkFromWorldPos(blockPos);
			if (chunk == null)
			{
				continue;
			}
			int x = toBlockXZ(blockPos.x);
			int z = toBlockXZ(blockPos.z);
			blockPos.y = chunk.GetHeight(x, z) + 1;
			_position.y = blockPos.y;
			if ((_checkBedrolls && isPositionInRangeOfBedrolls(blockPos.ToVector3())) || !chunk.CanMobsSpawnAtPos(x, Utils.Fastfloor(_position.y), z))
			{
				continue;
			}
			bool flag = isPositionFarFromPlayers(_position, _minDistance);
			if (!flag)
			{
				continue;
			}
			for (int j = 0; j < Players.list.Count; j++)
			{
				EntityPlayer entityPlayer = Players.list[j];
				if ((_position - entityPlayer.position).sqrMagnitude < 2500f && entityPlayer.IsInViewCone(_position))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				_position = blockPos.ToVector3() + new Vector3(0.5f, GetTerrainOffset(0, blockPos), 0.5f);
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPositionFarFromPlayers(Vector3 _position, int _minDistance)
	{
		int num = _minDistance * _minDistance;
		for (int i = 0; i < Players.list.Count; i++)
		{
			if (Players.list[i].GetDistanceSq(_position) < (float)num)
			{
				return false;
			}
		}
		return true;
	}

	public Vector3 FindSupportingBlockPos(Vector3 pos)
	{
		Vector3i vector3i = worldToBlockPos(pos);
		BlockValue block = GetBlock(vector3i);
		Block block2 = block.Block;
		if (block2.IsMovementBlocked(this, vector3i, block, BlockFace.Top))
		{
			return pos;
		}
		if (block2.IsElevator())
		{
			return pos;
		}
		vector3i.y++;
		block = GetBlock(vector3i);
		block2 = block.Block;
		if (block2.IsElevator(block.rotation))
		{
			return pos;
		}
		vector3i.y -= 2;
		block = GetBlock(vector3i);
		block2 = block.Block;
		if (!block2.IsElevator() && !block2.IsMovementBlocked(this, vector3i, block, BlockFace.Top))
		{
			Vector3 vector = new Vector3((float)vector3i.x + 0.5f, pos.y, (float)vector3i.z + 0.5f);
			Vector3 vector2 = pos - vector;
			int num = Mathf.RoundToInt((Mathf.Atan2(vector2.x, vector2.z) * 57.29578f + 22.5f) / 45f) & 7;
			int[] array = supportOrder[num];
			Vector3i vector3i2 = default(Vector3i);
			vector3i2.y = vector3i.y;
			for (int i = 0; i < 8; i++)
			{
				int num2 = array[i] * 2;
				vector3i2.x = vector3i.x + supportOffsets[num2];
				vector3i2.z = vector3i.z + supportOffsets[num2 + 1];
				block = GetBlock(vector3i2);
				block2 = block.Block;
				if (block2.IsMovementBlocked(this, vector3i2, block, BlockFace.Top))
				{
					pos.x = (float)vector3i2.x + 0.5f;
					pos.z = (float)vector3i2.z + 0.5f;
					break;
				}
			}
		}
		return pos;
	}

	public float GetTerrainOffset(int _clrIdx, Vector3i _blockPos)
	{
		float result = 0f;
		if (GetBlock(_clrIdx, _blockPos - Vector3i.up).Block.shape.IsTerrain())
		{
			sbyte density = GetDensity(_clrIdx, _blockPos);
			sbyte density2 = GetDensity(_clrIdx, _blockPos - Vector3i.up);
			result = MarchingCubes.GetDecorationOffsetY(density, density2);
		}
		return result;
	}

	public bool IsInPlayfield(Chunk _c)
	{
		ChunkCluster chunkCache = ChunkCache;
		if (!chunkCache.IsFixedSize)
		{
			return true;
		}
		if (IsEditor())
		{
			return true;
		}
		if (_c.X > chunkCache.ChunkMinPos.x && _c.Z > chunkCache.ChunkMinPos.y && _c.X < chunkCache.ChunkMaxPos.x)
		{
			return _c.Z < chunkCache.ChunkMaxPos.y;
		}
		return false;
	}

	public override BlockValue GetBlock(Vector3i _pos)
	{
		return ChunkCache?.GetBlock(_pos) ?? BlockValue.Air;
	}

	public override BlockValue GetBlock(int _clrIdx, Vector3i _pos)
	{
		return ChunkClusters[_clrIdx]?.GetBlock(_pos) ?? BlockValue.Air;
	}

	public override BlockValue GetBlock(int _x, int _y, int _z)
	{
		return GetBlock(new Vector3i(_x, _y, _z));
	}

	public override BlockValue GetBlock(int _clrIdx, int _x, int _y, int _z)
	{
		return ChunkClusters[_clrIdx]?.GetBlock(new Vector3i(_x, _y, _z)) ?? BlockValue.Air;
	}

	public WaterValue GetWater(int _x, int _y, int _z)
	{
		return GetWater(new Vector3i(_x, _y, _z));
	}

	public WaterValue GetWater(Vector3i _pos)
	{
		return ChunkCache?.GetWater(_pos) ?? WaterValue.Empty;
	}

	public float GetWaterPercent(Vector3i _pos)
	{
		return ChunkCache?.GetWater(_pos).GetMassPercent() ?? 0f;
	}

	public void HandleWaterLevelChanged(Vector3i _pos, float _waterPercent)
	{
		GameLightManager.Instance?.HandleWaterLevelChanged();
	}

	public BiomeDefinition GetBiome(string _name)
	{
		return Biomes.GetBiome(_name);
	}

	public BiomeDefinition GetBiome(int _x, int _z)
	{
		Chunk chunk = (Chunk)GetChunkFromWorldPos(_x, _z);
		if (chunk != null)
		{
			byte biomeId = chunk.GetBiomeId(toBlockXZ(_x), toBlockXZ(_z));
			return Biomes.GetBiome(biomeId);
		}
		return null;
	}

	public BiomeDefinition GetBiomeInWorld(int _x, int _z)
	{
		IChunkProvider chunkProvider = ChunkCache?.ChunkProvider;
		if (chunkProvider != null)
		{
			IBiomeProvider biomeProvider = chunkProvider.GetBiomeProvider();
			if (biomeProvider != null)
			{
				return biomeProvider.GetBiomeAt(_x, _z);
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IChunk GetChunkFromWorldPos(int x, int z)
	{
		return GetChunkSync(toChunkXZ(x), toChunkXZ(z));
	}

	public override IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return GetChunkSync(toChunkXZ(x), toChunkXZ(z));
	}

	public override IChunk GetChunkFromWorldPos(Vector3i _blockPos)
	{
		return GetChunkSync(toChunkXZ(_blockPos.x), toChunkXZ(_blockPos.z));
	}

	public override void GetChunkFromWorldPos(int _blockX, int _blockZ, ref IChunk _chunk)
	{
		_blockX >>= 4;
		_blockZ >>= 4;
		if (_chunk == null || _chunk.X != _blockX || _chunk.Z != _blockZ)
		{
			_chunk = GetChunkSync(_blockX, _blockZ);
		}
	}

	public override bool GetChunkFromWorldPos(Vector3i _blockPos, ref IChunk _chunk)
	{
		Vector3i vector3i = toChunkXYZ(_blockPos);
		if (_chunk == null || _chunk.ChunkPos != vector3i)
		{
			_chunk = GetChunkSync(vector3i);
		}
		return _chunk != null;
	}

	public override IChunk GetChunkSync(Vector3i chunkPos)
	{
		return GetChunkSync(chunkPos.x, chunkPos.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IChunk GetChunkSync(int chunkX, int chunkZ)
	{
		return ChunkCache?.GetChunkSync(chunkX, chunkZ);
	}

	public IChunk GetChunkSync(long _key)
	{
		return ChunkCache?.GetChunkSync(_key);
	}

	public bool IsChunkAreaLoaded(Vector3 _position)
	{
		return IsChunkAreaLoaded(Utils.Fastfloor(_position.x), 0, Utils.Fastfloor(_position.z));
	}

	public bool IsChunkAreaLoaded(int _blockPosX, int _, int _blockPosZ)
	{
		int num = toChunkXZ(_blockPosX - 8);
		int num2 = toChunkXZ(_blockPosZ - 8);
		int num3 = toChunkXZ(_blockPosX + 8);
		int num4 = toChunkXZ(_blockPosZ + 8);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				if (GetChunkSync(i, j) == null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsChunkAreaCollidersLoaded(Vector3 _position)
	{
		return IsChunkAreaCollidersLoaded(Utils.Fastfloor(_position.x), Utils.Fastfloor(_position.z));
	}

	public bool IsChunkAreaCollidersLoaded(int _blockPosX, int _blockPosZ)
	{
		int num = toChunkXZ(_blockPosX - 8);
		int num2 = toChunkXZ(_blockPosZ - 8);
		int num3 = toChunkXZ(_blockPosX + 8);
		int num4 = toChunkXZ(_blockPosZ + 8);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				Chunk chunk = (Chunk)GetChunkSync(i, j);
				if (chunk == null || !chunk.IsCollisionMeshGenerated)
				{
					return false;
				}
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int toChunkXZ(int _v)
	{
		return _v >> 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i toChunkXZ(Vector2i _v)
	{
		return new Vector2i(_v.x >> 4, _v.y >> 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i toChunkXZ(Vector3 _v)
	{
		return new Vector2i(Utils.Fastfloor(_v.x) >> 4, Utils.Fastfloor(_v.z) >> 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i toChunkXZ(Vector3i _v)
	{
		return new Vector2i(_v.x >> 4, _v.z >> 4);
	}

	public static Vector3i toChunkXYZCube(Vector3 _v)
	{
		return new Vector3i(Utils.Fastfloor(_v.x) >> 4, Utils.Fastfloor(_v.y) >> 4, Utils.Fastfloor(_v.z) >> 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i toChunkXYZ(Vector3i _v)
	{
		return new Vector3i(_v.x >> 4, _v.y >> 8, _v.z >> 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int toChunkY(int _v)
	{
		return _v >> 8;
	}

	public static Vector3 toChunkXyzWorldPos(Vector3i _v)
	{
		return new Vector3(_v.x & -16, _v.y & -256, _v.z & -16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i toBlock(Vector3i _p)
	{
		_p.x &= 15;
		_p.y &= 255;
		_p.z &= 15;
		return _p;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i toBlock(int _x, int _y, int _z)
	{
		return new Vector3i(_x & 0xF, _y & 0xFF, _z & 0xF);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int toBlockXZ(int _v)
	{
		return _v & 0xF;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int toBlockY(int _v)
	{
		return _v & 0xFF;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 blockToTransformPos(Vector3i _blockPos)
	{
		return new Vector3((float)_blockPos.x + 0.5f, _blockPos.y, (float)_blockPos.z + 0.5f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i worldToBlockPos(Vector3 _worldPos)
	{
		return new Vector3i(Utils.Fastfloor(_worldPos.x), Utils.Fastfloor(_worldPos.y), Utils.Fastfloor(_worldPos.z));
	}

	public override void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _blockValue, _updateLight: true)
		});
	}

	public override void SetBlockRPC(Vector3i _blockPos, BlockValue _blockValue)
	{
		gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_blockPos, _blockValue, _updateLight: true)
		});
	}

	public override void SetBlockRPC(Vector3i _blockPos, BlockValue _blockValue, sbyte _density)
	{
		gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_blockPos, _blockValue, _density)
		});
	}

	public override void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density)
	{
		gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _blockValue, _density)
		});
	}

	public override void SetBlockRPC(Vector3i _blockPos, sbyte _density)
	{
		gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(0, _blockPos, _density)
		});
	}

	public override void SetBlocksRPC(List<BlockChangeInfo> _blockChangeInfo)
	{
		gameManager.SetBlocksRPC(_blockChangeInfo);
	}

	public override void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density, int _changingEntityId)
	{
		gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _blockValue, _density, _changingEntityId)
		});
	}

	public override void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _changingEntityId)
	{
		gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _blockValue, _updateLight: true, _changingEntityId)
		});
	}

	public BlockValue SetBlock(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool bNotify, bool updateLight)
	{
		return ChunkClusters[_clrIdx].SetBlock(_blockPos, _blockValue, bNotify, updateLight);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsDaytime()
	{
		return !IsDark();
	}

	public bool IsDark()
	{
		float num = (float)(worldTime % 24000) / 1000f;
		if (!(num < (float)DawnHour))
		{
			return num > (float)DuskHour;
		}
		return true;
	}

	public override TileEntity GetTileEntity(int _clrIdx, Vector3i _pos)
	{
		return GetTileEntity(_pos);
	}

	public override TileEntity GetTileEntity(Vector3i _pos)
	{
		ChunkCluster chunkCache = ChunkCache;
		if (chunkCache == null)
		{
			return null;
		}
		Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_pos);
		if (chunk == null)
		{
			return null;
		}
		Vector3i blockPosInChunk = new Vector3i(toBlockXZ(_pos.x), toBlockY(_pos.y), toBlockXZ(_pos.z));
		return chunk.GetTileEntity(blockPosInChunk);
	}

	public TileEntity GetTileEntity(int _entityId)
	{
		Entity entity = GetEntity(_entityId);
		if (entity == null)
		{
			return null;
		}
		if (entity is EntityTrader && entity.IsAlive())
		{
			return ((EntityTrader)entity).TileEntityTrader;
		}
		if (entity.lootContainer == null)
		{
			string lootList = entity.GetLootList();
			if (!string.IsNullOrEmpty(lootList))
			{
				entity.lootContainer = new TileEntityLootContainer((Chunk)null);
				entity.lootContainer.entityId = entity.entityId;
				entity.lootContainer.lootListName = lootList;
				entity.SetLootContainerSize();
			}
		}
		return entity.lootContainer;
	}

	public void RemoveTileEntity(TileEntity _te)
	{
		Chunk chunk = _te.GetChunk();
		if (chunk != null)
		{
			chunk.RemoveTileEntity(this, _te);
		}
		else
		{
			Log.Error("RemoveTileEntity: chunk not found!");
		}
	}

	public BlockTrigger GetBlockTrigger(int _clrIdx, Vector3i _pos)
	{
		ChunkCluster chunkCluster = ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return null;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_pos);
		if (chunk == null)
		{
			return null;
		}
		Vector3i blockPosInChunk = new Vector3i(toBlockXZ(_pos.x), toBlockY(_pos.y), toBlockXZ(_pos.z));
		return chunk.GetBlockTrigger(blockPosInChunk);
	}

	public void OnUpdateTick(float _partialTicks, ArraySegment<long> _activeChunks)
	{
		updateChunkAddedRemovedCallbacks();
		WorldEventUpdateTime();
		WaterSplashCubes.Update();
		DecoManager.Instance.UpdateTick(this);
		MultiBlockManager.Instance.MainThreadUpdate();
		if (!IsEditor())
		{
			dmsConductor.Update();
		}
		checkPOIUnculling();
		updateChunksToUncull();
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		worldBlockTicker.Tick(_activeChunks, m_LocalPlayerEntity, rand);
		if (GameTimer.Instance.ticks % 20 == 0L)
		{
			bool flag = GameStats.GetBool(EnumGameStats.IsSpawnEnemies);
			int num = 0;
			ChunkCluster chunkCluster = ChunkClusters[num];
			bool flag2 = GameTimer.Instance.ticks % 40 == 0;
			for (int i = _activeChunks.Offset; i < _activeChunks.Count; i++)
			{
				long num2 = _activeChunks.Array[i];
				if ((flag2 && num2 % 2 != 0L) || (!flag2 && num2 % 2 == 0L))
				{
					continue;
				}
				int num3 = WorldChunkCache.extractClrIdx(num2);
				if (num3 != num)
				{
					ChunkCluster chunkCluster2 = ChunkClusters[num3];
					if (chunkCluster2 == null)
					{
						continue;
					}
					chunkCluster = chunkCluster2;
					num = num3;
				}
				Chunk chunk = chunkCluster?.GetChunkSync(num2);
				if (chunk == null)
				{
					continue;
				}
				if (chunk.NeedsTicking)
				{
					chunk.UpdateTick(this, flag);
				}
				if (IsEditor() || !chunk.IsAreaMaster() || !chunk.IsAreaMasterDominantBiomeInitialized(chunkCluster))
				{
					continue;
				}
				ChunkAreaBiomeSpawnData chunkBiomeSpawnData = chunk.GetChunkBiomeSpawnData();
				if (chunkBiomeSpawnData != null && chunkBiomeSpawnData.IsSpawnNeeded(Biomes, worldTime) && chunk.IsAreaMasterCornerChunksLoaded(chunkCluster))
				{
					if (areaMasterChunksToLock.ContainsKey(chunk.Key))
					{
						chunk.isModified |= chunkBiomeSpawnData.DelayAllEnemySpawningUntil(areaMasterChunksToLock[chunk.Key], Biomes);
						areaMasterChunksToLock.Remove(chunk.Key);
					}
					else
					{
						biomeSpawnManager.Update(string.Empty, flag, chunkBiomeSpawnData);
					}
				}
			}
		}
		if (GameTimer.Instance.ticks % 16 == 0L && GamePrefs.GetString(EnumGamePrefs.DynamicSpawner).Length > 0)
		{
			dynamicSpawnManager.Update(GamePrefs.GetString(EnumGamePrefs.DynamicSpawner), GameStats.GetBool(EnumGameStats.IsSpawnEnemies), null);
		}
		aiDirector.Tick(_partialTicks / 20f);
		TickSleeperVolumes();
	}

	public bool UncullPOI(PrefabInstance _pi)
	{
		if (_pi.AddChunksToUncull(this, chunksToUncull))
		{
			Log.Out("Unculling POI {0} {1}", _pi.location.Name, _pi.boundingBoxPosition);
			return true;
		}
		return false;
	}

	public void UncullChunk(Chunk _c)
	{
		if (_c.IsInternalBlocksCulled)
		{
			chunksToUncull.Add(_c);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkPOIUnculling()
	{
		if (GameTimer.Instance.ticks % 38 != 0L || GameStats.GetInt(EnumGameStats.OptionsPOICulling) == 0)
		{
			return;
		}
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		for (int i = 0; i < list.Count; i++)
		{
			EntityPlayer entityPlayer = list[i];
			if (!entityPlayer.Spawned)
			{
				continue;
			}
			Dictionary<int, PrefabInstance> prefabsAroundNear = entityPlayer.GetPrefabsAroundNear();
			if (prefabsAroundNear == null)
			{
				continue;
			}
			foreach (KeyValuePair<int, PrefabInstance> item in prefabsAroundNear)
			{
				PrefabInstance value = item.Value;
				if (value.Overlaps(entityPlayer.position, 6f))
				{
					UncullPOI(value);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateChunksToUncull()
	{
		if (chunksToUncull.list.Count == 0)
		{
			return;
		}
		msUnculling.ResetAndRestart();
		chunksToRegenerate.Clear();
		for (int num = chunksToUncull.list.Count - 1; num >= 0; num--)
		{
			Chunk chunk = chunksToUncull.list[num];
			if (chunk.InProgressUnloading)
			{
				chunksToUncull.Remove(chunk);
			}
			else
			{
				BlockFaceFlag num2 = chunk.RestoreCulledBlocks(this);
				chunksToUncull.Remove(chunk);
				if (!chunksToRegenerate.hashSet.Contains(chunk))
				{
					chunksToRegenerate.Add(chunk);
				}
				Chunk chunk2;
				if ((num2 & BlockFaceFlag.West) != BlockFaceFlag.None && (chunk2 = (Chunk)GetChunkSync(chunk.X - 1, chunk.Z)) != null && !chunksToRegenerate.hashSet.Contains(chunk2))
				{
					chunksToRegenerate.Add(chunk2);
				}
				if ((num2 & BlockFaceFlag.East) != BlockFaceFlag.None && (chunk2 = (Chunk)GetChunkSync(chunk.X + 1, chunk.Z)) != null && !chunksToRegenerate.hashSet.Contains(chunk2))
				{
					chunksToRegenerate.Add(chunk2);
				}
				if ((num2 & BlockFaceFlag.North) != BlockFaceFlag.None && (chunk2 = (Chunk)GetChunkSync(chunk.X, chunk.Z + 1)) != null && !chunksToRegenerate.hashSet.Contains(chunk2))
				{
					chunksToRegenerate.Add(chunk2);
				}
				if ((num2 & BlockFaceFlag.South) != BlockFaceFlag.None && (chunk2 = (Chunk)GetChunkSync(chunk.X, chunk.Z - 1)) != null && !chunksToRegenerate.hashSet.Contains(chunk2))
				{
					chunksToRegenerate.Add(chunk2);
				}
				if (msUnculling.ElapsedMilliseconds > 5)
				{
					break;
				}
			}
		}
		for (int num3 = chunksToRegenerate.list.Count - 1; num3 >= 0; num3--)
		{
			chunksToRegenerate.list[num3].NeedsRegeneration = true;
		}
	}

	public Vector3[] GetRandomSpawnPointPositions(int _count)
	{
		Vector3[] array = new Vector3[_count];
		List<Chunk> chunkArrayCopySync = ChunkCache.GetChunkArrayCopySync();
		int count = chunkArrayCopySync.Count;
		while (_count > 0)
		{
			for (int i = 0; i < chunkArrayCopySync.Count; i++)
			{
				Chunk chunk = chunkArrayCopySync[i];
				if (GetGameRandom().RandomRange(count) != 1)
				{
					continue;
				}
				Chunk[] neighbours = new Chunk[8];
				if (ChunkCache.GetNeighborChunks(chunk, neighbours))
				{
					if (chunk.FindRandomTopSoilPoint(this, out var x, out var y, out var z, 5))
					{
						array[^_count] = new Vector3(x, y, z);
						_count--;
					}
					if (_count == 0)
					{
						break;
					}
				}
			}
		}
		return array;
	}

	public Vector3 ClipBoundsMove(Entity _entity, Bounds _aabb, Vector3 move, Vector3 expandDir, float stepHeight)
	{
		if (stepHeight > 0f)
		{
			move.y = stepHeight;
		}
		Bounds bounds = BoundsUtils.ExpandDirectional(_aabb, expandDir);
		int num = Utils.Fastfloor(bounds.min.x - 0.5f);
		int num2 = Utils.Fastfloor(bounds.max.x + 1.5f);
		int num3 = Utils.Fastfloor(bounds.min.y - 0.5f);
		int num4 = Utils.Fastfloor(bounds.max.y + 1f);
		int num5 = Utils.Fastfloor(bounds.min.z - 0.5f);
		int num6 = Utils.Fastfloor(bounds.max.z + 1.5f);
		ClipBlock.ResetStorage();
		int num7 = 0;
		int num8 = 0;
		Chunk chunk = null;
		Vector3 blockPos = default(Vector3);
		for (int i = num; i < num2; i++)
		{
			blockPos.x = i;
			for (int j = num5; j < num6; j++)
			{
				blockPos.z = j;
				if (chunk == null || chunk.X != toChunkXZ(i) || chunk.Z != toChunkXZ(j))
				{
					chunk = (Chunk)GetChunkFromWorldPos(i, j);
					if (chunk == null)
					{
						continue;
					}
					if (!IsInPlayfield(chunk))
					{
						_clipBounds[num8++] = chunk.GetAABB();
					}
				}
				for (int k = num3; k < num4; k++)
				{
					if (k > 0 && k < 256)
					{
						BlockValue block = GetBlock(i, k, j);
						Block block2 = block.Block;
						if (block2.IsCollideMovement)
						{
							float yDistort = 0f;
							blockPos.y = k;
							_clipBlocks[num7++] = ClipBlock.New(block, block2, yDistort, blockPos, _aabb);
						}
					}
				}
			}
		}
		Vector3 min = _aabb.min;
		Vector3 max = _aabb.max;
		if (move.y != 0f && num7 > 0)
		{
			for (int l = 0; l < num7; l++)
			{
				ClipBlock clipBlock = _clipBlocks[l];
				IList<Bounds> clipBoundsList = clipBlock.block.GetClipBoundsList(clipBlock.value, clipBlock.pos);
				move.y = BoundsUtils.ClipBoundsMoveY(clipBlock.bmins, clipBlock.bmaxs, move.y, clipBoundsList, clipBoundsList.Count);
				if (move.y == 0f)
				{
					break;
				}
			}
		}
		if (move.y != 0f)
		{
			if (num8 > 0)
			{
				move.y = BoundsUtils.ClipBoundsMoveY(min, max, move.y, _clipBounds, num8);
			}
			min.y += move.y;
			max.y += move.y;
			for (int m = 0; m < num7; m++)
			{
				ClipBlock obj = _clipBlocks[m];
				obj.bmins.y += move.y;
				obj.bmaxs.y += move.y;
			}
		}
		if (move.x != 0f && num7 > 0)
		{
			for (int n = 0; n < num7; n++)
			{
				ClipBlock clipBlock2 = _clipBlocks[n];
				IList<Bounds> clipBoundsList2 = clipBlock2.block.GetClipBoundsList(clipBlock2.value, clipBlock2.pos);
				move.x = BoundsUtils.ClipBoundsMoveX(clipBlock2.bmins, clipBlock2.bmaxs, move.x, clipBoundsList2, clipBoundsList2.Count);
				if (move.x == 0f)
				{
					break;
				}
			}
		}
		if (move.x != 0f)
		{
			if (num8 > 0)
			{
				move.x = BoundsUtils.ClipBoundsMoveX(min, max, move.x, _clipBounds, num8);
			}
			min.x += move.x;
			max.x += move.x;
			for (int num9 = 0; num9 < num7; num9++)
			{
				ClipBlock obj2 = _clipBlocks[num9];
				obj2.bmins.x += move.x;
				obj2.bmaxs.x += move.x;
			}
		}
		if (move.z != 0f && num7 > 0)
		{
			for (int num10 = 0; num10 < num7; num10++)
			{
				ClipBlock clipBlock3 = _clipBlocks[num10];
				IList<Bounds> clipBoundsList3 = clipBlock3.block.GetClipBoundsList(clipBlock3.value, clipBlock3.pos);
				move.z = BoundsUtils.ClipBoundsMoveZ(clipBlock3.bmins, clipBlock3.bmaxs, move.z, clipBoundsList3, clipBoundsList3.Count);
				if (move.z == 0f)
				{
					break;
				}
			}
		}
		if (move.z != 0f)
		{
			if (num8 > 0)
			{
				move.z = BoundsUtils.ClipBoundsMoveZ(min, max, move.z, _clipBounds, num8);
			}
			min.z += move.z;
			max.z += move.z;
			for (int num11 = 0; num11 < num7; num11++)
			{
				ClipBlock obj3 = _clipBlocks[num11];
				obj3.bmins.z += move.z;
				obj3.bmaxs.z += move.z;
			}
		}
		if (stepHeight > 0f)
		{
			stepHeight = 0f - stepHeight;
			if (num7 > 0)
			{
				for (int num12 = 0; num12 < num7; num12++)
				{
					ClipBlock clipBlock4 = _clipBlocks[num12];
					IList<Bounds> clipBoundsList4 = clipBlock4.block.GetClipBoundsList(clipBlock4.value, clipBlock4.pos);
					stepHeight = BoundsUtils.ClipBoundsMoveY(clipBlock4.bmins, clipBlock4.bmaxs, stepHeight, clipBoundsList4, clipBoundsList4.Count);
					if (stepHeight == 0f)
					{
						break;
					}
				}
			}
			if (stepHeight != 0f && num8 > 0)
			{
				stepHeight = BoundsUtils.ClipBoundsMoveY(min, max, stepHeight, _clipBounds, num8);
			}
			move.y += stepHeight;
		}
		return move;
	}

	public List<Bounds> GetCollidingBounds(Entity _entity, Bounds _aabb, List<Bounds> collidingBoundingBoxes)
	{
		int num = Utils.Fastfloor(_aabb.min.x - 0.5f);
		int num2 = Utils.Fastfloor(_aabb.max.x + 0.5f);
		int num3 = Utils.Fastfloor(_aabb.min.y - 1f);
		int num4 = Utils.Fastfloor(_aabb.max.y + 1f);
		int num5 = Utils.Fastfloor(_aabb.min.z - 0.5f);
		int num6 = Utils.Fastfloor(_aabb.max.z + 0.5f);
		Chunk chunk = null;
		int num7 = num - 1;
		int num8 = 0;
		while (num7 <= num2 + 1)
		{
			if (num8 >= 50)
			{
				Log.Warning($"1BB exceeded size {50}: BB={_aabb.ToCultureInvariantString()}");
				return collidingBoundingBoxes;
			}
			int i = num5 - 1;
			for (int j = 0; i <= num6 + 1; i++, j++)
			{
				if (j >= 50)
				{
					Log.Warning($"2BB exceeded size {50}: BB={_aabb.ToCultureInvariantString()}");
					return collidingBoundingBoxes;
				}
				if (chunk == null || chunk.X != toChunkXZ(num7) || chunk.Z != toChunkXZ(i))
				{
					chunk = (Chunk)GetChunkFromWorldPos(num7, i);
					if (chunk == null)
					{
						continue;
					}
					if (!IsInPlayfield(chunk))
					{
						collidingBoundingBoxes.Add(chunk.GetAABB());
					}
				}
				int x = toBlockXZ(num7);
				int z = toBlockXZ(i);
				int num9 = num3;
				int num10 = 0;
				while (num9 < num4)
				{
					if (num9 > 0 && num9 < 255)
					{
						BlockValue block = chunk.GetBlock(x, num9, z);
						if (num10 >= 50)
						{
							Log.Warning($"3BB exceeded size {50}: BB={_aabb.ToCultureInvariantString()}");
							return collidingBoundingBoxes;
						}
						collBlockCache[num8, num10, j] = block;
						collDensityCache[num8, num10, j] = chunk.GetDensity(x, num9, z);
					}
					num9++;
					num10++;
				}
			}
			num7++;
			num8++;
		}
		int num11 = num;
		int num12 = 0;
		while (num11 <= num2)
		{
			if (num12 >= 50)
			{
				Log.Warning($"4BB exceeded size {50}: BB={_aabb.ToCultureInvariantString()}");
				return collidingBoundingBoxes;
			}
			int num13 = num5;
			int num14 = 0;
			while (num13 <= num6)
			{
				if (num14 >= 50)
				{
					Log.Warning($"5BB exceeded size {50}: BB={_aabb.ToCultureInvariantString()}");
					return collidingBoundingBoxes;
				}
				int num15 = num3;
				int num16 = 0;
				while (num15 < num4)
				{
					if (num15 > 0 && num15 < 255)
					{
						if (num16 >= 50)
						{
							Log.Warning($"6BB exceeded size {50}: BB={_aabb.ToCultureInvariantString()}");
							return collidingBoundingBoxes;
						}
						BlockValue blockValue = collBlockCache[num12 + 1, num16, num14 + 1];
						Block block2 = blockValue.Block;
						if (block2.IsCollideMovement)
						{
							float distortedAddY = 0f;
							if (block2.shape.IsTerrain())
							{
								distortedAddY = MarchingCubes.GetDecorationOffsetY(collDensityCache[num12 + 1, num16 + 1, num14 + 1], collDensityCache[num12 + 1, num16, num14 + 1]);
							}
							block2.GetCollidingAABB(blockValue, num11, num15, num13, distortedAddY, _aabb, collidingBoundingBoxes);
						}
					}
					num15++;
					num16++;
				}
				num13++;
				num14++;
			}
			num11++;
			num12++;
		}
		Bounds aabbOfEntity = _aabb;
		aabbOfEntity.Expand(0.25f);
		List<Entity> entitiesInBounds = GetEntitiesInBounds(_entity, aabbOfEntity);
		for (int k = 0; k < entitiesInBounds.Count; k++)
		{
			Bounds boundingBox = entitiesInBounds[k].getBoundingBox();
			if (boundingBox.Intersects(_aabb))
			{
				collidingBoundingBoxes.Add(boundingBox);
			}
			boundingBox = _entity.getBoundingBox();
			if (boundingBox.Intersects(_aabb))
			{
				collidingBoundingBoxes.Add(boundingBox);
			}
		}
		return collidingBoundingBoxes;
	}

	public List<Entity> GetEntitiesInBounds(Entity _excludeEntity, Bounds _aabbOfEntity)
	{
		entitiesWithinAABBExcludingEntity.Clear();
		int num = Utils.Fastfloor((_aabbOfEntity.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_aabbOfEntity.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_aabbOfEntity.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_aabbOfEntity.max.z + 5f) / 16f);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				ChunkCache.GetChunkSync(i, j)?.GetEntitiesInBounds(_excludeEntity, _aabbOfEntity, entitiesWithinAABBExcludingEntity, isAlive: true);
			}
		}
		return entitiesWithinAABBExcludingEntity;
	}

	public List<Entity> GetEntitiesInBounds(Entity _excludeEntity, Bounds _aabbOfEntity, bool _isAlive)
	{
		entitiesWithinAABBExcludingEntity.Clear();
		int num = Utils.Fastfloor((_aabbOfEntity.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_aabbOfEntity.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_aabbOfEntity.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_aabbOfEntity.max.z + 5f) / 16f);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				ChunkCache.GetChunkSync(i, j)?.GetEntitiesInBounds(_excludeEntity, _aabbOfEntity, entitiesWithinAABBExcludingEntity, _isAlive);
			}
		}
		return entitiesWithinAABBExcludingEntity;
	}

	public List<EntityAlive> GetLivingEntitiesInBounds(EntityAlive _excludeEntity, Bounds _aabbOfEntity)
	{
		livingEntitiesWithinAABBExcludingEntity.Clear();
		int num = Utils.Fastfloor((_aabbOfEntity.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_aabbOfEntity.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_aabbOfEntity.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_aabbOfEntity.max.z + 5f) / 16f);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				ChunkCache.GetChunkSync(i, j)?.GetLivingEntitiesInBounds(_excludeEntity, _aabbOfEntity, livingEntitiesWithinAABBExcludingEntity);
			}
		}
		return livingEntitiesWithinAABBExcludingEntity;
	}

	public void GetEntitiesInBounds(FastTags<TagGroup.Global> _tags, Bounds _bb, List<Entity> _list)
	{
		int num = Utils.Fastfloor((_bb.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_bb.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_bb.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_bb.max.z + 5f) / 16f);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				((Chunk)GetChunkSync(j, i))?.GetEntitiesInBounds(_tags, _bb, _list);
			}
		}
	}

	public List<Entity> GetEntitiesInBounds(Type _class, Bounds _bb, List<Entity> _list)
	{
		int num = Utils.Fastfloor((_bb.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_bb.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_bb.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_bb.max.z + 5f) / 16f);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				((Chunk)GetChunkSync(j, i))?.GetEntitiesInBounds(_class, _bb, _list);
			}
		}
		return _list;
	}

	public void GetEntitiesAround(EntityFlags _mask, Vector3 _pos, float _radius, List<Entity> _list)
	{
		int num = Utils.Fastfloor((_pos.x - _radius) / 16f);
		int num2 = Utils.Fastfloor((_pos.x + _radius) / 16f);
		int num3 = Utils.Fastfloor((_pos.z - _radius) / 16f);
		int num4 = Utils.Fastfloor((_pos.z + _radius) / 16f);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				((Chunk)GetChunkSync(j, i))?.GetEntitiesAround(_mask, _pos, _radius, _list);
			}
		}
	}

	public void GetEntitiesAround(EntityFlags _flags, EntityFlags _mask, Vector3 _pos, float _radius, List<Entity> _list)
	{
		_flags &= _mask;
		int num = Utils.Fastfloor((_pos.x - _radius) / 16f);
		int num2 = Utils.Fastfloor((_pos.x + _radius) / 16f);
		int num3 = Utils.Fastfloor((_pos.z - _radius) / 16f);
		int num4 = Utils.Fastfloor((_pos.z + _radius) / 16f);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				((Chunk)GetChunkSync(j, i))?.GetEntitiesAround(_flags, _mask, _pos, _radius, _list);
			}
		}
	}

	public int GetEntityAliveCount(EntityFlags _flags, EntityFlags _mask)
	{
		int num = 0;
		int count = EntityAlives.Count;
		for (int i = 0; i < count; i++)
		{
			if ((EntityAlives[i].entityFlags & _mask) == _flags)
			{
				num++;
			}
		}
		return num;
	}

	public void GetPlayersAround(Vector3 _pos, float _radius, List<EntityPlayer> _list)
	{
		float num = _radius * _radius;
		for (int num2 = Players.list.Count - 1; num2 >= 0; num2--)
		{
			EntityPlayer entityPlayer = Players.list[num2];
			if ((entityPlayer.position - _pos).sqrMagnitude <= num)
			{
				_list.Add(entityPlayer);
			}
		}
	}

	public void SetEntitiesVisibleNearToLocalPlayer()
	{
		EntityPlayerLocal primaryPlayer = GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		bool aimingGun = primaryPlayer.AimingGun;
		Vector3 vector = primaryPlayer.cameraTransform.position + Origin.position;
		for (int num = Entities.list.Count - 1; num >= 0; num--)
		{
			Entity entity = Entities.list[num];
			if (entity != primaryPlayer)
			{
				entity.VisiblityCheck((entity.position - vector).sqrMagnitude, aimingGun);
			}
		}
	}

	public void TickEntities(float _partialTicks)
	{
		int frameCount = Time.frameCount;
		int num = frameCount - tickEntityFrameCount;
		if (num <= 0)
		{
			num = 1;
		}
		tickEntityFrameCount = frameCount;
		tickEntityFrameCountAverage = tickEntityFrameCountAverage * 0.8f + (float)num * 0.2f;
		tickEntityPartialTicks = _partialTicks;
		tickEntityIndex = 0;
		tickEntityList.Clear();
		Entity primaryPlayer = GetPrimaryPlayer();
		int count = Entities.list.Count;
		for (int i = 0; i < count; i++)
		{
			Entity entity = Entities.list[i];
			if (entity != primaryPlayer)
			{
				tickEntityList.Add(entity);
			}
		}
		if ((bool)primaryPlayer)
		{
			TickEntity(primaryPlayer, _partialTicks);
		}
		EntityActivityUpdate();
		int num2 = (int)(tickEntityFrameCountAverage + 0.4f) - 1;
		if (num2 <= 0)
		{
			TickEntitiesFlush();
			return;
		}
		int num3 = (tickEntityList.Count - 25) / (num2 + 1);
		if (num3 < 0)
		{
			num3 = 0;
		}
		tickEntitySliceCount = (tickEntityList.Count - num3) / num2 + 1;
	}

	public void TickEntitiesFlush()
	{
		TickEntitiesSlice(tickEntityList.Count);
	}

	public void TickEntitiesSlice()
	{
		TickEntitiesSlice(tickEntitySliceCount);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickEntitiesSlice(int count)
	{
		int num = Utils.FastMin(tickEntityIndex + count, tickEntityList.Count);
		for (int i = tickEntityIndex; i < num; i++)
		{
			Entity entity = tickEntityList[i];
			if ((bool)entity)
			{
				TickEntity(entity, tickEntityPartialTicks);
			}
		}
		tickEntityIndex = num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickEntity(Entity e, float _partialTicks)
	{
		e.SetLastTickPos(e.position);
		e.OnUpdatePosition(_partialTicks);
		e.CheckPosition();
		if (e.IsSpawned() && !e.IsMarkedForUnload())
		{
			Chunk chunk = (Chunk)GetChunkSync(e.chunkPosAddedEntityTo.x, e.chunkPosAddedEntityTo.z);
			bool flag = false;
			if (chunk != null)
			{
				if (!chunk.hasEntities)
				{
					flag = true;
				}
				else
				{
					chunk.AdJustEntityTracking(e);
				}
			}
			int num = toChunkXZ(Utils.Fastfloor(e.position.x));
			int num2 = toChunkXZ(Utils.Fastfloor(e.position.z));
			if (flag || !e.addedToChunk || e.chunkPosAddedEntityTo.x != num || e.chunkPosAddedEntityTo.z != num2)
			{
				if (e.addedToChunk)
				{
					chunk?.RemoveEntityFromChunk(e);
				}
				chunk = (Chunk)GetChunkSync(num, num2);
				if (chunk != null)
				{
					e.addedToChunk = true;
					chunk.AddEntityToChunk(e);
				}
				else
				{
					e.addedToChunk = false;
				}
			}
			if (e is EntityPlayer || e is EntityDrone || IsChunkAreaLoaded(e.position))
			{
				if (e.CanUpdateEntity())
				{
					e.OnUpdateEntity();
				}
				else if (e is EntityAlive)
				{
					((EntityAlive)e).CheckDespawn();
				}
			}
			else
			{
				EntityAlive entityAlive = e as EntityAlive;
				if (entityAlive != null)
				{
					entityAlive.SetAttackTarget(null, 0);
					entityAlive.CheckDespawn();
				}
			}
		}
		if (e.IsMarkedForUnload() && !e.isEntityRemote && !e.bWillRespawn)
		{
			unloadEntity(e, e.IsDespawned ? EnumRemoveEntityReason.Despawned : EnumRemoveEntityReason.Killed);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickEntityRemove(Entity e)
	{
		int num = tickEntityList.IndexOf(e);
		if (num >= tickEntityIndex)
		{
			tickEntityList[num] = null;
		}
	}

	public void EntityActivityUpdate()
	{
		List<EntityPlayer> list = Players.list;
		if (list.Count == 0)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			list[num].aiClosest.Clear();
		}
		int count = EntityAlives.Count;
		for (int i = 0; i < count; i++)
		{
			EntityAlive entityAlive = EntityAlives[i];
			EntityPlayer closestPlayer = GetClosestPlayer(entityAlive.position, -1f, _isDead: false);
			if ((bool)closestPlayer)
			{
				closestPlayer.aiClosest.Add(entityAlive);
				entityAlive.aiClosestPlayer = closestPlayer;
				entityAlive.aiClosestPlayerDistSq = (closestPlayer.position - entityAlive.position).sqrMagnitude;
			}
			else
			{
				entityAlive.aiClosestPlayer = null;
				entityAlive.aiClosestPlayerDistSq = float.MaxValue;
			}
		}
		Vector3 vector = Vector3.zero;
		float num2 = 0f;
		if ((bool)m_LocalPlayerEntity)
		{
			vector = m_LocalPlayerEntity.cameraTransform.position + Origin.position;
			m_LocalPlayerEntity.emodel.ClothSimOn(!m_LocalPlayerEntity.AttachedToEntity);
			num2 = 625f;
			if (m_LocalPlayerEntity.AimingGun)
			{
				num2 = 3025f;
			}
		}
		int num3 = Utils.FastClamp(60 / list.Count, 4, 20);
		for (int num4 = list.Count - 1; num4 >= 0; num4--)
		{
			EntityPlayer entityPlayer = list[num4];
			entityPlayer.aiClosest.Sort([PublicizedFrom(EAccessModifier.Internal)] (EntityAlive e1, EntityAlive e2) => e1.aiClosestPlayerDistSq.CompareTo(e2.aiClosestPlayerDistSq));
			for (int num5 = 0; num5 < entityPlayer.aiClosest.Count; num5++)
			{
				EntityAlive entityAlive2 = entityPlayer.aiClosest[num5];
				if (num5 < num3 || entityAlive2.aiClosestPlayerDistSq < 64f)
				{
					entityAlive2.aiActiveScale = 1f;
					bool flag = entityAlive2.aiClosestPlayerDistSq < 36f;
					entityAlive2.emodel.JiggleOn(flag);
				}
				else
				{
					float aiActiveScale = ((entityAlive2.aiClosestPlayerDistSq < 225f) ? 0.3f : 0.1f);
					entityAlive2.aiActiveScale = aiActiveScale;
					entityAlive2.emodel.JiggleOn(_on: false);
				}
			}
			if (entityPlayer != m_LocalPlayerEntity)
			{
				bool flag2 = !entityPlayer.AttachedToEntity && (entityPlayer.position - vector).sqrMagnitude < num2;
				entityPlayer.emodel.ClothSimOn(flag2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addToChunk(Entity e)
	{
		if (!e.addedToChunk)
		{
			((Chunk)GetChunkFromWorldPos(e.GetBlockPosition()))?.AddEntityToChunk(e);
		}
	}

	public override void UnloadEntities(List<Entity> _entityList, bool _forceUnload = false)
	{
		for (int num = _entityList.Count - 1; num >= 0; num--)
		{
			Entity entity = _entityList[num];
			if (_forceUnload || (!entity.bWillRespawn && (!(entity.AttachedMainEntity != null) || !entity.AttachedMainEntity.bWillRespawn)))
			{
				unloadEntity(entity, EnumRemoveEntityReason.Unloaded);
			}
		}
	}

	public override Entity RemoveEntity(int _entityId, EnumRemoveEntityReason _reason)
	{
		Entity entity = GetEntity(_entityId);
		if (entity != null)
		{
			entity.MarkToUnload();
			unloadEntity(entity, _reason);
		}
		return entity;
	}

	public void unloadEntity(Entity _e, EnumRemoveEntityReason _reason)
	{
		EnumRemoveEntityReason unloadReason = _e.unloadReason;
		_e.unloadReason = _reason;
		if (!Entities.dict.ContainsKey(_e.entityId))
		{
			Log.Warning("{0} World unloadEntity !dict {1}, {2}, was {3}", GameManager.frameCount, _e, _reason, unloadReason);
			return;
		}
		if (this.EntityUnloadedDelegates != null)
		{
			this.EntityUnloadedDelegates(_e, _reason);
		}
		if (_e.NavObject != null)
		{
			if (_reason == EnumRemoveEntityReason.Unloaded && _e is EntitySupplyCrate)
			{
				_e.NavObject.TrackedPosition = _e.position;
			}
			else
			{
				NavObjectManager.Instance.UnRegisterNavObject(_e.NavObject);
			}
		}
		_e.OnEntityUnload();
		Entities.Remove(_e.entityId);
		TickEntityRemove(_e);
		EntityAlive entityAlive = _e as EntityAlive;
		if ((bool)entityAlive)
		{
			EntityAlives.Remove(entityAlive);
		}
		RemoveEntityFromMap(_e, _reason);
		if (_e.addedToChunk && _e.IsMarkedForUnload())
		{
			Chunk chunk = (Chunk)GetChunkSync(_e.chunkPosAddedEntityTo.x, _e.chunkPosAddedEntityTo.z);
			if (chunk != null && !chunk.InProgressUnloading)
			{
				chunk.RemoveEntityFromChunk(_e);
			}
		}
		if (!IsRemote())
		{
			if (VehicleManager.Instance != null)
			{
				EntityVehicle entityVehicle = _e as EntityVehicle;
				if ((bool)entityVehicle)
				{
					VehicleManager.Instance.RemoveTrackedVehicle(entityVehicle, _reason);
				}
			}
			if (DroneManager.Instance != null)
			{
				EntityDrone entityDrone = _e as EntityDrone;
				if ((bool)entityDrone)
				{
					DroneManager.Instance.RemoveTrackedDrone(entityDrone, _reason);
				}
			}
			if (TurretTracker.Instance != null)
			{
				EntityTurret entityTurret = _e as EntityTurret;
				if ((bool)entityTurret)
				{
					TurretTracker.Instance.RemoveTrackedTurret(entityTurret, _reason);
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			entityDistributer.Remove(_e, _reason);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _e is EntityAlive && PathFinderThread.Instance != null)
		{
			PathFinderThread.Instance.RemovePathsFor(_e.entityId);
		}
		if (_e is EntityPlayer)
		{
			Players.Remove(_e.entityId);
			gameManager.HandlePersistentPlayerDisconnected(_e.entityId);
			playerEntityUpdateCount++;
			NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(_e, "sleeping_bag");
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			aiDirector.RemoveEntity(_e);
		}
		audioManager.EntityRemovedFromWorld(_e, this);
		WeatherManager.EntityRemovedFromWorld(_e);
		LightManager.EntityRemovedFromWorld(_e, this);
	}

	public override Entity GetEntity(int _entityId)
	{
		Entities.dict.TryGetValue(_entityId, out var value);
		return value;
	}

	public override void ChangeClientEntityIdToServer(int _clientEntityId, int _serverEntityId)
	{
		Entity entity = GetEntity(_clientEntityId);
		if ((bool)entity)
		{
			Entities.Remove(_clientEntityId);
			entity.entityId = _serverEntityId;
			entity.clientEntityId = 0;
			Entities.Add(_serverEntityId, entity);
		}
	}

	public void SpawnEntityInWorld(Entity _entity)
	{
		if (_entity == null)
		{
			Log.Warning("Ignore spawning of empty entity");
			return;
		}
		if (this.EntityLoadedDelegates != null)
		{
			this.EntityLoadedDelegates(_entity);
		}
		AddEntityToMap(_entity);
		Entities.Add(_entity.entityId, _entity);
		addToChunk(_entity);
		EntityPlayer entityPlayer = _entity as EntityPlayer;
		EntityAlive entityAlive = ((!entityPlayer) ? (_entity as EntityAlive) : null);
		if ((bool)entityAlive)
		{
			EntityAlives.Add(entityAlive);
		}
		if (!IsRemote())
		{
			if (_entity is EntityVehicle vehicle && VehicleManager.Instance != null)
			{
				VehicleManager.Instance.AddTrackedVehicle(vehicle);
			}
			if (_entity is EntityDrone entityDrone)
			{
				if (DroneManager.Instance != null)
				{
					DroneManager.Instance.AddTrackedDrone(entityDrone);
				}
				if (entityDrone.OriginalItemValue == null)
				{
					entityDrone.InitDynamicSpawn();
				}
			}
			if (_entity is EntityTurret entityTurret)
			{
				if (TurretTracker.Instance != null)
				{
					TurretTracker.Instance.AddTrackedTurret(entityTurret);
				}
				if (entityTurret.OriginalItemValue.ItemClass == null)
				{
					entityTurret.InitDynamicSpawn();
				}
			}
		}
		if (audioManager != null)
		{
			audioManager.EntityAddedToWorld(_entity, this);
		}
		WeatherManager.EntityAddedToWorld(_entity);
		LightManager.EntityAddedToWorld(_entity, this);
		_entity.OnAddedToWorld();
		if (_entity.position.y < 1f)
		{
			Log.Warning("Spawned entity with wrong pos: " + _entity?.ToString() + " id=" + _entity.entityId + " pos=" + _entity.position.ToCultureInvariantString());
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			entityDistributer.Add(_entity);
		}
		if ((bool)entityPlayer)
		{
			Players.Add(_entity.entityId, entityPlayer);
			playerEntityUpdateCount++;
		}
		else if ((bool)entityAlive)
		{
			entityAlive.Spawned = true;
			GameEventManager.Current.HandleSpawnModifier(entityAlive);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			aiDirector.AddEntity(_entity);
		}
	}

	public void AddEntityToMap(Entity _entity)
	{
		if (_entity == null || !_entity.HasUIIcon() || _entity.GetMapObjectType() != EnumMapObjectType.Entity)
		{
			return;
		}
		if (_entity is EntityVehicle vehicle)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(primaryPlayer);
				if (uIForPlayer != null && uIForPlayer.xui != null && uIForPlayer.xui.GetWindow("mapArea") != null)
				{
					((XUiC_MapArea)uIForPlayer.xui.GetWindow("mapArea").Controller).RefreshVehiclePositionWaypoint(vehicle, _unloaded: false);
				}
			}
		}
		else if (_entity is EntityDrone drone)
		{
			EntityPlayerLocal primaryPlayer2 = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer2 != null)
			{
				LocalPlayerUI uIForPlayer2 = LocalPlayerUI.GetUIForPlayer(primaryPlayer2);
				if (uIForPlayer2 != null && uIForPlayer2.xui != null && uIForPlayer2.xui.GetWindow("mapArea") != null)
				{
					((XUiC_MapArea)uIForPlayer2.xui.GetWindow("mapArea").Controller).RefreshDronePositionWaypoint(drone, _unloaded: false);
				}
			}
		}
		else if (_entity is EntityEnemy || _entity is EntityEnemyAnimal)
		{
			ObjectOnMapAdd(new MapObjectZombie(_entity));
		}
		else if (_entity is EntityAnimal)
		{
			ObjectOnMapAdd(new MapObjectAnimal(_entity));
		}
		else
		{
			ObjectOnMapAdd(new MapObject(EnumMapObjectType.Entity, Vector3.zero, _entity.entityId, _entity, _bSelectable: false));
		}
	}

	public void RemoveEntityFromMap(Entity _entity, EnumRemoveEntityReason _reason)
	{
		if (_entity == null)
		{
			return;
		}
		EnumMapObjectType mapObjectType = _entity.GetMapObjectType();
		if (mapObjectType == EnumMapObjectType.SupplyDrop)
		{
			if (_reason == EnumRemoveEntityReason.Killed)
			{
				ObjectOnMapRemove(_entity.GetMapObjectType(), _entity.entityId);
			}
			return;
		}
		if (_entity is EntityVehicle entityVehicle)
		{
			EntityPlayerLocal primaryPlayer = GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(primaryPlayer);
				if (uIForPlayer != null)
				{
					switch (_reason)
					{
					case EnumRemoveEntityReason.Unloaded:
						if (entityVehicle.GetOwner() != null && entityVehicle.LocalPlayerIsOwner())
						{
							((XUiC_MapArea)uIForPlayer.xui.GetWindow("mapArea").Controller).RefreshVehiclePositionWaypoint(entityVehicle, _unloaded: true);
						}
						break;
					case EnumRemoveEntityReason.Killed:
					case EnumRemoveEntityReason.Despawned:
						((XUiC_MapArea)uIForPlayer.xui.GetWindow("mapArea").Controller).RemoveVehicleLastKnownWaypoint(entityVehicle);
						break;
					}
				}
			}
		}
		else if (_entity is EntityDrone entityDrone)
		{
			EntityPlayerLocal primaryPlayer2 = GetPrimaryPlayer();
			if (primaryPlayer2 != null)
			{
				LocalPlayerUI uIForPlayer2 = LocalPlayerUI.GetUIForPlayer(primaryPlayer2);
				if (uIForPlayer2 != null)
				{
					switch (_reason)
					{
					case EnumRemoveEntityReason.Unloaded:
						if (entityDrone.LocalPlayerIsOwner())
						{
							((XUiC_MapArea)uIForPlayer2.xui.GetWindow("mapArea").Controller).RefreshDronePositionWaypoint(entityDrone, _unloaded: true);
						}
						break;
					case EnumRemoveEntityReason.Killed:
					case EnumRemoveEntityReason.Despawned:
						((XUiC_MapArea)uIForPlayer2.xui.GetWindow("mapArea").Controller).RemoveDronePositionWaypoint(entityDrone.entityId);
						break;
					}
				}
			}
		}
		ObjectOnMapRemove(mapObjectType, _entity.entityId);
	}

	public void RefreshEntitiesOnMap()
	{
		foreach (Entity item in Entities.list)
		{
			RemoveEntityFromMap(item, EnumRemoveEntityReason.Undef);
			AddEntityToMap(item);
		}
	}

	public void LockAreaMasterChunksAround(Vector3i _blockPos, ulong _worldTimeToLock)
	{
		for (int i = -2; i <= 2; i++)
		{
			for (int j = -2; j <= 2; j++)
			{
				Vector3i vector3i = Chunk.ToAreaMasterChunkPos(new Vector3i(_blockPos.x + i * 80, 0, _blockPos.z + j * 80));
				Chunk chunk = (Chunk)GetChunkSync(vector3i.x, vector3i.z);
				if (chunk != null && chunk.GetChunkBiomeSpawnData() != null)
				{
					chunk.isModified |= chunk.GetChunkBiomeSpawnData().DelayAllEnemySpawningUntil(_worldTimeToLock, Biomes);
				}
				else
				{
					areaMasterChunksToLock[WorldChunkCache.MakeChunkKey(vector3i.x, vector3i.z)] = _worldTimeToLock;
				}
			}
		}
	}

	public bool IsWaterInBounds(Bounds _aabb)
	{
		Vector3 min = _aabb.min;
		Vector3 max = _aabb.max;
		int num = Utils.Fastfloor(min.x);
		int num2 = Utils.Fastfloor(max.x + 1f);
		int num3 = Utils.Fastfloor(min.y);
		int num4 = Utils.Fastfloor(max.y + 1f);
		int num5 = Utils.Fastfloor(min.z);
		int num6 = Utils.Fastfloor(max.z + 1f);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				for (int k = num5; k < num6; k++)
				{
					if (IsWater(i, j, k))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsMaterialInBounds(Bounds _aabb, MaterialBlock _material)
	{
		int num = Utils.Fastfloor(_aabb.min.x);
		int num2 = Utils.Fastfloor(_aabb.max.x + 1f);
		int num3 = Utils.Fastfloor(_aabb.min.y);
		int num4 = Utils.Fastfloor(_aabb.max.y + 1f);
		int num5 = Utils.Fastfloor(_aabb.min.z);
		int num6 = Utils.Fastfloor(_aabb.max.z + 1f);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				for (int k = num5; k < num6; k++)
				{
					if (GetBlock(i, j, k).Block.blockMaterial == _material)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override void AddFallingBlocks(IList<Vector3i> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			AddFallingBlock(_list[i]);
		}
	}

	public void AddFallingBlock(Vector3i _blockPos, bool includeOversized = false)
	{
		if (!fallingBlocksMap.ContainsKey(_blockPos))
		{
			BlockValue block = GetBlock(_blockPos);
			if (!block.ischild && !block.Block.StabilityIgnore && !block.isair && (includeOversized || !block.Block.isOversized))
			{
				DynamicMeshManager.AddFallingBlockObserver(_blockPos);
				fallingBlocks.Enqueue(_blockPos);
				fallingBlocksMap[_blockPos] = Time.time;
			}
		}
	}

	public void LetBlocksFall()
	{
		if (fallingBlocks.Count == 0)
		{
			return;
		}
		int num = 0;
		Vector3i zero = Vector3i.zero;
		while (fallingBlocks.Count > 0 && num < 2)
		{
			Vector3i vector3i = fallingBlocks.Dequeue();
			if (zero.Equals(vector3i))
			{
				fallingBlocks.Enqueue(vector3i);
				break;
			}
			fallingBlocksMap.Remove(vector3i);
			BlockValue block = GetBlock(vector3i.x, vector3i.y, vector3i.z);
			if (!block.isair)
			{
				TextureFullArray textureFullArray = GetTextureFullArray(vector3i.x, vector3i.y, vector3i.z);
				Block block2 = block.Block;
				block2.OnBlockStartsToFall(this, vector3i, block);
				DynamicMeshManager.ChunkChanged(vector3i, -1, block.type);
				if (block2.ShowModelOnFall())
				{
					EntityFallingBlock entity = (EntityFallingBlock)EntityFactory.CreateEntity(_transformPos: new Vector3((float)vector3i.x + 0.5f + RandomRange(-0.1f, 0.1f), (float)vector3i.y + 0.5f, (float)vector3i.z + 0.5f + RandomRange(-0.1f, 0.1f)), _et: EntityClass.FromString("fallingBlock"), _id: -1, _blockValue: block, _textureFull: textureFullArray, _count: 1, _transformRot: Vector3.zero, _lifetime: -1f, _playerId: -1, _skinName: null);
					SpawnEntityInWorld(entity);
					num++;
				}
			}
		}
	}

	public override IGameManager GetGameManager()
	{
		return gameManager;
	}

	public override Manager GetAudioManager()
	{
		return audioManager;
	}

	public override AIDirector GetAIDirector()
	{
		return aiDirector;
	}

	public override bool IsRemote()
	{
		return !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
	}

	public EntityPlayer GetClosestPlayer(float _x, float _y, float _z, int _notFromThisTeam, double _maxDistance)
	{
		float num = -1f;
		EntityPlayer result = null;
		for (int i = 0; i < Players.list.Count; i++)
		{
			EntityPlayer entityPlayer = Players.list[i];
			if (!entityPlayer.IsDead() && entityPlayer.Spawned && (_notFromThisTeam == 0 || entityPlayer.TeamNumber != _notFromThisTeam))
			{
				float distanceSq = entityPlayer.GetDistanceSq(new Vector3(_x, _y, _z));
				if ((_maxDistance < 0.0 || (double)distanceSq < _maxDistance * _maxDistance) && (num == -1f || distanceSq < num))
				{
					num = distanceSq;
					result = entityPlayer;
				}
			}
		}
		return result;
	}

	public EntityPlayer GetClosestPlayer(Entity _entity, float _distMax, bool _isDead)
	{
		return GetClosestPlayer(_entity.position, _distMax, _isDead);
	}

	public EntityPlayer GetClosestPlayer(Vector3 _pos, float _distMax, bool _isDead)
	{
		if (_distMax < 0f)
		{
			_distMax = float.MaxValue;
		}
		float num = _distMax * _distMax;
		EntityPlayer result = null;
		float num2 = float.MaxValue;
		for (int num3 = Players.list.Count - 1; num3 >= 0; num3--)
		{
			EntityPlayer entityPlayer = Players.list[num3];
			if (entityPlayer.IsDead() == _isDead && entityPlayer.Spawned)
			{
				float distanceSq = entityPlayer.GetDistanceSq(_pos);
				if (distanceSq < num2 && distanceSq <= num)
				{
					num2 = distanceSq;
					result = entityPlayer;
				}
			}
		}
		return result;
	}

	public EntityPlayer GetClosestPlayerSeen(EntityAlive _entity, float _distMax, float lightMin)
	{
		Vector3 position = _entity.position;
		if (_distMax < 0f)
		{
			_distMax = float.MaxValue;
		}
		float num = _distMax * _distMax;
		EntityPlayer result = null;
		float num2 = float.MaxValue;
		for (int num3 = Players.list.Count - 1; num3 >= 0; num3--)
		{
			EntityPlayer entityPlayer = Players.list[num3];
			if (!entityPlayer.IsDead() && entityPlayer.Spawned)
			{
				float distanceSq = entityPlayer.GetDistanceSq(position);
				if (distanceSq < num2 && distanceSq <= num && entityPlayer.Stealth.lightLevel >= lightMin && _entity.CanSee(entityPlayer))
				{
					num2 = distanceSq;
					result = entityPlayer;
				}
			}
		}
		return result;
	}

	public bool IsPlayerAliveAndNear(Vector3 _pos, float _distMax)
	{
		float num = _distMax * _distMax;
		for (int num2 = Players.list.Count - 1; num2 >= 0; num2--)
		{
			EntityPlayer entityPlayer = Players.list[num2];
			if (!entityPlayer.IsDead() && entityPlayer.Spawned && (entityPlayer.position - _pos).sqrMagnitude <= num)
			{
				return true;
			}
		}
		return false;
	}

	public override WorldBlockTicker GetWBT()
	{
		return worldBlockTicker;
	}

	public override bool IsOpenSkyAbove(int _clrIdx, int _x, int _y, int _z)
	{
		if (ChunkClusters[_clrIdx] == null)
		{
			return true;
		}
		return ((Chunk)GetChunkSync(_x >> 4, _z >> 4)).IsOpenSkyAbove(_x & 0xF, _y, _z & 0xF);
	}

	public override bool IsEditor()
	{
		return GameManager.Instance.IsEditMode();
	}

	public int GetGameMode()
	{
		return GameStats.GetInt(EnumGameStats.GameModeId);
	}

	public SpawnManagerDynamic GetDynamiceSpawnManager()
	{
		return dynamicSpawnManager;
	}

	public override bool CanPlaceLandProtectionBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return true;
		}
		if (InBoundsForPlayersPercent(blockPos.ToVector3CenterXZ()) < 0.5f)
		{
			return false;
		}
		m_lpChunkList.Clear();
		int num = GameStats.GetInt(EnumGameStats.LandClaimSize) - 1;
		int num2 = GameStats.GetInt(EnumGameStats.LandClaimDeadZone) + num;
		int num3 = num2 / 16 + 1;
		int num4 = num2 / 16 + 1;
		for (int i = -num3; i <= num3; i++)
		{
			int x = blockPos.x + i * 16;
			for (int j = -num4; j <= num4; j++)
			{
				int z = blockPos.z + j * 16;
				Chunk chunk = (Chunk)GetChunkFromWorldPos(new Vector3i(x, blockPos.y, z));
				if (chunk != null && !m_lpChunkList.Contains(chunk))
				{
					m_lpChunkList.Add(chunk);
					if (IsLandProtectedBlock(chunk, blockPos, lpRelative, num, num2, forKeystone: true))
					{
						m_lpChunkList.Clear();
						return false;
					}
				}
			}
		}
		int num5 = num2 / 2;
		Vector3i vector3i = new Vector3i(num5, num5, num5);
		Vector3i minPos = blockPos - vector3i;
		Vector3i maxPos = blockPos + vector3i;
		if (IsWithinTraderArea(minPos, maxPos))
		{
			return false;
		}
		m_lpChunkList.Clear();
		return true;
	}

	public bool IsEmptyPosition(Vector3i blockPos)
	{
		if (IsWithinTraderArea(blockPos))
		{
			return false;
		}
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return true;
		}
		m_lpChunkList.Clear();
		int num = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num2 = (num - 1) / 2;
		int num3 = num / 16 + 1;
		int num4 = num / 16 + 1;
		int num5 = blockPos.x - num2;
		int num6 = blockPos.z - num2;
		for (int i = -num3; i <= num3; i++)
		{
			int x = num5 + i * 16;
			for (int j = -num4; j <= num4; j++)
			{
				int z = num6 + j * 16;
				Chunk chunk = (Chunk)GetChunkFromWorldPos(new Vector3i(x, blockPos.y, z));
				if (chunk != null && !m_lpChunkList.Contains(chunk))
				{
					m_lpChunkList.Add(chunk);
					if (IsLandProtectedBlock(chunk, blockPos, null, num2, num2, forKeystone: false))
					{
						m_lpChunkList.Clear();
						return false;
					}
				}
			}
		}
		m_lpChunkList.Clear();
		return true;
	}

	public override bool CanPickupBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative)
	{
		if (IsWithinTraderArea(blockPos))
		{
			return false;
		}
		return CanPlaceBlockAt(blockPos, lpRelative);
	}

	public override bool CanPlaceBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative, bool traderAllowed = false)
	{
		if (!traderAllowed && IsWithinTraderArea(blockPos))
		{
			return false;
		}
		if (InBoundsForPlayersPercent(blockPos.ToVector3CenterXZ()) < 0.5f)
		{
			return false;
		}
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return true;
		}
		m_lpChunkList.Clear();
		int num = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num2 = (num - 1) / 2;
		int num3 = num / 16 + 1;
		int num4 = num / 16 + 1;
		int num5 = blockPos.x - num2;
		int num6 = blockPos.z - num2;
		for (int i = -num3; i <= num3; i++)
		{
			int x = num5 + i * 16;
			for (int j = -num4; j <= num4; j++)
			{
				int z = num6 + j * 16;
				Chunk chunk = (Chunk)GetChunkFromWorldPos(new Vector3i(x, blockPos.y, z));
				if (chunk != null && !m_lpChunkList.Contains(chunk))
				{
					m_lpChunkList.Add(chunk);
					if (IsLandProtectedBlock(chunk, blockPos, lpRelative, num2, num2, forKeystone: false))
					{
						m_lpChunkList.Clear();
						return false;
					}
				}
			}
		}
		m_lpChunkList.Clear();
		return true;
	}

	public override float GetLandProtectionHardnessModifier(Vector3i blockPos, EntityAlive lpRelative, PersistentPlayerData ppData)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return 1f;
		}
		if (lpRelative is EntityEnemy || lpRelative == null)
		{
			return 1f;
		}
		float num = 1f;
		BlockValue block = GetBlock(blockPos);
		if (!block.Equals(BlockValue.Air))
		{
			num = block.Block.LPHardnessScale;
			if (num == 0f)
			{
				return 1f;
			}
		}
		m_lpChunkList.Clear();
		int num2 = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num3 = (num2 - 1) / 2;
		int num4 = num2 / 16 + 1;
		int num5 = num2 / 16 + 1;
		int num6 = blockPos.x - num3;
		int num7 = blockPos.z - num3;
		float num8 = 1f;
		for (int i = -num4; i <= num4; i++)
		{
			int x = num6 + i * 16;
			for (int j = -num5; j <= num5; j++)
			{
				int z = num7 + j * 16;
				Chunk chunk = (Chunk)GetChunkFromWorldPos(new Vector3i(x, blockPos.y, z));
				if (chunk != null && !m_lpChunkList.Contains(chunk))
				{
					m_lpChunkList.Add(chunk);
					float landProtectionHardnessModifier = GetLandProtectionHardnessModifier(chunk, blockPos, ppData, num3);
					if (landProtectionHardnessModifier < 1f)
					{
						m_lpChunkList.Clear();
						return landProtectionHardnessModifier;
					}
					num8 = Math.Max(num8, landProtectionHardnessModifier);
				}
			}
		}
		m_lpChunkList.Clear();
		if (num8 > 1f)
		{
			if (lpRelative is EntityVehicle)
			{
				num8 *= 2f;
			}
			return num8 * num;
		}
		return num8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetLandProtectionHardnessModifier(Chunk chunk, Vector3i blockPos, PersistentPlayerData lpRelative, int halfClaimSize)
	{
		float num = 1f;
		PersistentPlayerList persistentPlayerList = gameManager.GetPersistentPlayerList();
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i vector3i = list[i] + worldPos;
				if (!BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					continue;
				}
				PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(vector3i);
				if (landProtectionBlockOwner == null || (lpRelative != null && (landProtectionBlockOwner == lpRelative || (!(blockPos == vector3i) && landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId)))))
				{
					continue;
				}
				int num2 = Math.Abs(vector3i.x - blockPos.x);
				int num3 = Math.Abs(vector3i.z - blockPos.z);
				if (num2 <= halfClaimSize && num3 <= halfClaimSize)
				{
					float landProtectionHardnessModifierForPlayer = GetLandProtectionHardnessModifierForPlayer(landProtectionBlockOwner);
					if (landProtectionHardnessModifierForPlayer < 1f)
					{
						return landProtectionHardnessModifierForPlayer;
					}
					num = Mathf.Max(num, landProtectionHardnessModifierForPlayer);
					if (lpRelative != null)
					{
						EntityPlayer entityPlayer = GetEntity(lpRelative.EntityId) as EntityPlayer;
						num = EffectManager.GetValue(PassiveEffects.LandClaimDamageModifier, entityPlayer.inventory.holdingItemItemValue, num, entityPlayer);
					}
				}
			}
		}
		return num;
	}

	public override bool IsMyLandProtectedBlock(Vector3i worldBlockPos, PersistentPlayerData lpRelative, bool traderAllowed = false)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return true;
		}
		if (!traderAllowed && IsWithinTraderArea(worldBlockPos))
		{
			return false;
		}
		m_lpChunkList.Clear();
		int num = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num2 = (num - 1) / 2;
		int num3 = num / 16 + 1;
		int num4 = num / 16 + 1;
		int num5 = worldBlockPos.x - num2;
		int num6 = worldBlockPos.z - num2;
		for (int i = -num3; i <= num3; i++)
		{
			int x = num5 + i * 16;
			for (int j = -num4; j <= num4; j++)
			{
				int z = num6 + j * 16;
				Chunk chunk = (Chunk)GetChunkFromWorldPos(new Vector3i(x, worldBlockPos.y, z));
				if (chunk != null && !m_lpChunkList.Contains(chunk))
				{
					m_lpChunkList.Add(chunk);
					if (IsMyLandClaimInChunk(chunk, worldBlockPos, lpRelative, num2, num2, forKeystone: false))
					{
						m_lpChunkList.Clear();
						return true;
					}
				}
			}
		}
		m_lpChunkList.Clear();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsLandProtectedBlock(Chunk chunk, Vector3i blockPos, PersistentPlayerData lpRelative, int claimSize, int deadZone, bool forKeystone)
	{
		PersistentPlayerList persistentPlayerList = gameManager.GetPersistentPlayerList();
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i pos = list[i] + worldPos;
				if (!BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					continue;
				}
				int num = Math.Abs(pos.x - blockPos.x);
				int num2 = Math.Abs(pos.z - blockPos.z);
				if (num > deadZone || num2 > deadZone)
				{
					continue;
				}
				PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(pos);
				if (landProtectionBlockOwner == null)
				{
					continue;
				}
				bool flag = IsLandProtectionValidForPlayer(landProtectionBlockOwner);
				if (flag && lpRelative != null)
				{
					if (lpRelative == landProtectionBlockOwner)
					{
						flag = false;
					}
					else if (landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId))
					{
						flag = num <= claimSize && num2 <= claimSize && forKeystone;
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsMyLandClaimInChunk(Chunk chunk, Vector3i blockPos, PersistentPlayerData lpRelative, int claimSize, int deadZone, bool forKeystone)
	{
		PersistentPlayerList persistentPlayerList = gameManager.GetPersistentPlayerList();
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i pos = list[i] + worldPos;
				if (!BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					continue;
				}
				int num = Math.Abs(pos.x - blockPos.x);
				int num2 = Math.Abs(pos.z - blockPos.z);
				if (num > deadZone || num2 > deadZone)
				{
					continue;
				}
				PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(pos);
				if (landProtectionBlockOwner != null)
				{
					bool flag = IsLandProtectionValidForPlayer(landProtectionBlockOwner);
					if (flag && lpRelative != null)
					{
						flag = ((lpRelative == landProtectionBlockOwner) ? (num <= claimSize && num2 <= claimSize) : (landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId) && num <= claimSize && num2 <= claimSize && forKeystone));
					}
					if (flag)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override EnumLandClaimOwner GetLandClaimOwner(Vector3i worldBlockPos, PersistentPlayerData lpRelative)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return EnumLandClaimOwner.Self;
		}
		if (IsWithinTraderArea(worldBlockPos))
		{
			return EnumLandClaimOwner.None;
		}
		m_lpChunkList.Clear();
		int num = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num2 = (num - 1) / 2;
		int num3 = num / 16 + 1;
		int num4 = num / 16 + 1;
		int num5 = worldBlockPos.x - num2;
		int num6 = worldBlockPos.z - num2;
		for (int i = -num3; i <= num3; i++)
		{
			int x = num5 + i * 16;
			for (int j = -num4; j <= num4; j++)
			{
				int z = num6 + j * 16;
				Chunk chunk = (Chunk)GetChunkFromWorldPos(new Vector3i(x, worldBlockPos.y, z));
				if (chunk != null && !m_lpChunkList.Contains(chunk))
				{
					m_lpChunkList.Add(chunk);
					EnumLandClaimOwner landClaimOwner = GetLandClaimOwner(chunk, worldBlockPos, lpRelative, num2, num2, forKeystone: false);
					if (landClaimOwner != EnumLandClaimOwner.None)
					{
						m_lpChunkList.Clear();
						return landClaimOwner;
					}
				}
			}
		}
		m_lpChunkList.Clear();
		return EnumLandClaimOwner.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumLandClaimOwner GetLandClaimOwner(Chunk chunk, Vector3i blockPos, PersistentPlayerData lpRelative, int claimSize, int deadZone, bool forKeystone)
	{
		PersistentPlayerList persistentPlayerList = gameManager.GetPersistentPlayerList();
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i pos = list[i] + worldPos;
				if (!BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					continue;
				}
				int num = Math.Abs(pos.x - blockPos.x);
				int num2 = Math.Abs(pos.z - blockPos.z);
				if (num > deadZone || num2 > deadZone)
				{
					continue;
				}
				PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(pos);
				if (landProtectionBlockOwner != null && IsLandProtectionValidForPlayer(landProtectionBlockOwner))
				{
					if (lpRelative == null)
					{
						return EnumLandClaimOwner.Other;
					}
					if (lpRelative == landProtectionBlockOwner)
					{
						return EnumLandClaimOwner.Self;
					}
					if (landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId))
					{
						return EnumLandClaimOwner.Ally;
					}
					return EnumLandClaimOwner.Other;
				}
			}
		}
		return EnumLandClaimOwner.None;
	}

	public bool GetLandClaimOwnerInParty(EntityPlayer player, PersistentPlayerData lpRelative)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return false;
		}
		Vector3i blockPosition = player.GetBlockPosition();
		if (IsWithinTraderArea(blockPosition))
		{
			return false;
		}
		m_lpChunkList.Clear();
		int num = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num2 = (num - 1) / 2;
		int num3 = num / 16 + 1;
		int num4 = num / 16 + 1;
		int num5 = blockPosition.x - num2;
		int num6 = blockPosition.z - num2;
		for (int i = -num3; i <= num3; i++)
		{
			int x = num5 + i * 16;
			for (int j = -num4; j <= num4; j++)
			{
				int z = num6 + j * 16;
				Chunk chunk = (Chunk)GetChunkFromWorldPos(new Vector3i(x, blockPosition.y, z));
				if (chunk != null && !m_lpChunkList.Contains(chunk))
				{
					m_lpChunkList.Add(chunk);
					if (GetLandClaimOwnerInParty(chunk, player, blockPosition, lpRelative, num2, num2))
					{
						m_lpChunkList.Clear();
						return true;
					}
				}
			}
		}
		m_lpChunkList.Clear();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetLandClaimOwnerInParty(Chunk chunk, EntityPlayer player, Vector3i blockPos, PersistentPlayerData lpRelative, int claimSize, int deadZone)
	{
		PersistentPlayerList persistentPlayerList = gameManager.GetPersistentPlayerList();
		bool flag = player.Party != null;
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i pos = list[i] + worldPos;
				if (!BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					continue;
				}
				int num = Math.Abs(pos.x - blockPos.x);
				int num2 = Math.Abs(pos.z - blockPos.z);
				if (num > deadZone || num2 > deadZone)
				{
					continue;
				}
				PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(pos);
				if (landProtectionBlockOwner == null)
				{
					continue;
				}
				if (lpRelative == null && player != null && landProtectionBlockOwner.EntityId == player.entityId)
				{
					lpRelative = landProtectionBlockOwner;
				}
				if (IsLandProtectionValidForPlayer(landProtectionBlockOwner) && lpRelative != null)
				{
					if (lpRelative == landProtectionBlockOwner)
					{
						if (num <= claimSize && num2 <= claimSize)
						{
							return true;
						}
					}
					else if (flag && landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId) && player.Party.ContainsMember(landProtectionBlockOwner.EntityId) && num <= claimSize && num2 <= claimSize)
					{
						return true;
					}
				}
				return false;
			}
		}
		return false;
	}

	public bool IsLandProtectionValidForPlayer(PersistentPlayerData ppData)
	{
		double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
		if (ppData.OfflineHours > num)
		{
			return false;
		}
		return true;
	}

	public float GetLandProtectionHardnessModifierForPlayer(PersistentPlayerData ppData)
	{
		float result = GameStats.GetInt(EnumGameStats.LandClaimOnlineDurabilityModifier);
		if (ppData.EntityId != -1)
		{
			return result;
		}
		double offlineHours = ppData.OfflineHours;
		double offlineMinutes = ppData.OfflineMinutes;
		float num = GameStats.GetInt(EnumGameStats.LandClaimOfflineDelay);
		if (num != 0f && offlineMinutes <= (double)num)
		{
			return result;
		}
		double num2 = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
		if (offlineHours > num2)
		{
			return 1f;
		}
		EnumLandClaimDecayMode enumLandClaimDecayMode = (EnumLandClaimDecayMode)GameStats.GetInt(EnumGameStats.LandClaimDecayMode);
		float num3 = GameStats.GetInt(EnumGameStats.LandClaimOfflineDurabilityModifier);
		if (num3 == 0f)
		{
			return 0f;
		}
		switch (enumLandClaimDecayMode)
		{
		case EnumLandClaimDecayMode.BuffedUntilExpired:
			return num3;
		case EnumLandClaimDecayMode.DecaySlowly:
		{
			double num5 = (offlineHours - 24.0) / (num2 - 24.0);
			return Mathf.Max(1f, (float)(1.0 - num5) * num3);
		}
		default:
		{
			double num4 = (offlineHours - 24.0) / (num2 - 24.0);
			return Mathf.Max(1f, (float)((1.0 - num4) * (1.0 - num4)) * num3);
		}
		}
	}

	public float GetDecorationOffsetY(Vector3i _blockPos)
	{
		sbyte density = GetDensity(0, _blockPos);
		sbyte density2 = GetDensity(0, _blockPos - Vector3i.up);
		return MarchingCubes.GetDecorationOffsetY(density, density2);
	}

	public EnumDecoAllowed GetDecoAllowedAt(int _x, int _z)
	{
		return ((Chunk)GetChunkFromWorldPos(_x, _z))?.GetDecoAllowedAt(toBlockXZ(_x), toBlockXZ(_z)) ?? EnumDecoAllowed.Nothing;
	}

	public void SetDecoAllowedAt(int _x, int _z, EnumDecoAllowed _decoAllowed)
	{
		((Chunk)GetChunkFromWorldPos(_x, _z))?.SetDecoAllowedAt(toBlockXZ(_x), toBlockXZ(_z), _decoAllowed);
	}

	public Vector3 GetTerrainNormalAt(int _x, int _z)
	{
		return ((Chunk)GetChunkFromWorldPos(_x, _z))?.GetTerrainNormal(toBlockXZ(_x), toBlockXZ(_z)) ?? Vector3.zero;
	}

	public bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		return ChunkCache.ChunkProvider.GetWorldExtent(out _minSize, out _maxSize);
	}

	public virtual bool IsPositionAvailable(int _clrIdx, Vector3 _position)
	{
		ChunkCluster chunkCluster = ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		Vector3i vector3i = worldToBlockPos(_position);
		for (int i = 0; i < Vector3i.MIDDLE_AND_HORIZONTAL_DIRECTIONS_DIAGONAL.Length; i++)
		{
			Vector3i vector3i2 = Vector3i.MIDDLE_AND_HORIZONTAL_DIRECTIONS_DIAGONAL[i] * 16;
			IChunk chunkFromWorldPos = chunkCluster.GetChunkFromWorldPos(vector3i + vector3i2);
			if (chunkFromWorldPos == null || !chunkFromWorldPos.GetAvailable())
			{
				return false;
			}
		}
		return true;
	}

	public bool GetBiomeIntensity(Vector3i _position, out BiomeIntensity _biomeIntensity)
	{
		Chunk chunk = (Chunk)GetChunkFromWorldPos(_position);
		if (chunk != null && !chunk.NeedsLightCalculation)
		{
			_biomeIntensity = chunk.GetBiomeIntensity(toBlockXZ(_position.x), toBlockXZ(_position.z));
			return true;
		}
		_biomeIntensity = BiomeIntensity.Default;
		return false;
	}

	public bool CanMobsSpawnAtPos(Vector3 _pos)
	{
		Vector3i blockPos = worldToBlockPos(_pos);
		return ((Chunk)GetChunkFromWorldPos(blockPos))?.CanMobsSpawnAtPos(toBlockXZ(blockPos.x), toBlockY(blockPos.y), toBlockXZ(blockPos.z)) ?? false;
	}

	public bool CanSleeperSpawnAtPos(Vector3 _pos, bool _checkBelow)
	{
		Vector3i blockPos = worldToBlockPos(_pos);
		return ((Chunk)GetChunkFromWorldPos(blockPos))?.CanSleeperSpawnAtPos(toBlockXZ(blockPos.x), toBlockY(blockPos.y), toBlockXZ(blockPos.z), _checkBelow) ?? false;
	}

	public bool CanPlayersSpawnAtPos(Vector3 _pos, bool _bAllowToSpawnOnAirPos = false)
	{
		Vector3i blockPos = worldToBlockPos(_pos);
		return ((Chunk)GetChunkFromWorldPos(blockPos))?.CanPlayersSpawnAtPos(toBlockXZ(blockPos.x), toBlockY(blockPos.y), toBlockXZ(blockPos.z), _bAllowToSpawnOnAirPos) ?? false;
	}

	public void CheckEntityCollisionWithBlocks(Entity _entity)
	{
		if (!_entity.CanCollideWithBlocks())
		{
			return;
		}
		for (int i = 0; i < ChunkClusters.Count; i++)
		{
			ChunkCluster chunkCluster = ChunkClusters[i];
			if (chunkCluster != null && chunkCluster.Overlaps(_entity.boundingBox))
			{
				chunkCluster.CheckCollisionWithBlocks(_entity);
			}
		}
	}

	public void OnChunkAdded(Chunk _c)
	{
		lock (newlyLoadedChunksThisUpdate)
		{
			newlyLoadedChunksThisUpdate.Add(_c.Key);
		}
	}

	public void OnChunkBeforeRemove(Chunk _c)
	{
		lock (newlyLoadedChunksThisUpdate)
		{
			newlyLoadedChunksThisUpdate.Remove(_c.Key);
		}
		if (worldBlockTicker != null)
		{
			worldBlockTicker.OnChunkRemoved(_c);
		}
		GameManager.Instance.prefabLODManager.TriggerUpdate();
	}

	public void OnChunkBeforeSave(Chunk _c)
	{
		if (worldBlockTicker != null)
		{
			worldBlockTicker.OnChunkBeforeSave(_c);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateChunkAddedRemovedCallbacks()
	{
		lock (newlyLoadedChunksThisUpdate)
		{
			int num = 0;
			ChunkCluster chunkCluster = ChunkClusters[num];
			for (int num2 = newlyLoadedChunksThisUpdate.Count - 1; num2 >= 0; num2--)
			{
				long key = newlyLoadedChunksThisUpdate[num2];
				int num3 = WorldChunkCache.extractClrIdx(key);
				if (num3 != num)
				{
					num = num3;
					chunkCluster = ChunkClusters[num];
				}
				if (chunkCluster != null)
				{
					Chunk chunkSync = chunkCluster.GetChunkSync(key);
					if (chunkSync != null && !chunkSync.NeedsDecoration)
					{
						chunkSync.OnLoad(this);
						if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
						{
							worldBlockTicker.OnChunkAdded(this, chunkSync, rand);
						}
						newlyLoadedChunksThisUpdate.RemoveAt(num2);
					}
				}
			}
		}
	}

	public override ulong GetWorldTime()
	{
		return worldTime;
	}

	public override WorldCreationData GetWorldCreationData()
	{
		return wcd;
	}

	public bool IsEntityInRange(int _entityId, int _refEntity, int _range)
	{
		if (_entityId == _refEntity)
		{
			return true;
		}
		if (Entities.dict.TryGetValue(_entityId, out var value) && Entities.dict.TryGetValue(_refEntity, out var value2))
		{
			return value.GetDistanceSq(value2) <= (float)(_range * _range);
		}
		return false;
	}

	public bool IsEntityInRange(int _entityId, Vector3 _position, int _range)
	{
		if (Entities.dict.TryGetValue(_entityId, out var value))
		{
			return value.GetDistanceSq(_position) <= (float)(_range * _range);
		}
		return false;
	}

	public bool IsPositionInBounds(Vector3 position)
	{
		GetWorldExtent(out var _minSize, out var _maxSize);
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Navezgane")
		{
			_minSize = new Vector3i(-2900, _minSize.y, -2900);
			_maxSize = new Vector3i(2900, _maxSize.y, 2900);
		}
		else if (!GameUtils.IsPlaytesting())
		{
			_minSize = new Vector3i(_minSize.x + 90, _minSize.y, _minSize.z + 90);
			_maxSize = new Vector3i(_maxSize.x - 90, _maxSize.y, _maxSize.z - 90);
		}
		Vector3Int vector3Int = _minSize;
		Vector3Int vector3Int2 = _maxSize;
		return new BoundsInt(vector3Int, vector3Int2 - vector3Int).Contains(Vector3Int.RoundToInt(position));
	}

	public float InBoundsForPlayersPercent(Vector3 _pos)
	{
		GetWorldExtent(out var _minSize, out var _maxSize);
		if (_maxSize.x - _minSize.x < 1024)
		{
			return 1f;
		}
		Vector2 vector = default(Vector2);
		vector.x = (float)(_minSize.x + _maxSize.x) * 0.5f;
		vector.y = (float)(_minSize.z + _maxSize.z) * 0.5f;
		float v = ((!(_pos.x < vector.x)) ? (((float)_maxSize.x - 50f - _pos.x) / 80f) : ((_pos.x - ((float)_minSize.x + 50f)) / 80f));
		v = Utils.FastClamp01(v);
		float v2 = ((!(_pos.z < vector.y)) ? (((float)_maxSize.z - 50f - _pos.z) / 80f) : ((_pos.z - ((float)_minSize.z + 50f)) / 80f));
		v2 = Utils.FastClamp01(v2);
		return Utils.FastMin(v, v2);
	}

	public bool AdjustBoundsForPlayers(ref Vector3 _pos, float _padPercent)
	{
		GetWorldExtent(out var _minSize, out var _maxSize);
		if (_maxSize.x - _minSize.x < 1024)
		{
			return false;
		}
		if (_maxSize.x == 0)
		{
			return false;
		}
		int num = (int)(50f + 80f * _padPercent);
		_minSize.x += num;
		_minSize.z += num;
		_maxSize.x -= num;
		_maxSize.z -= num;
		bool result = false;
		if (_pos.x < (float)_minSize.x)
		{
			_pos.x = _minSize.x;
			result = true;
		}
		else if (_pos.x > (float)_maxSize.x)
		{
			_pos.x = _maxSize.x;
			result = true;
		}
		if (_pos.z < (float)_minSize.z)
		{
			_pos.z = _minSize.z;
			result = true;
		}
		else if (_pos.z > (float)_maxSize.z)
		{
			_pos.z = _maxSize.z;
			result = true;
		}
		return result;
	}

	public bool IsPositionRadiated(Vector3 position)
	{
		IChunkProvider chunkProvider = ChunkCache.ChunkProvider;
		IBiomeProvider biomeProvider;
		if (chunkProvider != null && (biomeProvider = chunkProvider.GetBiomeProvider()) != null)
		{
			return biomeProvider.GetRadiationAt((int)position.x, (int)position.z) > 0f;
		}
		return false;
	}

	public bool IsPositionWithinPOI(Vector3 position, int offset)
	{
		return ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetPrefabFromWorldPosInsideWithOffset((int)position.x, (int)position.z, offset) != null;
	}

	public PrefabInstance GetPOIAtPosition(Vector3 _position, bool _checkTags = true)
	{
		return ChunkCache.ChunkProvider.GetDynamicPrefabDecorator()?.GetPrefabAtPosition(_position, _checkTags);
	}

	public void GetPOIsAtXZ(int _xMin, int _xMax, int _zMin, int _zMax, List<PrefabInstance> _list)
	{
		ChunkCache.ChunkProvider.GetDynamicPrefabDecorator()?.GetPrefabsAtXZ(_xMin, _xMax, _zMin, _zMax, _list);
	}

	public Vector3 ClampToValidWorldPos(Vector3 position)
	{
		GetWorldExtent(out var _minSize, out var _maxSize);
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Navezgane")
		{
			_minSize = new Vector3i(-2900, _minSize.y, -2900);
			_maxSize = new Vector3i(2900, _maxSize.y, 2900);
		}
		else if (!GameUtils.IsPlaytesting())
		{
			_minSize = new Vector3i(_minSize.x + 90, _minSize.y, _minSize.z + 90);
			_maxSize = new Vector3i(_maxSize.x - 90, _maxSize.y, _maxSize.z - 90);
		}
		float x = Mathf.Clamp(position.x, _minSize.x, _maxSize.x);
		float y = Mathf.Clamp(position.y, _minSize.y, _maxSize.y);
		float z = Mathf.Clamp(position.z, _minSize.z, _maxSize.z);
		return new Vector3(x, y, z);
	}

	public Vector3 ClampToValidWorldPosForMap(Vector2 position)
	{
		GetWorldExtent(out var _minSize, out var _maxSize);
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Navezgane")
		{
			_minSize = new Vector3i(-2920, _minSize.y, -2920);
			_maxSize = new Vector3i(2920, _maxSize.y, 2920);
		}
		float x = Mathf.Clamp(position.x, _minSize.x, _maxSize.x);
		float y = Mathf.Clamp(position.y, _minSize.z, _maxSize.z);
		return new Vector2(x, y);
	}

	public void ObjectOnMapAdd(MapObject _mo)
	{
		if (objectsOnMap != null)
		{
			objectsOnMap.Add(_mo);
		}
	}

	public void ObjectOnMapRemove(EnumMapObjectType _type, int _key)
	{
		if (objectsOnMap != null)
		{
			objectsOnMap.Remove(_type, _key);
		}
	}

	public void ObjectOnMapRemove(EnumMapObjectType _type, Vector3 _position)
	{
		if (objectsOnMap != null)
		{
			objectsOnMap.RemoveByPosition(_type, _position);
		}
	}

	public void ObjectOnMapRemove(EnumMapObjectType _type)
	{
		if (objectsOnMap != null)
		{
			objectsOnMap.RemoveByType(_type);
		}
	}

	public List<MapObject> GetObjectOnMapList(EnumMapObjectType _type)
	{
		if (objectsOnMap != null)
		{
			return objectsOnMap.GetList(_type);
		}
		return new List<MapObject>();
	}

	public void DebugAddSpawnedEntity(Entity entity)
	{
		if (!(GetPrimaryPlayer() == null) && entity is EntityAlive)
		{
			EntityAlive entityAlive = (EntityAlive)entity;
			SSpawnedEntity item = new SSpawnedEntity
			{
				distanceToLocalPlayer = (entityAlive.GetPosition() - GetPrimaryPlayer().GetPosition()).magnitude,
				name = entityAlive.EntityName,
				pos = entityAlive.GetPosition(),
				timeSpawned = Time.time
			};
			Last4Spawned.Add(item);
			if (Last4Spawned.Count > 4)
			{
				Last4Spawned.RemoveAt(0);
			}
		}
	}

	public static void SetWorldAreas(List<TraderArea> _traders)
	{
		traderAreas = _traders;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTraders()
	{
		if (traderAreas == null)
		{
			return;
		}
		DynamicPrefabDecorator dynamicPrefabDecorator = ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			dynamicPrefabDecorator.ClearTraders();
			for (int i = 0; i < traderAreas.Count; i++)
			{
				dynamicPrefabDecorator.AddTrader(traderAreas[i]);
			}
		}
		traderAreas = null;
	}

	public bool IsWithinTraderArea(Vector3i _worldBlockPos)
	{
		return GetTraderAreaAt(_worldBlockPos) != null;
	}

	public bool IsWithinTraderPlacingProtection(Vector3i _worldBlockPos)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			return dynamicPrefabDecorator.GetTraderAtPosition(_worldBlockPos, 2) != null;
		}
		return false;
	}

	public bool IsWithinTraderPlacingProtection(Bounds _bounds)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			_bounds.Expand(4f);
			Vector3i minPos = worldToBlockPos(_bounds.min);
			Vector3i maxPos = worldToBlockPos(_bounds.max);
			return dynamicPrefabDecorator.IsWithinTraderArea(minPos, maxPos);
		}
		return false;
	}

	public bool IsWithinTraderArea(Vector3i _minPos, Vector3i _maxPos)
	{
		return ChunkCache.ChunkProvider.GetDynamicPrefabDecorator()?.IsWithinTraderArea(_minPos, _maxPos) ?? false;
	}

	public TraderArea GetTraderAreaAt(Vector3i _pos)
	{
		return ChunkCache.ChunkProvider.GetDynamicPrefabDecorator()?.GetTraderAtPosition(_pos, 0);
	}

	public override int AddSleeperVolume(SleeperVolume _sleeperVolume)
	{
		lock (sleeperVolumes)
		{
			sleeperVolumes.Add(_sleeperVolume);
			if (!sleeperVolumeMap.TryGetValue(_sleeperVolume.BoxMin, out var value))
			{
				value = new List<int>();
				sleeperVolumeMap.Add(_sleeperVolume.BoxMin, value);
			}
			value.Add(sleeperVolumes.Count - 1);
			return sleeperVolumes.Count - 1;
		}
	}

	public override int FindSleeperVolume(Vector3i mins, Vector3i maxs)
	{
		if (sleeperVolumeMap.TryGetValue(mins, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				int num = value[i];
				if (sleeperVolumes[num].BoxMax == maxs)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public override int GetSleeperVolumeCount()
	{
		return sleeperVolumes.Count;
	}

	public override SleeperVolume GetSleeperVolume(int index)
	{
		return sleeperVolumes[index];
	}

	public void CheckSleeperVolumeTouching(EntityPlayer _player)
	{
		if (!GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			return;
		}
		Vector3i blockPosition = _player.GetBlockPosition();
		Chunk chunk = (Chunk)GetChunkFromWorldPos(blockPosition);
		if (chunk == null)
		{
			return;
		}
		List<int> list = chunk.GetSleeperVolumes();
		lock (sleeperVolumes)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i];
				if (num < sleeperVolumes.Count)
				{
					sleeperVolumes[num].CheckTouching(this, _player);
				}
			}
		}
	}

	public void CheckSleeperVolumeNoise(Vector3 position)
	{
		if (!GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			return;
		}
		position.y += 0.1f;
		Chunk chunk = (Chunk)GetChunkFromWorldPos(worldToBlockPos(position));
		if (chunk == null)
		{
			return;
		}
		List<int> list = chunk.GetSleeperVolumes();
		lock (sleeperVolumes)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i];
				if (num < sleeperVolumes.Count)
				{
					sleeperVolumes[num].CheckNoise(this, position);
				}
			}
		}
	}

	public void WriteSleeperVolumes(BinaryWriter _bw)
	{
		_bw.Write(sleeperVolumes.Count);
		for (int i = 0; i < sleeperVolumes.Count; i++)
		{
			sleeperVolumes[i].Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadSleeperVolumes(BinaryReader _br)
	{
		sleeperVolumes.Clear();
		sleeperVolumeMap.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			SleeperVolume sleeperVolume = SleeperVolume.Read(_br);
			sleeperVolumes.Add(sleeperVolume);
			if (!sleeperVolumeMap.TryGetValue(sleeperVolume.BoxMin, out var value))
			{
				value = new List<int>();
				sleeperVolumeMap.Add(sleeperVolume.BoxMin, value);
			}
			value.Add(sleeperVolumes.Count - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupSleeperVolumes()
	{
		for (int i = 0; i < sleeperVolumes.Count; i++)
		{
			sleeperVolumes[i].AddToPrefabInstance();
		}
	}

	public void NotifySleeperVolumesEntityDied(EntityAlive entity)
	{
		lock (sleeperVolumes)
		{
			for (int i = 0; i < sleeperVolumes.Count; i++)
			{
				sleeperVolumes[i].EntityDied(entity);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickSleeperVolumes()
	{
		if (!GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			return;
		}
		lock (sleeperVolumes)
		{
			SleeperVolume.TickSpawnCount = 0;
			for (int i = 0; i < sleeperVolumes.Count; i++)
			{
				sleeperVolumes[i].Tick(this);
			}
		}
	}

	public override int AddTriggerVolume(TriggerVolume _triggerVolume)
	{
		lock (triggerVolumes)
		{
			triggerVolumes.Add(_triggerVolume);
			if (!triggerVolumeMap.TryGetValue(_triggerVolume.BoxMin, out var value))
			{
				value = new List<int>();
				triggerVolumeMap.Add(_triggerVolume.BoxMin, value);
			}
			value.Add(triggerVolumes.Count - 1);
			return triggerVolumes.Count - 1;
		}
	}

	public override void ResetTriggerVolumes(long chunkKey)
	{
		Vector2i vector2i = WorldChunkCache.extractXZ(chunkKey);
		Bounds bounds = Chunk.CalculateAABB(vector2i.x, 0, vector2i.y);
		foreach (TriggerVolume triggerVolume in triggerVolumes)
		{
			if (triggerVolume.Intersects(bounds))
			{
				triggerVolume.Reset();
			}
		}
	}

	public override void ResetSleeperVolumes(long chunkKey)
	{
		Vector2i vector2i = WorldChunkCache.extractXZ(chunkKey);
		Bounds bounds = Chunk.CalculateAABB(vector2i.x, 0, vector2i.y);
		foreach (SleeperVolume sleeperVolume in sleeperVolumes)
		{
			if (sleeperVolume.Intersects(bounds))
			{
				sleeperVolume.DespawnAndReset(this);
			}
		}
	}

	public override int FindTriggerVolume(Vector3i mins, Vector3i maxs)
	{
		if (triggerVolumeMap.TryGetValue(mins, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				int num = value[i];
				if (triggerVolumes[num].BoxMax == maxs)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public override int GetTriggerVolumeCount()
	{
		return triggerVolumes.Count;
	}

	public override TriggerVolume GetTriggerVolume(int index)
	{
		return triggerVolumes[index];
	}

	public void CheckTriggerVolumeTrigger(EntityPlayer _player)
	{
		Vector3i blockPosition = _player.GetBlockPosition();
		Chunk chunk = (Chunk)GetChunkFromWorldPos(blockPosition);
		if (chunk == null)
		{
			return;
		}
		List<int> list = chunk.GetTriggerVolumes();
		lock (triggerVolumes)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i];
				if (num < triggerVolumes.Count)
				{
					triggerVolumes[num].CheckTouching(this, _player);
				}
			}
		}
	}

	public void WriteTriggerVolumes(BinaryWriter _bw)
	{
		_bw.Write(triggerVolumes.Count);
		for (int i = 0; i < triggerVolumes.Count; i++)
		{
			triggerVolumes[i].Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadTriggerVolumes(BinaryReader _br)
	{
		triggerVolumes.Clear();
		triggerVolumeMap.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			TriggerVolume triggerVolume = TriggerVolume.Read(_br);
			triggerVolumes.Add(triggerVolume);
			if (!triggerVolumeMap.TryGetValue(triggerVolume.BoxMin, out var value))
			{
				value = new List<int>();
				triggerVolumeMap.Add(triggerVolume.BoxMin, value);
			}
			value.Add(triggerVolumes.Count - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTriggerVolumes()
	{
		for (int i = 0; i < triggerVolumes.Count; i++)
		{
			triggerVolumes[i].AddToPrefabInstance();
		}
	}

	public override int AddWallVolume(WallVolume _wallVolume)
	{
		lock (wallVolumes)
		{
			wallVolumes.Add(_wallVolume);
			if (!wallVolumeMap.TryGetValue(_wallVolume.BoxMin, out var value))
			{
				value = new List<int>();
				wallVolumeMap.Add(_wallVolume.BoxMin, value);
			}
			value.Add(wallVolumes.Count - 1);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				NetPackageWallVolume package = NetPackageManager.GetPackage<NetPackageWallVolume>();
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package.Setup(_wallVolume));
			}
			return wallVolumes.Count - 1;
		}
	}

	public override int FindWallVolume(Vector3i mins, Vector3i maxs)
	{
		if (wallVolumeMap.TryGetValue(mins, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				int num = value[i];
				if (wallVolumes[num].BoxMax == maxs)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public override int GetWallVolumeCount()
	{
		return wallVolumes.Count;
	}

	public override WallVolume GetWallVolume(int index)
	{
		if (index >= wallVolumes.Count)
		{
			Debug.LogWarning($"Wall Volume Error: Index {index} | wallVolumeCount: {wallVolumes.Count}");
		}
		return wallVolumes[index];
	}

	public override List<WallVolume> GetAllWallVolumes()
	{
		return wallVolumes;
	}

	public void WriteWallVolumes(BinaryWriter _bw)
	{
		_bw.Write(wallVolumes.Count);
		for (int i = 0; i < wallVolumes.Count; i++)
		{
			wallVolumes[i].Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadWallVolumes(BinaryReader _br)
	{
		wallVolumes.Clear();
		wallVolumeMap.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			WallVolume wallVolume = WallVolume.Read(_br);
			wallVolumes.Add(wallVolume);
			if (!wallVolumeMap.TryGetValue(wallVolume.BoxMin, out var value))
			{
				value = new List<int>();
				wallVolumeMap.Add(wallVolume.BoxMin, value);
			}
			value.Add(wallVolumes.Count - 1);
		}
	}

	public void SetWallVolumesForClient(List<WallVolume> wallVolumeData)
	{
		wallVolumes.Clear();
		wallVolumeMap.Clear();
		foreach (WallVolume wallVolumeDatum in wallVolumeData)
		{
			wallVolumes.Add(wallVolumeDatum);
			if (!wallVolumeMap.TryGetValue(wallVolumeDatum.BoxMin, out var value))
			{
				value = new List<int>();
				wallVolumeMap.Add(wallVolumeDatum.BoxMin, value);
			}
			value.Add(wallVolumes.Count - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupWallVolumes()
	{
		for (int i = 0; i < wallVolumes.Count; i++)
		{
			wallVolumes[i].AddToPrefabInstance();
		}
	}

	public void AddBlockData(Vector3i v3i, object bd)
	{
		blockData.Add(v3i, bd);
	}

	public object GetBlockData(Vector3i v3i)
	{
		if (!blockData.TryGetValue(v3i, out var value))
		{
			return null;
		}
		return value;
	}

	public void ClearBlockData(Vector3i v3i)
	{
		blockData.Remove(v3i);
	}

	public void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _bStopStabilityUpdate, bool _bRegenerateChunk, bool _bFillEmptyBlocks, bool _isReset = false)
	{
		ChunkCache.ChunkProvider.RebuildTerrain(_chunks, _areaStart, _areaSize, _bStopStabilityUpdate, _bRegenerateChunk, _bFillEmptyBlocks, _isReset);
	}

	public override GameRandom GetGameRandom()
	{
		return rand;
	}

	public float RandomRange(float _min, float _max)
	{
		return rand.RandomFloat * (_max - _min) + _min;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DuskDawnInit()
	{
		(DuskHour, DawnHour) = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
	}

	public void SetTime(ulong _time)
	{
		worldTime = _time;
		if ((bool)m_WorldEnvironment)
		{
			m_WorldEnvironment.WorldTimeChanged();
		}
	}

	public void SetTimeJump(ulong _time, bool _isSeek = false)
	{
		SetTime(_time);
		SkyManager.bUpdateSunMoonNow = true;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			aiDirector.BloodMoonComponent.TimeChanged(_isSeek);
		}
	}

	public bool IsWorldEvent(WorldEvent _event)
	{
		if (_event == WorldEvent.BloodMoon)
		{
			return isEventBloodMoon;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldEventUpdateTime()
	{
		WorldDay = GameUtils.WorldTimeToDays(worldTime);
		WorldHour = GameUtils.WorldTimeToHours(worldTime);
		int num = GameUtils.WorldTimeToDays(eventWorldTime);
		int num2 = GameUtils.WorldTimeToHours(eventWorldTime);
		if (num == WorldDay && num2 == WorldHour)
		{
			return;
		}
		eventWorldTime = worldTime;
		(int duskHour, int dawnHour) tuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
		int item = tuple.duskHour;
		int item2 = tuple.dawnHour;
		bool num3 = isEventBloodMoon;
		isEventBloodMoon = false;
		int num4 = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		if (WorldDay == num4)
		{
			if (WorldHour >= item)
			{
				isEventBloodMoon = true;
			}
		}
		else if (WorldDay > 1 && WorldDay == num4 + 1 && WorldHour < item2)
		{
			isEventBloodMoon = true;
		}
		if (num3 == isEventBloodMoon)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = GetPrimaryPlayer();
		if ((bool)primaryPlayer)
		{
			if (isEventBloodMoon && WorldHour == item)
			{
				primaryPlayer.BloodMoonParticipation = true;
			}
			else if (!isEventBloodMoon && WorldHour == item2 && primaryPlayer.BloodMoonParticipation)
			{
				QuestEventManager.Current.BloodMoonSurvived();
				primaryPlayer.BloodMoonParticipation = false;
			}
		}
	}

	public override void AddPendingDowngradeBlock(Vector3i _blockPos)
	{
		pendingUpgradeDowngradeBlocks.Add(_blockPos);
	}

	public override bool TryRetrieveAndRemovePendingDowngradeBlock(Vector3i _blockPos)
	{
		if (pendingUpgradeDowngradeBlocks.Contains(_blockPos))
		{
			pendingUpgradeDowngradeBlocks.Remove(_blockPos);
			return true;
		}
		return false;
	}

	public IEnumerator ResetPOIS(List<PrefabInstance> prefabInstances, FastTags<TagGroup.Global> questTags, int entityID, int[] sharedWith, QuestClass questClass)
	{
		for (int k = 0; k < prefabInstances.Count; k++)
		{
			PrefabInstance prefabInstance = prefabInstances[k];
			yield return prefabInstance.ResetTerrain(this);
		}
		for (int i = 0; i < prefabInstances.Count; i++)
		{
			PrefabInstance prefabInstance2 = prefabInstances[i];
			triggerManager.RemoveFromUpdateList(prefabInstance2);
			prefabInstance2.LastQuestClass = questClass;
			prefabInstance2.ResetBlocksAndRebuild(this, questTags);
			for (int j = 0; j < prefabInstance2.prefab.SleeperVolumes.Count; j++)
			{
				Vector3i startPos = prefabInstance2.prefab.SleeperVolumes[j].startPos;
				Vector3i size = prefabInstance2.prefab.SleeperVolumes[j].size;
				int num = GameManager.Instance.World.FindSleeperVolume(prefabInstance2.boundingBoxPosition + startPos, prefabInstance2.boundingBoxPosition + startPos + size);
				if (num != -1)
				{
					GetSleeperVolume(num).DespawnAndReset(this);
				}
			}
			for (int l = 0; l < prefabInstance2.prefab.TriggerVolumes.Count; l++)
			{
				Vector3i startPos2 = prefabInstance2.prefab.TriggerVolumes[l].startPos;
				Vector3i size2 = prefabInstance2.prefab.TriggerVolumes[l].size;
				int num2 = GameManager.Instance.World.FindTriggerVolume(prefabInstance2.boundingBoxPosition + startPos2, prefabInstance2.boundingBoxPosition + startPos2 + size2);
				if (num2 != -1)
				{
					GetTriggerVolume(num2).Reset();
				}
			}
			triggerManager.RefreshTriggers(prefabInstance2, questTags);
			if (prefabInstance2.prefab.GetQuestTag(questTags) && (prefabInstance2.lockInstance == null || prefabInstance2.lockInstance.CheckQuestLock()))
			{
				prefabInstance2.lockInstance = new QuestLockInstance(entityID);
				if (sharedWith != null)
				{
					prefabInstance2.lockInstance.AddQuesters(sharedWith);
				}
			}
		}
		bool finished = false;
		while (!finished)
		{
			int m;
			for (m = 0; m < prefabInstances.Count && prefabInstances[m].bPrefabCopiedIntoWorld; m++)
			{
			}
			finished = m >= prefabInstances.Count;
			if (!finished)
			{
				yield return null;
			}
		}
	}

	public bool IsRandomWorld()
	{
		bool result = false;
		if (ChunkCache.ChunkProvider != null && ChunkCache.ChunkProvider.WorldInfo != null)
		{
			result = ChunkCache.ChunkProvider.WorldInfo.RandomGeneratedWorld;
		}
		return result;
	}
}
