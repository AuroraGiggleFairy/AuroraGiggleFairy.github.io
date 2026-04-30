using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddScoreClient : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int zombieKills;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int playerKills;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int otherTeamNumber;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int conditions;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityAddScoreClient Setup(int _entityId, int _zombieKills, int _playerKills, int _otherTeamNumber, int _conditions)
	{
		entityId = _entityId;
		zombieKills = _zombieKills;
		playerKills = _playerKills;
		otherTeamNumber = _otherTeamNumber;
		conditions = _conditions;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		zombieKills = _reader.ReadInt16();
		playerKills = _reader.ReadInt16();
		otherTeamNumber = _reader.ReadInt16();
		conditions = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write((short)zombieKills);
		_writer.Write((short)playerKills);
		_writer.Write((short)otherTeamNumber);
		_writer.Write(conditions);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityAlive entityAlive = (EntityAlive)_world.GetEntity(entityId);
			if (entityAlive != null)
			{
				entityAlive.AddScore(0, zombieKills, playerKills, otherTeamNumber, conditions);
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
