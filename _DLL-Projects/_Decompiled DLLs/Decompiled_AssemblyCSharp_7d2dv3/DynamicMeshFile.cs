using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Noemax.GZip;
using UnityEngine;
using UnityEngine.Rendering;

public class DynamicMeshFile
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static MicroStopwatch stop = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Bounds> bounds = new List<Bounds>(10);

	public static ConcurrentDictionary<int, int> TileLinks = new ConcurrentDictionary<int, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 TerrainMeshOffset = new Vector3(0f, -0.2f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material _itemMaterial = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material _RegionMaterial = null;

	public static int ReadMeshMax = 2;

	public static WaitForSeconds ReadMeshWait = null;

	public static string MeshLocation;

	public static DateTime ItemMin = new DateTime(2000, 1, 1, 0, 0, 0);

	public static WaitForSeconds WaitForSecond = new WaitForSeconds(1f);

	public static WaitForSeconds WaitForPoint1Second = new WaitForSeconds(0.1f);

	public static ConcurrentQueue<VoxelMesh> VoxelMeshPool = new ConcurrentQueue<VoxelMesh>();

	public static ConcurrentQueue<VoxelMeshTerrain> VoxelTerrainMeshPool = new ConcurrentQueue<VoxelMeshTerrain>();

	public static WaitForSeconds WaitPointOne = new WaitForSeconds(0.1f);

	public static WaitForSeconds WaitPointTwo = new WaitForSeconds(0.2f);

	public static WaitForSeconds WaitOne = new WaitForSeconds(1f);

	public static Vector3i CurrentlyLoadingRegionPosition = new Vector3i(int.MaxValue, 0, 0);

	public static Dictionary<int, Material[]> TerrainSharedMaterials = new Dictionary<int, Material[]>();

	public static DynamicMeshContainer CurrentlyLoadingItem;

	public static string DisabledImpostersFile => MeshLocation + "DisabledImposters.list";

	public static void CleanUp()
	{
		MeshLists.MeshListCache.Clear();
		MeshLists.LastLargest = 0;
	}

	public static Bounds GetBoundsFromVerts(ArrayListMP<Vector3> verts)
	{
		float num = verts[0].x;
		float num2 = verts[0].x;
		float num3 = verts[0].y;
		float num4 = verts[0].y;
		float num5 = verts[0].z;
		float num6 = verts[0].z;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.x);
			num2 = Math.Max(num2, vector.x);
			num3 = Math.Min(num3, vector.y);
			num4 = Math.Max(num4, vector.y);
			num5 = Math.Min(num5, vector.z);
			num6 = Math.Max(num6, vector.z);
		}
		Vector3 vector2 = new Vector3(num, num3, num5);
		Vector3 vector3 = new Vector3(num2, num4, num6);
		Bounds result = default(Bounds);
		result.size = vector3 - vector2;
		result.center = result.size / 2f + vector2;
		return result;
	}

	public static Bounds GetBoundsFromVerts(List<Vector3> verts)
	{
		float num = verts[0].x;
		float num2 = verts[0].x;
		float num3 = verts[0].y;
		float num4 = verts[0].y;
		float num5 = verts[0].z;
		float num6 = verts[0].z;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.x);
			num2 = Math.Max(num2, vector.x);
			num3 = Math.Min(num3, vector.y);
			num4 = Math.Max(num4, vector.y);
			num5 = Math.Min(num5, vector.z);
			num6 = Math.Max(num6, vector.z);
		}
		Vector3 vector2 = new Vector3(num, num3, num5);
		Vector3 vector3 = new Vector3(num2, num4, num6);
		Bounds result = default(Bounds);
		result.size = vector3 - vector2;
		result.center = result.size / 2f + vector2;
		return result;
	}

	public static Bounds GetBoundsFromVerts(Vector3[] verts)
	{
		float num = verts[0].x;
		float num2 = verts[0].x;
		float num3 = verts[0].y;
		float num4 = verts[0].y;
		float num5 = verts[0].z;
		float num6 = verts[0].z;
		for (int i = 0; i < verts.Length; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.x);
			num2 = Math.Max(num2, vector.x);
			num3 = Math.Min(num3, vector.y);
			num4 = Math.Max(num4, vector.y);
			num5 = Math.Min(num5, vector.z);
			num6 = Math.Max(num6, vector.z);
		}
		Vector3 vector2 = new Vector3(num, num3, num5);
		Vector3 vector3 = new Vector3(num2, num4, num6);
		Bounds result = default(Bounds);
		result.size = vector3 - vector2;
		result.center = result.size / 2f + vector2;
		return result;
	}

	public static Bounds GetBoundsFromVertsJustY(ArrayListMP<Vector3> verts, Bounds bounds)
	{
		float num = verts[0].y;
		float num2 = verts[0].y;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.y);
			num2 = Math.Max(num2, vector.y);
		}
		Vector3 min = new Vector3(0f, num, 0f);
		Vector3 max = new Vector3(0f, num2, 0f);
		bounds.SetMinMax(min, max);
		return bounds;
	}

	public static Bounds GetBoundsFromVerts(ArrayListMP<Vector3> verts, List<Bounds> boundsList, int index)
	{
		if (index == boundsList.Count)
		{
			boundsList.Add(default(Bounds));
		}
		while (index >= boundsList.Count)
		{
			boundsList.Add(default(Bounds));
		}
		Bounds result = boundsList[index];
		float num = verts[0].x;
		float num2 = verts[0].x;
		float num3 = verts[0].y;
		float num4 = verts[0].y;
		float num5 = verts[0].z;
		float num6 = verts[0].z;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.x);
			num2 = Math.Max(num2, vector.x);
			num3 = Math.Min(num3, vector.y);
			num4 = Math.Max(num4, vector.y);
			num5 = Math.Min(num5, vector.z);
			num6 = Math.Max(num6, vector.z);
		}
		Vector3 vector2 = new Vector3(num, num3, num5);
		Vector3 vector3 = new Vector3(num2, num4, num6);
		result.size = vector3 - vector2;
		result.center = result.size / 2f + vector2;
		return result;
	}

	public static VoxelMesh GetMeshFromPool()
	{
		return GetMeshFromPool(500);
	}

	public static VoxelMesh GetMeshFromPool(int size)
	{
		if (VoxelMeshPool.TryDequeue(out var result))
		{
			return result;
		}
		return new VoxelMesh(0, size);
	}

	public static VoxelMeshTerrain GetTerrainMeshFromPool()
	{
		return GetTerrainMeshFromPool(500);
	}

	public static VoxelMeshTerrain GetTerrainMeshFromPool(int size)
	{
		if (VoxelTerrainMeshPool.TryDequeue(out var result))
		{
			return result;
		}
		return new VoxelMeshTerrain(5, size);
	}

	public static void AddMeshToPool(VoxelMesh mesh)
	{
		if (mesh is VoxelMeshTerrain)
		{
			AddMeshToPool((VoxelMeshTerrain)mesh);
			return;
		}
		mesh.ClearMesh();
		VoxelMeshPool.Enqueue(mesh);
	}

	public static void AddMeshToPool(VoxelMeshTerrain mesh)
	{
		mesh.ClearMesh();
		VoxelTerrainMeshPool.Enqueue(mesh);
	}

	public static GameObject CreateMeshObject(string name, bool isRegion)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.material = GetOpaqueMaterial(isRegion);
		gameObject.isStatic = true;
		return gameObject;
	}

	public static GameObject CreateTerrainMeshObject(string name)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		gameObject.isStatic = true;
		return gameObject;
	}

	public static void LoadRegionGameObjectSync(string path, DynamicMeshRegion region)
	{
		if (!SdFile.Exists(path))
		{
			return;
		}
		DateTime now = DateTime.Now;
		while ((DateTime.Now - now).TotalSeconds < 10.0)
		{
			bool flag = false;
			try
			{
				using (SdFile.OpenRead(path))
				{
					flag = true;
				}
			}
			catch (Exception ex)
			{
				Log.Warning("Access file error: " + ex.Message);
				goto IL_0042;
			}
			break;
			IL_0042:
			if (!flag)
			{
				return;
			}
		}
		using Stream baseStream = GetReadStream(path);
		using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
		pooledBinaryReader.SetBaseStream(baseStream);
		int num = pooledBinaryReader.ReadInt32();
		int num2 = pooledBinaryReader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			Vector3i worldPos = new Vector3i(pooledBinaryReader.ReadInt32(), 0, pooledBinaryReader.ReadInt32());
			region.AddChunk(worldPos.x, worldPos.z);
			DynamicMeshManager.Instance.AddChunkStub(worldPos, region);
		}
		long ticks = pooledBinaryReader.ReadInt64();
		region.CreateDate = new DateTime(ticks);
		int num3 = pooledBinaryReader.ReadInt32();
		int y = pooledBinaryReader.ReadInt32();
		int num4 = pooledBinaryReader.ReadInt32();
		region.Rect = new Rect(num3, num4, num, num);
		region.WorldPosition = new Vector3i(num3, y, num4);
		int num5 = pooledBinaryReader.ReadInt32();
		if (num5 == 0)
		{
			if (region.MarkedForDeletion)
			{
				Log.Warning("Removing region. Delete file? " + region.ToDebugLocation());
			}
			else
			{
				region.MarkedForDeletion = true;
			}
			return;
		}
		GameObject regionMeshRendererFromPool = DynamicMeshItem.GetRegionMeshRendererFromPool();
		Vector3 position = ((region.RegionObject == null) ? Vector3.zero : region.RegionObject.transform.position);
		for (int j = 0; j < num5; j++)
		{
			GameObject gameObject = ((j == 0) ? regionMeshRendererFromPool : DynamicMeshItem.GetRegionMeshRendererFromPool());
			MeshFilter component = gameObject.GetComponent<MeshFilter>();
			Mesh mesh = component.mesh ?? new Mesh();
			if (j != 0)
			{
				gameObject.transform.parent = regionMeshRendererFromPool.transform;
			}
			gameObject.SetActive(value: false);
			ReadMesh(5, pooledBinaryReader, MeshDescription.meshes[0].textureAtlas.uvMapping, mesh, lockMesh: true, null);
			component.mesh = mesh;
			if (j != 0)
			{
				gameObject.SetActive(value: true);
			}
		}
		regionMeshRendererFromPool.transform.position = position;
		if (region.RegionObject != null)
		{
			DynamicMeshItem.AddToMeshPool(region.RegionObject);
		}
		region.RegionObject = regionMeshRendererFromPool;
		region.SetPosition();
		region.SetVisibleNew(region.VisibleChunks == 0 && !region.IsPlayerInRegion(), "load region sync finished");
		region.IsMeshLoaded = true;
		DynamicMeshManager.Instance.UpdateDynamicPrefabDecoratorRegions(region);
		if (pooledBinaryReader.BaseStream.Position == pooledBinaryReader.BaseStream.Length)
		{
			return;
		}
		num5 = pooledBinaryReader.ReadInt32();
		if (num5 > 0)
		{
			for (int k = 0; k < num5; k++)
			{
				GameObject terrainMeshRendererFromPool = DynamicMeshItem.GetTerrainMeshRendererFromPool();
				MeshFilter component2 = terrainMeshRendererFromPool.GetComponent<MeshFilter>();
				MeshRenderer component3 = terrainMeshRendererFromPool.GetComponent<MeshRenderer>();
				Mesh mesh2 = component2.mesh ?? new Mesh();
				terrainMeshRendererFromPool.transform.parent = region.RegionObject.transform;
				terrainMeshRendererFromPool.transform.localPosition = Vector3.zero;
				terrainMeshRendererFromPool.name = "T";
				ReadMeshTerrain(pooledBinaryReader, mesh2, component3, lockMesh: true);
				component2.mesh = mesh2;
				GameUtils.SetMeshVertexAttributes(mesh2);
				mesh2.UploadMeshData(markNoLongerReadable: true);
				terrainMeshRendererFromPool.SetActive(value: true);
			}
		}
	}

	public static Stream GetCreateStream(string path)
	{
		Stream stream = SdFile.Create(path);
		if (DynamicMeshManager.CompressFiles)
		{
			return new DeflateOutputStream(stream, 3, leaveOpen: false);
		}
		return stream;
	}

	public static Stream GetReadStream(string path)
	{
		Stream stream = SdFile.OpenRead(path);
		if (DynamicMeshManager.CompressFiles)
		{
			return new DeflateInputStream(stream);
		}
		return stream;
	}

	public static void WriteRegionHeaderData(DynamicMeshRegion region, int tryCount)
	{
		if (tryCount > 10)
		{
			Log.Error("Could not save region header: " + region.ToDebugLocation());
			return;
		}
		string meshLocation = MeshLocation;
		if (!SdDirectory.Exists(meshLocation))
		{
			SdDirectory.CreateDirectory(meshLocation);
		}
		string path = region.Path;
		try
		{
			using Stream baseStream = GetCreateStream(path);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			pooledBinaryWriter.Write(160);
			Vector3i[] array = region.LoadedChunks.Distinct().ToArray();
			pooledBinaryWriter.Write(array.Length);
			Vector3i[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Vector3i vector3i = array2[i];
				pooledBinaryWriter.Write(vector3i.x);
				pooledBinaryWriter.Write(vector3i.z);
			}
			pooledBinaryWriter.Write(region.CreateDate.Ticks);
			pooledBinaryWriter.Write(region.WorldPosition.x);
			pooledBinaryWriter.Write(region.WorldPosition.y);
			pooledBinaryWriter.Write(region.WorldPosition.z);
			pooledBinaryWriter.Write((byte)0);
			pooledBinaryWriter.Write(0);
			pooledBinaryWriter.Write(0);
		}
		catch (Exception ex)
		{
			tryCount++;
			Log.Warning("Write region header error: " + ex.Message + " attempt: " + tryCount);
			Thread.Sleep(500 * tryCount);
			WriteRegionHeaderData(region, tryCount);
		}
	}

	public static void WriteRegion(DynamicMeshRegion region, int tryCount)
	{
		if (tryCount > 5)
		{
			Vector3i worldPosition = region.WorldPosition;
			Log.Error("Could not save region: " + worldPosition.ToString());
			return;
		}
		string meshLocation = MeshLocation;
		if (!SdDirectory.Exists(meshLocation))
		{
			SdDirectory.CreateDirectory(meshLocation);
		}
		int streamLength = region.GetStreamLength();
		streamLength *= tryCount;
		byte[] fromPool = DynamicMeshThread.ChunkDataQueue.GetFromPool(streamLength);
		try
		{
			using MemoryStream baseStream = new MemoryStream(fromPool);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter.SetBaseStream(baseStream);
			pooledBinaryWriter.Write(160);
			Vector3i[] array = region.LoadedChunks.Distinct().ToArray();
			pooledBinaryWriter.Write(array.Length);
			Vector3i[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Vector3i vector3i = array2[i];
				pooledBinaryWriter.Write(vector3i.x);
				pooledBinaryWriter.Write(vector3i.z);
			}
			region.CreateDate = DateTime.Now;
			pooledBinaryWriter.Write(region.CreateDate.Ticks);
			_ = pooledBinaryWriter.BaseStream.Position;
		}
		catch (Exception ex)
		{
			if (!(ex.Message == "Memory stream is not expandable") || tryCount > 1)
			{
				Log.Warning("Write region error: " + region.ToDebugLocation() + "  " + ex.Message + "  attempt: " + tryCount);
			}
			tryCount++;
			Thread.Sleep(100 * tryCount);
			WriteRegion(region, tryCount);
		}
	}

	public static void WriteVoxelMeshes(BinaryWriter _bw, List<VoxelMesh> meshes, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder)
	{
		if (DynamicMeshManager.Allow32BitMeshes)
		{
			Write32BitVoxelMeshes(_bw, meshes, worldPos, updateTime, builder);
		}
		else
		{
			Write16BitVoxelMeshes(_bw, meshes, worldPos, updateTime, builder);
		}
	}

	public static void Write32BitVoxelMeshes(BinaryWriter _bw, List<VoxelMesh> meshes, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder)
	{
		int value = 1;
		_ = DynamicMeshManager.Allow32BitMeshes;
		if (meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMesh d) => d.Vertices.Count) < 65535)
		{
			Write16BitVoxelMeshes(_bw, meshes, worldPos, updateTime, builder);
			return;
		}
		_bw.Write(worldPos.x);
		if (builder != null)
		{
			_bw.Write(builder.yOffset);
		}
		else
		{
			_bw.Write(worldPos.y);
		}
		_bw.Write(worldPos.z);
		if (builder != null)
		{
			_bw.Write(updateTime);
		}
		_bw.Write(value);
		for (int num = 0; num < meshes.Count; num++)
		{
			VoxelMesh voxelMesh = meshes[num];
			if (bounds.Count < num + 1)
			{
				bounds.Add(default(Bounds));
			}
			if (voxelMesh.Vertices.Count == 0)
			{
				bounds[num].SetMinMax(Vector3.zero, Vector3.zero);
			}
			else
			{
				bounds.Add(GetBoundsFromVertsJustY(voxelMesh.Vertices, bounds[num]));
			}
		}
		WriteVoxelMeshesToDisk(_bw, meshes);
	}

	public static void Write16BitVoxelMeshes(BinaryWriter _bw, List<VoxelMesh> meshes, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder)
	{
		int num = 0;
		ushort num2 = ushort.MaxValue;
		foreach (VoxelMesh mesh in meshes)
		{
			if (mesh.Vertices.Count > 0)
			{
				num += (int)Math.Ceiling((double)mesh.Vertices.Count / (double)(int)num2);
			}
		}
		_bw.Write(worldPos.x);
		if (builder != null)
		{
			_bw.Write(builder.yOffset);
		}
		else
		{
			_bw.Write(worldPos.y);
		}
		_bw.Write(worldPos.z);
		if (builder != null)
		{
			_bw.Write(updateTime);
		}
		_bw.Write(num);
		int num3 = 0;
		float num4 = 0f;
		for (int i = 0; i < meshes.Count; i++)
		{
			VoxelMesh voxelMesh = meshes[i];
			if (bounds.Count < i + 1)
			{
				bounds.Add(default(Bounds));
			}
			if (voxelMesh.Vertices.Count == 0)
			{
				bounds[i].SetMinMax(Vector3.zero, Vector3.zero);
			}
			else
			{
				bounds.Add(GetBoundsFromVertsJustY(voxelMesh.Vertices, bounds[i]));
			}
		}
		for (int j = 0; j < meshes.Count; j++)
		{
			VoxelMesh voxelMesh2 = meshes[j];
			if (voxelMesh2.Vertices.Count != 0)
			{
				WriteVoxelMeshToDisk(_bw, voxelMesh2, MeshDescription.meshes[0].textureAtlas.uvMapping, num4, builder, 0);
				num4 -= bounds[j].size.y;
				num3 += voxelMesh2.Vertices.Count;
			}
		}
	}

	public static void WriteVoxelMeshesWithTerrain(BinaryWriter _bw, List<VoxelMesh> meshes, List<VoxelMeshTerrain> terrain, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder, DynamicMeshRegion region)
	{
		WriteVoxelMeshes(_bw, meshes, worldPos, updateTime, builder);
		if (terrain == null)
		{
			return;
		}
		int num = 0;
		int num2 = (DynamicMeshManager.Allow32BitMeshes ? int.MaxValue : 65535);
		foreach (VoxelMeshTerrain item in terrain)
		{
			if (item.Vertices.Count > 0)
			{
				num += (int)Math.Ceiling((double)item.Vertices.Count / (double)num2);
			}
		}
		_bw.Write(num);
		int num3 = 0;
		float num4 = 0f;
		for (int i = 0; i < terrain.Count; i++)
		{
			VoxelMeshTerrain voxelMeshTerrain = terrain[i];
			if (bounds.Count < i + 1)
			{
				bounds.Add(default(Bounds));
			}
			if (voxelMeshTerrain.Vertices.Count == 0)
			{
				bounds[i].SetMinMax(Vector3.zero, Vector3.zero);
			}
			else
			{
				bounds.Add(GetBoundsFromVertsJustY(voxelMeshTerrain.Vertices, bounds[i]));
			}
		}
		if (terrain.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.Vertices.Count) < 65535 || !DynamicMeshManager.Allow32BitMeshes)
		{
			for (int num5 = 0; num5 < terrain.Count; num5++)
			{
				VoxelMeshTerrain voxelMeshTerrain2 = terrain[num5];
				if (voxelMeshTerrain2.Vertices.Count != 0)
				{
					WriteTerrainVoxelMeshToDisk(_bw, voxelMeshTerrain2, num4, builder, region, 0);
					num4 -= bounds[num5].size.y;
					num3 += voxelMeshTerrain2.Vertices.Count;
				}
			}
		}
		else
		{
			WriteTerrainVoxelMeshesToDisk(_bw, terrain);
		}
	}

	public static void InitTiles()
	{
		TileLinks.Clear();
		TileLinks.TryAdd(0, 0);
		TileLinks.TryAdd(1, 7);
		TileLinks.TryAdd(10, 11);
		TileLinks.TryAdd(100, 170);
		TileLinks.TryAdd(101, 170);
		TileLinks.TryAdd(102, 170);
		TileLinks.TryAdd(103, 170);
		TileLinks.TryAdd(104, 171);
		TileLinks.TryAdd(105, 172);
		TileLinks.TryAdd(106, 173);
		TileLinks.TryAdd(107, 173);
		TileLinks.TryAdd(108, 173);
		TileLinks.TryAdd(109, 173);
		TileLinks.TryAdd(11, 11);
		TileLinks.TryAdd(110, 174);
		TileLinks.TryAdd(112, 190);
		TileLinks.TryAdd(113, 191);
		TileLinks.TryAdd(114, 192);
		TileLinks.TryAdd(115, 192);
		TileLinks.TryAdd(116, 192);
		TileLinks.TryAdd(117, 192);
		TileLinks.TryAdd(118, 193);
		TileLinks.TryAdd(119, 193);
		TileLinks.TryAdd(12, 11);
		TileLinks.TryAdd(120, 193);
		TileLinks.TryAdd(121, 193);
		TileLinks.TryAdd(122, 194);
		TileLinks.TryAdd(123, 194);
		TileLinks.TryAdd(124, 194);
		TileLinks.TryAdd(125, 194);
		TileLinks.TryAdd(126, 196);
		TileLinks.TryAdd(127, 214);
		TileLinks.TryAdd(128, 217);
		TileLinks.TryAdd(129, 217);
		TileLinks.TryAdd(13, 12);
		TileLinks.TryAdd(130, 217);
		TileLinks.TryAdd(131, 217);
		TileLinks.TryAdd(132, 226);
		TileLinks.TryAdd(133, 226);
		TileLinks.TryAdd(134, 226);
		TileLinks.TryAdd(135, 226);
		TileLinks.TryAdd(136, 227);
		TileLinks.TryAdd(137, 227);
		TileLinks.TryAdd(138, 227);
		TileLinks.TryAdd(139, 227);
		TileLinks.TryAdd(14, 12);
		TileLinks.TryAdd(140, 229);
		TileLinks.TryAdd(141, 229);
		TileLinks.TryAdd(142, 229);
		TileLinks.TryAdd(143, 229);
		TileLinks.TryAdd(144, 241);
		TileLinks.TryAdd(145, 241);
		TileLinks.TryAdd(146, 241);
		TileLinks.TryAdd(147, 241);
		TileLinks.TryAdd(148, 245);
		TileLinks.TryAdd(149, 245);
		TileLinks.TryAdd(15, 12);
		TileLinks.TryAdd(150, 245);
		TileLinks.TryAdd(151, 245);
		TileLinks.TryAdd(152, 246);
		TileLinks.TryAdd(153, 247);
		TileLinks.TryAdd(154, 248);
		TileLinks.TryAdd(155, 261);
		TileLinks.TryAdd(156, 262);
		TileLinks.TryAdd(157, 263);
		TileLinks.TryAdd(158, 265);
		TileLinks.TryAdd(159, 266);
		TileLinks.TryAdd(16, 12);
		TileLinks.TryAdd(160, 267);
		TileLinks.TryAdd(161, 268);
		TileLinks.TryAdd(162, 269);
		TileLinks.TryAdd(163, 269);
		TileLinks.TryAdd(164, 269);
		TileLinks.TryAdd(165, 269);
		TileLinks.TryAdd(166, 270);
		TileLinks.TryAdd(167, 272);
		TileLinks.TryAdd(168, 276);
		TileLinks.TryAdd(169, 282);
		TileLinks.TryAdd(17, 13);
		TileLinks.TryAdd(170, 282);
		TileLinks.TryAdd(171, 282);
		TileLinks.TryAdd(172, 282);
		TileLinks.TryAdd(173, 284);
		TileLinks.TryAdd(174, 299);
		TileLinks.TryAdd(175, 299);
		TileLinks.TryAdd(176, 299);
		TileLinks.TryAdd(177, 299);
		TileLinks.TryAdd(178, 302);
		TileLinks.TryAdd(179, 302);
		TileLinks.TryAdd(18, 13);
		TileLinks.TryAdd(180, 302);
		TileLinks.TryAdd(181, 302);
		TileLinks.TryAdd(182, 303);
		TileLinks.TryAdd(183, 307);
		TileLinks.TryAdd(184, 311);
		TileLinks.TryAdd(185, 312);
		TileLinks.TryAdd(186, 314);
		TileLinks.TryAdd(187, 315);
		TileLinks.TryAdd(188, 319);
		TileLinks.TryAdd(189, 320);
		TileLinks.TryAdd(19, 13);
		TileLinks.TryAdd(190, 321);
		TileLinks.TryAdd(191, 328);
		TileLinks.TryAdd(192, 328);
		TileLinks.TryAdd(193, 328);
		TileLinks.TryAdd(194, 328);
		TileLinks.TryAdd(195, 329);
		TileLinks.TryAdd(196, 329);
		TileLinks.TryAdd(197, 329);
		TileLinks.TryAdd(198, 329);
		TileLinks.TryAdd(199, 330);
		TileLinks.TryAdd(2, 7);
		TileLinks.TryAdd(20, 13);
		TileLinks.TryAdd(200, 331);
		TileLinks.TryAdd(201, 332);
		TileLinks.TryAdd(202, 332);
		TileLinks.TryAdd(203, 332);
		TileLinks.TryAdd(204, 332);
		TileLinks.TryAdd(206, 335);
		TileLinks.TryAdd(209, 340);
		TileLinks.TryAdd(21, 14);
		TileLinks.TryAdd(210, 341);
		TileLinks.TryAdd(211, 342);
		TileLinks.TryAdd(212, 344);
		TileLinks.TryAdd(213, 344);
		TileLinks.TryAdd(214, 344);
		TileLinks.TryAdd(215, 344);
		TileLinks.TryAdd(216, 345);
		TileLinks.TryAdd(217, 346);
		TileLinks.TryAdd(218, 352);
		TileLinks.TryAdd(219, 355);
		TileLinks.TryAdd(22, 14);
		TileLinks.TryAdd(220, 356);
		TileLinks.TryAdd(221, 358);
		TileLinks.TryAdd(222, 361);
		TileLinks.TryAdd(223, 372);
		TileLinks.TryAdd(226, 379);
		TileLinks.TryAdd(227, 380);
		TileLinks.TryAdd(228, 382);
		TileLinks.TryAdd(229, 384);
		TileLinks.TryAdd(23, 14);
		TileLinks.TryAdd(230, 385);
		TileLinks.TryAdd(231, 385);
		TileLinks.TryAdd(232, 385);
		TileLinks.TryAdd(233, 385);
		TileLinks.TryAdd(234, 391);
		TileLinks.TryAdd(235, 407);
		TileLinks.TryAdd(236, 408);
		TileLinks.TryAdd(237, 408);
		TileLinks.TryAdd(238, 408);
		TileLinks.TryAdd(239, 408);
		TileLinks.TryAdd(24, 14);
		TileLinks.TryAdd(240, 410);
		TileLinks.TryAdd(241, 413);
		TileLinks.TryAdd(246, 422);
		TileLinks.TryAdd(247, 427);
		TileLinks.TryAdd(248, 428);
		TileLinks.TryAdd(249, 429);
		TileLinks.TryAdd(25, 15);
		TileLinks.TryAdd(250, 429);
		TileLinks.TryAdd(251, 429);
		TileLinks.TryAdd(252, 429);
		TileLinks.TryAdd(253, 430);
		TileLinks.TryAdd(254, 435);
		TileLinks.TryAdd(255, 435);
		TileLinks.TryAdd(256, 435);
		TileLinks.TryAdd(257, 435);
		TileLinks.TryAdd(258, 436);
		TileLinks.TryAdd(259, 442);
		TileLinks.TryAdd(26, 15);
		TileLinks.TryAdd(260, 443);
		TileLinks.TryAdd(261, 443);
		TileLinks.TryAdd(262, 443);
		TileLinks.TryAdd(263, 443);
		TileLinks.TryAdd(264, 445);
		TileLinks.TryAdd(265, 446);
		TileLinks.TryAdd(266, 519);
		TileLinks.TryAdd(267, 525);
		TileLinks.TryAdd(268, 531);
		TileLinks.TryAdd(269, 532);
		TileLinks.TryAdd(27, 15);
		TileLinks.TryAdd(270, 534);
		TileLinks.TryAdd(271, 534);
		TileLinks.TryAdd(272, 534);
		TileLinks.TryAdd(273, 534);
		TileLinks.TryAdd(274, 535);
		TileLinks.TryAdd(275, 535);
		TileLinks.TryAdd(276, 535);
		TileLinks.TryAdd(277, 535);
		TileLinks.TryAdd(278, 536);
		TileLinks.TryAdd(279, 536);
		TileLinks.TryAdd(28, 15);
		TileLinks.TryAdd(280, 536);
		TileLinks.TryAdd(281, 536);
		TileLinks.TryAdd(282, 537);
		TileLinks.TryAdd(283, 537);
		TileLinks.TryAdd(284, 537);
		TileLinks.TryAdd(285, 537);
		TileLinks.TryAdd(286, 538);
		TileLinks.TryAdd(287, 538);
		TileLinks.TryAdd(288, 538);
		TileLinks.TryAdd(289, 538);
		TileLinks.TryAdd(29, 21);
		TileLinks.TryAdd(290, 539);
		TileLinks.TryAdd(291, 539);
		TileLinks.TryAdd(292, 539);
		TileLinks.TryAdd(293, 539);
		TileLinks.TryAdd(294, 540);
		TileLinks.TryAdd(295, 540);
		TileLinks.TryAdd(296, 540);
		TileLinks.TryAdd(297, 540);
		TileLinks.TryAdd(298, 541);
		TileLinks.TryAdd(299, 542);
		TileLinks.TryAdd(3, 7);
		TileLinks.TryAdd(30, 22);
		TileLinks.TryAdd(300, 543);
		TileLinks.TryAdd(301, 544);
		TileLinks.TryAdd(302, 544);
		TileLinks.TryAdd(303, 544);
		TileLinks.TryAdd(304, 544);
		TileLinks.TryAdd(305, 545);
		TileLinks.TryAdd(306, 545);
		TileLinks.TryAdd(307, 545);
		TileLinks.TryAdd(308, 545);
		TileLinks.TryAdd(309, 546);
		TileLinks.TryAdd(31, 23);
		TileLinks.TryAdd(310, 546);
		TileLinks.TryAdd(311, 546);
		TileLinks.TryAdd(312, 546);
		TileLinks.TryAdd(313, 547);
		TileLinks.TryAdd(314, 548);
		TileLinks.TryAdd(315, 548);
		TileLinks.TryAdd(316, 548);
		TileLinks.TryAdd(317, 548);
		TileLinks.TryAdd(318, 549);
		TileLinks.TryAdd(319, 549);
		TileLinks.TryAdd(32, 23);
		TileLinks.TryAdd(320, 549);
		TileLinks.TryAdd(321, 549);
		TileLinks.TryAdd(322, 552);
		TileLinks.TryAdd(323, 553);
		TileLinks.TryAdd(324, 553);
		TileLinks.TryAdd(325, 553);
		TileLinks.TryAdd(326, 553);
		TileLinks.TryAdd(327, 554);
		TileLinks.TryAdd(328, 555);
		TileLinks.TryAdd(329, 571);
		TileLinks.TryAdd(33, 23);
		TileLinks.TryAdd(330, 571);
		TileLinks.TryAdd(331, 571);
		TileLinks.TryAdd(332, 571);
		TileLinks.TryAdd(333, 572);
		TileLinks.TryAdd(334, 580);
		TileLinks.TryAdd(335, 581);
		TileLinks.TryAdd(336, 582);
		TileLinks.TryAdd(337, 582);
		TileLinks.TryAdd(338, 582);
		TileLinks.TryAdd(339, 582);
		TileLinks.TryAdd(34, 23);
		TileLinks.TryAdd(340, 583);
		TileLinks.TryAdd(341, 584);
		TileLinks.TryAdd(342, 585);
		TileLinks.TryAdd(343, 585);
		TileLinks.TryAdd(344, 585);
		TileLinks.TryAdd(345, 585);
		TileLinks.TryAdd(346, 586);
		TileLinks.TryAdd(347, 587);
		TileLinks.TryAdd(348, 588);
		TileLinks.TryAdd(349, 589);
		TileLinks.TryAdd(35, 24);
		TileLinks.TryAdd(350, 590);
		TileLinks.TryAdd(351, 590);
		TileLinks.TryAdd(352, 590);
		TileLinks.TryAdd(353, 590);
		TileLinks.TryAdd(354, 591);
		TileLinks.TryAdd(355, 591);
		TileLinks.TryAdd(356, 591);
		TileLinks.TryAdd(357, 591);
		TileLinks.TryAdd(358, 592);
		TileLinks.TryAdd(359, 592);
		TileLinks.TryAdd(36, 25);
		TileLinks.TryAdd(360, 592);
		TileLinks.TryAdd(361, 592);
		TileLinks.TryAdd(362, 593);
		TileLinks.TryAdd(364, 597);
		TileLinks.TryAdd(365, 598);
		TileLinks.TryAdd(37, 43);
		TileLinks.TryAdd(38, 43);
		TileLinks.TryAdd(39, 43);
		TileLinks.TryAdd(4, 8);
		TileLinks.TryAdd(40, 43);
		TileLinks.TryAdd(41, 44);
		TileLinks.TryAdd(42, 46);
		TileLinks.TryAdd(43, 50);
		TileLinks.TryAdd(44, 50);
		TileLinks.TryAdd(45, 50);
		TileLinks.TryAdd(46, 50);
		TileLinks.TryAdd(47, 51);
		TileLinks.TryAdd(48, 51);
		TileLinks.TryAdd(49, 51);
		TileLinks.TryAdd(5, 8);
		TileLinks.TryAdd(50, 51);
		TileLinks.TryAdd(51, 52);
		TileLinks.TryAdd(52, 52);
		TileLinks.TryAdd(53, 52);
		TileLinks.TryAdd(54, 52);
		TileLinks.TryAdd(55, 53);
		TileLinks.TryAdd(56, 53);
		TileLinks.TryAdd(57, 53);
		TileLinks.TryAdd(58, 53);
		TileLinks.TryAdd(59, 54);
		TileLinks.TryAdd(6, 8);
		TileLinks.TryAdd(60, 55);
		TileLinks.TryAdd(61, 56);
		TileLinks.TryAdd(62, 57);
		TileLinks.TryAdd(63, 57);
		TileLinks.TryAdd(64, 57);
		TileLinks.TryAdd(65, 57);
		TileLinks.TryAdd(66, 58);
		TileLinks.TryAdd(67, 59);
		TileLinks.TryAdd(68, 61);
		TileLinks.TryAdd(69, 62);
		TileLinks.TryAdd(7, 8);
		TileLinks.TryAdd(70, 64);
		TileLinks.TryAdd(71, 65);
		TileLinks.TryAdd(72, 67);
		TileLinks.TryAdd(73, 73);
		TileLinks.TryAdd(74, 74);
		TileLinks.TryAdd(75, 74);
		TileLinks.TryAdd(76, 74);
		TileLinks.TryAdd(77, 74);
		TileLinks.TryAdd(78, 75);
		TileLinks.TryAdd(79, 75);
		TileLinks.TryAdd(8, 9);
		TileLinks.TryAdd(80, 75);
		TileLinks.TryAdd(81, 75);
		TileLinks.TryAdd(82, 76);
		TileLinks.TryAdd(83, 77);
		TileLinks.TryAdd(84, 77);
		TileLinks.TryAdd(85, 77);
		TileLinks.TryAdd(86, 77);
		TileLinks.TryAdd(87, 78);
		TileLinks.TryAdd(88, 79);
		TileLinks.TryAdd(89, 81);
		TileLinks.TryAdd(9, 11);
		TileLinks.TryAdd(90, 84);
		TileLinks.TryAdd(91, 116);
		TileLinks.TryAdd(92, 168);
		TileLinks.TryAdd(93, 168);
		TileLinks.TryAdd(94, 168);
		TileLinks.TryAdd(95, 168);
		TileLinks.TryAdd(96, 169);
		TileLinks.TryAdd(97, 169);
		TileLinks.TryAdd(98, 169);
		TileLinks.TryAdd(99, 169);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteVoxelMeshesToDisk(BinaryWriter writer, List<VoxelMesh> meshes)
	{
		UVRectTiling[] uvMapping = MeshDescription.meshes[0].textureAtlas.uvMapping;
		if (!DynamicMeshManager.Allow32BitMeshes)
		{
			Log.Error("Can't write combined meshes to disk unless in 32 bit mode");
			return;
		}
		int value = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMesh d) => d.Vertices.Count);
		int value2 = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMesh d) => d.Indices.Count);
		writer.Write((uint)value);
		for (int num = 0; num < meshes.Count; num++)
		{
			ArrayListMP<Vector3> vertices = meshes[num].Vertices;
			for (int num2 = 0; num2 < vertices.Count; num2++)
			{
				writer.Write((short)((double)vertices[num2].x * 100.0));
				writer.Write((short)((double)vertices[num2].y * 100.0));
				writer.Write((short)((double)vertices[num2].z * 100.0));
			}
		}
		writer.Write((uint)value);
		for (int num3 = 0; num3 < meshes.Count; num3++)
		{
			VoxelMesh voxelMesh = meshes[num3];
			ArrayListMP<Color> colorVertices = voxelMesh.ColorVertices;
			ArrayListMP<Vector2> uvs = voxelMesh.Uvs;
			for (int num4 = 0; num4 < colorVertices.Count; num4++)
			{
				int num5 = (int)colorVertices[num4].g;
				int value3 = -1;
				if (!TileLinks.TryGetValue(num5, out value3))
				{
					for (int num6 = 0; num6 < uvMapping.Length; num6++)
					{
						UVRectTiling uVRectTiling = uvMapping[num6];
						if (uVRectTiling.index == num5 || num6 + 1 >= uvMapping.Length || (float)uVRectTiling.index + uVRectTiling.uv.width * uVRectTiling.uv.height > (float)num5)
						{
							value3 = num6;
							TileLinks.TryAdd(num5, value3);
							break;
						}
					}
					if (value3 == -1)
					{
						value3 = 0;
						TileLinks.TryAdd(num5, value3);
					}
				}
				if (value3 == -1)
				{
					value3 = 0;
				}
				writer.Write((short)value3);
				writer.Write((byte)(num5 - uvMapping[value3].index));
				bool value4 = (double)colorVertices[num4].a > 0.5;
				writer.Write(value4);
				writer.Write((ushort)((double)uvs[num4].x * 10000.0));
				writer.Write((ushort)((double)uvs[num4].y * 10000.0));
			}
		}
		writer.Write((uint)value2);
		int num7 = 0;
		for (int num8 = 0; num8 < meshes.Count; num8++)
		{
			ArrayListMP<int> indices = meshes[num8].Indices;
			for (int num9 = 0; num9 < indices.Count; num9++)
			{
				writer.Write(indices[num9] + num7);
			}
			num7 += meshes[num8].Vertices.Count;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int WriteVoxelMeshToDisk(BinaryWriter writer, VoxelMesh mesh, UVRectTiling[] tiling, float yOff, DynamicMeshChunkProcessor builder, int vertOffset)
	{
		ArrayListMP<Vector3> vertices = mesh.Vertices;
		ArrayListMP<int> indices = mesh.Indices;
		ArrayListMP<Vector2> uvs = mesh.Uvs;
		ArrayListMP<Color> colorVertices = mesh.ColorVertices;
		int num = vertices.Count - vertOffset;
		int num2 = (DynamicMeshManager.Allow32BitMeshes ? int.MaxValue : 65535);
		if (num > num2)
		{
			Log.Warning("OVER MAX VERTS: ATTEMPTING SPLIT: " + num + "   triangles: " + indices.Count);
			num = num2;
		}
		int num3 = vertOffset + num;
		writer.Write((uint)num);
		for (int i = vertOffset; i < num3; i++)
		{
			writer.Write((short)((double)vertices[i].x * 100.0));
			writer.Write((short)((double)vertices[i].y * 100.0));
			writer.Write((short)((double)vertices[i].z * 100.0));
		}
		writer.Write((uint)num);
		for (int j = vertOffset; j < num3; j++)
		{
			int num4 = (int)colorVertices[j].g;
			int value = -1;
			if (!TileLinks.TryGetValue(num4, out value))
			{
				for (int k = 0; k < tiling.Length; k++)
				{
					UVRectTiling uVRectTiling = tiling[k];
					if (uVRectTiling.index == num4 || k + 1 >= tiling.Length || (float)uVRectTiling.index + uVRectTiling.uv.width * uVRectTiling.uv.height > (float)num4)
					{
						value = k;
						TileLinks.TryAdd(num4, value);
						break;
					}
				}
				if (value == -1)
				{
					value = 0;
					TileLinks.TryAdd(num4, value);
				}
			}
			if (value == -1)
			{
				value = 0;
			}
			writer.Write((short)value);
			writer.Write((byte)(num4 - tiling[value].index));
			bool value2 = (double)colorVertices[j].a > 0.5;
			writer.Write(value2);
			writer.Write((ushort)((double)uvs[j].x * 10000.0));
			writer.Write((ushort)((double)uvs[j].y * 10000.0));
		}
		int num5 = indices.Count - vertOffset;
		if (num5 > num2 && num == num2)
		{
			num5 = num2;
		}
		writer.Write((uint)num5);
		for (int l = vertOffset; l < vertOffset + num5; l++)
		{
			writer.Write((ushort)(indices[l] - vertOffset));
		}
		if (vertOffset + num < vertices.Count)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("WRITING SECOND MESH");
			}
			if (vertOffset > 0)
			{
				throw new NotImplementedException("Can't write more than two dynamic meshes per VoxelMesh");
			}
			num += WriteVoxelMeshToDisk(writer, mesh, tiling, yOff, builder, vertOffset + num);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteTerrainVoxelMeshesToDisk(BinaryWriter writer, List<VoxelMeshTerrain> meshes)
	{
		if (!DynamicMeshManager.Allow32BitMeshes)
		{
			Log.Error("Can't write combined meshes to disk unless in 32 bit mode");
			return;
		}
		int num = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.Vertices.Count);
		int num2 = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.Indices.Count);
		int num3 = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.submeshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (TerrainSubMesh e) => e.triangles.Count));
		int num4 = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.ColorVertices.Count);
		if (num != num4)
		{
			Log.Error("Invalid colours");
		}
		int value = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.Uvs.Count);
		int value2 = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.UvsCrack.Count);
		int value3 = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.Uvs3.Count);
		int value4 = meshes.Sum([PublicizedFrom(EAccessModifier.Internal)] (VoxelMeshTerrain d) => d.Uvs4.Count);
		writer.Write((uint)num);
		for (int num5 = 0; num5 < meshes.Count; num5++)
		{
			ArrayListMP<Vector3> vertices = meshes[num5].Vertices;
			for (int num6 = 0; num6 < vertices.Count; num6++)
			{
				writer.Write((short)((double)vertices[num6].x * 100.0));
				writer.Write((short)((double)vertices[num6].y * 100.0));
				writer.Write((short)((double)vertices[num6].z * 100.0));
			}
		}
		writer.Write((uint)value);
		for (int num7 = 0; num7 < meshes.Count; num7++)
		{
			ArrayListMP<Vector2> uvs = meshes[num7].Uvs;
			for (int num8 = 0; num8 < uvs.Count; num8++)
			{
				writer.Write((ushort)((double)uvs[num8].x * 10000.0));
				writer.Write((ushort)((double)uvs[num8].y * 10000.0));
			}
		}
		writer.Write((uint)value2);
		for (int num9 = 0; num9 < meshes.Count; num9++)
		{
			ArrayListMP<Vector2> uvsCrack = meshes[num9].UvsCrack;
			for (int num10 = 0; num10 < uvsCrack.Count; num10++)
			{
				writer.Write((ushort)((double)uvsCrack[num10].x * 10000.0));
				writer.Write((ushort)((double)uvsCrack[num10].y * 10000.0));
			}
		}
		writer.Write((uint)value3);
		for (int num11 = 0; num11 < meshes.Count; num11++)
		{
			ArrayListMP<Vector2> uvs2 = meshes[num11].Uvs3;
			for (int num12 = 0; num12 < uvs2.Count; num12++)
			{
				writer.Write((ushort)((double)uvs2[num12].x * 10000.0));
				writer.Write((ushort)((double)uvs2[num12].y * 10000.0));
			}
		}
		writer.Write((uint)value4);
		for (int num13 = 0; num13 < meshes.Count; num13++)
		{
			ArrayListMP<Vector2> uvs3 = meshes[num13].Uvs4;
			for (int num14 = 0; num14 < uvs3.Count; num14++)
			{
				writer.Write((ushort)((double)uvs3[num14].x * 10000.0));
				writer.Write((ushort)((double)uvs3[num14].y * 10000.0));
			}
		}
		int value5 = 1;
		writer.Write((uint)value5);
		writer.Write((uint)num2);
		int num15 = 0;
		if (num2 % 3 != 0 || num3 % 3 != 0)
		{
			Log.Out("Weird triangles");
		}
		for (int num16 = 0; num16 < meshes.Count; num16++)
		{
			VoxelMeshTerrain voxelMeshTerrain = meshes[num16];
			if (voxelMeshTerrain.submeshes.Count > 0)
			{
				foreach (TerrainSubMesh submesh in voxelMeshTerrain.submeshes)
				{
					for (int num17 = 0; num17 < submesh.triangles.Count; num17++)
					{
						writer.Write(submesh.triangles[num17] + num15);
					}
				}
			}
			else
			{
				for (int num18 = 0; num18 < voxelMeshTerrain.Indices.Count; num18++)
				{
					writer.Write(voxelMeshTerrain.Indices[num18] + num15);
				}
			}
			num15 += voxelMeshTerrain.Vertices.Count;
		}
		writer.Write((uint)num);
		for (int num19 = 0; num19 < meshes.Count; num19++)
		{
			ArrayListMP<Color> colorVertices = meshes[num19].ColorVertices;
			for (int num20 = 0; num20 < colorVertices.Count; num20++)
			{
				Color color = colorVertices[num20];
				writer.Write((ushort)(color.r * 10000f));
				writer.Write((ushort)(color.g * 10000f));
				writer.Write((ushort)(color.b * 10000f));
				writer.Write((ushort)(color.a * 10000f));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteTerrainVoxelMeshToDisk(BinaryWriter writer, VoxelMeshTerrain mesh, float yOff, DynamicMeshChunkProcessor builder, DynamicMeshRegion region, int vertOffset)
	{
		ArrayListMP<Vector3> vertices = mesh.Vertices;
		ArrayListMP<int> indices = mesh.Indices;
		ArrayListMP<Vector2> uvs = mesh.Uvs;
		ArrayListMP<Vector2> uvsCrack = mesh.UvsCrack;
		ArrayListMP<Vector2> uvs2 = mesh.Uvs3;
		ArrayListMP<Vector2> uvs3 = mesh.Uvs4;
		ArrayListMP<Color> colorVertices = mesh.ColorVertices;
		int num = vertices.Count - vertOffset;
		int num2 = (DynamicMeshManager.Allow32BitMeshes ? int.MaxValue : 65535);
		if (num > num2)
		{
			Log.Warning("OVER MAX VERTS: ATTEMPTING SPLIT: " + num + "   triangles: " + indices.Count);
			num = num2;
		}
		int num3 = vertOffset + num;
		writer.Write((uint)num);
		for (int i = vertOffset; i < num3; i++)
		{
			writer.Write((short)((double)vertices[i].x * 100.0));
			writer.Write((short)((double)vertices[i].y * 100.0));
			writer.Write((short)((double)vertices[i].z * 100.0));
		}
		writer.Write((uint)num);
		for (int j = vertOffset; j < num3; j++)
		{
			writer.Write((ushort)((double)uvs[j].x * 10000.0));
			writer.Write((ushort)((double)uvs[j].y * 10000.0));
		}
		writer.Write((uint)uvsCrack.Count);
		for (int k = 0; k < uvsCrack.Count; k++)
		{
			writer.Write((ushort)((double)uvsCrack[k].x * 10000.0));
			writer.Write((ushort)((double)uvsCrack[k].y * 10000.0));
		}
		writer.Write((uint)uvs2.Count);
		for (int l = 0; l < uvs2.Count; l++)
		{
			writer.Write((ushort)((double)uvs2[l].x * 10000.0));
			writer.Write((ushort)((double)uvs2[l].y * 10000.0));
		}
		writer.Write((uint)uvs3.Count);
		for (int m = 0; m < uvs3.Count; m++)
		{
			writer.Write((ushort)((double)uvs3[m].x * 10000.0));
			writer.Write((ushort)((double)uvs3[m].y * 10000.0));
		}
		int value = 1;
		writer.Write((uint)value);
		ArrayListMP<int> indices2 = mesh.Indices;
		int count = indices2.Count;
		writer.Write((uint)count);
		for (int n = 0; n < count; n++)
		{
			writer.Write((ushort)(indices2[n] - vertOffset));
		}
		int num4 = colorVertices.Count - vertOffset;
		if (num4 > num2 && num == num2)
		{
			num4 = num2;
		}
		writer.Write((uint)num4);
		for (int num5 = vertOffset; num5 < vertOffset + num4; num5++)
		{
			Color color = colorVertices[num5];
			writer.Write((ushort)(color.r * 10000f));
			writer.Write((ushort)(color.g * 10000f));
			writer.Write((ushort)(color.b * 10000f));
			writer.Write((ushort)(color.a * 10000f));
		}
		if (vertOffset + num < vertices.Count)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("WRITING SECOND MESH");
			}
			if (vertOffset > 0)
			{
				throw new NotImplementedException("Can't write more than two dynamic meshes per VoxelMesh");
			}
			WriteTerrainVoxelMeshToDisk(writer, mesh, yOff, builder, region, vertOffset + num);
		}
	}

	public static void ReadItemMesh(BinaryReader reader, DynamicMeshItem item, bool isVisible)
	{
		if (reader.BaseStream.Position == reader.BaseStream.Length)
		{
			Log.Out("ReadItemMesh Zero length: " + item.ToDebugLocation());
			return;
		}
		int x = reader.ReadInt32();
		int y = reader.ReadInt32();
		int z = reader.ReadInt32();
		item.WorldPosition.x = x;
		item.WorldPosition.y = y;
		item.WorldPosition.z = z;
		int updateTime = reader.ReadInt32();
		item.UpdateTime = updateTime;
		GameObject gameObject = DynamicMeshItem.GetItemMeshRendererFromPool();
		int num = reader.ReadInt32();
		int num2 = 0;
		while (true)
		{
			if (num2 < num)
			{
				GameObject gameObject2 = ((num2 == 0) ? gameObject : DynamicMeshItem.GetItemMeshRendererFromPool());
				MeshFilter component = gameObject2.GetComponent<MeshFilter>();
				Mesh mesh = component.mesh ?? new Mesh();
				if (num2 != 0)
				{
					gameObject2.transform.parent = gameObject.transform;
				}
				mesh = ReadMesh(3, reader, MeshDescription.meshes[0].textureAtlas.uvMapping, mesh, lockMesh: false, item);
				if (item.State == DynamicItemState.Invalid)
				{
					DynamicMeshItem.AddToMeshPool(gameObject2);
					if (num2 == 0)
					{
						DynamicMeshManager.Instance.RemoveItem(item, removedFromWorld: true);
						gameObject = null;
						break;
					}
				}
				else if (mesh.vertexCount == 0)
				{
					DynamicMeshItem.AddToMeshPool(gameObject2);
				}
				else
				{
					gameObject2.SetActive(value: true);
					mesh.RecalculateBounds();
					mesh.RecalculateNormals();
					mesh.RecalculateTangents();
					component.mesh = mesh;
				}
				num2++;
				continue;
			}
			if (reader.BaseStream.Position == reader.BaseStream.Length)
			{
				break;
			}
			int num3 = reader.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				GameObject itemMeshRendererFromPool = DynamicMeshItem.GetItemMeshRendererFromPool();
				MeshFilter component2 = itemMeshRendererFromPool.GetComponent<MeshFilter>();
				Mesh mesh2 = component2.mesh ?? new Mesh();
				MeshRenderer component3 = itemMeshRendererFromPool.GetComponent<MeshRenderer>();
				itemMeshRendererFromPool.transform.parent = gameObject.transform;
				if (DynamicMeshManager.DebugItemPositions)
				{
					itemMeshRendererFromPool.name = "Terrain";
				}
				mesh2 = ReadMeshTerrain(reader, mesh2, component3, lockMesh: false);
				if (mesh2.vertexCount == 0)
				{
					DynamicMeshItem.AddToMeshPool(itemMeshRendererFromPool);
					continue;
				}
				itemMeshRendererFromPool.SetActive(value: true);
				mesh2.RecalculateBounds();
				mesh2.RecalculateNormals();
				mesh2.RecalculateTangents();
				component2.mesh = mesh2;
			}
			break;
		}
		DynamicMeshItem.AddToMeshPool(item.ChunkObject);
		item.ChunkObject = gameObject;
		item.State = DynamicItemState.Loaded;
		item.SetVisible(isVisible, "read item mesh");
	}

	public static Material GetOpaqueMaterial(bool isRegion)
	{
		Material material = (isRegion ? _RegionMaterial : _itemMaterial);
		if (material != null)
		{
			return material;
		}
		MeshDescription meshDescription = MeshDescription.meshes[0];
		material = Resources.Load<Material>("Materials/DistantPOI_TA_DM");
		material = UnityEngine.Object.Instantiate(material);
		material.SetTexture("_MainTex", meshDescription.TexDiffuse);
		material.SetTexture("_Normal", meshDescription.TexNormal);
		material.SetTexture("_MetallicGlossMap", meshDescription.TexSpecular);
		material.SetTexture("_OcclusionMap", meshDescription.TexOcclusion);
		return material;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static VoxelMesh ReadVoxelMesh(BinaryReader reader, UVRectTiling[] tilings, Vector3i worldPos)
	{
		if (reader.BaseStream.Length == reader.BaseStream.Position)
		{
			Log.Out("Read voxel mesh error: end of stream");
			return GetMeshFromPool(1);
		}
		int num = 0;
		uint num2 = 0u;
		try
		{
			num2 = reader.ReadUInt32();
			bool flag = num2 > 65535;
			num = 1;
			VoxelMesh meshFromPool = GetMeshFromPool((int)num2);
			if (num2 == 0)
			{
				return meshFromPool;
			}
			for (uint num3 = 0u; num3 < num2; num3++)
			{
				meshFromPool.Vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			num = 2;
			uint num4 = reader.ReadUInt32();
			num = 3;
			for (uint num5 = 0u; num5 < num4; num5++)
			{
				int num6 = reader.ReadInt16();
				int num7 = reader.ReadByte();
				int num8 = ((num6 < tilings.Length) ? (tilings[num6].index + num7) : 0);
				bool flag2 = reader.ReadBoolean();
				meshFromPool.Uvs.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
				meshFromPool.ColorVertices.Add(new Color(0f, num8, 0f, flag2 ? 1 : 0));
			}
			num = 4;
			uint num9 = reader.ReadUInt32();
			num = 5;
			for (uint num10 = 0u; num10 < num9; num10++)
			{
				if (flag)
				{
					meshFromPool.Indices.Add(reader.ReadInt32());
				}
				else
				{
					meshFromPool.Indices.Add(reader.ReadUInt16());
				}
			}
			num = 6;
			return meshFromPool;
		}
		catch (Exception ex)
		{
			string[] obj = new string[8] { "Read voxel mesh error: ", null, null, null, null, null, null, null };
			Vector3i vector3i = worldPos;
			obj[1] = vector3i.ToString();
			obj[2] = " verts: ";
			obj[3] = num2.ToString();
			obj[4] = " stage:  ";
			obj[5] = num.ToString();
			obj[6] = "  message: ";
			obj[7] = ex.Message;
			Log.Out(string.Concat(obj));
		}
		return GetMeshFromPool(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static VoxelMeshTerrain ReadVoxelMeshTerrain(BinaryReader reader, Vector3i worldPos, out bool cancelLoad)
	{
		cancelLoad = false;
		if (reader.BaseStream.Length == reader.BaseStream.Position)
		{
			Vector3i vector3i = worldPos;
			Log.Out("Read voxel mesh terrain error: end of stream: " + vector3i.ToString());
			cancelLoad = true;
			return GetTerrainMeshFromPool(1);
		}
		int num = 0;
		uint num2 = 0u;
		try
		{
			num2 = reader.ReadUInt32();
			bool flag = num2 > 65535;
			num = 1;
			VoxelMeshTerrain terrainMeshFromPool = GetTerrainMeshFromPool((int)num2);
			for (uint num3 = 0u; num3 < num2; num3++)
			{
				terrainMeshFromPool.Vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			num = 2;
			uint num4 = reader.ReadUInt32();
			for (uint num5 = 0u; num5 < num4; num5++)
			{
				terrainMeshFromPool.Uvs.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			}
			num4 = reader.ReadUInt32();
			for (uint num6 = 0u; num6 < num4; num6++)
			{
				terrainMeshFromPool.UvsCrack.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			}
			num4 = reader.ReadUInt32();
			for (uint num7 = 0u; num7 < num4; num7++)
			{
				terrainMeshFromPool.Uvs3.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			}
			num4 = reader.ReadUInt32();
			for (uint num8 = 0u; num8 < num4; num8++)
			{
				terrainMeshFromPool.Uvs4.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			}
			num = 4;
			uint num9 = reader.ReadUInt32();
			for (int i = 0; i < num9; i++)
			{
				terrainMeshFromPool.submeshes.Add(new TerrainSubMesh(terrainMeshFromPool.submeshes));
				TerrainSubMesh terrainSubMesh = terrainMeshFromPool.submeshes[i];
				uint num10 = reader.ReadUInt32();
				num = 5;
				for (uint num11 = 0u; num11 < num10; num11++)
				{
					if (flag)
					{
						terrainSubMesh.triangles.Add(reader.ReadInt32());
					}
					else
					{
						terrainSubMesh.triangles.Add(reader.ReadUInt16());
					}
				}
			}
			num = 6;
			uint num12 = reader.ReadUInt32();
			for (uint num13 = 0u; num13 < num12; num13++)
			{
				terrainMeshFromPool.ColorVertices.Add(new Color((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			}
			return terrainMeshFromPool;
		}
		catch (Exception ex)
		{
			string[] obj = new string[8] { "Read voxel terrain mesh error: ", null, null, null, null, null, null, null };
			Vector3i vector3i = worldPos;
			obj[1] = vector3i.ToString();
			obj[2] = " verts: ";
			obj[3] = num2.ToString();
			obj[4] = " stage:  ";
			obj[5] = num.ToString();
			obj[6] = "  message: ";
			obj[7] = ex.Message;
			Log.Out(string.Concat(obj));
		}
		cancelLoad = true;
		return GetTerrainMeshFromPool(1);
	}

	public static void SetMeshMax(int max, bool orMore)
	{
		if (!orMore || ReadMeshMax < max)
		{
			ReadMeshMax = max;
		}
	}

	public static IEnumerator ReadMeshCoroutine(int version, BinaryReader reader, UVRectTiling[] tilings, Mesh mesh, DynamicMeshRegion region, DynamicMeshItem item, int maxTime)
	{
		MeshLists cache = MeshLists.GetList();
		List<Vector3> vertices = cache.Vertices;
		List<int> triangles = cache.Triangles;
		List<Vector2> uvs = cache.Uvs;
		List<Color> colours = cache.Colours;
		if (reader.BaseStream.Position == reader.BaseStream.Length)
		{
			Log.Warning("Corrupted mesh file");
			MeshLists.ReturnList(cache);
			yield break;
		}
		uint vertCount = reader.ReadUInt32();
		bool is32Bit = vertCount > 65535 && DynamicMeshManager.Allow32BitMeshes;
		mesh.indexFormat = (is32Bit ? IndexFormat.UInt32 : IndexFormat.UInt16);
		if (!DynamicMeshManager.CompressFiles && reader.BaseStream.Position + vertCount * 6 > reader.BaseStream.Length)
		{
			Log.Error("Not enough data in file for verts. Position: " + reader.BaseStream.Position + " expecting: " + vertCount * 6 + " Length: " + reader.BaseStream.Length);
			region?.OnCorrupted();
			item?.OnCorrupted();
			MeshLists.ReturnList(cache);
			yield break;
		}
		for (uint vCount = 0u; vCount < vertCount; vCount++)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		uint uvCount = reader.ReadUInt32();
		if (!DynamicMeshManager.CompressFiles && reader.BaseStream.Position + uvCount * 8 > reader.BaseStream.Length)
		{
			Log.Error("Not enough data in file for uv.Position: " + reader.BaseStream.Position + " expecting: " + uvCount * 8 + " Length: " + reader.BaseStream.Length);
			region?.OnCorrupted();
			item?.OnCorrupted();
			MeshLists.ReturnList(cache);
			yield break;
		}
		for (uint vCount = 0u; vCount < uvCount; vCount++)
		{
			int num = reader.ReadInt16();
			int num2 = reader.ReadByte();
			int num3 = ((num < tilings.Length) ? (tilings[num].index + num2) : 0);
			bool flag = reader.ReadBoolean();
			uvs.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			colours.Add(new Color(0f, num3, 0f, flag ? 1 : 0));
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		uint triangleCount = reader.ReadUInt32();
		if (reader.BaseStream.Position + triangleCount * 2 > reader.BaseStream.Length)
		{
			Log.Error("Not enough data in file for triangles.Position: " + reader.BaseStream.Position + " expecting: " + triangleCount * 2 + " Length: " + reader.BaseStream.Length);
			region?.OnCorrupted();
			item?.OnCorrupted();
			MeshLists.ReturnList(cache);
			yield break;
		}
		for (uint vCount = 0u; vCount < triangleCount; vCount++)
		{
			if (is32Bit)
			{
				triangles.Add(reader.ReadInt32());
			}
			else
			{
				triangles.Add(reader.ReadUInt16());
			}
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		if (vertices.Count != colours.Count)
		{
			string text = ((item == null) ? ("R" + region.ToDebugLocation()) : ("I" + item.ToDebugLocation()));
			Log.Warning("Count mismatch in mesh " + text + "     " + vertices.Count + " vs " + colours.Count);
		}
		if (triangles.Count % 3 != 0)
		{
			string text2 = ((item == null) ? ("R" + region.ToDebugLocation()) : ("I" + item.ToDebugLocation()));
			Log.Warning("Triangle mismatch in mesh " + text2 + "     " + vertices.Count + " vs " + colours.Count);
		}
		GameUtils.SetMeshVertexAttributes(mesh);
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();
		mesh.SetColors(colours);
		if (region != null)
		{
			mesh.UploadMeshData(markNoLongerReadable: true);
		}
		if (item != null)
		{
			Bounds boundsFromVerts = GetBoundsFromVerts(vertices);
			bool flag2 = boundsFromVerts.size.z > 0f;
			bool flag3 = boundsFromVerts.size.x > 0f;
			if (!flag3 || !flag2)
			{
				item.State = DynamicItemState.Invalid;
				if (DynamicMeshManager.DoLog)
				{
					Log.Out($"Invalid mesh at {item.ToDebugLocation()} depth :{flag2} width: {flag3}. Marking for deletion");
				}
			}
		}
		MeshLists.ReturnList(cache);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mesh ReadMesh(int version, BinaryReader reader, UVRectTiling[] tilings, Mesh mesh, bool lockMesh, DynamicMeshItem item)
	{
		uint num = reader.ReadUInt32();
		bool flag = num > 65535 && DynamicMeshManager.Allow32BitMeshes;
		mesh.indexFormat = (flag ? IndexFormat.UInt32 : IndexFormat.UInt16);
		MeshLists list = MeshLists.GetList();
		List<Vector3> vertices = list.Vertices;
		List<Color> colours = list.Colours;
		List<int> triangles = list.Triangles;
		List<Vector2> uvs = list.Uvs;
		for (uint num2 = 0u; num2 < num; num2++)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		uint num3 = reader.ReadUInt32();
		for (uint num4 = 0u; num4 < num3; num4++)
		{
			int num5 = reader.ReadInt16();
			int num6 = reader.ReadByte();
			int num7 = ((num5 < tilings.Length) ? (tilings[num5].index + num6) : 0);
			bool flag2 = reader.ReadBoolean();
			uvs.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			colours.Add(new Color(0f, num7, 0f, flag2 ? 1 : 0));
		}
		uint num8 = reader.ReadUInt32();
		for (uint num9 = 0u; num9 < num8; num9++)
		{
			if (flag)
			{
				triangles.Add(reader.ReadInt32());
			}
			else
			{
				triangles.Add(reader.ReadUInt16());
			}
		}
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		if (item != null)
		{
			Bounds boundsFromVerts = GetBoundsFromVerts(vertices);
			bool flag3 = boundsFromVerts.size.z > 0f;
			bool flag4 = boundsFromVerts.size.x > 0f;
			if (!flag4 || !flag3)
			{
				item.State = DynamicItemState.Invalid;
				if (DynamicMeshManager.DoLog)
				{
					Log.Out($"Invalid mesh at {item.ToDebugLocation()} depth :{flag3} width: {flag4}. Marking for deletion");
				}
				MeshLists.ReturnList(list);
				return null;
			}
		}
		GameUtils.SetMeshVertexAttributes(mesh);
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();
		mesh.SetColors(colours);
		if (lockMesh)
		{
			mesh.UploadMeshData(markNoLongerReadable: true);
		}
		MeshLists.ReturnList(list);
		return mesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mesh ReadMeshTerrain(BinaryReader reader, Mesh mesh, MeshRenderer rend, bool lockMesh)
	{
		uint num = reader.ReadUInt32();
		bool flag = num > 65535 && DynamicMeshManager.Allow32BitMeshes;
		mesh.indexFormat = (flag ? IndexFormat.UInt32 : IndexFormat.UInt16);
		MeshLists list = MeshLists.GetList();
		List<Vector3> vertices = list.Vertices;
		List<Color> colours = list.Colours;
		List<List<int>> terrainTriangles = list.TerrainTriangles;
		List<Vector2> uvs = list.Uvs;
		List<Vector2> uvs2 = list.Uvs2;
		List<Vector2> uvs3 = list.Uvs3;
		List<Vector2> uvs4 = list.Uvs4;
		for (uint num2 = 0u; num2 < num; num2++)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		uint num3 = reader.ReadUInt32();
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
			if (terrainTriangles.Count < i + 1)
			{
				terrainTriangles.Add(new List<int>());
			}
			List<int> list2 = terrainTriangles[i];
			for (uint num10 = 0u; num10 < num9; num10++)
			{
				if (flag)
				{
					list2.Add(reader.ReadInt32());
				}
				else
				{
					list2.Add(reader.ReadUInt16());
				}
			}
		}
		uint num11 = reader.ReadUInt32();
		for (uint num12 = 0u; num12 < num11; num12++)
		{
			colours.Add(new Color((float)(int)reader.ReadUInt16() / 10000f + 1f, (float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
		}
		int num13 = rend.sharedMaterials.Length;
		if (!TerrainSharedMaterials.ContainsKey(num13))
		{
			Material[] array = new Material[num13];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = MeshDescription.meshes[5].material;
			}
			TerrainSharedMaterials.Add(num13, array);
		}
		rend.sharedMaterials = TerrainSharedMaterials[num13];
		mesh.SetVertices(vertices);
		mesh.subMeshCount = (int)num8;
		for (int k = 0; k < num8; k++)
		{
			mesh.SetTriangles(terrainTriangles[k], k);
		}
		GameUtils.SetMeshVertexAttributes(mesh);
		mesh.SetUVs(0, uvs);
		mesh.SetUVs(1, uvs2);
		mesh.SetUVs(2, uvs3);
		mesh.SetUVs(3, uvs4);
		mesh.RecalculateNormals();
		mesh.SetColors(colours);
		if (lockMesh)
		{
			mesh.UploadMeshData(markNoLongerReadable: true);
		}
		MeshLists.ReturnList(list);
		return mesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator ReadMeshTerrainCoroutine(BinaryReader reader, Mesh mesh, MeshRenderer rend, bool lockMesh, int maxTime, DynamicMeshRegion region, DynamicMeshItem item)
	{
		CurrentlyLoadingItem = ((region == null) ? ((DynamicMeshContainer)item) : ((DynamicMeshContainer)region));
		MeshLists cache = MeshLists.GetList();
		List<Vector3> vertices = cache.Vertices;
		List<List<int>> triangles = cache.TerrainTriangles;
		List<Color> colours = cache.Colours;
		List<Vector2> uvs = cache.Uvs;
		List<Vector2> uvs2 = cache.Uvs2;
		List<Vector2> uvs3 = cache.Uvs3;
		List<Vector2> uvs4 = cache.Uvs4;
		uint vertCount = reader.ReadUInt32();
		bool is32Bit = vertCount > 65535 && DynamicMeshManager.Allow32BitMeshes;
		mesh.indexFormat = (is32Bit ? IndexFormat.UInt32 : IndexFormat.UInt16);
		for (uint vCount = 0u; vCount < vertCount; vCount++)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		uint uvCount = reader.ReadUInt32();
		for (uint vCount = 0u; vCount < uvCount; vCount++)
		{
			uvs.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		uvCount = reader.ReadUInt32();
		for (uint vCount = 0u; vCount < uvCount; vCount++)
		{
			uvs2.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		uvCount = reader.ReadUInt32();
		for (uint vCount = 0u; vCount < uvCount; vCount++)
		{
			uvs3.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		uvCount = reader.ReadUInt32();
		for (uint vCount = 0u; vCount < uvCount; vCount++)
		{
			uvs4.Add(new Vector2((float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		uint submeshCount = reader.ReadUInt32();
		for (int x = 0; x < submeshCount; x++)
		{
			uint num = reader.ReadUInt32();
			if (triangles.Count < x + 1)
			{
				triangles.Add(new List<int>());
			}
			List<int> list = triangles[x];
			for (uint num2 = 0u; num2 < num; num2++)
			{
				if (is32Bit)
				{
					list.Add(reader.ReadInt32());
				}
				else
				{
					list.Add(reader.ReadUInt16());
				}
			}
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		uint colourCount = reader.ReadUInt32();
		for (uint vCount = 0u; vCount < colourCount; vCount++)
		{
			colours.Add(new Color((float)(int)reader.ReadUInt16() / 10000f + 1f, (float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f, (float)(int)reader.ReadUInt16() / 10000f));
			if (stop.ElapsedMilliseconds > maxTime)
			{
				yield return ReadMeshWait;
				stop.ResetAndRestart();
			}
		}
		try
		{
			int num3 = rend.sharedMaterials.Length;
			if (!TerrainSharedMaterials.ContainsKey(num3))
			{
				Material[] array = new Material[num3];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = MeshDescription.meshes[5].material;
				}
				TerrainSharedMaterials.Add(num3, array);
			}
			rend.sharedMaterials = TerrainSharedMaterials[num3];
		}
		catch (Exception)
		{
			bool num4 = region != null;
			string text = (num4 ? "R:" : "C:") + (item?.ToDebugLocation() ?? region.ToDebugLocation());
			Log.Warning("Setting shared material failed on terrain material. Reloading " + text);
			if (!num4)
			{
				DynamicMeshManager.Instance.AddItemLoadRequest(item, urgent: true);
			}
			MeshLists.ReturnList(cache);
			CurrentlyLoadingItem = null;
			yield break;
		}
		mesh.SetVertices(vertices);
		if (stop.ElapsedMilliseconds > maxTime)
		{
			yield return ReadMeshWait;
			stop.ResetAndRestart();
		}
		mesh.subMeshCount = (int)submeshCount;
		for (int j = 0; j < submeshCount; j++)
		{
			mesh.SetTriangles(triangles[j], j);
		}
		if (stop.ElapsedMilliseconds > maxTime)
		{
			yield return ReadMeshWait;
			stop.ResetAndRestart();
		}
		GameUtils.SetMeshVertexAttributes(mesh);
		mesh.SetUVs(0, uvs);
		mesh.SetUVs(1, uvs2);
		mesh.SetUVs(2, uvs3);
		mesh.SetUVs(3, uvs4);
		mesh.RecalculateNormals();
		mesh.SetColors(colours);
		mesh.UploadMeshData(lockMesh);
		MeshLists.ReturnList(cache);
		CurrentlyLoadingItem = null;
	}
}
