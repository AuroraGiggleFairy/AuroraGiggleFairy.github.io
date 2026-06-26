using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorSleeperVolume : NetPackage
{
	public enum EChangeType
	{
		Added,
		Changed,
		Removed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EChangeType changeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int volumeId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool used;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPriority;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isQuestExclude;

	[PublicizedFrom(EAccessModifier.Private)]
	public short spawnCountMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public short spawnCountMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public short groupId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int flags;

	[PublicizedFrom(EAccessModifier.Private)]
	public string minScript;

	public NetPackageEditorSleeperVolume Setup(EChangeType _changeType, int _prefabInstanceId, int _volumeId, Prefab.PrefabSleeperVolume _volume)
	{
		changeType = _changeType;
		prefabInstanceId = _prefabInstanceId;
		volumeId = _volumeId;
		used = _volume.used;
		startPos = _volume.startPos;
		size = _volume.size;
		groupName = _volume.groupName;
		isPriority = _volume.isPriority;
		isQuestExclude = _volume.isQuestExclude;
		spawnCountMin = _volume.spawnCountMin;
		spawnCountMax = _volume.spawnCountMax;
		groupId = _volume.groupId;
		flags = _volume.flags;
		minScript = _volume.minScript;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		changeType = (EChangeType)_br.ReadByte();
		prefabInstanceId = _br.ReadInt32();
		volumeId = _br.ReadInt32();
		used = _br.ReadBoolean();
		startPos = StreamUtils.ReadVector3i(_br);
		size = StreamUtils.ReadVector3i(_br);
		groupName = _br.ReadString();
		isPriority = _br.ReadBoolean();
		isQuestExclude = _br.ReadBoolean();
		spawnCountMin = _br.ReadInt16();
		spawnCountMax = _br.ReadInt16();
		groupId = _br.ReadInt16();
		flags = _br.ReadInt32();
		minScript = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)changeType);
		_bw.Write(prefabInstanceId);
		_bw.Write(volumeId);
		_bw.Write(used);
		StreamUtils.Write(_bw, startPos);
		StreamUtils.Write(_bw, size);
		_bw.Write(groupName);
		_bw.Write(isPriority);
		_bw.Write(isQuestExclude);
		_bw.Write(spawnCountMin);
		_bw.Write(spawnCountMax);
		_bw.Write(groupId);
		_bw.Write(flags);
		_bw.Write(minScript ?? "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Prefab.PrefabSleeperVolume volumeSettings = new Prefab.PrefabSleeperVolume
		{
			used = used,
			startPos = startPos,
			size = size,
			groupName = groupName,
			isPriority = isPriority,
			isQuestExclude = isQuestExclude,
			spawnCountMin = spawnCountMin,
			spawnCountMax = spawnCountMax,
			groupId = groupId,
			flags = flags,
			minScript = minScript
		};
		if (!_world.IsRemote())
		{
			EChangeType eChangeType = changeType;
			if (eChangeType == EChangeType.Added || (uint)(eChangeType - 1) > 1u)
			{
				throw new ArgumentOutOfRangeException();
			}
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(prefabInstanceId, volumeId, volumeSettings);
		}
		else
		{
			EChangeType eChangeType = changeType;
			if ((uint)eChangeType > 2u)
			{
				throw new ArgumentOutOfRangeException();
			}
			PrefabSleeperVolumeManager.Instance.AddUpdateSleeperPropertiesClient(prefabInstanceId, volumeId, volumeSettings);
		}
	}

	public override int GetLength()
	{
		return 38 + groupName.Length + 1 + 1 + 2 + 2;
	}
}
