using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ChunkPreviewManager
{
	public static ChunkPreviewManager Instance;

	public List<DynamicMeshItem> Items = new List<DynamicMeshItem>();

	public ChunkPreviewData PreviewData;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshPrefabPreviewThread ThreadData;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject PreviewChunksContainer;

	public ConcurrentQueue<DynamicMeshVoxelLoad> ChunkPreviewMeshData = new ConcurrentQueue<DynamicMeshVoxelLoad>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool StopRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime NextUpdate = DateTime.Now;

	public Prefab Prefab => PreviewData.PrefabData;

	public Vector3i WorldPosition => PreviewData.WorldPosition;

	public ChunkPreviewManager()
	{
		Instance = this;
		PreviewData = new ChunkPreviewData();
		PreviewChunksContainer = GameObject.Find("PreviewChunks");
		if (PreviewChunksContainer == null)
		{
			PreviewChunksContainer = new GameObject("PreviewChunks");
		}
		DynamicMeshThread.SetDefaultThreads();
		ThreadData = new DynamicMeshPrefabPreviewThread();
		ThreadData.PreviewData = PreviewData;
		ThreadData.StartThread();
		GameManager.Instance.StartCoroutine(LoadPreviewMesh());
	}

	public void AddChunkPreviewLoadData(DynamicMeshVoxelLoad loadData)
	{
		ChunkPreviewMeshData.Enqueue(loadData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LoadPreviewMesh()
	{
		while (!StopRequested)
		{
			if (!ChunkPreviewMeshData.TryDequeue(out var voxelData))
			{
				yield return null;
				continue;
			}
			while (voxelData.Item.State == DynamicItemState.Loading)
			{
				Log.Out("delaying load");
				yield return null;
			}
			voxelData.Item.DestroyChunk();
			yield return GameManager.Instance.StartCoroutine(voxelData.Item.CreateMeshFromVoxelCoroutine(isVisible: true, null, voxelData));
			AddPreviewChunk(voxelData);
			if (voxelData.Item.ChunkObject != null)
			{
				voxelData.Item.ChunkObject.transform.parent = PreviewChunksContainer.transform;
			}
			voxelData.DisposeMeshes();
			voxelData = null;
		}
	}

	public void ClearAll()
	{
		foreach (DynamicMeshItem item in Items)
		{
			item.DestroyChunk();
		}
		Items.Clear();
	}

	public void AddPreviewChunk(DynamicMeshVoxelLoad loadData)
	{
		DynamicMeshItem item = loadData.Item;
		if (IsPositionInArea(loadData.Item.WorldPosition))
		{
			SetChunkGoVisiblity(item.Key, isVisible: false);
		}
		else
		{
			SetChunkGoVisiblity(item.Key, isVisible: true);
			loadData.Item.DestroyChunk();
		}
		CheckItems();
	}

	public bool IsPositionInArea(Vector3 pos)
	{
		return IsPositionInArea(new Vector3i(pos));
	}

	public void ActivationChanged(PrefabInstance pi)
	{
		if (pi == null)
		{
			SetWorldPosition(new Vector3i(WorldPosition.x, -512, WorldPosition.z));
			CheckItems();
		}
		else
		{
			SetPrefab(pi.prefab, pi, pi.boundingBoxPosition);
		}
	}

	public bool IsActivePrefab(PrefabInstance pi)
	{
		if (pi.prefab == PreviewData.PrefabData && IsPositionInArea(pi.boundingBoxPosition))
		{
			return true;
		}
		return false;
	}

	public bool IsPositionInArea(Vector3i fullposition)
	{
		if (WorldPosition.y < -256)
		{
			return false;
		}
		Vector3i chunkPositionFromWorldPosition = DynamicMeshUnity.GetChunkPositionFromWorldPosition(fullposition);
		Vector3i chunkPositionFromWorldPosition2 = DynamicMeshUnity.GetChunkPositionFromWorldPosition(PreviewData.WorldPosition);
		Vector3i chunkPositionFromWorldPosition3 = DynamicMeshUnity.GetChunkPositionFromWorldPosition(PreviewData.WorldPosition + PreviewData.PrefabData.size);
		int x = chunkPositionFromWorldPosition2.x;
		int x2 = chunkPositionFromWorldPosition3.x;
		int z = chunkPositionFromWorldPosition2.z;
		int z2 = chunkPositionFromWorldPosition3.z;
		if (chunkPositionFromWorldPosition.x < x || chunkPositionFromWorldPosition.x > x2 || chunkPositionFromWorldPosition.z < z || chunkPositionFromWorldPosition.z > z2)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckItems()
	{
		for (int num = Items.Count - 1; num >= 0; num--)
		{
			DynamicMeshItem dynamicMeshItem = Items[num];
			if (!IsPositionInArea(dynamicMeshItem.WorldPosition))
			{
				SetChunkGoVisiblity(dynamicMeshItem.Key, isVisible: true);
				dynamicMeshItem.DestroyChunk();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChunkGoVisiblity(long chunkKey, bool isVisible)
	{
		string text = $"Chunk_{DynamicMeshUnity.GetChunkSectionX(chunkKey)},{DynamicMeshUnity.GetChunkSectionZ(chunkKey)}";
		foreach (ChunkGameObject usedChunkGameObject in GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects())
		{
			Transform transform = usedChunkGameObject.transform;
			if (transform.name == text)
			{
				if (isVisible)
				{
					transform.gameObject.transform.localScale = Vector3.one;
				}
				else
				{
					transform.gameObject.transform.localScale = Vector3.zero;
				}
			}
		}
	}

	public void CleanUp()
	{
		ThreadData?.StopThread();
	}

	public void SetWorldPosition(Vector3i worldPosition)
	{
		PreviewData.WorldPosition = worldPosition;
		if (Prefab != null)
		{
			int num = DynamicMeshUnity.RoundChunk(worldPosition.x + Prefab.size.x) + 16;
			int num2 = DynamicMeshUnity.RoundChunk(worldPosition.z + Prefab.size.z) + 16;
			for (int i = worldPosition.x; i <= num; i += 16)
			{
				for (int j = worldPosition.z; j <= num2; j += 16)
				{
					StartChunkPreview(new Vector3i(i, 0, j));
				}
			}
		}
		CheckItems();
	}

	public void StartChunkPreview(Vector3i chunkPos)
	{
		DynamicMeshItem item = Get(chunkPos);
		ThreadData.AddChunk(item);
	}

	public void SetPrefab(PrefabInstance prefab)
	{
		SetPrefab(prefab.prefab, prefab, WorldPosition);
	}

	public void SetPrefab(Prefab prefab)
	{
		SetPrefab(prefab, null, WorldPosition);
	}

	public void SetPrefab(Prefab prefab, PrefabInstance instance, Vector3i worldPosition)
	{
		PrefabInstance prefabInstance = PreviewData.PrefabInstance;
		if (prefabInstance != null)
		{
			GameManager.Instance.prefabLODManager.GetInstance(prefabInstance.id)?.go.SetActive(value: true);
		}
		PreviewData.PrefabData = prefab;
		PreviewData.PrefabInstance = instance;
		if (instance != null)
		{
			GameManager.Instance.prefabLODManager.GetInstance(instance.id)?.go.SetActive(value: false);
		}
		SetWorldPosition(worldPosition);
	}

	public DynamicMeshItem Get(Vector3i worldPos)
	{
		worldPos = DynamicMeshUnity.GetChunkPositionFromWorldPosition(worldPos);
		foreach (DynamicMeshItem item in Items)
		{
			if (item.WorldPosition.x == worldPos.x && item.WorldPosition.z == worldPos.z)
			{
				return item;
			}
		}
		DynamicMeshItem dynamicMeshItem = new DynamicMeshItem(worldPos);
		Items.Add(dynamicMeshItem);
		return dynamicMeshItem;
	}

	public void Update()
	{
		if (NextUpdate > DateTime.Now)
		{
			return;
		}
		NextUpdate = DateTime.Now.AddSeconds(1.0);
		foreach (DynamicMeshItem item in Items)
		{
			SetChunkGoVisiblity(item.Key, item.ChunkObject == null || !IsPositionInArea(item.WorldPosition));
		}
	}
}
