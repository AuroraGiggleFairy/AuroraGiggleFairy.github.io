[PublicizedFrom(EAccessModifier.Internal)]
public class MyRegionFileManager : RegionFileManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager terrainRegionManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkProvider chunkProvider;

	public MyRegionFileManager(World _world, IChunkProvider _chunkProvider, RegionFileManager _terrainRegionCache, string _loadDirectory, string _saveDirectory, int _maxChunksInCache, bool _bAutoSaveOnChunkDrop)
		: base(_loadDirectory, _saveDirectory, _maxChunksInCache, _bAutoSaveOnChunkDrop)
	{
		terrainRegionManager = _terrainRegionCache;
		world = _world;
		chunkProvider = _chunkProvider;
	}

	public override void SaveChunkSnapshot(Chunk _chunk, bool _saveIfUnchanged)
	{
		if (world.IsEditor() && !chunkProvider.IsDecorationsEnabled())
		{
			Chunk chunk = MemoryPools.PoolChunks.AllocSync(_bReset: true);
			chunk.X = _chunk.X;
			chunk.Z = _chunk.Z;
			Chunk.ToTerrain(_chunk, chunk);
			chunk.NeedsDecoration = false;
			terrainRegionManager.AddChunkSync(chunk);
		}
		base.SaveChunkSnapshot(_chunk, _saveIfUnchanged);
	}
}
