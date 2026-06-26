public static class RegionFilePlatform
{
	public static IRegionFilePlatformFactory CreateFactory()
	{
		return new RegionFileFactorySectorBased();
	}
}
