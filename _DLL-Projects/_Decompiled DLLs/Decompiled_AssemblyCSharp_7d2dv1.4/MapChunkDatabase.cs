using System;
using System.Collections.Generic;
using System.IO;

public class MapChunkDatabase : DatabaseWithFixedDS<int, ushort[]>, IMapChunkDatabase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxMapChunks = 131072;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EXT = "map";

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> chunksSent = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i clientMapMiddlePosition = new Vector2i(int.MaxValue, int.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bClientMapMiddlePositionUpdated;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bNetworkDataAvail;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> toSendList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ushort[]> mapPiecesList = new List<ushort[]>();

	public MapChunkDatabase(int _playerId)
		: base(7364973, 4, GamePrefs.GetInt(EnumGamePrefs.MaxUncoveredMapChunksPerPlayer), 512, 0, 131072)
	{
		playerId = _playerId;
	}

	public void Add(int _chunkX, int _chunkZ, ushort[] _mapColors)
	{
		int key = IMapChunkDatabase.ToChunkDBKey(_chunkX, _chunkZ);
		ushort[] array = GetDS(key);
		if (array == null)
		{
			array = new ushort[_mapColors.Length];
		}
		Array.Copy(_mapColors, array, _mapColors.Length);
		SetDS(key, array);
	}

	public override void Clear()
	{
		base.Clear();
		chunksSent.Clear();
		bNetworkDataAvail = false;
	}

	public ushort[] GetMapColors(long _chunkKey)
	{
		return GetDS(IMapChunkDatabase.ToChunkDBKey(_chunkKey));
	}

	public void Add(List<int> _chunks, List<ushort[]> _mapPieces)
	{
		for (int i = 0; i < _chunks.Count; i++)
		{
			SetDS(_chunks[i], _mapPieces[i]);
		}
		bNetworkDataAvail = true;
	}

	public bool Contains(long _chunkKey)
	{
		return ContainsDS(IMapChunkDatabase.ToChunkDBKey(_chunkKey));
	}

	public bool IsNetworkDataAvail()
	{
		return bNetworkDataAvail;
	}

	public void ResetNetworkDataAvail()
	{
		bNetworkDataAvail = false;
	}

	public NetPackage GetMapChunkPackagesToSend()
	{
		if (!bClientMapMiddlePositionUpdated)
		{
			return null;
		}
		bClientMapMiddlePositionUpdated = false;
		toSendList.Clear();
		mapPiecesList.Clear();
		int num = World.toChunkXZ(clientMapMiddlePosition.x);
		int num2 = World.toChunkXZ(clientMapMiddlePosition.y);
		int num3 = 8;
		for (int i = -num3; i <= num3; i++)
		{
			for (int j = -num3; j <= num3; j++)
			{
				int num4 = IMapChunkDatabase.ToChunkDBKey(WorldChunkCache.MakeChunkKey(num + i, num2 + j));
				if (!chunksSent.Contains(num4) && ContainsDS(num4))
				{
					toSendList.Add(num4);
					chunksSent.Add(num4);
					mapPiecesList.Add(GetDS(num4));
				}
			}
		}
		if (toSendList.Count == 0)
		{
			return null;
		}
		return NetPackageManager.GetPackage<NetPackageMapChunks>().Setup(playerId, toSendList, mapPiecesList);
	}

	public void LoadAsync(ThreadManager.TaskInfo _taskInfo)
	{
		Load(((IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter).dir, ((IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter).file + ".map");
	}

	public void SaveAsync(ThreadManager.TaskInfo _taskInfo)
	{
		Save(((IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter).dir, ((IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter).file + ".map");
	}

	public void SetClientMapMiddlePosition(Vector2i _pos)
	{
		if (!clientMapMiddlePosition.Equals(_pos))
		{
			clientMapMiddlePosition = _pos;
			bClientMapMiddlePositionUpdated = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int readKey(BinaryReader _br)
	{
		return _br.ReadInt32();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void writeKey(BinaryWriter _bw, int _key)
	{
		_bw.Write(_key);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void copyFromRead(byte[] _dataRead, ushort[] _data)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			_data[i] = (ushort)(_dataRead[i * 2] | (_dataRead[i * 2 + 1] << 8));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void copyToWrite(ushort[] _data, byte[] _dataWrite)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			_dataWrite[i * 2] = (byte)(_data[i] & 0xFF);
			_dataWrite[i * 2 + 1] = (byte)((_data[i] >> 8) & 0xFF);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override ushort[] allocateDataStorage()
	{
		return new ushort[256];
	}
}
