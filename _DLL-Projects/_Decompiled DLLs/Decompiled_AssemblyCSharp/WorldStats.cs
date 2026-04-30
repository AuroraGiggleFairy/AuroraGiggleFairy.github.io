using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldStats
{
	public class MeshStats
	{
		public readonly int Vertices;

		public readonly int Triangles;

		public MeshStats(int _vertices, int _triangles)
		{
			Vertices = _vertices;
			Triangles = _triangles;
		}

		public static MeshStats FromProperties(DynamicProperties _props)
		{
			int vertices = _props.GetInt("Vertices");
			int triangles = _props.GetInt("Triangles");
			return new MeshStats(vertices, triangles);
		}

		public DynamicProperties ToProperties()
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			DictionarySave<string, string> values = dynamicProperties.Values;
			int vertices = Vertices;
			values["Vertices"] = vertices.ToString();
			DictionarySave<string, string> values2 = dynamicProperties.Values;
			vertices = Triangles;
			values2["Triangles"] = vertices.ToString();
			return dynamicProperties;
		}
	}

	public readonly MeshStats BlockEntities;

	public readonly MeshStats[] ChunkMeshes;

	public readonly float LightsVolume;

	public int TotalVertices
	{
		get
		{
			int num = 0;
			if (BlockEntities != null)
			{
				num = BlockEntities.Vertices;
			}
			if (ChunkMeshes != null)
			{
				MeshStats[] chunkMeshes = ChunkMeshes;
				foreach (MeshStats meshStats in chunkMeshes)
				{
					if (meshStats != null)
					{
						num += meshStats.Vertices;
					}
				}
			}
			return num;
		}
	}

	public int TotalTriangles
	{
		get
		{
			int num = 0;
			if (BlockEntities != null)
			{
				num = BlockEntities.Triangles;
			}
			if (ChunkMeshes != null)
			{
				MeshStats[] chunkMeshes = ChunkMeshes;
				foreach (MeshStats meshStats in chunkMeshes)
				{
					if (meshStats != null)
					{
						num += meshStats.Triangles;
					}
				}
			}
			return num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldStats(MeshStats _blockEntities, MeshStats[] _chunkMeshes, float _lightsVolume)
	{
		BlockEntities = _blockEntities;
		ChunkMeshes = _chunkMeshes;
		LightsVolume = _lightsVolume;
	}

	public static WorldStats FromProperties(DynamicProperties _props)
	{
		MeshStats blockEntities = null;
		if (_props.Classes.TryGetValue("BlockEntities", out var _value))
		{
			blockEntities = MeshStats.FromProperties(_value);
		}
		MeshStats[] array = new MeshStats[(MeshDescription.meshes.Length != 0) ? MeshDescription.meshes.Length : 20];
		for (int i = 0; i < array.Length; i++)
		{
			if (_props.Classes.TryGetValue("ChunkMeshes" + i, out _value))
			{
				array[i] = MeshStats.FromProperties(_value);
			}
		}
		float _result = 0f;
		if (_props.Values.TryGetValue("LightsVolume", out var _value2) && !StringParsers.TryParseFloat(_value2, out _result))
		{
			Log.Warning("Could not parse LightsVolume string '" + _value2 + "'");
		}
		return new WorldStats(blockEntities, array, _result);
	}

	public DynamicProperties ToProperties()
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		if (BlockEntities != null)
		{
			dynamicProperties.Classes["BlockEntities"] = BlockEntities.ToProperties();
		}
		if (ChunkMeshes != null)
		{
			for (int i = 0; i < ChunkMeshes.Length; i++)
			{
				if (ChunkMeshes[i] != null)
				{
					dynamicProperties.Classes["ChunkMeshes" + i] = ChunkMeshes[i].ToProperties();
				}
			}
		}
		dynamicProperties.Values["LightsVolume"] = LightsVolume.ToCultureInvariantString();
		dynamicProperties.Values["TotalVertices"] = TotalVertices.ToString();
		dynamicProperties.Values["TotalTriangles"] = TotalTriangles.ToString();
		return dynamicProperties;
	}

	public static WorldStats CaptureWorldStats()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		IList<ChunkGameObject> displayedChunkGameObjects = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjects();
		int _verts = 0;
		int _tris = 0;
		float _lightsVolume = 0f;
		int[] array = new int[MeshDescription.meshes.Length];
		int[] array2 = new int[MeshDescription.meshes.Length];
		foreach (ChunkGameObject item in displayedChunkGameObjects)
		{
			Chunk chunk = item.GetChunk();
			if (!item.blockEntitiesParentT)
			{
				Log.Warning("{0} WorldStats CaptureWorldStats cgo {1}, null", Time.frameCount, chunk);
				continue;
			}
			foreach (Transform item2 in item.blockEntitiesParentT)
			{
				calcLightsVolume(item2.gameObject, ref _lightsVolume);
				calcComplexityGameObject(item2.gameObject, ref _verts, ref _tris, _onlyActive: true, chunk);
			}
			for (int i = 0; i < 16; i++)
			{
				ChunkGameObjectLayer layer = item.GetLayer(i);
				if (layer == null)
				{
					continue;
				}
				for (int j = 0; j < layer.m_MeshFilter.Length; j++)
				{
					MeshFilter[] array3 = layer.m_MeshFilter[j];
					if (array3 == null)
					{
						continue;
					}
					MeshFilter[] array4 = array3;
					foreach (MeshFilter meshFilter in array4)
					{
						if (meshFilter != null)
						{
							Mesh sharedMesh = meshFilter.sharedMesh;
							if (sharedMesh != null)
							{
								array[j] += sharedMesh.vertexCount;
								array2[j] += sharedMesh.triangles.Length / 3;
							}
						}
					}
				}
			}
		}
		MeshStats[] array5 = new MeshStats[array.Length];
		for (int l = 0; l < array.Length; l++)
		{
			array5[l] = new MeshStats(array[l], array2[l]);
		}
		WorldStats result = new WorldStats(new MeshStats(_verts, _tris), array5, _lightsVolume);
		Log.Out("Measuring took {0} ms", microStopwatch.ElapsedMilliseconds);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void calcLightsVolume(GameObject _go, ref float _lightsVolume)
	{
		LightLOD componentInChildren = _go.GetComponentInChildren<LightLOD>();
		if (!(componentInChildren == null) && componentInChildren.bSwitchedOn)
		{
			_lightsVolume += 4.1887903f * Mathf.Pow(componentInChildren.lightRange, 3f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void calcComplexityGameObject(GameObject _go, ref int _verts, ref int _tris, bool _onlyActive = true, Chunk _onChunk = null)
	{
		int num = 0;
		int num2 = 0;
		BlockEntityData blockEntity = _onChunk.GetBlockEntity(_go.transform);
		if (blockEntity == null)
		{
			Log.Warning("GameObject without BlockEntity: " + _go.GetGameObjectPath());
		}
		else if (blockEntity.blockValue.Block.IsSleeperBlock)
		{
			return;
		}
		HashSet<MeshRenderer> hashSet = new HashSet<MeshRenderer>();
		LODGroup[] componentsInChildren = _go.transform.GetComponentsInChildren<LODGroup>(!_onlyActive);
		foreach (LODGroup lODGroup in componentsInChildren)
		{
			if (lODGroup == null)
			{
				continue;
			}
			LOD[] lODs = lODGroup.GetLODs();
			for (int j = 0; j < lODs.Length; j++)
			{
				Renderer[] renderers = lODs[j].renderers;
				foreach (Renderer renderer in renderers)
				{
					if (renderer == null)
					{
						continue;
					}
					MeshRenderer meshRenderer = renderer as MeshRenderer;
					if (meshRenderer == null)
					{
						continue;
					}
					hashSet.Add(meshRenderer);
					if (j != 0 || !meshRenderer.gameObject.activeInHierarchy)
					{
						continue;
					}
					MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
					if (component == null)
					{
						continue;
					}
					Mesh sharedMesh = component.sharedMesh;
					if (!sharedMesh)
					{
						continue;
					}
					if (!sharedMesh.isReadable)
					{
						EntityMeshCache component2 = _go.GetComponent<EntityMeshCache>();
						if (component2 != null && component2.TryGetMeshData(sharedMesh.name, out var data))
						{
							num += data.vertexCount;
							num2 += data.triCount;
						}
					}
					else
					{
						num += sharedMesh.vertexCount;
						num2 += sharedMesh.triangles.Length;
					}
				}
			}
		}
		MeshRenderer[] componentsInChildren2 = _go.transform.GetComponentsInChildren<MeshRenderer>(!_onlyActive);
		foreach (MeshRenderer meshRenderer2 in componentsInChildren2)
		{
			if (hashSet.Contains(meshRenderer2))
			{
				continue;
			}
			MeshFilter component3 = meshRenderer2.GetComponent<MeshFilter>();
			if (component3 == null)
			{
				continue;
			}
			Mesh sharedMesh2 = component3.sharedMesh;
			if (!sharedMesh2)
			{
				continue;
			}
			if (!sharedMesh2.isReadable)
			{
				EntityMeshCache component4 = _go.GetComponent<EntityMeshCache>();
				if (component4 != null && component4.TryGetMeshData(sharedMesh2.name, out var data2))
				{
					num += data2.vertexCount;
					num2 += data2.triCount;
				}
			}
			else
			{
				num += sharedMesh2.vertexCount;
				num2 += sharedMesh2.triangles.Length;
			}
		}
		num2 /= 3;
		_verts += num;
		_tris += num2;
	}

	public static IEnumerator CaptureCameraStatsCo(XUi _xui)
	{
		PrefabEditModeManager.Instance.UpdateMinMax();
		Vector3i minPos = PrefabEditModeManager.Instance.minPos;
		Vector3i maxPos = PrefabEditModeManager.Instance.maxPos;
		BoundsInt boundsInt = new BoundsInt(minPos, maxPos);
		GameObject uiRootObj = Object.FindObjectOfType<UIRoot>().gameObject;
		Camera playerCam = _xui.playerUI.entityPlayer.playerCamera;
		GameObject camObj = new GameObject("StatsCam");
		Camera camera = camObj.AddComponent<Camera>();
		camera.CopyFrom(playerCam);
		camera.farClipPlane = 10000f;
		camObj.AddComponent<AudioListener>();
		Transform transform = camera.transform;
		transform.position = new Vector3(boundsInt.center.x, boundsInt.yMax + 2000, boundsInt.center.z);
		transform.eulerAngles = new Vector3(90f, 0f, 0f);
		float oldLodBias = QualitySettings.lodBias;
		ShadowQuality oldShadowQuality = QualitySettings.shadows;
		float oldShadowDistance = QualitySettings.shadowDistance;
		bool oldDisableChunkLoDs = GamePrefs.GetBool(EnumGamePrefs.OptionsDisableChunkLODs);
		ReflectionManager.ApplyOptions(useSimple: true);
		playerCam.gameObject.SetActive(value: false);
		uiRootObj.SetActive(value: false);
		QualitySettings.lodBias = 1000000f;
		QualitySettings.shadows = ShadowQuality.Disable;
		QualitySettings.shadowDistance = 1000000f;
		SkyManager.skyManager.gameObject.SetActive(value: false);
		GamePrefs.Set(EnumGamePrefs.OptionsDisableChunkLODs, _value: true);
		IList<ChunkGameObject> displayedChunkGameObjects = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjects();
		for (int i = 0; i < displayedChunkGameObjects.Count; i++)
		{
			displayedChunkGameObjects[i].CheckLODs();
		}
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		ReflectionManager.ApplyOptions();
		playerCam.gameObject.SetActive(value: true);
		uiRootObj.SetActive(value: true);
		QualitySettings.lodBias = oldLodBias;
		QualitySettings.shadows = oldShadowQuality;
		QualitySettings.shadowDistance = oldShadowDistance;
		SkyManager.skyManager.gameObject.SetActive(value: true);
		GamePrefs.Set(EnumGamePrefs.OptionsDisableChunkLODs, oldDisableChunkLoDs);
		Object.Destroy(camObj);
	}
}
