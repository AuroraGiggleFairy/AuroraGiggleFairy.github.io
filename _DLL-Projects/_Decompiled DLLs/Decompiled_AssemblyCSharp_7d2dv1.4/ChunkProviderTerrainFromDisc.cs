using System.Collections;
using UnityEngine;

public class ChunkProviderTerrainFromDisc : ChunkProviderGenerateWorld
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager terrainRegionFileManager;

	public ChunkProviderTerrainFromDisc(ChunkCluster _cc, string _levelName)
		: base(_cc, _levelName)
	{
		SetDecorationsEnabled(_bEnable: true);
	}

	public override IEnumerator Init(World _world)
	{
		yield return base.Init(_world);
		m_BiomeProvider = new WorldBiomeProviderFromImage(levelName, _world.Biomes);
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = new WorldDecoratorPOIFromImage(levelName, GetDynamicPrefabDecorator(), 6144, 6144, null, _bChangeWaterDensity: false);
		m_Decorators.Add(worldDecoratorPOIFromImage);
		yield return worldDecoratorPOIFromImage.InitData();
		m_Decorators.Add(new WorldDecoratorBlocksFromBiome(m_BiomeProvider, GetDynamicPrefabDecorator()));
		string saveGameRegionDirDefault = GameIO.GetSaveGameRegionDirDefault();
		string saveDirectory = (_world.IsEditor() ? GameIO.GetSaveGameRegionDirDefault() : null);
		terrainRegionFileManager = new RegionFileManager(saveGameRegionDirDefault, saveDirectory, 0, _bSaveOnChunkDrop: true);
		m_TerrainGenerator = new TerrainFromChunk();
		((TerrainFromChunk)m_TerrainGenerator).Init(terrainRegionFileManager, m_BiomeProvider, _world.Seed);
		string text = (_world.IsEditor() ? null : GameIO.GetSaveGameRegionDir());
		m_RegionFileManager = new MyRegionFileManager(_world, this, terrainRegionFileManager, text, text, 0, !_world.IsEditor());
		yield return null;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		terrainRegionFileManager.Cleanup();
	}

	public override void SaveAll()
	{
		base.SaveAll();
		if (world.IsEditor())
		{
			m_RegionFileManager.MakePersistent(world.ChunkCache, _bSaveEvenIfUnchanged: false);
			m_RegionFileManager.WaitSaveDone();
			terrainRegionFileManager.MakePersistent(null, _bSaveEvenIfUnchanged: false);
			terrainRegionFileManager.WaitSaveDone();
		}
	}

	public override bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] mapColors)
	{
		terrainRegionFileManager.SetCacheSize(1000);
		bool overviewMap = base.GetOverviewMap(_startPos, _size, mapColors);
		terrainRegionFileManager.SetCacheSize(0);
		return overviewMap;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void generateTerrain(World _world, Chunk _chunk, GameRandom _random)
	{
		long key = _chunk.Key;
		if (terrainRegionFileManager.ContainsChunkSync(key))
		{
			Chunk chunkSync = terrainRegionFileManager.GetChunkSync(key);
			if (chunkSync != null)
			{
				((TerrainFromChunk)m_TerrainGenerator).SetTerrainChunk(chunkSync);
				m_TerrainGenerator.GenerateTerrain(_world, _chunk, _random);
				((TerrainFromChunk)m_TerrainGenerator).SetTerrainChunk(null);
				_chunk.CopyLightsFrom(chunkSync);
				_chunk.isModified = false;
				terrainRegionFileManager.RemoveChunkSync(key);
				MemoryPools.PoolChunks.FreeSync(chunkSync);
			}
		}
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.ChunkDataDriven;
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		Vector2i minPos = ((WorldDecoratorPOIFromImage)m_Decorators[0]).m_Poi.MinPos;
		Vector2i maxPos = ((WorldDecoratorPOIFromImage)m_Decorators[0]).m_Poi.MaxPos;
		_minSize = new Vector3i(minPos.x, 0, minPos.y) + new Vector3i(80, 0, 80);
		_maxSize = new Vector3i(maxPos.x, 255, maxPos.y) - new Vector3i(80, 0, 80);
		return true;
	}

	public override int GetPOIBlockIdOverride(int _x, int _z)
	{
		WorldGridCompressedData<byte> poi = ((WorldDecoratorPOIFromImage)m_Decorators[0]).m_Poi;
		if (!poi.Contains(_x, _z))
		{
			return 0;
		}
		byte data = poi.GetData(_x, _z);
		if (data == 0 || data == byte.MaxValue)
		{
			return 0;
		}
		return world.Biomes.getPoiForColor(data)?.m_BlockValue.type ?? 0;
	}

	public override float GetPOIHeightOverride(int x, int z)
	{
		WorldGridCompressedData<byte> poi = ((WorldDecoratorPOIFromImage)m_Decorators[0]).m_Poi;
		byte b = 0;
		if (!poi.Contains(x, z) || (b = poi.GetData(x, z)) == byte.MaxValue || b == 0)
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
