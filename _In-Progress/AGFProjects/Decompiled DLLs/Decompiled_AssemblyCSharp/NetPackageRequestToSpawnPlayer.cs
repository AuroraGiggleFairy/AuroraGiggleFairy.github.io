using UnityEngine.Scripting;

[Preserve]
public class NetPackageRequestToSpawnPlayer : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkViewDim;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerProfile playerProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nearEntityId;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageRequestToSpawnPlayer Setup(int _chunkViewDim, PlayerProfile _playerProfile, int _nearEntityId)
	{
		chunkViewDim = _chunkViewDim;
		playerProfile = _playerProfile;
		nearEntityId = _nearEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		chunkViewDim = _reader.ReadInt16();
		playerProfile = PlayerProfile.Read(_reader);
		nearEntityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((short)chunkViewDim);
		playerProfile.Write(_writer);
		_writer.Write(nearEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.RequestToSpawnPlayer(base.Sender, chunkViewDim, playerProfile, nearEntityId);
	}

	public override int GetLength()
	{
		return 50;
	}
}
