using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

[DebuggerDisplay("{Name} O:{OpaqueMesh.Vertices.Count} T:{TerrainMesh.Vertices.Count}")]
public class DyMeshData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentQueue<DyMeshData> Cache = new ConcurrentQueue<DyMeshData>();

	public static int ActiveItems = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int InstanceCount;

	public string Name;

	public VoxelMesh OpaqueMesh;

	public VoxelMeshTerrain TerrainMesh;

	public static int TotalItems => ActiveItems + Cache.Count;

	public void Reset()
	{
		ResetMesh(OpaqueMesh);
		ResetTerrainMesh(TerrainMesh);
	}

	public static void ResetMeshLayer(VoxelMeshLayer layer)
	{
		VoxelMesh[] meshes = layer.meshes;
		for (int i = 0; i < meshes.Length; i++)
		{
			ResetMesh(meshes[i]);
		}
	}

	public static void ResetMesh(VoxelMesh mesh)
	{
		if (mesh is VoxelMeshTerrain mesh2)
		{
			ResetTerrainMesh(mesh2);
		}
		else
		{
			ResetOpaqueMesh(mesh);
		}
	}

	public static void ResetTerrainMesh(VoxelMeshTerrain mesh)
	{
		ResetOpaqueMesh(mesh);
		mesh.submeshes.Clear();
	}

	public static void ResetOpaqueMesh(VoxelMesh mesh)
	{
		mesh.CurTriangleIndex = 0;
		mesh.Vertices.Count = 0;
		mesh.Indices.Count = 0;
		mesh.Uvs.Count = 0;
		if (mesh.UvsCrack != null)
		{
			mesh.UvsCrack.Count = 0;
		}
		if (mesh.Uvs3 != null)
		{
			mesh.Uvs3.Count = 0;
		}
		if (mesh.Uvs4 != null)
		{
			mesh.Uvs4.Count = 0;
		}
		mesh.ColorVertices.Count = 0;
		mesh.m_Normals.Count = 0;
		mesh.m_Tangents.Count = 0;
		if (mesh.m_CollVertices != null)
		{
			mesh.m_CollVertices.Count = 0;
			mesh.m_CollIndices.Count = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string DebugDyMeshdata(DyMeshData data)
	{
		object[] obj = new object[23]
		{
			data.OpaqueMesh.Vertices.Items.Length,
			data.OpaqueMesh.Indices.Items.Length,
			data.OpaqueMesh.Uvs.Items.Length,
			data.OpaqueMesh.UvsCrack.Items.Length,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null
		};
		ArrayListMP<Vector2> uvs = data.OpaqueMesh.Uvs3;
		obj[4] = ((uvs != null) ? uvs.Items.Length : 0);
		ArrayListMP<Vector2> uvs2 = data.OpaqueMesh.Uvs4;
		obj[5] = ((uvs2 != null) ? uvs2.Items.Length : 0);
		ArrayListMP<Color> colorVertices = data.OpaqueMesh.ColorVertices;
		obj[6] = ((colorVertices != null) ? colorVertices.Items.Length : 0);
		ArrayListMP<Vector3> normals = data.OpaqueMesh.m_Normals;
		obj[7] = ((normals != null) ? normals.Items.Length : 0);
		ArrayListMP<Vector4> tangents = data.OpaqueMesh.m_Tangents;
		obj[8] = ((tangents != null) ? tangents.Items.Length : 0);
		ArrayListMP<Vector3> collVertices = data.OpaqueMesh.m_CollVertices;
		obj[9] = ((collVertices != null) ? collVertices.Items.Length : 0);
		ArrayListMP<int> collIndices = data.OpaqueMesh.m_CollIndices;
		obj[10] = ((collIndices != null) ? collIndices.Items.Length : 0);
		obj[11] = data.TerrainMesh.Vertices.Items.Length;
		obj[12] = data.TerrainMesh.Indices.Items.Length;
		obj[13] = data.TerrainMesh.Uvs.Items.Length;
		obj[14] = data.TerrainMesh.UvsCrack.Items.Length;
		ArrayListMP<Vector2> uvs3 = data.TerrainMesh.Uvs3;
		obj[15] = ((uvs3 != null) ? uvs3.Items.Length : 0);
		ArrayListMP<Vector2> uvs4 = data.TerrainMesh.Uvs4;
		obj[16] = ((uvs4 != null) ? uvs4.Items.Length : 0);
		ArrayListMP<Color> colorVertices2 = data.TerrainMesh.ColorVertices;
		obj[17] = ((colorVertices2 != null) ? colorVertices2.Items.Length : 0);
		ArrayListMP<Vector3> normals2 = data.TerrainMesh.m_Normals;
		obj[18] = ((normals2 != null) ? normals2.Items.Length : 0);
		ArrayListMP<Vector4> tangents2 = data.TerrainMesh.m_Tangents;
		obj[19] = ((tangents2 != null) ? tangents2.Items.Length : 0);
		ArrayListMP<Vector3> collVertices2 = data.TerrainMesh.m_CollVertices;
		obj[20] = ((collVertices2 != null) ? collVertices2.Items.Length : 0);
		ArrayListMP<int> collIndices2 = data.TerrainMesh.m_CollIndices;
		obj[21] = ((collIndices2 != null) ? collIndices2.Items.Length : 0);
		obj[22] = data.TerrainMesh.submeshes?.Count ?? 0;
		return string.Format("\r\nOpaque\r\nVertices: {0}\r\nIndices: {1}\r\nUvs1: {2}\r\nUvs2: {3}\r\nUvs3: {4}\r\nUvs4: {5}\r\nColorVertices: {6}\r\nm_Normals: {7}\r\nm_Tangents: {8}\r\nm_CollVertices: {9}\r\nm_CollIndices: {10}\r\n\r\nTerrain\r\nVertices: {11}\r\nIndices: {12}\r\nUvs1: {13}\r\nUvs2: {14}\r\nUvs3: {15}\r\nUvs4: {16}\r\nColorVertices: {17}\r\nm_Normals: {18}\r\nm_Tangents: {19}\r\nm_CollVertices: {20}\r\nm_CollIndices: {21}\r\nsubmeshes: {22}\r\n\r\n", obj);
	}

	public static DyMeshData AddToCache(DyMeshData data)
	{
		if (data == null)
		{
			return null;
		}
		ActiveItems--;
		if (TotalItems < DynamicMeshSettings.MaxDyMeshData)
		{
			if (Cache.Any([PublicizedFrom(EAccessModifier.Internal)] (DyMeshData d) => d.Name == data.Name))
			{
				Log.Error("duplicate in cache: " + data.Name);
				return null;
			}
			data.Reset();
			Cache.Enqueue(data);
		}
		return null;
	}

	public static DyMeshData GetFromCache()
	{
		if (!Cache.TryDequeue(out var result))
		{
			if (TotalItems >= DynamicMeshSettings.MaxDyMeshData)
			{
				return null;
			}
			result = Create();
		}
		ActiveItems++;
		return result;
	}

	public static DyMeshData Create(int opaqueSize = 32000, int terrainSize = 32000)
	{
		DyMeshData obj = new DyMeshData
		{
			OpaqueMesh = new VoxelMesh(0, opaqueSize),
			TerrainMesh = new VoxelMeshTerrain(5, terrainSize)
		};
		int num = ++InstanceCount;
		obj.Name = num.ToString();
		return obj;
	}
}
