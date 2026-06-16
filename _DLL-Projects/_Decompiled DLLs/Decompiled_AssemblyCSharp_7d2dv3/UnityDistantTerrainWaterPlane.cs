using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UnityDistantTerrainWaterPlane
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class FixedMeshData
	{
		public int count;

		public Vector3[] vertices;

		public Vector3[] normals;

		public Vector4[] tangents;

		public Vector2[] uvs;

		public int[] triangles;

		public FixedMeshData(int _vertCount, int _triCount)
		{
			count = _vertCount;
			vertices = new Vector3[count];
			normals = new Vector3[count];
			tangents = new Vector4[count];
			uvs = new Vector2[count];
			triangles = new int[_triCount];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class DynamicMeshData
	{
		public List<Vector3> vertices;

		public List<Vector3> normals;

		public List<Vector4> tangents;

		public List<Vector2> uvs;

		public List<int> triangles;

		[PublicizedFrom(EAccessModifier.Private)]
		public static Vector4 defaultWaterPlaneTangent = new Vector4(1f, 0f, 0f, 1f);

		public DynamicMeshData(int _triCapacity)
		{
			vertices = new List<Vector3>(_triCapacity / 2);
			normals = new List<Vector3>(_triCapacity / 2);
			tangents = new List<Vector4>(_triCapacity / 2);
			uvs = new List<Vector2>(_triCapacity / 2);
			triangles = new List<int>(_triCapacity);
		}

		public void Clear()
		{
			vertices.Clear();
			normals.Clear();
			tangents.Clear();
			uvs.Clear();
			triangles.Clear();
		}

		public void AddQuad(int _x, int _y, int _z, int _width, int _height, int _scale, int _totalCountP1)
		{
			int count = vertices.Count;
			_x *= _scale;
			_z *= _scale;
			_width *= _scale;
			_height *= _scale;
			vertices.Add(new Vector3(_x, (float)_y + 0.77f, _z));
			vertices.Add(new Vector3(_x + _width, (float)_y + 0.77f, _z + _height));
			vertices.Add(new Vector3(_x + _width, (float)_y + 0.77f, _z));
			normals.Add(Vector3.up);
			normals.Add(Vector3.up);
			normals.Add(Vector3.up);
			tangents.Add(defaultWaterPlaneTangent);
			tangents.Add(defaultWaterPlaneTangent);
			tangents.Add(defaultWaterPlaneTangent);
			triangles.Add(count);
			triangles.Add(1 + count);
			triangles.Add(2 + count);
			count = vertices.Count;
			vertices.Add(new Vector3(_x, (float)_y + 0.77f, _z));
			vertices.Add(new Vector3(_x, (float)_y + 0.77f, _z + _height));
			vertices.Add(new Vector3(_x + _width, (float)_y + 0.77f, _z + _height));
			normals.Add(Vector3.up);
			normals.Add(Vector3.up);
			normals.Add(Vector3.up);
			tangents.Add(defaultWaterPlaneTangent);
			tangents.Add(defaultWaterPlaneTangent);
			tangents.Add(defaultWaterPlaneTangent);
			triangles.Add(count);
			triangles.Add(1 + count);
			triangles.Add(2 + count);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWaterPlaneChunkSize = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public FixedMeshData fixedWaterPlaneHiRes;

	[PublicizedFrom(EAccessModifier.Private)]
	public FixedMeshData fixedWaterPlaneLoRes;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshData dynamicWaterPlane;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] dynamiceWaterPlaneChunkArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnityDistantTerrain.Config terrainConfig;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material waterMaterial;

	public UnityDistantTerrainWaterPlane(UnityDistantTerrain.Config _terrainConfig, Material _waterMaterial)
	{
		terrainConfig = _terrainConfig;
		waterMaterial = _waterMaterial;
	}

	public void createDynamicWaterPlane_Step1(int _xStart, int _zStart, int _tileSize, int _waterChunks16x16Width, byte[] _waterChunks16x16)
	{
		int num = terrainConfig.ChunkWorldSize / 16 + 1;
		int num2 = num * num;
		if (dynamiceWaterPlaneChunkArr == null || dynamiceWaterPlaneChunkArr.Length != num2 * num2)
		{
			dynamiceWaterPlaneChunkArr = new int[num2];
		}
		else
		{
			Array.Clear(dynamiceWaterPlaneChunkArr, 0, dynamiceWaterPlaneChunkArr.Length);
		}
		int num3 = _tileSize / 16;
		int num4 = _tileSize / 16;
		int num5 = _xStart / 16 + _zStart / 16 * _waterChunks16x16Width;
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				int num6 = _waterChunks16x16[num5 + i + j * _waterChunks16x16Width];
				if (num6 != 0)
				{
					dynamiceWaterPlaneChunkArr[i + j * num] = 0x1010000 | num6;
				}
			}
		}
		if (dynamicWaterPlane == null)
		{
			dynamicWaterPlane = new DynamicMeshData(num * num);
		}
		else
		{
			dynamicWaterPlane.Clear();
		}
		for (int k = 0; k < dynamiceWaterPlaneChunkArr.Length; k++)
		{
			int num7 = dynamiceWaterPlaneChunkArr[k] >> 16;
			if (num7 != 0)
			{
				int x = k % num;
				int z = k / num;
				int width = num7 >> 8;
				int height = num7 & 0xFF;
				int y = dynamiceWaterPlaneChunkArr[k] & 0xFFFF;
				dynamicWaterPlane.AddQuad(x, y, z, width, height, 16, num);
			}
		}
	}

	public GameObject createDynamicWaterPlane_Step2(GameObject _existingWaterPlane, Transform _parent, string _tag = null)
	{
		if (_existingWaterPlane == null)
		{
			_existingWaterPlane = new GameObject("WaterHi");
			_existingWaterPlane.transform.parent = _parent;
			_existingWaterPlane.transform.localPosition = new Vector3(0f, 0f, 0f);
			_existingWaterPlane.layer = 14;
		}
		if (dynamicWaterPlane == null || dynamicWaterPlane.vertices.Count == 0)
		{
			return _existingWaterPlane;
		}
		MeshFilter meshFilter = _existingWaterPlane.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = _existingWaterPlane.AddComponent<MeshFilter>();
		}
		else
		{
			meshFilter.mesh.Clear();
		}
		Mesh mesh = meshFilter.mesh;
		mesh.SetVertices(dynamicWaterPlane.vertices);
		mesh.SetNormals(dynamicWaterPlane.normals);
		mesh.SetTangents(dynamicWaterPlane.tangents);
		mesh.SetTriangles(dynamicWaterPlane.triangles, 0);
		MeshRenderer meshRenderer = _existingWaterPlane.GetComponent<MeshRenderer>();
		if (meshRenderer == null)
		{
			meshRenderer = _existingWaterPlane.AddComponent<MeshRenderer>();
		}
		meshRenderer.material = waterMaterial;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		if (_tag != null)
		{
			_existingWaterPlane.tag = _tag;
		}
		dynamicWaterPlane.Clear();
		_existingWaterPlane.SetActive(value: true);
		return _existingWaterPlane;
	}

	public void Cleanup()
	{
		dynamicWaterPlane = null;
		fixedWaterPlaneHiRes = null;
		fixedWaterPlaneLoRes = null;
	}
}
