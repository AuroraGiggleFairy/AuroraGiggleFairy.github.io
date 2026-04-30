using System;
using System.Collections.Generic;
using System.IO;
using Unity.Profiling;
using UnityEngine;

public abstract class RegionFileAccessMultipleChunks : RegionFileAccessAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Region : Dictionary<Vector2, RegionExtensions>
	{
		public Region()
			: base((IEqualityComparer<Vector2>)Vector2EqualityComparer.Instance)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class RegionExtensions : Dictionary<string, RegionFile>
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Region> regionTable = new Dictionary<string, Region>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<RegionFile> regionsWithRemovedChunks = new HashSet<RegionFile>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkMemoryStreamWriter writeStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkMemoryStreamReader readStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_OptimizeLayoutsMarker = new ProfilerMarker("RegionFileAccess.OptimizeLayout");

	public RegionFileAccessMultipleChunks()
	{
		writeStream = new ChunkMemoryStreamWriter();
		readStream = new ChunkMemoryStreamReader();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ReadDirectory(string _dir, Action<long, string, uint> _chunkAndTimeStampHandler, int chunksPerRegionPerDimension)
	{
		if (_dir == null)
		{
			return;
		}
		if (!SdDirectory.Exists(_dir))
		{
			SdDirectory.CreateDirectory(_dir);
			return;
		}
		SdFileInfo[] files = new SdDirectoryInfo(_dir).GetFiles();
		foreach (SdFileInfo sdFileInfo in files)
		{
			string[] array = sdFileInfo.Name.Split('.');
			if (array.Length != 4)
			{
				if (!sdFileInfo.Name.EqualsCaseInsensitive("PendingResets.7pr"))
				{
					Debug.LogError("Invalid region file name: " + sdFileInfo.FullName);
				}
				continue;
			}
			if (!int.TryParse(array[1], out var result) || !int.TryParse(array[2], out var result2))
			{
				Debug.LogError("Failed to parse region coordinates from region file name: " + sdFileInfo.FullName);
				continue;
			}
			string text = array[3];
			RegionFile rFC = GetRFC(result, result2, _dir, text);
			for (int j = result * chunksPerRegionPerDimension; j < result * chunksPerRegionPerDimension + chunksPerRegionPerDimension; j++)
			{
				for (int k = result2 * chunksPerRegionPerDimension; k < result2 * chunksPerRegionPerDimension + chunksPerRegionPerDimension; k++)
				{
					if (rFC.HasChunk(j, k))
					{
						rFC.GetTimestampInfo(j, k, out var timeStamp);
						_chunkAndTimeStampHandler(WorldChunkCache.MakeChunkKey(j, k), text, timeStamp);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFile GetRFC(int _regionX, int _regionZ, string _dir, string _ext)
	{
		lock (regionTable)
		{
			if (!regionTable.TryGetValue(_dir, out var value))
			{
				value = new Region();
				regionTable.Add(_dir, value);
			}
			Vector2 key = new Vector2(_regionX, _regionZ);
			if (!value.TryGetValue(key, out var value2))
			{
				value2 = new RegionExtensions();
				value.Add(key, value2);
			}
			if (!value2.TryGetValue(_ext, out var value3))
			{
				value3 = OpenRegionFile(_dir, _regionX, _regionZ, _ext);
				value2.Add(_ext, value3);
			}
			return value3;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract RegionFile OpenRegionFile(string _dir, int _regionX, int _regionZ, string _ext);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void GetRegionCoords(int _chunkX, int _chunkZ, out int _regionX, out int _regionZ);

	public override Stream GetOutputStream(string _dir, int _chunkX, int _chunkZ, string _ext)
	{
		writeStream.Init(this, _dir, _chunkX, _chunkZ, _ext);
		return writeStream;
	}

	public override Stream GetInputStream(string _dir, int _chunkX, int _chunkZ, string _ext)
	{
		GetRegionCoords(_chunkX, _chunkZ, out var _regionX, out var _regionZ);
		RegionFile rFC = GetRFC(_regionX, _regionZ, _dir, _ext);
		if (!rFC.HasChunk(_chunkX, _chunkZ))
		{
			return null;
		}
		rFC.ReadData(_chunkX, _chunkZ, readStream);
		readStream.Position = 0L;
		return readStream;
	}

	public void Write(string _dir, int _chunkX, int _chunkZ, string _ext, byte[] _buf, int _bufLength)
	{
		GetRegionCoords(_chunkX, _chunkZ, out var _regionX, out var _regionZ);
		GetRFC(_regionX, _regionZ, _dir, _ext).WriteData(_chunkX, _chunkZ, _bufLength, 0, _buf, _saveHeaderToFile: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int MediumIntByteArrayToInt(byte[] bytes)
	{
		return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16);
	}

	public override void Remove(string _dir, int _chunkX, int _chunkZ)
	{
		GetRegionCoords(_chunkX, _chunkZ, out var _regionX, out var _regionZ);
		lock (regionTable)
		{
			if (!regionTable.TryGetValue(_dir, out var value))
			{
				return;
			}
			Vector2 key = new Vector2(_regionX, _regionZ);
			if (!value.TryGetValue(key, out var value2))
			{
				return;
			}
			foreach (RegionFile value3 in value2.Values)
			{
				if (value3.HasChunk(_chunkX, _chunkZ))
				{
					value3.RemoveChunk(_chunkX, _chunkZ);
					regionsWithRemovedChunks.Add(value3);
				}
			}
		}
	}

	public override void OptimizeLayouts()
	{
		foreach (RegionFile regionsWithRemovedChunk in regionsWithRemovedChunks)
		{
			using (s_OptimizeLayoutsMarker.Auto())
			{
				if (regionsWithRemovedChunk.ChunkCount() > 0)
				{
					regionsWithRemovedChunk.OptimizeLayout();
					continue;
				}
				regionsWithRemovedChunk.GetPositionAndPath(out var regionX, out var regionZ, out var fullFilePath);
				RemoveRegionFromCache(regionX, regionZ, Path.GetDirectoryName(fullFilePath));
				SdFile.Delete(fullFilePath);
			}
		}
		regionsWithRemovedChunks.Clear();
	}

	public override void Close()
	{
		OptimizeLayouts();
		ClearCache();
	}

	public override void ClearCache()
	{
		lock (regionTable)
		{
			foreach (Region value in regionTable.Values)
			{
				foreach (RegionExtensions value2 in value.Values)
				{
					foreach (RegionFile value3 in value2.Values)
					{
						value3.SaveHeaderData();
						value3.Close();
					}
					value2.Clear();
				}
				value.Clear();
			}
			regionTable.Clear();
		}
	}

	public override void RemoveRegionFromCache(int _regionX, int _regionZ, string _dir)
	{
		lock (regionTable)
		{
			if (!regionTable.TryGetValue(_dir, out var value))
			{
				return;
			}
			Vector2 key = new Vector2(_regionX, _regionZ);
			if (!value.TryGetValue(key, out var value2))
			{
				return;
			}
			foreach (RegionFile value3 in value2.Values)
			{
				value3.SaveHeaderData();
				value3.Close();
			}
			value2.Clear();
			value.Remove(key);
		}
	}

	public override int GetChunkByteCount(string _dir, int _chunkX, int _chunkZ)
	{
		GetRegionCoords(_chunkX, _chunkZ, out var _regionX, out var _regionZ);
		int num = 0;
		lock (regionTable)
		{
			if (!regionTable.TryGetValue(_dir, out var value))
			{
				return 0;
			}
			if (!value.TryGetValue(new Vector2(_regionX, _regionZ), out var value2))
			{
				return 0;
			}
			foreach (RegionFile value3 in value2.Values)
			{
				num += value3.GetChunkByteCount(_chunkX, _chunkZ);
			}
			return num;
		}
	}

	public override long GetTotalByteCount(string _dir)
	{
		long num = 0L;
		lock (regionTable)
		{
			foreach (Region value in regionTable.Values)
			{
				foreach (RegionExtensions value2 in value.Values)
				{
					foreach (RegionFile value3 in value2.Values)
					{
						num += value3.Length;
					}
				}
			}
			return num;
		}
	}
}
