using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGameObjectLayer : IMemoryPoolableObject
{
	public GameObject m_ParentGO;

	public MeshFilter[] m_MeshFilter;

	public MeshRenderer[] m_MeshRenderer;

	public MeshCollider[] m_MeshCollider;

	public GameObject[] m_MeshesGO;

	public bool isGrassCastShadows;

	public static int InstanceCount;

	public ChunkGameObjectLayer()
	{
		int num = MeshDescription.meshes.Length;
		m_MeshFilter = new MeshFilter[num];
		m_MeshRenderer = new MeshRenderer[num];
		m_MeshCollider = new MeshCollider[num];
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

	public void Init(int _chunkLayerIdx, IReadOnlyDictionary<string, int> _layerMappingTable, Transform _chunkT, bool _bStatic)
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
			MeshCollider meshCollider = m_MeshCollider[i];
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
		MeshFilter[] meshFilter = m_MeshFilter;
		foreach (MeshFilter meshFilter2 in meshFilter)
		{
			if ((bool)meshFilter2)
			{
				Mesh sharedMesh = meshFilter2.sharedMesh;
				if ((bool)sharedMesh)
				{
					meshFilter2.sharedMesh = null;
					VoxelMesh.AddPooledMesh(sharedMesh);
				}
			}
		}
		MeshCollider[] meshCollider = m_MeshCollider;
		foreach (MeshCollider meshCollider2 in meshCollider)
		{
			if ((bool)meshCollider2)
			{
				Mesh sharedMesh2 = meshCollider2.sharedMesh;
				if ((bool)sharedMesh2)
				{
					meshCollider2.sharedMesh = null;
					UnityEngine.Object.Destroy(sharedMesh2);
				}
			}
		}
	}

	public void Cleanup()
	{
		Span<MeshFilter> span = m_MeshFilter.AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref MeshFilter reference = ref span[i];
			if ((bool)reference)
			{
				Mesh sharedMesh = reference.sharedMesh;
				if ((bool)sharedMesh)
				{
					reference.sharedMesh = null;
					UnityEngine.Object.Destroy(sharedMesh);
				}
				GameUtils.NullThenDestroy(ref reference);
			}
		}
		Span<MeshRenderer> span2 = m_MeshRenderer.AsSpan();
		for (int i = 0; i < span2.Length; i++)
		{
			ref MeshRenderer reference2 = ref span2[i];
			if ((bool)reference2)
			{
				GameUtils.NullThenDestroy(ref reference2);
			}
		}
		Span<MeshCollider> span3 = m_MeshCollider.AsSpan();
		for (int i = 0; i < span3.Length; i++)
		{
			ref MeshCollider reference3 = ref span3[i];
			if ((bool)reference3)
			{
				Mesh sharedMesh2 = reference3.sharedMesh;
				if ((bool)sharedMesh2)
				{
					reference3.sharedMesh = null;
					UnityEngine.Object.Destroy(sharedMesh2);
				}
				GameUtils.NullThenDestroy(ref reference3);
			}
		}
		Span<GameObject> span4 = m_MeshesGO.AsSpan();
		for (int i = 0; i < span4.Length; i++)
		{
			ref GameObject reference4 = ref span4[i];
			if ((bool)reference4)
			{
				GameUtils.NullThenDestroy(ref reference4);
			}
		}
		GameUtils.NullThenDestroy(ref m_ParentGO);
	}
}
