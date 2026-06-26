using UnityEngine;

public class MapObjectZombie : MapObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum TrackingTypes
	{
		None,
		Tracking,
		Quest
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TrackingTypes trackingType;

	public MapObjectZombie(Entity _entity)
		: base(EnumMapObjectType.Entity, Vector3.zero, _entity.entityId, _entity, _bSelectable: false)
	{
	}

	public MapObjectZombie(MapObjectZombie _other)
		: base(EnumMapObjectType.Entity, _other.position, _other.entity.entityId, _other.entity, _bSelectable: false)
	{
	}

	public override void RefreshData()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			_ = (EntityAlive)entity;
			entity.world.GetPrimaryPlayer();
		}
		trackingType = TrackingTypes.None;
	}

	public override bool IsOnCompass()
	{
		return trackingType != TrackingTypes.None;
	}

	public override string GetMapIcon()
	{
		if (trackingType != TrackingTypes.Tracking)
		{
			return entity.GetMapIcon();
		}
		return entity.GetTrackerIcon();
	}

	public override string GetCompassIcon()
	{
		if (trackingType != TrackingTypes.Tracking)
		{
			return entity.GetCompassIcon();
		}
		return entity.GetTrackerIcon();
	}

	public override bool UseUpDownCompassIcons()
	{
		return trackingType == TrackingTypes.Quest;
	}

	public override bool IsCompassIconClamped()
	{
		return trackingType == TrackingTypes.Quest;
	}

	public override bool NearbyCompassBlink()
	{
		return true;
	}

	public override bool IsMapIconEnabled()
	{
		return trackingType == TrackingTypes.Tracking;
	}

	public override float GetMaxCompassIconScale()
	{
		return 1f;
	}

	public override float GetMinCompassIconScale()
	{
		return 0.6f;
	}

	public override Color GetMapIconColor()
	{
		if (trackingType != TrackingTypes.Quest)
		{
			return entity.GetMapIconColor();
		}
		return Color.red;
	}

	public override float GetMaxCompassDistance()
	{
		return (trackingType == TrackingTypes.Quest) ? 32 : 100;
	}

	public override bool IsShowName()
	{
		return false;
	}
}
