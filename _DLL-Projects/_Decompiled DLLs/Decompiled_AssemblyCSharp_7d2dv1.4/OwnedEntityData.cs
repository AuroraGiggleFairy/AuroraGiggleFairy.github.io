using System;
using UnityEngine;
using UnityEngine.Scripting;

[Serializable]
[Preserve]
public class OwnedEntityData
{
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId = -1;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public int classId = -1;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastKnownPosition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ushort saveFlags;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cHasLastKnownPosition = 1;

	[field: NonSerialized]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityCreationData EntityCreationData
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public int Id => entityId;

	public int ClassId => classId;

	public Vector3 LastKnownPosition => lastKnownPosition;

	public bool hasLastKnownPosition => (saveFlags & 1) > 0;

	public OwnedEntityData()
	{
	}

	public OwnedEntityData(Entity _entity)
	{
		entityId = _entity.entityId;
		classId = _entity.entityClass;
		EntityCreationData = new EntityCreationData(_entity);
	}

	public OwnedEntityData(int _entityId, int _classId)
	{
		entityId = _entityId;
		classId = _classId;
	}

	public void SetLastKnownPosition(Vector3 pos)
	{
		lastKnownPosition = pos;
		saveFlags |= 1;
	}

	public void ClearLastKnownPostition()
	{
		lastKnownPosition = Vector3.zero;
		saveFlags = (ushort)(saveFlags & -2);
	}

	public void Read(PooledBinaryReader _br)
	{
		entityId = _br.ReadInt32();
		classId = _br.ReadInt32();
		saveFlags = _br.ReadUInt16();
		if ((saveFlags & 1) > 0)
		{
			lastKnownPosition.x = _br.ReadInt32();
			lastKnownPosition.y = _br.ReadInt32();
			lastKnownPosition.z = _br.ReadInt32();
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(Id);
		_bw.Write(ClassId);
		_bw.Write(saveFlags);
		if ((saveFlags & 1) > 0)
		{
			_bw.Write((int)lastKnownPosition.x);
			_bw.Write((int)lastKnownPosition.y);
			_bw.Write((int)lastKnownPosition.z);
		}
	}
}
