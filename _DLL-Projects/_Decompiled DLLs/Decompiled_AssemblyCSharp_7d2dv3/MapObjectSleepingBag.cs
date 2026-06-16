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
		if (entity is EntityPlayer entityPlayer)
		{
			if (!(entityPlayer is EntityPlayerLocal))
			{
				return entityPlayer.IsFriendOfLocalPlayer;
			}
			return true;
		}
		return true;
	}

	public override Color GetMapIconColor()
	{
		if (entity is EntityPlayer entityPlayer && !(entityPlayer is EntityPlayerLocal))
		{
			_ = entityPlayer.IsFriendOfLocalPlayer;
			return Color.green * 0.75f;
		}
		return Color.white;
	}
}
