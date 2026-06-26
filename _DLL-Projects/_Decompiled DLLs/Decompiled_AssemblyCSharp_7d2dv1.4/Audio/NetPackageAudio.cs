using UnityEngine;
using UnityEngine.Scripting;

namespace Audio;

[Preserve]
public class NetPackageAudio : NetPackage
{
	public int playOnEntityId;

	public string soundGroupName;

	public bool play;

	public Vector3 position;

	public bool playOnEntity;

	public float occlusion;

	public bool signalOnly;

	public NetPackageAudio Setup(int _playOnEntityId, string _soundGroupName, float _occlusion, bool _play, bool _signalOnly = false)
	{
		playOnEntity = true;
		playOnEntityId = _playOnEntityId;
		soundGroupName = _soundGroupName;
		play = _play;
		occlusion = _occlusion;
		signalOnly = _signalOnly;
		return this;
	}

	public NetPackageAudio Setup(Vector3 _position, string _soundGroupName, float _occlusion, bool _play, int entityId = -1)
	{
		playOnEntity = false;
		position = _position;
		playOnEntityId = entityId;
		soundGroupName = _soundGroupName;
		play = _play;
		occlusion = _occlusion;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		playOnEntityId = _reader.ReadInt32();
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
		signalOnly = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(playOnEntityId);
		_writer.Write((soundGroupName != null) ? soundGroupName : "");
		_writer.Write(play);
		_writer.Write(position.x);
		_writer.Write(position.y);
		_writer.Write(position.z);
		_writer.Write(playOnEntity);
		_writer.Write(occlusion);
		_writer.Write(signalOnly);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || string.IsNullOrEmpty(soundGroupName))
		{
			return;
		}
		if (playOnEntity && playOnEntityId >= 0)
		{
			Entity entity = _world.GetEntity(playOnEntityId);
			if (entity == null)
			{
				return;
			}
			if (GameManager.IsDedicatedServer && Manager.ServerAudio != null)
			{
				if (play)
				{
					Manager.ServerAudio.Play(entity, soundGroupName, occlusion, signalOnly);
				}
				else
				{
					Manager.ServerAudio.Stop(playOnEntityId, soundGroupName);
				}
			}
			else if (!GameManager.IsDedicatedServer && Manager.ServerAudio != null)
			{
				if (play)
				{
					Manager.Play(entity, soundGroupName);
					Manager.ServerAudio.Play(entity, soundGroupName, occlusion, signalOnly);
				}
				else
				{
					Manager.Stop(playOnEntityId, soundGroupName);
					Manager.ServerAudio.Stop(playOnEntityId, soundGroupName);
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
						Manager.Play(entity, soundGroupName);
					}
				}
				else
				{
					Manager.Stop(playOnEntityId, soundGroupName);
				}
			}
		}
		else if (GameManager.IsDedicatedServer && Manager.ServerAudio != null)
		{
			if (play)
			{
				Manager.ServerAudio.Play(position, soundGroupName, occlusion, playOnEntityId);
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
				Manager.Play(position, soundGroupName, playOnEntityId);
				Manager.ServerAudio.Play(position, soundGroupName, occlusion, playOnEntityId);
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
				Manager.Play(position, soundGroupName, playOnEntityId);
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
