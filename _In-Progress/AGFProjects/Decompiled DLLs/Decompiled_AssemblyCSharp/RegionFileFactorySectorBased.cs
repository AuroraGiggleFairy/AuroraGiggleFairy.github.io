public class RegionFileFactorySectorBased : IRegionFilePlatformFactory
{
	public RegionFileAccessAbstract CreateRegionFileAccess()
	{
		return new RegionFileAccessSectorBased();
	}

	public IRegionFileChunkSnapshotUtil CreateSnapshotUtil(RegionFileAccessAbstract regionFileAccess)
	{
		return new ChunkSnapshotUtil(regionFileAccess);
	}

	public IRegionFileDebugUtil CreateDebugUtil()
	{
		return new RegionFileDebugUtilSectorBased();
	}
}
