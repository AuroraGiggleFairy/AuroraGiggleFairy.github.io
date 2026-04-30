using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DynamicMeshItem : DynamicMeshContainer, IEquatable<DynamicMeshItem>
{
	public static HashSet<GameObject> MeshPool = new HashSet<GameObject>();

	public static HashSet<GameObject> TerrainMeshPool = new HashSet<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static int cacheId = 0;

	public GameObject ChunkObject;

	public int UpdateTime;

	public int PackageLength;

	public DynamicItemState State = DynamicItemState.UpdateRequired;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Rect Rect { get; set; }

	public int Triangles
	{
		get
		{
			int num = 0;
			if (ChunkObject != null)
			{
				num += ChunkObject.GetComponent<MeshFilter>().mesh.triangles.Length;
				foreach (Transform item in ChunkObject.transform)
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
			if (ChunkObject != null)
			{
				num += ChunkObject.GetComponent<MeshFilter>().mesh.vertexCount;
				foreach (Transform item in ChunkObject.transform)
				{
					num += item.gameObject.GetComponent<MeshFilter>().mesh.vertexCount;
				}
			}
			return num;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (ChunkObject != null)
			{
				return ChunkObject.activeSelf;
			}
			return false;
		}
	}

	public bool IsChunkInView
	{
		get
		{
			if (GameManager.IsDedicatedServer)
			{
				return false;
			}
			if (DynamicMeshManager.Instance == null)
			{
				return false;
			}
			Vector3 position = player.position;
			int viewSize = DynamicMeshManager.GetViewSize();
			int num = World.toChunkXZ(Utils.Fastfloor(position.x)) * 16;
			int num2 = World.toChunkXZ(Utils.Fastfloor(position.z)) * 16;
			int num3 = num - viewSize;
			int num4 = num + viewSize;
			int num5 = num2 - viewSize;
			int num6 = num2 + viewSize;
			if (WorldPosition.x > num3 && WorldPosition.x <= num4)
			{
				if (WorldPosition.z > num5)
				{
					return WorldPosition.z <= num6;
				}
				return false;
			}
			return false;
		}
	}

	public bool IsChunkInGame
	{
		get
		{
			if (GameManager.IsDedicatedServer)
			{
				return false;
			}
			return DynamicMeshManager.ChunkGameObjects.Contains(Key);
		}
	}

	public string Path => DynamicMeshUnity.GetItemPath(Key);

	public static EntityPlayerLocal player
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (GameManager.Instance.World != null)
			{
				return GameManager.Instance.World.GetPrimaryPlayer();
			}
			return null;
		}
	}

	public override GameObject GetGameObject()
	{
		return ChunkObject;
	}

	public DynamicMeshItem(Vector3i pos)
	{
		WorldPosition = pos;
		Rect = new Rect(pos.x, pos.z, 16f, 16f);
		Key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(pos.x), World.toChunkXZ(pos.z));
	}

	public static void AddToMeshPool(GameObject go)
	{
		if (!(go == null))
		{
			DynamicMeshManager.MeshDestroy(go);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool AddToPoolInternal(GameObject go)
	{
		foreach (GameObject item in MeshPool)
		{
			if (item.GetInstanceID() == go.GetInstanceID())
			{
				Log.Warning("Duplicate pool add. Name: " + go.name);
				return false;
			}
		}
		MeshPool.Add(go);
		go.transform.parent = null;
		go.SetActive(value: false);
		go.GetComponent<MeshFilter>().mesh.Clear(keepVertexLayout: false);
		return true;
	}

	public static GameObject GetItemMeshRendererFromPool()
	{
		GameObject gameObject;
		if (MeshPool.Count > 0)
		{
			gameObject = MeshPool.Last();
			MeshPool.Remove(gameObject);
		}
		else
		{
			gameObject = DynamicMeshFile.CreateMeshObject(string.Empty, isRegion: false);
		}
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.parent = DynamicMeshManager.ParentTransform;
		return gameObject;
	}

	public static GameObject GetRegionMeshRendererFromPool()
	{
		GameObject gameObject;
		if (MeshPool.Count > 0)
		{
			gameObject = MeshPool.Last();
			MeshPool.Remove(gameObject);
		}
		else
		{
			gameObject = DynamicMeshFile.CreateMeshObject(string.Empty, isRegion: true);
		}
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.parent = DynamicMeshManager.ParentTransform;
		return gameObject;
	}

	public static GameObject GetTerrainMeshRendererFromPool()
	{
		GameObject gameObject;
		if (TerrainMeshPool.Count > 0)
		{
			gameObject = TerrainMeshPool.Last();
			TerrainMeshPool.Remove(gameObject);
		}
		else
		{
			gameObject = DynamicMeshFile.CreateTerrainMeshObject(string.Empty);
		}
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.parent = DynamicMeshManager.ParentTransform;
		return gameObject;
	}

	public void CleanUp()
	{
		if (ChunkObject != null)
		{
			DynamicMeshManager.MeshDestroy(ChunkObject);
		}
		State = DynamicItemState.UpdateRequired;
	}

	public Vector3i GetRegionLocation()
	{
		return DynamicMeshUnity.GetRegionPositionFromWorldPosition(WorldPosition);
	}

	public long GetRegionKey()
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(DynamicMeshUnity.RoundRegion(WorldPosition.x)), World.toChunkXZ(DynamicMeshUnity.RoundRegion(WorldPosition.z)));
	}

	public int ReadUpdateTimeFromFile()
	{
		using Stream baseStream = SdFile.OpenRead(Path);
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		pooledBinaryReader.SetBaseStream(baseStream);
		return pooledBinaryReader.ReadInt32();
	}

	public DynamicMeshRegion GetRegion()
	{
		DynamicMeshRegion.GetRegionFromWorldPosition(WorldPosition);
		DynamicMeshRegion.Regions.TryGetValue(GetRegionKey(), out var value);
		return value;
	}

	public void SetVisible(bool active, string reason)
	{
		if (!(ChunkObject == null) && active != ChunkObject.activeSelf)
		{
			if (DynamicMeshManager.DebugItemPositions)
			{
				ChunkObject.name = "C " + ToDebugLocation() + ": " + reason + " (" + active + ")";
			}
			ChunkObject.SetActive(active);
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Chunk " + WorldPosition.x + "," + WorldPosition.z + " Visible: " + active + " reason: " + reason + " inview: " + IsChunkInView);
			}
		}
	}

	public void ForceHide()
	{
		if (!(ChunkObject == null) && ChunkObject.activeSelf)
		{
			if (DynamicMeshManager.DebugItemPositions)
			{
				ChunkObject.name = "C " + ToDebugLocation() + ": forceHide";
			}
			ChunkObject.SetActive(value: false);
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Chunk " + WorldPosition.x + "," + WorldPosition.z + " ForceHide");
			}
		}
	}

	public void OnCorrupted()
	{
		if (DynamicMeshManager.DoLog)
		{
			Vector3i worldPosition = WorldPosition;
			DynamicMeshManager.LogMsg("Corrupted item. Adding for regen " + worldPosition.ToString());
		}
		DynamicMeshManager.Instance.AddChunk(WorldPosition, primary: true);
	}

	public bool LoadIfEmpty(string caller, bool urgentLoad, bool regionInBuffer)
	{
		if (ChunkObject != null)
		{
			return false;
		}
		if (State == DynamicItemState.ReadyToDelete)
		{
			return false;
		}
		if (State == DynamicItemState.Empty)
		{
			return false;
		}
		if (State == DynamicItemState.LoadRequested)
		{
			return false;
		}
		if (!DynamicMeshManager.Instance.IsInLoadableArea(Key))
		{
			return false;
		}
		State = DynamicItemState.LoadRequested;
		DynamicMeshManager.Instance.AddItemLoadRequest(this, urgentLoad);
		return true;
	}

	public bool Load(string caller, bool urgentLoad, bool regionInBuffer)
	{
		if (State == DynamicItemState.ReadyToDelete)
		{
			return false;
		}
		if (State == DynamicItemState.LoadRequested)
		{
			return false;
		}
		State = DynamicItemState.LoadRequested;
		DynamicMeshManager.Instance.AddItemLoadRequest(this, urgentLoad);
		return true;
	}

	public float DistanceToPlayer(Vector3i playerPos)
	{
		return Math.Abs(Mathf.Sqrt(Mathf.Pow(playerPos.x - WorldPosition.x, 2f) + Mathf.Pow(playerPos.z - WorldPosition.z, 2f)));
	}

	public float DistanceToPlayer()
	{
		EntityPlayerLocal entityPlayerLocal = player;
		Vector3 vector = ((entityPlayerLocal == null) ? Vector3.zero : entityPlayerLocal.position);
		return Math.Abs(Mathf.Sqrt(Mathf.Pow(vector.x - (float)WorldPosition.x, 2f) + Mathf.Pow(vector.z - (float)WorldPosition.z, 2f)));
	}

	public float DistanceToPlayer(float x, float z)
	{
		return Math.Abs(Mathf.Sqrt(Mathf.Pow(x - (float)WorldPosition.x, 2f) + Mathf.Pow(z - (float)WorldPosition.z, 2f)));
	}

	public bool DestroyChunk()
	{
		bool result = false;
		if (ChunkObject != null)
		{
			result = true;
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Destroying chunk " + ToDebugLocation());
			}
			DynamicMeshManager.MeshDestroy(ChunkObject);
		}
		return result;
	}

	public void DestroyMesh()
	{
		DynamicMeshManager.Instance.AddObjectForDestruction(ChunkObject);
		ChunkObject = null;
	}

	public bool CreateMeshSync(bool isVisible)
	{
		if (ChunkObject != null)
		{
			SetVisible(isVisible, "Create mesh exists");
			return false;
		}
		string path = Path;
		if (!SdFile.Exists(path))
		{
			State = DynamicItemState.Empty;
			return false;
		}
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false))
		{
			using Stream baseStream = DynamicMeshFile.GetReadStream(path);
			pooledBinaryReader.SetBaseStream(baseStream);
			if (pooledBinaryReader.BaseStream.Position == pooledBinaryReader.BaseStream.Length)
			{
				State = DynamicItemState.Empty;
			}
			else
			{
				DynamicMeshFile.ReadItemMesh(pooledBinaryReader, this, isVisible);
			}
		}
		if (ChunkObject != null)
		{
			SetVisible(isVisible, "create mesh complete");
			SetPosition();
			Quaternion identity = Quaternion.identity;
			ChunkObject.transform.parent = DynamicMeshManager.ParentTransform;
			ChunkObject.transform.rotation = identity;
		}
		PackageLength = GetStreamLength();
		State = DynamicItemState.Loaded;
		return true;
	}

	public void SetPosition()
	{
		if (!(ChunkObject == null))
		{
			ChunkObject.transform.position = WorldPosition.ToVector3() - Origin.position;
		}
	}

	public bool FileExists()
	{
		return SdFile.Exists(Path);
	}

	public IEnumerator CreateMeshFromVoxelCoroutine(bool isVisible, MicroStopwatch stop, DynamicMeshVoxelLoad data)
	{
		GameObject oldMesh = ChunkObject;
		ChunkObject = null;
		State = DynamicItemState.Loading;
		yield return GameManager.Instance.StartCoroutine(data.CreateMeshCoroutine(this));
		if (ChunkObject != null)
		{
			Quaternion identity = Quaternion.identity;
			ChunkObject.transform.parent = DynamicMeshManager.ParentTransform;
			ChunkObject.transform.rotation = identity;
			SetVisible(isVisible, "create mesh complete coroutine");
			SetPosition();
			State = DynamicItemState.Loaded;
		}
		else
		{
			State = DynamicItemState.Empty;
		}
		if (oldMesh != null)
		{
			AddToMeshPool(oldMesh);
		}
	}

	public override int GetHashCode()
	{
		return Key.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DynamicMeshItem dynamicMeshItem))
		{
			return false;
		}
		return dynamicMeshItem.Key == Key;
	}

	public int GetStreamLength()
	{
		int num = 20;
		if (ChunkObject == null)
		{
			return num;
		}
		MeshFilter component = ChunkObject.GetComponent<MeshFilter>();
		num += 12 + component.mesh.vertexCount * 6 + component.mesh.vertexCount * 8 + component.mesh.triangles.Length * 2;
		foreach (Transform item in ChunkObject.transform)
		{
			component = item.gameObject.GetComponent<MeshFilter>();
			num += 12 + component.mesh.vertexCount * 6 + component.mesh.vertexCount * 8 + component.mesh.triangles.Length * 2;
		}
		return num;
	}

	public int GetStreamLength(List<VoxelMesh> meshes, List<VoxelMeshTerrain> terrainMeshes)
	{
		int num = 20;
		foreach (VoxelMesh mesh in meshes)
		{
			num += mesh.GetByteLength();
		}
		foreach (VoxelMeshTerrain terrainMesh in terrainMeshes)
		{
			num += terrainMesh.GetByteLength();
		}
		return num;
	}

	public bool Equals(DynamicMeshItem other)
	{
		return other.Key == Key;
	}
}
