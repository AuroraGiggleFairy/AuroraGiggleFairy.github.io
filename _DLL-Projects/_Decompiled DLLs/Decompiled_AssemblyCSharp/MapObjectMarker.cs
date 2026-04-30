using UnityEngine;

public class MapObjectMarker : MapObject
{
	public MapObjectMarker(Vector3 _position, long _key)
		: base(EnumMapObjectType.MapQuickMarker, _position, _key, null, _bSelectable: false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_map_waypoint_set";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_map_waypoint_set";
	}

	public override Color GetMapIconColor()
	{
		return Color.red;
	}

	public override bool IsOnCompass()
	{
		return true;
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		return true;
	}

	public override void SetPosition(Vector3 _pos)
	{
		position = _pos;
	}

	public override bool IsCenterOnLeftBottomCorner()
	{
		return true;
	}
}
