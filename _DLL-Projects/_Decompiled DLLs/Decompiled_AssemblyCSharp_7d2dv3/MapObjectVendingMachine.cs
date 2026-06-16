using UnityEngine;

public class MapObjectVendingMachine : MapObject
{
	public MapObjectVendingMachine(Vector3 _position, Entity _entity)
		: base(EnumMapObjectType.VendingMachine, _position, _entity.entityId, _entity, _bSelectable: false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_vending";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_vending";
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

	public override Color GetMapIconColor()
	{
		return Color.white;
	}
}
