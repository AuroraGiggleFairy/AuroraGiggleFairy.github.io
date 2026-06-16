using System;
using System.Collections.Generic;
using System.ComponentModel;
using Platform;

public interface IMapChunkDatabase
{
	public class DirectoryPlayerId
	{
		public string file;

		public string dir;

		public DirectoryPlayerId(string _dir, string _file)
		{
			file = _file;
			dir = _dir;
		}
	}

	void Clear();

	ushort[] GetMapColors(long _chunkKey);

	void Add(Vector3i _chunkPos, World _world)
	{
		int num = (_world.IsEditor() ? 7 : 4);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				Chunk chunk = (Chunk)_world.GetChunkSync(_chunkPos.x + i, _chunkPos.z + j);
				if (chunk != null && !chunk.NeedsDecoration)
				{
					Add(_chunkPos.x + i, _chunkPos.z + j, chunk.GetMapColors());
				}
			}
		}
	}

	void Add(int _chunkX, int _chunkZ, ushort[] _mapColors);

	void Add(List<int> _chunks, List<ushort[]> _mapPieces);

	bool Contains(long _chunkKey);

	bool IsNetworkDataAvail();

	void ResetNetworkDataAvail();

	NetPackage GetMapChunkPackagesToSend();

	void LoadAsync(ThreadManager.TaskInfo _taskInfo);

	void SaveAsync(ThreadManager.TaskInfo _taskInfo);

	void SetClientMapMiddlePosition(Vector2i _pos);

	static bool TryCreateOrLoad(int _entityId, out IMapChunkDatabase _mapDatabase, Func<DirectoryPlayerId> _parameterSupplier)
	{
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			if (_entityId != -1 && (bool)GameManager.Instance)
			{
				World world = GameManager.Instance.World;
				if (world != null && world.GetPrimaryPlayer()?.entityId == _entityId)
				{
					goto IL_0068;
				}
			}
			_mapDatabase = null;
			return false;
		}
		goto IL_0068;
		IL_0068:
		IMapChunkDatabase mapChunkDatabase = LaunchPrefs.MapChunkDatabase.Value switch
		{
			MapChunkDatabaseType.Fixed => new MapChunkDatabase(_entityId), 
			MapChunkDatabaseType.Region => new MapChunkDatabaseByRegion(_entityId), 
			_ => throw new InvalidEnumArgumentException(string.Format("Unknown {0}: {1}", "MapChunkDatabaseType", LaunchPrefs.MapChunkDatabase.Value)), 
		};
		DirectoryPlayerId parameter = _parameterSupplier();
		ThreadManager.AddSingleTask(mapChunkDatabase.LoadAsync, parameter);
		_mapDatabase = mapChunkDatabase;
		return true;
	}

	static int ToChunkDBKey(long _worldChunkKey)
	{
		return ToChunkDBKey(WorldChunkCache.extractX(_worldChunkKey), WorldChunkCache.extractZ(_worldChunkKey));
	}

	static int ToChunkDBKey(int _chunkX, int _chunkZ)
	{
		return ((_chunkZ & 0xFFFF) << 16) | (_chunkX & 0xFFFF);
	}

	static void FromChunkDBKey(int _chunkDBKey, out int _chunkX, out int _chunkZ)
	{
		_chunkX = _chunkDBKey & 0xFFFF;
		if (_chunkX > 32767)
		{
			_chunkX |= -65536;
		}
		_chunkZ = (_chunkDBKey >> 16) & 0xFFFF;
		if (_chunkZ > 32767)
		{
			_chunkZ |= -65536;
		}
	}
}
