using System;

public class RegionFileDebugUtilSectorBased : IRegionFileDebugUtil
{
	public string GetLocationString(int chunkX, int chunkZ)
	{
		int num = (int)Math.Floor((double)chunkX / 32.0);
		int num2 = (int)Math.Floor((double)chunkZ / 32.0);
		return $"XZ: {chunkX}/{chunkZ}  Region: r.{num}.{num2}";
	}
}
