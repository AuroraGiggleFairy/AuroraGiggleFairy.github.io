using PrefabVolumes;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddVolumeFromClient : NetPackage
{
	public enum EAddType
	{
		New,
		Clone
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EAddType addType;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabVolumeAbs.EVolumeType volumeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int existingIndex;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackageEditorAddVolumeFromClient Setup(PrefabVolumeAbs.EVolumeType _volumeType, Vector3i _startPos, Vector3i _size)
	{
		addType = EAddType.New;
		volumeType = _volumeType;
		startPos = _startPos;
		size = _size;
		return this;
	}

	public NetPackageEditorAddVolumeFromClient Setup(PrefabVolumeAbs.EVolumeType _volumeType, int _prefabInstanceId, int _existingIndex, Vector3i _offset)
	{
		addType = EAddType.Clone;
		volumeType = _volumeType;
		startPos = _offset;
		prefabInstanceId = _prefabInstanceId;
		existingIndex = _existingIndex;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		addType = (EAddType)_br.ReadByte();
		volumeType = (PrefabVolumeAbs.EVolumeType)_br.ReadByte();
		startPos = StreamUtils.ReadVector3i(_br);
		size = StreamUtils.ReadVector3i(_br);
		prefabInstanceId = _br.ReadInt16();
		existingIndex = _br.ReadInt16();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)addType);
		_bw.Write((byte)volumeType);
		StreamUtils.Write(_bw, startPos);
		StreamUtils.Write(_bw, size);
		_bw.Write((short)prefabInstanceId);
		_bw.Write((short)existingIndex);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (addType == EAddType.New)
		{
			PrefabVolumeManager.Instance.AddVolumeServer(volumeType, startPos, size);
		}
		else
		{
			PrefabVolumeManager.Instance.CloneVolumeServer(volumeType, prefabInstanceId, existingIndex, startPos);
		}
	}

	public override int GetLength()
	{
		return 29;
	}
}
