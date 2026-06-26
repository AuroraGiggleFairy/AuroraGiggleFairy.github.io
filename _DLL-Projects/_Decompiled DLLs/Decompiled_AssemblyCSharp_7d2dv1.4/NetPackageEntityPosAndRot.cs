using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityPosAndRot : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 pos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 rot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool onGround;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion qrot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUseQRotation;

	public override bool ReliableDelivery => false;

	public NetPackageEntityPosAndRot Setup(Entity _entity)
	{
		entityId = _entity.entityId;
		pos = _entity.position;
		rot = _entity.rotation;
		onGround = _entity.onGround;
		qrot = _entity.qrotation;
		bUseQRotation = _entity.IsQRotationUsed();
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
		pos = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		bUseQRotation = _br.ReadBoolean();
		if (!bUseQRotation)
		{
			rot = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		}
		else
		{
			qrot = new Quaternion(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		}
		onGround = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
		_bw.Write(pos.x);
		_bw.Write(pos.y);
		_bw.Write(pos.z);
		_bw.Write(bUseQRotation);
		if (!bUseQRotation)
		{
			_bw.Write(rot.x);
			_bw.Write(rot.y);
			_bw.Write(rot.z);
		}
		else
		{
			_bw.Write(qrot.x);
			_bw.Write(qrot.y);
			_bw.Write(qrot.z);
			_bw.Write(qrot.w);
		}
		_bw.Write(onGround);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !ValidEntityIdForSender(entityId, _allowAttachedToEntity: true))
		{
			return;
		}
		Entity entity = _world.GetEntity(entityId);
		if (entity == null)
		{
			return;
		}
		Entity attachedMainEntity = entity.AttachedMainEntity;
		if (!(attachedMainEntity != null) || _world.GetPrimaryPlayerId() != attachedMainEntity.entityId)
		{
			entity.serverPos = NetEntityDistributionEntry.EncodePos(pos);
			if (bUseQRotation)
			{
				entity.SetPosAndQRotFromNetwork(pos, qrot, 3);
			}
			else
			{
				entity.SetPosAndRotFromNetwork(pos, rot, 3);
			}
			entity.onGround = onGround;
		}
	}

	public override int GetLength()
	{
		return 25;
	}
}
