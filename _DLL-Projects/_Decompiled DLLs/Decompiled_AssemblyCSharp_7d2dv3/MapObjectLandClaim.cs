using UnityEngine;

public class MapObjectLandClaim : MapObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int MapObjectLandCLaimKeys;

	public MapObjectLandClaim(Vector3 _position, Entity _entity)
		: base(EnumMapObjectType.LandClaim, _position, MapObjectLandCLaimKeys++, _entity, _bSelectable: false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_brick";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_brick";
	}

	public override bool IsOnCompass()
	{
		return IsMapIconEnabled();
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		if (entity != null)
		{
			return entity is EntityPlayerLocal;
		}
		return false;
	}

	public override Color GetMapIconColor()
	{
		return Color.white;
	}
}
