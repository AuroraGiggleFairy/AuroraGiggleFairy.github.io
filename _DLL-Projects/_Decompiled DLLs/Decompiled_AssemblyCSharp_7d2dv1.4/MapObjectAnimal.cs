using UnityEngine;

public class MapObjectAnimal : MapObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTracked;

	public MapObjectAnimal(Entity _entity)
		: base(EnumMapObjectType.Entity, Vector3.zero, _entity.entityId, _entity, _bSelectable: false)
	{
	}

	public MapObjectAnimal(MapObjectAnimal _other)
		: base(EnumMapObjectType.Entity, _other.position, _other.entity.entityId, _other.entity, _bSelectable: false)
	{
	}

	public override void RefreshData()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			if (!((EntityAlive)entity).IsAlive())
			{
				isTracked = false;
				return;
			}
			EntityPlayerLocal primaryPlayer = entity.world.GetPrimaryPlayer();
			if (primaryPlayer != null && EffectManager.GetValue(PassiveEffects.Tracking, null, 0f, primaryPlayer, null, entity.EntityTags) > 0f)
			{
				isTracked = true;
				return;
			}
		}
		isTracked = false;
	}

	public override bool IsOnCompass()
	{
		return isTracked;
	}

	public override string GetMapIcon()
	{
		if (!isTracked)
		{
			return entity.GetMapIcon();
		}
		return entity.GetTrackerIcon();
	}

	public override string GetCompassIcon()
	{
		if (!isTracked)
		{
			return entity.GetCompassIcon();
		}
		return entity.GetTrackerIcon();
	}

	public override bool NearbyCompassBlink()
	{
		return true;
	}

	public override bool IsMapIconEnabled()
	{
		return isTracked;
	}

	public override float GetMaxCompassIconScale()
	{
		return 1f;
	}

	public override float GetMinCompassIconScale()
	{
		return 0.6f;
	}

	public override bool UseUpDownCompassIcons()
	{
		return false;
	}

	public override float GetMaxCompassDistance()
	{
		return 500f;
	}

	public override bool IsShowName()
	{
		return false;
	}
}
