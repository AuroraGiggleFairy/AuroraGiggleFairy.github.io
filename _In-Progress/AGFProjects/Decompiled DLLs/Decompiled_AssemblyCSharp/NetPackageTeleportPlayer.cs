using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTeleportPlayer : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 pos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3? viewDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool onlyIfNotFlying;

	public NetPackageTeleportPlayer Setup(Vector3 _newPos, Vector3? _viewDirection = null, bool _onlyIfNotFlying = false)
	{
		pos = _newPos;
		viewDirection = _viewDirection;
		onlyIfNotFlying = _onlyIfNotFlying;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		pos = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		if (_reader.ReadBoolean())
		{
			viewDirection = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		}
		else
		{
			viewDirection = null;
		}
		onlyIfNotFlying = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(pos.x);
		_writer.Write(pos.y);
		_writer.Write(pos.z);
		_writer.Write(viewDirection.HasValue);
		if (viewDirection.HasValue)
		{
			_writer.Write(viewDirection.Value.x);
			_writer.Write(viewDirection.Value.y);
			_writer.Write(viewDirection.Value.z);
		}
		_writer.Write(onlyIfNotFlying);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				primaryPlayer.TeleportToPosition(pos, onlyIfNotFlying, viewDirection);
			}
			else
			{
				Log.Out("Discarding " + GetType().Name + " (no local player)");
			}
		}
	}

	public override int GetLength()
	{
		return 13;
	}
}
