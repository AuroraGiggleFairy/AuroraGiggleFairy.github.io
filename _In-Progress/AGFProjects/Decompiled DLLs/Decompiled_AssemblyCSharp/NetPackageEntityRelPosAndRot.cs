using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRelPosAndRot : NetPackageEntityRotation
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i dPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onGround;

	[PublicizedFrom(EAccessModifier.Private)]
	public short updateSteps;

	public override bool ReliableDelivery => false;

	public NetPackageEntityRelPosAndRot Setup(int _entityId, Vector3i _deltaPos, Vector3i _absRot, Quaternion _qrot, bool _onGround, bool _bUseQRot, int _updateSteps)
	{
		Setup(_entityId, _absRot, _qrot, _bUseQRot);
		dPos = _deltaPos;
		onGround = _onGround;
		updateSteps = (short)_updateSteps;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		dPos = new Vector3i(_reader.ReadInt16(), _reader.ReadInt16(), _reader.ReadInt16());
		onGround = _reader.ReadBoolean();
		updateSteps = _reader.ReadInt16();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((short)dPos.x);
		_writer.Write((short)dPos.y);
		_writer.Write((short)dPos.z);
		_writer.Write(onGround);
		_writer.Write(updateSteps);
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
			entity.serverPos += dPos;
			Vector3 pos = entity.serverPos.ToVector3() / 32f;
			Vector3 vector = new Vector3((float)(rot.x * 360) / 256f, (float)(rot.y * 360) / 256f, (float)(rot.z * 360) / 256f);
			if (bUseQRotation)
			{
				entity.SetPosAndQRotFromNetwork(pos, qrot, updateSteps);
			}
			else
			{
				entity.SetPosAndRotFromNetwork(pos, vector, updateSteps);
			}
			entity.onGround = onGround;
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
