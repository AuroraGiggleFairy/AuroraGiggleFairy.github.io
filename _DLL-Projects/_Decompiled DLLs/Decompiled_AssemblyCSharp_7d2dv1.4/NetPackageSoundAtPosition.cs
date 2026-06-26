using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSoundAtPosition : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 pos;

	[PublicizedFrom(EAccessModifier.Private)]
	public string audioClipName;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioRolloffMode mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public int distance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	public NetPackageSoundAtPosition Setup(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, int _entityId)
	{
		pos = _pos;
		audioClipName = _audioClipName;
		mode = _mode;
		distance = _distance;
		entityId = _entityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		pos = StreamUtils.ReadVector3(_br);
		audioClipName = _br.ReadString();
		mode = (AudioRolloffMode)_br.ReadByte();
		distance = _br.ReadInt32();
		entityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, pos);
		_bw.Write(audioClipName);
		_bw.Write((byte)mode);
		_bw.Write(distance);
		_bw.Write(entityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (!_world.IsRemote())
			{
				_world.gameManager.PlaySoundAtPositionServer(pos, audioClipName, mode, distance, entityId);
			}
			else
			{
				_world.gameManager.PlaySoundAtPositionClient(pos, audioClipName, mode, distance);
			}
		}
	}

	public override int GetLength()
	{
		return 40;
	}
}
