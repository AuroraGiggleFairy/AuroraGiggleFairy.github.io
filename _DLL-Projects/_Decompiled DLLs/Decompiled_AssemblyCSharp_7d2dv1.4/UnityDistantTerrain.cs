using System.Collections.Generic;
using UnityEngine;

public class UnityDistantTerrain
{
	public struct Config
	{
		public int DataWidth;

		public int DataHeight;

		public int DataTileSize;

		public int DataSteps;

		public int SplatSteps;

		public int MetersPerHeightPix;

		public int MaxHeight;

		public int PixelError;

		public int ChunkWorldSize => DataTileSize;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public class TerrainAndWater
	{
		public Terrain terrain;

		public GameObject waterPlane;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class VoxeChunkInfo
	{
		public bool isDirty;

		public byte a0;

		public byte a1;

		public byte a2;

		public byte a3;

		public bool IsEmpty()
		{
			if (a0 == 0 && a1 == 0 && a2 == 0)
			{
				return a3 == 0;
			}
			return false;
		}
	}

	public const int cSplatBorderSize = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileArea<float[,]> terrainHeights;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileArea<Color32[]> splat0Arr;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileArea<Color32[]> splat1Arr;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileArea<Color32[]> splat2Arr;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject cacheParentObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform cacheParentT;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject visibleParentObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform visibleParentT;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material terrainMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material waterMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Config terrainConfig;

	[PublicizedFrom(EAccessModifier.Private)]
	public int visibleChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileArea<TerrainAndWater> terrainTiles;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TerrainAndWater> terrainCache = new List<TerrainAndWater>();

	[PublicizedFrom(EAccessModifier.Private)]
	public TileArea<Texture2D>[] splatMapCache = new TileArea<Texture2D>[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public List<uint> tempKeysToRemove = new List<uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> tempPositions = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAnyTerrainDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isNeighborsDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClipTextureDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<uint> visibleTerrainTilesOfObservers = new HashSet<uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D terrainClipTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32[] terrainClipCols;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnityDistantTerrainWaterPlane waterPlane;

	[PublicizedFrom(EAccessModifier.Private)]
	public int waterChunks16x16Width;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] waterChunks16x16;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<uint> visibleVoxelChunks = new HashSet<uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] pixelErrorDistanceDiv = new float[5] { 60f, 75f, 90f, 160f, 300f };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] basemapDistances = new float[5] { 300f, 300f, 400f, 550f, 800f };

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 colClipTerrainAndWater = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 colClipWater = new Color32(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 colNoClip = new Color32(0, 0, 0, 0);

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, VoxeChunkInfo> dictChunkHeightsArr = new Dictionary<uint, VoxeChunkInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<uint> toRemove = new List<uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Terrain> toUpdateTerrain = new HashSet<Terrain>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i[] posAround = new Vector3i[9]
	{
		new Vector3i(0, 0, 0),
		new Vector3i(-1, 0, -1),
		new Vector3i(0, 0, -1),
		new Vector3i(1, 0, -1),
		new Vector3i(-1, 0, 0),
		new Vector3i(1, 0, 0),
		new Vector3i(-1, 0, 1),
		new Vector3i(0, 0, 1),
		new Vector3i(1, 0, 1)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public float[,] heights8x8 = new float[9, 9];

	public void Init(GameObject _parent, Config _terrainConfig, int _visibleChunks, Material _terrainMaterial, Material _waterMaterial, int _waterChunks16x16Width, byte[] _waterChunks16x16, ITileArea<float[,]> _heights, TileArea<Color32[]> _splat0, TileArea<Color32[]> _splat1, TileArea<Color32[]> _splat2)
	{
		Transform transform = _parent.transform;
		terrainConfig = _terrainConfig;
		visibleChunks = _visibleChunks;
		terrainMaterial = _terrainMaterial;
		waterMaterial = _waterMaterial;
		terrainHeights = _heights;
		splat0Arr = _splat0;
		splat1Arr = _splat1;
		splat2Arr = _splat2;
		waterChunks16x16Width = _waterChunks16x16Width;
		waterChunks16x16 = _waterChunks16x16;
		cacheParentObj = new GameObject("Cache");
		cacheParentT = cacheParentObj.transform;
		cacheParentT.SetParent(transform, worldPositionStays: false);
		cacheParentObj.SetActive(value: false);
		visibleParentObj = new GameObject("Terrain");
		visibleParentT = visibleParentObj.transform;
		visibleParentT.SetParent(transform, worldPositionStays: false);
		visibleParentT.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
		terrainTiles = new TileArea<TerrainAndWater>(terrainHeights.Config);
		for (int i = 0; i < visibleChunks * visibleChunks; i++)
		{
			AddTerrainAndWaterToCache();
		}
		if (splat0Arr != null)
		{
			for (int j = 0; j < splatMapCache.Length; j++)
			{
				splatMapCache[j] = new TileArea<Texture2D>(splat0Arr.config);
			}
		}
		waterPlane = new UnityDistantTerrainWaterPlane(_terrainConfig, _waterMaterial);
		Origin.Add(transform, -1);
		transform.position = -Origin.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddTerrainAndWaterToCache()
	{
		GameObject gameObject = new GameObject("Terrain");
		gameObject.transform.SetParent(cacheParentT, worldPositionStays: false);
		Terrain terrain = gameObject.AddComponent<Terrain>();
		GameObject gameObject2 = new GameObject("Water");
		gameObject2.transform.SetParent(terrain.transform, worldPositionStays: false);
		gameObject2.transform.localPosition = new Vector3(0f, 0.25f, 0f);
		gameObject2.AddComponent<MeshFilter>();
		gameObject2.AddComponent<MeshRenderer>();
		terrainCache.Add(new TerrainAndWater
		{
			terrain = terrain,
			waterPlane = gameObject2
		});
		TerrainData terrainData = new TerrainData
		{
			heightmapResolution = terrainConfig.DataTileSize / terrainConfig.DataSteps + 1,
			size = new Vector3(terrainConfig.DataTileSize, terrainConfig.MaxHeight, terrainConfig.DataTileSize)
		};
		if (terrainMaterial != null)
		{
			terrain.materialTemplate = terrainMaterial;
		}
		terrain.terrainData = terrainData;
		terrain.drawInstanced = false;
		terrain.drawTreesAndFoliage = false;
		terrain.heightmapMaximumLOD = 0;
		terrain.heightmapPixelError = terrainConfig.PixelError;
		int num = Mathf.Clamp(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality), 0, basemapDistances.Length - 1);
		float basemapDistance = basemapDistances[num];
		terrain.basemapDistance = basemapDistance;
		gameObject.SetActive(value: false);
	}

	public void Cleanup()
	{
		if ((bool)visibleParentT)
		{
			Origin.Remove(visibleParentT.parent);
		}
		for (int i = 0; i < splatMapCache.Length; i++)
		{
			TileArea<Texture2D> tileArea = splatMapCache[i];
			if (tileArea == null)
			{
				continue;
			}
			foreach (KeyValuePair<uint, Texture2D> datum in tileArea.Data)
			{
				Object.Destroy(datum.Value);
			}
			splatMapCache[i] = null;
		}
		Object.Destroy(cacheParentObj);
		Object.Destroy(visibleParentObj);
		terrainCache.Clear();
		Object.Destroy(terrainClipTexture);
		terrainClipTexture = null;
		visibleVoxelChunks.Clear();
		waterPlane.Cleanup();
		isAnyTerrainDirty = false;
		terrainHeights.Cleanup();
	}

	public void FrameUpdate(EntityPlayerLocal _player)
	{
		BuildAroundPos(_player.position);
		UpdateChunks();
		UpdateTextureApply();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildAroundPos(Vector3 _position)
	{
		tempPositions.Clear();
		tempPositions.Add(_position);
		BuildAroundPos(tempPositions);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildAroundPos(List<Vector3> _positions)
	{
		int chunkWorldSize = terrainConfig.ChunkWorldSize;
		float num = 0f;
		int num2 = (visibleChunks - 1) / 2;
		int num3 = num2;
		if ((visibleChunks & 1) == 0)
		{
			num = (float)chunkWorldSize * -0.5f;
			num3++;
		}
		visibleTerrainTilesOfObservers.Clear();
		for (int i = 0; i < _positions.Count; i++)
		{
			int num4 = Utils.Fastfloor((_positions[i].x + num) / (float)chunkWorldSize);
			int num5 = Utils.Fastfloor((_positions[i].z + num) / (float)chunkWorldSize);
			int num6 = num4 - num2;
			int num7 = num4 + num3;
			int num8 = num5 - num2;
			int num9 = num5 + num3;
			for (int j = num6; j <= num7; j++)
			{
				for (int k = num8; k <= num9; k++)
				{
					visibleTerrainTilesOfObservers.Add(TileAreaUtils.MakeKey(j, k));
				}
			}
		}
		tempKeysToRemove.Clear();
		foreach (KeyValuePair<uint, TerrainAndWater> datum in terrainTiles.Data)
		{
			if (!visibleTerrainTilesOfObservers.Contains(datum.Key))
			{
				tempKeysToRemove.Add(datum.Key);
			}
		}
		for (int l = 0; l < tempKeysToRemove.Count; l++)
		{
			uint key = tempKeysToRemove[l];
			TerrainAndWater terrainAndWater = terrainTiles[key];
			terrainTiles.Remove(key);
			terrainCache.Add(terrainAndWater);
			terrainAndWater.waterPlane.gameObject.SetActive(value: false);
			terrainAndWater.terrain.gameObject.SetActive(value: false);
			terrainAndWater.terrain.transform.SetParent(cacheParentT, worldPositionStays: false);
		}
		bool flag = false;
		int num10 = -(terrainConfig.DataWidth / terrainConfig.DataTileSize) / 2;
		int v = terrainConfig.DataWidth / terrainConfig.DataTileSize / 2 - 1;
		v = Utils.FastMax(0, v);
		int num11 = -(terrainConfig.DataHeight / terrainConfig.DataTileSize) / 2;
		int v2 = terrainConfig.DataHeight / terrainConfig.DataTileSize / 2 - 1;
		v2 = Utils.FastMax(0, v2);
		float num12 = (float)chunkWorldSize * 0.5f;
		float num13 = pixelErrorDistanceDiv[GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality)];
		foreach (uint visibleTerrainTilesOfObserver in visibleTerrainTilesOfObservers)
		{
			int tileXPos = TileAreaUtils.GetTileXPos(visibleTerrainTilesOfObserver);
			int tileZPos = TileAreaUtils.GetTileZPos(visibleTerrainTilesOfObserver);
			TerrainAndWater terrainAndWater2 = terrainTiles[visibleTerrainTilesOfObserver];
			if (terrainAndWater2 != null)
			{
				float num14 = float.MaxValue;
				for (int m = 0; m < _positions.Count; m++)
				{
					float num15 = _positions[m].x - ((float)(tileXPos * chunkWorldSize) + num12);
					float num16 = _positions[m].z - ((float)(tileZPos * chunkWorldSize) + num12);
					float num17 = num15 * num15 + num16 * num16;
					if (num17 < num14)
					{
						num14 = num17;
					}
				}
				float num18 = 5f;
				if (num14 > 102400f)
				{
					float num19 = Mathf.Sqrt(num14);
					num18 += (num19 - 320f) / num13;
				}
				terrainAndWater2.terrain.heightmapPixelError = (int)num18;
			}
			else if (tileXPos >= num10 && tileXPos <= v && tileZPos >= num11 && tileZPos <= v2)
			{
				terrainAndWater2 = CreateAndConfigureTerrain(tileXPos, tileZPos);
				terrainAndWater2.terrain.transform.SetParent(visibleParentT, worldPositionStays: false);
				terrainTiles[visibleTerrainTilesOfObserver] = terrainAndWater2;
				flag = true;
				isNeighborsDirty = true;
				break;
			}
		}
		if (!flag && isNeighborsDirty)
		{
			isNeighborsDirty = false;
			foreach (uint visibleTerrainTilesOfObserver2 in visibleTerrainTilesOfObservers)
			{
				TerrainAndWater terrainAndWater3 = terrainTiles[visibleTerrainTilesOfObserver2];
				if (terrainAndWater3 != null)
				{
					int tileXPos2 = TileAreaUtils.GetTileXPos(visibleTerrainTilesOfObserver2);
					int tileZPos2 = TileAreaUtils.GetTileZPos(visibleTerrainTilesOfObserver2);
					TerrainAndWater terrainAndWater4 = terrainTiles[TileAreaUtils.MakeKey(tileXPos2 - 1, tileZPos2)];
					TerrainAndWater terrainAndWater5 = terrainTiles[TileAreaUtils.MakeKey(tileXPos2, tileZPos2 + 1)];
					TerrainAndWater terrainAndWater6 = terrainTiles[TileAreaUtils.MakeKey(tileXPos2 + 1, tileZPos2)];
					TerrainAndWater terrainAndWater7 = terrainTiles[TileAreaUtils.MakeKey(tileXPos2, tileZPos2 - 1)];
					terrainAndWater3.terrain.SetNeighbors(terrainAndWater4?.terrain, terrainAndWater5?.terrain, terrainAndWater6?.terrain, terrainAndWater7?.terrain);
				}
			}
		}
		DynamicMeshManager.UpdateDistantTerrainBounds(terrainTiles, terrainConfig);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TerrainAndWater GetFromCacheOrCreate()
	{
		if (terrainCache.Count == 0)
		{
			AddTerrainAndWaterToCache();
		}
		TerrainAndWater result = terrainCache[terrainCache.Count - 1];
		terrainCache.RemoveAt(terrainCache.Count - 1);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TerrainAndWater CreateAndConfigureTerrain(int cx, int cz)
	{
		TerrainAndWater fromCacheOrCreate = GetFromCacheOrCreate();
		Terrain terrain = fromCacheOrCreate.terrain;
		GameObject gameObject = terrain.gameObject;
		gameObject.name = "Terrain " + cx + "/" + cz;
		terrain.transform.localPosition = new Vector3(cx * terrainConfig.ChunkWorldSize, 0f, cz * terrainConfig.ChunkWorldSize);
		float[,] heights = terrainHeights[cx, cz];
		terrain.terrainData.SetHeights(0, 0, heights);
		gameObject.SetActive(value: true);
		if (splat0Arr != null)
		{
			Material materialTemplate = terrain.materialTemplate;
			int num = terrainConfig.DataTileSize / terrainConfig.SplatSteps + 2;
			int height = num;
			Texture2D texture2D = splatMapCache[0][cx, cz];
			if (texture2D == null)
			{
				texture2D = new Texture2D(num, height, TextureFormat.RGBA32, mipChain: true);
				texture2D.SetPixels32(splat0Arr[cx, cz]);
				texture2D.filterMode = FilterMode.Bilinear;
				texture2D.wrapMode = TextureWrapMode.Clamp;
				texture2D.name = "Splat0 " + cx + "/" + cz;
				texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: true);
				splatMapCache[0][cx, cz] = texture2D;
			}
			Texture2D texture2D2 = splatMapCache[1][cx, cz];
			if (texture2D2 == null)
			{
				texture2D2 = new Texture2D(num, height, TextureFormat.RGBA32, mipChain: true);
				texture2D2.SetPixels32(splat1Arr[cx, cz]);
				texture2D2.filterMode = FilterMode.Bilinear;
				texture2D2.wrapMode = TextureWrapMode.Clamp;
				texture2D2.name = "Splat1 " + cx + "/" + cz;
				texture2D2.Apply(updateMipmaps: true, makeNoLongerReadable: true);
				splatMapCache[1][cx, cz] = texture2D2;
			}
			Texture2D texture2D3 = splatMapCache[2][cx, cz];
			if (texture2D3 == null)
			{
				texture2D3 = new Texture2D(num, height, TextureFormat.RGBA32, mipChain: true);
				texture2D3.SetPixels32(splat2Arr[cx, cz]);
				texture2D3.filterMode = FilterMode.Bilinear;
				texture2D3.wrapMode = TextureWrapMode.Clamp;
				texture2D3.name = "Splat2 " + cx + "/" + cz;
				texture2D3.Apply(updateMipmaps: true, makeNoLongerReadable: true);
				splatMapCache[2][cx, cz] = texture2D3;
			}
			materialTemplate.SetTexture("_CustomControl0", texture2D);
			materialTemplate.SetTexture("_CustomControl1", texture2D2);
			materialTemplate.SetTexture("_CustomControl2", texture2D3);
		}
		if (waterChunks16x16Width != 0)
		{
			waterPlane.createDynamicWaterPlane_Step1((cx - terrainHeights.Config.tileStart.x) * terrainConfig.ChunkWorldSize, (cz - terrainHeights.Config.tileStart.y) * terrainConfig.ChunkWorldSize, terrainConfig.ChunkWorldSize, waterChunks16x16Width, waterChunks16x16);
			waterPlane.createDynamicWaterPlane_Step2(fromCacheOrCreate.waterPlane, terrain.transform.parent);
		}
		return fromCacheOrCreate;
	}

	public void OnChunkUpdate(int _chunkX, int _chunkZ, bool _visible)
	{
		int num = terrainConfig.DataWidth / 16;
		if (_chunkX < -num / 2 || _chunkX >= num / 2)
		{
			return;
		}
		int num2 = terrainConfig.DataHeight / 16;
		if (_chunkZ < -num2 / 2 || _chunkZ >= num2 / 2)
		{
			return;
		}
		uint num3 = TileAreaUtils.MakeKey(_chunkX, _chunkZ);
		if (!dictChunkHeightsArr.TryGetValue(num3, out var value))
		{
			value = new VoxeChunkInfo();
			dictChunkHeightsArr[num3] = value;
		}
		if (_visible)
		{
			visibleVoxelChunks.Add(num3);
			value.a3 = 1;
		}
		else
		{
			visibleVoxelChunks.Remove(num3);
			value.a3 = 0;
		}
		value.isDirty = true;
		isAnyTerrainDirty = true;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < posAround.Length; j++)
			{
				UpdateVoxelChunkInfo(_chunkX + posAround[j].x, _chunkZ + posAround[j].z, visibleVoxelChunks);
			}
		}
		UpdateTerrainTextureData(_chunkX, _chunkZ, _visible);
	}

	public void UpdateChunks()
	{
		if (!isAnyTerrainDirty)
		{
			return;
		}
		isAnyTerrainDirty = false;
		toRemove.Clear();
		toUpdateTerrain.Clear();
		foreach (KeyValuePair<uint, VoxeChunkInfo> item in dictChunkHeightsArr)
		{
			VoxeChunkInfo value = item.Value;
			if (value.isDirty)
			{
				int tileXPos = TileAreaUtils.GetTileXPos(item.Key);
				int tileZPos = TileAreaUtils.GetTileZPos(item.Key);
				if (UpdateChunkHeights(tileXPos, tileZPos, value))
				{
					value.isDirty = false;
				}
			}
			if (!value.isDirty && value.IsEmpty())
			{
				toRemove.Add(item.Key);
			}
		}
		for (int i = 0; i < toRemove.Count; i++)
		{
			dictChunkHeightsArr.Remove(toRemove[i]);
		}
		foreach (Terrain item2 in toUpdateTerrain)
		{
			item2.terrainData.SyncHeightmap();
		}
		toUpdateTerrain.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateTerrainTextureData(int chunkX, int chunkZ, bool _bVisible)
	{
		int num = terrainConfig.DataWidth / 16;
		int num2 = terrainConfig.DataHeight / 16;
		if (terrainClipTexture == null)
		{
			terrainClipTexture = new Texture2D(num, num2, TextureFormat.RGB24, mipChain: false);
			terrainClipTexture.wrapMode = TextureWrapMode.Clamp;
			terrainClipTexture.filterMode = FilterMode.Point;
			terrainClipCols = new Color32[num * num2];
			for (int i = 0; i < terrainClipCols.Length; i++)
			{
				terrainClipCols[i] = colNoClip;
			}
			waterMaterial.SetTexture("_ClipChunks", terrainClipTexture);
		}
		int num3 = num / 2;
		int num4 = num2 / 2;
		foreach (KeyValuePair<uint, VoxeChunkInfo> item in dictChunkHeightsArr)
		{
			VoxeChunkInfo value = item.Value;
			Color32 color = ((value.a0 == 2 && value.a1 == 2 && value.a2 == 2 && value.a3 == 2) ? colClipTerrainAndWater : ((value.a3 == 0) ? colNoClip : colClipWater));
			int tileXPos = TileAreaUtils.GetTileXPos(item.Key);
			int tileZPos = TileAreaUtils.GetTileZPos(item.Key);
			terrainClipCols[tileXPos + num3 + (tileZPos + num4) * num2] = color;
		}
		isClipTextureDirty = true;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateTextureApply()
	{
		if (!isClipTextureDirty)
		{
			return false;
		}
		isClipTextureDirty = false;
		terrainClipTexture.SetPixels32(terrainClipCols);
		terrainClipTexture.Apply();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVoxelChunkInfo(int _chunkX, int _chunkZ, HashSet<uint> _visibleVoxelChunks)
	{
		int num = terrainConfig.DataWidth / 16;
		int num2 = terrainConfig.DataHeight / 16;
		if (_chunkX < -num / 2 || _chunkZ < -num2 / 2 || _chunkX >= num / 2 || _chunkZ >= num2 / 2)
		{
			return;
		}
		uint key = TileAreaUtils.MakeKey(_chunkX, _chunkZ);
		if (!dictChunkHeightsArr.TryGetValue(key, out var value))
		{
			value = new VoxeChunkInfo();
			dictChunkHeightsArr[key] = value;
		}
		int tileX = _chunkX - 1;
		int tileX2 = _chunkX + 1;
		int tileZ = _chunkZ - 1;
		int tileZ2 = _chunkZ + 1;
		bool flag = value.a3 > 0 && _visibleVoxelChunks.Contains(TileAreaUtils.MakeKey(tileX, _chunkZ));
		if (flag)
		{
			if (value.a2 == 0)
			{
				value.a2 = 1;
				value.isDirty = true;
				isAnyTerrainDirty = true;
			}
		}
		else if (value.a2 != 0)
		{
			value.a2 = 0;
			value.isDirty = true;
			isAnyTerrainDirty = true;
		}
		bool flag2 = value.a3 > 0 && _visibleVoxelChunks.Contains(TileAreaUtils.MakeKey(_chunkX, tileZ));
		if (flag2)
		{
			if (value.a1 == 0)
			{
				value.a1 = 1;
				value.isDirty = (isAnyTerrainDirty = true);
			}
		}
		else if (value.a1 != 0)
		{
			value.a1 = 0;
			value.isDirty = (isAnyTerrainDirty = true);
		}
		if (value.a3 > 0 && flag2 && flag && _visibleVoxelChunks.Contains(TileAreaUtils.MakeKey(tileX, tileZ)))
		{
			if (value.a0 == 0)
			{
				value.a0 = 1;
				value.isDirty = (isAnyTerrainDirty = true);
			}
		}
		else if (value.a0 != 0)
		{
			value.a0 = 0;
			value.isDirty = (isAnyTerrainDirty = true);
		}
		if (value.a0 >= 1 && value.a1 > 0 && value.a2 > 0 && value.a3 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(tileX, _chunkZ), out var value2) && value2.a1 > 0 && value2.a3 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(tileX, tileZ), out var value3) && value3.a3 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(_chunkX, tileZ), out var value4) && value4.a2 > 0 && value4.a3 > 0)
		{
			if (value.a0 != 2)
			{
				value.a0 = 2;
				value.isDirty = (isAnyTerrainDirty = true);
			}
		}
		else if (value.a0 == 2)
		{
			value.a0 = 1;
			value.isDirty = (isAnyTerrainDirty = true);
		}
		if (value.a1 >= 1 && value.a0 > 0 && value.a2 > 0 && value.a3 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(tileX2, _chunkZ), out var value5) && value5.a0 > 0 && value5.a2 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(_chunkX, tileZ), out value4) && value4.a2 > 0 && value4.a3 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(tileX2, tileZ), out var value6) && value6.a2 > 0)
		{
			if (value.a1 != 2)
			{
				value.a1 = 2;
				value.isDirty = (isAnyTerrainDirty = true);
			}
		}
		else if (value.a1 == 2)
		{
			value.a1 = 1;
			value.isDirty = (isAnyTerrainDirty = true);
		}
		if (value.a2 >= 1 && value.a0 > 0 && value.a1 > 0 && value.a3 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(tileX, _chunkZ), out value2) && value2.a3 > 0 && value2.a1 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(tileX, tileZ2), out var value7) && value7.a1 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(_chunkX, tileZ2), out var value8) && value8.a0 > 0 && value8.a1 > 0)
		{
			if (value.a2 != 2)
			{
				value.a2 = 2;
				value.isDirty = (isAnyTerrainDirty = true);
			}
		}
		else if (value.a2 == 2)
		{
			value.a2 = 1;
			value.isDirty = (isAnyTerrainDirty = true);
		}
		if (value.a3 >= 1 && value.a0 > 0 && value.a1 > 0 && value.a2 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(tileX2, _chunkZ), out value5) && value5.a2 > 0 && value5.a0 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(tileX2, tileZ2), out var value9) && value9.a0 > 0 && dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(_chunkX, tileZ2), out value8) && value8.a0 > 0 && value8.a1 > 0)
		{
			if (value.a3 != 2)
			{
				value.a3 = 2;
				value.isDirty = (isAnyTerrainDirty = true);
			}
		}
		else if (value.a3 == 2)
		{
			value.a3 = 1;
			value.isDirty = (isAnyTerrainDirty = true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateChunkHeights(int _chunkX, int _chunkZ, VoxeChunkInfo _vciMiddle)
	{
		int num = _chunkX * 16;
		int num2 = _chunkZ * 16;
		int chunkWorldSize = terrainConfig.ChunkWorldSize;
		int num3 = Utils.Fastfloor((float)num / (float)chunkWorldSize);
		int num4 = Utils.Fastfloor((float)num2 / (float)chunkWorldSize);
		uint key = TileAreaUtils.MakeKey(num3, num4);
		TerrainAndWater terrainAndWater = terrainTiles[key];
		if (terrainAndWater == null)
		{
			return false;
		}
		float[,] array = terrainHeights[num3, num4];
		if (array == null)
		{
			return false;
		}
		int num5 = num - num3 * chunkWorldSize;
		int num6 = num2 - num4 * chunkWorldSize;
		int num7 = num5 / terrainConfig.MetersPerHeightPix;
		num6 /= terrainConfig.MetersPerHeightPix;
		int num8 = 16 / terrainConfig.MetersPerHeightPix;
		int num9 = num7 / num8 * num8;
		int num10 = num6 / num8 * num8;
		VoxeChunkInfo value = null;
		VoxeChunkInfo value2 = null;
		VoxeChunkInfo value3 = null;
		float num11 = array[num10, num9];
		if (_vciMiddle.a0 == 1)
		{
			num11 -= 0.0025f;
		}
		else if (_vciMiddle.a0 == 2)
		{
			num11 = 0f;
		}
		heights8x8[0, 0] = num11;
		for (int i = 1; i < 8; i++)
		{
			num11 = array[num10, num9 + i];
			if (_vciMiddle.a1 == 1)
			{
				num11 -= 0.0025f;
			}
			else if (_vciMiddle.a1 == 2)
			{
				num11 = 0f;
			}
			heights8x8[0, i] = num11;
		}
		num11 = array[num10, num9 + 8];
		if (value != null || dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(_chunkX + 1, _chunkZ), out value))
		{
			if (value.a0 == 1)
			{
				num11 -= 0.0025f;
			}
			else if (value.a0 == 2)
			{
				num11 = 0f;
			}
		}
		heights8x8[0, 8] = num11;
		for (int j = 1; j < 8; j++)
		{
			num11 = array[num10 + j, num9];
			if (_vciMiddle.a2 == 1)
			{
				num11 -= 0.0025f;
			}
			else if (_vciMiddle.a2 == 2)
			{
				num11 = 0f;
			}
			heights8x8[j, 0] = num11;
		}
		for (int k = 1; k < 8; k++)
		{
			for (int l = 1; l < 8; l++)
			{
				num11 = array[num10 + k, num9 + l];
				if (_vciMiddle.a3 == 1)
				{
					num11 -= 0.0025f;
				}
				else if (_vciMiddle.a3 == 2)
				{
					num11 = 0f;
				}
				heights8x8[k, l] = num11;
			}
		}
		for (int m = 1; m < 8; m++)
		{
			num11 = array[num10 + m, num9 + 8];
			if (value != null)
			{
				if (value.a2 == 1)
				{
					num11 -= 0.0025f;
				}
				else if (value.a2 == 2)
				{
					num11 = 0f;
				}
			}
			heights8x8[m, 8] = num11;
		}
		num11 = array[num10 + 8, num9];
		if (value3 != null || dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(_chunkX, _chunkZ + 1), out value3))
		{
			if (value3.a0 == 1)
			{
				num11 -= 0.0025f;
			}
			else if (value3.a0 == 2)
			{
				num11 = 0f;
			}
		}
		heights8x8[8, 0] = num11;
		for (int n = 1; n < 8; n++)
		{
			num11 = array[num10 + 8, num9 + n];
			if (value3 != null)
			{
				if (value3.a1 == 1)
				{
					num11 -= 0.0025f;
				}
				else if (value3.a1 == 2)
				{
					num11 = 0f;
				}
			}
			heights8x8[8, n] = num11;
		}
		num11 = array[num10 + 8, num9 + 8];
		if (value2 != null || dictChunkHeightsArr.TryGetValue(TileAreaUtils.MakeKey(_chunkX + 1, _chunkZ + 1), out value2))
		{
			if (value2.a0 == 1)
			{
				num11 -= 0.0025f;
			}
			else if (value2.a0 == 2)
			{
				num11 = 0f;
			}
		}
		heights8x8[8, 8] = num11;
		terrainAndWater.terrain.terrainData.SetHeightsDelayLOD(num9, num10, heights8x8);
		toUpdateTerrain.Add(terrainAndWater.terrain);
		return true;
	}
}
