using System;

public class RegionFileAccessRaw : RegionFileAccessMultipleChunks
{
	public override int ChunksPerRegionPerDimension => 8;

	public override void ReadDirectory(string _dir, Action<long, string, uint> _chunkAndTimeStampHandler)
	{
		ReadDirectory(_dir, _chunkAndTimeStampHandler, 8);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override RegionFile OpenRegionFile(string _dir, int _regionX, int _regionZ, string _ext)
	{
		string text = RegionFile.ConstructFullFilePath(_dir, _regionX, _regionZ, _ext);
		if (SdFile.Exists(text))
		{
			return RegionFileRaw.Load(text, _regionX, _regionZ);
		}
		return RegionFileRaw.New(text, _regionX, _regionZ, 1024);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetRegionCoords(int _chunkX, int _chunkZ, out int _regionX, out int _regionZ)
	{
		_regionX = (int)Math.Floor((double)_chunkX / 8.0);
		_regionZ = (int)Math.Floor((double)_chunkZ / 8.0);
	}
}
