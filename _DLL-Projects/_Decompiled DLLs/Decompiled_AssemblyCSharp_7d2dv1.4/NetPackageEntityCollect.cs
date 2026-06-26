using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityCollect : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	public NetPackageEntityCollect Setup(int _entityId, int _playerId)
	{
		entityId = _entityId;
		playerId = _playerId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
		playerId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
		_bw.Write(playerId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && ValidEntityIdForSender(playerId))
		{
			if (!_world.IsRemote())
			{
				_world.GetGameManager().CollectEntityServer(entityId, playerId);
			}
			else
			{
				_world.GetGameManager().CollectEntityClient(entityId, playerId);
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}
}
