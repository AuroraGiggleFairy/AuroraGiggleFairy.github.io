using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySpawnResponse : NetPackage
{
	public bool success;

	public ItemValue itemValue;

	public NetPackageEntitySpawnResponse Setup(bool _success, ItemValue _itemValue)
	{
		success = _success;
		itemValue = _itemValue;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		success = _reader.ReadBoolean();
		itemValue = new ItemValue();
		itemValue.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(success);
		itemValue.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
		bool flag = itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("vehicle"));
		bool flag2 = itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("drone"));
		bool flag3 = itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretRanged")) || itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretMelee"));
		if (success)
		{
			if (flag)
			{
				if (primaryPlayer.inventory.holdingItem.Equals(itemValue.ItemClass) && primaryPlayer.inventory.holdingItem.Actions[1] is ItemActionSpawnVehicle itemActionSpawnVehicle)
				{
					itemActionSpawnVehicle.ClearPreview(primaryPlayer.inventory.holdingItemData.actionData[1]);
				}
				primaryPlayer.inventory.DecItem(itemValue, 1);
				primaryPlayer.PlayOneShot("placeblock");
			}
			else
			{
				if (primaryPlayer.inventory.holdingItem.Equals(itemValue.ItemClass) && primaryPlayer.inventory.holdingItem.Actions[1] is ItemActionSpawnTurret itemActionSpawnTurret)
				{
					itemActionSpawnTurret.ClearPreview(primaryPlayer.inventory.holdingItemData.actionData[1]);
				}
				primaryPlayer.inventory.DecItem(itemValue, 1);
				primaryPlayer.PlayOneShot("placeblock");
			}
		}
		else if (flag)
		{
			GameManager.ShowTooltip(primaryPlayer, "uiCannotAddVehicle");
		}
		else if (flag2)
		{
			GameManager.ShowTooltip(primaryPlayer, "uiCannotAddDrone");
		}
		else if (flag3)
		{
			GameManager.ShowTooltip(primaryPlayer, "uiCannotAddTurret");
		}
	}

	public override int GetLength()
	{
		return 20;
	}
}
