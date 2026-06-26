using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Rendering;

public class DynamicMeshVoxelLoad
{
	public static ConcurrentQueue<VoxelMeshLayer> LayerCache = new ConcurrentQueue<VoxelMeshLayer>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshItem Item { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public DyMeshData Data { get; set; }

	public void DisposeMeshes()
	{
		Data = DyMeshData.AddToCache(Data);
	}

	public IEnumerator CreateMeshCoroutine(DynamicMeshItem item)
	{
		DateTime start = DateTime.Now;
		if (item.Key != Item.Key)
		{
			Log.Error("mismatching items in Mesh creation");
		}
		GameObject child = DynamicMeshItem.GetItemMeshRendererFromPool();
		child.SetActive(value: false);
		bool hasMeshes = false;
		VoxelMesh opaqueMesh = Data.OpaqueMesh;
		if (opaqueMesh.Vertices.Count > 0)
		{
			hasMeshes = true;
			MeshFilter component = child.GetComponent<MeshFilter>();
			Mesh mesh = ((!(component.sharedMesh == null)) ? component.sharedMesh : new Mesh());
			mesh.indexFormat = ((opaqueMesh.Vertices.Count > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			component.sharedMesh = mesh;
			opaqueMesh.copyToMesh(mesh, opaqueMesh.Vertices, opaqueMesh.Indices, opaqueMesh.Uvs, opaqueMesh.UvsCrack, opaqueMesh.Normals, opaqueMesh.Tangents, opaqueMesh.ColorVertices);
			if ((DateTime.Now - start).TotalMilliseconds > 3.0)
			{
				start = DateTime.Now;
				yield return null;
			}
		}
		if ((DateTime.Now - start).TotalMilliseconds > 3.0)
		{
			start = DateTime.Now;
			yield return null;
		}
		VoxelMeshTerrain terrainMesh = Data.TerrainMesh;
		if (terrainMesh != null && terrainMesh.Vertices != null && terrainMesh.Vertices.Count != 0)
		{
			hasMeshes = true;
			GameObject terrainMeshRendererFromPool = DynamicMeshItem.GetTerrainMeshRendererFromPool();
			terrainMeshRendererFromPool.SetActive(value: true);
			terrainMeshRendererFromPool.transform.parent = child.transform;
			MeshFilter component2 = terrainMeshRendererFromPool.GetComponent<MeshFilter>();
			Mesh mesh2 = ((!(component2.sharedMesh == null)) ? component2.sharedMesh : new Mesh());
			mesh2.indexFormat = ((mesh2.vertexCount > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			component2.sharedMesh = mesh2;
			CopyTerrain(terrainMesh, mesh2, component2, new MeshTiming(), Item);
			if ((DateTime.Now - start).TotalMilliseconds > 3.0)
			{
				_ = DateTime.Now;
				yield return null;
			}
		}
		if (hasMeshes)
		{
			item.ChunkObject = child;
		}
		else
		{
			DynamicMeshManager.MeshDestroy(child);
		}
	}

	public static void CopyTerrain(VoxelMeshTerrain terrain, Mesh mesh, MeshFilter filter, MeshTiming time, DynamicMeshItem item)
	{
		time.Reset();
		MeshUnsafeCopyHelper.CopyVertices(terrain.Vertices, mesh);
		time.CopyVerts = time.GetTime();
		time.Reset();
		MeshUnsafeCopyHelper.CopyUV(terrain.Uvs, mesh);
		time.CopyUv = time.time;
		time.Reset();
		MeshUnsafeCopyHelper.CopyUV2(terrain.UvsCrack, mesh);
		time.CopyUv2 = time.time;
		if (terrain?.Uvs3.Items != null)
		{
			time.Reset();
			MeshUnsafeCopyHelper.CopyUV3(terrain.Uvs3, mesh);
			time.CopyUv3 = time.time;
		}
		if (terrain?.Uvs4.Items != null)
		{
			time.Reset();
			MeshUnsafeCopyHelper.CopyUV4(terrain.Uvs4, mesh);
			time.CopyUv4 = time.time;
		}
		time.Reset();
		MeshUnsafeCopyHelper.CopyColors(terrain.ColorVertices, mesh);
		time.CopyColours = time.time;
		time.Reset();
		if (terrain.Indices.Count > 0)
		{
			MeshUnsafeCopyHelper.CopyTriangles(terrain.Indices, mesh);
		}
		else
		{
			mesh.subMeshCount = ((terrain.Indices.Count > 0) ? 1 : terrain.submeshes.Count);
			for (int i = 0; i < terrain.submeshes.Count; i++)
			{
				MeshUnsafeCopyHelper.CopyTriangles(terrain.submeshes[i].triangles, mesh, i);
			}
		}
		time.CopyTriangles = time.time;
		if (terrain.Normals.Count == 0)
		{
			time.Reset();
			mesh.RecalculateNormals();
			time.NormalRecalc = time.time;
		}
		else
		{
			time.Reset();
			MeshUnsafeCopyHelper.CopyNormals(terrain.Normals, mesh);
			time.CopyNormals = time.time;
		}
		time.Reset();
		time.CopyTangents = time.time;
		time.Reset();
		GameUtils.SetMeshVertexAttributes(mesh);
		mesh.UploadMeshData(markNoLongerReadable: false);
		time.UploadMesh = time.time;
		Renderer component = filter.GetComponent<Renderer>();
		int num = component.sharedMaterials.Length;
		if (!DynamicMeshFile.TerrainSharedMaterials.ContainsKey(num))
		{
			Material[] array = new Material[num];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = MeshDescription.meshes[5].material;
			}
			DynamicMeshFile.TerrainSharedMaterials.Add(num, array);
		}
		component.sharedMaterials = DynamicMeshFile.TerrainSharedMaterials[num];
	}

	public static DynamicMeshVoxelLoad Create(DynamicMeshItem item, DyMeshData data)
	{
		return new DynamicMeshVoxelLoad
		{
			Item = item,
			Data = data
		};
	}
}
