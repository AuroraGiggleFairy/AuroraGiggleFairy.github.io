using UnityEngine;
using UnityEngine.Scripting;

namespace Audio;

[Preserve]
public class NetPackageAudio : NetPackageEntityTargeted
{
	public string soundGroupName;

	public bool play;

	public Vector3 position;

	public bool playOnEntity;

	public float occlusion;

	public bool signalOnly;

	public float volumeScale;

	public NetPackageAudio Setup(int _playOnEntityId, string _soundGroupName, float _occlusion, bool _play, bool _signalOnly = false, float _volumeScale = 1f)
	{
		Setup(_playOnEntityId);
		playOnEntity = true;
		soundGroupName = _soundGroupName;
		play = _play;
		occlusion = _occlusion;
		signalOnly = _signalOnly;
		volumeScale = _volumeScale;
		return this;
	}

	public NetPackageAudio Setup(Vector3 _position, string _soundGroupName, float _occlusion, bool _play, int _entityId = -1, float _volumeScale = 1f)
	{
		Setup(_entityId);
		playOnEntity = false;
		position = _position;
		soundGroupName = _soundGroupName;
		play = _play;
		occlusion = _occlusion;
		volumeScale = _volumeScale;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		soundGroupName = _reader.ReadString();
		play = _reader.ReadBoolean();
		float x = _reader.ReadSingle();
		float y = _reader.ReadSingle();
		float z = _reader.ReadSingle();
		position.x = x;
		position.y = y;
		position.z = z;
		playOnEntity = _reader.ReadBoolean();
		occlusion = _reader.ReadSingle();
		volumeScale = _reader.ReadSingle();
		signalOnly = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((soundGroupName != null) ? soundGroupName : "");
		_writer.Write(play);
		_writer.Write(position.x);
		_writer.Write(position.y);
		_writer.Write(position.z);
		_writer.Write(playOnEntity);
		_writer.Write(occlusion);
		_writer.Write(volumeScale);
		_writer.Write(signalOnly);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || string.IsNullOrEmpty(soundGroupName))
		{
			return;
		}
		if (playOnEntity && entityId >= 0)
		{
			Entity entity = _world.GetEntity(entityId);
			if (entity == null)
			{
				return;
			}
			if (GameManager.IsDedicatedServer && Manager.ServerAudio != null)
			{
				if (play)
				{
					Manager.ServerAudio.Play(entity, soundGroupName, occlusion, signalOnly, volumeScale);
				}
				else
				{
					Manager.ServerAudio.Stop(entityId, soundGroupName);
				}
			}
			else if (!GameManager.IsDedicatedServer && Manager.ServerAudio != null)
			{
				if (play)
				{
					Manager.Play(entity, soundGroupName, volumeScale);
					Manager.ServerAudio.Play(entity, soundGroupName, occlusion, signalOnly, volumeScale);
				}
				else
				{
					Manager.Stop(entityId, soundGroupName);
					Manager.ServerAudio.Stop(entityId, soundGroupName);
				}
			}
			else
			{
				if (Manager.ServerAudio != null)
				{
					return;
				}
				if (play)
				{
					if (!signalOnly)
					{
						Manager.Play(entity, soundGroupName, volumeScale);
					}
				}
				else
				{
					Manager.Stop(entityId, soundGroupName);
				}
			}
		}
		else if (GameManager.IsDedicatedServer && Manager.ServerAudio != null)
		{
			if (play)
			{
				Manager.ServerAudio.Play(position, soundGroupName, occlusion, entityId, volumeScale);
			}
			else
			{
				Manager.ServerAudio.Stop(position, soundGroupName);
			}
		}
		else if (!GameManager.IsDedicatedServer && Manager.ServerAudio != null)
		{
			if (play)
			{
				Manager.Play(position, soundGroupName, entityId, wantHandle: false, volumeScale);
				Manager.ServerAudio.Play(position, soundGroupName, occlusion, entityId, volumeScale);
			}
			else
			{
				Manager.Stop(position, soundGroupName);
				Manager.ServerAudio.Stop(position, soundGroupName);
			}
		}
		else if (Manager.ServerAudio == null)
		{
			if (play)
			{
				Manager.Play(position, soundGroupName, entityId, wantHandle: false, volumeScale);
			}
			else
			{
				Manager.Stop(position, soundGroupName);
			}
		}
	}

	public override int GetLength()
	{
		return 10;
	}
}
