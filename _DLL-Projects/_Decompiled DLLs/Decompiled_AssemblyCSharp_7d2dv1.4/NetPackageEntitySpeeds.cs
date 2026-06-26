using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySpeeds : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int movementState;

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedForward;

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedStrafe;

	public override bool ReliableDelivery => false;

	public NetPackageEntitySpeeds Setup(Entity _entity)
	{
		entityId = _entity.entityId;
		movementState = _entity.MovementState;
		speedForward = _entity.speedForward;
		speedStrafe = _entity.speedStrafe;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		movementState = _reader.ReadByte();
		speedForward = _reader.ReadSingle();
		speedStrafe = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write((byte)movementState);
		_writer.Write(speedForward);
		_writer.Write(speedStrafe);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = _world.GetEntity(entityId);
		if (entity != null)
		{
			entity.MovementState = movementState;
			entity.speedForward = speedForward;
			entity.speedStrafe = speedStrafe;
			if (!_world.IsRemote())
			{
				_world.entityDistributer.SendPacketToTrackedPlayers(entityId, entityId, this, _inRangeOnly: true);
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
