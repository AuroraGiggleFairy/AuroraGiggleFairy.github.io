using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class ChunkProviderGenerateWorldFromImage : ChunkProviderGenerateWorld
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayWithOffset<byte> m_Dtm;

	public IBackedArray<ushort> rawdata;

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bLoadDTM;

	public ChunkProviderGenerateWorldFromImage(ChunkCluster _cc, string _levelName, bool _bLoadDTM = true)
		: base(_cc, _levelName)
	{
		bLoadDTM = _bLoadDTM;
	}

	public override void Cleanup()
	{
		heightMap?.Dispose();
		heightMap = null;
		rawdata?.Dispose();
		rawdata = null;
		base.Cleanup();
	}

	public override IEnumerator Init(World _world)
	{
		if (bLoadDTM)
		{
			yield return base.Init(_world);
		}
		else
		{
			world = _world;
		}
		Stopwatch sw = new Stopwatch();
		sw.Start();
		if (bLoadDTM)
		{
			loadDTM();
		}
		Log.Out("Loading and parsing of dtm.png took " + sw.ElapsedMilliseconds + "ms");
		sw.Reset();
		sw.Start();
		heightMap?.Dispose();
		heightMap = new HeightMap(m_Dtm.DimX, m_Dtm.DimY, 255f, rawdata);
		m_BiomeProvider = new WorldBiomeProviderFromImage(levelName, world.Biomes);
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = new WorldDecoratorPOIFromImage(levelName, GetDynamicPrefabDecorator(), m_Dtm.DimX, m_Dtm.DimY, null, _bChangeWaterDensity: false, 1, heightMap);
		m_Decorators.Add(worldDecoratorPOIFromImage);
		yield return worldDecoratorPOIFromImage.InitData();
		m_Decorators.Add(new WorldDecoratorBlocksFromBiome(m_BiomeProvider, GetDynamicPrefabDecorator()));
		Log.Out("Loading and parsing of generator took " + sw.ElapsedMilliseconds + "ms");
		sw.Reset();
		sw.Start();
		if (bLoadDTM)
		{
			m_TerrainGenerator = new TerrainFromDTM();
			((TerrainFromDTM)m_TerrainGenerator).Init(m_Dtm, m_BiomeProvider, levelName, world.Seed);
			string text = (_world.IsEditor() ? null : GameIO.GetSaveGameRegionDir());
			m_RegionFileManager = new RegionFileManager(text, text, 0, !_world.IsEditor());
			MultiBlockManager.Instance.Initialize(m_RegionFileManager);
		}
		yield return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadDTM()
	{
		Texture2D texture2D = null;
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(levelName);
		if (SdFile.Exists(location.FullPath + "/dtm.png"))
		{
			texture2D = TextureUtils.LoadTexture(location.FullPath + "/dtm.png");
			Log.Out("Loading local DTM");
		}
		else if (SdFile.Exists(location.FullPath + "/dtm.tga"))
		{
			texture2D = TextureUtils.LoadTexture(location.FullPath + "/dtm.tga");
			Log.Out("Loading local DTM");
		}
		else
		{
			texture2D = Resources.Load("Data/Worlds/" + levelName + "/dtm", typeof(Texture2D)) as Texture2D;
		}
		Log.Out("DTM image size w= " + texture2D.width + ", h = " + texture2D.height);
		Color[] pixels = texture2D.GetPixels();
		m_Dtm = new ArrayWithOffset<byte>(texture2D.width, texture2D.height);
		rawdata?.Dispose();
		rawdata = BackedArrays.Create<ushort>(pixels.Length);
		int width = texture2D.width;
		int height = texture2D.height;
		for (int i = 0; i < height; i++)
		{
			Span<ushort> span;
			using (rawdata.GetSpan(i * width, width, out span))
			{
				for (int j = 0; j < width; j++)
				{
					m_Dtm[j + m_Dtm.MinPos.x, i + m_Dtm.MinPos.y] = (byte)(pixels[width * i + j].grayscale * 255f);
					span[j] = (ushort)((byte)(pixels[width * i + j].grayscale * 255f) | ((byte)((pixels[width * i + j].grayscale * 255f - 255f) * 255f) << 8));
				}
			}
		}
		Resources.UnloadAsset(texture2D);
	}

	public override void ReloadAllChunks()
	{
		loadDTM();
		((TerrainFromDTM)m_TerrainGenerator).Init(m_Dtm, m_BiomeProvider, levelName, world.Seed);
		base.ReloadAllChunks();
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.GenerateFromDtm;
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = new Vector3i(m_Dtm.MinPos.x, 0, m_Dtm.MinPos.y);
		_maxSize = new Vector3i(m_Dtm.MaxPos.x, 255, m_Dtm.MaxPos.y);
		return true;
	}

	public override int GetPOIBlockIdOverride(int _x, int _z)
	{
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = (WorldDecoratorPOIFromImage)m_Decorators[0];
		WorldGridCompressedData<byte> poi = worldDecoratorPOIFromImage.m_Poi;
		int x = _x / worldDecoratorPOIFromImage.worldScale;
		int y = _z / worldDecoratorPOIFromImage.worldScale;
		if (!poi.Contains(x, y))
		{
			return 0;
		}
		byte data = poi.GetData(x, y);
		if (data == 0 || data == byte.MaxValue)
		{
			return 0;
		}
		return world.Biomes.getPoiForColor(data)?.m_BlockValue.type ?? 0;
	}

	public override float GetPOIHeightOverride(int x, int z)
	{
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = (WorldDecoratorPOIFromImage)m_Decorators[0];
		WorldGridCompressedData<byte> poi = worldDecoratorPOIFromImage.m_Poi;
		byte b = 0;
		if (!poi.Contains(x / worldDecoratorPOIFromImage.worldScale, z / worldDecoratorPOIFromImage.worldScale) || (b = poi.GetData(x / worldDecoratorPOIFromImage.worldScale, z / worldDecoratorPOIFromImage.worldScale)) == byte.MaxValue || b == 0)
		{
			return 0f;
		}
		PoiMapElement poiForColor = world.Biomes.getPoiForColor(b);
		if (poiForColor == null)
		{
			return 0f;
		}
		if (!poiForColor.m_BlockValue.Block.blockMaterial.IsLiquid)
		{
			return 0f;
		}
		return poiForColor.m_YPosFill;
	}
}
