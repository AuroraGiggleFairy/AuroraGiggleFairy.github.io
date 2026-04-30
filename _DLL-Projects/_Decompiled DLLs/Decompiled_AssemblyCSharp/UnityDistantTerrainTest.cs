using UnityEngine;

public class UnityDistantTerrainTest
{
	public const string cGameObjectName = "DistantUnityTerrain";

	public static UnityDistantTerrainTest Instance;

	public UnityDistantTerrain unityTerrain;

	public IBackedArray<ushort> HeightMap;

	public int hmWidth = 6144;

	public int hmHeight = 6144;

	public Material TerrainMaterial;

	public Material WaterMaterial;

	public int WaterChunks16x16Width;

	public byte[] WaterChunks16x16;

	public GameObject parentObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTileSize = 512;

	public static void Create()
	{
		Instance = new UnityDistantTerrainTest();
	}

	public void LoadTerrain()
	{
		if (!parentObj)
		{
			parentObj = new GameObject("DistantUnityTerrain");
		}
		UnityDistantTerrain.Config terrainConfig = new UnityDistantTerrain.Config
		{
			DataWidth = hmWidth,
			DataHeight = hmHeight,
			DataTileSize = 512,
			DataSteps = 2,
			SplatSteps = 1,
			MetersPerHeightPix = 2,
			MaxHeight = 256,
			PixelError = 5
		};
		int visibleChunks = Mathf.CeilToInt((float)(Mathf.Clamp(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality), 2, 4) * 512) / (float)terrainConfig.ChunkWorldSize * 2f) + 1;
		int num = terrainConfig.DataWidth / terrainConfig.DataTileSize;
		int num2 = terrainConfig.DataHeight / terrainConfig.DataTileSize;
		TileAreaConfig tileAreaConfig = new TileAreaConfig
		{
			tileStart = new Vector2i(-num / 2, -num2 / 2),
			tileEnd = new Vector2i(Utils.FastMax(0, num / 2 - 1), Utils.FastMax(0, num2 / 2 - 1)),
			tileSizeInWorldUnits = terrainConfig.ChunkWorldSize,
			bWrapAroundX = false,
			bWrapAroundZ = false
		};
		TileAreaConfig tileAreaConfig2 = tileAreaConfig;
		tileAreaConfig2.tileSizeInWorldUnits = terrainConfig.DataTileSize;
		ITileArea<float[,]> heights = LoadTerrainHeightTiles(tileAreaConfig, HeightMap, terrainConfig.DataWidth, terrainConfig.DataHeight, terrainConfig.DataTileSize);
		unityTerrain = new UnityDistantTerrain();
		unityTerrain.Init(parentObj, terrainConfig, visibleChunks, TerrainMaterial, WaterMaterial, WaterChunks16x16Width, WaterChunks16x16, heights, null, null, null);
	}

	public ITileArea<float[,]> LoadTerrainHeightTiles(TileAreaConfig _config, IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceAtPix)
	{
		if (PlatformOptimizations.FileBackedTerrainTiles)
		{
			TileFile<float> tileFile = HeightMapUtils.ConvertAndSliceUnityHeightmapQuarteredToFile(_rawHeightMap, _heightMapWidth, _heightMapHeight, _sliceAtPix);
			return new TileAreaCache<float>(_config, tileFile, 9);
		}
		float[,][,] data = HeightMapUtils.ConvertAndSliceUnityHeightmapQuartered(_rawHeightMap, _heightMapWidth, _heightMapHeight, _sliceAtPix);
		return new TileArea<float[,]>(_config, data);
	}

	public void FrameUpdate(EntityPlayerLocal _player)
	{
		if (unityTerrain != null)
		{
			unityTerrain.FrameUpdate(_player);
		}
	}

	public void OnChunkVisible(int _chunkX, int _chunkZ, bool _visible)
	{
		if (unityTerrain != null)
		{
			unityTerrain.OnChunkUpdate(_chunkX, _chunkZ, _visible);
		}
	}

	public void Cleanup()
	{
		if (unityTerrain != null)
		{
			unityTerrain.Cleanup();
			unityTerrain = null;
		}
		Object.Destroy(parentObj);
	}
}
