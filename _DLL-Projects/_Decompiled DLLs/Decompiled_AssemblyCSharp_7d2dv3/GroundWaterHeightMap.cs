public class GroundWaterHeightMap
{
	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldGridCompressedData<byte> poiColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBiomes biomes;

	public GroundWaterHeightMap(World _world)
	{
		world = _world;
	}

	public bool TryInit()
	{
		if (poiColors != null && biomes != null)
		{
			return true;
		}
		if (!(world.ChunkCache.ChunkProvider is ChunkProviderGenerateWorldFromRaw { poiFromImage: var poiFromImage }))
		{
			return false;
		}
		if (poiFromImage == null)
		{
			return false;
		}
		poiColors = poiFromImage.m_Poi;
		biomes = world.Biomes;
		if (poiColors != null)
		{
			return biomes != null;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PoiMapElement GetPoiMapElement(int _worldX, int _worldZ)
	{
		if (!poiColors.Contains(_worldX, _worldZ))
		{
			return null;
		}
		byte data = poiColors.GetData(_worldX, _worldZ);
		if (data == 0)
		{
			return null;
		}
		return biomes.getPoiForColor(data);
	}

	public bool TryGetWaterHeightAt(int _worldX, int _worldZ, out int _height)
	{
		PoiMapElement poiMapElement = GetPoiMapElement(_worldX, _worldZ);
		if (poiMapElement == null)
		{
			_height = 0;
			return false;
		}
		if (poiMapElement.m_BlockValue.type != 240)
		{
			_height = 0;
			return false;
		}
		_height = poiMapElement.m_YPosFill;
		return true;
	}
}
