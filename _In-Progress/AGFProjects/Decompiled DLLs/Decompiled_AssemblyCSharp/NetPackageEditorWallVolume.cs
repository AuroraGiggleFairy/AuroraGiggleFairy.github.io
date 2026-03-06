using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorWallVolume : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEditorSleeperVolume.EChangeType changeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int volumeId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	public NetPackageEditorWallVolume Setup(NetPackageEditorSleeperVolume.EChangeType _changeType, int _prefabInstanceId, int _volumeId, Prefab.PrefabWallVolume _volume)
	{
		changeType = _changeType;
		prefabInstanceId = _prefabInstanceId;
		volumeId = _volumeId;
		startPos = _volume.startPos;
		size = _volume.size;
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Prefab.PrefabWallVolume volumeSettings = new Prefab.PrefabWallVolume
		{
			startPos = startPos,
			size = size
		};
		if (!_world.IsRemote())
		{
			switch (changeType)
			{
			case NetPackageEditorSleeperVolume.EChangeType.Changed:
				PrefabVolumeManager.Instance.UpdateWallPropertiesServer(prefabInstanceId, volumeId, volumeSettings);
				break;
			case NetPackageEditorSleeperVolume.EChangeType.Removed:
				PrefabVolumeManager.Instance.UpdateWallPropertiesServer(prefabInstanceId, volumeId, volumeSettings, remove: true);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		else
		{
			switch (changeType)
			{
			case NetPackageEditorSleeperVolume.EChangeType.Added:
			case NetPackageEditorSleeperVolume.EChangeType.Changed:
				PrefabVolumeManager.Instance.AddUpdateWallPropertiesClient(prefabInstanceId, volumeId, volumeSettings);
				break;
			case NetPackageEditorSleeperVolume.EChangeType.Removed:
				PrefabVolumeManager.Instance.AddUpdateWallPropertiesClient(prefabInstanceId, volumeId, volumeSettings, remove: true);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public override void read(PooledBinaryReader _br)
	{
		changeType = (NetPackageEditorSleeperVolume.EChangeType)_br.ReadByte();
		prefabInstanceId = _br.ReadInt32();
		volumeId = _br.ReadInt32();
		startPos = StreamUtils.ReadVector3i(_br);
		size = StreamUtils.ReadVector3i(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)changeType);
		_bw.Write(prefabInstanceId);
		_bw.Write(volumeId);
		StreamUtils.Write(_bw, startPos);
		StreamUtils.Write(_bw, size);
	}

	public override int GetLength()
	{
		return 37;
	}
}
