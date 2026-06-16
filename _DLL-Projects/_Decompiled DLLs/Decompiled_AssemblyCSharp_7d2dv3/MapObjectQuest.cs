using UnityEngine;

public class MapObjectQuest : MapObject
{
	public bool IsSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public string icon;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int newID;

	public MapObjectQuest(Vector3 _position, string newIcon = "ui_game_symbol_quest")
		: base(EnumMapObjectType.Quest, _position, ++newID, null, _bSelectable: false)
	{
		icon = newIcon;
	}

	public override string GetMapIcon()
	{
		return icon;
	}

	public override string GetCompassIcon()
	{
		return icon;
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
		return true;
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
