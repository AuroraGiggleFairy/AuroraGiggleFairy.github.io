using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityMapMarkerRemove : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum RemoveByTypes
	{
		EntityID,
		Position
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumMapObjectType mapObjectType;

	[PublicizedFrom(EAccessModifier.Private)]
	public RemoveByTypes RemoveByType;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

	public NetPackageEntityMapMarkerRemove Setup(EnumMapObjectType _mapObjectType, int _entityId)
	{
		mapObjectType = _mapObjectType;
		entityId = _entityId;
		RemoveByType = RemoveByTypes.EntityID;
		return this;
	}

	public NetPackageEntityMapMarkerRemove Setup(EnumMapObjectType _mapObjectType, Vector3 _position)
	{
		mapObjectType = _mapObjectType;
		position = _position;
		RemoveByType = RemoveByTypes.Position;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		RemoveByType = (RemoveByTypes)_reader.ReadInt32();
		if (RemoveByType == RemoveByTypes.EntityID)
		{
			entityId = _reader.ReadInt32();
		}
		else
		{
			position = StreamUtils.ReadVector3(_reader);
		}
		mapObjectType = (EnumMapObjectType)_reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((int)RemoveByType);
		if (RemoveByType == RemoveByTypes.EntityID)
		{
			_writer.Write(entityId);
		}
		else
		{
			StreamUtils.Write(_writer, position);
		}
		_writer.Write((int)mapObjectType);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null)
		{
			if (RemoveByType == RemoveByTypes.EntityID)
			{
				_world.ObjectOnMapRemove(mapObjectType, entityId);
			}
			else
			{
				_world.ObjectOnMapRemove(mapObjectType, position);
			}
		}
	}

	public override int GetLength()
	{
		return 4;
	}
}
