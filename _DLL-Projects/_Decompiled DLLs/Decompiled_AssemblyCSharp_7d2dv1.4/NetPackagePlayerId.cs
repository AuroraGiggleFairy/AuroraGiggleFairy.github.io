using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerId : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public int teamNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerDataFile playerDataFile;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkViewDim;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackagePlayerId Setup(int _id, int _teamNumber, PlayerDataFile _playerDataFile, int _chunkViewDim)
	{
		id = _id;
		teamNumber = _teamNumber;
		playerDataFile = _playerDataFile;
		chunkViewDim = _chunkViewDim;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		id = _reader.ReadInt32();
		teamNumber = _reader.ReadInt16();
		playerDataFile = new PlayerDataFile();
		playerDataFile.Read(_reader, uint.MaxValue);
		chunkViewDim = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(id);
		_writer.Write((short)teamNumber);
		playerDataFile.Write(_writer);
		_writer.Write(chunkViewDim);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.PlayerId(id, teamNumber, playerDataFile, chunkViewDim);
	}

	public override int GetLength()
	{
		return 40;
	}
}
