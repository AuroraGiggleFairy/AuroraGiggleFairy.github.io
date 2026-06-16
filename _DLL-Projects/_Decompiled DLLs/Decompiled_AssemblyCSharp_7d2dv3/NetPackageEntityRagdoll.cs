using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRagdoll : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsForce = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsMode = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsState = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte flags;

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

	[PublicizedFrom(EAccessModifier.Private)]
	public byte mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte state;

	public NetPackageEntityRagdoll Setup(Entity _entity, byte _state)
	{
		entityId = _entity.entityId;
		flags = 4;
		state = _state;
		return this;
	}

	public NetPackageEntityRagdoll Setup(Entity _entity, byte _mode, float _duration, EnumBodyPartHit _bodyPart, Vector3 _forceVec, Vector3 _forceWorldPos)
	{
		entityId = _entity.entityId;
		flags = 1;
		if (_mode != 0)
		{
			flags |= 2;
			mode = _mode;
		}
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
		flags = _br.ReadByte();
		if ((flags & 1) > 0)
		{
			duration = _br.ReadSingle();
			bodyPart = (EnumBodyPartHit)_br.ReadInt16();
			forceVec = StreamUtils.ReadVector3(_br);
			forceWorldPos = StreamUtils.ReadVector3(_br);
			hipPos = StreamUtils.ReadVector3(_br);
		}
		mode = 0;
		if ((flags & 2) > 0)
		{
			mode = _br.ReadByte();
		}
		state = 0;
		if ((flags & 4) > 0)
		{
			state = _br.ReadByte();
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(entityId);
		_bw.Write(flags);
		if ((flags & 1) > 0)
		{
			_bw.Write(duration);
			_bw.Write((short)bodyPart);
			StreamUtils.Write(_bw, forceVec);
			StreamUtils.Write(_bw, forceWorldPos);
			StreamUtils.Write(_bw, hipPos);
		}
		if ((flags & 2) > 0)
		{
			_bw.Write(mode);
		}
		if ((flags & 4) > 0)
		{
			_bw.Write(state);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(entityId) as EntityAlive;
		if (entityAlive == null)
		{
			Log.Out("Discarding " + GetType().Name + " for entity Id=" + entityId);
			return;
		}
		if ((flags & 1) > 0)
		{
			entityAlive.emodel.DoRagdoll((EModelBase.RagdollMode)mode, duration, bodyPart, forceVec, forceWorldPos, isRemote: true);
		}
		if ((flags & 4) > 0)
		{
			entityAlive.emodel.SetRagdollState(state);
		}
	}

	public override int GetLength()
	{
		return 48;
	}
}
