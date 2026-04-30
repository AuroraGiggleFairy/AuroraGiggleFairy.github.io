using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ConcurrentCollections;
using UniLinq;
using UnityEngine;

public class DynamicMeshRegion : DynamicMeshContainer
{
	public static ConcurrentDictionary<long, DynamicMeshRegion> Regions = new ConcurrentDictionary<long, DynamicMeshRegion>();

	public static int BufferIndexSize = 1;

	public static int ItemLoadIndex = 3;

	public static int ItemUnloadIndex = ItemLoadIndex + 1;

	public byte VisibleChunks;

	public List<DynamicMeshItem> LoadedItems = new List<DynamicMeshItem>();

	public List<DynamicMeshItem> UnloadedItems = new List<DynamicMeshItem>();

	public HashSet<DynamicMeshItem> OnLoadingQueue = new HashSet<DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject _regionObject;

	public ConcurrentQueue<Vector3i> AddChunksThreaded = new ConcurrentQueue<Vector3i>();

	public ConcurrentHashSet<Vector3i> LoadedChunksThreaded = new ConcurrentHashSet<Vector3i>();

	public List<Vector3i> LoadedChunks = new List<Vector3i>();

	public DateTime CreateDate = DateTime.Now;

	public DateTime NextLoadTime = DateTime.Now;

	public DynamicRegionState State;

	public bool InBuffer;

	public bool FastTrackLoaded;

	public bool MarkedForDeletion;

	public bool OutsideLoadArea = true;

	public bool IsThreadedRegion;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Rect Rect { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool RegenRequired { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int xIndex { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int zIndex { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> Instances { get; set; }

	public GameObject RegionObject
	{
		get
		{
			return _regionObject;
		}
		set
		{
			if (_regionObject != null)
			{
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Removing old region mesh: " + ToDebugLocation() + " buff: " + InBuffer + "  oldPos " + _regionObject.transform.position.ToString() + " vs " + _regionObject.transform.position.ToString());
				}
				DynamicMeshManager.MeshDestroy(_regionObject);
			}
			_regionObject = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsMeshLoaded { get; set; }

	public string Path => DynamicMeshFile.MeshLocation + Key + ".group";

	public int Triangles
	{
		get
		{
			int num = 0;
			if (RegionObject != null && RegionObject.GetComponent<MeshFilter>().mesh.isReadable)
			{
				num += RegionObject.GetComponent<MeshFilter>().mesh.triangles.Length;
				foreach (Transform item in RegionObject.transform)
				{
					num += item.gameObject.GetComponent<MeshFilter>().mesh.triangles.Length;
				}
			}
			return num;
		}
	}

	public int Vertices
	{
		get
		{
			int num = 0;
			if (RegionObject != null && RegionObject.GetComponent<MeshFilter>().mesh.isReadable)
			{
				num += RegionObject.GetComponent<MeshFilter>().mesh.vertexCount;
				foreach (Transform item in RegionObject.transform)
				{
					num += item.gameObject.GetComponent<MeshFilter>().mesh.vertexCount;
				}
			}
			return num;
		}
	}

	public int RegionObjects
	{
		get
		{
			int result = 0;
			if (RegionObject != null)
			{
				result = RegionObject.transform.childCount + 1;
			}
			return result;
		}
	}

	public EntityPlayer GetPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (GameManager.Instance == null)
			{
				return null;
			}
			if (GameManager.Instance.World == null)
			{
				return null;
			}
			if (GameManager.IsDedicatedServer)
			{
				if (GameManager.Instance.World.Players.Count <= 0)
				{
					return null;
				}
				return GameManager.Instance.World.Players.list[0];
			}
			return GameManager.Instance.World.GetPrimaryPlayer();
		}
	}

	public DynamicMeshRegion(long key)
	{
		Vector3i vector3i = (WorldPosition = new Vector3i(WorldChunkCache.extractX(key) * 16, 0, WorldChunkCache.extractZ(key) * 16));
		Key = key;
		Rect = new Rect(vector3i.x, vector3i.z, 160f, 160f);
		xIndex = (int)((double)vector3i.x / 160.0);
		zIndex = (int)((double)vector3i.z / 160.0);
	}

	public DynamicMeshRegion(Vector3i worldPos)
	{
		WorldPosition = DynamicMeshUnity.GetRegionPositionFromWorldPosition(worldPos);
		Key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(WorldPosition.x), World.toChunkXZ(WorldPosition.z));
		Rect = new Rect(WorldPosition.x, WorldPosition.z, 160f, 160f);
		xIndex = (int)((double)worldPos.x / 160.0);
		zIndex = (int)((double)worldPos.z / 160.0);
	}

	public void AddToLoadingQueue(DynamicMeshItem item)
	{
		OnLoadingQueue.Add(item);
		if (OnLoadingQueue.Count == 0)
		{
			SetVisibleNew(active: false, "LoadingQueueEmpty");
		}
	}

	public void RemoveFromLoadingQueue(DynamicMeshItem item)
	{
		OnLoadingQueue.Remove(item);
	}

	public override GameObject GetGameObject()
	{
		return RegionObject;
	}

	public bool IsInBuffer()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return false;
		}
		return DynamicMeshUnity.IsInBuffer(primaryPlayer.position.x, primaryPlayer.position.z, BufferIndexSize, xIndex, zIndex);
	}

