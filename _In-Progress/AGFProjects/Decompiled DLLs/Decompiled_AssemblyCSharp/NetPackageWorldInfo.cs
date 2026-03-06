using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldInfo : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string gameMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public string levelName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string gameName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string guid;

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList ppList;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong ticks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fixedSizeCC;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstTimeJoin;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, uint> worldFileHashes;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<WallVolume> wallVolumes;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] worldHashesData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long worldDataSize;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageWorldInfo Setup(string _gameMode, string _levelName, string _gameName, string _guid, PersistentPlayerList _playerList, ulong _ticks, bool _fixedSizeCC, bool _firstTimeJoin, List<WallVolume> wallVolumeData)
	{
		gameMode = _gameMode;
		levelName = _levelName;
		gameName = _gameName;
		ppList = _playerList;
		ticks = _ticks;
		guid = _guid;
		fixedSizeCC = _fixedSizeCC;
		firstTimeJoin = _firstTimeJoin;
		wallVolumes = wallVolumeData;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		gameMode = _reader.ReadString();
		levelName = _reader.ReadString();
		gameName = _reader.ReadString();
		guid = _reader.ReadString();
		ppList = (_reader.ReadBoolean() ? PersistentPlayerList.Read(_reader) : new PersistentPlayerList());
		ticks = _reader.ReadUInt64();
		fixedSizeCC = _reader.ReadBoolean();
		firstTimeJoin = _reader.ReadBoolean();
		int num = _reader.ReadInt32();
		worldFileHashes = new Dictionary<string, uint>();
		for (int i = 0; i < num; i++)
		{
			string key = _reader.ReadString();
			uint value = _reader.ReadUInt32();
			worldFileHashes.Add(key, value);
		}
		worldDataSize = _reader.ReadInt64();
		wallVolumes = new List<WallVolume>();
		for (uint num2 = (uint)_reader.ReadInt32(); num2 != 0; num2--)
		{
			WallVolume item = WallVolume.Read(_reader);
			wallVolumes.Add(item);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(gameMode);
		_writer.Write(levelName);
		_writer.Write(gameName);
		_writer.Write(guid);
		_writer.Write(ppList != null);
		ppList?.Write(_writer);
		_writer.Write(ticks);
		_writer.Write(fixedSizeCC);
		_writer.Write(firstTimeJoin);
		_writer.Write(worldHashesData);
		_writer.Write(worldDataSize);
		uint count = (uint)wallVolumes.Count;
		_writer.Write(count);
		foreach (WallVolume wallVolume in wallVolumes)
		{
			wallVolume.Write(_writer);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.WorldInfo(gameMode, levelName, gameName, guid, ppList, ticks, fixedSizeCC, firstTimeJoin, worldFileHashes, worldDataSize, wallVolumes);
	}

	public override int GetLength()
	{
		return 48 + worldHashesData.Length + 4 + wallVolumes.Count * 25 + 8;
	}

	public static void PrepareWorldHashes()
	{
		worldHashesData = null;
		ChunkProviderGenerateWorldFromRaw obj = GameManager.Instance.World.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
		Dictionary<string, uint> dictionary = obj?.worldFileCrcs;
		worldDataSize = obj?.worldFileTotalSize ?? 0;
		List<string> list = null;
		if (dictionary != null)
		{
			list = GameUtils.GetWorldFilesToTransmitToClient(dictionary.Keys);
		}
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		if (dictionary != null)
		{
			binaryWriter.Write(list.Count);
			foreach (string item in list)
			{
				binaryWriter.Write(item);
				binaryWriter.Write(dictionary[item]);
			}
		}
		else
		{
			binaryWriter.Write(0);
		}
		worldHashesData = memoryStream.ToArray();
	}
}
