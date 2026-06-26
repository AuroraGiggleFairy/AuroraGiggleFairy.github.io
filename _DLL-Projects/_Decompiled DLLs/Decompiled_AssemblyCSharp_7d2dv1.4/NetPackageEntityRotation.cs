using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRotation : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i rot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion qrot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUseQRotation;

	public override bool ReliableDelivery => false;

	public NetPackageEntityRotation Setup(int _entityId, Vector3i _rot, Quaternion _qrot, bool _bUseQRot)
	{
		entityId = _entityId;
		rot = _rot;
		qrot = _qrot;
		bUseQRotation = _bUseQRot;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
		bUseQRotation = _br.ReadBoolean();
		if (!bUseQRotation)
		{
			rot = new Vector3i(_br.ReadInt16(), _br.ReadInt16(), _br.ReadInt16());
		}
		else
		{
			qrot = new Quaternion(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
		_bw.Write(bUseQRotation);
		if (!bUseQRotation)
		{
			_bw.Write((short)rot.x);
			_bw.Write((short)rot.y);
			_bw.Write((short)rot.z);
		}
		else
		{
			_bw.Write(qrot.x);
			_bw.Write(qrot.y);
			_bw.Write(qrot.z);
			_bw.Write(qrot.w);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !ValidEntityIdForSender(entityId))
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
			if (bUseQRotation)
			{
				entity.SetQRotFromNetwork(qrot, 3);
				return;
			}
			Vector3 vector = new Vector3((float)(rot.x * 360) / 256f, (float)(rot.y * 360) / 256f, (float)(rot.z * 360) / 256f);
			entity.SetRotFromNetwork(vector, 3);
		}
	}

	public override int GetLength()
	{
		return 12;
	}
}
