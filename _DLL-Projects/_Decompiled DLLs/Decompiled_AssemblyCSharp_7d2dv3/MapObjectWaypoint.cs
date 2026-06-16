using UnityEngine;

public class MapObjectWaypoint : MapObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public string iconName;

	public Waypoint waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int MapObjectWaypointKeys;

	public MapObjectWaypoint(Waypoint _w)
		: base(EnumMapObjectType.MapMarker, _w.pos.ToVector3(), MapObjectWaypointKeys, null, _bSelectable: false)
	{
		waypoint = _w;
		_w.MapObjectKey = MapObjectWaypointKeys++;
		name = _w.name.Text;
		iconName = _w.icon;
	}

	public override string GetMapIcon()
	{
		return iconName;
	}

	public override string GetCompassIcon()
	{
		return iconName;
	}

	public override bool IsOnCompass()
	{
		return true;
	}

	public override bool IsTracked()
	{
		return waypoint.bTracked;
	}

	public override float GetMaxCompassDistance()
	{
		return waypoint.bTracked ? 1000 : 1000;
	}

	public override float GetMinCompassDistance()
	{
		return waypoint.bTracked ? 250 : 0;
	}

	public override float GetMaxCompassIconScale()
	{
		return base.GetMaxCompassIconScale();
	}

	public override float GetMinCompassIconScale()
	{
		return base.GetMinCompassIconScale();
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

	public override string GetName()
	{
		return waypoint.name.Text;
	}

	public override bool IsShowName()
	{
		return false;
	}
}
