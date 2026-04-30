using System.Collections.Generic;
using UnityEngine;

public class ChunkGameObjectLayer : IMemoryPoolableObject
{
	public GameObject m_ParentGO;

	public MeshFilter[][] m_MeshFilter;

	public MeshRenderer[][] m_MeshRenderer;

	public MeshCollider[][] m_MeshCollider;

	public GameObject[] m_MeshesGO;

	public bool isGrassCastShadows;

	public static int InstanceCount;

	public ChunkGameObjectLayer()
	{
		int num = MeshDescription.meshes.Length;
		m_MeshFilter = new MeshFilter[num][];
		m_MeshRenderer = new MeshRenderer[num][];
		m_MeshCollider = new MeshCollider[num][];
		m_MeshesGO = new GameObject[num];
		m_ParentGO = new GameObject("CLayer");
		Transform transform = m_ParentGO.transform;
		for (int i = 0; i < num; i++)
		{
			MeshDescription meshDescription = MeshDescription.meshes[i];
			GameObject gameObject = new GameObject(meshDescription.Name);
			m_MeshesGO[i] = gameObject;
			gameObject.transform.SetParent(transform, worldPositionStays: false);
			VoxelMesh.CreateMeshFilter(i, 0, gameObject, meshDescription.Tag, _bAllowLOD: true, out m_MeshFilter[i], out m_MeshRenderer[i], out m_MeshCollider[i]);
		}
		if (OcclusionManager.Instance.cullChunkLayers)
		{
			Occludee.Add(m_ParentGO);
		}
		m_ParentGO.SetActive(value: false);
	}

	public void Init(int _chunkLayerIdx, Dictionary<string, int> _layerMappingTable, Transform _chunkT, bool _bStatic)
	{
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			MeshDescription meshDescription = MeshDescription.meshes[i];
			GameObject gameObject = m_MeshesGO[i];
			gameObject.isStatic = _bStatic;
			if (!string.IsNullOrEmpty(meshDescription.MeshLayerName))
			{
				gameObject.layer = _layerMappingTable[meshDescription.MeshLayerName];
			}
			else
			{
				gameObject.layer = 0;
			}
			MeshCollider meshCollider = m_MeshCollider[i][0];
			if ((bool)meshCollider)
			{
				if ((bool)meshCollider.sharedMesh)
				{
					Log.Warning("ChunkGameObjectLayer Init collider '{0}' should be null", meshCollider.sharedMesh.name);
				}
				GameObject gameObject2 = meshCollider.gameObject;
				gameObject2.isStatic = _bStatic;
				gameObject2.layer = _layerMappingTable[meshDescription.ColliderLayerName];
			}
		}
		m_ParentGO.name = "CLayer" + _chunkLayerIdx.ToString("00");
		m_ParentGO.transform.SetParent(_chunkT, worldPositionStays: false);
	}

	public void Reset()
	{
		int num = m_MeshFilter.Length;
		for (int i = 0; i < num; i++)
		{
			MeshFilter[] array = m_MeshFilter[i];
			foreach (MeshFilter meshFilter in array)
			{
				if ((bool)meshFilter)
				{
					Mesh sharedMesh = meshFilter.sharedMesh;
					if ((bool)sharedMesh)
					{
						meshFilter.sharedMesh = null;
						VoxelMesh.AddPooledMesh(sharedMesh);
					}
				}
			}
			MeshCollider meshCollider = m_MeshCollider[i][0];
			if ((bool)meshCollider)
			{
				Mesh sharedMesh2 = meshCollider.sharedMesh;
				if ((bool)sharedMesh2)
				{
					meshCollider.sharedMesh = null;
					Object.Destroy(sharedMesh2);
				}
			}
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < m_MeshFilter.Length; i++)
		{
			for (int j = 0; j < m_MeshFilter[i].Length; j++)
			{
				MeshFilter meshFilter = m_MeshFilter[i][j];
				if ((bool)meshFilter)
				{
					Mesh sharedMesh = meshFilter.sharedMesh;
					if ((bool)sharedMesh)
					{
						meshFilter.sharedMesh = null;
						Object.Destroy(sharedMesh);
					}
				}
			}
		}
		for (int k = 0; k < m_MeshRenderer.Length; k++)
		{
			MeshRenderer[] array = m_MeshRenderer[k];
			for (int l = 0; l < array.Length; l++)
			{
				MeshRenderer meshRenderer = array[l];
				if ((bool)meshRenderer)
				{
					Object.Destroy(meshRenderer);
					array[l] = null;
				}
			}
		}
		for (int m = 0; m < m_MeshCollider.Length; m++)
		{
			if (m_MeshCollider[m][0] != null && m_MeshCollider[m][0].sharedMesh != null)
			{
				m_MeshCollider[m][0].sharedMesh.Clear(keepVertexLayout: false);
				Object.Destroy(m_MeshCollider[m][0].sharedMesh);
			}
		}
		for (int n = 0; n < m_MeshesGO.Length; n++)
		{
			if (m_MeshesGO[n] != null)
			{
				Object.Destroy(m_MeshesGO[n]);
			}
		}
		Object.Destroy(m_ParentGO);
	}
}
