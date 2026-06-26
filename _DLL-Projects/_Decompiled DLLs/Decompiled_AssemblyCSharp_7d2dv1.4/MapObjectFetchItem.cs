using UnityEngine;

public class MapObjectFetchItem : MapObject
{
	public bool IsSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int newID;

	public MapObjectFetchItem(Vector3 _position)
		: base(EnumMapObjectType.FetchItem, _position, ++newID, null, _bSelectable: false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_fetch_loot";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_fetch_loot";
	}

	public override string GetCompassDownIcon()
	{
		return "ui_game_symbol_fetch_loot_down";
	}

	public override string GetCompassUpIcon()
	{
		return "ui_game_symbol_fetch_loot_up";
	}

	public override bool UseUpDownCompassIcons()
	{
		return true;
	}

	public override bool IsOnCompass()
	{
		return true;
	}

	public override bool IsCompassIconClamped()
	{
		return true;
	}

	public override bool NearbyCompassBlink()
	{
		return true;
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		return false;
	}

	public override Color GetMapIconColor()
	{
		if (IsSelected)
		{
			return new Color32(byte.MaxValue, 180, 0, byte.MaxValue);
		}
		return Color.white;
	}
}
