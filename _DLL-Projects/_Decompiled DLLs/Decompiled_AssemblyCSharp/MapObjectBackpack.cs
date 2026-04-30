using UnityEngine;

public class MapObjectBackpack : MapObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal owningLocalPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity myBackpack;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeBackpackChecked;

	public MapObjectBackpack(EntityPlayerLocal _epl, Vector3 _position, int _key)
		: base(EnumMapObjectType.Backpack, _position, _key, null, _bSelectable: false)
	{
		owningLocalPlayer = _epl;
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_backpack";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_backpack";
	}

	public override Color GetMapIconColor()
	{
		return Color.cyan;
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

	public override Vector3 GetPosition()
	{
		if (myBackpack == null && Time.time - lastTimeBackpackChecked > 5f)
		{
			lastTimeBackpackChecked = Time.time;
			World world = GameManager.Instance.World;
			for (int num = world.Entities.list.Count - 1; num >= 0; num--)
			{
				if (world.Entities.list[num] is EntityBackpack && ((EntityBackpack)world.Entities.list[num]).RefPlayerId == owningLocalPlayer.entityId)
				{
					myBackpack = world.Entities.list[num];
					break;
				}
			}
		}
		if (myBackpack != null && myBackpack.IsMarkedForUnload())
		{
			myBackpack = null;
		}
		if (!(myBackpack != null))
		{
			return base.GetPosition();
		}
		return myBackpack.position;
	}
}
