using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorTriggerVolume : NetPackage
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

	[PublicizedFrom(EAccessModifier.Private)]
	public List<byte> triggersIndices = new List<byte>();

	public NetPackageEditorTriggerVolume Setup(NetPackageEditorSleeperVolume.EChangeType _changeType, int _prefabInstanceId, int _volumeId, Prefab.PrefabTriggerVolume _volume)
	{
		changeType = _changeType;
		prefabInstanceId = _prefabInstanceId;
		volumeId = _volumeId;
		startPos = _volume.startPos;
		size = _volume.size;
		triggersIndices = _volume.TriggersIndices;
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Prefab.PrefabTriggerVolume volumeSettings = new Prefab.PrefabTriggerVolume
		{
			startPos = startPos,
			size = size
		};
		if (!_world.IsRemote())
		{
			switch (changeType)
			{
			case NetPackageEditorSleeperVolume.EChangeType.Changed:
				PrefabTriggerVolumeManager.Instance.UpdateTriggerPropertiesServer(prefabInstanceId, volumeId, volumeSettings);
				break;
			case NetPackageEditorSleeperVolume.EChangeType.Removed:
				PrefabTriggerVolumeManager.Instance.UpdateTriggerPropertiesServer(prefabInstanceId, volumeId, volumeSettings, remove: true);
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
				PrefabTriggerVolumeManager.Instance.AddUpdateTriggerPropertiesClient(prefabInstanceId, volumeId, volumeSettings);
				break;
			case NetPackageEditorSleeperVolume.EChangeType.Removed:
				PrefabTriggerVolumeManager.Instance.AddUpdateTriggerPropertiesClient(prefabInstanceId, volumeId, volumeSettings, remove: true);
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
		int num = _br.ReadByte();
		triggersIndices.Clear();
		for (int i = 0; i < num; i++)
		{
			triggersIndices.Add(_br.ReadByte());
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)changeType);
		_bw.Write(prefabInstanceId);
		_bw.Write(volumeId);
		StreamUtils.Write(_bw, startPos);
		StreamUtils.Write(_bw, size);
		_bw.Write((byte)triggersIndices.Count);
		for (int i = 0; i < triggersIndices.Count; i++)
		{
			_bw.Write(triggersIndices[i]);
		}
	}

	public override int GetLength()
	{
		return 37;
	}
}
