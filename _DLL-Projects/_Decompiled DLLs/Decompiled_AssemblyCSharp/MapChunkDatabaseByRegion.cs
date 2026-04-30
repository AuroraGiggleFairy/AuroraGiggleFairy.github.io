using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class MapChunkDatabaseByRegion : IMapChunkDatabase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class RegionData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public const int VERSION = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ushort[][][] m_regionData;

		[PublicizedFrom(EAccessModifier.Private)]
		public string m_lastRegionPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_dirty;

		public RegionData()
		{
			m_regionData = new ushort[32][][];
			for (int i = 0; i < 32; i++)
			{
				m_regionData[i] = new ushort[32][];
			}
			m_dirty = true;
		}

		public ushort[] GetChunkData(Vector2i offset)
		{
			return m_regionData[offset.y][offset.x];
		}

		public void SetChunkData(Vector2i offset, ushort[] mapColors, bool skipCopy)
		{
			if (skipCopy)
			{
				m_regionData[offset.y][offset.x] = mapColors;
				m_dirty = true;
				return;
			}
			ushort[] array = m_regionData[offset.y][offset.x];
			if (array == null)
			{
				array = new ushort[256];
				m_regionData[offset.y][offset.x] = array;
			}
			Array.Copy(mapColors, array, 256);
			m_dirty = true;
		}

		public void Load(string regionFilePath)
		{
			using Stream stream = SdFile.Open(regionFilePath, FileMode.Open, FileAccess.Read);
			using GZipStream baseStream = new GZipStream(stream, CompressionMode.Decompress);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: true);
			pooledBinaryReader.SetBaseStream(stream);
			pooledBinaryReader.ReadInt32();
			pooledBinaryReader.SetBaseStream(baseStream);
			ushort[] array = null;
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					if (array == null)
					{
						array = new ushort[256];
					}
					for (int k = 0; k < 256; k++)
					{
						array[k] = pooledBinaryReader.ReadUInt16();
					}
					if (Enumerable.SequenceEqual(EMPTY_CHUNK_DATA, array))
					{
						m_regionData[i][j] = null;
						continue;
					}
					m_regionData[i][j] = array;
					array = null;
				}
			}
			m_lastRegionPath = regionFilePath;
			m_dirty = false;
		}

		public void Save(string regionFilePath)
		{
			if (!m_dirty && regionFilePath == m_lastRegionPath)
			{
				return;
			}
			using Stream stream = SdFile.Open(regionFilePath, FileMode.Create, FileAccess.Write);
			using GZipStream baseStream = new GZipStream(stream, CompressionMode.Compress);
			using PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: true);
			pooledBinaryWriter.SetBaseStream(stream);
			pooledBinaryWriter.Write(1);
			pooledBinaryWriter.SetBaseStream(baseStream);
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					ushort[] array = m_regionData[i][j] ?? EMPTY_CHUNK_DATA;
					for (int k = 0; k < 256; k++)
					{
						pooledBinaryWriter.Write(array[k]);
					}
				}
			}
			m_lastRegionPath = regionFilePath;
			m_dirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilePrefix = "r.";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FileElementSeparator = ".";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilePostfix = ".7rm";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int REGION_CHUNK_WIDTH = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHUNK_TO_REGION_SHIFT = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int REGION_CHUNK_AREA = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHUNK_DATA_LENGTH = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ushort[] EMPTY_CHUNK_DATA = new ushort[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Vector2i, RegionData> m_regions;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_regionsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<int> m_chunksSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> m_chunkIdsToSend;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ushort[]> m_chunkDataToSend;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_networkDataAvailable;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i m_clientMapMiddlePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_clientMapMiddlePositionUpdated;

	public MapChunkDatabaseByRegion(int playerId)
	{
		m_regions = new Dictionary<Vector2i, RegionData>();
		m_chunksSent = new HashSet<int>();
		m_chunkIdsToSend = new List<int>();
		m_chunkDataToSend = new List<ushort[]>();
		m_playerId = playerId;
	}

	public void Clear()
	{
		lock (m_regionsLock)
		{
			m_regions.Clear();
		}
	}

	public ushort[] GetMapColors(long _chunkKey)
	{
		ToRegionAndOffset(_chunkKey, out var regionPos, out var chunkOffset);
		RegionData value;
		lock (m_regionsLock)
		{
			if (!m_regions.TryGetValue(regionPos, out value))
			{
				return null;
			}
		}
		return value.GetChunkData(chunkOffset);
	}

	public void Add(int _chunkX, int _chunkZ, ushort[] _mapColors)
	{
		Add(_chunkX, _chunkZ, _mapColors, skipCopy: false);
	}

	public void Add(List<int> _chunks, List<ushort[]> _mapPieces)
	{
		for (int i = 0; i < _chunks.Count; i++)
		{
			IMapChunkDatabase.FromChunkDBKey(_chunks[i], out var _chunkX, out var _chunkZ);
			Add(_chunkX, _chunkZ, _mapPieces[i], skipCopy: true);
		}
		m_networkDataAvailable = true;
	}

	public bool Contains(long _chunkKey)
	{
		ToRegionAndOffset(_chunkKey, out var regionPos, out var chunkOffset);
		RegionData value;
		lock (m_regionsLock)
		{
			if (!m_regions.TryGetValue(regionPos, out value))
			{
				return false;
			}
		}
		return value.GetChunkData(chunkOffset) != null;
	}

	public bool IsNetworkDataAvail()
	{
		return m_networkDataAvailable;
	}

	public void ResetNetworkDataAvail()
	{
		m_networkDataAvailable = false;
	}

	public NetPackage GetMapChunkPackagesToSend()
	{
		if (!m_clientMapMiddlePositionUpdated)
		{
			return null;
		}
		m_clientMapMiddlePositionUpdated = false;
		m_chunkIdsToSend.Clear();
		m_chunkDataToSend.Clear();
		int num = World.toChunkXZ(m_clientMapMiddlePosition.x);
		int num2 = World.toChunkXZ(m_clientMapMiddlePosition.y);
		int num3 = 8;
		lock (m_regionsLock)
		{
			for (int i = -num3; i <= num3; i++)
			{
				for (int j = -num3; j <= num3; j++)
				{
					long worldChunkKey = WorldChunkCache.MakeChunkKey(num + i, num2 + j);
					int item = IMapChunkDatabase.ToChunkDBKey(worldChunkKey);
					if (m_chunksSent.Contains(item))
					{
						continue;
					}
					ToRegionAndOffset(worldChunkKey, out var regionPos, out var chunkOffset);
					if (m_regions.TryGetValue(regionPos, out var value))
					{
						ushort[] chunkData = value.GetChunkData(chunkOffset);
						if (chunkData != null)
						{
							m_chunksSent.Add(item);
							m_chunkIdsToSend.Add(item);
							m_chunkDataToSend.Add(chunkData);
						}
					}
				}
			}
		}
		if (m_chunkIdsToSend.Count == 0)
		{
			return null;
		}
		return NetPackageManager.GetPackage<NetPackageMapChunks>().Setup(m_playerId, m_chunkIdsToSend, m_chunkDataToSend);
	}

	public void LoadAsync(ThreadManager.TaskInfo _taskInfo)
	{
		IMapChunkDatabase.DirectoryPlayerId directoryPlayerId = (IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter;
		Load(Path.Join(directoryPlayerId.dir, directoryPlayerId.file));
	}

	public void SaveAsync(ThreadManager.TaskInfo _taskInfo)
	{
		IMapChunkDatabase.DirectoryPlayerId directoryPlayerId = (IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter;
		Save(Path.Join(directoryPlayerId.dir, directoryPlayerId.file));
	}

	public void SetClientMapMiddlePosition(Vector2i _pos)
	{
		m_clientMapMiddlePosition = _pos;
		m_clientMapMiddlePositionUpdated = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Add(int chunkX, int chunkZ, ushort[] mapColors, bool skipCopy)
	{
		ToRegionAndOffset(chunkX, chunkZ, out var regionPos, out var chunkOffset);
		RegionData value;
		lock (m_regionsLock)
		{
			if (!m_regions.TryGetValue(regionPos, out value))
			{
				value = new RegionData();
				m_regions.Add(regionPos, value);
			}
		}
		value.SetChunkData(chunkOffset, mapColors, skipCopy);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Load(string rootDirectory)
	{
		if (!SdDirectory.Exists(rootDirectory))
		{
			return;
		}
		lock (m_regionsLock)
		{
			HashSet<Vector2i> hashSet = new HashSet<Vector2i>(m_regions.Keys);
			foreach (SdFileInfo item in new SdDirectoryInfo(rootDirectory).EnumerateFiles())
			{
				if (!TryGetRegionPosFromFileName(item.Name, out var regionPos))
				{
					continue;
				}
				try
				{
					hashSet.Add(regionPos);
					if (!m_regions.TryGetValue(regionPos, out var value))
					{
						value = new RegionData();
						m_regions.Add(regionPos, value);
					}
					value.Load(item.FullName);
					hashSet.Remove(regionPos);
				}
				catch (Exception ex)
				{
					Log.Warning(string.Format("[{0}] Failed to load region ({1}, {2}) from '{3}': {4}", "MapChunkDatabaseByRegion", regionPos.x, regionPos.y, item.FullName, ex));
				}
			}
			foreach (Vector2i item2 in hashSet)
			{
				m_regions.Remove(item2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save(string rootDirectory)
	{
		SdDirectory.CreateDirectory(rootDirectory);
		Dictionary<Vector2i, string> dictionary = new Dictionary<Vector2i, string>();
		foreach (SdFileInfo item in new SdDirectoryInfo(rootDirectory).EnumerateFiles())
		{
			if (TryGetRegionPosFromFileName(item.Name, out var regionPos))
			{
				dictionary[regionPos] = item.FullName;
			}
		}
		Vector2i key;
		lock (m_regionsLock)
		{
			foreach (KeyValuePair<Vector2i, RegionData> region in m_regions)
			{
				region.Deconstruct(out key, out var value);
				Vector2i vector2i = key;
				RegionData regionData = value;
				string regionDataPath = GetRegionDataPath(rootDirectory, vector2i);
				try
				{
					dictionary.Remove(vector2i);
					regionData.Save(regionDataPath);
				}
				catch (Exception ex)
				{
					Log.Warning(string.Format("[{0}] Failed to save region ({1}, {2}) to '{3}': {4}", "MapChunkDatabaseByRegion", vector2i.x, vector2i.y, regionDataPath, ex));
				}
			}
		}
		foreach (KeyValuePair<Vector2i, string> item2 in dictionary)
		{
			item2.Deconstruct(out key, out var value2);
			Vector2i vector2i2 = key;
			string text = value2;
			try
			{
				SdFile.Delete(text);
			}
			catch (Exception ex2)
			{
				Log.Warning(string.Format("[{0}] Failed to delete region ({1}, {2}) to '{3}': {4}", "MapChunkDatabaseByRegion", vector2i2.x, vector2i2.y, text, ex2));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToRegionAndOffset(long worldChunkKey, out Vector2i regionPos, out Vector2i chunkOffset)
	{
		int chunkX = WorldChunkCache.extractX(worldChunkKey);
		int chunkZ = WorldChunkCache.extractZ(worldChunkKey);
		ToRegionAndOffset(chunkX, chunkZ, out regionPos, out chunkOffset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToRegionAndOffset(Vector3i chunkPos, out Vector2i regionPos, out Vector2i chunkOffset)
	{
		ToRegionAndOffset(chunkPos.x, chunkPos.y, out regionPos, out chunkOffset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToRegionAndOffset(int chunkX, int chunkZ, out Vector2i regionPos, out Vector2i chunkOffset)
	{
		int num = chunkX >> 5;
		int num2 = chunkZ >> 5;
		regionPos = new Vector2i(num, num2);
		int x = chunkX - num * 32;
		int y = chunkZ - num2 * 32;
		chunkOffset = new Vector2i(x, y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetRegionDataPath(string rootDirectory, Vector2i regionPos)
	{
		return Path.Join(rootDirectory, string.Format("{0}{1}{2}{3}{4}", "r.", regionPos.x, ".", regionPos.y, ".7rm"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetRegionPosFromFileName(StringSpan name, out Vector2i regionPos)
	{
		if (name.IndexOf("r.") != 0)
		{
			regionPos = default(Vector2i);
			return false;
		}
		name = name.Slice("r.".Length);
		int num = name.Length - ".7rm".Length;
		if (num < 0 || name.LastIndexOf(".7rm") != num)
		{
			regionPos = default(Vector2i);
			return false;
		}
		name = name.Slice(0, num);
		int result = 0;
		bool flag = false;
		int result2 = 0;
		bool flag2 = false;
		StringSpan.StringSplitEnumerator enumerator = name.GetSplitEnumerator(".").GetEnumerator();
		while (enumerator.MoveNext())
		{
			StringSpan current = enumerator.Current;
			if (flag2)
			{
				regionPos = default(Vector2i);
				return false;
			}
			if (flag)
			{
				if (!int.TryParse(current.AsSpan(), out result2))
				{
					regionPos = default(Vector2i);
					return false;
				}
				flag2 = true;
			}
			else
			{
				if (!int.TryParse(current.AsSpan(), out result))
				{
					regionPos = default(Vector2i);
					return false;
				}
				flag = true;
			}
		}
		if (!flag || !flag2)
		{
			regionPos = default(Vector2i);
			return false;
		}
		regionPos = new Vector2i(result, result2);
		return true;
	}
}
