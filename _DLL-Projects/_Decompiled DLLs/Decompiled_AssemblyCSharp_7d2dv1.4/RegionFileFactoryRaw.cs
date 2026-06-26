public class RegionFileFactoryRaw : IRegionFilePlatformFactory
{
	public RegionFileAccessAbstract CreateRegionFileAccess()
	{
		return new RegionFileAccessRaw();
	}

	public IRegionFileChunkSnapshotUtil CreateSnapshotUtil(RegionFileAccessAbstract regionFileAccess)
	{
		return new ChunkSnapshotUtil(regionFileAccess);
	}

	public IRegionFileDebugUtil CreateDebugUtil()
	{
		return new RegionFileDebugUtilRaw();
	}
}
