using UnityEngine.Scripting;

[Preserve]
public class NetPackagePickupBlock : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs persistentPlayerId;

	public NetPackagePickupBlock Setup(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _playerId, PersistentPlayerData _persistentPlayerData)
	{
		clrIdx = _clrIdx;
		blockPos = _blockPos;
		blockValue = _blockValue;
		playerId = _playerId;
		persistentPlayerId = _persistentPlayerData?.PrimaryId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		clrIdx = _br.ReadInt32();
		blockPos = StreamUtils.ReadVector3i(_br);
		blockValue = new BlockValue(_br.ReadUInt32());
		playerId = _br.ReadInt32();
		persistentPlayerId = PlatformUserIdentifierAbs.FromStream(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(clrIdx);
		StreamUtils.Write(_bw, blockPos);
		_bw.Write(blockValue.rawData);
		_bw.Write(playerId);
		persistentPlayerId.ToStream(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && ValidEntityIdForSender(playerId) && ValidUserIdForSender(persistentPlayerId))
		{
			if (!_world.IsRemote())
			{
				_world.GetGameManager().PickupBlockServer(clrIdx, blockPos, blockValue, playerId, persistentPlayerId);
			}
			else
			{
				_world.GetGameManager().PickupBlockClient(clrIdx, blockPos, blockValue, playerId);
			}
		}
	}

	public override int GetLength()
	{
		return 36;
	}
}
