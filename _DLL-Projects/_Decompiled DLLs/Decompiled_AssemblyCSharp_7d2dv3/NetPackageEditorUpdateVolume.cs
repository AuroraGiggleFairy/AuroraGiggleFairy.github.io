using System;
using PrefabVolumes;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorUpdateVolume : NetPackage
{
	public enum EChangeType
	{
		Added,
		Changed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EChangeType changeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int volumeId;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabVolumeAbs volume;

	public NetPackageEditorUpdateVolume Setup(EChangeType _changeType, int _prefabInstanceId, int _volumeId, PrefabVolumeAbs _volume)
	{
		changeType = _changeType;
		prefabInstanceId = _prefabInstanceId;
		volumeId = _volumeId;
		volume = _volume.Clone();
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		changeType = (EChangeType)_br.ReadByte();
		prefabInstanceId = _br.ReadInt32();
		volumeId = _br.ReadInt32();
		PrefabVolumeAbs.EVolumeType volumeType = (PrefabVolumeAbs.EVolumeType)_br.ReadByte();
		volume = PrefabVolumeAbs.CreateByType(volumeType);
		volume.Read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)changeType);
		_bw.Write(prefabInstanceId);
		_bw.Write(volumeId);
		_bw.Write((byte)volume.VolumeType);
		volume.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			EChangeType eChangeType = changeType;
			if (eChangeType == EChangeType.Added || eChangeType != EChangeType.Changed)
			{
				throw new ArgumentOutOfRangeException();
			}
			PrefabVolumeManager.Instance.UpdatePropertiesServer(prefabInstanceId, volumeId, volume);
		}
		else
		{
			EChangeType eChangeType = changeType;
			if ((uint)eChangeType > 1u)
			{
				throw new ArgumentOutOfRangeException();
			}
			PrefabVolumeManager.Instance.AddUpdatePropertiesClient(prefabInstanceId, volumeId, volume);
		}
	}

	public override int GetLength()
	{
		return 14 + volume.SerializedSize;
	}
}
