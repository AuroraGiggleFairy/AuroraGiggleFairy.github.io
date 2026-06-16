using UnityEngine;

public class MapObjectSupplyDrop : MapObject
{
	public MapObjectSupplyDrop(Vector3 _position, long entityID)
		: base(EnumMapObjectType.SupplyDrop, _position, entityID, null, _bSelectable: false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_airdrop";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_airdrop";
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

	public override bool IsShowName()
	{
		return false;
	}

	public override float GetMaxCompassDistance()
	{
		return 4096f;
	}

	public override Color GetMapIconColor()
	{
		return new Color32(byte.MaxValue, 180, 0, byte.MaxValue);
	}
}