	public static bool IsInBuffer(int x, int z)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return false;
		}
		if (Math.Abs((int)(primaryPlayer.position.x / 160f) - x / 160) <= BufferIndexSize)
		{
			return Math.Abs((int)(primaryPlayer.position.x / 160f) - z / 160) <= BufferIndexSize;
		}
		return false;
	}

	public bool IsInItemLoad()
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return false;
		}
		return DynamicMeshUnity.IsInBuffer(primaryPlayer.position.x, primaryPlayer.position.z, ItemLoadIndex, xIndex, zIndex);
	}

	public bool IsInItemLoad(float x, float z)
	{
		return DynamicMeshUnity.IsInBuffer(x, z, ItemLoadIndex, xIndex, zIndex);
	}

	public bool IsInItemUnload()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return false;
		}
		return !DynamicMeshUnity.IsInBuffer(primaryPlayer.position.x, primaryPlayer.position.z, ItemUnloadIndex, xIndex, zIndex);
	}

	public bool FileExists()
	{
		return SdFile.Exists(Path);
	}

	public static DynamicMeshRegion GetRegionFromWorldPosition(Vector3i worldPos)
	{
		return DynamicMeshManager.Instance.GetRegion(worldPos);
	}

	public static DynamicMeshRegion GetRegionFromWorldPosition(float worldX, float worldZ)
	{
		return GetRegionFromWorldPosition((int)worldX, (int)worldZ);
	}

	public static DynamicMeshRegion GetRegionFromWorldPosition(int worldX, int worldZ)
	{
		long regionKeyFromWorldPosition = DynamicMeshUnity.GetRegionKeyFromWorldPosition(worldX, worldZ);
		Regions.TryGetValue(regionKeyFromWorldPosition, out var value);
		return value;
	}

	public bool AddItemToLoadedList(DynamicMeshItem item)
	{
		if (item == null)
		{
			return false;
		}
		for (int i = 0; i < UnloadedItems.Count; i++)
		{
			if (UnloadedItems[i]?.Key == item.Key)
			{
				UnloadedItems.RemoveAt(i);
				LoadedItems.Add(item);
				return true;
			}
		}
		return false;
	}

	public bool HideIfAllLoaded()
	{
		if (RegionObject == null)
		{
			return false;
		}
		if (UnloadedItems.Count > 0)
		{
			if (LoadedItems.Count <= 0 || UnloadedItems.Count != 1 || UnloadedItems[0].WorldPosition.x != WorldPosition.x || UnloadedItems[0].WorldPosition.z != WorldPosition.z)
			{
				return false;
			}
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Override for single item " + ToDebugLocation());
			}
		}
		else
		{
			if (!RegionObject.activeSelf)
			{
				return false;
			}
			if (LoadedItems.Count == 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsVisible()
	{
		if (RegionObject != null)
		{
			return RegionObject.activeSelf;
		}
		return false;
	}

	public bool AddChunk(int x, int z)
	{
		if (HasChunk(x, z))
		{
			return false;
		}
		LoadedChunks.Add(new Vector3i(x, 0, z));
		return true;
	}

	public bool AddChunk(Vector3i chunk)
	{
		if (HasChunk(chunk.x, chunk.z))
		{
			return false;
		}
		LoadedChunks.Add(chunk);
		return true;
	}

	public bool AddThreadedChunk(int x, int z)
	{
		AddChunksThreaded.Enqueue(new Vector3i(x, 0, z));
		return true;
	}

	public bool HasChunk(int x, int z)
	{
		for (int i = 0; i < LoadedChunks.Count; i++)
		{
			if (LoadedChunks[i].x == x && LoadedChunks[i].z == z)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasChunkAny(int x, int z)
	{
		for (int i = 0; i < LoadedItems.Count; i++)
		{
			Vector3i worldPosition = LoadedItems[i].WorldPosition;
			if (worldPosition.x == x && worldPosition.z == z)
			{
				return true;
			}
		}
		for (int j = 0; j < UnloadedItems.Count; j++)
		{
			Vector3i worldPosition2 = UnloadedItems[j].WorldPosition;
			if (worldPosition2.x == x && worldPosition2.z == z)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsRegionLoadedAndActive(bool doDebug)
	{
		if (DynamicMeshManager.Instance.PrefabCheck == PrefabCheckState.Run)
		{
			return true;
		}
		if (RegionObject == null || LoadedItems.Count == 0 || LoadedItems.Any([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshItem d) => d.State != DynamicItemState.Loaded && d.State != DynamicItemState.ReadyToDelete && d.State != DynamicItemState.Empty))
		{
			if (UnloadedItems.Count > 0)
			{
				LoadItems(urgent: true, !IsVisible(), includeUnloaded: true, "IsRegionLoadedAndActive");
				if (UnloadedItems.Count == 1)
				{
					DynamicMeshItem dynamicMeshItem = UnloadedItems[0];
					if (dynamicMeshItem.WorldPosition.x == WorldPosition.x && dynamicMeshItem.WorldPosition.z == WorldPosition.z)
					{
						return true;
					}
				}
			}
			if (doDebug)
			{
				if (RegionObject == null)
				{
					LogMsg("LoadedAndActive Failed: regionObject null on " + ToDebugLocation());
				}
				if (UnloadedItems.Count > 0)
				{
					LogMsg("LoadedAndActive Failed: unloaded items on " + ToDebugLocation());
				}
				if (LoadedItems.Count == 0)
				{
					LogMsg("LoadedAndActive Failed: no loaded items on " + ToDebugLocation());
				}
				if (LoadedItems.Any([PublicizedFrom(EAccessModifier.Internal)] (DynamicMeshItem d) => d.State != DynamicItemState.Loaded && d.State != DynamicItemState.Empty))
				{
					LogMsg("LoadedAndActive Failed: loaded or empty");
				}
			}
			return false;
		}
		return true;
	}

	public bool ContainsPrefab(PrefabInstance p)
	{
		if (!Intersects(p.boundingBoxPosition.x, p.boundingBoxPosition.z, p.boundingBoxPosition.x + p.boundingBoxSize.x, p.boundingBoxPosition.z + p.boundingBoxSize.z))
		{
			return Intersects(p.boundingBoxPosition.x, p.boundingBoxPosition.z + p.boundingBoxSize.z, p.boundingBoxPosition.x + p.boundingBoxSize.x, p.boundingBoxPosition.z);
		}
		return true;
	}

	public bool Intersects(int x1, int y1, int x2, int y2)
	{
		int num = Math.Min(x1, x2);
		int num2 = Math.Max(x1, x2);
		int num3 = Math.Min(y1, y2);
		int num4 = Math.Max(y1, y2);
		if (Rect.xMin > (float)num2 || Rect.xMax < (float)num)
		{
			return false;
		}
		if (Rect.yMin > (float)num4 || Rect.yMax < (float)num3)
		{
			return false;
		}
		if (Rect.xMin < (float)num && (float)num2 < Rect.xMax)
		{
			return true;
		}
		if (Rect.yMin < (float)num3 && (float)num4 < Rect.yMax)
		{
			return true;
		}
		Func<float, float> func = [PublicizedFrom(EAccessModifier.Internal)] (float num7) => (float)y1 - (num7 - (float)x1) * (float)((y1 - y2) / (x2 - x1));
		float num5 = func(Rect.xMin);
		float num6 = func(Rect.xMax);
		if (Rect.yMax < num5 && Rect.yMax < num6)
		{
			return false;
		}
		if (Rect.yMin > num5 && Rect.yMin > num6)
		{
			return false;
		}
		return true;
	}

	public void OnChunkVisible(DynamicMeshItem item)
	{
		VisibleChunks++;
		ShowItems();
		HideRegion("onChunkVisible");
	}

	public void OnChunkUnloaded(DynamicMeshItem item)
	{
		if (VisibleChunks > 0)
		{
			VisibleChunks--;
		}
		if (VisibleChunks == 0)
		{
			Vector3i worldPosition = WorldPosition;
			SetVisibleNew(active: true, "All chunks unloaded on " + worldPosition.ToString());
			HideItems();
		}
	}

	public void SetVisibleNew(bool active, string reason, bool updateItems = true)
	{
		if (RegionObject != null && active != RegionObject.activeSelf && (!active || !IsPlayerInRegion()))
		{
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Changing view state for " + ToDebugLocation() + " to visible: " + active + "      Reason: " + reason);
			}
			RegionObject.SetActive(active);
			if (DynamicMeshManager.DebugItemPositions)
			{
				RegionObject.name = ToDebugLocation() + ": " + reason;
			}
		}
	}

	public void HideItems()
	{
		foreach (DynamicMeshItem loadedItem in LoadedItems)
		{
			loadedItem.SetVisible(active: false, "Region hide");
		}
	}

	public void ShowItems()
	{
		foreach (DynamicMeshItem loadedItem in LoadedItems)
		{
			loadedItem.SetVisible(!loadedItem.IsChunkInGame, "Region show");
		}
		if (OnLoadingQueue.Count == 0)
		{
			SetVisibleNew(active: false, "ShowItems HideRegion");
		}
	}

	public bool LoadItems(bool urgent, bool visible, bool includeUnloaded, string reason)
	{
		if (!IsInItemLoad())
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Load items ignore as outside " + ToDebugLocation());
			}
			HideItems();
			return false;
		}
		bool flag = false;
		for (int i = 0; i < LoadedItems.Count; i++)
		{
			DynamicMeshItem dynamicMeshItem = LoadedItems[i];
			if (dynamicMeshItem != null)
			{
				flag = dynamicMeshItem.LoadIfEmpty("region load '" + reason + "'", urgent, InBuffer) || flag;
			}
		}
		if (includeUnloaded)
		{
			for (int j = 0; j < UnloadedItems.Count; j++)
			{
				DynamicMeshItem dynamicMeshItem2 = UnloadedItems[j];
				if (dynamicMeshItem2 != null)
				{
					flag = dynamicMeshItem2.LoadIfEmpty("region load unloaded", urgent, InBuffer) || flag;
				}
			}
		}
		return UnloadedItems.Count > 0 || flag;
	}

	public void ShowDebug()
	{
		if (DynamicMeshManager.DoLog)
		{
			Vector3i worldPosition = WorldPosition;
			DynamicMeshManager.LogMsg("Region: " + worldPosition.ToString() + "  Object: " + ((RegionObject == null) ? "null" : RegionObject.activeSelf.ToString()));
		}
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("Chunks: " + LoadedChunks.Count);
		}
		foreach (Vector3i loadedChunk in LoadedChunks)
		{
			Log.Out(loadedChunk.ToString());
		}
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("Items: " + LoadedItems.Count + " vs Unloaded: " + UnloadedItems.Count);
		}
		foreach (DynamicMeshItem loadedItem in LoadedItems)
		{
			if (DynamicMeshManager.DoLog)
			{
				string[] array = new string[5];
				Vector3i worldPosition = loadedItem.WorldPosition;
				array[0] = worldPosition.ToString();
				array[1] = "  Object: ";
				array[2] = ((loadedItem.ChunkObject == null) ? "null" : loadedItem.ChunkObject.activeSelf.ToString());
				array[3] = "  State: ";
				array[4] = loadedItem.State.ToString();
				DynamicMeshManager.LogMsg(string.Concat(array));
			}
		}
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("--unloaded--");
		}
		foreach (DynamicMeshItem unloadedItem in UnloadedItems)
		{
			if (DynamicMeshManager.DoLog)
			{
				string[] array2 = new string[5];
				Vector3i worldPosition = unloadedItem.WorldPosition;
				array2[0] = worldPosition.ToString();
				array2[1] = "  Object: ";
				array2[2] = ((unloadedItem.ChunkObject == null) ? "null" : unloadedItem.ChunkObject.activeSelf.ToString());
				array2[3] = "  State: ";
				array2[4] = unloadedItem.State.ToString();
				DynamicMeshManager.LogMsg(string.Concat(array2));
			}
		}
	}

	public void OnCorrupted()
	{
		if (DynamicMeshManager.DoLog)
		{
			Vector3i worldPosition = WorldPosition;
			DynamicMeshManager.LogMsg("Corrupted region. Adding for regen " + worldPosition.ToString());
		}
		foreach (DynamicMeshItem loadedItem in LoadedItems)
		{
			DynamicMeshManager.Instance.AddChunk(loadedItem.WorldPosition, primary: true);
		}
	}

	public void SetPosition()
	{
		if (RegionObject != null)
		{
			Vector3 vector = WorldPosition.ToVector3() - Origin.position;
			if (RegionObject.transform.position != vector)
			{
				RegionObject.transform.position = vector;
			}
		}
	}

	public void HideRegion(string debugReason)
	{
		SetVisibleNew(active: false, debugReason);
		ShowItems();
	}

	public void SetViewStats(bool inBuffer, bool shouldLoadItems, bool shouldUnloadItems, bool isOutsideMaxRegionArea)
	{
		OutsideLoadArea = isOutsideMaxRegionArea;
		if (shouldLoadItems)
		{
			LoadItems(urgent: false, visible: true, includeUnloaded: true, "setViewStatsShouldLoadItem");
		}
		if (IsPlayerInRegion())
		{
			HideRegion("set view stats all items loaded");
		}
		if (!inBuffer && !isOutsideMaxRegionArea)
		{
			SetVisibleNew(active: true, "SetViewStats visible");
		}
		if (RegionObject == null && FileExists())
		{
			if (isOutsideMaxRegionArea)
			{
				SetState(DynamicRegionState.Unloaded, forceChange: false);
			}
			else if (State != DynamicRegionState.StartLoad)
			{
				SetState(DynamicRegionState.StartLoad, forceChange: false);
				DynamicMeshManager.AddRegionLoadMeshes(Key);
			}
		}
		if (inBuffer != InBuffer)
		{
			InBuffer = inBuffer;
			if (inBuffer)
			{
				if (!LoadItems(urgent: true, visible: true, includeUnloaded: true, "setViewInBuffer"))
				{
					SetState(DynamicRegionState.Loaded, forceChange: false);
				}
			}
			else
			{
				SetVisibleNew(active: true, "SetViewStats leftBuffer");
			}
		}
		if (shouldUnloadItems && LoadedItems.Count > 0)
		{
			ClearItems();
		}
	}

	public float DistanceToPlayer()
	{
		EntityPlayer getPlayer = GetPlayer;
		if (getPlayer == null)
		{
			return 999999f;
		}
		Vector3 position = getPlayer.position;
		int num = 80;
		return Math.Abs(Mathf.Sqrt(Mathf.Pow(position.x - (float)(WorldPosition.x + num), 2f) + Mathf.Pow(position.z - (float)(WorldPosition.z + num), 2f)));
	}

	public void SetState(DynamicRegionState newState, bool forceChange)
	{
		if (!forceChange && newState == DynamicRegionState.Unloading && State == DynamicRegionState.Unloaded)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Can't change state from unloading to unloaded");
			}
		}
		else
		{
			State = newState;
		}
	}

	public void RemoveChunk(int x, int z, string reason, bool removedFromWorld)
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("Removing chunk " + x + "," + z + ": " + reason);
		}
		DynamicMeshThread.ChunkDataQueue.MarkForDeletion(DynamicMeshUnity.GetRegionKeyFromWorldPosition(x, z));
		for (int i = 0; i < UnloadedItems.Count; i++)
		{
			DynamicMeshItem dynamicMeshItem = UnloadedItems[i];
			if (dynamicMeshItem != null && dynamicMeshItem.WorldPosition.x == x && dynamicMeshItem.WorldPosition.z == z)
			{
				dynamicMeshItem.DestroyChunk();
				if (removedFromWorld)
				{
					UnloadedItems.RemoveAt(i);
				}
				else
				{
					dynamicMeshItem.State = DynamicItemState.ReadyToDelete;
				}
				break;
			}
		}
		for (int j = 0; j < LoadedItems.Count; j++)
		{
			DynamicMeshItem dynamicMeshItem2 = LoadedItems[j];
			if (dynamicMeshItem2 != null && dynamicMeshItem2.WorldPosition.x == x && dynamicMeshItem2.WorldPosition.z == z)
			{
				dynamicMeshItem2.DestroyChunk();
				if (removedFromWorld)
				{
					LoadedItems.RemoveAt(j);
				}
				else
				{
					dynamicMeshItem2.State = DynamicItemState.ReadyToDelete;
				}
				break;
			}
		}
		LoadedChunks.Remove(new Vector3i(x, 0, z));
	}

	public void AddItem(DynamicMeshItem item)
	{
		int num = 0;
		int num2 = 0;
		try
		{
			num = 1;
			num2 = ((item == null) ? 1 : 0);
			if (item == null)
			{
				Log.Error("null item tried to be added");
				return;
			}
			num = 2;
			bool flag = false;
			for (int i = 0; i < LoadedItems.Count; i++)
			{
				DynamicMeshItem dynamicMeshItem = LoadedItems[i];
				num = 3;
				if (dynamicMeshItem != null)
				{
					num = 4;
					num2 = ((item == null) ? 1 : 0) + ((dynamicMeshItem == null) ? 10 : 0);
					if (dynamicMeshItem.Key == item.Key)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				num = 5;
				for (int j = 0; j < UnloadedItems.Count; j++)
				{
					num = 6;
					DynamicMeshItem dynamicMeshItem2 = UnloadedItems[j];
					num2 = ((item == null) ? 1 : 0);
					if (dynamicMeshItem2 != null)
					{
						num = 7;
						num2 = ((item == null) ? 1 : 0) + ((dynamicMeshItem2 == null) ? 10 : 0);
						if (dynamicMeshItem2.Key == item.Key)
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (!flag)
			{
				num = 8;
				if (GameManager.IsDedicatedServer)
				{
					num = 9;
					LoadedItems.Add(item);
				}
				else
				{
					num = 10;
					UnloadedItems.Add(item);
				}
			}
			num = 11;
		}
		catch (Exception)
		{
			Log.Error("Add Item error at stage: " + num + " nulls: " + num2);
		}
	}

	public int GetStreamLength()
	{
		int val = 8 + LoadedChunks.Distinct().Count() * 8 + 8 + 12 + 1 + 4;
		return Math.Max(10240, val);
	}

	public void CleanUp()
	{
		if (RegionObject != null)
		{
			DynamicMeshManager.MeshDestroy(RegionObject);
			RegionObject = null;
		}
		ClearMeshes();
		State = DynamicRegionState.Unloaded;
		IsMeshLoaded = false;
	}

	public bool IsPlayerInRegion()
	{
		EntityPlayerLocal player = DynamicMeshManager.player;
		if (player == null)
		{
			return false;
		}
		return DynamicMeshManager.Instance.GetRegion((int)player.position.x, (int)player.position.z).WorldPosition == WorldPosition;
	}

	public void DistanceChecks()
	{
		float num = DistanceToPlayer();
		bool inBuffer = IsInBuffer();
		bool shouldLoadItems = IsInItemLoad();
		bool shouldUnloadItems = IsInItemUnload();
		bool flag = num >= (float)DynamicMeshSettings.MaxViewDistance || DynamicMeshManager.IsOutsideDistantTerrain(this);
		SetPosition();
		SetViewStats(inBuffer, shouldLoadItems, shouldUnloadItems, flag);
		if (State == DynamicRegionState.Unloaded && num < (float)DynamicMeshSettings.MaxViewDistance && RegionObject == null)
		{
			_ = OutsideLoadArea;
		}
		if (!(RegionObject == null) || State == DynamicRegionState.Unloaded)
		{
		}
		if (RegionObject != null && flag && DynamicMeshFile.CurrentlyLoadingRegionPosition != WorldPosition)
		{
			CleanUp();
		}
	}

	public static void LogMsg(string msg)
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(msg);
		}
	}

	public void ClearItems()
	{
		foreach (DynamicMeshItem loadedItem in LoadedItems)
		{
			loadedItem.CleanUp();
			UnloadedItems.Add(loadedItem);
		}
		LoadedItems.Clear();
		SetVisibleNew(active: true, "itemsUnloaded");
	}

	public void ClearMeshes()
	{
	}
}
