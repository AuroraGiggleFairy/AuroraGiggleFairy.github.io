using System;

public class RegionFileAccessSectorBased : RegionFileAccessMultipleChunks
{
	public override int ChunksPerRegionPerDimension => 32;

	public override void ReadDirectory(string _dir, Action<long, string, uint> _chunkAndTimeStampHandler)
	{
		ReadDirectory(_dir, _chunkAndTimeStampHandler, 32);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override RegionFile OpenRegionFile(string _dir, int _regionX, int _regionZ, string _ext)
	{
		return RegionFileSectorBased.Get(_dir, _regionX, _regionZ, _ext);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetRegionCoords(int _chunkX, int _chunkZ, out int _regionX, out int _regionZ)
	{
		_regionX = (int)Math.Floor((double)_chunkX / 32.0);
		_regionZ = (int)Math.Floor((double)_chunkZ / 32.0);
	}
}
