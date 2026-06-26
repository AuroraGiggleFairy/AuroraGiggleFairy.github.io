public interface IRegionFilePlatformFactory
{
	RegionFileAccessAbstract CreateRegionFileAccess();

	IRegionFileChunkSnapshotUtil CreateSnapshotUtil(RegionFileAccessAbstract regionFileAccess);

	IRegionFileDebugUtil CreateDebugUtil();
}
