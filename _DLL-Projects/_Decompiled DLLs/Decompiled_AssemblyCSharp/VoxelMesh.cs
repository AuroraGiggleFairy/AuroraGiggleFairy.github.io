using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class VoxelMesh
{
	public enum EnumMeshType
	{
		Blocks,
		Models,
		Terrain,
		Decals
	}

	public enum CreateFlags
	{
		None,
		Collider,
		Cracks,
		Default
	}

	public static float COLOR_SOUTH = 0.9f;

	public static float COLOR_WEST = 0.8f;

	public static float COLOR_NORTH = 0.7f;

	public static float COLOR_EAST = 0.85f;

	public static float COLOR_TOP = 1f;

	public static float COLOR_BOTTOM = 0.65f;

	public ArrayListMP<Vector3> m_Vertices;

	public ArrayListMP<int> m_Indices;

	public ArrayListMP<Vector2> m_Uvs;

	public ArrayListMP<Vector2> UvsCrack;

	public ArrayListMP<Vector2> m_Uvs3;

	public ArrayListMP<Vector2> m_Uvs4;

	public ArrayListMP<Vector3> m_Normals;

	public ArrayListMP<Vector4> m_Tangents;

	public ArrayListMP<Color> m_ColorVertices;

	public ArrayListMP<Vector3> m_CollVertices;

	public ArrayListMP<int> m_CollIndices;

	public int CurTriangleIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_Triangles;

	public int meshIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTemperatureDefault = -10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float temperature;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPoolMeshMax = 250;

	public static List<Mesh> meshPool = new List<Mesh>();

	public ArrayListMP<int> Indices
	{
		get
		{
			return m_Indices;
		}
		set
		{
			m_Indices = value;
		}
	}

	public ArrayListMP<Vector2> Uvs
	{
		get
		{
			return m_Uvs;
		}
		set
		{
			m_Uvs = value;
		}
	}

	public ArrayListMP<Vector2> Uvs3
	{
		get
		{
			return m_Uvs3;
		}
		set
		{
			m_Uvs3 = value;
		}
	}

	public ArrayListMP<Vector2> Uvs4
	{
		get
		{
			return m_Uvs4;
		}
		set
		{
			m_Uvs4 = value;
		}
	}

	public ArrayListMP<Vector3> Vertices
	{
		get
		{
			return m_Vertices;
		}
		set
		{
			m_Vertices = value;
		}
	}

	public ArrayListMP<Vector3> Normals
	{
		get
		{
			return m_Normals;
		}
		set
		{
			m_Normals = value;
		}
	}

	public ArrayListMP<Vector4> Tangents
	{
		get
		{
			return m_Tangents;
		}
		set
		{
			m_Tangents = value;
		}
	}

	public ArrayListMP<Color> ColorVertices
	{
		get
		{
			return m_ColorVertices;
		}
		set
		{
			m_ColorVertices = value;
		}
	}

	public ArrayListMP<int> CollIndices => m_CollIndices;

	public ArrayListMP<Vector3> CollVertices => m_CollVertices;

	public int Size
	{
		get
		{
			int num = m_Indices.Count * 4 + m_Uvs.Count * 8 + m_Vertices.Count * 12 + m_ColorVertices.Count * 16 + m_Normals.Count * 12 + m_Tangents.Count * 16;
			if (UvsCrack != null)
			{
				num += UvsCrack.Count * 8;
			}
			return num;
		}
	}

	public int Triangles => m_Triangles;

	public VoxelMesh(int _meshIndex, int _minSize = 1024, CreateFlags _flags = CreateFlags.Default)
	{
		meshIndex = _meshIndex;
		m_Vertices = new ArrayListMP<Vector3>(MemoryPools.poolVector3, _minSize);
		m_Indices = new ArrayListMP<int>(MemoryPools.poolInt, _minSize);
		m_Uvs = new ArrayListMP<Vector2>(MemoryPools.poolVector2, _minSize);
		if ((_flags & CreateFlags.Cracks) > CreateFlags.None)
		{
			UvsCrack = new ArrayListMP<Vector2>(MemoryPools.poolVector2, _minSize);
		}
		m_Normals = new ArrayListMP<Vector3>(MemoryPools.poolVector3, _minSize);
		m_Tangents = new ArrayListMP<Vector4>(MemoryPools.poolVector4, _minSize);
		m_ColorVertices = new ArrayListMP<Color>(MemoryPools.poolColor, _minSize);
		if ((_flags & CreateFlags.Collider) > CreateFlags.None)
		{
			m_CollVertices = new ArrayListMP<Vector3>(MemoryPools.poolVector3, _minSize);
			m_CollIndices = new ArrayListMP<int>(MemoryPools.poolInt, _minSize);
		}
	}

	public static VoxelMesh Create(int _meshIdx, EnumMeshType _meshType, int _minSize = 500)
	{
		return _meshType switch
		{
			EnumMeshType.Terrain => new VoxelMeshTerrain(_meshIdx, _minSize), 
			EnumMeshType.Decals => new VoxelMesh(_meshIdx, _minSize, CreateFlags.None), 
			_ => new VoxelMesh(_meshIdx, _minSize), 
		};
	}

	public void SetTemperature(float _temperature)
	{
		temperature = _temperature;
	}

	public virtual void Write(BinaryWriter _bw)
	{
		_bw.Write((uint)m_Vertices.Count);
		for (int i = 0; i < m_Vertices.Count; i++)
		{
			_bw.Write(m_Vertices[i].x);
			_bw.Write(m_Vertices[i].y);
			_bw.Write(m_Vertices[i].z);
		}
		_bw.Write((uint)m_Normals.Count);
		for (int j = 0; j < m_Normals.Count; j++)
		{
			_bw.Write(m_Normals[j].x);
			_bw.Write(m_Normals[j].y);
			_bw.Write(m_Normals[j].z);
		}
		_bw.Write((uint)m_Uvs.Count);
		for (int k = 0; k < m_Uvs.Count; k++)
		{
			_bw.Write(m_Uvs[k].x);
			_bw.Write(m_Uvs[k].y);
		}
		int num = UvsCrack?.Count ?? 0;
		_bw.Write((uint)num);
		for (int l = 0; l < num; l++)
		{
			_bw.Write(UvsCrack[l].x);
			_bw.Write(UvsCrack[l].y);
		}
		_bw.Write((uint)m_ColorVertices.Count);
		for (int m = 0; m < m_ColorVertices.Count; m++)
		{
			_bw.Write(m_ColorVertices[m].r);
			_bw.Write(m_ColorVertices[m].g);
			_bw.Write(m_ColorVertices[m].b);
			_bw.Write(m_ColorVertices[m].a);
		}
		_bw.Write((uint)m_Indices.Count);
		for (int n = 0; n < m_Indices.Count; n++)
		{
			_bw.Write(m_Indices[n]);
		}
	}

	public virtual void Read(BinaryReader _br)
	{
		uint num = _br.ReadUInt32();
		m_Vertices.Clear();
		int num2 = m_Vertices.Alloc((int)num);
		for (int i = 0; i < num; i++)
		{
			m_Vertices[num2++] = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		}
		num = _br.ReadUInt32();
		m_Normals.Clear();
		num2 = m_Normals.Alloc((int)num);
		for (int j = 0; j < num; j++)
		{
			m_Normals[num2++] = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		}
		num = _br.ReadUInt32();
		m_Uvs.Clear();
		num2 = m_Uvs.Alloc((int)num);
		for (int k = 0; k < num; k++)
		{
			m_Uvs[num2++] = new Vector2(_br.ReadSingle(), _br.ReadSingle());
		}
		num = _br.ReadUInt32();
		UvsCrack?.Clear();
		if (num != 0)
		{
			num2 = UvsCrack.Alloc((int)num);
			for (int l = 0; l < num; l++)
			{
				UvsCrack[num2++] = new Vector2(_br.ReadSingle(), _br.ReadSingle());
			}
		}
		num = _br.ReadUInt32();
		m_ColorVertices.Clear();
		num2 = m_ColorVertices.Alloc((int)num);
		for (int m = 0; m < num; m++)
		{
			m_ColorVertices[num2++] = new Color(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		}
		num = _br.ReadUInt32();
		m_Indices.Clear();
		num2 = m_Indices.Alloc((int)num);
		for (int n = 0; n < num; n++)
		{
			m_Indices[num2++] = _br.ReadInt32();
		}
		m_Tangents.Clear();
	}

	public static float BlockFaceToColor(BlockFace _blockFace)
	{
		return _blockFace switch
		{
			BlockFace.Top => COLOR_TOP, 
			BlockFace.Bottom => COLOR_BOTTOM, 
			BlockFace.North => COLOR_NORTH, 
			BlockFace.South => COLOR_SOUTH, 
			BlockFace.East => COLOR_EAST, 
			BlockFace.West => COLOR_WEST, 
			_ => 1f, 
		};
	}

	public static void CreateMeshFilter(int _meshIndex, int _yOffset, GameObject _gameObject, string _meshTag, bool _bAllowLOD, out MeshFilter[] _mf, out MeshRenderer[] _mr)
	{
		_mf = new MeshFilter[1];
		_mr = new MeshRenderer[1];
		CreateMeshFilter(_meshIndex, _yOffset, _gameObject, _meshTag, _bAllowLOD, out _mf[0], out _mr[0]);
	}

	public static void CreateMeshFilter(int _meshIndex, int _yOffset, GameObject _gameObject, string _meshTag, bool _bAllowLOD, out MeshFilter[] _mf, out MeshRenderer[] _mr, out MeshCollider[] _mc)
	{
		_mf = new MeshFilter[1];
		_mr = new MeshRenderer[1];
		CreateMeshFilter(_meshIndex, _yOffset, _gameObject, _meshTag, _bAllowLOD, out _mf[0], out _mr[0]);
		_mc = new MeshCollider[1];
		CreateMeshCollider(_meshIndex, _gameObject, _meshTag, ref _mc[0]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateMeshFilter(int _meshIndex, int _yOffset, GameObject _gameObject, string _meshTag, bool _bAllowLOD, out MeshFilter _mf, out MeshRenderer _mr)
	{
		_mf = null;
		_mr = null;
		MeshDescription meshDescription = MeshDescription.meshes[_meshIndex];
		_gameObject.transform.localPosition = Vector3.zero;
		if (!string.IsNullOrEmpty(_meshTag))
		{
			_gameObject.tag = _meshTag;
		}
		if (meshDescription.materials == null)
		{
			return;
		}
		_mf = _gameObject.GetComponent<MeshFilter>();
		if (_mf == null)
		{
			_mf = _gameObject.AddComponent<MeshFilter>();
		}
		_mr = _gameObject.GetComponent<MeshRenderer>();
		if (_mr == null)
		{
			_mr = _gameObject.AddComponent<MeshRenderer>();
		}
		if (_meshIndex != 5)
		{
			if (meshDescription.materials.Length > 1)
			{
				_mr.sharedMaterials = meshDescription.materials;
			}
			else
			{
				_mr.sharedMaterial = meshDescription.materials[0];
			}
		}
		_mr.receiveShadows = meshDescription.bReceiveShadows;
		_mr.shadowCastingMode = (meshDescription.bCastShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateMeshCollider(int _meshIndex, GameObject _gameObject, string _meshTag, ref MeshCollider _mc)
	{
		MeshDescription meshDescription = MeshDescription.meshes[_meshIndex];
		if (meshDescription.ColliderLayerName != null && meshDescription.ColliderLayerName.Length > 0)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = _gameObject.name + "Collider";
			if (!string.IsNullOrEmpty(_meshTag))
			{
				gameObject.tag = _meshTag;
			}
			gameObject.transform.parent = _gameObject.transform.parent;
			_mc = gameObject.AddComponent<MeshCollider>();
		}
	}

	public static Mesh GetPooledMesh()
	{
		int count = meshPool.Count;
		Mesh mesh;
		if (count > 0)
		{
			count--;
			mesh = meshPool[count];
			meshPool.RemoveAt(count);
		}
		else
		{
			mesh = new Mesh();
			mesh.name = "Pool";
		}
		return mesh;
	}

	public static void AddPooledMesh(Mesh mesh)
	{
		if (meshPool.Count < 250)
		{
			mesh.Clear(keepVertexLayout: false);
			meshPool.Add(mesh);
		}
		else
		{
			UnityEngine.Object.Destroy(mesh);
		}
	}

	public virtual int CopyToMesh(MeshFilter[] _mf, MeshRenderer[] _mr, int _lodLevel, Action _onCopyComplete = null)
	{
		MeshFilter meshFilter = _mf[0];
		Mesh mesh = meshFilter.sharedMesh;
		int count = m_Vertices.Count;
		if (count == 0)
		{
			if ((bool)mesh)
			{
				meshFilter.sharedMesh = null;
				AddPooledMesh(mesh);
			}
			_onCopyComplete?.Invoke();
			return 0;
		}
		if (!mesh)
		{
			mesh = (meshFilter.sharedMesh = GetPooledMesh());
		}
		else
		{
			mesh.Clear(keepVertexLayout: false);
		}
		if (count != m_ColorVertices.Count)
		{
			Log.Error("ERROR: VM.mesh[{2}].Vertices.Count ({0}) != VM.mesh[{2}].ColorSides.Count ({1})", count, m_ColorVertices.Count, meshIndex);
			_onCopyComplete?.Invoke();
			return m_Triangles;
		}
		if (count != m_Uvs.Count)
		{
			Log.Error("ERROR: VM.mesh.chunkMesh[{2}].Vertices.Count ({0}) != VM.mesh[{2}].Uvs.Count ({1})", count, m_Uvs.Count, meshIndex);
			_onCopyComplete?.Invoke();
			return m_Triangles;
		}
		copyToMesh(mesh, m_Vertices, m_Indices, m_Uvs, UvsCrack, m_Normals, m_Tangents, m_ColorVertices, _onCopyComplete);
		return m_Triangles;
	}

	public void copyToMesh(Mesh _mesh, ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector2> _uvs, ArrayListMP<Vector2> _uvCracks, ArrayListMP<Vector3> _normals, ArrayListMP<Vector4> _tangents, ArrayListMP<Color> _colorVertices, Action _onCopyComplete = null)
	{
		if (MeshDataManager.Enabled)
		{
			MeshDataManager.Instance.Add(_mesh, _vertices, _indices, _normals, _tangents, _colorVertices, _uvs, _uvCracks, cloneData: false, generateNormals: true, generateTangents: true, recalculateUvDistributionMetrics: false, sameFrameUpload: true, _onCopyComplete);
			MeshDataManager.Instance.StartBatches();
			return;
		}
		MeshUnsafeCopyHelper.CopyVertices(_vertices, _mesh);
		if (_uvs.Count > 0)
		{
			MeshUnsafeCopyHelper.CopyUV(_uvs, _mesh);
			if (_uvCracks != null)
			{
				MeshUnsafeCopyHelper.CopyUV2(_uvCracks, _mesh);
			}
		}
		MeshUnsafeCopyHelper.CopyColors(_colorVertices, _mesh);
		MeshUnsafeCopyHelper.CopyTriangles(_indices, _mesh);
		if (_normals.Count == 0)
		{
			_mesh.RecalculateNormals();
		}
		else
		{
			if (_vertices.Count != _normals.Count)
			{
				Log.Error("ERROR: Vertices.Count ({0}) != Normals.Count ({1}), MeshIdx={2} TriIdx={3}", _vertices.Count, _normals.Count, meshIndex, CurTriangleIndex);
			}
			MeshUnsafeCopyHelper.CopyNormals(_normals, _mesh);
		}
		if (_uvs.Count > 0)
		{
			if (_tangents.Count == 0)
			{
				Utils.CalculateMeshTangents(_vertices, _indices, _normals, _uvs, _tangents, _mesh);
			}
			if (_vertices.Count != _tangents.Count)
			{
				Log.Out("copyToMesh {0} verts #{1} != tangents #{2}, MeshIdx={3} TriIdx={4}", _mesh.name, _vertices.Count, _tangents.Count, meshIndex, CurTriangleIndex);
			}
			else
			{
				MeshUnsafeCopyHelper.CopyTangents(_tangents, _mesh);
			}
		}
		_mesh.RecalculateUVDistributionMetrics();
		GameUtils.SetMeshVertexAttributes(_mesh, compressPosition: false);
		_mesh.UploadMeshData(markNoLongerReadable: false);
		_onCopyComplete?.Invoke();
	}

	public virtual int CopyToColliders(int _clrIdx, MeshCollider _meshCollider, out Mesh mesh)
	{
		if (m_CollIndices.Count == 0)
		{
			GameManager.Instance.World.m_ChunkManager.BakeDestroyCancel(_meshCollider);
			mesh = null;
			return 0;
		}
		mesh = ResetMesh(_meshCollider);
		MeshUnsafeCopyHelper.CopyVertices(m_CollVertices, mesh);
		MeshUnsafeCopyHelper.CopyTriangles(m_CollIndices, mesh);
		return m_CollIndices.Count / 3;
	}

	public Mesh ResetMesh(MeshCollider _meshCollider)
	{
		Mesh mesh = GameManager.Instance.World.m_ChunkManager.BakeCancelAndGetMesh(_meshCollider);
		if (!mesh)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear(keepVertexLayout: false);
		}
		return mesh;
	}

	public virtual void GetColorForTextureId(int _subMeshIdx, ref Transvoxel.BuildVertex _data)
	{
		_data.color = Color.black;
		_data.uv = Vector2.zero;
		_data.uv2 = Vector2.zero;
		_data.uv3 = Vector2.zero;
		_data.uv4 = Vector2.zero;
	}

	public virtual int FindOrCreateSubMesh(int _t0, int _t1, int _t2)
	{
		return 0;
	}

	public virtual void AddIndices(int _i0, int _i1, int _i2, int _submesh)
	{
		m_Indices.Add(_i0);
		m_Indices.Add(_i1);
		m_Indices.Add(_i2);
	}

	public virtual void ClearMesh()
	{
		CurTriangleIndex = 0;
		m_Vertices.Clear();
		m_Indices.Clear();
		m_Uvs.Clear();
		if (UvsCrack != null)
		{
			UvsCrack.Clear();
		}
		if (m_Uvs3 != null)
		{
			m_Uvs3.Clear();
		}
		if (m_Uvs4 != null)
		{
			m_Uvs4.Clear();
		}
		m_ColorVertices.Clear();
		m_Normals.Clear();
		m_Tangents.Clear();
		if (m_CollVertices != null)
		{
			m_CollVertices.Clear();
			m_CollIndices.Clear();
		}
	}

	public virtual void SizeToChunkDefaults(int _meshIndex)
	{
		GetDefaultSizes(_meshIndex, out var _vertMax, out var _indexMax, out var _colliderMax);
		m_Vertices.Grow(_vertMax);
		m_Indices.Grow(_indexMax);
		m_Uvs.Grow(_vertMax);
		if (UvsCrack != null)
		{
			UvsCrack.Grow(_vertMax);
		}
		if (m_Uvs3 != null)
		{
			m_Uvs3.Grow(_vertMax);
			m_Uvs4.Grow(_vertMax);
		}
		m_ColorVertices.Grow(_vertMax);
		m_Normals.Grow(_vertMax);
		m_Tangents.Grow(_vertMax);
		if (m_CollVertices != null)
		{
			m_CollVertices.Grow(_colliderMax);
			m_CollIndices.Grow(_colliderMax);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetDefaultSizes(int _meshIndex, out int _vertMax, out int _indexMax, out int _colliderMax)
	{
		switch (_meshIndex)
		{
		case 0:
			_vertMax = 65536;
			_indexMax = 131072;
			_colliderMax = 32768;
			break;
		case 1:
			_vertMax = 16384;
			_indexMax = 32768;
			_colliderMax = 32768;
			break;
		case 2:
			_vertMax = 4096;
			_indexMax = 8192;
			_colliderMax = 4096;
			break;
		case 3:
			_vertMax = 16384;
			_indexMax = 32768;
			_colliderMax = 8192;
			break;
		case 4:
			_vertMax = 128;
			_indexMax = 128;
			_colliderMax = 0;
			break;
		case 5:
			_vertMax = 1024;
			_indexMax = 1024;
			_colliderMax = 1024;
			break;
		default:
			_vertMax = 512;
			_indexMax = 1024;
			_colliderMax = 512;
			break;
		}
	}

	public virtual void Finished()
	{
		m_Triangles = m_Indices.Count / 3;
	}

	public virtual Color[] UpdateColors(byte _suncolor, byte _blockcolor)
	{
		for (int i = 0; i < m_ColorVertices.Count; i++)
		{
			m_ColorVertices[i] = Lighting.ToColor(_suncolor, _blockcolor, 1f);
		}
		return m_ColorVertices.ToArray();
	}

	public void AddBlockSide(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, BlockValue _blockValue, BlockFace blockFace, Lighting _lighting, int meshIndex)
	{
		Color color = _lighting.ToColor();
		AddQuadWithCracks(v1, color, v2, color, v3, color, v4, color, _blockValue.Block.getUVRectFromSideAndMetadata(meshIndex, blockFace, v1, _blockValue), WorldConstants.MapDamageToUVRect(_blockValue), bSwitchUvHorizontal: false);
	}

	public void AddBlockSide(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, BlockValue _blockValue, float colorBlockFace, BlockFace blockFace, byte sunlight, byte blocklight, int meshIndex)
	{
		Color color = Lighting.ToColor(sunlight, blocklight, colorBlockFace);
		AddQuadWithCracks(v1, color, v2, color, v3, color, v4, color, _blockValue.Block.getUVRectFromSideAndMetadata(meshIndex, blockFace, v1, _blockValue), WorldConstants.MapDamageToUVRect(_blockValue), bSwitchUvHorizontal: false);
	}

	public void AddBlockSideTri(Vector3 v1, Vector3 v2, Vector3 v3, int _meshIdx, BlockValue _blockValue, float colorBlockFace, BlockFace blockFace, byte sunlight, byte blocklight)
	{
		Color color = Lighting.ToColor(sunlight, blocklight, colorBlockFace);
		AddTriWithCracks(v1, color, v2, color, v3, color, WorldConstants.MapBlockToUVRect(_meshIdx, _blockValue, blockFace), WorldConstants.MapDamageToUVRect(_blockValue), bSwitchUvHorizontal: false);
	}

	public void AddBlockSideTri(Vector3 v1, Color c1, Vector3 v2, Color c2, Vector3 v3, Color c3, Rect uv1, Rect uv2)
	{
		AddTriWithCracks(v1, c1, v2, c2, v3, c3, uv1, uv2, bSwitchUvHorizontal: false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AddQuad(Vector3 _v0, Vector2 _uv0, Vector3 _v1, Vector2 _uv1, Vector3 _v2, Vector2 _uv3, Vector3 _v3, Vector2 _uv4, byte sunlight, byte blocklight, float sideColor)
	{
		if (m_Vertices.Count <= 786428)
		{
			m_Vertices.Add(_v0);
			m_Vertices.Add(_v1);
			m_Vertices.Add(_v2);
			m_Vertices.Add(_v3);
			int count = m_CollVertices.Count;
			m_CollVertices.Add(_v0);
			m_CollVertices.Add(_v1);
			m_CollVertices.Add(_v2);
			m_CollVertices.Add(_v3);
			m_Normals.Add(Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized);
			m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
			m_Normals.Add(Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized);
			m_Normals.Add(Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized);
			Color item = Lighting.ToColor(sunlight, blocklight, sideColor);
			item.a = temperature;
			m_ColorVertices.Add(item);
			m_ColorVertices.Add(item);
			m_ColorVertices.Add(item);
			m_ColorVertices.Add(item);
			m_Indices.Add(CurTriangleIndex);
			m_Indices.Add(CurTriangleIndex + 2);
			m_Indices.Add(CurTriangleIndex + 1);
			m_Indices.Add(CurTriangleIndex + 3);
			m_Indices.Add(CurTriangleIndex + 2);
			m_Indices.Add(CurTriangleIndex);
			m_CollIndices.Add(count);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count + 1);
			m_CollIndices.Add(count + 3);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count);
			m_Uvs.Add(_uv0);
			m_Uvs.Add(_uv1);
			m_Uvs.Add(_uv3);
			m_Uvs.Add(_uv4);
			UvsCrack.Add(Vector2.zero);
			UvsCrack.Add(Vector2.zero);
			UvsCrack.Add(Vector2.zero);
			UvsCrack.Add(Vector2.zero);
			CurTriangleIndex += 4;
		}
	}

	public void CreateFromQuadList(Vector3[] _vertices, Color _color)
	{
		m_Vertices.Add(_vertices[0]);
		m_Vertices.Add(_vertices[0] + new Vector3(1f, 0f, 0f));
		m_Vertices.Add(_vertices[0] + new Vector3(1f, 0f, 1f));
		m_Vertices.Add(_vertices[0] + new Vector3(0f, 0f, 1f));
		m_Vertices.Add(_vertices[0] + new Vector3(0.5f, 0.15f, 0.5f));
		m_CollVertices.Add(_vertices[0]);
		m_CollVertices.Add(_vertices[0] + new Vector3(1f, 0f, 0f));
		m_CollVertices.Add(_vertices[0] + new Vector3(1f, 0f, 1f));
		m_CollVertices.Add(_vertices[0] + new Vector3(0f, 0f, 1f));
		m_CollVertices.Add(_vertices[0] + new Vector3(0.5f, 0.15f, 0.5f));
		m_ColorVertices.Add(_color);
		m_ColorVertices.Add(_color);
		m_ColorVertices.Add(_color);
		m_ColorVertices.Add(_color);
		m_ColorVertices.Add(_color);
		m_Normals.Add(new Vector3(0f, 1f, 0f));
		m_Normals.Add(new Vector3(0f, 1f, 0f));
		m_Normals.Add(new Vector3(0f, 1f, 0f));
		m_Normals.Add(new Vector3(0f, 1f, 0f));
		m_Normals.Add(new Vector3(0f, 1f, 0f));
		Uvs.Add(Vector2.zero);
		Uvs.Add(Vector2.zero);
		Uvs.Add(Vector2.zero);
		Uvs.Add(Vector2.zero);
		Uvs.Add(Vector2.zero);
		UvsCrack.Add(Vector2.zero);
		UvsCrack.Add(Vector2.zero);
		UvsCrack.Add(Vector2.zero);
		UvsCrack.Add(Vector2.zero);
		UvsCrack.Add(Vector2.zero);
		m_Indices.Add(CurTriangleIndex + 4);
		m_Indices.Add(CurTriangleIndex);
		m_Indices.Add(CurTriangleIndex + 3);
		m_CollIndices.Add(CurTriangleIndex + 4);
		m_CollIndices.Add(CurTriangleIndex);
		m_CollIndices.Add(CurTriangleIndex + 3);
		m_Indices.Add(CurTriangleIndex + 4);
		m_Indices.Add(CurTriangleIndex + 1);
		m_Indices.Add(CurTriangleIndex);
		m_CollIndices.Add(CurTriangleIndex + 4);
		m_CollIndices.Add(CurTriangleIndex + 1);
		m_CollIndices.Add(CurTriangleIndex);
		m_Indices.Add(CurTriangleIndex + 4);
		m_Indices.Add(CurTriangleIndex + 2);
		m_Indices.Add(CurTriangleIndex + 1);
		m_CollIndices.Add(CurTriangleIndex + 4);
		m_CollIndices.Add(CurTriangleIndex + 2);
		m_CollIndices.Add(CurTriangleIndex + 1);
		m_Indices.Add(CurTriangleIndex + 4);
		m_Indices.Add(CurTriangleIndex + 3);
		m_Indices.Add(CurTriangleIndex + 2);
		m_CollIndices.Add(CurTriangleIndex + 4);
		m_CollIndices.Add(CurTriangleIndex + 3);
		m_CollIndices.Add(CurTriangleIndex + 2);
		CurTriangleIndex += 5;
	}

	public void AddBasicQuad(Vector3[] _vertices, Color _color, Vector2 _UVdata, bool bForceNormalsUp = false, bool bAlternateWinding = false)
	{
		if (m_Vertices.Count <= 786428 && _vertices.Length == 4)
		{
			Vector3 vector = _vertices[0];
			Vector3 vector2 = _vertices[1];
			Vector3 vector3 = _vertices[2];
			Vector3 vector4 = _vertices[3];
			int num = m_Vertices.Alloc(4);
			m_Vertices[num] = vector;
			m_Vertices[num + 1] = vector2;
			m_Vertices[num + 2] = vector3;
			m_Vertices[num + 3] = vector4;
			int num2 = m_CollVertices.Alloc(4);
			m_CollVertices[num] = vector;
			m_CollVertices[num + 1] = vector2;
			m_CollVertices[num + 2] = vector3;
			m_CollVertices[num + 3] = vector4;
			num = m_Normals.Alloc(4);
			if (bForceNormalsUp)
			{
				Vector3 up = Vector3.up;
				m_Normals[num] = up;
				m_Normals[num + 1] = up;
				m_Normals[num + 2] = up;
				m_Normals[num + 3] = up;
			}
			else
			{
				m_Normals[num] = Vector3.Cross(vector4 - vector, vector2 - vector).normalized;
				m_Normals[num + 1] = Vector3.Cross(vector - vector2, vector3 - vector2).normalized;
				m_Normals[num + 2] = Vector3.Cross(vector2 - vector3, vector4 - vector3).normalized;
				m_Normals[num + 3] = Vector3.Cross(vector3 - vector4, vector - vector4).normalized;
			}
			num = m_ColorVertices.Alloc(4);
			m_ColorVertices[num] = _color;
			m_ColorVertices[num + 1] = _color;
			m_ColorVertices[num + 2] = _color;
			m_ColorVertices[num + 3] = _color;
			num = m_Indices.Alloc(6);
			m_Indices[num] = (bAlternateWinding ? (CurTriangleIndex + 3) : CurTriangleIndex);
			m_Indices[num + 1] = CurTriangleIndex + 2;
			m_Indices[num + 2] = CurTriangleIndex + 1;
			m_Indices[num + 3] = CurTriangleIndex + 3;
			m_Indices[num + 4] = (bAlternateWinding ? (CurTriangleIndex + 1) : (CurTriangleIndex + 2));
			m_Indices[num + 5] = CurTriangleIndex;
			num = m_CollIndices.Alloc(6);
			m_CollIndices[num] = num2;
			m_CollIndices[num + 1] = num2 + 2;
			m_CollIndices[num + 2] = num2 + 1;
			m_CollIndices[num + 3] = num2 + 3;
			m_CollIndices[num + 4] = num2 + 2;
			m_CollIndices[num + 5] = num2;
			num = m_Uvs.Alloc(4);
			m_Uvs.Items[num].x = 0f;
			m_Uvs.Items[num].y = 1f;
			m_Uvs.Items[++num].x = 1f;
			m_Uvs.Items[num].y = 1f;
			m_Uvs.Items[++num].x = 1f;
			m_Uvs.Items[num].y = 0f;
			m_Uvs.Items[++num].x = 0f;
			m_Uvs.Items[num].y = 0f;
			num = UvsCrack.Alloc(4);
			UvsCrack[num] = _UVdata;
			UvsCrack[num + 1] = _UVdata;
			UvsCrack[num + 2] = _UVdata;
			UvsCrack[num + 3] = _UVdata;
			CurTriangleIndex += 4;
		}
	}

	public void AddQuadNoCollision(Vector3 _v0, Vector3 _v1, Vector3 _v2, Vector3 _v3, Color _color, Rect _uvTex)
	{
		if (m_Vertices.Count <= 786428)
		{
			int num = m_Vertices.Alloc(4);
			m_Vertices[num] = _v0;
			m_Vertices[num + 1] = _v1;
			m_Vertices[num + 2] = _v2;
			m_Vertices[num + 3] = _v3;
			num = m_Normals.Alloc(4);
			m_Normals[num] = Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized;
			m_Normals[num + 1] = Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized;
			m_Normals[num + 2] = Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized;
			m_Normals[num + 3] = Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized;
			_color.a = temperature;
			num = m_ColorVertices.Alloc(4);
			m_ColorVertices[num] = _color;
			m_ColorVertices[num + 1] = _color;
			m_ColorVertices[num + 2] = _color;
			m_ColorVertices[num + 3] = _color;
			num = m_Indices.Alloc(6);
			m_Indices[num] = CurTriangleIndex;
			m_Indices[num + 1] = CurTriangleIndex + 2;
			m_Indices[num + 2] = CurTriangleIndex + 1;
			m_Indices[num + 3] = CurTriangleIndex + 3;
			m_Indices[num + 4] = CurTriangleIndex + 2;
			m_Indices[num + 5] = CurTriangleIndex;
			num = m_Uvs.Alloc(4);
			float x = _uvTex.x;
			float y = _uvTex.y;
			float width = _uvTex.width;
			float height = _uvTex.height;
			m_Uvs.Items[num].x = x;
			m_Uvs.Items[num].y = y;
			m_Uvs.Items[++num].x = x + width;
			m_Uvs.Items[num].y = y;
			m_Uvs.Items[++num].x = x + width;
			m_Uvs.Items[num].y = y + height;
			m_Uvs.Items[++num].x = x;
			m_Uvs.Items[num].y = y + height;
			CurTriangleIndex += 4;
		}
	}

	public void AddQuadWithCracks(Vector3 _v0, Color _c0, Vector3 _v1, Color _c1, Vector3 _v2, Color _c2, Vector3 _v3, Color _c3, Rect _uvTex, Rect _uvOverlay, bool bSwitchUvHorizontal)
	{
		if (m_Vertices.Count <= 786428)
		{
			int num = m_Vertices.Alloc(4);
			m_Vertices[num] = _v0;
			m_Vertices[num + 1] = _v1;
			m_Vertices[num + 2] = _v2;
			m_Vertices[num + 3] = _v3;
			int count = m_CollVertices.Count;
			m_CollVertices.Add(_v0);
			m_CollVertices.Add(_v1);
			m_CollVertices.Add(_v2);
			m_CollVertices.Add(_v3);
			num = m_Normals.Alloc(4);
			m_Normals[num] = Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized;
			m_Normals[num + 1] = Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized;
			m_Normals[num + 2] = Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized;
			m_Normals[num + 3] = Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized;
			_c3.a = (_c2.a = (_c1.a = (_c0.a = temperature)));
			num = m_ColorVertices.Alloc(4);
			m_ColorVertices[num] = _c0;
			m_ColorVertices[num + 1] = _c1;
			m_ColorVertices[num + 2] = _c2;
			m_ColorVertices[num + 3] = _c3;
			num = m_Indices.Alloc(6);
			m_Indices[num] = CurTriangleIndex;
			m_Indices[num + 1] = CurTriangleIndex + 2;
			m_Indices[num + 2] = CurTriangleIndex + 1;
			m_Indices[num + 3] = CurTriangleIndex + 3;
			m_Indices[num + 4] = CurTriangleIndex + 2;
			m_Indices[num + 5] = CurTriangleIndex;
			m_CollIndices.Add(count);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count + 1);
			m_CollIndices.Add(count + 3);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count);
			num = m_Uvs.Alloc(4);
			float x = _uvTex.x;
			float y = _uvTex.y;
			float width = _uvTex.width;
			float height = _uvTex.height;
			if (!bSwitchUvHorizontal)
			{
				m_Uvs.Items[num].x = x;
				m_Uvs.Items[num].y = y;
				m_Uvs.Items[++num].x = x + width;
				m_Uvs.Items[num].y = y;
				m_Uvs.Items[++num].x = x + width;
				m_Uvs.Items[num].y = y + height;
				m_Uvs.Items[++num].x = x;
				m_Uvs.Items[num].y = y + height;
			}
			else
			{
				m_Uvs.Items[num].x = x + width;
				m_Uvs.Items[num].y = y;
				m_Uvs.Items[++num].x = x;
				m_Uvs.Items[num].y = y;
				m_Uvs.Items[++num].x = x;
				m_Uvs.Items[num].y = y + height;
				m_Uvs.Items[++num].x = x + width;
				m_Uvs.Items[num].y = y + height;
			}
			num = UvsCrack.Alloc(4);
			x = _uvOverlay.x;
			y = _uvOverlay.y;
			width = _uvOverlay.width;
			height = _uvOverlay.height;
			UvsCrack.Items[num].x = x;
			UvsCrack.Items[num].y = y;
			UvsCrack.Items[++num].x = x;
			UvsCrack.Items[num].y = y + height;
			UvsCrack.Items[++num].x = x + width;
			UvsCrack.Items[num].y = y + height;
			UvsCrack.Items[++num].x = x + width;
			UvsCrack.Items[num].y = y;
			CurTriangleIndex += 4;
		}
	}

	public void AddRectangle(Vector3 _v0, Vector2 _uv0, Color _c0, Vector3 _v1, Vector2 _uv1, Color _c1, Vector3 _v2, Vector2 _uv2, Color _c2, Vector3 _v3, Vector2 _uv3, Color _c3, Rect _uvTex, Rect _uvOverlay)
	{
		if (m_Vertices.Count <= 786428)
		{
			m_Vertices.Add(_v0);
			m_Vertices.Add(_v1);
			m_Vertices.Add(_v2);
			m_Vertices.Add(_v3);
			int count = m_CollVertices.Count;
			m_CollVertices.Add(_v0);
			m_CollVertices.Add(_v1);
			m_CollVertices.Add(_v2);
			m_CollVertices.Add(_v3);
			m_Normals.Add(Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized);
			m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
			m_Normals.Add(Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized);
			m_Normals.Add(Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized);
			_c3.a = (_c2.a = (_c1.a = (_c0.a = temperature)));
			m_ColorVertices.Add(_c0);
			m_ColorVertices.Add(_c1);
			m_ColorVertices.Add(_c2);
			m_ColorVertices.Add(_c3);
			m_Indices.Add(CurTriangleIndex);
			m_Indices.Add(CurTriangleIndex + 2);
			m_Indices.Add(CurTriangleIndex + 1);
			m_Indices.Add(CurTriangleIndex + 3);
			m_Indices.Add(CurTriangleIndex + 2);
			m_Indices.Add(CurTriangleIndex);
			m_CollIndices.Add(count);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count + 1);
			m_CollIndices.Add(count + 3);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count);
			m_Uvs.Add(new Vector2(_uvTex.x + _uv0.x * _uvTex.width, _uvTex.y + _uv0.y * _uvTex.height));
			m_Uvs.Add(new Vector2(_uvTex.x + _uv1.x * _uvTex.width, _uvTex.y + _uv1.y * _uvTex.height));
			m_Uvs.Add(new Vector2(_uvTex.x + _uv2.x * _uvTex.width, _uvTex.y + _uv2.y * _uvTex.height));
			m_Uvs.Add(new Vector2(_uvTex.x + _uv3.x * _uvTex.width, _uvTex.y + _uv3.y * _uvTex.height));
			UvsCrack.Add(new Vector2(_uvOverlay.x + _uv0.x * _uvOverlay.width, _uvOverlay.y + _uv0.y * _uvOverlay.height));
			UvsCrack.Add(new Vector2(_uvOverlay.x + _uv1.x * _uvOverlay.width, _uvOverlay.y + _uv1.y * _uvOverlay.height));
			UvsCrack.Add(new Vector2(_uvOverlay.x + _uv2.x * _uvOverlay.width, _uvOverlay.y + _uv2.y * _uvOverlay.height));
			UvsCrack.Add(new Vector2(_uvOverlay.x + _uv3.x * _uvOverlay.width, _uvOverlay.y + _uv3.y * _uvOverlay.height));
			CurTriangleIndex += 4;
		}
	}

	public void AddRectangle(Vector3 _v0, Vector2 _uv0, Vector3 _v1, Vector2 _uv1, Vector3 _v2, Vector2 _uv2, Vector3 _v3, Vector2 _uv3, Vector3 _normal, Vector3 _normalTop, Vector4 _tangent, Rect _uvTex, byte _sunlight, byte _blocklight)
	{
		if (m_Vertices.Count <= 786428)
		{
			int num = m_Vertices.Alloc(4);
			m_Vertices[num] = _v0;
			m_Vertices[num + 1] = _v1;
			m_Vertices[num + 2] = _v2;
			m_Vertices[num + 3] = _v3;
			num = m_Normals.Alloc(4);
			m_Normals[num] = _normal;
			m_Normals[num + 1] = _normal;
			m_Normals[num + 2] = _normalTop;
			m_Normals[num + 3] = _normalTop;
			num = m_Tangents.Alloc(4);
			m_Tangents[num] = _tangent;
			m_Tangents[num + 1] = _tangent;
			m_Tangents[num + 2] = _tangent;
			m_Tangents[num + 3] = _tangent;
			Color value = Lighting.ToColor(_sunlight, _blocklight);
			value.a = temperature;
			num = m_ColorVertices.Alloc(4);
			m_ColorVertices[num] = value;
			m_ColorVertices[num + 1] = value;
			m_ColorVertices[num + 2] = value;
			m_ColorVertices[num + 3] = value;
			num = m_Indices.Alloc(6);
			m_Indices[num] = CurTriangleIndex;
			m_Indices[num + 1] = CurTriangleIndex + 2;
			m_Indices[num + 2] = CurTriangleIndex + 1;
			m_Indices[num + 3] = CurTriangleIndex + 3;
			m_Indices[num + 4] = CurTriangleIndex + 2;
			m_Indices[num + 5] = CurTriangleIndex;
			num = m_Uvs.Alloc(4);
			float x = _uvTex.x;
			float y = _uvTex.y;
			float width = _uvTex.width;
			float height = _uvTex.height;
			m_Uvs.Items[num].x = x + _uv0.x * width;
			m_Uvs.Items[num].y = y + _uv0.y * height;
			m_Uvs.Items[++num].x = x + _uv1.x * width;
			m_Uvs.Items[num].y = y + _uv1.y * height;
			m_Uvs.Items[++num].x = x + _uv2.x * width;
			m_Uvs.Items[num].y = y + _uv2.y * height;
			m_Uvs.Items[++num].x = x + _uv3.x * width;
			m_Uvs.Items[num].y = y + _uv3.y * height;
			num = UvsCrack.Alloc(4);
			UvsCrack[num] = Vector2.zero;
			UvsCrack[num + 1] = Vector2.zero;
			UvsCrack[num + 2] = Vector2.zero;
			UvsCrack[num + 3] = Vector2.zero;
			CurTriangleIndex += 4;
		}
	}

	public void AddRectangleColliderPair(Vector3 _v0, Vector3 _v1, Vector3 _v2, Vector3 _v3)
	{
		if (m_CollVertices.Count <= 786428)
		{
			int num = m_CollVertices.Alloc(4);
			m_CollVertices[num] = _v0;
			m_CollVertices[num + 1] = _v1;
			m_CollVertices[num + 2] = _v2;
			m_CollVertices[num + 3] = _v3;
			int num2 = m_CollIndices.Alloc(12);
			m_CollIndices[num2] = num;
			m_CollIndices[num2 + 1] = num + 1;
			m_CollIndices[num2 + 2] = num + 2;
			m_CollIndices[num2 + 3] = num;
			m_CollIndices[num2 + 4] = num + 2;
			m_CollIndices[num2 + 5] = num + 3;
			m_CollIndices[num2 + 6] = num;
			m_CollIndices[num2 + 7] = num + 2;
			m_CollIndices[num2 + 8] = num + 1;
			m_CollIndices[num2 + 9] = num + 3;
			m_CollIndices[num2 + 10] = num + 2;
			m_CollIndices[num2 + 11] = num;
		}
	}

	public void AddColoredRectangle(Vector3 _v0, Color _c0, Vector3 _v1, Color _c1, Vector3 _v2, Color _c2, Vector3 _v3, Color _c3)
	{
		if (m_Vertices.Count <= 786428)
		{
			m_Vertices.Add(_v0);
			m_Vertices.Add(_v1);
			m_Vertices.Add(_v2);
			m_Vertices.Add(_v3);
			int count = m_CollVertices.Count;
			m_CollVertices.Add(_v0);
			m_CollVertices.Add(_v1);
			m_CollVertices.Add(_v2);
			m_CollVertices.Add(_v3);
			m_Normals.Add(Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized);
			m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
			m_Normals.Add(Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized);
			m_Normals.Add(Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized);
			_c3.a = (_c2.a = (_c1.a = (_c0.a = temperature)));
			m_ColorVertices.Add(_c0);
			m_ColorVertices.Add(_c1);
			m_ColorVertices.Add(_c2);
			m_ColorVertices.Add(_c3);
			m_Indices.Add(CurTriangleIndex);
			m_Indices.Add(CurTriangleIndex + 2);
			m_Indices.Add(CurTriangleIndex + 1);
			m_Indices.Add(CurTriangleIndex + 3);
			m_Indices.Add(CurTriangleIndex + 2);
			m_Indices.Add(CurTriangleIndex);
			m_CollIndices.Add(count);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count + 1);
			m_CollIndices.Add(count + 3);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count);
			CurTriangleIndex += 4;
		}
	}

	public void AddTriWithCracks(Vector3 _v0, Color _c0, Vector3 _v1, Color _c1, Vector3 _v2, Color _c2, Rect uvTex, Rect uvOverlay, bool bSwitchUvHorizontal)
	{
		if (m_Vertices.Count <= 786429)
		{
			m_Vertices.Add(_v0);
			m_Vertices.Add(_v1);
			m_Vertices.Add(_v2);
			int count = m_CollVertices.Count;
			m_CollVertices.Add(_v0);
			m_CollVertices.Add(_v1);
			m_CollVertices.Add(_v2);
			m_Normals.Add(Vector3.Cross(_v2 - _v0, _v1 - _v0).normalized);
			m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
			m_Normals.Add(Vector3.Cross(_v1 - _v2, _v0 - _v2).normalized);
			_c2.a = (_c1.a = (_c0.a = temperature));
			ColorVertices.Add(_c0);
			ColorVertices.Add(_c1);
			ColorVertices.Add(_c2);
			m_Indices.Add(CurTriangleIndex);
			m_Indices.Add(CurTriangleIndex + 2);
			m_Indices.Add(CurTriangleIndex + 1);
			m_CollIndices.Add(count);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count + 1);
			Uvs.Add(new Vector2(uvTex.x + 0f, uvTex.y + 0f));
			if (!bSwitchUvHorizontal)
			{
				Uvs.Add(new Vector2(uvTex.x + 0f, uvTex.y + uvTex.height - 0f));
			}
			else
			{
				Uvs.Add(new Vector2(uvTex.x + uvTex.width - 0f, uvTex.y + 0f));
			}
			Uvs.Add(new Vector2(uvTex.x + uvTex.width - 0f, uvTex.y + uvTex.height - 0f));
			UvsCrack.Add(new Vector2(uvOverlay.x + 0f, uvOverlay.y + 0f));
			if (!bSwitchUvHorizontal)
			{
				UvsCrack.Add(new Vector2(uvOverlay.x + 0f, uvOverlay.y + uvOverlay.height - 0f));
			}
			else
			{
				UvsCrack.Add(new Vector2(uvOverlay.x + uvOverlay.width - 0f, uvOverlay.y + 0f));
			}
			UvsCrack.Add(new Vector2(uvOverlay.x + uvOverlay.width - 0f, uvOverlay.y + uvOverlay.height - 0f));
			CurTriangleIndex += 3;
		}
	}

	public void AddTriangle(Vector3 _v0, Vector2 _uv0, Color _c0, Vector3 _v1, Vector2 _uv1, Color _c1, Vector3 _v2, Vector2 _uv2, Color _c2, Rect uvTex, Rect uvOverlay)
	{
		if (m_Vertices.Count <= 786429)
		{
			m_Vertices.Add(_v0);
			m_Vertices.Add(_v1);
			m_Vertices.Add(_v2);
			int count = m_CollVertices.Count;
			m_CollVertices.Add(_v0);
			m_CollVertices.Add(_v1);
			m_CollVertices.Add(_v2);
			m_Normals.Add(Vector3.Cross(_v2 - _v0, _v1 - _v0).normalized);
			m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
			m_Normals.Add(Vector3.Cross(_v1 - _v2, _v0 - _v2).normalized);
			_c2.a = (_c1.a = (_c0.a = temperature));
			ColorVertices.Add(_c0);
			ColorVertices.Add(_c1);
			ColorVertices.Add(_c2);
			m_Indices.Add(CurTriangleIndex);
			m_Indices.Add(CurTriangleIndex + 2);
			m_Indices.Add(CurTriangleIndex + 1);
			m_CollIndices.Add(count);
			m_CollIndices.Add(count + 2);
			m_CollIndices.Add(count + 1);
			Uvs.Add(new Vector2(uvTex.x + _uv0.x * uvTex.width, uvTex.y + _uv0.y * uvTex.height));
			Uvs.Add(new Vector2(uvTex.x + _uv1.x * uvTex.width, uvTex.y + _uv1.y * uvTex.height));
			Uvs.Add(new Vector2(uvTex.x + _uv2.x * uvTex.width, uvTex.y + _uv2.y * uvTex.height));
			UvsCrack.Add(new Vector2(uvOverlay.x + _uv0.x * uvOverlay.width, uvOverlay.y + _uv0.y * uvOverlay.height));
			UvsCrack.Add(new Vector2(uvOverlay.x + _uv1.x * uvOverlay.width, uvOverlay.y + _uv1.y * uvOverlay.height));
			UvsCrack.Add(new Vector2(uvOverlay.x + _uv2.x * uvOverlay.width, uvOverlay.y + _uv2.y * uvOverlay.height));
			CurTriangleIndex += 3;
		}
	}

	public virtual void AddMesh(Vector3 _drawPos, int _count, Vector3[] _vertices, Vector3[] _normals, ArrayListMP<int> _indices, ArrayListMP<Vector2> _uvs, byte _sunlight, byte _blocklight, VoxelMesh _specialColliders, int damage)
	{
		if (_count + m_Vertices.Count <= 786432)
		{
			int curTriangleIndex = CurTriangleIndex;
			CurTriangleIndex += _count;
			m_Vertices.AddRange(_vertices, 0, _count);
			m_Normals.AddRange(_normals, 0, _count);
			Color value = new Color((float)(int)_sunlight / 15f, 0f, 0f, temperature);
			int num = m_ColorVertices.Alloc(_count);
			for (int i = 0; i < _count; i++)
			{
				m_ColorVertices[num + i] = value;
			}
			num = m_Indices.Alloc(_indices.Count);
			for (int j = 0; j < _indices.Count; j++)
			{
				m_Indices[num + j] = _indices[j] + curTriangleIndex;
			}
			m_Uvs.AddRange(_uvs.Items, 0, _uvs.Count);
			Vector2 value2 = new Vector2(damage, 0f);
			num = UvsCrack.Alloc(_uvs.Count);
			for (int k = 0; k < _uvs.Count; k++)
			{
				UvsCrack[num + k] = value2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void calculateMeshTangentsDummy(Mesh mesh)
	{
		int num = mesh.vertices.Length;
		Vector4[] array = new Vector4[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Vector4.one;
		}
		mesh.tangents = array;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void calculateMeshNormalsDummy(Mesh mesh)
	{
		int num = mesh.vertices.Length;
		Vector3[] array = new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Vector3.one;
		}
		mesh.normals = array;
	}

	public void CheckVertexLimit(int _count)
	{
	}

	public void AddRectXYFacingNorth(float _x, float _y, float _z, int _xAdd, int _yAdd)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y + (float)_yAdd, _z));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		m_Indices.Add(count);
		m_Indices.Add(count + 1);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 3);
		m_Indices.Add(count);
	}

	public void AddRectXYFacingNorth(float _x, float _y, float _z, int _xAdd, int _yAdd, Color _c)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y + (float)_yAdd, _z));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		_c.a = temperature;
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_Indices.Add(count);
		m_Indices.Add(count + 1);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 3);
		m_Indices.Add(count);
	}

	public void AddRectXYFacingSouth(float _x, float _y, float _z, int _xAdd, int _zAdd)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y + (float)_zAdd, _z));
		m_Vertices.Add(new Vector3(_x, _y + (float)_zAdd, _z));
		m_Indices.Add(count);
		m_Indices.Add(count + 3);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 1);
		m_Indices.Add(count);
	}

	public void AddRectXYFacingSouth(float _x, float _y, float _z, int _xAdd, int _zAdd, Color _c)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y + (float)_zAdd, _z));
		m_Vertices.Add(new Vector3(_x, _y + (float)_zAdd, _z));
		_c.a = temperature;
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_Indices.Add(count);
		m_Indices.Add(count + 3);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 1);
		m_Indices.Add(count);
	}

	public void AddRectYZFacingWest(float _x, float _y, float _z, int _yAdd, int _zAdd)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		m_Indices.Add(count);
		m_Indices.Add(count + 1);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 3);
		m_Indices.Add(count);
	}

	public void AddRectYZFacingWest(float _x, float _y, float _z, int _yAdd, int _zAdd, Color _c)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		_c.a = temperature;
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_Indices.Add(count);
		m_Indices.Add(count + 1);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 3);
		m_Indices.Add(count);
	}

	public void AddRectYZFacingEast(float _x, float _y, float _z, int _yAdd, int _zAdd)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		m_Indices.Add(count);
		m_Indices.Add(count + 3);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 1);
		m_Indices.Add(count);
	}

	public void AddRectYZFacingEast(float _x, float _y, float _z, int _yAdd, int _zAdd, Color _c)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		_c.a = temperature;
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_Indices.Add(count);
		m_Indices.Add(count + 3);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 1);
		m_Indices.Add(count);
	}

	public void AddRectXZFacingUp(float _x, float _y, float _z, int _xAdd, int _zAdd)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		m_Indices.Add(count);
		m_Indices.Add(count + 3);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 1);
		m_Indices.Add(count);
	}

	public void AddRectXZFacingUp(float _x, float _y, float _z, int _xAdd, int _zAdd, Color _c)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		_c.a = temperature;
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_Indices.Add(count);
		m_Indices.Add(count + 3);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 1);
		m_Indices.Add(count);
	}

	public void AddRectXZFacingDown(float _x, float _y, float _z, int _xAdd, int _zAdd)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		m_Indices.Add(count);
		m_Indices.Add(count + 1);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 3);
		m_Indices.Add(count);
	}

	public void AddRectXZFacingDown(float _x, float _y, float _z, int _xAdd, int _zAdd, Color _c)
	{
		int count = m_Vertices.Count;
		m_Vertices.Add(new Vector3(_x, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z + (float)_zAdd));
		m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		_c.a = temperature;
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_ColorVertices.Add(_c);
		m_Indices.Add(count);
		m_Indices.Add(count + 1);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 2);
		m_Indices.Add(count + 3);
		m_Indices.Add(count);
	}

	public static float GetTemperature(BiomeDefinition bd)
	{
		if (bd == null)
		{
			return -10f;
		}
		return 0f;
	}

	public void ClearTemperatureValues()
	{
		for (int i = 0; i < m_ColorVertices.Count; i++)
		{
			m_ColorVertices[i] = new Color(m_ColorVertices[i].r, m_ColorVertices[i].g, m_ColorVertices[i].b, 100f);
		}
	}
}
