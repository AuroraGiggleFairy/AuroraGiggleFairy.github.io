using System;
using System.IO;

public abstract class RegionFileAccessAbstract
{
	public abstract int ChunksPerRegionPerDimension { get; }

	public RegionFileAccessAbstract()
	{
	}

	public static bool ExtractKey(string _filename, out long _key)
	{
		_key = 0L;
		if (_filename.Length > 0 && _filename.StartsWith("r.") && _filename.EndsWith(".ttc"))
		{
			string text = GameIO.RemoveExtension(_filename.Substring(2), ".ttc");
			int num = text.IndexOf(".");
			if (num > 0 && int.TryParse(text.Substring(0, num), out var result) && int.TryParse(text.Substring(num + 1), out var result2))
			{
				_key = WorldChunkCache.MakeChunkKey(result, result2);
				return true;
			}
		}
		return false;
	}

	public static string MakeFilename(int _x, int _z)
	{
		return "r." + _x + "." + _z + ".ttc";
	}

	public virtual void Close()
	{
	}

	public abstract void ReadDirectory(string _dir, Action<long, string, uint> _chunkAndTimeStampHandler);

	public abstract Stream GetOutputStream(string _dir, int _chunkX, int _chunkZ, string _ext);

	public abstract Stream GetInputStream(string _dir, int _chunkX, int _chunkZ, string _ext);

	public abstract void Remove(string _dir, int _chunkX, int _chunkZ);

	public virtual void OptimizeLayouts()
	{
	}

	public abstract void ClearCache();

	public abstract void RemoveRegionFromCache(int _regionX, int _regionZ, string _dir);

	public virtual int GetChunkByteCount(string _dir, int _chunkX, int _chunkZ)
	{
		throw new NotImplementedException();
	}

	public virtual long GetTotalByteCount(string _dir)
	{
		throw new NotImplementedException();
	}
}
