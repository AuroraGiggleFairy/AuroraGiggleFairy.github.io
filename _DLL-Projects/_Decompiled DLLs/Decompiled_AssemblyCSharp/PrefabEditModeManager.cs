using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PrefabEditModeManager
{
	public const int cGroundGridYDefault = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SingleFacingBoxName = "single";

	public static PrefabEditModeManager Instance;

	public PathAbstractions.AbstractedLocation LoadedPrefab;

	public Prefab VoxelPrefab;

	public GameObject ImposterPrefab;

	public bool NeedsSaving;

	public Vector3i minPos;

	public Vector3i maxPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PathAbstractions.AbstractedLocation, Prefab> loadedPrefabHeaders = new Dictionary<PathAbstractions.AbstractedLocation, Prefab>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int curGridYPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject groundGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public FileSystemWatcher xmlWatcher;

	[PublicizedFrom(EAccessModifier.Private)]
	public SelectionBox boxShowFacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowFacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowQuestLoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowBlockTriggers;

	public List<byte> TriggerLayers = new List<byte>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showCompositionGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public int highlightBlockId;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HighlightingBlocks
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool HighlightQuestLoot
	{
		get
		{
			return bShowQuestLoot;
		}
		set
		{
			bShowQuestLoot = value;
			NavObjectManager.Instance.UnRegisterNavObjectByClass("editor_quest_loot_container");
			if (!value)
			{
				return;
			}
			foreach (Chunk item in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
			{
				if (!item.IndexedBlocks.TryGetValue(Constants.cQuestLootFetchContainerIndexName, out var _value))
				{
					continue;
				}
				Vector3i worldPos = item.GetWorldPos();
				foreach (Vector3i item2 in _value)
				{
					NavObjectManager.Instance.RegisterNavObject("editor_quest_loot_container", (worldPos + item2).ToVector3Center());
				}
			}
		}
	}

	public bool HighlightBlockTriggers
	{
		get
		{
			return bShowBlockTriggers;
		}
		set
		{
			bShowBlockTriggers = value;
			NavObjectManager.Instance.UnRegisterNavObjectByClass("editor_block_trigger");
			if (!value)
			{
				return;
			}
			foreach (Chunk item in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
			{
				List<BlockTrigger> list = item.GetBlockTriggers().list;
				Vector3i worldPos = item.GetWorldPos();
				for (int i = 0; i < list.Count; i++)
				{
					NavObject navObject = NavObjectManager.Instance.RegisterNavObject("editor_block_trigger", (worldPos + list[i].LocalChunkPos).ToVector3Center());
					navObject.name = list[i].TriggerDisplay();
					navObject.OverrideColor = ((list[i].TriggeredByIndices.Count > 0) ? Color.blue : Color.red);
				}
			}
		}
	}

	public event Action<PrefabInstance> OnPrefabChanged;

	public PrefabEditModeManager()
	{
		Instance = this;
	}

	public void Init()
	{
		ReloadAllXmls();
		InitXmlWatcher();
		if (IsActive())
		{
			SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetVisible(_bVisible: false);
		}
		NeedsSaving = false;
		GameManager.Instance.World.ChunkClusters[0].OnBlockChangedDelegates += blockChangeDelegate;
	}

	public void Update()
	{
		if (IsActive())
		{
			updateCompositionGrid();
		}
	}

	public bool IsActive()
	{
		if (GameManager.Instance.IsEditMode())
		{
			return GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty";
		}
		return false;
	}

	public void LoadRecentlyUsedOrCreateNew()
	{
		if (VoxelPrefab == null)
		{
			string text = GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab);
			PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.AbstractedLocation.None;
			if (!string.IsNullOrEmpty(text))
			{
				abstractedLocation = PathAbstractions.PrefabsSearchPaths.GetLocation(text);
			}
			if (abstractedLocation.Exists())
			{
				ThreadManager.StartCoroutine(loadLastUsedPrefabLater());
			}
			else
			{
				NewVoxelPrefab();
			}
		}
	}

	public void LoadRecentlyUsed()
	{
		string text = GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab);
		if (!string.IsNullOrEmpty(text) && (VoxelPrefab == null || text != VoxelPrefab.PrefabName))
		{
			ThreadManager.StartCoroutine(loadLastUsedPrefabLater());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator loadLastUsedPrefabLater()
	{
		yield return new WaitForSeconds(1f);
		ChunkCluster cc = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
		foreach (Chunk c in chunkArrayCopySync)
		{
			if (!cc.IsOnBorder(c) && !c.IsEmpty())
			{
				while (c.NeedsRegeneration || c.NeedsCopying)
				{
					yield return new WaitForSeconds(1f);
				}
			}
		}
		PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab));
		LoadVoxelPrefab(location);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitXmlWatcher()
	{
		string gameDir = GameIO.GetGameDir("Data/Prefabs");
		Log.Out("Watching prefabs folder for XML changes: " + gameDir);
		xmlWatcher = new FileSystemWatcher(gameDir, "*.xml");
		xmlWatcher.IncludeSubdirectories = true;
		xmlWatcher.Changed += OnXmlFileChanged;
		xmlWatcher.Created += OnXmlFileChanged;
		xmlWatcher.Deleted += OnXmlFileChanged;
		xmlWatcher.EnableRaisingEvents = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnXmlFileChanged(object _sender, FileSystemEventArgs _e)
	{
		Log.Out($"Prefab XML {_e.ChangeType}: {_e.Name}");
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_e.Name);
		PathAbstractions.AbstractedLocation abstractedLocation = new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.GameData, fileNameWithoutExtension, Path.ChangeExtension(_e.FullPath, ".tts"), null, _isFolder: false);
		if (_e.ChangeType == WatcherChangeTypes.Deleted)
		{
			lock (loadedPrefabHeaders)
			{
				loadedPrefabHeaders.Remove(abstractedLocation);
				return;
			}
		}
		LoadXml(abstractedLocation);
		if (VoxelPrefab != null && VoxelPrefab.location == abstractedLocation)
		{
			Log.Out("Applying XML changes to loaded prefab");
			VoxelPrefab.LoadXMLData(VoxelPrefab.location);
		}
		else if (VoxelPrefab != null)
		{
			Log.Out($"XML changed not related to loaded prefab. (Loaded: {VoxelPrefab.location}, FP {VoxelPrefab.location.FullPath}; Changed: {abstractedLocation}, FP {abstractedLocation.FullPath})");
		}
	}

	public void ReloadAllXmls()
	{
		lock (loadedPrefabHeaders)
		{
			loadedPrefabHeaders.Clear();
			foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList())
			{
				LoadXml(availablePaths);
			}
		}
	}

	public void LoadXml(PathAbstractions.AbstractedLocation _location)
	{
		Prefab prefab = new Prefab();
		prefab.LoadXMLData(_location);
		lock (loadedPrefabHeaders)
		{
			loadedPrefabHeaders[_location] = prefab;
		}
	}

	public void Cleanup()
	{
		if (xmlWatcher != null)
		{
			xmlWatcher.EnableRaisingEvents = false;
			xmlWatcher = null;
		}
		lock (loadedPrefabHeaders)
		{
			loadedPrefabHeaders.Clear();
		}
		if ((bool)groundGrid)
		{
			UnityEngine.Object.Destroy(groundGrid);
			groundGrid = null;
		}
		if (prefabInstanceId != -1)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
			dynamicPrefabDecorator.RemovePrefabAndSelection(GameManager.Instance.World, dynamicPrefabDecorator.GetPrefab(prefabInstanceId), _bCleanFromWorld: false);
			prefabInstanceId = -1;
		}
		GameManager.Instance.World.ChunkClusters[0].OnBlockChangedDelegates -= blockChangeDelegate;
		ClearImposterPrefab();
		ClearVoxelPrefab();
		this.OnPrefabChanged = null;
		HighlightBlocks(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockChangeDelegate(Vector3i pos, BlockValue bvOld, sbyte oldDens, TextureFullArray oldTex, BlockValue bvNew)
	{
		NeedsSaving = true;
	}

	public void FindPrefabs(string _group, List<PathAbstractions.AbstractedLocation> _result)
	{
		lock (loadedPrefabHeaders)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> loadedPrefabHeader in loadedPrefabHeaders)
			{
				if (_group == null)
				{
					_result.Add(loadedPrefabHeader.Key);
				}
				else if (_group.Length == 0)
				{
					if (loadedPrefabHeader.Value.editorGroups == null || loadedPrefabHeader.Value.editorGroups.Count == 0)
					{
						_result.Add(loadedPrefabHeader.Key);
					}
				}
				else
				{
					if (loadedPrefabHeader.Value.editorGroups == null)
					{
						continue;
					}
					for (int i = 0; i < loadedPrefabHeader.Value.editorGroups.Count; i++)
					{
						if (string.Compare(loadedPrefabHeader.Value.editorGroups[i], _group, StringComparison.OrdinalIgnoreCase) == 0)
						{
							_result.Add(loadedPrefabHeader.Key);
							break;
						}
					}
				}
			}
		}
	}

	public void GetAllTags(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		lock (loadedPrefabHeaders)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> loadedPrefabHeader in loadedPrefabHeaders)
			{
				if (loadedPrefabHeader.Value.Tags.IsEmpty)
				{
					continue;
				}
				foreach (string tagName in loadedPrefabHeader.Value.Tags.GetTagNames())
				{
					if (!_result.ContainsCaseInsensitive(tagName))
					{
						_result.Add(tagName);
					}
				}
			}
		}
		if (_considerLoadedPrefab == null || _considerLoadedPrefab.Tags.IsEmpty)
		{
			return;
		}
		foreach (string tagName2 in _considerLoadedPrefab.Tags.GetTagNames())
		{
			if (!_result.ContainsCaseInsensitive(tagName2))
			{
				_result.Add(tagName2);
			}
		}
	}

	public void GetAllThemeTags(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		lock (loadedPrefabHeaders)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> loadedPrefabHeader in loadedPrefabHeaders)
			{
				if (loadedPrefabHeader.Value.ThemeTags.IsEmpty)
				{
					continue;
				}
				foreach (string tagName in loadedPrefabHeader.Value.ThemeTags.GetTagNames())
				{
					if (!_result.ContainsCaseInsensitive(tagName))
					{
						_result.Add(tagName);
					}
				}
			}
		}
		if (_considerLoadedPrefab == null || _considerLoadedPrefab.ThemeTags.IsEmpty)
		{
			return;
		}
		foreach (string tagName2 in _considerLoadedPrefab.ThemeTags.GetTagNames())
		{
			if (!_result.ContainsCaseInsensitive(tagName2))
			{
				_result.Add(tagName2);
			}
		}
	}

	public void GetAllGroups(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		lock (loadedPrefabHeaders)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> loadedPrefabHeader in loadedPrefabHeaders)
			{
				if (loadedPrefabHeader.Value.editorGroups == null)
				{
					continue;
				}
				foreach (string editorGroup in loadedPrefabHeader.Value.editorGroups)
				{
					if (!_result.ContainsCaseInsensitive(editorGroup))
					{
						_result.Add(editorGroup);
					}
				}
			}
		}
		if (_considerLoadedPrefab?.editorGroups == null)
		{
			return;
		}
		foreach (string editorGroup2 in _considerLoadedPrefab.editorGroups)
		{
			if (!_result.ContainsCaseInsensitive(editorGroup2))
			{
				_result.Add(editorGroup2);
			}
		}
	}

	public void GetAllZones(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		string[] array;
		lock (loadedPrefabHeaders)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> loadedPrefabHeader in loadedPrefabHeaders)
			{
				string[] allowedZones = loadedPrefabHeader.Value.GetAllowedZones();
				if (allowedZones == null)
				{
					continue;
				}
				array = allowedZones;
				foreach (string item in array)
				{
					if (!_result.ContainsCaseInsensitive(item))
					{
						_result.Add(item);
					}
				}
			}
		}
		if (_considerLoadedPrefab == null)
		{
			return;
		}
		string[] allowedZones2 = _considerLoadedPrefab.GetAllowedZones();
		if (allowedZones2 == null)
		{
			return;
		}
		array = allowedZones2;
		foreach (string item2 in array)
		{
			if (!_result.ContainsCaseInsensitive(item2))
			{
				_result.Add(item2);
			}
		}
	}

	public void GetAllQuestTags(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		string[] array;
		if (_considerLoadedPrefab != null)
		{
			array = _considerLoadedPrefab.GetQuestTags().ToString().Split(',');
			Array.Sort(array);
			for (int i = 0; i < array.Length; i++)
			{
				string item = array[i].Trim();
				if (!_result.ContainsCaseInsensitive(item))
				{
					_result.Add(item);
				}
			}
		}
		array = QuestEventManager.allQuestTags.ToString().Split(',');
		Array.Sort(array);
		for (int j = 0; j < array.Length; j++)
		{
			string item2 = array[j].Trim();
			if (!_result.ContainsCaseInsensitive(item2))
			{
				_result.Add(item2);
			}
		}
	}

	public bool HasPrefabImposter(PathAbstractions.AbstractedLocation _location)
	{
		return SdFile.Exists(_location.FullPathNoExtension + ".mesh");
	}

	public void ClearImposterPrefab()
	{
		UnityEngine.Object.Destroy(ImposterPrefab);
		ImposterPrefab = null;
	}

	public bool LoadImposterPrefab(PathAbstractions.AbstractedLocation _location)
	{
		ClearImposterPrefab();
		ClearVoxelPrefab();
		if (!SdFile.Exists(_location.FullPathNoExtension + ".mesh"))
		{
			return false;
		}
		LoadedPrefab = _location;
		bool bTextureArray = MeshDescription.meshes[0].bTextureArray;
		ImposterPrefab = SimpleMeshFile.ReadGameObject(_location.FullPathNoExtension + ".mesh", 0f, null, bTextureArray);
		ImposterPrefab.transform.name = _location.Name;
		ImposterPrefab.transform.position = new Vector3(0f, -3f, 0f);
		return true;
	}

	public bool IsShowingImposterPrefab()
	{
		return ImposterPrefab != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeAllChunks()
	{
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunks();
		GameManager.Instance.World.ChunkCache.Clear();
		WaterSimulationNative.Instance.Clear();
	}

	public void ClearVoxelPrefab()
	{
		SelectionBoxManager.Instance.Unselect();
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").Clear();
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").Clear();
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("InfoVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("WallVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("POIMarker").Clear();
		SleeperVolumeToolManager.CleanUp();
		prefabInstanceId = -1;
		LoadedPrefab = PathAbstractions.AbstractedLocation.None;
		VoxelPrefab = null;
		removeAllChunks();
		DecoManager.Instance.OnWorldUnloaded();
		ThreadManager.RunCoroutineSync(DecoManager.Instance.OnWorldLoaded(1024, 1024, GameManager.Instance.World, null));
		TogglePrefabFacing(_bShow: false);
		HighlightQuestLoot = HighlightQuestLoot;
		HighlightBlockTriggers = HighlightBlockTriggers;
		showCompositionGrid = false;
	}

	public bool NewVoxelPrefab()
	{
		ClearImposterPrefab();
		ClearVoxelPrefab();
		VoxelPrefab = new Prefab();
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (prefabInstanceId != -1)
		{
			dynamicPrefabDecorator.RemovePrefabAndSelection(GameManager.Instance.World, dynamicPrefabDecorator.GetPrefab(prefabInstanceId), _bCleanFromWorld: false);
		}
		dynamicPrefabDecorator.ClearAllPrefabs();
		prefabInstanceId = dynamicPrefabDecorator.CreateNewPrefabAndActivate(Prefab.LocationForNewPrefab("New Prefab"), Vector3i.zero, VoxelPrefab).id;
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetVisible(_bVisible: false);
		curGridYPos = VoxelPrefab.yOffset;
		if ((bool)groundGrid)
		{
			groundGrid.transform.position = new Vector3(0f, (float)(1 - curGridYPos) - 0.01f, 0f);
		}
		ToggleGroundGrid(_bForceOn: true);
		for (int i = -10; i <= 10; i++)
		{
			for (int j = -10; j <= 10; j++)
			{
				Chunk chunk = MemoryPools.PoolChunks.AllocSync(_bReset: true);
				chunk.X = i;
				chunk.Z = j;
				chunk.ResetBiomeIntensity(BiomeIntensity.Default);
				chunk.NeedsRegeneration = true;
				chunk.NeedsLightCalculation = false;
				chunk.NeedsDecoration = false;
				chunk.ResetLights(byte.MaxValue);
				GameManager.Instance.World.ChunkCache.AddChunkSync(chunk);
				WaterSimulationNative.Instance.InitializeChunk(chunk);
			}
		}
		NeedsSaving = false;
		GamePrefs.Set(EnumGamePrefs.LastLoadedPrefab, string.Empty);
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
		WaterSimulationNative.Instance.SetPaused(_isPaused: true);
		return true;
	}

	public bool LoadVoxelPrefab(PathAbstractions.AbstractedLocation _location, bool _bBulk = false, bool _bIgnoreExcludeImposterCheck = false)
	{
		ClearImposterPrefab();
		ClearVoxelPrefab();
		highlightBlocks(0);
		if (_location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			Log.Out("No prefab found to load!");
			return false;
		}
		VoxelPrefab = new Prefab();
		if (!VoxelPrefab.Load(_location, _applyMapping: true, _fixChildblocks: true, _allowMissingBlocks: true))
		{
			Log.Out($"Error loading prefab {_location}");
			VoxelPrefab = null;
			return false;
		}
		if (!_bIgnoreExcludeImposterCheck && _bBulk && VoxelPrefab.bExcludeDistantPOIMesh)
		{
			VoxelPrefab = null;
			return false;
		}
		int num = VoxelPrefab.size.x * VoxelPrefab.size.y * VoxelPrefab.size.z;
		if (!_bIgnoreExcludeImposterCheck && _bBulk && ((VoxelPrefab.size.y <= 6 && num < 1500) || (VoxelPrefab.size.y > 6 && num < 100)))
		{
			VoxelPrefab = null;
			return false;
		}
		LoadedPrefab = _location;
		curGridYPos = VoxelPrefab.yOffset;
		if ((bool)groundGrid)
		{
			groundGrid.transform.position = new Vector3(0f, (float)(1 - curGridYPos) - 0.01f, 0f);
		}
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		removeAllChunks();
		int num2 = -1 * VoxelPrefab.size.x / 2;
		int num3 = -1 * VoxelPrefab.size.z / 2;
		int num4 = num2 + VoxelPrefab.size.x;
		int num5 = num3 + VoxelPrefab.size.z;
		chunkCache.ChunkMinPos = new Vector2i((num2 - 1) / 16 - 1, (num3 - 1) / 16 - 1);
		chunkCache.ChunkMinPos -= new Vector2i(2, 2);
		chunkCache.ChunkMaxPos = new Vector2i(num4 / 16 + 1, num5 / 16 + 1);
		chunkCache.ChunkMaxPos += new Vector2i(2, 2);
		List<Chunk> list = new List<Chunk>();
		for (int i = chunkCache.ChunkMinPos.x; i <= chunkCache.ChunkMaxPos.x; i++)
		{
			for (int j = chunkCache.ChunkMinPos.y; j <= chunkCache.ChunkMaxPos.y; j++)
			{
				Chunk chunk = MemoryPools.PoolChunks.AllocSync(_bReset: true);
				chunk.X = i;
				chunk.Z = j;
				chunk.SetFullSunlight();
				chunk.NeedsLightCalculation = false;
				chunk.NeedsDecoration = false;
				chunk.NeedsRegeneration = false;
				chunkCache.AddChunkSync(chunk, _bOmitCallbacks: true);
				list.Add(chunk);
			}
		}
		Vector3i vector3i = new Vector3i(num2, 1, num3);
		VoxelPrefab.CopyIntoLocal(chunkCache, vector3i, _bOverwriteExistingBlocks: true, _bSetChunkToRegenerate: false, FastTags<TagGroup.Global>.none);
		for (int k = 0; k < list.Count; k++)
		{
			Chunk chunk2 = list[k];
			chunk2.NeedsLightCalculation = false;
			chunk2.NeedsRegeneration = true;
			WaterSimulationNative.Instance.InitializeChunk(chunk2);
		}
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (prefabInstanceId != -1)
		{
			dynamicPrefabDecorator.RemovePrefabAndSelection(GameManager.Instance.World, dynamicPrefabDecorator.GetPrefab(prefabInstanceId), _bCleanFromWorld: false);
		}
		dynamicPrefabDecorator.ClearAllPrefabs();
		prefabInstanceId = dynamicPrefabDecorator.CreateNewPrefabAndActivate(VoxelPrefab.location, vector3i, VoxelPrefab, _bSetActive: false).id;
		NeedsSaving = false;
		GamePrefs.Set(EnumGamePrefs.LastLoadedPrefab, _location.Name);
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
		HighlightQuestLoot = HighlightQuestLoot;
		HighlightBlockTriggers = HighlightBlockTriggers;
		WaterSimulationNative.Instance.SetPaused(_isPaused: true);
		highlightBlocks(highlightBlockId);
		return true;
	}

	public bool SaveVoxelPrefab()
	{
		if (VoxelPrefab == null)
		{
			return false;
		}
		bool ignorePaintTextures = Chunk.IgnorePaintTextures;
		Chunk.IgnorePaintTextures = false;
		updatePrefabBounds();
		Chunk.IgnorePaintTextures = ignorePaintTextures;
		EnumInsideOutside[] eInsideOutside = VoxelPrefab.UpdateInsideOutside(minPos, maxPos);
		VoxelPrefab.RecalcInsideDevices(eInsideOutside);
		bool num = VoxelPrefab.Save(VoxelPrefab.location);
		if (num)
		{
			LoadedPrefab = VoxelPrefab.location;
			LoadXml(VoxelPrefab.location);
			GamePrefs.Set(EnumGamePrefs.LastLoadedPrefab, VoxelPrefab.PrefabName);
		}
		NeedsSaving = false;
		return num;
	}

	public void UpdateMinMax()
	{
		if (VoxelPrefab == null)
		{
			return;
		}
		Vector3i vector3i = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);
		Vector3i vector3i2 = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
		foreach (Chunk item in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					for (int k = 0; k < 256; k++)
					{
						int blockId = item.GetBlockId(i, k, j);
						WaterValue water = item.GetWater(i, k, j);
						if (blockId != 0 || water.HasMass() || item.GetDensity(i, k, j) < 0)
						{
							Vector3i vector3i3 = item.ToWorldPos(new Vector3i(i, k, j));
							if (vector3i.x > vector3i3.x)
							{
								vector3i.x = vector3i3.x;
							}
							if (vector3i.y > vector3i3.y)
							{
								vector3i.y = vector3i3.y;
							}
							if (vector3i.z > vector3i3.z)
							{
								vector3i.z = vector3i3.z;
							}
							if (vector3i2.x < vector3i3.x)
							{
								vector3i2.x = vector3i3.x;
							}
							if (vector3i2.y < vector3i3.y)
							{
								vector3i2.y = vector3i3.y;
							}
							if (vector3i2.z < vector3i3.z)
							{
								vector3i2.z = vector3i3.z;
							}
						}
					}
				}
			}
		}
		if (vector3i.x == int.MaxValue)
		{
			vector3i = Vector3i.zero;
			vector3i2 = Vector3i.zero;
		}
		minPos = vector3i;
		maxPos = vector3i2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePrefabBounds()
	{
		if (VoxelPrefab != null)
		{
			VoxelPrefab.yOffset = curGridYPos;
			UpdateMinMax();
			VoxelPrefab.CopyFromWorldWithEntities(GameManager.Instance.World, minPos, maxPos, new List<int>());
			if (prefabInstanceId != -1)
			{
				GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(prefabInstanceId).UpdateBoundingBoxPosAndScale(minPos, VoxelPrefab.size);
			}
		}
	}

	public void SetGroundLevel(int _yOffset)
	{
		curGridYPos = _yOffset;
		if ((bool)groundGrid)
		{
			groundGrid.transform.position = new Vector3(0f, (float)(1 - curGridYPos) - 0.01f, 0f);
		}
		if (VoxelPrefab != null)
		{
			VoxelPrefab.yOffset = curGridYPos;
			if (prefabInstanceId != -1 && this.OnPrefabChanged != null)
			{
				PrefabInstance prefab = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(prefabInstanceId);
				this.OnPrefabChanged(prefab);
			}
			NeedsSaving = true;
		}
	}

	public void MoveGroundGridUpOrDown(int _deltaY)
	{
		SetGroundLevel(Utils.FastClamp(curGridYPos - _deltaY, -200, 0));
	}

	public void ToggleGroundGrid(bool _bForceOn = false)
	{
		if (groundGrid == null)
		{
			groundGrid = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/GroundGrid/GroundGrid"));
			groundGrid.transform.position = new Vector3(0f, (float)(1 - curGridYPos) + 0.01f, 0f);
			for (int i = 0; i < groundGrid.transform.childCount; i++)
			{
				groundGrid.transform.GetChild(i).gameObject.tag = "B_Mesh";
			}
		}
		else
		{
			groundGrid.SetActive(_bForceOn || !groundGrid.activeSelf);
		}
	}

	public bool IsGroundGrid()
	{
		if (groundGrid != null)
		{
			return groundGrid.activeSelf;
		}
		return false;
	}

	public void ToggleCompositionGrid()
	{
		showCompositionGrid = !showCompositionGrid;
		if (showCompositionGrid)
		{
			SelectionCategory category = SelectionBoxManager.Instance.GetCategory("DynamicPrefabs");
			bool num = category.IsVisible();
			UpdatePrefabBounds();
			if (!num)
			{
				category.SetVisible(_bVisible: false);
			}
		}
	}

	public bool IsCompositionGrid()
	{
		if (IsActive())
		{
			return showCompositionGrid;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCompositionGrid()
	{
		Color color = new Color32(0, byte.MaxValue, 40, byte.MaxValue);
		Color color2 = new Color32(0, byte.MaxValue, 200, byte.MaxValue);
		Color color3 = new Color32(0, byte.MaxValue, 200, byte.MaxValue);
		if (!showCompositionGrid)
		{
			return;
		}
		Vector3i vector3i = maxPos + Vector3i.one;
		Vector3i vector3i2 = vector3i - minPos;
		if (!(vector3i2 == Vector3i.one))
		{
			EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World?.GetPrimaryPlayer();
			if (!(entityPlayerLocal == null))
			{
				float y = (float)(1 - curGridYPos) + 0.05f;
				float num = (float)vector3i2.x / 1.618034f;
				int num2 = Mathf.RoundToInt(num);
				int num3 = Mathf.RoundToInt(((float)vector3i2.x - num) / 2f);
				float num4 = (float)vector3i2.z / 1.618034f;
				int num5 = Mathf.RoundToInt(num4);
				int num6 = Mathf.RoundToInt(((float)vector3i2.z - num4) / 2f);
				float num7 = (float)vector3i2.x / 2f;
				float num8 = (float)vector3i2.z / 2f;
				DebugLines.Create(_pos1: new Vector3(minPos.x + num2, y, minPos.z), _pos2: new Vector3(minPos.x + num2, y, vector3i.z), _name: "GoldenRatio_X1", _parentT: entityPlayerLocal.RootTransform, _color1: color, _color2: color, _width1: 0.1f, _width2: 0.1f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3(vector3i.x - num2, y, minPos.z), _pos2: new Vector3(vector3i.x - num2, y, vector3i.z), _name: "GoldenRatio_X2", _parentT: entityPlayerLocal.RootTransform, _color1: color, _color2: color, _width1: 0.1f, _width2: 0.1f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3(minPos.x, y, minPos.z + num5), _pos2: new Vector3(vector3i.x, y, minPos.z + num5), _name: "GoldenRatio_Z1", _parentT: entityPlayerLocal.RootTransform, _color1: color, _color2: color, _width1: 0.1f, _width2: 0.1f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3(minPos.x, y, vector3i.z - num5), _pos2: new Vector3(vector3i.x, y, vector3i.z - num5), _name: "GoldenRatio_Z2", _parentT: entityPlayerLocal.RootTransform, _color1: color, _color2: color, _width1: 0.1f, _width2: 0.1f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3((float)minPos.x + num7, y, minPos.z), _pos2: new Vector3((float)minPos.x + num7, y, vector3i.z), _name: "GoldenRatio_InnerX", _parentT: entityPlayerLocal.RootTransform, _color1: color2, _color2: color2, _width1: 0.03f, _width2: 0.03f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3(minPos.x, y, (float)minPos.z + num8), _pos2: new Vector3(vector3i.x, y, (float)minPos.z + num8), _name: "GoldenRatio_InnerZ", _parentT: entityPlayerLocal.RootTransform, _color1: color2, _color2: color2, _width1: 0.03f, _width2: 0.03f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3(minPos.x + num3, y, minPos.z), _pos2: new Vector3(minPos.x + num3, y, vector3i.z), _name: "GoldenRatio_OuterX1", _parentT: entityPlayerLocal.RootTransform, _color1: color3, _color2: color3, _width1: 0.03f, _width2: 0.03f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3(vector3i.x - num3, y, minPos.z), _pos2: new Vector3(vector3i.x - num3, y, vector3i.z), _name: "GoldenRatio_OuterX2", _parentT: entityPlayerLocal.RootTransform, _color1: color3, _color2: color3, _width1: 0.03f, _width2: 0.03f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3(minPos.x, y, minPos.z + num6), _pos2: new Vector3(vector3i.x, y, minPos.z + num6), _name: "GoldenRatio_OuterZ1", _parentT: entityPlayerLocal.RootTransform, _color1: color3, _color2: color3, _width1: 0.03f, _width2: 0.03f, _duration: 0.1f);
				DebugLines.Create(_pos1: new Vector3(minPos.x, y, vector3i.z - num6), _pos2: new Vector3(vector3i.x, y, vector3i.z - num6), _name: "GoldenRatio_OuterZ2", _parentT: entityPlayerLocal.RootTransform, _color1: color3, _color2: color3, _width1: 0.03f, _width2: 0.03f, _duration: 0.1f);
			}
		}
	}

	public void UpdatePrefabBounds()
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (prefabInstanceId == -1)
		{
			VoxelPrefab = new Prefab();
			prefabInstanceId = dynamicPrefabDecorator.CreateNewPrefabAndActivate(VoxelPrefab.location, Vector3i.zero, VoxelPrefab).id;
		}
		if (prefabInstanceId != -1)
		{
			SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").GetBox(dynamicPrefabDecorator.GetPrefab(prefabInstanceId).name).SetVisible(_visible: true);
			SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetVisible(_bVisible: true);
		}
		updatePrefabBounds();
	}

	public bool IsPrefabFacing()
	{
		return bShowFacing;
	}

	public void TogglePrefabFacing(bool _bShow)
	{
		if (VoxelPrefab == null)
		{
			return;
		}
		if (_bShow && boxShowFacing == null)
		{
			boxShowFacing = SelectionBoxManager.Instance.GetCategory("PrefabFacing").AddBox("single", Vector3i.zero, Vector3i.one, _bDrawDirection: true);
		}
		bShowFacing = _bShow;
		updateFacing();
		if (boxShowFacing != null)
		{
			if (bShowFacing)
			{
				boxShowFacing.SetPositionAndSize(new Vector3(0f, 2f, -VoxelPrefab.size.z / 2 - 3), Vector3i.one);
			}
			SelectionBoxManager.Instance.SetActive("PrefabFacing", "single", bShowFacing);
			SelectionBoxManager.Instance.GetCategory("PrefabFacing").GetBox("single").SetVisible(bShowFacing);
			SelectionBoxManager.Instance.GetCategory("PrefabFacing").SetVisible(bShowFacing);
		}
	}

	public void RotatePrefabFacing()
	{
		if (VoxelPrefab != null)
		{
			VoxelPrefab.rotationToFaceNorth++;
			VoxelPrefab.rotationToFaceNorth &= 3;
			updateFacing();
			NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFacing()
	{
		if (VoxelPrefab != null)
		{
			float facing = 0f;
			switch (VoxelPrefab.rotationToFaceNorth)
			{
			case 1:
				facing = 90f;
				break;
			case 2:
				facing = 180f;
				break;
			case 3:
				facing = 270f;
				break;
			}
			SelectionBoxManager.Instance.SetFacingDirection("PrefabFacing", "single", facing);
		}
	}

	public void MovePrefabUpOrDown(int _deltaY)
	{
		updatePrefabBounds();
		Vector3i destinationPos = minPos + _deltaY * Vector3i.up;
		if (destinationPos.y < 1 || maxPos.y + _deltaY > 250)
		{
			return;
		}
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = chunkCache.GetChunkArrayCopySync();
		foreach (Chunk item in chunkArrayCopySync)
		{
			item.RemoveAllTileEntities();
			if (item.IsEmpty())
			{
				continue;
			}
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					for (int k = 0; k < 254; k++)
					{
						item.SetWater(i, k, j, WaterValue.Empty);
						BlockValue block = item.GetBlock(i, k, j);
						if (!block.isair && !block.ischild)
						{
							chunkCache.SetBlock(item.ToWorldPos(new Vector3i(i, k, j)), BlockValue.Air, _isNotify: true, _isUpdateLight: false);
							item.SetDensity(i, k, j, MarchingCubes.DensityAir);
							item.SetTextureFull(i, k, j, 0L);
						}
					}
				}
			}
		}
		VoxelPrefab.CopyIntoLocal(chunkCache, destinationPos, _bOverwriteExistingBlocks: true, _bSetChunkToRegenerate: false, FastTags<TagGroup.Global>.none);
		foreach (Chunk item2 in chunkArrayCopySync)
		{
			item2.NeedsRegeneration = true;
		}
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
		UpdateMinMax();
		if (prefabInstanceId != -1)
		{
			GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(prefabInstanceId).UpdateBoundingBoxPosAndScale(minPos, VoxelPrefab.size, _moveSleepers: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkLayerEmpty(int _y, List<Chunk> chunks)
	{
		bool bAllEmpty = true;
		for (int i = 0; i < chunks.Count; i++)
		{
			chunks[i].LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int x, int y, int z, BlockValue bv) =>
			{
				if (y == _y)
				{
					bAllEmpty = false;
				}
			});
		}
		return bAllEmpty;
	}

	public void StripTextures()
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			chunk.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int x, int y, int z, BlockValue bv) =>
			{
				if (bUseSelection)
				{
					Vector3i vector3i = chunk.ToWorldPos(new Vector3i(x, y, z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				if (chunk.GetTextureFull(x, y, z) != 0L)
				{
					chunk.SetTextureFull(x, y, z, 0L);
					changedChunks.Add(chunk);
				}
			});
		}
		foreach (Chunk item in changedChunks)
		{
			item.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			NeedsSaving = true;
		}
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
	}

	public void StripInternalTextures()
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		World world = GameManager.Instance.World;
		List<Chunk> chunkArrayCopySync = world.ChunkCache.GetChunkArrayCopySync();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			chunk.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int x, int y, int z, BlockValue bv) =>
			{
				if (bUseSelection)
				{
					Vector3i vector3i = chunk.ToWorldPos(new Vector3i(x, y, z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				if (chunk.GetTextureFull(x, y, z) != 0L && bv.Block.shape is BlockShapeNew blockShapeNew)
				{
					for (int j = 0; j < 6; j++)
					{
						BlockFace face = (BlockFace)j;
						Vector3i vector3i2 = new Vector3i(BlockFaceFlags.OffsetForFace(face));
						Vector3i pos = chunk.ToWorldPos(new Vector3i(x, y, z)) + vector3i2;
						BlockValue block = world.GetBlock(pos);
						if (!block.isair && block.Block.shape is BlockShapeNew blockShapeNew2)
						{
							BlockFace face2 = BlockFaceFlags.OppositeFace(face);
							BlockShapeNew.EnumFaceOcclusionInfo faceInfo = blockShapeNew.GetFaceInfo(bv, face);
							BlockShapeNew.EnumFaceOcclusionInfo faceInfo2 = blockShapeNew2.GetFaceInfo(block, face2);
							if ((faceInfo == BlockShapeNew.EnumFaceOcclusionInfo.Full && faceInfo2 == BlockShapeNew.EnumFaceOcclusionInfo.Full) || (faceInfo == BlockShapeNew.EnumFaceOcclusionInfo.Part && faceInfo2 == BlockShapeNew.EnumFaceOcclusionInfo.Full) || (faceInfo == BlockShapeNew.EnumFaceOcclusionInfo.Part && faceInfo2 == BlockShapeNew.EnumFaceOcclusionInfo.Part && blockShapeNew == blockShapeNew2 && bv.rotation == block.rotation))
							{
								face = blockShapeNew.GetRotatedBlockFace(bv, face);
								for (int k = 0; k < 1; k++)
								{
									world.ChunkCache.SetBlockFaceTexture(chunk.ToWorldPos(new Vector3i(x, y, z)), face, 0, k);
								}
								changedChunks.Add(chunk);
							}
						}
					}
				}
			});
		}
		foreach (Chunk item in changedChunks)
		{
			item.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			NeedsSaving = true;
		}
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
	}

	public void GetLootAndFetchLootContainerCount(out int _loot, out int _fetchLoot, out int _restorePower)
	{
		int tempLoot = 0;
		int tempFetchLoot = 0;
		int tempRestorePower = 0;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			chunkArrayCopySync[i].LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int _, int _, int _, BlockValue bv) =>
			{
				Block block = bv.Block;
				if (block != null)
				{
					bool flag = block.IndexName != null;
					if (flag && block.IndexName == Constants.cQuestLootFetchContainerIndexName)
					{
						tempFetchLoot++;
					}
					else if (flag && block.IndexName == Constants.cQuestRestorePowerIndexName)
					{
						tempRestorePower++;
					}
					else if (block is BlockLoot)
					{
						tempLoot++;
					}
					else if (block is BlockCompositeTileEntity blockCompositeTileEntity && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>())
					{
						tempLoot++;
					}
				}
			});
		}
		_loot = tempLoot;
		_fetchLoot = tempFetchLoot;
		_restorePower = tempRestorePower;
	}

	public void HighlightBlocks(Block _blockClass)
	{
		highlightBlockId = _blockClass?.blockID ?? 0;
		HighlightingBlocks = highlightBlockId > 0;
		highlightBlocks(highlightBlockId);
	}

	public void ToggleHighlightBlocks()
	{
		HighlightingBlocks = !HighlightingBlocks;
		highlightBlocks(HighlightingBlocks ? highlightBlockId : 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void highlightBlocks(int _blockId)
	{
		BlockHighlighter.Cleanup();
		if (_blockId <= 0)
		{
			return;
		}
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			chunk.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int _x, int _y, int _z, BlockValue _bv) =>
			{
				if (_bv.type == _blockId)
				{
					BlockHighlighter.AddBlock(chunk.worldPosIMin + new Vector3i(_x, _y, _z));
				}
			});
		}
	}
}
