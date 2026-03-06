using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PrefabHelpers
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string cInnerBlockReplace = "imposterBlock";

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue cInnerBlockBVReplace;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i dim;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EnumInsideOutside[] eInsideOutside;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue[] blockIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] textureIdxOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool[] bTouched;

	public static Coroutine IteratePrefabs(bool _ignoreExcludeImposterCheck = false, Action<Prefab> _onPrefabLoaded = null, Action<PathAbstractions.AbstractedLocation, Prefab> _onChunksBuilt = null, Func<PathAbstractions.AbstractedLocation, bool> _prefabPathFilter = null, Func<Prefab, bool> _prefabContentFilter = null, Action _cleanupAction = null)
	{
		return ThreadManager.StartCoroutine(runBulk(_ignoreExcludeImposterCheck, _onPrefabLoaded, _onChunksBuilt, _prefabPathFilter, _prefabContentFilter, _cleanupAction));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator runBulk(bool _ignoreExcludeImposterCheck, Action<Prefab> _onPrefabLoaded, Action<PathAbstractions.AbstractedLocation, Prefab> _onChunksBuilt, Func<PathAbstractions.AbstractedLocation, bool> _prefabPathFilter, Func<Prefab, bool> _prefabContentFilter, Action _cleanupAction)
	{
		foreach (PathAbstractions.AbstractedLocation availablePaths in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList())
		{
			if ((_prefabPathFilter == null || _prefabPathFilter(availablePaths)) && PrefabEditModeManager.Instance.LoadVoxelPrefab(availablePaths, _bBulk: true, _ignoreExcludeImposterCheck) && (_prefabContentFilter == null || _prefabContentFilter(PrefabEditModeManager.Instance.VoxelPrefab)))
			{
				yield return processPrefab(_onPrefabLoaded, _onChunksBuilt, PrefabEditModeManager.Instance.VoxelPrefab, availablePaths);
			}
		}
		_cleanupAction?.Invoke();
		Log.Out("Processing prefabs done");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator processPrefab(Action<Prefab> _onPrefabLoaded, Action<PathAbstractions.AbstractedLocation, Prefab> _onChunksBuilt, Prefab _prefab, PathAbstractions.AbstractedLocation _path)
	{
		Log.Out("Processing " + _path.Name);
		_onPrefabLoaded?.Invoke(_prefab);
		ChunkCluster cc = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
		foreach (Chunk c in chunkArrayCopySync)
		{
			if (!cc.IsOnBorder(c) && !c.IsEmpty())
			{
				while (c.NeedsRegeneration || c.NeedsCopying)
				{
					yield return null;
				}
			}
		}
		_onChunksBuilt?.Invoke(_path, _prefab);
	}

	public static void Cleanup()
	{
		cInnerBlockBVReplace = BlockValue.Air;
		BlockShapeNew.bImposterGenerationActive = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Init()
	{
		cInnerBlockBVReplace = Block.GetBlockValue(cInnerBlockReplace);
	}

	public static void convert(Action _callbackOnDone = null)
	{
		SimplifyPrefab();
		ThreadManager.StartCoroutine(convertWaitForAllChunksBuilt(_callbackOnDone));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator convertWaitForAllChunksBuilt(Action _callbackOnDone)
	{
		Prefab prefab = PrefabEditModeManager.Instance.VoxelPrefab;
		if (prefab != null)
		{
			ChunkCluster cc = GameManager.Instance.World.ChunkCache;
			List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
			foreach (Chunk c in chunkArrayCopySync)
			{
				if (!cc.IsOnBorder(c) && !c.IsEmpty())
				{
					while (c.NeedsRegeneration || c.NeedsCopying)
					{
						yield return new WaitForEndOfFrame();
					}
				}
			}
			yield return new WaitForEndOfFrame();
			if (combine(_bCombineSliced: true))
			{
				export();
				UnityEngine.Object.Destroy(PrefabEditModeManager.Instance.ImposterPrefab);
				bool bTextureArray = MeshDescription.meshes[0].bTextureArray;
				PrefabEditModeManager.Instance.ImposterPrefab = SimpleMeshFile.ReadGameObject(prefab.location.FullPathNoExtension + ".mesh", 0f, null, bTextureArray);
				PrefabEditModeManager.Instance.ImposterPrefab.transform.name = prefab.PrefabName;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Done " + prefab.location);
			}
			else
			{
				Log.Out("Skipping " + prefab.location);
			}
		}
		_callbackOnDone?.Invoke();
	}

	public static void convertInsideOutside(Action _callbackOnDone = null)
	{
		ThreadManager.StartCoroutine(convertInsideOutsideWaitForAllChunksBuilt(_callbackOnDone));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator convertInsideOutsideWaitForAllChunksBuilt(Action _callbackOnDone)
	{
		ChunkCluster cc = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
		foreach (Chunk c in chunkArrayCopySync)
		{
			if (!cc.IsOnBorder(c) && !c.IsEmpty())
			{
				while (c.NeedsRegeneration || c.NeedsCopying)
				{
					yield return new WaitForSeconds(1f);
				}
			}
		}
		PrefabEditModeManager.Instance.SaveVoxelPrefab();
		PrefabEditModeManager.Instance.ClearVoxelPrefab();
		_callbackOnDone?.Invoke();
	}

	public static void SimplifyPrefab(bool _bOnlySimplify1 = false)
	{
		Prefab voxelPrefab = PrefabEditModeManager.Instance.VoxelPrefab;
		if (voxelPrefab == null)
		{
			return;
		}
		Init();
		new MicroStopwatch();
		MicroStopwatch microStopwatch = new MicroStopwatch();
		World world = GameManager.Instance.World;
		ChunkCluster chunkCache = world.ChunkCache;
		List<Chunk> chunkArrayCopySync = chunkCache.GetChunkArrayCopySync();
		dim.x = (chunkCache.ChunkMaxPos.x - chunkCache.ChunkMinPos.x + 1) * 16;
		dim.z = (chunkCache.ChunkMaxPos.y - chunkCache.ChunkMinPos.y + 1) * 16;
		dim.y = 0;
		foreach (Chunk item in chunkArrayCopySync)
		{
			dim.y = Utils.FastMax(dim.y, item.GetMaxHeight());
		}
		dim.y++;
		int num = dim.x * dim.y * dim.z;
		eInsideOutside = new EnumInsideOutside[num];
		bTouched = new bool[num];
		blockIds = new BlockValue[num];
		textureIdxOverride = new byte[num];
		int num2 = -chunkCache.ChunkMinPos.x * 16;
		int num3 = -chunkCache.ChunkMinPos.y * 16;
		foreach (Chunk item2 in chunkArrayCopySync)
		{
			Vector3i pos = item2.ToWorldPos();
			pos.x += num2;
			pos.z += num3;
			item2.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int x, int y, int z, BlockValue bv) =>
			{
				if (y < dim.y)
				{
					blockIds[x + pos.x + y * dim.x + (z + pos.z) * dim.x * dim.y] = bv;
				}
			}, _bIncludeChilds: true);
			item2.ClearWater();
		}
		simplifyPrefab1();
		if (!voxelPrefab.bExcludePOICulling)
		{
			insideOutsidePrefab();
		}
		else
		{
			for (int num4 = 0; num4 < eInsideOutside.Length; num4++)
			{
				eInsideOutside[num4] = EnumInsideOutside.Outside;
			}
		}
		if (!_bOnlySimplify1)
		{
			simplifyPrefab2();
		}
		foreach (Chunk item3 in chunkArrayCopySync)
		{
			Vector3i vector3i = item3.ToWorldPos();
			for (int num5 = dim.y - 1; num5 >= 0; num5--)
			{
				for (int num6 = 0; num6 < 16; num6++)
				{
					int num7 = (vector3i.y + num5) * dim.x + (vector3i.z + num6 + num3) * dim.x * dim.y;
					for (int num8 = 0; num8 < 16; num8++)
					{
						int num9 = vector3i.x + num8 + num2 + num7;
						if (eInsideOutside[num9] == EnumInsideOutside.Inside)
						{
							item3.SetBlock(world, num8, num5, num6, cInnerBlockBVReplace);
							item3.SetDensity(num8, num5, num6, MarchingCubes.DensityAir);
							continue;
						}
						item3.SetDensity(num8, num5, num6, MarchingCubes.DensityAir);
						if (bTouched[num9])
						{
							item3.SetBlock(world, num8, num5, num6, blockIds[num9]);
							long num10 = textureIdxOverride[num9];
							if (num10 != 0L && item3.GetTextureFull(num8, num5, num6) == 0L)
							{
								long texturefull = num10 | (num10 << 8) | (num10 << 16) | (num10 << 24) | (num10 << 32) | (num10 << 40);
								item3.SetTextureFull(num8, num5, num6, texturefull);
							}
						}
					}
				}
			}
		}
		Log.Out("SimplifyPrefab {0}, time {1}", dim, microStopwatch.ElapsedMilliseconds);
		rebuildMesh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void simplifyPrefab1()
	{
		bool flag = PrefabEditModeManager.Instance.VoxelPrefab.PrefabName.Contains("rwg_tile");
		int num = -(PrefabEditModeManager.Instance.VoxelPrefab.yOffset - 1);
		for (int i = 0; i < blockIds.Length; i++)
		{
			BlockValue blockValue = blockIds[i];
			if (blockValue.isair)
			{
				continue;
			}
			Block block = blockValue.Block;
			if ((i / dim.x % dim.y < num && !flag) || block.bImposterExclude || block.shape is BlockShapeDistantDecoTree || (block.IsTerrainDecoration && block.ImposterExchange == 0))
			{
				blockIds[i] = BlockValue.Air;
				bTouched[i] = true;
				continue;
			}
			if (block.ImposterExchange != 0)
			{
				byte rotation = blockValue.rotation;
				if (blockValue.ischild)
				{
					int num2 = blockValue.parentx + blockValue.parenty * dim.x + blockValue.parentz * dim.x * dim.y;
					rotation = blockIds[i + num2].rotation;
				}
				blockIds[i] = new BlockValue((uint)block.ImposterExchange);
				blockIds[i].rotation = rotation;
				bTouched[i] = true;
				textureIdxOverride[i] = block.ImposterExchangeTexIdx;
			}
			if (blockIds[i].Block.MeshIndex == 5)
			{
				blockIds[i] = BlockValue.Air;
				bTouched[i] = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void simplifyPrefab2()
	{
		for (int i = 0; i < blockIds.Length; i++)
		{
			BlockValue blockValue = blockIds[i];
			if (!blockValue.isair)
			{
				Block block = blockValue.Block;
				if (block.shape is BlockShapeModelEntity || block.shape is BlockShapeExt3dModel || block.MeshIndex != 0 || block.bImposterExcludeAndStop || block.blockMaterial.IsLiquid)
				{
					blockIds[i] = BlockValue.Air;
					bTouched[i] = true;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void insideOutsidePrefab()
	{
		int num = dim.x * dim.y;
		for (int i = 0; i < dim.z; i++)
		{
			for (int j = 0; j < dim.x; j++)
			{
				int num2 = j + i * num;
				for (int num3 = dim.y - 1; num3 >= 0; num3--)
				{
					int num4 = num2 + num3 * dim.x;
					eInsideOutside[num4] = EnumInsideOutside.Outside;
					if (!blockIds[num4].Block.bImposterDontBlock)
					{
						eInsideOutside[num4] = EnumInsideOutside.Border;
						break;
					}
				}
			}
		}
		for (int k = 0; k < dim.z; k++)
		{
			for (int l = 0; l < dim.y; l++)
			{
				int num5 = l * dim.x + k * num;
				for (int m = 0; m < dim.x; m++)
				{
					int num6 = m + num5;
					eInsideOutside[num6] = EnumInsideOutside.Outside;
					if (!blockIds[num6].isair && !blockIds[num6].Block.bImposterDontBlock)
					{
						eInsideOutside[num6] = EnumInsideOutside.Border;
						break;
					}
				}
				for (int num7 = dim.x - 1; num7 >= 0; num7--)
				{
					int num8 = num7 + num5;
					eInsideOutside[num8] = EnumInsideOutside.Outside;
					if (!blockIds[num8].isair && !blockIds[num8].Block.bImposterDontBlock)
					{
						eInsideOutside[num8] = EnumInsideOutside.Border;
						break;
					}
				}
			}
		}
		for (int n = 0; n < dim.y; n++)
		{
			for (int num9 = 0; num9 < dim.x; num9++)
			{
				int num10 = num9 + n * dim.x;
				for (int num11 = 0; num11 < dim.z; num11++)
				{
					int num12 = num10 + num11 * num;
					eInsideOutside[num12] = EnumInsideOutside.Outside;
					if (!blockIds[num12].isair && !blockIds[num12].Block.bImposterDontBlock)
					{
						eInsideOutside[num12] = EnumInsideOutside.Border;
						break;
					}
				}
				for (int num13 = dim.z - 1; num13 >= 0; num13--)
				{
					int num14 = num10 + num13 * num;
					eInsideOutside[num14] = EnumInsideOutside.Outside;
					if (!blockIds[num14].isair && !blockIds[num14].Block.bImposterDontBlock)
					{
						eInsideOutside[num14] = EnumInsideOutside.Border;
						break;
					}
				}
			}
		}
	}

	public static void mergePrefab(bool _bRebuildMesh = true)
	{
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		dim = new Vector3i((chunkCache.ChunkMaxPos.x - chunkCache.ChunkMinPos.x + 1) * 16, 256, (chunkCache.ChunkMaxPos.y - chunkCache.ChunkMinPos.y + 1) * 16);
		eInsideOutside = new EnumInsideOutside[dim.x * dim.y * dim.z];
		bTouched = new bool[dim.x * dim.y * dim.z];
		blockIds = new BlockValue[dim.x * dim.y * dim.z];
		int[] oldBlockIds = new int[dim.x * dim.y * dim.z];
		int num = -chunkCache.ChunkMinPos.x * 16;
		int num2 = -chunkCache.ChunkMinPos.y * 16;
		List<Chunk> chunkArrayCopySync = chunkCache.GetChunkArrayCopySync();
		foreach (Chunk item in chunkArrayCopySync)
		{
			Vector3i wp = item.ToWorldPos(new Vector3i(num, 0, num2));
			item.LoopOverAllBlocks([PublicizedFrom(EAccessModifier.Internal)] (int x, int y, int z, BlockValue bv) =>
			{
				blockIds[x + wp.x + y * dim.x + (z + wp.z) * dim.x * dim.y] = bv;
				oldBlockIds[x + wp.x + y * dim.x + (z + wp.z) * dim.x * dim.y] = bv.type;
			}, _bIncludeChilds: true);
		}
		for (int num3 = 0; num3 < blockIds.Length; num3++)
		{
			BlockValue blockValue = blockIds[num3];
			if (blockValue.Block.MergeIntoId != 0)
			{
				blockIds[num3].type = blockValue.Block.MergeIntoId;
				bTouched[num3] = true;
			}
		}
		foreach (Chunk item2 in chunkArrayCopySync)
		{
			for (int num4 = 0; num4 < 16; num4++)
			{
				for (int num5 = 0; num5 < 16; num5++)
				{
					for (int num6 = 252; num6 > 0; num6--)
					{
						Vector3i vector3i = item2.ToWorldPos(new Vector3i(num4 + num, num6, num5 + num2));
						int num7 = vector3i.x + num6 * dim.x + vector3i.z * dim.x * dim.y;
						if (bTouched[num7])
						{
							item2.SetBlock(GameManager.Instance.World, num4, num6, num5, blockIds[num7]);
							if (Block.list[oldBlockIds[num7]].MergeIntoTexIds != null)
							{
								long num8 = 0L;
								long textureFull = item2.GetTextureFull(num4, num6, num5);
								int[] mergeIntoTexIds = Block.list[oldBlockIds[num7]].MergeIntoTexIds;
								for (int num9 = 0; num9 < 6; num9++)
								{
									num8 = (((textureFull & (255L << num9 * 8)) != 0L) ? (num8 | (textureFull & (255L << num9 * 8))) : (num8 | ((long)mergeIntoTexIds[num9] << num9 * 8)));
								}
								item2.SetTextureFull(num4, num6, num5, num8);
							}
						}
					}
				}
			}
		}
		if (_bRebuildMesh)
		{
			rebuildMesh();
		}
	}

	public static void cull()
	{
		Init();
		MicroStopwatch microStopwatch = new MicroStopwatch();
		new MicroStopwatch();
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		int num = 0;
		int num2 = 0;
		List<Chunk> chunkArrayCopySync = chunkCache.GetChunkArrayCopySync();
		Log.Out("copy from took " + microStopwatch.ElapsedMilliseconds);
		microStopwatch.ResetAndRestart();
		PrefabEditModeManager.Instance.UpdateMinMax();
		eInsideOutside = PrefabEditModeManager.Instance.VoxelPrefab.UpdateInsideOutside(PrefabEditModeManager.Instance.minPos, PrefabEditModeManager.Instance.maxPos);
		Log.Out("insideOutsidePrefab took " + microStopwatch.ElapsedMilliseconds);
		microStopwatch.ResetAndRestart();
		Vector3i size = PrefabEditModeManager.Instance.VoxelPrefab.size;
		Vector3i minPos = PrefabEditModeManager.Instance.minPos;
		Vector3i maxPos = PrefabEditModeManager.Instance.maxPos;
		int num3 = 0;
		foreach (Chunk item in chunkArrayCopySync)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					for (int num4 = 252; num4 > 0; num4--)
					{
						Vector3i vector3i = item.ToWorldPos(new Vector3i(i + num, num4, j + num2));
						if (vector3i.x >= minPos.x && vector3i.y >= minPos.y && vector3i.z >= minPos.z && vector3i.x <= maxPos.x && vector3i.y <= maxPos.y && vector3i.z <= maxPos.z)
						{
							int num5 = vector3i.x - minPos.x + (num4 - minPos.y) * size.x + (vector3i.z - minPos.z) * size.x * size.y;
							if (eInsideOutside[num5] == EnumInsideOutside.Inside)
							{
								num3++;
								item.SetBlock(GameManager.Instance.World, i, num4, j, cInnerBlockBVReplace);
								item.SetDensity(i, num4, j, MarchingCubes.DensityAir);
							}
						}
					}
				}
			}
		}
		Debug.LogError("COUNT: " + num3);
		foreach (Chunk item2 in chunkArrayCopySync)
		{
			item2.NeedsRegeneration = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void rebuildMesh()
	{
		BlockShapeNew.bImposterGenerationActive = true;
		foreach (Chunk item in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
		{
			item.NeedsRegeneration = true;
		}
	}

	public static void export()
	{
		using Stream baseStream = SdFile.Create(PrefabEditModeManager.Instance.VoxelPrefab.location.FullPathNoExtension + ".mesh");
		using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter.SetBaseStream(baseStream);
		SimpleMeshFile.WriteGameObject(pooledBinaryWriter, PrefabEditModeManager.Instance.ImposterPrefab);
	}

	public static bool combine(bool _bCombineSliced)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			return false;
		}
		Dictionary<string, List<CombineInstance>> dictionary = null;
		if (_bCombineSliced)
		{
			dictionary = new Dictionary<string, List<CombineInstance>>();
		}
		List<MeshFilter> list = new List<MeshFilter>();
		List<ChunkGameObject> usedChunkGameObjects = GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects();
		for (int i = 0; i < usedChunkGameObjects.Count; i++)
		{
			MeshFilter[] componentsInChildren = usedChunkGameObjects[i].GetComponentsInChildren<MeshFilter>();
			list.AddRange(componentsInChildren);
		}
		UnityEngine.Object.Destroy(PrefabEditModeManager.Instance.ImposterPrefab);
		PrefabEditModeManager.Instance.ImposterPrefab = new GameObject(PrefabEditModeManager.Instance.VoxelPrefab.PrefabName);
		List<CombineInstance> list2 = new List<CombineInstance>();
		int num = 0;
		int num2 = 0;
		while (num2 < list.Count)
		{
			Mesh sharedMesh = list[num2].sharedMesh;
			if (sharedMesh == null || sharedMesh.vertexCount == 0)
			{
				num2++;
				continue;
			}
			if (!_bCombineSliced)
			{
				if (num + sharedMesh.vertexCount > 65000)
				{
					Mesh mesh = new Mesh();
					mesh.CombineMeshes(list2.ToArray());
					combine_createSubGameObject(null, mesh, Vector3.zero);
					list2.Clear();
					num = 0;
				}
				num += sharedMesh.vertexCount;
				list2.Add(new CombineInstance
				{
					mesh = sharedMesh,
					transform = list[num2].transform.localToWorldMatrix
				});
			}
			else
			{
				string name = list[num2].transform.parent.parent.name;
				if (!name.StartsWith("Chunk_"))
				{
					num2++;
					continue;
				}
				name = name.Substring("Chunk_".Length);
				string[] array = name.Split(',');
				int num3 = int.Parse(array[0]);
				int num4 = int.Parse(array[1]);
				name = string.Empty + Utils.Fastfloor((float)num3 / 2f) + "," + Utils.Fastfloor((float)num4 / 2f);
				if (!dictionary.TryGetValue(name, out var value))
				{
					value = (dictionary[name] = new List<CombineInstance>());
				}
				int num5 = 0;
				for (int j = 0; j < value.Count; j++)
				{
					num5 += value[j].mesh.vertexCount;
				}
				if (num5 + sharedMesh.vertexCount > 65000)
				{
					Mesh mesh2 = new Mesh();
					mesh2.CombineMeshes(value.ToArray());
					combine_createSubGameObject(name, mesh2, Vector3.zero);
					value.Clear();
				}
				CombineInstance item = new CombineInstance
				{
					mesh = sharedMesh,
					transform = list[num2].transform.localToWorldMatrix
				};
				value.Add(item);
			}
			list[num2].gameObject.SetActive(value: false);
			num2++;
		}
		if (!_bCombineSliced)
		{
			if (num > 0)
			{
				Mesh mesh3 = new Mesh();
				mesh3.CombineMeshes(list2.ToArray());
				combine_createSubGameObject(null, mesh3, Vector3.zero);
			}
		}
		else
		{
			foreach (KeyValuePair<string, List<CombineInstance>> item2 in dictionary)
			{
				Mesh mesh4 = new Mesh();
				mesh4.CombineMeshes(item2.Value.ToArray());
				combine_createSubGameObject(item2.Key, mesh4, Vector3.zero);
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject combine_createSubGameObject(string _goName, Mesh _mesh, Vector3 _goPosition)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = PrefabEditModeManager.Instance.ImposterPrefab.transform;
		gameObject.transform.localPosition = _goPosition;
		gameObject.AddComponent<MeshRenderer>().sharedMaterial = MeshDescription.GetOpaqueMaterial();
		gameObject.AddComponent<MeshFilter>().mesh = _mesh;
		_mesh.RecalculateNormals();
		Vector3[] normals = _mesh.normals;
		Vector3[] vertices = _mesh.vertices;
		bool[] array = new bool[vertices.Length];
		bool flag = false;
		float num = (float)(-PrefabEditModeManager.Instance.VoxelPrefab.yOffset + 4) + 0.25f;
		for (int i = 0; i < normals.Length; i++)
		{
			if (normals[i].y < -0.9f && vertices[i].y <= num)
			{
				array[i] = true;
				flag = true;
			}
		}
		Vector3 vector = new Vector3(0f, 3f, 0f);
		if (flag)
		{
			Vector2[] uv = _mesh.uv;
			_ = _mesh.colors;
			Vector2[] uv2 = _mesh.uv2;
			int[] array2 = new int[vertices.Length];
			List<Vector3> list = new List<Vector3>();
			List<Vector2> list2 = new List<Vector2>();
			List<Vector2> list3 = new List<Vector2>();
			int num2 = 0;
			for (int j = 0; j < array.Length; j++)
			{
				if (!array[j])
				{
					list.Add(vertices[j] + vector);
					list2.Add(uv[j]);
					list3.Add(uv2[j]);
				}
				else
				{
					num2++;
				}
				array2[j] = num2;
			}
			int[] indices = _mesh.GetIndices(0);
			List<int> list4 = new List<int>();
			for (int k = 0; k < indices.Length; k += 3)
			{
				if (!array[indices[k]] || !array[indices[k + 1]] || !array[indices[k + 2]])
				{
					list4.Add(indices[k] - array2[indices[k]]);
					list4.Add(indices[k + 1] - array2[indices[k + 1]]);
					list4.Add(indices[k + 2] - array2[indices[k + 2]]);
				}
			}
			_mesh.Clear();
			_mesh.SetVertices(list);
			_mesh.SetTriangles(list4, 0);
			_mesh.SetUVs(0, list2);
			_mesh.SetUVs(1, list3);
		}
		else
		{
			for (int l = 0; l < vertices.Length; l++)
			{
				vertices[l] += vector;
			}
			_mesh.vertices = vertices;
		}
		gameObject.transform.name = _goName ?? ("mesh_" + _mesh.vertexCount);
		return gameObject;
	}

	public static void DensityChange(int _densityMatch, int _densitySet)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		PrefabEditModeManager.Instance.UpdateMinMax();
		Vector3i minPos = PrefabEditModeManager.Instance.minPos;
		Vector3i maxPos = PrefabEditModeManager.Instance.maxPos;
		int num = 0;
		Vector3i pos = default(Vector3i);
		foreach (Chunk item in chunkArrayCopySync)
		{
			for (int i = 0; i < 16; i++)
			{
				pos.z = i;
				for (int j = 0; j < 16; j++)
				{
					pos.x = j;
					for (int num2 = 255; num2 >= 0; num2--)
					{
						pos.y = num2;
						Vector3i vector3i = item.ToWorldPos(pos);
						if (vector3i.x >= minPos.x && vector3i.y >= minPos.y && vector3i.z >= minPos.z && vector3i.x <= maxPos.x && vector3i.y <= maxPos.y && vector3i.z <= maxPos.z && item.GetBlockId(j, num2, i) != 0 && item.GetDensity(j, num2, i) == _densityMatch)
						{
							num++;
							item.SetDensity(j, num2, i, (sbyte)_densitySet);
						}
					}
				}
			}
		}
		Log.Out("DensityChange {0} chunks, {1} blocks, in {2}ms", chunkArrayCopySync.Count, num, (float)microStopwatch.ElapsedMicroseconds * 0.001f);
		foreach (Chunk item2 in chunkArrayCopySync)
		{
			item2.NeedsRegeneration = true;
		}
	}

	public static void SmoothPOI(int _passes, bool _land)
	{
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < _passes; i++)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Pass {i + 1}");
			List<BlockChangeInfo> list = new List<BlockChangeInfo>();
			foreach (Chunk item in chunkArrayCopySync)
			{
				if (item != null)
				{
					smoothChunk(list, item, chunkCache, _land);
				}
			}
			BlockToolSelection.Instance.BeginUndo(0);
			GameManager.Instance.SetBlocksRPC(list);
			BlockToolSelection.Instance.EndUndo(0);
			foreach (Chunk item2 in chunkArrayCopySync)
			{
				item2?.RecalcHeights();
			}
		}
		foreach (Chunk item3 in chunkArrayCopySync)
		{
			if (item3 != null)
			{
				item3.NeedsRegeneration = true;
			}
		}
		if (PrefabEditModeManager.Instance != null && PrefabEditModeManager.Instance.IsActive())
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void smoothChunk(List<BlockChangeInfo> _listBci, Chunk _chunk, ChunkCluster _regionCache, bool _bLand)
	{
		bool selectionActive = BlockToolSelection.Instance.SelectionActive;
		Vector3i selectionMin = BlockToolSelection.Instance.SelectionMin;
		Vector3i vector3i = selectionMin + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		Chunk[] array = new Chunk[4]
		{
			_regionCache.GetChunkSync(_chunk.X, _chunk.Z + 1),
			_regionCache.GetChunkSync(_chunk.X, _chunk.Z - 1),
			_regionCache.GetChunkSync(_chunk.X + 1, _chunk.Z),
			_regionCache.GetChunkSync(_chunk.X - 1, _chunk.Z)
		};
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				if (selectionActive)
				{
					Vector3i vector3i2 = _chunk.ToWorldPos(new Vector3i(i, 0, j));
					if (vector3i2.x < selectionMin.x || vector3i2.x > vector3i.x || vector3i2.z < selectionMin.z || vector3i2.z > vector3i.z)
					{
						continue;
					}
				}
				int num = (int)_chunk.GetHeight(i, j) + ((!_bLand) ? 1 : 0);
				int num2 = 0;
				num2 = ((j + 1 < 16) ? (num2 + _chunk.GetDensity(i, num, j + 1)) : ((array[0] == null) ? (num2 + _chunk.GetDensity(i, num, j)) : (num2 + array[0].GetDensity(i, num, 0))));
				num2 = ((j - 1 >= 0) ? (num2 + _chunk.GetDensity(i, num, j - 1)) : ((array[1] == null) ? (num2 + _chunk.GetDensity(i, num, j)) : (num2 + array[1].GetDensity(i, num, 15))));
				num2 = ((i + 1 < 16) ? (num2 + _chunk.GetDensity(i + 1, num, j)) : ((array[2] == null) ? (num2 + _chunk.GetDensity(i, num, j)) : (num2 + array[2].GetDensity(0, num, j))));
				num2 = ((i - 1 >= 0) ? (num2 + _chunk.GetDensity(i - 1, num, j)) : ((array[3] == null) ? (num2 + _chunk.GetDensity(i, num, j)) : (num2 + array[3].GetDensity(15, num, j))));
				sbyte b = (sbyte)((float)num2 / 4f);
				BlockValue block = _chunk.GetBlock(i, num, j);
				bool flag = block.Block.shape.IsTerrain();
				if (b > -1 && !block.Equals(BlockValue.Air) && flag)
				{
					_listBci.Add(new BlockChangeInfo(_chunk.ToWorldPos(i, num - 1, j), block, _updateLight: false));
					_listBci.Add(new BlockChangeInfo(_chunk.ToWorldPos(i, num, j), BlockValue.Air, b));
				}
				else if (b < 0 && block.Equals(BlockValue.Air))
				{
					_listBci.Add(new BlockChangeInfo(_chunk.ToWorldPos(i, num, j), _chunk.GetBlock(i, num - 1, j), b));
					if (num >= 2 && _chunk.GetBlock(i, num - 1, j).Block.shape.IsTerrain())
					{
						_listBci.Add(new BlockChangeInfo(_chunk.ToWorldPos(i, num - 1, j), _chunk.GetBlock(i, num - 2, j), MarchingCubes.DensityTerrain));
					}
				}
				else if (flag || block.Equals(BlockValue.Air))
				{
					_listBci.Add(new BlockChangeInfo(_chunk.ToWorldPos(i, num, j), block, b));
				}
			}
		}
	}
}
