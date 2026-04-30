using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class DynamicMeshVoxelRegionLoad
{
	public static ConcurrentDictionary<int, int> TileLinks = new ConcurrentDictionary<int, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Bounds> bounds = new List<Bounds>(10);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector3i WorldPosition { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<VoxelMesh> Meshes { get; set; }

	public static DynamicMeshVoxelRegionLoad Create(Vector3i position, List<VoxelMesh> meshes)
	{
		return new DynamicMeshVoxelRegionLoad
		{
			WorldPosition = position,
			Meshes = meshes
		};
	}

	public IEnumerator CreateMeshCoroutine(DynamicMeshRegion region)
	{
		_ = DateTime.Now;
		GameObject newRegionObject = DynamicMeshItem.GetRegionMeshRendererFromPool();
		newRegionObject.transform.position = new Vector3(0f, 0f, 0f);
		newRegionObject.SetActive(value: false);
		DateTime start = DateTime.Now;
		foreach (VoxelMesh mesh in Meshes)
		{
			MeshTiming time = new MeshTiming();
			if (mesh is VoxelMeshTerrain)
			{
				CreateTerrainGo(mesh as VoxelMeshTerrain, newRegionObject, time);
				if ((DateTime.Now - start).TotalMilliseconds > 3.0)
				{
					start = DateTime.Now;
					yield return null;
				}
			}
			else
			{
				yield return DynamicMeshManager.Instance.StartCoroutine(CreateOpaqueGo(mesh, newRegionObject, time));
				start = DateTime.Now;
			}
		}
		region.RegionObject = newRegionObject;
		region.IsMeshLoaded = true;
		region.SetPosition();
		region.SetVisibleNew(region.VisibleChunks == 0 || !region.InBuffer, "vox region coroutine finished");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CreateOpaqueGo(VoxelMesh vm, GameObject parent, MeshTiming time)
	{
		if (vm.Vertices.Count > 0)
		{
			GameObject itemMeshRendererFromPool = DynamicMeshItem.GetItemMeshRendererFromPool();
			itemMeshRendererFromPool.name = "O";
			itemMeshRendererFromPool.transform.parent = parent.transform;
			itemMeshRendererFromPool.transform.localPosition = Vector3.zero;
			MeshFilter component = itemMeshRendererFromPool.GetComponent<MeshFilter>();
			Mesh mesh = ((!(component.sharedMesh == null)) ? component.sharedMesh : new Mesh());
			mesh.indexFormat = ((vm.Vertices.Count >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			component.sharedMesh = mesh;
			yield return copyToMesh(mesh, vm.Vertices, vm.Indices, vm.Uvs, vm.UvsCrack, vm.Normals, vm.Tangents, vm.ColorVertices, time);
		}
	}

	public IEnumerator copyToMesh(Mesh _mesh, ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector2> _uvs, ArrayListMP<Vector2> _uvCracks, ArrayListMP<Vector3> _normals, ArrayListMP<Vector4> _tangents, ArrayListMP<Color> _colorVertices, MeshTiming time)
	{
		DateTime start = DateTime.Now;
		time.Reset();
		MeshUnsafeCopyHelper.CopyVertices(_vertices, _mesh);
		time.CopyVerts = time.GetTime();
		if ((DateTime.Now - start).TotalMilliseconds > 3.0)
		{
			start = DateTime.Now;
			yield return null;
		}
		if (_uvs.Count > 0)
		{
			time.Reset();
			MeshUnsafeCopyHelper.CopyUV(_uvs, _mesh);
			time.CopyUv = time.GetTime();
			if ((DateTime.Now - start).TotalMilliseconds > 3.0)
			{
				start = DateTime.Now;
				yield return null;
			}
			time.Reset();
			if (_uvCracks.Items != null)
			{
				MeshUnsafeCopyHelper.CopyUV2(_uvCracks, _mesh);
			}
			time.CopyUv2 = time.GetTime();
			if ((DateTime.Now - start).TotalMilliseconds > 3.0)
			{
				start = DateTime.Now;
				yield return null;
			}
		}
		time.Reset();
		MeshUnsafeCopyHelper.CopyColors(_colorVertices, _mesh);
		time.CopyColours = time.GetTime();
		if ((DateTime.Now - start).TotalMilliseconds > 3.0)
		{
			start = DateTime.Now;
			yield return null;
		}
		time.Reset();
		MeshUnsafeCopyHelper.CopyTriangles(_indices, _mesh);
		time.CopyTriangles = time.GetTime();
		if ((DateTime.Now - start).TotalMilliseconds > 3.0)
		{
			start = DateTime.Now;
			yield return null;
		}
		if (_normals.Count == 0)
		{
			Log.Error(WorldPosition.ToString() + " mesh normals zero on opaque");
			_mesh.RecalculateNormals();
		}
		else
		{
			if (_vertices.Count != _normals.Count)
			{
				Log.Error("ERROR: Vertices.Count ({0}) != Normals.Count ({1})", _vertices.Count, _normals.Count);
			}
			time.Reset();
			MeshUnsafeCopyHelper.CopyNormals(_normals, _mesh);
			time.CopyNormals = time.GetTime();
			if ((DateTime.Now - start).TotalMilliseconds > 3.0)
			{
				start = DateTime.Now;
				yield return null;
			}
		}
		if (_uvs.Count > 0)
		{
			if (_tangents.Count == 0)
			{
				Log.Error(WorldPosition.ToString() + " Tangents empty for region!");
			}
			if (_vertices.Count != _tangents.Count)
			{
				Log.Error("ERROR: Vertices.Count ({0}) != tangents.Count ({1})", _vertices.Count, _tangents.Count);
			}
			else
			{
				time.Reset();
				MeshUnsafeCopyHelper.CopyTangents(_tangents, _mesh);
				time.CopyTangents = time.GetTime();
				if ((DateTime.Now - start).TotalMilliseconds > 3.0)
				{
					_ = DateTime.Now;
					yield return null;
				}
			}
		}
		time.Reset();
		GameUtils.SetMeshVertexAttributes(_mesh);
		_mesh.UploadMeshData(markNoLongerReadable: false);
		time.UploadMesh = time.GetTime();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateTerrainGo(VoxelMeshTerrain terrain, GameObject parent, MeshTiming time)
	{
		if (terrain != null && terrain.Vertices != null && terrain.Vertices.Count != 0)
		{
			GameObject terrainMeshRendererFromPool = DynamicMeshItem.GetTerrainMeshRendererFromPool();
			terrainMeshRendererFromPool.name = "T";
			terrainMeshRendererFromPool.transform.parent = parent.transform;
			terrainMeshRendererFromPool.transform.localPosition = Vector3.zero;
			terrainMeshRendererFromPool.SetActive(value: true);
			MeshFilter component = terrainMeshRendererFromPool.GetComponent<MeshFilter>();
			Mesh mesh = ((!(component.sharedMesh == null)) ? component.sharedMesh : new Mesh());
			mesh.indexFormat = ((terrain.Vertices.Count >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			component.sharedMesh = mesh;
			DynamicMeshVoxelLoad.CopyTerrain(terrain, mesh, component, time, null);
		}
	}

	public static DynamicMeshLoadResult LoadRegionFromFile(BinaryReader reader, DyMeshRegionLoadRequest load)
	{
		DynamicMeshLoadResult dynamicMeshLoadResult = ReadVoxelMesh(reader, load.OpaqueMesh);
		if (dynamicMeshLoadResult != DynamicMeshLoadResult.Ok)
		{
			Log.Warning("Region file was corrupted. Adding for regeneration");
			Vector3i worldPosFromKey = DynamicMeshUnity.GetWorldPosFromKey(load.Key);
			DynamicMeshThread.AddRegionUpdateData(worldPosFromKey.x, worldPosFromKey.z, isUrgent: false);
			return dynamicMeshLoadResult;
		}
		dynamicMeshLoadResult = ReadTerrainMesh(reader, load.TerrainMesh);
		if (dynamicMeshLoadResult != DynamicMeshLoadResult.Ok)
		{
			Log.Warning("Region file was corrupted. Adding for regeneration");
			Vector3i worldPosFromKey2 = DynamicMeshUnity.GetWorldPosFromKey(load.Key);
			DynamicMeshThread.AddRegionUpdateData(worldPosFromKey2.x, worldPosFromKey2.z, isUrgent: false);
			return dynamicMeshLoadResult;
		}
		if (load.OpaqueMesh.Tangents.Count != load.OpaqueMesh.Vertices.Count)
		{
			MeshCalculations.CalculateMeshTangents(load.OpaqueMesh.Vertices, load.OpaqueMesh.Triangles, load.OpaqueMesh.Normals, load.OpaqueMesh.Uvs, load.OpaqueMesh.Tangents);
		}
		return DynamicMeshLoadResult.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DynamicMeshLoadResult ReadVoxelMesh(BinaryReader reader, MeshLists cache)
	{
		if (reader.ReadInt32() == 0)
		{
			return DynamicMeshLoadResult.Ok;
		}
		UVRectTiling[] uvMapping = MeshDescription.meshes[0].textureAtlas.uvMapping;
		List<Vector3> vertices = cache.Vertices;
		List<Vector2> uvs = cache.Uvs;
		List<Color> colours = cache.Colours;
		List<int> triangles = cache.Triangles;
		List<Vector3> normals = cache.Normals;
		List<Vector4> tangents = cache.Tangents;
		int num = 0;
		uint num2 = 0u;
		try
		{
			num2 = reader.ReadUInt32();
			num = 1;
			if (num2 == 0)
			{
				return DynamicMeshLoadResult.Ok;
			}
			if (reader.BaseStream.Position + num2 * 6 > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			vertices.Capacity = (int)Math.Max(vertices.Capacity, num2);
			for (uint num3 = 0u; num3 < num2; num3++)
			{
				vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			num = 2;
			uint num4 = reader.ReadUInt32();
			uvs.Capacity = (int)Math.Max(uvs.Capacity, num4);
			colours.Capacity = (int)Math.Max(colours.Capacity, num4);
			if (reader.BaseStream.Position + num4 * 8 > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			num = 3;
			for (uint num5 = 0u; num5 < num4; num5++)
			{
				int num6 = reader.ReadInt16();
				int num7 = reader.ReadByte();
				int num8 = ((num6 < uvMapping.Length) ? (uvMapping[num6].index + num7) : 0);
				bool flag = reader.ReadBoolean();
				uvs.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
				colours.Add(new Color(0f, num8, 0f, flag ? 1 : 0));
			}
			num = 4;
			uint num9 = reader.ReadUInt32();
			if (reader.BaseStream.Position + num9 * 4 > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			triangles.Capacity = (int)Math.Max(triangles.Capacity, num9);
			num = 5;
			for (uint num10 = 0u; num10 < num9; num10++)
			{
				triangles.Add(reader.ReadInt32());
			}
			num = 6;
			uint num11 = reader.ReadUInt32();
			if (reader.BaseStream.Position + num11 * 6 > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			normals.Capacity = (int)Math.Max(normals.Capacity, num11);
			for (uint num12 = 0u; num12 < num11; num12++)
			{
				normals.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			num = 7;
			uint num13 = reader.ReadUInt32();
			if (reader.BaseStream.Position + num13 * 8 > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			tangents.Capacity = (int)Math.Max(tangents.Capacity, num13);
			for (uint num14 = 0u; num14 < num13; num14++)
			{
				tangents.Add(new Vector4((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			return DynamicMeshLoadResult.Ok;
		}
		catch (Exception ex)
		{
			Log.Out("Read voxel mesh error:  verts: " + num2 + " stage:  " + num + "  message: " + ex.Message);
		}
		return DynamicMeshLoadResult.Error;
	}

	public static DynamicMeshLoadResult ReadTerrainMesh(BinaryReader reader, MeshLists cache)
	{
		if (reader.ReadInt32() == 0)
		{
			return DynamicMeshLoadResult.Ok;
		}
		List<Vector3> vertices = cache.Vertices;
		List<int> triangles = cache.Triangles;
		List<Color> colours = cache.Colours;
		List<Vector2> uvs = cache.Uvs;
		List<Vector2> uvs2 = cache.Uvs2;
		List<Vector2> uvs3 = cache.Uvs3;
		List<Vector2> uvs4 = cache.Uvs4;
		List<Vector3> normals = cache.Normals;
		List<Vector4> tangents = cache.Tangents;
		uint num = reader.ReadUInt32();
		if (reader.BaseStream.Position + num * 6 > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		vertices.Capacity = (int)Math.Max(vertices.Capacity, num);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		uint num3 = reader.ReadUInt32();
		if (reader.BaseStream.Position + num3 * 4 * 4 > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		uvs.Capacity = (int)Math.Max(uvs.Capacity, num3);
		uvs2.Capacity = (int)Math.Max(uvs2.Capacity, num3);
		uvs3.Capacity = (int)Math.Max(uvs3.Capacity, num3);
		uvs4.Capacity = (int)Math.Max(uvs4.Capacity, num3);
		for (uint num4 = 0u; num4 < num3; num4++)
		{
			uvs.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
		}
		num3 = reader.ReadUInt32();
		for (uint num5 = 0u; num5 < num3; num5++)
		{
			uvs2.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
		}
		num3 = reader.ReadUInt32();
		for (uint num6 = 0u; num6 < num3; num6++)
		{
			uvs3.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
		}
		num3 = reader.ReadUInt32();
		for (uint num7 = 0u; num7 < num3; num7++)
		{
			uvs4.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
		}
		uint num8 = reader.ReadUInt32();
		for (int i = 0; i < num8; i++)
		{
			uint num9 = reader.ReadUInt32();
			if (reader.BaseStream.Position + num9 * 4 > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			for (uint num10 = 0u; num10 < num9; num10++)
			{
				triangles.Add(reader.ReadInt32());
			}
		}
		uint num11 = reader.ReadUInt32();
		if (reader.BaseStream.Position + num11 * 8 > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		colours.Capacity = (int)Math.Max(colours.Capacity, num11);
		for (uint num12 = 0u; num12 < num11; num12++)
		{
			colours.Add(new Color((float)(int)reader.ReadUInt16() / 10000f + 1f, (float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
		}
		uint num13 = reader.ReadUInt32();
		if (reader.BaseStream.Position + num13 * 6 > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		normals.Capacity = (int)Math.Max(normals.Capacity, num13);
		for (uint num14 = 0u; num14 < num13; num14++)
		{
			normals.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		uint num15 = reader.ReadUInt32();
		if (reader.BaseStream.Position + num15 * 8 > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		tangents.Capacity = (int)Math.Max(tangents.Capacity, num15);
		for (uint num16 = 0u; num16 < num15; num16++)
		{
			tangents.Add(new Vector4((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		return DynamicMeshLoadResult.Ok;
	}

	public static void SaveRegionToFile(BinaryWriter _bw, VoxelMesh meshes, VoxelMeshTerrain terrain, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder, DynamicMeshRegion region)
	{
		WriteOpaqueMesh(_bw, meshes);
		WriteTerrainVoxelMeshesToDisk(_bw, terrain);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteTerrainVoxelMeshesToDisk(BinaryWriter writer, VoxelMeshTerrain mesh)
	{
		int num = ((mesh.Vertices.Count != 0) ? 1 : 0);
		writer.Write(num);
		if (num == 0)
		{
			return;
		}
		int count = mesh.Vertices.Count;
		int count2 = mesh.Indices.Count;
		int num2 = mesh.submeshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (TerrainSubMesh e) => e.triangles.Count);
		int count3 = mesh.ColorVertices.Count;
		if (count != count3)
		{
			Log.Error("Invalid colours");
		}
		int count4 = mesh.Uvs.Count;
		int count5 = mesh.UvsCrack.Count;
		int count6 = mesh.Uvs3.Count;
		int count7 = mesh.Uvs4.Count;
		writer.Write((uint)count);
		ArrayListMP<Vector3> vertices = mesh.Vertices;
		for (int num3 = 0; num3 < vertices.Count; num3++)
		{
			writer.Write((short)((double)vertices[num3].x * 100.0));
			writer.Write((short)((double)vertices[num3].y * 100.0));
			writer.Write((short)((double)vertices[num3].z * 100.0));
		}
		writer.Write((uint)count4);
		ArrayListMP<Vector2> uvs = mesh.Uvs;
		for (int num4 = 0; num4 < uvs.Count; num4++)
		{
			writer.Write((ushort)((double)uvs[num4].x * 10000.0));
			writer.Write((ushort)((double)uvs[num4].y * 10000.0));
		}
		writer.Write((uint)count5);
		ArrayListMP<Vector2> uvsCrack = mesh.UvsCrack;
		for (int num5 = 0; num5 < uvsCrack.Count; num5++)
		{
			writer.Write((ushort)((double)uvsCrack[num5].x * 10000.0));
			writer.Write((ushort)((double)uvsCrack[num5].y * 10000.0));
		}
		writer.Write((uint)count6);
		ArrayListMP<Vector2> uvs2 = mesh.Uvs3;
		for (int num6 = 0; num6 < uvs2.Count; num6++)
		{
			writer.Write((ushort)((double)uvs2[num6].x * 10000.0));
			writer.Write((ushort)((double)uvs2[num6].y * 10000.0));
		}
		writer.Write((uint)count7);
		ArrayListMP<Vector2> uvs3 = mesh.Uvs4;
		for (int num7 = 0; num7 < uvs3.Count; num7++)
		{
			writer.Write((ushort)((double)uvs3[num7].x * 10000.0));
			writer.Write((ushort)((double)uvs3[num7].y * 10000.0));
		}
		int value = 1;
		writer.Write((uint)value);
		writer.Write((uint)count2);
		int num8 = 0;
		if (count2 % 3 != 0 || num2 % 3 != 0)
		{
			Log.Out("Weird triangles");
		}
		if (mesh.submeshes.Count > 0)
		{
			foreach (TerrainSubMesh submesh in mesh.submeshes)
			{
				for (int num9 = 0; num9 < submesh.triangles.Count; num9++)
				{
					writer.Write(submesh.triangles[num9] + num8);
				}
			}
		}
		else
		{
			for (int num10 = 0; num10 < mesh.Indices.Count; num10++)
			{
				writer.Write(mesh.Indices[num10] + num8);
			}
		}
		num8 += mesh.Vertices.Count;
		writer.Write((uint)count);
		ArrayListMP<Color> colorVertices = mesh.ColorVertices;
		for (int num11 = 0; num11 < colorVertices.Count; num11++)
		{
			Color color = colorVertices[num11];
			writer.Write((ushort)(color.r * 10000f));
			writer.Write((ushort)(color.g * 10000f));
			writer.Write((ushort)(color.b * 10000f));
			writer.Write((ushort)(color.a * 10000f));
		}
		int count8 = mesh.Normals.Count;
		writer.Write((uint)count8);
		ArrayListMP<Vector3> normals = mesh.Normals;
		for (int num12 = 0; num12 < normals.Count; num12++)
		{
			Vector3 vector = normals[num12];
			writer.Write((short)((double)vector.x * 100.0));
			writer.Write((short)((double)vector.y * 100.0));
			writer.Write((short)((double)vector.z * 100.0));
		}
		int count9 = mesh.Tangents.Count;
		writer.Write((uint)count9);
		ArrayListMP<Vector4> tangents = mesh.Tangents;
		for (int num13 = 0; num13 < tangents.Count; num13++)
		{
			Vector4 vector2 = tangents[num13];
			writer.Write((short)((double)vector2.x * 100.0));
			writer.Write((short)((double)vector2.y * 100.0));
			writer.Write((short)((double)vector2.z * 100.0));
			writer.Write((short)((double)vector2.w * 100.0));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteOpaqueMesh(BinaryWriter writer, VoxelMesh mesh)
	{
		int num = ((mesh.Vertices.Count != 0) ? 1 : 0);
		writer.Write(num);
		if (num == 0)
		{
			return;
		}
		UVRectTiling[] uvMapping = MeshDescription.meshes[0].textureAtlas.uvMapping;
		if (!DynamicMeshManager.Allow32BitMeshes)
		{
			Log.Error("Can't write combined meshes to disk unless in 32 bit mode");
			return;
		}
		int count = mesh.Vertices.Count;
		int count2 = mesh.Indices.Count;
		writer.Write((uint)count);
		ArrayListMP<Vector3> vertices = mesh.Vertices;
		for (int i = 0; i < vertices.Count; i++)
		{
			writer.Write((short)((double)vertices[i].x * 100.0));
			writer.Write((short)((double)vertices[i].y * 100.0));
			writer.Write((short)((double)vertices[i].z * 100.0));
		}
		writer.Write((uint)count);
		ArrayListMP<Color> colorVertices = mesh.ColorVertices;
		ArrayListMP<Vector2> uvs = mesh.Uvs;
		for (int j = 0; j < colorVertices.Count; j++)
		{
			int num2 = (int)colorVertices[j].g;
			int value = -1;
			if (!TileLinks.TryGetValue(num2, out value))
			{
				for (int k = 0; k < uvMapping.Length; k++)
				{
					UVRectTiling uVRectTiling = uvMapping[k];
					if (uVRectTiling.index == num2 || k + 1 >= uvMapping.Length || (float)uVRectTiling.index + uVRectTiling.uv.width * uVRectTiling.uv.height > (float)num2)
					{
						value = k;
						TileLinks.TryAdd(num2, value);
						break;
					}
				}
				if (value == -1)
				{
					value = 0;
					TileLinks.TryAdd(num2, value);
				}
			}
			if (value == -1)
			{
				value = 0;
			}
			writer.Write((short)value);
			writer.Write((byte)(num2 - uvMapping[value].index));
			bool value2 = (double)colorVertices[j].a > 0.5;
			writer.Write(value2);
			writer.Write((ushort)((double)uvs[j].x * 10000.0));
			writer.Write((ushort)((double)uvs[j].y * 10000.0));
		}
		writer.Write((uint)count2);
		ArrayListMP<int> indices = mesh.Indices;
		for (int l = 0; l < indices.Count; l++)
		{
			writer.Write(indices[l]);
		}
		int count3 = mesh.Normals.Count;
		writer.Write((uint)count3);
		ArrayListMP<Vector3> normals = mesh.Normals;
		for (int m = 0; m < normals.Count; m++)
		{
			Vector3 vector = normals[m];
			writer.Write((short)((double)vector.x * 100.0));
			writer.Write((short)((double)vector.y * 100.0));
			writer.Write((short)((double)vector.z * 100.0));
		}
		int count4 = mesh.Tangents.Count;
		writer.Write((uint)count4);
		ArrayListMP<Vector4> tangents = mesh.Tangents;
		for (int n = 0; n < tangents.Count; n++)
		{
			Vector4 vector2 = tangents[n];
			writer.Write((short)((double)vector2.x * 100.0));
			writer.Write((short)((double)vector2.y * 100.0));
			writer.Write((short)((double)vector2.z * 100.0));
			writer.Write((short)((double)vector2.w * 100.0));
		}
	}
}
