using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityPhysics : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagIsMaster = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagIsCollided = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagOnGround = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public int EntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 Pos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion QRot;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 Velocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 AngularVelocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort Flags;

	public NetPackageEntityPhysics Setup(Entity _entity)
	{
		EntityId = _entity.entityId;
		Pos = _entity.position;
		QRot = _entity.qrotation;
		ushort num = 0;
		if (_entity.isPhysicsMaster)
		{
			num |= 1;
		}
		if (_entity.isCollided)
		{
			num |= 2;
		}
		if (_entity.onGround)
		{
			num |= 4;
		}
		Velocity = _entity.GetVelocityPerSecond();
		AngularVelocity = _entity.GetAngularVelocityPerSecond();
		Flags = num;
		return this;
	}

	public NetPackageEntityPhysics Setup(NetPackageEntityPhysics _p)
	{
		EntityId = _p.EntityId;
		Pos = _p.Pos;
		QRot = _p.QRot;
		Velocity = _p.Velocity;
		AngularVelocity = _p.AngularVelocity;
		Flags = _p.Flags;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		Flags = _br.ReadUInt16();
		EntityId = _br.ReadInt32();
		Pos.x = _br.ReadSingle();
		Pos.y = _br.ReadSingle();
		Pos.z = _br.ReadSingle();
		QRot.x = _br.ReadSingle();
		QRot.y = _br.ReadSingle();
		QRot.z = _br.ReadSingle();
		QRot.w = _br.ReadSingle();
		Velocity.x = _br.ReadSingle();
		Velocity.y = _br.ReadSingle();
		Velocity.z = _br.ReadSingle();
		AngularVelocity.x = _br.ReadSingle();
		AngularVelocity.y = _br.ReadSingle();
		AngularVelocity.z = _br.ReadSingle();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(Flags);
		_bw.Write(EntityId);
		_bw.Write(Pos.x);
		_bw.Write(Pos.y);
		_bw.Write(Pos.z);
		_bw.Write(QRot.x);
		_bw.Write(QRot.y);
		_bw.Write(QRot.z);
		_bw.Write(QRot.w);
		_bw.Write(Velocity.x);
		_bw.Write(Velocity.y);
		_bw.Write(Velocity.z);
		_bw.Write(AngularVelocity.x);
		_bw.Write(AngularVelocity.y);
		_bw.Write(AngularVelocity.z);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = _world.GetEntity(EntityId);
		if (entity == null || entity.isPhysicsMaster)
		{
			return;
		}
		Entity attachedMainEntity = entity.AttachedMainEntity;
		if (attachedMainEntity != null && _world.GetPrimaryPlayerId() == attachedMainEntity.entityId)
		{
			return;
		}
		entity.serverPos = NetEntityDistributionEntry.EncodePos(Pos);
		entity.isCollided = (Flags & 2) > 0;
		entity.onGround = (Flags & 4) > 0;
		entity.PhysicsMasterSetTargetOrientation(Pos, QRot);
		entity.SetVelocityPerSecond(Velocity, AngularVelocity);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if ((Flags & 1) == 0)
			{
				entity.PhysicsMasterBecome();
			}
			NetPackageEntityPhysics package = NetPackageManager.GetPackage<NetPackageEntityPhysics>().Setup(this);
			_world.entityDistributer.SendPacketToTrackedPlayers(entity.entityId, entity.belongsPlayerId, package);
		}
	}

	public override int GetLength()
	{
		return 58;
	}
}
