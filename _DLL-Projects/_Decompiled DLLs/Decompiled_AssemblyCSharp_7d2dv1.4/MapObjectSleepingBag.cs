using UnityEngine;

public class MapObjectSleepingBag : MapObject
{
	public MapObjectSleepingBag(Vector3 _position, Entity _entity)
		: base(EnumMapObjectType.SleepingBag, _position, _entity.entityId, _entity, _bSelectable: false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_map_bed";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_map_bed";
	}

	public override bool IsOnCompass()
	{
		if (IsMapIconEnabled())
		{
			return entity is EntityPlayerLocal;
		}
		return false;
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		if (entity != null && entity is EntityPlayer)
		{
			if (!(entity is EntityPlayerLocal))
			{
				return ((EntityPlayer)entity).IsFriendOfLocalPlayer;
			}
			return true;
		}
		return true;
	}

	public override Color GetMapIconColor()
	{
		if (entity != null && entity is EntityPlayer && !(entity is EntityPlayerLocal))
		{
			_ = ((EntityPlayer)entity).IsFriendOfLocalPlayer;
			return Color.green * 0.75f;
		}
		return Color.white;
	}
}
