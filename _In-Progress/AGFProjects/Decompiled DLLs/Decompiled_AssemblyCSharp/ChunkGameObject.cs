using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChunkGameObject : MonoBehaviour
{
	public Transform blockEntitiesParentT;

	public static int InstanceCount;

	public Chunk chunk;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCluster chunkCluster;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ChunkGameObjectLayer[] layers = new ChunkGameObjectLayer[16];

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public VoxelMeshLayer vml;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentlyCopiedMeshIdx;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCopyCollidersThisCall;

	public Transform wallVolumesParentT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] wallVolumes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch nextMS = new MicroStopwatch(_bStart: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkGameObject()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		blockEntitiesParentT = new GameObject("_BlockEntities").transform;
		blockEntitiesParentT.SetParent(base.transform, worldPositionStays: false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDisable()
	{
		if (chunk != null)
		{
			chunk.OnHide();
		}
		if (vml != null)
		{
			vml.TryFree();
			vml = null;
		}
	}

	public void SetStatic(bool _bStatic)
	{
		for (int i = 0; i < layers.Length; i++)
		{
			if (layers[i] == null)
			{
				continue;
			}
			for (int j = 0; j < MeshDescription.meshes.Length; j++)
			{
				layers[i].m_MeshesGO[j].isStatic = _bStatic;
			}
			for (int k = 0; k < layers[i].m_MeshCollider.Length; k++)
			{
				if (layers[i].m_MeshCollider[k] != null)
				{
					layers[i].m_MeshCollider[k][0].gameObject.isStatic = _bStatic;
				}
			}
		}
		base.transform.gameObject.isStatic = _bStatic;
	}

	public Chunk GetChunk()
	{
		return chunk;
	}

	public void SetChunk(Chunk _chunk, ChunkCluster _chunkCluster)
	{
		if (chunk != null && chunk != _chunk)
		{
			chunk.OnHide();
			lock (chunk)
			{
				chunk.IsCollisionMeshGenerated = false;
				chunk.IsDisplayed = false;
			}
		}
		for (int i = 0; i < layers.Length; i++)
		{
			ChunkGameObjectLayer chunkGameObjectLayer = layers[i];
			if (chunkGameObjectLayer != null)
			{
				chunkGameObjectLayer.m_ParentGO.SetActive(value: false);
				chunkGameObjectLayer.m_ParentGO.transform.SetParent(null, worldPositionStays: false);
				MemoryPools.poolCGOL.FreeSync(chunkGameObjectLayer);
				layers[i] = null;
			}
		}
		chunk = _chunk;
		chunkCluster = _chunkCluster;
		Transform transform = base.transform;
		if (chunk != null)
		{
			chunk.IsDisplayed = true;
			transform.name = _chunk.ToString();
			transform.localPosition = new Vector3(chunk.X * 16, 0f, chunk.Z * 16) - Origin.position;
			GameManager.Instance.StartCoroutine(HandleWallVolumes(chunk, chunkCluster));
		}
		else
		{
			transform.name = "ChunkEmpty";
			RemoveWallVolumes();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveWallVolumes()
	{
		if ((bool)wallVolumesParentT)
		{
			UnityEngine.Object.Destroy(wallVolumesParentT.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator HandleWallVolumes(Chunk _chunk, ChunkCluster _chunkCluster)
	{
		RemoveWallVolumes();
		List<int> wallVolumesId = _chunk.GetWallVolumes();
		WorldBase world = _chunkCluster.GetWorld();
		if (wallVolumesId.Count > 0)
		{
			while (WallVolumesNotLoaded(wallVolumesId, world))
			{
				yield return new WaitForSeconds(1f);
			}
			if (!wallVolumesParentT)
			{
				wallVolumesParentT = new GameObject("_WallVolumes").transform;
				wallVolumesParentT.SetParent(base.transform, worldPositionStays: false);
			}
			Vector3 vector = _chunk.GetWorldPos();
			wallVolumes = new GameObject[wallVolumesId.Count];
			for (int i = 0; i < wallVolumesId.Count; i++)
			{
				int index = wallVolumesId[i];
				WallVolume wallVolume = world.GetWallVolume(index);
				GameObject gameObject = new GameObject(index.ToString());
				gameObject.layer = 16;
				Transform obj = gameObject.transform;
				obj.SetParent(wallVolumesParentT, worldPositionStays: false);
				obj.localPosition = wallVolume.Center - vector;
				gameObject.AddComponent<BoxCollider>().size = wallVolume.BoxMax - wallVolume.BoxMin;
				wallVolumes[i] = gameObject;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool WallVolumesNotLoaded(List<int> wallVolumesId, WorldBase world)
	{
		int wallVolumeCount = world.GetWallVolumeCount();
		for (int i = 0; i < wallVolumesId.Count; i++)
		{
			if (wallVolumesId[i] >= wallVolumeCount)
			{
				return true;
			}
		}
		return false;
	}

	public int StartCopyMeshLayer()
	{
		currentlyCopiedMeshIdx = 0;
		isCopyCollidersThisCall = false;
		ChunkGameObjectLayer chunkGameObjectLayer;
		while (true)
		{
			vml = chunk.GetMeshLayer();
			if (vml == null)
			{
				return -1;
			}
			chunkGameObjectLayer = layers[vml.idx];
			if (vml.HasContent())
			{
				break;
			}
			if (chunkGameObjectLayer != null)
			{
				layers[vml.idx] = null;
				chunkGameObjectLayer.m_ParentGO.SetActive(value: false);
				chunkGameObjectLayer.m_ParentGO.transform.SetParent(null, worldPositionStays: false);
				MemoryPools.poolCGOL.FreeSync(chunkGameObjectLayer);
			}
			MemoryPools.poolVML.FreeSync(vml);
			vml = null;
		}
		if (chunkGameObjectLayer == null)
		{
			chunkGameObjectLayer = MemoryPools.poolCGOL.AllocSync(_bReset: false);
			chunkGameObjectLayer.Init(vml.idx, chunkCluster.LayerMappingTable, base.transform, base.gameObject.isStatic);
			layers[vml.idx] = chunkGameObjectLayer;
		}
		vml.StartCopyMeshes();
		return vml.idx;
	}

	public void EndCopyMeshLayer()
	{
		if (vml != null)
		{
			ChunkGameObjectLayer chunkGameObjectLayer = layers[vml.idx];
			if (chunkGameObjectLayer != null)
			{
				chunkGameObjectLayer.m_ParentGO.SetActive(value: true);
				Occludee.Refresh(chunkGameObjectLayer.m_ParentGO);
			}
			vml.EndCopyMeshes();
			vml = null;
		}
	}

	public bool CreateFromChunkNext(out int _startIdx, out int _endIdx, out int _triangles, out int _colliderTriangles)
	{
		nextMS.ResetAndRestart();
		_startIdx = currentlyCopiedMeshIdx;
		_triangles = 0;
		_colliderTriangles = 0;
		while (currentlyCopiedMeshIdx < MeshDescription.meshes.Length)
		{
			if (!isCopyCollidersThisCall && !chunk.NeedsOnlyCollisionMesh)
			{
				_triangles += copyToMesh(currentlyCopiedMeshIdx);
				isCopyCollidersThisCall = true;
			}
			else
			{
				_colliderTriangles += copyToColliders(currentlyCopiedMeshIdx);
				isCopyCollidersThisCall = false;
				currentlyCopiedMeshIdx++;
			}
			if (!((float)nextMS.ElapsedMilliseconds < 0.5f))
			{
				break;
			}
		}
		_endIdx = currentlyCopiedMeshIdx - 1;
		return currentlyCopiedMeshIdx < MeshDescription.meshes.Length;
	}

	public void CreateMeshAll(out int triangles, out int colliderTriangles)
	{
		triangles = 0;
		colliderTriangles = 0;
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			colliderTriangles += copyToColliders(i);
		}
		if (!chunk.NeedsOnlyCollisionMesh)
		{
			for (int j = 0; j < MeshDescription.meshes.Length; j++)
			{
				triangles += copyToMesh(j);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int copyToMesh(int _meshIdx)
	{
		ChunkGameObjectLayer chunkGameObjectLayer = layers[vml.idx];
		if (chunkGameObjectLayer == null)
		{
			return 0;
		}
		MeshFilter[] array = chunkGameObjectLayer.m_MeshFilter[_meshIdx];
		int num = vml.CopyToMesh(_meshIdx, array, chunkGameObjectLayer.m_MeshRenderer[_meshIdx], 0);
		bool active = num != 0;
		if (!GameManager.bShowPaintables && _meshIdx == 0)
		{
			active = false;
		}
		array[0].gameObject.SetActive(active);
		if (num > 0)
		{
			CheckLODs(_meshIdx);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int copyToColliders(int _meshIdx)
	{
		ChunkGameObjectLayer chunkGameObjectLayer = layers[vml.idx];
		if (chunkGameObjectLayer == null)
		{
			return 0;
		}
		MeshCollider meshCollider = chunkGameObjectLayer.m_MeshCollider[_meshIdx][0];
		if (meshCollider == null)
		{
			return 0;
		}
		Mesh mesh;
		int num = vml.meshes[_meshIdx].CopyToColliders(chunk.ClrIdx, meshCollider, out mesh);
		if (num != 0)
		{
			GameManager.Instance.World.m_ChunkManager.BakeAdd(mesh, meshCollider);
		}
		bool active = num != 0;
		if (!GameManager.bShowPaintables && _meshIdx == 0)
		{
			active = false;
		}
		meshCollider.gameObject.SetActive(active);
		return num;
	}

	public void Cleanup()
	{
		base.gameObject.SetActive(value: false);
		for (int i = 0; i < layers.Length; i++)
		{
			if (layers[i] != null)
			{
				layers[i].Cleanup();
			}
		}
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(base.transform.GetChild(num).gameObject);
		}
		for (int j = 0; j < layers.Length; j++)
		{
			if (layers[j] != null)
			{
				UnityEngine.Object.Destroy(layers[j].m_ParentGO);
			}
		}
		RemoveWallVolumes();
	}

	public void CheckLODs(int _limitToMesh = -1)
	{
		if (chunk == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!primaryPlayer)
		{
			return;
		}
		float num = primaryPlayer.position.x - (float)(chunk.X * 16 + 8);
		float num2 = primaryPlayer.position.z - (float)(chunk.Z * 16 + 8);
		float num3 = num * num + num2 * num2;
		bool flag = GamePrefs.GetBool(EnumGamePrefs.OptionsDisableChunkLODs);
		if (_limitToMesh == -1 || _limitToMesh == 4)
		{
			SetLOD(4, (!flag && !(num3 < 1681f)) ? 1 : 0);
		}
		if (_limitToMesh != -1 && _limitToMesh != 3)
		{
			return;
		}
		float num4 = 48f;
		float num5 = 0f;
		int num6 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxGrassDistance);
		int num7 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		switch (num6)
		{
		case 1:
			num4 = 64f;
			break;
		case 2:
			num4 = 96f;
			break;
		case 3:
			num4 = 112f;
			if (num7 >= 3)
			{
				num5 = ((num7 == 3) ? 2.3f : 3.6f) * 16f;
			}
			break;
		}
		bool flag2 = num3 < num4 * num4;
		bool flag3 = num3 < num5 * num5;
		for (int i = 0; i < layers.Length; i++)
		{
			ChunkGameObjectLayer chunkGameObjectLayer = layers[i];
			if (chunkGameObjectLayer == null)
			{
				continue;
			}
			GameObject gameObject = chunkGameObjectLayer.m_MeshesGO[3];
			if (!gameObject)
			{
				continue;
			}
			if (flag2)
			{
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(value: true);
				}
				if (flag3)
				{
					if (!chunkGameObjectLayer.isGrassCastShadows)
					{
						chunkGameObjectLayer.isGrassCastShadows = true;
						chunkGameObjectLayer.m_MeshRenderer[3][0].shadowCastingMode = ShadowCastingMode.On;
					}
				}
				else if (chunkGameObjectLayer.isGrassCastShadows)
				{
					chunkGameObjectLayer.isGrassCastShadows = false;
					chunkGameObjectLayer.m_MeshRenderer[3][0].shadowCastingMode = ShadowCastingMode.Off;
				}
			}
			else if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLOD(int _meshIdx, int _lodLevel)
	{
		if (_lodLevel == 0)
		{
			for (int i = 0; i < layers.Length; i++)
			{
				ChunkGameObjectLayer chunkGameObjectLayer = layers[i];
				if (chunkGameObjectLayer != null)
				{
					GameObject gameObject = chunkGameObjectLayer.m_MeshesGO[_meshIdx];
					if ((bool)gameObject && !gameObject.activeSelf)
					{
						gameObject.SetActive(value: true);
					}
				}
			}
			return;
		}
		for (int j = 0; j < layers.Length; j++)
		{
			ChunkGameObjectLayer chunkGameObjectLayer2 = layers[j];
			if (chunkGameObjectLayer2 != null)
			{
				GameObject gameObject2 = chunkGameObjectLayer2.m_MeshesGO[_meshIdx];
				if ((bool)gameObject2 && gameObject2.activeSelf)
				{
					gameObject2.SetActive(value: false);
				}
			}
		}
	}

	public ChunkGameObjectLayer GetLayer(int _layer)
	{
		return layers[_layer];
	}
}
