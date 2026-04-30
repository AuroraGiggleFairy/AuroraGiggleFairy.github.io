using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRagdoll : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte state;

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumBodyPartHit bodyPart;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 forceVec;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 forceWorldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 hipPos;

	public NetPackageEntityRagdoll Setup(Entity _entity, sbyte _state)
	{
		entityId = _entity.entityId;
		state = _state;
		return this;
	}

	public NetPackageEntityRagdoll Setup(Entity _entity, float _duration, EnumBodyPartHit _bodyPart, Vector3 _forceVec, Vector3 _forceWorldPos)
	{
		entityId = _entity.entityId;
		state = -1;
		duration = _duration;
		bodyPart = _bodyPart;
		forceVec = _forceVec;
		forceWorldPos = _forceWorldPos;
		hipPos = _entity.emodel.GetHipPosition();
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
		state = _br.ReadSByte();
		if (state < 0)
		{
			duration = _br.ReadSingle();
			bodyPart = (EnumBodyPartHit)_br.ReadInt16();
			forceVec = StreamUtils.ReadVector3(_br);
			forceWorldPos = StreamUtils.ReadVector3(_br);
			hipPos = StreamUtils.ReadVector3(_br);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
		_bw.Write(state);
		if (state < 0)
		{
			_bw.Write(duration);
			_bw.Write((short)bodyPart);
			StreamUtils.Write(_bw, forceVec);
			StreamUtils.Write(_bw, forceWorldPos);
			StreamUtils.Write(_bw, hipPos);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
			if (entityAlive == null)
			{
				Log.Out("Discarding " + GetType().Name + " for entity Id=" + entityId);
			}
			else if (state < 0)
			{
				entityAlive.emodel.DoRagdoll(duration, bodyPart, forceVec, forceWorldPos, isRemote: true);
			}
			else
			{
				entityAlive.emodel.SetRagdollState(state);
			}
		}
	}

	public override int GetLength()
	{
		return 48;
	}
}
