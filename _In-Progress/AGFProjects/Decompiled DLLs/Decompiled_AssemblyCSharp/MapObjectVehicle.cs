using UnityEngine;

public class MapObjectVehicle : MapObject
{
	public MapObjectVehicle(Entity _entity)
		: base(EnumMapObjectType.Entity, Vector3.zero, _entity.entityId, _entity, _bSelectable: false)
	{
	}

	public MapObjectVehicle(MapObjectVehicle _other)
		: base(EnumMapObjectType.Entity, _other.position, _other.entity.entityId, _other.entity, _bSelectable: false)
	{
	}

	public override bool IsOnCompass()
	{
		return !(entity as EntityVehicle).HasDriver;
	}

	public override string GetCompassIcon()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetMapIcon();
		}
		return null;
	}

	public override Vector3 GetRotation()
	{
		return Vector3.zero;
	}
}
