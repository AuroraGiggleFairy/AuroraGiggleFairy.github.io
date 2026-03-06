using System;

public class RegionFileDebugUtilRaw : IRegionFileDebugUtil
{
	public string GetLocationString(int chunkX, int chunkZ)
	{
		int num = (int)Math.Floor((double)chunkX / 8.0);
		int num2 = (int)Math.Floor((double)chunkZ / 8.0);
		return $"XZ: {chunkX}/{chunkZ}  Region: r.{num}.{num2}";
	}
}
