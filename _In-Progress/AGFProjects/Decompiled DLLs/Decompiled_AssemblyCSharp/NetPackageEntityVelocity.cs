using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityVelocity : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float max = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 motion;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAdd;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityVelocity Setup(int _entityId, Vector3 _motion, bool _bAdd)
	{
		entityId = _entityId;
		bAdd = _bAdd;
		if (_motion.x < -8f)
		{
			_motion.x = -8f;
		}
		if (_motion.y < -8f)
		{
			_motion.y = -8f;
		}
		if (_motion.z < -8f)
		{
			_motion.z = -8f;
		}
		if (_motion.x > 8f)
		{
			_motion.x = 8f;
		}
		if (_motion.y > 8f)
		{
			_motion.y = 8f;
		}
		if (_motion.z > 8f)
		{
			_motion.z = 8f;
		}
		motion = _motion;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		bAdd = _reader.ReadBoolean();
		motion = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(bAdd);
		_writer.Write(motion.x);
		_writer.Write(motion.y);
		_writer.Write(motion.z);
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
			if (!bAdd)
			{
				entity.SetVelocity(motion);
			}
			else
			{
				entity.AddVelocity(motion);
			}
		}
		else
		{
			Log.Out("Discarding " + GetType().Name + " for entity Id=" + entityId);
		}
	}

	public override int GetLength()
	{
		return 16;
	}
}
