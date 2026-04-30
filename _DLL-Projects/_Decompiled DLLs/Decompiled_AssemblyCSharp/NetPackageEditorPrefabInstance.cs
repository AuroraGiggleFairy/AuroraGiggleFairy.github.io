using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorPrefabInstance : NetPackage
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
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i boundingBoxPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i boundingBoxSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filename;

	[PublicizedFrom(EAccessModifier.Private)]
	public int localRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int yOffset;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEditorPrefabInstance Setup(EChangeType _changeType, PrefabInstance _prefabInstance)
	{
		changeType = _changeType;
		id = _prefabInstance.id;
		boundingBoxPosition = _prefabInstance.boundingBoxPosition;
		boundingBoxSize = _prefabInstance.boundingBoxSize;
		name = _prefabInstance.name;
		size = _prefabInstance.prefab.size;
		filename = _prefabInstance.prefab.PrefabName;
		localRotation = _prefabInstance.prefab.GetLocalRotation();
		yOffset = _prefabInstance.prefab.yOffset;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		changeType = (EChangeType)_br.ReadByte();
		id = _br.ReadInt32();
		boundingBoxPosition = StreamUtils.ReadVector3i(_br);
		boundingBoxSize = StreamUtils.ReadVector3i(_br);
		name = _br.ReadString();
		size = StreamUtils.ReadVector3i(_br);
		filename = _br.ReadString();
		localRotation = _br.ReadInt32();
		yOffset = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)changeType);
		_bw.Write(id);
		StreamUtils.Write(_bw, boundingBoxPosition);
		StreamUtils.Write(_bw, boundingBoxSize);
		_bw.Write(name);
		StreamUtils.Write(_bw, size);
		_bw.Write(filename);
		_bw.Write(localRotation);
		_bw.Write(yOffset);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.IsRemote())
		{
			switch (changeType)
			{
			case EChangeType.Added:
				PrefabSleeperVolumeManager.Instance.PrefabLoadedClient(id, boundingBoxPosition, boundingBoxSize, name, size, filename, localRotation, yOffset);
				break;
			case EChangeType.Changed:
				PrefabSleeperVolumeManager.Instance.PrefabChangedClient(id, boundingBoxPosition, boundingBoxSize, name, size, filename, localRotation, yOffset);
				break;
			case EChangeType.Removed:
				PrefabSleeperVolumeManager.Instance.PrefabRemovedClient(id);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public override int GetLength()
	{
		return 33 + name.Length + 12 + filename.Length + 4;
	}
}
