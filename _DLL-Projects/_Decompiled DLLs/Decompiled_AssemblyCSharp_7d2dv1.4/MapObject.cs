using System;
using UnityEngine;

public class MapObject
{
	public EnumMapObjectType type;

	public long key;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position;

	public bool bSelectable;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Entity entity;

	public MapObject(EnumMapObjectType _type, Vector3 _position, long _key, Entity _entity, bool _bSelectable)
	{
		type = _type;
		position = _position;
		key = _key;
		bSelectable = _bSelectable;
		entity = _entity;
	}

	public MapObject(MapObject _other)
	{
		type = _other.type;
		position = _other.position;
		key = _other.key;
		bSelectable = _other.bSelectable;
		entity = _other.entity;
	}

	public virtual Vector3 GetPosition()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetPosition();
		}
		return position;
	}

	public virtual void SetPosition(Vector3 _pos)
	{
		if (type == EnumMapObjectType.Entity)
		{
			throw new Exception("Setting of position not allowed!");
		}
		position = _pos;
	}

	public virtual Vector3 GetRotation()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			if (entity.AttachedToEntity != null)
			{
				return entity.AttachedToEntity.rotation;
			}
			return entity.rotation;
		}
		return Vector3.zero;
	}

	public virtual bool IsTracked()
	{
		return true;
	}

	public virtual bool IsMapIconEnabled()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.IsDrawMapIcon();
		}
		return true;
	}

	public virtual float GetMaxCompassDistance()
	{
		return 1024f;
	}

	public virtual float GetMinCompassDistance()
	{
		return 0f;
	}

	public virtual float GetMaxCompassIconScale()
	{
		return 1.25f;
	}

	public virtual float GetMinCompassIconScale()
	{
		return 0.5f;
	}

	public virtual bool IsCompassIconClamped()
	{
		return false;
	}

	public virtual bool NearbyCompassBlink()
	{
		return false;
	}

	public virtual Vector3 GetMapIconScale()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetMapIconScale();
		}
		return Vector3.one;
	}

	public virtual string GetMapIcon()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetMapIcon();
		}
		return "";
	}

	public virtual string GetCompassIcon()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetCompassIcon();
		}
		return null;
	}

	public virtual string GetCompassUpIcon()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetCompassUpIcon();
		}
		return "";
	}

	public virtual string GetCompassDownIcon()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetCompassDownIcon();
		}
		return "";
	}

	public virtual bool UseUpDownCompassIcons()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetCompassDownIcon() != null;
		}
		return false;
	}

	public virtual bool IsMapIconBlinking()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.IsMapIconBlinking();
		}
		return false;
	}

	public virtual Color GetMapIconColor()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			EntityPlayerLocal primaryPlayer = entity.world.GetPrimaryPlayer();
			if (primaryPlayer != null && primaryPlayer.Party != null && primaryPlayer.Party.MemberList.Contains(entity as EntityPlayer))
			{
				int num = primaryPlayer.Party.MemberList.IndexOf(entity as EntityPlayer);
				return Constants.TrackedFriendColors[num % Constants.TrackedFriendColors.Length];
			}
			return entity.GetMapIconColor();
		}
		return Color.white;
	}

	public virtual bool CanMapIconBeSelected()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.CanMapIconBeSelected();
		}
		return false;
	}

	public virtual bool IsOnCompass()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			EntityPlayerLocal primaryPlayer = entity.world.GetPrimaryPlayer();
			if (primaryPlayer != null && primaryPlayer.Party != null && entity != primaryPlayer)
			{
				return primaryPlayer.Party.MemberList.Contains(entity as EntityPlayer);
			}
		}
		return false;
	}

	public virtual int GetLayerForMapIcon()
	{
		if (type == EnumMapObjectType.Entity && entity != null)
		{
			return entity.GetLayerForMapIcon();
		}
		return 2;
	}

	public virtual string GetName()
	{
		if (type == EnumMapObjectType.Entity && entity is EntityAlive)
		{
			bool flag = !SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer;
			EntityPlayerLocal primaryPlayer = entity.world.GetPrimaryPlayer();
			if (!(entity is EntityPlayerLocal) && !(entity is EntityVehicle))
			{
				return ((EntityAlive)entity).EntityName;
			}
			if (primaryPlayer == entity && flag)
			{
				return Localization.Get("xuiMapSelfLabel");
			}
		}
		return null;
	}

	public virtual bool IsShowName()
	{
		return true;
	}

	public virtual bool IsCenterOnLeftBottomCorner()
	{
		return false;
	}

	public virtual float GetCompassIconScale(float _distance)
	{
		float t = 1f - _distance / GetMaxCompassDistance();
		return Mathf.Lerp(GetMinCompassIconScale(), GetMaxCompassIconScale(), t);
	}

	public virtual void RefreshData()
	{
	}
}
