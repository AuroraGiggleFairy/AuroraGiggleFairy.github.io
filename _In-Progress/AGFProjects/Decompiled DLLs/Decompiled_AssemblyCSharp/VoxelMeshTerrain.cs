using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VoxelMeshTerrain : VoxelMesh
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isInitStatic;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] mainTexPropertyIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] mainTexPropertyNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] bumpMapPropertyIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] bumpMapPropertyNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] sideTexPropertyIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] sideTexPropertyNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] sideBumpMapPropertyIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] sideBumpMapPropertyNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MicroSplatPropData msPropData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D msPropTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MicroSplatProceduralTextureConfig msProcData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D msProcCurveTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D msProcParamTex;

	public bool IsPreviewVoxelMesh;

	public List<TerrainSubMesh> submeshes = new List<TerrainSubMesh>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColSplatMap = new Color(0f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColUnderTerrain4to10 = new Color(0f, 0f, 0f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColUnderTerrain1 = new Color(1f, 0f, 0f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColUnderTerrain2 = new Color(0f, 1f, 0f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColUnderTerrain3 = new Color(0f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 cUvEmpty = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 cUvUnderTerrain1_0 = new Vector2(1f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 cUvUnderTerrain0_1 = new Vector2(0f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] texIds = new int[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitStatic()
	{
		mainTexPropertyIds = new int[3];
		mainTexPropertyNames = new string[3];
		bumpMapPropertyIds = new int[3];
		bumpMapPropertyNames = new string[3];
		sideTexPropertyIds = new int[3];
		sideTexPropertyNames = new string[3];
		sideBumpMapPropertyIds = new int[3];
		sideBumpMapPropertyNames = new string[3];
		for (int i = 0; i < mainTexPropertyIds.Length; i++)
		{
			mainTexPropertyNames[i] = "_MainTex" + ((i > 0) ? (i + 1).ToString() : "");
			mainTexPropertyIds[i] = Shader.PropertyToID(mainTexPropertyNames[i]);
			bumpMapPropertyNames[i] = "_BumpMap" + ((i > 0) ? (i + 1).ToString() : "");
			bumpMapPropertyIds[i] = Shader.PropertyToID(bumpMapPropertyNames[i]);
			sideTexPropertyNames[i] = "_SideTex" + ((i > 0) ? (i + 1).ToString() : "");
			sideTexPropertyIds[i] = Shader.PropertyToID(sideTexPropertyNames[i]);
			sideBumpMapPropertyNames[i] = "_SideBumpMap" + ((i > 0) ? (i + 1).ToString() : "");
			sideBumpMapPropertyIds[i] = Shader.PropertyToID(sideBumpMapPropertyNames[i]);
		}
		InitMicroSplat();
		isInitStatic = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void InitMicroSplat()
	{
		msPropData = LoadManager.LoadAssetFromAddressables<MicroSplatPropData>("TerrainTextures", "Microsplat/MicroSplatTerrainInGame_propdata.asset", null, null, _deferLoading: false, _loadSync: true).Asset;
		msPropTex = msPropData.GetTexture();
		msProcData = LoadManager.LoadAssetFromAddressables<MicroSplatProceduralTextureConfig>("TerrainTextures", "Microsplat/MicroSplatTerrainInGame_proceduraltexture.asset", null, null, _deferLoading: false, _loadSync: true).Asset;
		msProcCurveTex = msProcData.GetCurveTexture();
		msProcParamTex = msProcData.GetParamTexture();
	}

	public VoxelMeshTerrain(int _meshIndex, int _minSize = 500)
		: base(_meshIndex, _minSize)
	{
		m_Uvs3 = new ArrayListMP<Vector2>(MemoryPools.poolVector2, _minSize);
		m_Uvs4 = new ArrayListMP<Vector2>(MemoryPools.poolVector2, _minSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int EncodeTexIds(int _mainTexId, int _sideTexId)
	{
		return (_mainTexId << 16) | _sideTexId;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int DecodeMainTexId(int _fullTexId)
	{
		return _fullTexId >> 16;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int DecodeSideTexId(int _fullTexId)
	{
		return _fullTexId & 0xFFFF;
	}

	public override Color[] UpdateColors(byte _suncolor, byte _blockcolor)
	{
		float b = (float)(int)_suncolor / 15f;
		float a = (float)(int)_blockcolor / 15f;
		for (int i = 0; i < m_ColorVertices.Count; i++)
		{
			Color value = m_ColorVertices[i];
			value.b = b;
			value.a = a;
			m_ColorVertices[i] = value;
		}
		return m_ColorVertices.ToArray();
	}

	public override void GetColorForTextureId(int _subMeshIdx, ref Transvoxel.BuildVertex _data)
	{
		if (!World.IsSplatMapAvailable || IsPreviewVoxelMesh)
		{
			_data.uv = (_data.uv2 = (_data.uv3 = (_data.uv4 = cUvEmpty)));
			_data.color = submeshes[_subMeshIdx].GetColorForTextureId(_data.texture);
		}
		else
		{
			int texId = DecodeMainTexId(_data.texture);
			GetColorForTexId(texId, ref _data);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetColorForTexId(int _texId, ref Transvoxel.BuildVertex _data)
	{
		_data.uv = (_data.uv2 = (_data.uv3 = (_data.uv4 = cUvEmpty)));
		if (_data.bTopSoil)
		{
			_data.color = cColSplatMap;
			return;
		}
		_data.color = cColUnderTerrain4to10;
		switch (_texId)
		{
		default:
			_data.color = cColSplatMap;
			break;
		case 2:
			_data.color = cColUnderTerrain1;
			break;
		case 11:
			_data.color = cColUnderTerrain2;
			break;
		case 34:
			_data.color = cColUnderTerrain3;
			break;
		case 10:
			_data.uv = cUvUnderTerrain1_0;
			break;
		case 33:
			_data.uv = cUvUnderTerrain0_1;
			break;
		case 300:
			_data.uv2 = cUvUnderTerrain1_0;
			break;
		case 1:
			_data.uv2 = cUvUnderTerrain0_1;
			break;
		case 184:
			_data.uv3 = cUvUnderTerrain1_0;
			break;
		case 440:
			_data.uv3 = cUvUnderTerrain0_1;
			break;
		case 316:
			_data.uv4 = cUvUnderTerrain1_0;
			break;
		case 438:
			_data.uv4 = cUvUnderTerrain0_1;
			break;
		}
	}

	public override int FindOrCreateSubMesh(int _t0, int _t1, int _t2)
	{
		texIds[0] = _t0;
		texIds[1] = ((_t1 != _t0) ? _t1 : (-1));
		texIds[2] = ((_t2 != _t0 && _t2 != _t1) ? _t2 : (-1));
		for (int i = 0; i < submeshes.Count; i++)
		{
			if (submeshes[i].Contains(texIds))
			{
				return i;
			}
		}
		for (int j = 0; j < submeshes.Count; j++)
		{
			if (submeshes[j].CanAdd(texIds))
			{
				return j;
			}
		}
		TerrainSubMesh item = new TerrainSubMesh(submeshes, 4096);
		item.Add(texIds);
		submeshes.Add(item);
		return submeshes.Count - 1;
	}

	public override void AddIndices(int _i0, int _i1, int _i2, int _submesh)
	{
		ArrayListMP<int> triangles = submeshes[_submesh].triangles;
		int num = triangles.Alloc(3);
		triangles.Items[num] = _i0;
		triangles.Items[num + 1] = _i1;
		triangles.Items[num + 2] = _i2;
	}

	public override void ClearMesh()
	{
		base.ClearMesh();
		submeshes.Clear();
	}

	public override void Finished()
	{
		m_Triangles = 0;
		for (int i = 0; i < submeshes.Count; i++)
		{
			m_Triangles += submeshes[i].triangles.Count / 3;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConfigureTerrainMaterial(Material mat, ChunkProviderGenerateWorldFromRaw cpr)
	{
		mat.SetTexture("_CustomControl0", cpr.splats[0]);
		mat.SetTexture("_CustomControl1", cpr.splats[1]);
		if ((bool)msPropTex)
		{
			mat.SetTexture("_PerTexProps", msPropTex);
		}
		if ((bool)msProcData)
		{
			mat.SetTexture("_ProcTexCurves", msProcCurveTex);
			mat.SetTexture("_ProcTexParams", msProcParamTex);
			mat.SetInt("_PCLayerCount", msProcData.layers.Count);
			if (cpr.procBiomeMask1 != null && mat.HasProperty("_ProcTexBiomeMask"))
			{
				mat.SetTexture("_ProcTexBiomeMask", cpr.procBiomeMask1);
			}
			if (cpr.procBiomeMask2 != null && mat.HasProperty("_ProcTexBiomeMask2"))
			{
				mat.SetTexture("_ProcTexBiomeMask2", cpr.procBiomeMask2);
			}
		}
		Vector2i worldSize = cpr.GetWorldSize();
		mat.SetVector("_WorldDim", new Vector4(worldSize.x, worldSize.y));
	}

	public void ApplyMaterials(MeshRenderer _mr, TextureAtlasTerrain _ta, float _tilingFac, bool _bDistant = false)
	{
		if (!isInitStatic)
		{
			InitStatic();
		}
		if (World.IsSplatMapAvailable && !IsPreviewVoxelMesh)
		{
			MeshDescription meshDescription = MeshDescription.meshes[5];
			if (GameManager.Instance != null && GameManager.Instance.World != null && GameManager.Instance.World.ChunkCache.ChunkProvider is ChunkProviderGenerateWorldFromRaw cpr)
			{
				ConfigureTerrainMaterial(meshDescription.material, cpr);
				ConfigureTerrainMaterial(meshDescription.materialDistant, cpr);
			}
			Material[] array = _mr.sharedMaterials;
			if (submeshes.Count != array.Length)
			{
				array = new Material[submeshes.Count];
			}
			for (int i = 0; i < submeshes.Count; i++)
			{
				if (DecodeMainTexId(submeshes[i].textureIds.Data[0]) >= 5000)
				{
					array[i] = new Material(_bDistant ? MeshDescription.meshes[1].materialDistant : MeshDescription.meshes[1].material);
				}
				else
				{
					array[i] = meshDescription.material;
				}
			}
			_mr.sharedMaterials = array;
			return;
		}
		Material[] array2 = _mr.sharedMaterials;
		Utils.CleanupMaterials(array2);
		if (submeshes.Count != array2.Length)
		{
			array2 = new Material[submeshes.Count];
		}
		for (int j = 0; j < submeshes.Count; j++)
		{
			if (DecodeMainTexId(submeshes[j].textureIds.Data[0]) >= 5000)
			{
				array2[j] = new Material(_bDistant ? MeshDescription.meshes[1].materialDistant : MeshDescription.meshes[1].material);
			}
			else if (IsPreviewVoxelMesh && World.IsSplatMapAvailable)
			{
				array2[j] = new Material(_bDistant ? MeshDescription.meshes[5].prefabTerrainMaterialDistant : MeshDescription.meshes[5].prefabPreviewMaterial);
			}
			else
			{
				array2[j] = new Material(_bDistant ? MeshDescription.meshes[5].materialDistant : MeshDescription.meshes[5].material);
			}
			Utils.MarkMaterialAsSafeForManualCleanup(array2[j]);
		}
		for (int k = 0; k < submeshes.Count; k++)
		{
			for (int l = 0; l < submeshes[k].textureIds.Size; l++)
			{
				if (!submeshes[k].textureIds.DataAvail[l])
				{
					continue;
				}
				int fullTexId = submeshes[k].textureIds.Data[l];
				int num = DecodeMainTexId(fullTexId);
				int num2 = DecodeSideTexId(fullTexId);
				if (num < 5000)
				{
					if (num < 0 || num >= _ta.uvMapping.Length)
					{
						Log.Error($"Error in terrain mesh generation, texture id {num} not found");
						return;
					}
					Vector2 value = new Vector2(1f / ((float)_ta.uvMapping[num].blockW * _tilingFac), 1f / ((float)_ta.uvMapping[num].blockH * _tilingFac));
					array2[k].SetTexture(mainTexPropertyIds[l], _ta.diffuse[num]);
					array2[k].SetTextureScale(mainTexPropertyNames[l], value);
					array2[k].SetTexture(bumpMapPropertyIds[l], _ta.normal[num]);
					array2[k].SetTextureScale(bumpMapPropertyNames[l], value);
					value = new Vector2(1f / ((float)_ta.uvMapping[num2].blockW * _tilingFac), 1f / ((float)_ta.uvMapping[num2].blockH * _tilingFac));
					array2[k].SetTexture(sideTexPropertyIds[l], _ta.diffuse[num2]);
					array2[k].SetTextureScale(sideTexPropertyNames[l], value);
					array2[k].SetTexture(sideBumpMapPropertyIds[l], _ta.normal[num2]);
					array2[k].SetTextureScale(sideBumpMapPropertyNames[l], value);
				}
			}
		}
		_mr.sharedMaterials = array2;
	}

	public override int CopyToMesh(MeshFilter[] _mf, MeshRenderer[] _mr, int _lodLevel, Action _onCopyComplete = null)
	{
		MeshFilter meshFilter = _mf[0];
		Mesh mesh = meshFilter.sharedMesh;
		int count = m_Vertices.Count;
		if (count == 0)
		{
			if ((bool)mesh)
			{
				meshFilter.sharedMesh = null;
				VoxelMesh.AddPooledMesh(mesh);
			}
			_onCopyComplete?.Invoke();
			return 0;
		}
		if (!mesh)
		{
			mesh = (meshFilter.sharedMesh = VoxelMesh.GetPooledMesh());
		}
		else
		{
			mesh.Clear(keepVertexLayout: false);
		}
		MeshRenderer mr = _mr[0];
		TextureAtlasTerrain ta = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
		ApplyMaterials(mr, ta, 1f);
		if (count != m_ColorVertices.Count)
		{
			Log.Error("ERROR: VMT.mesh[{2}].Vertices.Count ({0}) != VMT.mesh[{2}].ColorSides.Count ({1})", count, m_ColorVertices.Count, meshIndex);
		}
		if (count != m_Uvs.Count)
		{
			Log.Error("ERROR: VMT.mesh[{2}].Vertices.Count ({0}) != VMT.mesh[{2}].Uvs.Count ({1})", count, m_Uvs.Count, meshIndex);
		}
		MeshUnsafeCopyHelper.CopyVertices(m_Vertices, mesh);
		MeshUnsafeCopyHelper.CopyUV(m_Uvs, mesh);
		if (UvsCrack != null)
		{
			MeshUnsafeCopyHelper.CopyUV2(UvsCrack, mesh);
		}
		if (m_Uvs3 != null && m_Uvs3.Items != null)
		{
			MeshUnsafeCopyHelper.CopyUV3(m_Uvs3, mesh);
		}
		if (m_Uvs4 != null && m_Uvs4.Items != null)
		{
			MeshUnsafeCopyHelper.CopyUV4(m_Uvs4, mesh);
		}
		MeshUnsafeCopyHelper.CopyColors(m_ColorVertices, mesh);
		mesh.subMeshCount = submeshes.Count;
		for (int i = 0; i < submeshes.Count; i++)
		{
			MeshUnsafeCopyHelper.CopyTriangles(submeshes[i].triangles, mesh, i);
		}
		if (m_Normals.Count == 0)
		{
			mesh.RecalculateNormals();
		}
		else
		{
			if (count != m_Normals.Count)
			{
				Log.Error("ERROR: Vertices.Count ({0}) != Normals.Count ({1})", count, m_Normals.Count, CurTriangleIndex);
			}
			MeshUnsafeCopyHelper.CopyNormals(m_Normals, mesh);
		}
		mesh.RecalculateTangents();
		mesh.RecalculateUVDistributionMetrics();
		GameUtils.SetMeshVertexAttributes(mesh, compressPosition: false);
		mesh.UploadMeshData(markNoLongerReadable: false);
		_onCopyComplete?.Invoke();
		return m_Triangles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string MakeTransformPathName(Transform _t, int _parentMax = 1)
	{
		string text = _t.name;
		for (int i = 0; i < _parentMax; i++)
		{
			_t = _t.parent;
			if (!_t)
			{
				break;
			}
			text = _t.name + "," + text;
		}
		return text;
	}

	public override int CopyToColliders(int _clrIdx, MeshCollider _meshCollider, out Mesh mesh)
	{
		if (m_Vertices.Count == 0)
		{
			GameManager.Instance.World.m_ChunkManager.BakeDestroyCancel(_meshCollider);
			mesh = null;
			return 0;
		}
		mesh = ResetMesh(_meshCollider);
		int num = 0;
		mesh.subMeshCount = submeshes.Count;
		MeshUnsafeCopyHelper.CopyVertices(m_Vertices, mesh);
		for (int i = 0; i < submeshes.Count; i++)
		{
			MeshUnsafeCopyHelper.CopyTriangles(submeshes[i].triangles, mesh, i);
			num += submeshes[i].triangles.Count / 3;
		}
		return num;
	}
}
