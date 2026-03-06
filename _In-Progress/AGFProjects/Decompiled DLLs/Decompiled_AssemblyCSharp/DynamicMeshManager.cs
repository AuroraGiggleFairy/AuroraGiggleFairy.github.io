using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ConcurrentCollections;
using UniLinq;
using UnityEngine;
using UnityEngine.Rendering;

public class DynamicMeshManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class DisabledImposterChunkManager : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public const string ClippingOnKeyword = "PREFAB_CLIPPING_ON";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly int chunkBufferID = Shader.PropertyToID("_ChunkClipValues");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly int chunkDimensionsID = Shader.PropertyToID("_WorldChunkDimensions");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly GlobalKeyword featureKeyword = GlobalKeyword.Create("PREFAB_CLIPPING_ON");

		[PublicizedFrom(EAccessModifier.Private)]
		public DynamicMeshManager dynamicMeshManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public ComputeBuffer chunkClipValues;

		[PublicizedFrom(EAccessModifier.Private)]
		public int[] chunkClipValuesArray;

		[PublicizedFrom(EAccessModifier.Private)]
		public int worldChunkDimensions;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposedValue;

		public DisabledImposterChunkManager(DynamicMeshManager dynamicMeshManager)
		{
			this.dynamicMeshManager = dynamicMeshManager;
			worldChunkDimensions = GameManager.Instance.World.ChunkCache.ChunkProvider.GetWorldSize().x / 16;
			int num = worldChunkDimensions * worldChunkDimensions;
			chunkClipValues = new ComputeBuffer(num, 4);
			chunkClipValuesArray = new int[num];
			Shader.SetGlobalInteger(chunkDimensionsID, worldChunkDimensions);
			Shader.EnableKeyword(in featureKeyword);
			Update();
		}

		public void Update()
		{
			if (chunkClipValuesArray == null)
			{
				return;
			}
			int num = worldChunkDimensions / 2;
			for (int i = 0; i < chunkClipValuesArray.Length; i++)
			{
				int x = i % worldChunkDimensions - num;
				int y = i / worldChunkDimensions - num;
				long key = WorldChunkCache.MakeChunkKey(x, y);
				if (dynamicMeshManager.ItemsDictionary.ContainsKey(key))
				{
					chunkClipValuesArray[i] = -1;
				}
				else
				{
					chunkClipValuesArray[i] = 1;
				}
			}
			chunkClipValues.SetData(chunkClipValuesArray);
			Shader.SetGlobalBuffer(chunkBufferID, chunkClipValues);
		}

		public static void DisableShaderKeyword()
		{
			Shader.DisableKeyword(in featureKeyword);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					DisableShaderKeyword();
					chunkClipValues.Dispose();
				}
				chunkClipValuesArray = null;
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	public int DebugOption;

	public float DebugY = -0.7f;

	public float TestZ = -1f;

	public static int DebugX;

	public static int DebugZ;

	public static bool DebugItemPositions;

	public static bool DebugReleases;

	public int QueueDelay = 5;

	public static bool ForceMeshGeneration;

	public static string FileMissing;

	public static bool Allow32BitMeshes;

	public static GUIStyle DebugStyle;

	public static bool CONTENT_ENABLED;

	public static bool CompressFiles;

	public static bool DisableScopeTexture;

	public static int GuiY;

	public static float MaxRebuildTime;

	public PrefabCheckState PrefabCheck;

	public static Rect ViewRect;

	public int QuadSize = 10000;

	public ConcurrentDictionary<long, DynamicMeshItem> ItemsDictionary;

	public static bool DisableLOD;

	public DynamicObserver Observer = new DynamicObserver();

	public DynamicObserver ObserverPrep = new DynamicObserver();

	public Vector3 ObserverPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ObserverPosNext;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ObserverRequest ObserverRequestInfo;

	public static DynamicMeshManager Instance;

	public static bool DisableMoveVerts;

	public static bool ShowDebug;

	public Queue ToRemove = new Queue();

	public static int CombineType;

	public static HashSet<long> ChunkGameObjects;

	public List<DynamicObserver> Observers = new List<DynamicObserver>();

	public DynamicMeshServerStatus ClientMessage;

	public ConcurrentHashSet<DynamicMeshVoxelLoad> BufferRegionLoadRequests = new ConcurrentHashSet<DynamicMeshVoxelLoad>();

	public LinkedList<DynamicMeshItem> ChunkMeshLoadRequests = new LinkedList<DynamicMeshItem>();

	public object _chunkMeshDataLock = new object();

	public LinkedList<DynamicMeshVoxelLoad> ChunkMeshData = new LinkedList<DynamicMeshVoxelLoad>();

	public LinkedList<long> RegionsAvailableToLoad = new LinkedList<long>();

	public int AvailableRegionLoadRequests;

	public ConcurrentQueue<DyMeshRegionLoadRequest> RegionFileLoadRequests = new ConcurrentQueue<DyMeshRegionLoadRequest>();

	public static MicroStopwatch MeshLoadStop;

	public int LongestRegionLoad;

	public static bool DebugReport;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float nextUpdate;

	public static bool ShowGui;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float nextShowHide;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ProcessItemLoadReady = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime ForceNextItemLoad = DateTime.Now.AddDays(1.0);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ProcessRegionReady = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime ForceNextRegion = DateTime.Now.AddDays(1.0);

	public static bool testMessage;

	public static GameObject Parent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedListNode<DynamicMeshItem> CachedItem;

	public float time;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D backgroundTexture;

	public static float ThreadDistance;

	public static float ObserverDistance;

	public static float ItemLoadDistance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DisabledImposterChunkManager disabledImposterChunkManager;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool disabledImposterChunksDirty;

	public List<DynamicMeshUpdateData> UpdateData = new List<DynamicMeshUpdateData>(10);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int PointerSize;

	public ConcurrentQueue<GameObject> ToBeDestroyed = new ConcurrentQueue<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime nextOrphanCheck;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<GameObject> potentialOrphans = new HashSet<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<GameObject> checkForOrphans = new HashSet<GameObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int dtMinX;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int dtMaxX;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int dtMinZ;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int dtMaxZ;

	public static bool DoLog;

	public static bool DoLogNet;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<Vector2i> ChunksToRemove = new ConcurrentQueue<Vector2i>();

	public DynamicMeshRegion NearestRegionWithUnloaded;

	public bool FindNearestUnloadedItems;

	public Vector3i? PrimaryLocation;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, HashSet<long>> RequestKeys = new Dictionary<long, HashSet<long>>();

	public static int ShowHideCheckTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] gcGenCount = new int[3];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static string ChunkChangedInItemLoad;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime NextFallingCheck = DateTime.Now;

	public static Transform ParentTransform
	{
		get
		{
			if (!(Instance == null))
			{
				return Instance.transform;
			}
			return null;
		}
	}

	public static bool IsServer => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

	public static EntityPlayerLocal player
	{
		get
		{
			if (GameManager.Instance.World != null)
			{
				return GameManager.Instance.World.GetPrimaryPlayer();
			}
			return null;
		}
	}

	public void ForceOrphanChecks()
	{
		StartCoroutine(DoubleOrphanCheck());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DoubleOrphanCheck()
	{
		yield return StartCoroutine(CheckForOrphans(debugLogOnly: false));
		yield return StartCoroutine(CheckForOrphans(debugLogOnly: false));
	}

	public IEnumerator CheckForOrphans(bool debugLogOnly)
	{
		int returns = 0;
		DateTime dateTime = DateTime.Now.AddMilliseconds(2.0);
		checkForOrphans.Clear();
		for (int i = 0; i < ParentTransform.childCount; i++)
		{
			Transform child = ParentTransform.GetChild(i);
			checkForOrphans.Add(child.gameObject);
		}
		foreach (DynamicMeshItem value in ItemsDictionary.Values)
		{
			if (!(value.ChunkObject == null))
			{
				checkForOrphans.Remove(value.ChunkObject);
				potentialOrphans.Remove(value.ChunkObject);
				if (dateTime < DateTime.Now)
				{
					returns++;
					yield return null;
					dateTime = DateTime.Now.AddMilliseconds(2.0);
				}
			}
		}
		foreach (DynamicMeshRegion value2 in DynamicMeshRegion.Regions.Values)
		{
			if (!(value2.RegionObject == null))
			{
				checkForOrphans.Remove(value2.RegionObject);
				potentialOrphans.Remove(value2.RegionObject);
				if (dateTime < DateTime.Now)
				{
					returns++;
					yield return null;
					dateTime = DateTime.Now.AddMilliseconds(2.0);
				}
			}
		}
		foreach (GameObject checkForOrphan in checkForOrphans)
		{
			if (potentialOrphans.Contains(checkForOrphan))
			{
				Log.Warning("Found orphaned mesh in dymesh parent " + checkForOrphan.name);
				if (!debugLogOnly)
				{
					UnityEngine.Object.Destroy(checkForOrphan);
				}
				potentialOrphans.Remove(checkForOrphan);
			}
			else
			{
				potentialOrphans.Add(checkForOrphan);
			}
		}
	}

	public static void EnabledChanged(bool newvalue)
	{
		if (!(GameManager.Instance == null) && GameManager.Instance.World != null)
		{
			OnWorldUnload();
			if (newvalue)
			{
				Init();
			}
		}
	}

	public void AddObjectForDestruction(GameObject go)
	{
		ToBeDestroyed.Enqueue(go);
	}

	public static void AddRegionLoadMeshes(long key)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		foreach (long item in Instance.RegionsAvailableToLoad)
		{
			if (item == key)
			{
				return;
			}
		}
		Instance.RegionsAvailableToLoad.AddLast(key);
	}

	public void AddChunkLoadData(DynamicMeshVoxelLoad loadData)
	{
		lock (_chunkMeshDataLock)
		{
			Instance.ChunkMeshData.AddLast(loadData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static DynamicMeshManager()
	{
		DebugX = 1840;
		DebugZ = 672;
		DebugItemPositions = false;
		DebugReleases = false;
		FileMissing = "FM";
		Allow32BitMeshes = true;
		DebugStyle = new GUIStyle();
		CONTENT_ENABLED = true;
		CompressFiles = true;
		DisableScopeTexture = true;
		GuiY = 0;
		MaxRebuildTime = 300f;
		ViewRect = default(Rect);
		DisableLOD = false;
		DisableMoveVerts = false;
		ShowDebug = true;
		CombineType = 1;
		ChunkGameObjects = new HashSet<long>();
		MeshLoadStop = new MicroStopwatch();
		DebugReport = false;
		ShowGui = false;
		testMessage = true;
		backgroundTexture = null;
		PointerSize = 8;
		DoLog = false;
		DoLogNet = false;
		ShowHideCheckTime = 3000;
		ChunkChangedInItemLoad = "ChunkChangedInItemLoad";
	}

	public static bool IsOutsideDistantTerrain(DynamicMeshRegion region)
	{
		if (!CONTENT_ENABLED || Instance == null)
		{
			return false;
		}
		if (!(region.Rect.xMin < (float)dtMinX) && !(region.Rect.xMax > (float)dtMaxX) && !(region.Rect.yMin < (float)dtMinZ))
		{
			return region.Rect.yMax > (float)dtMaxZ;
		}
		return true;
	}

	public static bool IsOutsideDistantTerrain(float minx, float maxx, float minz, float maxz)
	{
		if (!CONTENT_ENABLED || Instance == null)
		{
			return false;
		}
		if (!(minx < (float)dtMinX) && !(maxx > (float)dtMaxX) && !(minz < (float)dtMinZ))
		{
			return maxz > (float)dtMaxZ;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void UpdateDistantTerrainBounds(TileArea<UnityDistantTerrain.TerrainAndWater> data, UnityDistantTerrain.Config terrainConfig)
	{
		if (Instance == null)
		{
			return;
		}
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		int num4 = int.MinValue;
		if (data.Data.Count == 0)
		{
			dtMinX = int.MinValue;
			dtMinZ = int.MinValue;
			dtMaxX = -2147482624;
			dtMaxZ = -2147482624;
			return;
		}
		foreach (uint key in data.Data.Keys)
		{
			int tileXPos = TileAreaUtils.GetTileXPos(key);
			int tileZPos = TileAreaUtils.GetTileZPos(key);
			num = Math.Min(num, tileXPos);
			num2 = Math.Min(num2, tileZPos);
			num3 = Math.Max(num3, tileXPos);
			num4 = Math.Max(num4, tileZPos);
		}
		int dataTileSize = terrainConfig.DataTileSize;
		num *= dataTileSize;
		num2 *= dataTileSize;
		num3 *= dataTileSize;
		num4 *= dataTileSize;
		num3 += dataTileSize;
		num4 += dataTileSize;
		if (num != dtMinX || num3 != dtMaxX || dtMinZ != num2 || dtMaxZ != num4)
		{
			dtMinX = num;
			dtMaxX = num3;
			dtMinZ = num2;
			dtMaxZ = num4;
			Instance.ShowOrHidePrefabs();
			GameManager.Instance.prefabLODManager.TriggerUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyObjects()
	{
		GameObject result;
		while (ToBeDestroyed.TryDequeue(out result))
		{
			if (!(result == null))
			{
				if (DynamicMeshFile.CurrentlyLoadingItem?.GetGameObject() == result)
				{
					Log.Warning("Object in use when destroying... delaying");
					ToBeDestroyed.Enqueue(result);
					break;
				}
				result.SetActive(value: false);
				MeshDestroy(result);
			}
		}
	}

	public static void WarmUp()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SendClientMessage()
	{
		while (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return new WaitForSeconds(1f);
		}
		ClientMessage = DynamicMeshServerStatus.ClientMessageSent;
		NetPackageDynamicClientArrive package = NetPackageManager.GetPackage<NetPackageDynamicClientArrive>();
		package.BuildData();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package);
		LogMsg("Sending client arrive message. Items: " + package.Items.Count);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ProcessChunkRegionRequests()
	{
		ProcessRegionReady = false;
		ForceNextRegion = DateTime.Now.AddSeconds(60.0);
		DyMeshRegionLoadRequest load = null;
		if (RegionFileLoadRequests.Count != 0 && RegionFileLoadRequests.TryDequeue(out load))
		{
			if (PrefabCheck == PrefabCheckState.Waiting)
			{
				PrefabCheck = PrefabCheckState.Ready;
			}
			DynamicMeshRegion region = GetRegion(load.Key);
			if (DoLog)
			{
				Log.Out("Loading region go-process " + region.ToDebugLocation());
			}
			if (region != null)
			{
				if (region.OutsideLoadArea)
				{
					region.SetState(DynamicRegionState.Unloaded, forceChange: false);
				}
				else
				{
					yield return StartCoroutine(load.CreateMeshCoroutine(region));
					if (region.RegionObject != null)
					{
						region.SetState(DynamicRegionState.Loaded, forceChange: false);
						bool flag = !region.InBuffer && region.LoadedItems.Count > 0 && region.UnloadedItems.Count == 0 && region.RegionObject != null && region.RegionObject.activeSelf;
						region.LoadItems(urgent: false, !flag, includeUnloaded: false, "regionLoadRequest");
						UpdateDynamicPrefabDecoratorRegions(region);
					}
				}
			}
		}
		if (load != null)
		{
			MeshLists.ReturnList(load.OpaqueMesh);
			MeshLists.ReturnList(load.TerrainMesh);
		}
		AvailableRegionLoadRequests = Math.Min(DynamicMeshSettings.MaxRegionMeshData, AvailableRegionLoadRequests + 1);
		ProcessRegionReady = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ProcessItemMeshGeneration()
	{
		ProcessItemLoadReady = false;
		ForceNextItemLoad = DateTime.Now.AddSeconds(30.0);
		DynamicMeshVoxelLoad voxelData = null;
		if (ChunkMeshData.Count != 0)
		{
			bool lockTaken = false;
			Monitor.TryEnter(_chunkMeshDataLock, 1, ref lockTaken);
			if (lockTaken)
			{
				float num = 9999999f;
				foreach (DynamicMeshVoxelLoad chunkMeshDatum in ChunkMeshData)
				{
					float num2 = chunkMeshDatum.Item.DistanceToPlayer();
					if (num2 < num)
					{
						voxelData = chunkMeshDatum;
						num = num2;
						if (num2 < 50f)
						{
							break;
						}
					}
				}
				if (voxelData != null)
				{
					ChunkMeshData.Remove(voxelData);
				}
				Monitor.Exit(_chunkMeshDataLock);
				if (voxelData != null)
				{
					DynamicMeshItem item = GetItemOrNull(voxelData.Item.Key);
					if (item != null)
					{
						DynamicMeshRegion region = GetRegion(item);
						region.RemoveFromLoadingQueue(item);
						if (!region.IsInItemLoad())
						{
							item.State = DynamicItemState.Waiting;
						}
						else
						{
							item.State = DynamicItemState.Loading;
							if (DoLog)
							{
								Log.Out("Item " + item.ToDebugLocation() + " loading request");
							}
							ItemLoadDistance = item.DistanceToPlayer();
							yield return StartCoroutine(item.CreateMeshFromVoxelCoroutine(isVisible: false, MeshLoadStop, voxelData));
							region.AddChunk(item.WorldPosition.x, item.WorldPosition.z);
							if (region.WorldPosition.x != DynamicMeshUnity.RoundRegion(item.WorldPosition.x))
							{
								string[] obj = new string[6] { "Region mismatch: ", null, null, null, null, null };
								Vector3i worldPosition = region.WorldPosition;
								obj[1] = worldPosition.ToString();
								obj[2] = " vs ";
								obj[3] = (item.WorldPosition.x / 16 * 16).ToString();
								obj[4] = " for  ";
								worldPosition = item.WorldPosition;
								obj[5] = worldPosition.ToString();
								Log.Error(string.Concat(obj));
							}
							if (item.State != DynamicItemState.ReadyToDelete)
							{
								region.AddItemToLoadedList(item);
								bool flag = region.IsVisible();
								item.SetVisible(!flag && !item.IsChunkInView, "loadItemRequest");
								item.State = DynamicItemState.Loaded;
							}
						}
					}
				}
			}
		}
		ProcessItemLoadReady = true;
		voxelData?.DisposeMeshes();
	}

	public void ForceLoadDataAroundPosition(Vector3i pos, int regionRadius)
	{
		if (!CONTENT_ENABLED)
		{
			return;
		}
		DateTime now = DateTime.Now;
		CONTENT_ENABLED = false;
		foreach (DynamicMeshRegion value in DynamicMeshRegion.Regions.Values)
		{
			value?.DistanceChecks();
		}
		CONTENT_ENABLED = true;
		DyMeshRegionLoadRequest dyMeshRegionLoadRequest = DyMeshRegionLoadRequest.Create(0L);
		foreach (long item in RegionsAvailableToLoad)
		{
			dyMeshRegionLoadRequest.Key = item;
			DynamicMeshRegion region = GetRegion(dyMeshRegionLoadRequest.Key);
			if (region == null)
			{
				continue;
			}
			if (region.OutsideLoadArea || region.DistanceToPlayer() > 1000f)
			{
				region.SetState(DynamicRegionState.Unloaded, forceChange: false);
				continue;
			}
			dyMeshRegionLoadRequest.OpaqueMesh.Reset();
			dyMeshRegionLoadRequest.TerrainMesh.Reset();
			DynamicMeshThread.RegionStorage.LoadRegion(dyMeshRegionLoadRequest);
			dyMeshRegionLoadRequest.CreateMeshSync(region);
			if (region.RegionObject != null)
			{
				region.SetState(DynamicRegionState.Loaded, forceChange: false);
				bool flag = !region.InBuffer && region.LoadedItems.Count > 0 && region.UnloadedItems.Count == 0 && region.RegionObject != null && region.RegionObject.activeSelf;
				region.LoadItems(urgent: false, !flag, includeUnloaded: false, "regionLoadRequest");
				UpdateDynamicPrefabDecoratorRegions(region);
			}
		}
		MeshLists.ReturnList(dyMeshRegionLoadRequest.OpaqueMesh);
		MeshLists.ReturnList(dyMeshRegionLoadRequest.TerrainMesh);
		RegionsAvailableToLoad.Clear();
		foreach (ChunkGameObject value2 in GameManager.Instance.World.ChunkClusters[0].DisplayedChunkGameObjects.Dict.Values)
		{
			if (!ChunkGameObjects.Contains(value2.chunk.Key))
			{
				DynamicMeshThread.AddChunkGameObject(value2.chunk);
			}
		}
		Log.Out("Force load took " + (int)(DateTime.Now - now).TotalSeconds + " seconds");
	}

	public void ClearPrefabs()
	{
		if (base.gameObject != null)
		{
			foreach (Transform item in base.gameObject.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		if (ItemsDictionary != null)
		{
			ItemsDictionary.Clear();
			DynamicMeshRegion.Regions.Clear();
		}
	}

	public void Cleanup()
	{
		DynamicMeshThread.StopThreadRequest();
		ClearPrefabs();
		StopAllCoroutines();
		Vector2i result;
		while (ChunksToRemove.TryDequeue(out result))
		{
		}
		foreach (DynamicMeshRegion value in DynamicMeshRegion.Regions.Values)
		{
			value.CleanUp();
		}
		ItemsDictionary.Clear();
		UpdateData.Clear();
		ChunkMeshData.Clear();
		foreach (Transform item in base.transform)
		{
			ToBeDestroyed.Enqueue(item.gameObject);
		}
		DestroyObjects();
		disabledImposterChunkManager.Dispose();
	}

	public bool HalEventChunkChanged(object chunkObject)
	{
		if (chunkObject == null || !(chunkObject is Chunk))
		{
			return false;
		}
		ChunkChanged(((Chunk)chunkObject).GetWorldPos(), -1, 1);
		return true;
	}

	public static bool IsValidGameMode()
	{
		if (GameManager.Instance?.World == null)
		{
			Log.Out("GM or World is not initialised yet for dynamic mesh");
			return false;
		}
		EnumGameMode gameMode = (EnumGameMode)GameManager.Instance.World.GetGameMode();
		if (!GameManager.Instance.IsEditMode() && !GameUtils.IsPlaytesting() && gameMode != EnumGameMode.Creative)
		{
			return gameMode != EnumGameMode.EditWorld;
		}
		return false;
	}

	public void Awake()
	{
		LogMsg("Awake");
		CONTENT_ENABLED = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		if (!CONTENT_ENABLED)
		{
			Log.Out("Dynamic mesh disabled");
			return;
		}
		if (backgroundTexture == null)
		{
			backgroundTexture = Texture2D.blackTexture;
		}
		DynamicMeshFile.TerrainSharedMaterials.Clear();
		if (Instance != null)
		{
			Instance.StopAllCoroutines();
			ClearPrefabs();
			if (Instance != this)
			{
				UnityEngine.Object.Destroy(Instance);
			}
			Instance = null;
		}
		if (!IsValidGameMode())
		{
			Log.Out("Dynamic Mesh will not run in this game mode");
			return;
		}
		Instance = this;
		DynamicMeshFile.MeshLocation = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? (GameIO.GetSaveGameDir() + "/DynamicMeshes/") : (GameIO.GetSaveGameLocalDir() + "/DynamicMeshes/"));
		LogMsg("Mesh location: " + DynamicMeshFile.MeshLocation);
		BufferRegionLoadRequests.Clear();
		AvailableRegionLoadRequests = DynamicMeshSettings.MaxRegionMeshData;
		DyMeshData.ActiveItems = 0;
		ChunkMeshData.Clear();
		ChunkMeshLoadRequests.Clear();
		ConnectionManager.OnClientDisconnected -= DynamicMeshServer.OnClientDisconnect;
		ConnectionManager.OnClientDisconnected += DynamicMeshServer.OnClientDisconnect;
		UpdateData.Clear();
		ItemsDictionary = new ConcurrentDictionary<long, DynamicMeshItem>();
		DynamicMeshThread.StopThreadForce();
		if (!CONTENT_ENABLED)
		{
			LogMsg("Disabled");
			return;
		}
		DateTime now = DateTime.Now;
		LoadItemsDedicated();
		LogMsg("Loading all items took: " + (DateTime.Now - now).TotalSeconds + " seconds.");
		disabledImposterChunkManager = new DisabledImposterChunkManager(this);
		if (!GameManager.IsDedicatedServer)
		{
			if (player != null)
			{
				DynamicMeshThread.PlayerPositionX = player.position.x;
				DynamicMeshThread.PlayerPositionZ = player.position.z;
				ForceLoadDataAroundPosition(new Vector3i(player.position), DynamicMeshSettings.MaxViewDistance / 2);
			}
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (DoLog)
				{
					LogMsg("Saying hello to server. Regions: " + DynamicMeshRegion.Regions.Values.Count);
				}
				ShowOrHidePrefabs();
			}
		}
		DynamicMeshThread.StartThread();
		PrefabCheck = (SdFile.Exists(DynamicMeshFile.MeshLocation + "!!ChunksChecked.info") ? PrefabCheckState.Run : PrefabCheckState.Waiting);
		if (PrefabCheck != PrefabCheckState.Run)
		{
			CheckPrefabs("thread Started");
		}
		MeshDescription.meshes[0].bTextureArray = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ShowErrorStackTraces(string _msg, string _trace, LogType _type)
	{
		if (_type == LogType.Error || _type == LogType.Exception)
		{
			Log.Out("Callback " + _type.ToString() + ":" + _msg + " | " + _trace);
		}
	}

	public static void OnWorldUnload()
	{
		DynamicMeshThread.StopThreadRequest();
		DateTime dateTime = DateTime.Now.AddSeconds(1.0);
		while (DynamicMeshThread.RequestThreadStop && DateTime.Now < dateTime)
		{
			Thread.Sleep(100);
		}
		DynamicMeshThread.CleanUp();
		ChunkGameObjects.Clear();
		if (Instance != null)
		{
			Instance.Cleanup();
		}
		DynamicMeshFile.CleanUp();
		DynamicMeshFile.MeshLocation = null;
	}

	public void CheckPrefabsInRegion(int x, int z)
	{
		int xMin = DynamicMeshUnity.RoundRegion(x);
		int zMin = DynamicMeshUnity.RoundRegion(z);
		int xMax = xMin + 160;
		int zMax = zMin + 160;
		LogMsg("Checking prefabs in region");
		PrefabCheck = PrefabCheckState.Warming;
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		EntityPlayerLocal entityPlayerLocal = player;
		if (dynamicPrefabDecorator == null || entityPlayerLocal == null)
		{
			return;
		}
		Vector3 playerPos = new Vector3(x, 0f, z);
		List<PrefabInstance> list = (from d in dynamicPrefabDecorator.allPrefabs
			where d.boundingBoxPosition.x >= xMin && d.boundingBoxPosition.x + d.boundingBoxSize.x < xMax && d.boundingBoxPosition.z >= zMin && d.boundingBoxPosition.z + d.boundingBoxSize.z < zMax
			orderby Math.Abs(Vector3.Distance(playerPos, d.boundingBoxPosition.ToVector3()))
			select d).ToList();
		LogMsg("Found prefabs: " + list.Count + "   Already loaded: " + ItemsDictionary.Count);
		Dictionary<Vector3i, List<Vector3i>> dictionary = new Dictionary<Vector3i, List<Vector3i>>();
		foreach (PrefabInstance item in list)
		{
			Instance.CheckPrefab(item, dictionary);
		}
		List<Vector3i> list2 = dictionary.Keys.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (Vector3i d) => DynamicMeshUnity.Distance(d, playerPos)).ToList();
		DynamicMeshThread.RegionsToCheck = new List<List<Vector3i>>();
		int num = 0;
		foreach (Vector3i item2 in list2)
		{
			foreach (Vector3i p in dictionary[item2])
			{
				DynamicMeshRegion regionFromWorldPosition = DynamicMeshRegion.GetRegionFromWorldPosition(p.x, p.z);
				if (regionFromWorldPosition == null || !regionFromWorldPosition.LoadedChunks.Any([PublicizedFrom(EAccessModifier.Internal)] (Vector3i d) => d.x == p.x && d.z == p.z))
				{
					num++;
					AddChunk(p, primary: false);
				}
			}
		}
		LogMsg("Keys: " + list2.Count + " loaded: " + num);
		PrefabCheck = PrefabCheckState.WaitingForCompleteCheck;
	}

	public void CheckPrefabs(string source, bool forceRegen = false)
	{
		if (!IsServer || (!forceRegen && (DynamicMeshSettings.OnlyPlayerAreas || !DynamicMeshSettings.NewWorldFullRegen)))
		{
			return;
		}
		LogMsg("Checking prefabs " + source);
		if (GameManager.Instance.World.ChunkClusters.Count == 0)
		{
			LogMsg("Clusters zero");
			PrefabCheck = PrefabCheckState.WaitingForCompleteCheck;
			return;
		}
		PrefabCheck = PrefabCheckState.Warming;
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		_ = player;
		if (dynamicPrefabDecorator == null)
		{
			LogMsg("No deco found");
			return;
		}
		Vector3 playerPos = (IsServer ? Vector3.zero : player.GetPosition());
		List<PrefabInstance> list = dynamicPrefabDecorator.allPrefabs.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (PrefabInstance d) => Math.Abs(Vector3.Distance(playerPos, d.boundingBoxPosition.ToVector3()))).ToList();
		LogMsg("Prefabs: " + list.Count);
		LogMsg("Found prefabs: " + list.Count + "   Already loaded: " + ItemsDictionary.Count);
		Dictionary<Vector3i, List<Vector3i>> dictionary = new Dictionary<Vector3i, List<Vector3i>>();
		foreach (PrefabInstance item in list)
		{
			Instance.CheckPrefab(item, dictionary);
		}
		List<Vector3i> list2 = dictionary.Keys.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (Vector3i d) => DynamicMeshUnity.Distance(d, playerPos)).ToList();
		DynamicMeshThread.RegionsToCheck = new List<List<Vector3i>>();
		int num = 0;
		foreach (Vector3i item2 in list2)
		{
			foreach (Vector3i p in dictionary[item2])
			{
				DynamicMeshRegion regionFromWorldPosition = DynamicMeshRegion.GetRegionFromWorldPosition(p.x, p.z);
				if (regionFromWorldPosition == null || !regionFromWorldPosition.LoadedChunks.Any([PublicizedFrom(EAccessModifier.Internal)] (Vector3i d) => d.x == p.x && d.z == p.z))
				{
					num++;
					AddChunk(p, primary: false);
				}
			}
		}
		LogMsg("Keys: " + list2.Count + " loaded: " + num);
		PrefabCheck = PrefabCheckState.WaitingForCompleteCheck;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckPrefab(PrefabInstance p, Dictionary<Vector3i, List<Vector3i>> positions)
	{
		Vector3i boundingBoxPosition = p.boundingBoxPosition;
		Vector3i boundingBoxSize = p.boundingBoxSize;
		int itemPosition = DynamicMeshUnity.GetItemPosition(boundingBoxPosition.x);
		int num = DynamicMeshUnity.GetItemPosition(boundingBoxPosition.x + boundingBoxSize.x) + 16;
		int itemPosition2 = DynamicMeshUnity.GetItemPosition(boundingBoxPosition.z);
		int num2 = DynamicMeshUnity.GetItemPosition(boundingBoxPosition.z + boundingBoxSize.z) + 16;
		for (int i = itemPosition; i <= num; i += 16)
		{
			for (int j = itemPosition2; j <= num2; j += 16)
			{
				long itemKey = DynamicMeshUnity.GetItemKey(i, j);
				ItemsDictionary.TryGetValue(itemKey, out var value);
				if (value != null && value.FileExists())
				{
					continue;
				}
				Vector3i regionPositionFromWorldPosition = DynamicMeshUnity.GetRegionPositionFromWorldPosition(i, j);
				if (!positions.ContainsKey(regionPositionFromWorldPosition))
				{
					positions.Add(regionPositionFromWorldPosition, new List<Vector3i>());
				}
				List<Vector3i> list = positions[regionPositionFromWorldPosition];
				bool flag = false;
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k].x == i && list[k].z == j)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Vector3i item = new Vector3i(i, 0, j);
					if (DoLog)
					{
						LogMsg("Adding chunk " + i + "," + j);
					}
					list.Add(item);
				}
			}
		}
	}

	public void RefreshAll()
	{
		foreach (DynamicMeshItem value in ItemsDictionary.Values)
		{
			DynamicMeshThread.ToGenerate.Enqueue(value);
		}
	}

	public void AddChunkStub(Vector3i worldPos, DynamicMeshRegion region)
	{
		long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
		AddChunk(key, addToThread: false, primary: false, region);
	}

	public static void AddDataFromServer(int x, int z)
	{
		if (!(Instance == null))
		{
			Vector3i worldPos = new Vector3i(x, 0, z);
			DynamicMeshRegion region = Instance.GetRegion(x, z);
			bool flag = region.IsInItemLoad();
			Instance.AddChunkStub(worldPos, region);
			DynamicMeshItem itemFromWorldPosition = Instance.GetItemFromWorldPosition(x, z);
			if (DoLog)
			{
				LogMsg("Add data from server " + x + "," + z + " itemLoad: " + flag + "  state: " + itemFromWorldPosition.State);
			}
			itemFromWorldPosition.State = DynamicItemState.UpdateRequired;
			if (flag)
			{
				itemFromWorldPosition.Load("data from server", urgentLoad: true, region.InBuffer);
			}
		}
	}

	public DynamicMeshItem AddChunk(Vector3i worldPos, bool primary)
	{
		long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
		return AddChunk(key, addToThread: true, primary, null);
	}

	public DynamicMeshItem AddChunk(long key, bool addToThread, bool primary, DynamicMeshRegion region)
	{
		Vector3i pos = new Vector3i(WorldChunkCache.extractX(key) * 16, 0, WorldChunkCache.extractZ(key) * 16);
		DynamicMeshThread.AddRegionChunk(pos.x, pos.z, key);
		if (!ItemsDictionary.TryGetValue(key, out var value))
		{
			value = new DynamicMeshItem(pos);
			if (region == null)
			{
				region = GetRegion(value);
			}
			if (region == null)
			{
				return value;
			}
			region.AddItem(value);
			disabledImposterChunksDirty |= ItemsDictionary.TryAdd(key, value);
		}
		DynamicMeshUnity.AddDisabledImposterChunk(value.Key);
		if (addToThread)
		{
			if (primary)
			{
				DynamicMeshThread.RequestPrimaryQueue(value);
			}
			else
			{
				DynamicMeshThread.RequestSecondaryQueue(value);
			}
		}
		return value;
	}

	public static void LogMsg(string msg)
	{
		Log.Out("Dymesh: {0}", msg);
	}

	public static void MeshDestroy(GameObject go)
	{
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component.sharedMesh);
			component.sharedMesh = null;
		}
		MeshRenderer component2 = go.GetComponent<MeshRenderer>();
		if (component2 != null)
		{
			component2.sharedMaterial = null;
			UnityEngine.Object.Destroy(component2);
		}
		foreach (Transform item in go.transform)
		{
			MeshDestroy(item.gameObject);
		}
		UnityEngine.Object.Destroy(go);
	}

	public void UpdateDynamicPrefabDecoratorRegions(DynamicMeshRegion region)
	{
		UpdateDynamicPrefabDecoratorRegion(region);
		DynamicMeshRegion region2 = GetRegion(region.WorldPosition + new Vector3i(160, 0, -160));
		UpdateDynamicPrefabDecoratorRegion(region2);
		region2 = GetRegion(region.WorldPosition + new Vector3i(-160, 0, 160));
		UpdateDynamicPrefabDecoratorRegion(region2);
		region2 = GetRegion(region.WorldPosition + new Vector3i(160, 0, 160));
		UpdateDynamicPrefabDecoratorRegion(region2);
		region2 = GetRegion(region.WorldPosition + new Vector3i(-160, 0, -160));
		UpdateDynamicPrefabDecoratorRegion(region2);
	}

	public void UpdateDynamicPrefabDecoratorRegion(DynamicMeshRegion region)
	{
		if (DisableLOD || region == null || region.RegionObject == null || region.LoadedItems.Any([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshItem d) => d.State != DynamicItemState.Loaded && d.State != DynamicItemState.Empty && d.State != DynamicItemState.ReadyToDelete))
		{
			return;
		}
		_ = DateTime.Now;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (GameManager.Instance == null)
		{
			LogMsg("GM Null");
		}
		if (GameManager.Instance.World == null)
		{
			LogMsg("world Null");
		}
		if (GameManager.Instance.World.ChunkClusters == null)
		{
			LogMsg("cluster Null");
		}
		if (GameManager.Instance.World.ChunkClusters.Count == 0)
		{
			LogMsg("cluster zero");
		}
		if (GameManager.Instance.World.ChunkClusters[0].ChunkProvider == null)
		{
			LogMsg("Provider Null");
		}
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator == null)
		{
			LogMsg("dec Null");
		}
		else
		{
			if (region.Instances != null)
			{
				return;
			}
			region.Instances = new List<PrefabInstance>();
			List<PrefabInstance> dynamicPrefabs = dynamicPrefabDecorator.GetDynamicPrefabs();
			if (dynamicPrefabs == null || dynamicPrefabs.Count == 0)
			{
				LogMsg("prefabs Null or empty");
				return;
			}
			for (int num = dynamicPrefabs.Count - 1; num >= 0; num--)
			{
				PrefabInstance prefabInstance = dynamicPrefabs[num];
				if (region.ContainsPrefab(prefabInstance))
				{
					region.Instances.Add(prefabInstance);
				}
			}
		}
	}

	public bool PointInRectAndRegionExists(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, int x, int y)
	{
		bool flag = x >= topLeftX && x <= bottomRightX && y >= bottomRightY && y <= topLeftY;
		if (flag)
		{
			DynamicMeshRegion regionFromWorldPosition = DynamicMeshRegion.GetRegionFromWorldPosition(x, y);
			if (regionFromWorldPosition == null || regionFromWorldPosition.RegionObject == null)
			{
				return false;
			}
		}
		return flag;
	}

	public bool IsRegionLoadedAndActive(int x, int y, bool doDebug)
	{
		if (Instance.PrefabCheck == PrefabCheckState.Run)
		{
			return true;
		}
		DynamicMeshRegion regionFromWorldPosition = DynamicMeshRegion.GetRegionFromWorldPosition(x, y);
		if (regionFromWorldPosition != null)
		{
			return regionFromWorldPosition.IsRegionLoadedAndActive(doDebug);
		}
		if (doDebug)
		{
			Log.Out("LoadedAndActive Failed: region is null");
		}
		return false;
	}

	public void ArrangeChunkRemoval(int x, int z)
	{
		ChunksToRemove.Enqueue(new Vector2i(x, z));
	}

	public void DisableLodGO(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		UnityEngine.Object.Destroy(go.GetComponent<MeshRenderer>());
		UnityEngine.Object.Destroy(go.GetComponent<MeshFilter>());
		foreach (Transform item in go.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	public bool StartObserver(Vector3 pos, Vector3 next)
	{
		ObserverRequestInfo = ObserverRequest.Start;
		ObserverPos = pos;
		ObserverPosNext = next;
		return true;
	}

	public void StopObserver()
	{
		if (Observer != null && Observer.Observer != null && ObserverRequestInfo != ObserverRequest.Stop)
		{
			ObserverRequestInfo = ObserverRequest.Stop;
		}
	}

	public void HideChunk(Vector3i worldPos)
	{
		long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
		if (ItemsDictionary.TryGetValue(key, out var value))
		{
			value.SetVisible(active: false, "Hide chunk world pos");
		}
	}

	public void HideChunk(long key)
	{
		if (ItemsDictionary.TryGetValue(key, out var value))
		{
			value.SetVisible(active: false, "Hide chunk world pos");
		}
	}

	public void ShowChunk(long key)
	{
		if (ItemsDictionary.TryGetValue(key, out var value))
		{
			value.GetRegion().OnChunkUnloaded(value);
			if (value.ChunkObject != null)
			{
				value.SetVisible(active: true, "Show chunk force");
			}
		}
	}

	public void LoadItemsDedicated()
	{
		LogMsg("Loading Items: " + DynamicMeshFile.MeshLocation);
		string[] files = SdDirectory.GetFiles(DynamicMeshFile.MeshLocation);
		SdFileInfo[] array = (from fi in new SdDirectoryInfo(DynamicMeshFile.MeshLocation).GetFiles("*.*")
			where fi.Length == 0
			select fi).ToArray();
		if (array.Length != 0)
		{
			LogMsg("Found " + array.Length + " files @ zero size. Deleting...");
			SdFileInfo[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				array2[num].Delete();
			}
		}
		string[] array3 = files;
		foreach (string text in array3)
		{
			if (text.EndsWith(".update"))
			{
				long key = long.Parse(Path.GetFileNameWithoutExtension(text));
				DynamicMeshItem dynamicMeshItem = AddChunk(key, addToThread: false, primary: false, null);
				dynamicMeshItem.UpdateTime = dynamicMeshItem.ReadUpdateTimeFromFile();
			}
		}
		LogMsg("Loaded Items: " + ItemsDictionary.Count);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			StartCoroutine(SendClientMessage());
		}
		if (files.Length == 0)
		{
			CheckPrefabs(" (load items dedicated)");
		}
	}

	public void LoadItemsChunksDedicated()
	{
		LogMsg("Loading Items: " + DynamicMeshFile.MeshLocation);
		string[] files = SdDirectory.GetFiles(DynamicMeshFile.MeshLocation, "*.chunk");
		new Dictionary<string, Vector3>();
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Path.GetFileNameWithoutExtension(array[i]).Split(',');
			float x = float.Parse(array2[0]);
			float z = float.Parse(array2[1]);
			AddChunkStub(new Vector3i(x, 0f, z), null);
		}
		if (ItemsDictionary.Count == 0)
		{
			CheckPrefabs("LoadItemsChunksDedicated");
		}
		LogMsg("Loaded Items: " + ItemsDictionary.Count);
	}

	public static int GetViewSize()
	{
		return GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance) * 16;
	}

	public static int GetBufferSize()
	{
		return GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance) * 16 * 3;
	}

	public static bool RectContainsRect(Rect r1, Rect r2)
	{
		float xMin = r1.xMin;
		float xMax = r1.xMax;
		float yMin = r1.yMin;
		float yMax = r1.yMax;
		float xMin2 = r2.xMin;
		float xMax2 = r2.xMax;
		float yMin2 = r2.yMin;
		float yMax2 = r2.yMax;
		if (xMax2 >= xMin && xMin2 <= xMax)
		{
			if (yMax2 >= yMin)
			{
				return yMin2 <= yMax;
			}
			return false;
		}
		return false;
	}

	public static bool RectContainsPoint(Rect r1, int x, int z)
	{
		float xMin = r1.xMin;
		float xMax = r1.xMax;
		float yMin = r1.yMin;
		float yMax = r1.yMax;
		if ((float)x >= xMin && (float)x <= xMax)
		{
			if ((float)z >= yMin)
			{
				return (float)z <= yMax;
			}
			return false;
		}
		return false;
	}

	public static void OriginUpdate()
	{
		if (!(Instance == null))
		{
			Instance.ShowOrHidePrefabs();
			Instance.SetItemPositions();
		}
	}

	public void SetItemPositions()
	{
		long[] array = ItemsDictionary.Keys.ToArray();
		foreach (long key in array)
		{
			ItemsDictionary[key].SetPosition();
		}
	}

	public void ShowOrHidePrefabs()
	{
		if (player == null || DynamicMeshRegion.Regions == null)
		{
			return;
		}
		if (DebugReport)
		{
			LogMsg("Regions to process: " + DynamicMeshRegion.Regions.Count);
			Vector3 position = player.position;
			LogMsg("Player Position: " + position.ToString());
		}
		Vector3i vector3i = new Vector3i(player.position);
		IChunk chunkFromWorldPos = GameManager.Instance.World.GetChunkFromWorldPos(vector3i);
		if (chunkFromWorldPos == null)
		{
			return;
		}
		Vector3i worldPos = chunkFromWorldPos.GetWorldPos();
		int viewSize = GetViewSize();
		DictionarySave<long, ChunkGameObject> dictionarySave = GameManager.Instance?.World.ChunkClusters[0].DisplayedChunkGameObjects;
		if (dictionarySave == null)
		{
			return;
		}
		for (int i = worldPos.x - viewSize; i <= worldPos.x + viewSize; i += 16)
		{
			for (int j = worldPos.z - viewSize; j <= worldPos.z + viewSize; j += 16)
			{
				Vector3i vector3i2 = new Vector3i(i, 0, j);
				DynamicMeshItem itemOrNull = GetItemOrNull(vector3i2);
				if (itemOrNull != null && dictionarySave.ContainsKey(itemOrNull.Key))
				{
					itemOrNull.SetVisible(active: false, "showHide");
				}
				if (DebugReport)
				{
					Vector3i vector3i3 = vector3i2;
					LogMsg("World pos: " + vector3i3.ToString());
					LogMsg("Chunk Item: " + ((itemOrNull == null) ? "null" : itemOrNull.IsVisible.ToString()));
				}
			}
		}
		DynamicMeshRegion dynamicMeshRegion = GetItemOrNull(vector3i)?.GetRegion();
		if (dynamicMeshRegion != null)
		{
			dynamicMeshRegion.SetVisibleNew(active: false, "showHide");
			foreach (DynamicMeshItem loadedItem in dynamicMeshRegion.LoadedItems)
			{
				loadedItem.SetVisible(!loadedItem.IsChunkInGame, "Region updateItems");
			}
		}
		foreach (DynamicMeshRegion value in DynamicMeshRegion.Regions.Values)
		{
			value?.DistanceChecks();
		}
		DebugReport = false;
	}

	public bool AddRegionChecks()
	{
		if (DynamicMeshThread.RegionsToCheck == null)
		{
			return false;
		}
		if (DynamicMeshThread.RegionsToCheck.Count == 0)
		{
			return false;
		}
		DynamicMeshThread.AddRegionChecks = false;
		List<Vector3i> list = DynamicMeshThread.RegionsToCheck[0];
		DynamicMeshThread.RegionsToCheck.RemoveAt(0);
		LogMsg("Adding new region to check: " + list.Count + "  at " + Time.time + "  remaining: " + DynamicMeshThread.RegionsToCheck.Count);
		foreach (Vector3i item in list)
		{
			AddChunk(item, primary: false);
		}
		if (DynamicMeshThread.RegionsToCheck.Count == 0)
		{
			DynamicMeshThread.RegionsToCheck = null;
		}
		return true;
	}

	public void OnGUI()
	{
		if (!CONTENT_ENABLED || !ShowGui || GameManager.Instance == null || GameManager.Instance.World == null || player == null)
		{
			return;
		}
		Vector3i regionPositionFromWorldPosition = DynamicMeshUnity.GetRegionPositionFromWorldPosition(player.position);
		Vector3i vector3i = new Vector3i(World.toChunkXZ((int)player.position.x) * 16, 0, World.toChunkXZ((int)player.position.z) * 16);
		string text = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? "SP /p2p Host" : "Dedi / p2p");
		try
		{
			string text2 = "p: " + player.position.ToString() + " - " + player.transform.position.ToString() + "\nc: " + vector3i.x + "," + vector3i.z + "\nr:" + regionPositionFromWorldPosition.x + "," + regionPositionFromWorldPosition.z + "\n Buff : " + BufferRegionLoadRequests.Count + "\n Items: " + ChunkMeshData.Count + "\n ItemGen: " + DynamicMeshThread.MeshGenCount + "\n Reg  : " + RegionsAvailableToLoad.Count + $" ({AvailableRegionLoadRequests})" + "\n Server : " + SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer + " / " + SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient + "\n Thread P/S : " + DynamicMeshThread.PrimaryQueue.Count + " / " + DynamicMeshThread.SecondaryQueue.Count + "\n Game: " + text + "\n Packets: " + NetPackageDynamicMesh.Count + "\n ThreadDistance: " + ThreadDistance + "\n ObserverDistance: " + ObserverDistance + "\n ItemLoadDistance: " + ItemLoadDistance + "\n ItemCache: " + DynamicMeshThread.ChunkDataQueue.ChunkData.Count + " (" + DynamicMeshThread.ChunkDataQueue.LiveItems + " live)\n WorldChunks: " + GameManager.Instance.World.ChunkCache.chunks.list.Count + "\n ThreadNext: " + DynamicMeshThread.nextChunks.Count + "\n ThreadQueue: " + DynamicMeshThread.Queue + "\n RegionUpdates: " + DynamicMeshThread.RegionUpdates.Count + "\n RegionUpdatesDebug: " + DynamicMeshThread.RegionUpdatesDebug + "\n SyncPackets: " + DynamicMeshServer.SyncRequests.Count + " (" + DynamicMeshServer.ActiveSyncs.Count + ")\n DataSaveQueue: " + DynamicMeshThread.ChunkDataQueue.ChunkData.Count + "\n DataCache: " + DynamicMeshChunkData.ActiveDataItems + "/" + DynamicMeshChunkData.Cache.Count + " (" + DynamicMeshChunkData.Cache.Sum([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshChunkData d) => (double)d.GetStreamSize() / 1024.0 / 1024.0) + "MB)\n vMeshCache: " + DynamicMeshVoxelLoad.LayerCache.Count + "\n DymeshData: " + DyMeshData.ActiveItems + "/" + DyMeshData.TotalItems;
			foreach (DynamicMeshChunkProcessor item in DynamicMeshThread.BuilderManager.BuilderThreads.ToList())
			{
				if (item != null)
				{
					text2 = text2 + "\n Thread: " + item.Item?.ToDebugLocation() + " Data " + (int)item.MeshDataTime + "ms Mesh: " + (int)item.ExportTime + " Inactive: " + (int)item.InactiveTime;
				}
			}
			Color contentColor = GUI.contentColor;
			Color color = GUI.color;
			int depth = GUI.depth;
			GUI.depth = 0;
			GUI.DrawTexture(new Rect(0f, Screen.height, 500f, 500f), Texture2D.blackTexture, ScaleMode.StretchToFill);
			GUI.color = DebugStyle.normal.textColor;
			GUI.contentColor = DebugStyle.normal.textColor;
			GUI.Label(new Rect(0f, GuiY, 500f, 500f), text2, DebugStyle);
			GUI.contentColor = contentColor;
			GUI.color = color;
			GUI.depth = depth;
		}
		catch (Exception)
		{
		}
	}

	public DynamicMeshRegion GetNearestUnloadedRegion()
	{
		if (PrimaryLocation.HasValue)
		{
			NearestRegionWithUnloaded = GetRegion(PrimaryLocation.Value);
		}
		if (NearestRegionWithUnloaded == null)
		{
			DynamicMeshRegion nearestRegionWithUnloaded = (from d in DynamicMeshRegion.Regions.Values
				where !d.FastTrackLoaded && d.UnloadedItems.Any()
				orderby d.DistanceToPlayer()
				select d).ToList().FirstOrDefault();
			(from d in DynamicMeshRegion.Regions.Values
				where d.UnloadedItems.Any()
				orderby d.DistanceToPlayer()
				select d).FirstOrDefault();
			NearestRegionWithUnloaded = nearestRegionWithUnloaded;
			if (NearestRegionWithUnloaded != null)
			{
				NearestRegionWithUnloaded.FastTrackLoaded = true;
			}
		}
		PrimaryLocation = null;
		FindNearestUnloadedItems = false;
		return NearestRegionWithUnloaded;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MonitorGC()
	{
		for (int i = 0; i < 3; i++)
		{
			int num = GC.CollectionCount(i);
			if (num != gcGenCount[i])
			{
				gcGenCount[i] = num;
				Log.Out($"Gen {i} has fired: " + num);
			}
		}
	}

	public void Update()
	{
		if (!CONTENT_ENABLED || Instance == null)
		{
			return;
		}
		time = Time.time;
		if (FindNearestUnloadedItems)
		{
			GetNearestUnloadedRegion();
		}
		while (ChunksToRemove.Count > 0)
		{
			if (ChunksToRemove.TryDequeue(out var result))
			{
				if (DoLog)
				{
					LogMsg("Remove chunk from region " + result.x + "," + result.y);
				}
				DynamicMeshItem itemFromWorldPosition = GetItemFromWorldPosition(result.x, result.y);
				RemoveItem(itemFromWorldPosition, removedFromWorld: true);
			}
		}
		if (ProcessItemLoadReady || ForceNextItemLoad < DateTime.Now)
		{
			if (ForceNextItemLoad < DateTime.Now)
			{
				Log.Warning("Forcing mesh processing after large delay");
			}
			StartCoroutine(ProcessItemMeshGeneration());
		}
		if (RegionFileLoadRequests.Count > 0)
		{
			StartCoroutine(ProcessChunkRegionRequests());
		}
		DestroyObjects();
		CheckFallingObservers();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicMeshServer.Update();
		}
		if (Observer.StopTime < Time.time)
		{
			Observer.Stop();
			ObserverPrep.Stop();
		}
		if (nextShowHide < Time.time)
		{
			ShowOrHidePrefabs();
			nextShowHide = Time.time + (float)(ShowHideCheckTime / 1000);
			CheckGameObjects();
		}
		if (ObserverRequestInfo != ObserverRequest.None)
		{
			if (ObserverRequestInfo == ObserverRequest.Start)
			{
				Observer.Start(ObserverPos);
			}
			else if (ObserverRequestInfo == ObserverRequest.Stop)
			{
				Observer.StopTime = Time.time + 3f;
			}
			ObserverRequestInfo = ObserverRequest.None;
		}
		while (DynamicMeshThread.ReadyForCollection.Count > 0)
		{
			if (DynamicMeshThread.ReadyForCollection.TryDequeue(out var result2))
			{
				DynamicMeshItem itemFromWorldPosition2 = GetItemFromWorldPosition(result2.X, result2.Z);
				DynamicMeshRegion region = itemFromWorldPosition2.GetRegion();
				if (itemFromWorldPosition2.ChunkObject != null || (region != null && region.IsInItemLoad()))
				{
					AddItemLoadRequest(itemFromWorldPosition2, urgent: false);
				}
			}
		}
		while (DynamicMeshThread.ChunkReadyForCollection.Count > 0)
		{
			if (DynamicMeshThread.ChunkReadyForCollection.TryRemoveFirst(out var returnValue))
			{
				DynamicMeshItem itemFromWorldPosition3 = GetItemFromWorldPosition(returnValue.x, returnValue.y);
				DynamicMeshRegion region = itemFromWorldPosition3.GetRegion();
				if (itemFromWorldPosition3.ChunkObject != null || (region != null && region.IsInItemLoad()))
				{
					DynamicMeshThread.AddChunkGenerationRequest(itemFromWorldPosition3);
				}
			}
		}
		while (AvailableRegionLoadRequests > 0 && RegionsAvailableToLoad.Count > 0)
		{
			LinkedListNode<long> linkedListNode = null;
			float num = 99999f;
			for (LinkedListNode<long> linkedListNode2 = RegionsAvailableToLoad.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
			{
				long value = linkedListNode2.Value;
				DynamicMeshRegion region = GetRegion(value);
				if (region.OutsideLoadArea)
				{
					if (linkedListNode == linkedListNode2)
					{
						linkedListNode = null;
					}
					RegionsAvailableToLoad.Remove(linkedListNode2);
				}
				else
				{
					float num2 = region.DistanceToPlayer();
					if (num2 < num)
					{
						linkedListNode = linkedListNode2;
						num = num2;
						if (num < 100f)
						{
							break;
						}
					}
				}
			}
			if (linkedListNode == null)
			{
				break;
			}
			long value2 = linkedListNode.Value;
			RegionsAvailableToLoad.Remove(linkedListNode);
			DynamicMeshThread.AddRegionLoadRequest(DyMeshRegionLoadRequest.Create(value2));
			AvailableRegionLoadRequests--;
		}
		List<DynamicMeshUpdateData> list = new List<DynamicMeshUpdateData>();
		for (int i = 0; i < UpdateData.Count; i++)
		{
			DynamicMeshUpdateData dynamicMeshUpdateData = UpdateData[i];
			if (dynamicMeshUpdateData.UpdateTime < Time.time || dynamicMeshUpdateData.IsUrgent || dynamicMeshUpdateData.MaxTime < Time.time)
			{
				AddChunk(dynamicMeshUpdateData.Key, dynamicMeshUpdateData.AddToThread, primary: true, null);
				list.Add(dynamicMeshUpdateData);
			}
		}
		foreach (DynamicMeshUpdateData item in list)
		{
			UpdateData.Remove(item);
		}
		if (!(nextUpdate > Time.time))
		{
			if (DynamicMeshThread.RegionsToCheck != null && DynamicMeshThread.AddRegionChecks)
			{
				AddRegionChecks();
			}
			nextUpdate = Time.time + 1f;
			EntityPlayerLocal entityPlayerLocal = player;
			if (entityPlayerLocal != null)
			{
				DynamicMeshThread.PlayerPositionX = entityPlayerLocal.position.x;
				DynamicMeshThread.PlayerPositionZ = entityPlayerLocal.position.z;
			}
			if (DynamicMeshChunkProcessor.DebugOnMainThread)
			{
				DynamicMeshThread.BuilderManager.MainThreadRunJobs();
			}
			if (nextOrphanCheck < DateTime.Now)
			{
				nextOrphanCheck = DateTime.Now.AddSeconds(10.0);
				StartCoroutine(CheckForOrphans(debugLogOnly: true));
			}
			if (disabledImposterChunksDirty)
			{
				disabledImposterChunkManager.Update();
				disabledImposterChunksDirty = false;
			}
		}
	}

	public void AddItemLoadRequest(DynamicMeshItem item, bool urgent)
	{
		item.GetRegion().AddToLoadingQueue(item);
		DynamicMeshThread.AddChunkGenerationRequest(item);
	}

	public bool IsInLoadableArea(long key)
	{
		if (DynamicMeshSettings.OnlyPlayerAreas)
		{
			return HandlePlayerOnlyAreas(key, DynamicMeshUnity.GetWorldPosFromKey(key));
		}
		return true;
	}

	public DynamicMeshItem GetItemOrNull(Vector3i worldPos)
	{
		long itemKey = DynamicMeshUnity.GetItemKey(worldPos.x, worldPos.z);
		if (ItemsDictionary.TryGetValue(itemKey, out var value))
		{
			return value;
		}
		return null;
	}

	public DynamicMeshItem GetItemOrNull(long key)
	{
		if (ItemsDictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public DynamicMeshItem GetItemFromWorldPosition(int x, int z)
	{
		long itemKey = DynamicMeshUnity.GetItemKey(x, z);
		if (ItemsDictionary.TryGetValue(itemKey, out var value))
		{
			return value;
		}
		return AddChunk(itemKey, addToThread: false, primary: false, null);
	}

	public DynamicMeshRegion GetRegion(int x, int z)
	{
		return GetRegion(DynamicMeshUnity.GetRegionKeyFromWorldPosition(x, z));
	}

	public DynamicMeshRegion GetRegion(Vector3i worldPos)
	{
		return GetRegion(DynamicMeshUnity.GetRegionKeyFromWorldPosition(worldPos.x, worldPos.z));
	}

	public DynamicMeshRegion GetRegion(DynamicMeshItem item)
	{
		return GetRegion(item.GetRegionKey());
	}

	public DynamicMeshRegion GetRegion(long key)
	{
		DynamicMeshRegion.Regions.TryGetValue(key, out var value);
		if (value == null)
		{
			value = new DynamicMeshRegion(key);
			if (!DynamicMeshRegion.Regions.TryAdd(key, value))
			{
				DynamicMeshRegion.Regions.TryGetValue(key, out value);
			}
		}
		return value;
	}

	public static void Init()
	{
		CONTENT_ENABLED = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		DisabledImposterChunkManager.DisableShaderKeyword();
		GameManager.Instance.StopCoroutine(DelayStartForWorldLoad());
		GameManager.Instance.StartCoroutine(DelayStartForWorldLoad());
	}

	public static void EnableErrorCallstackLogs()
	{
		Log.LogCallbacks -= ShowErrorStackTraces;
		Log.LogCallbacks += ShowErrorStackTraces;
	}

	public static void DisableErrorCallstackLogs()
	{
		Log.LogCallbacks -= ShowErrorStackTraces;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator DelayStartForWorldLoad()
	{
		while (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			Log.Out("Dynamic mesh waiting for world");
			yield return DynamicMeshFile.WaitOne;
		}
		if (!CONTENT_ENABLED)
		{
			Log.Out("Dynamic mesh disabled on world start");
			yield break;
		}
		LogMsg("Prepping dynamic mesh. Resend Default: " + DynamicMeshServer.ResendPackages);
		DynamicMeshFile.MeshLocation = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? (GameIO.GetSaveGameDir() + "/DynamicMeshes/") : (GameIO.GetSaveGameLocalDir() + "/DynamicMeshes/"));
		LogMsg("Mesh location: " + DynamicMeshFile.MeshLocation);
		if (!SdDirectory.Exists(DynamicMeshFile.MeshLocation))
		{
			SdDirectory.CreateDirectory(DynamicMeshFile.MeshLocation);
		}
		yield return new WaitForSeconds(1f);
		if (Parent != null)
		{
			UnityEngine.Object.Destroy(Parent);
			Parent = null;
		}
		DebugStyle.fontSize = ((Screen.width > 3000) ? 30 : 14);
		DebugStyle.normal.textColor = Color.magenta;
		if ((bool)Instance)
		{
			OnWorldUnload();
			Instance.ClearPrefabs();
		}
		LogMsg("Warming dynamic mesh");
		if (Instance == null || Parent == null)
		{
			LogMsg("Creating dynamic mesh manager");
			Parent = new GameObject("DynamicMeshes");
			Parent.AddComponent<DynamicMeshManager>();
		}
		DynamicMeshBlockSwap.Init();
		DynamicMeshSettings.Validate();
		if (!GameManager.IsDedicatedServer && Application.isEditor && SdDirectory.Exists("D:\\7DaysToDie\\trunkCode"))
		{
			DynamicMeshConsoleCmd.DebugAll();
		}
	}

	public void ReorderGameObjects()
	{
		List<Transform> regions = new List<Transform>();
		List<Transform> list = new List<Transform>();
		foreach (Transform item in Parent.transform)
		{
			list.Add(item);
		}
		regions = (from d in list
			where !d.gameObject.name.StartsWith("C")
			orderby d.gameObject.name
			select d).ToList();
		list.RemoveAll([PublicizedFrom(EAccessModifier.Internal)] (Transform d) => regions.Contains(d));
		int num = 0;
		foreach (Transform item2 in regions)
		{
			item2.SetSiblingIndex(num++);
			if (item2.gameObject.name == string.Empty)
			{
				continue;
			}
			string[] array = item2.gameObject.name.Replace("(sync)", "").Replace("R ", "").Split(',');
			int num2 = int.Parse(array[0]);
			int num3 = int.Parse(array[1]);
			foreach (Transform item3 in list)
			{
				string[] array2 = item3.gameObject.name.Substring(2, item3.gameObject.name.IndexOf(":") - 2).Split(',');
				int num4 = int.Parse(array2[0]);
				int num5 = int.Parse(array2[1]);
				if (num4 >= num2 && num5 >= num3 && num4 < num2 + 160 && num5 < num3 + 160)
				{
					item3.SetSiblingIndex(num++);
				}
			}
		}
	}

	public void RemoveItem(DynamicMeshItem item, bool removedFromWorld)
	{
		if (item != null)
		{
			GetRegion(item).RemoveChunk(item.WorldPosition.x, item.WorldPosition.z, "removeItem", removedFromWorld);
			if (removedFromWorld && !GameManager.IsDedicatedServer)
			{
				disabledImposterChunksDirty |= ItemsDictionary.TryRemove(item.Key, out var _);
			}
			DynamicMeshThread.RemoveRegionChunk(item.WorldPosition.x, item.WorldPosition.z, item.Key);
			DynamicMeshUnity.RemoveDisabledImposterChunk(item.Key);
			if (DoLog)
			{
				LogMsg("Item removed: " + item.ToDebugLocation());
			}
		}
	}

	public static void ChunkChanged(Vector3i worldPos, int entityId, int blockType)
	{
		if (!(Instance != null) || (blockType != -1 && !DynamicMeshBlockSwap.IsValidBlock(blockType)))
		{
			return;
		}
		if (ThreadManager.IsMainThread() && player != null && player.entityId == entityId)
		{
			DynamicMeshRegion region = Instance.GetRegion(worldPos);
			if (region.IsInItemLoad())
			{
				region.SetVisibleNew(active: false, ChunkChangedInItemLoad);
			}
		}
		if (IsServer)
		{
			Instance.AddUpdateData(worldPos, isUrgent: false, addToThread: true);
			int num = worldPos.x & 0xF;
			switch (num)
			{
			case 0:
				Instance.AddUpdateData(worldPos + new Vector3i(-16, 0, 0), isUrgent: false, addToThread: true);
				break;
			case 15:
				Instance.AddUpdateData(worldPos + new Vector3i(16, 0, 0), isUrgent: false, addToThread: true);
				break;
			}
			if ((worldPos.z & 0xF) == 0)
			{
				Instance.AddUpdateData(worldPos + new Vector3i(0, 0, -16), isUrgent: false, addToThread: true);
			}
			else if (num == 15)
			{
				Instance.AddUpdateData(worldPos + new Vector3i(0, 0, 16), isUrgent: false, addToThread: true);
			}
		}
	}

	public bool AddUpdateData(Vector3i worldPos, bool isUrgent, bool addToThread)
	{
		long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
		return AddUpdateData(key, isUrgent, addToThread, checkPlayerArea: true);
	}

	public bool AddUpdateData(int worldX, int worldZ, bool isUrgent, bool addToThread)
	{
		long key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldX), World.toChunkXZ(worldZ));
		return AddUpdateData(key, isUrgent, addToThread, checkPlayerArea: true);
	}

	public bool AddUpdateData(long key, bool isUrgent, bool addToThread, bool checkPlayerArea, int delayScale = 1)
	{
		if (DynamicMeshThread.ChunkDataQueue == null || Instance == null)
		{
			return false;
		}
		DynamicMeshUpdateData dynamicMeshUpdateData = null;
		Vector3i chunkPosition = new Vector3i(WorldChunkCache.extractX(key) * 16, 0, WorldChunkCache.extractZ(key) * 16);
		Vector3 position = chunkPosition.ToVector3();
		if (checkPlayerArea && !HandlePlayerOnlyAreas(key, position))
		{
			return false;
		}
		int num = 0;
		while (num < UpdateData.Count)
		{
			DynamicMeshUpdateData dynamicMeshUpdateData2 = UpdateData[num++];
			if (dynamicMeshUpdateData2.Key == key)
			{
				dynamicMeshUpdateData = dynamicMeshUpdateData2;
				break;
			}
		}
		if (dynamicMeshUpdateData == null)
		{
			dynamicMeshUpdateData = new DynamicMeshUpdateData();
			dynamicMeshUpdateData.ChunkPosition = chunkPosition;
			dynamicMeshUpdateData.Key = key;
			dynamicMeshUpdateData.MaxTime = time + MaxRebuildTime;
			dynamicMeshUpdateData.AddToThread = addToThread;
			UpdateData.Add(dynamicMeshUpdateData);
		}
		if (DoLog)
		{
			LogMsg("Adding update " + key + "  Time: " + dynamicMeshUpdateData.UpdateTime + " pos: " + dynamicMeshUpdateData.ToDebugLocation());
		}
		dynamicMeshUpdateData.UpdateTime = time + (float)(QueueDelay * delayScale);
		dynamicMeshUpdateData.IsUrgent |= isUrgent;
		dynamicMeshUpdateData.AddToThread |= addToThread;
		return true;
	}

	public bool HandlePlayerOnlyAreas(long key, Vector3 position)
	{
		if (!DynamicMeshSettings.OnlyPlayerAreas)
		{
			return true;
		}
		if (!IsPositionInRange(position))
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPositionInRange(Vector3 _position)
	{
		float x = _position.x;
		float z = _position.z;
		int num = Math.Max(16, (DynamicMeshSettings.PlayerAreaChunkBuffer - 1) / 2 * 16);
		DictionaryList<int, EntityPlayer> players = GameManager.Instance.World.Players;
		for (int i = 0; i < players.list.Count; i++)
		{
			for (int j = 0; j < players.list[i].SpawnPoints.Count; j++)
			{
				Vector3i vector3i = players.list[i].SpawnPoints[j];
				int num2 = vector3i.x - num;
				int num3 = vector3i.x + num + 16;
				int num4 = vector3i.z - num;
				int num5 = vector3i.z + num + 16;
				if (x >= (float)num2 && x < (float)num3 && z >= (float)num4 && z < (float)num5)
				{
					return true;
				}
			}
		}
		foreach (PersistentPlayerData value in GameManager.Instance.persistentPlayers.m_lpBlockMap.Values)
		{
			for (int k = 0; k < value.LPBlocks.Count; k++)
			{
				Vector3i vector3i2 = value.LPBlocks[k];
				int num2 = DynamicMeshUnity.GetItemPosition(vector3i2.x) - num;
				int num3 = DynamicMeshUnity.GetItemPosition(vector3i2.x) + num + 16;
				int num4 = DynamicMeshUnity.GetItemPosition(vector3i2.z) - num;
				int num5 = DynamicMeshUnity.GetItemPosition(vector3i2.z) + num + 16;
				if (x >= (float)num2 && x < (float)num3 && z >= (float)num4 && z < (float)num5)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void ImportVox(string name, Vector3 pos, int blockId)
	{
		if (blockId == 0)
		{
			blockId = 502;
		}
		if (Instance == null || GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		GameManager.bPhysicsActive = false;
		string text = DynamicMeshFile.MeshLocation + name + ".vox";
		if (!SdFile.Exists(text))
		{
			Log.Out("File " + text + " does not exist. Cancelling import");
			return;
		}
		byte[] buffer = Convert.FromBase64String(SdFile.ReadAllText(text));
		BlockValue blockValue = new BlockValue
		{
			type = blockId
		};
		BlockValue blockValue2 = new BlockValue
		{
			type = 1
		};
		HashSet<Vector3i> hashSet = new HashSet<Vector3i>();
		int num;
		int num3;
		List<BlockChangeInfo> list;
		using (MemoryStream baseStream = new MemoryStream(buffer))
		{
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			num = pooledBinaryReader.ReadByte();
			int num2 = pooledBinaryReader.ReadByte();
			num3 = pooledBinaryReader.ReadByte();
			list = new List<BlockChangeInfo>(num * num2 * num3 / 2);
			while (pooledBinaryReader.BaseStream.Position != pooledBinaryReader.BaseStream.Length)
			{
				byte b = pooledBinaryReader.ReadByte();
				byte b2 = pooledBinaryReader.ReadByte();
				byte b3 = pooledBinaryReader.ReadByte();
				float num4 = pos.x + (float)(int)b;
				float y = pos.y + (float)(int)b2;
				float num5 = pos.z + (float)(int)b3;
				BlockChangeInfo blockChangeInfo = new BlockChangeInfo(new Vector3i(num4, y, num5), blockValue, 0);
				if (!hashSet.Contains(blockChangeInfo.pos))
				{
					list.Add(blockChangeInfo);
					hashSet.Add(blockChangeInfo.pos);
				}
				blockChangeInfo = new BlockChangeInfo(new Vector3i(num4 + 1f, y, num5), blockValue2, 0);
				if (!hashSet.Contains(blockChangeInfo.pos))
				{
					list.Add(blockChangeInfo);
				}
				blockChangeInfo = new BlockChangeInfo(new Vector3i(num4 - 1f, y, num5), blockValue2, 0);
				if (!hashSet.Contains(blockChangeInfo.pos))
				{
					list.Add(blockChangeInfo);
				}
				blockChangeInfo = new BlockChangeInfo(new Vector3i(num4, y, num5 + 1f), blockValue2, 0);
				if (!hashSet.Contains(blockChangeInfo.pos))
				{
					list.Add(blockChangeInfo);
				}
				blockChangeInfo = new BlockChangeInfo(new Vector3i(num4, y, num5 - 1f), blockValue2, 0);
				if (!hashSet.Contains(blockChangeInfo.pos))
				{
					list.Add(blockChangeInfo);
				}
			}
		}
		Log.Out("Setting " + list.Count + " blocks");
		GameManager.Instance.ChangeBlocks(null, list);
		Log.Out(name + " imported");
		GameManager.bPhysicsActive = true;
		int num6 = (num + 32) / 2;
		int num7 = (num3 + 32) / 2;
		for (int i = (int)pos.x - num6; (float)i < pos.x + (float)num6; i += 16)
		{
			for (int j = (int)pos.z - num7; (float)j < pos.z + (float)num7; j += 16)
			{
				Instance.AddChunk(new Vector3i(i, 0, j), primary: true);
			}
		}
	}

	public static void AddFallingBlockObserver(Vector3i pos)
	{
		if (Instance == null || !IsServer)
		{
			return;
		}
		foreach (DynamicObserver observer in Instance.Observers)
		{
			if (observer.ContainsPoint(pos))
			{
				return;
			}
		}
		DynamicObserver dynamicObserver = new DynamicObserver();
		dynamicObserver.Start(pos.ToVector3());
		Instance.Observers.Add(dynamicObserver);
		Instance.NextFallingCheck = DateTime.Now.AddSeconds(5.0);
	}

	public void CheckGameObjects()
	{
		int num = 0;
		foreach (Transform item in ParentTransform)
		{
			GameObject gameObject = item.gameObject;
			if (gameObject.name.StartsWith("C ") && !(gameObject.name == ""))
			{
				string[] array = gameObject.name.Substring(2, gameObject.name.IndexOf(" ", 3) - 3).Split(',');
				int x = int.Parse(array[0]);
				int z = int.Parse(array[1]);
				DynamicMeshItem itemFromWorldPosition = GetItemFromWorldPosition(x, z);
				if (itemFromWorldPosition.ChunkObject == null)
				{
					AddObjectForDestruction(gameObject);
					num++;
				}
				if (!itemFromWorldPosition.GetRegion().IsInItemLoad())
				{
					num++;
					AddObjectForDestruction(gameObject);
				}
				if (itemFromWorldPosition.ChunkObject != gameObject)
				{
					num++;
					AddObjectForDestruction(gameObject);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckFallingObservers()
	{
		if (NextFallingCheck > DateTime.Now)
		{
			return;
		}
		NextFallingCheck = DateTime.Now.AddSeconds(5.0);
		for (int num = Observers.Count - 1; num >= 0; num--)
		{
			DynamicObserver dynamicObserver = Observers[num];
			if (!dynamicObserver.HasFallingBlocks())
			{
				dynamicObserver.Stop();
				Observers.RemoveAt(num);
			}
		}
	}
}
