using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageItemActionEffects : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte slotIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte actionIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionFiringState firingState;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 direction;

	[PublicizedFrom(EAccessModifier.Private)]
	public int userData;

	public NetPackageItemActionEffects Setup(int _entityId, int _slotIdx, int _actionIdx, ItemActionFiringState _firingState, Vector3 _startPos, Vector3 _direction, int _userData)
	{
		entityId = _entityId;
		slotIdx = (byte)_slotIdx;
		actionIdx = (byte)_actionIdx;
		firingState = _firingState;
		startPos = _startPos;
		direction = _direction;
		userData = _userData;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		entityId = _reader.ReadInt32();
		slotIdx = _reader.ReadByte();
		actionIdx = _reader.ReadByte();
		firingState = (ItemActionFiringState)_reader.ReadByte();
		if (_reader.ReadBoolean())
		{
			startPos = StreamUtils.ReadVector3(_reader);
			direction = StreamUtils.ReadVector3(_reader);
		}
		else
		{
			startPos = Vector3.zero;
			direction = Vector3.zero;
		}
		userData = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(entityId);
		_writer.Write(slotIdx);
		_writer.Write(actionIdx);
		_writer.Write((byte)firingState);
		bool flag = !startPos.Equals(Vector3.zero) || !direction.Equals(Vector3.zero);
		_writer.Write(flag);
		if (flag)
		{
			StreamUtils.Write(_writer, startPos);
			StreamUtils.Write(_writer, direction);
		}
		_writer.Write(userData);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (!_world.IsRemote())
			{
				_world.GetGameManager().ItemActionEffectsServer(entityId, slotIdx, actionIdx, (int)firingState, startPos, direction, userData);
			}
			else
			{
				_world.GetGameManager().ItemActionEffectsClient(entityId, slotIdx, actionIdx, (int)firingState, startPos, direction, userData);
			}
		}
	}

	public override int GetLength()
	{
		return 50;
	}
}
