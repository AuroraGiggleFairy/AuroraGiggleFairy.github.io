using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerInventory : NetPackage
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] toolbelt;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] bag;

	[PublicizedFrom(EAccessModifier.Private)]
	public Equipment equipment;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack dragAndDropItem;

	public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

	public NetPackagePlayerInventory Setup(EntityPlayerLocal _player, bool _changedToolbelt, bool _changedBag, bool _changedEquipment, bool _changedDragAndDropItem)
	{
		if (_changedToolbelt)
		{
			toolbelt = ((_player.AttachedToEntity != null && _player.saveInventory != null) ? _player.saveInventory.CloneItemStack() : _player.inventory.CloneItemStack());
		}
		if (_changedBag)
		{
			bag = _player.bag.GetSlots();
		}
		if (_changedEquipment)
		{
			equipment = _player.equipment.Clone();
		}
		if (_changedDragAndDropItem)
		{
			dragAndDropItem = _player.DragAndDropItem.Clone();
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		if (_reader.ReadBoolean())
		{
			toolbelt = GameUtils.ReadItemStack(_reader);
		}
		if (_reader.ReadBoolean())
		{
			bag = GameUtils.ReadItemStack(_reader);
		}
		if (_reader.ReadBoolean())
		{
			ItemValue[] array = GameUtils.ReadItemValueArray(_reader);
			equipment = new Equipment();
			int num = Utils.FastMin(array.Length, equipment.GetSlotCount());
			for (int i = 0; i < num; i++)
			{
				equipment.SetSlotItemRaw(i, array[i]);
			}
			for (int j = 0; j < num; j++)
			{
				equipment.SetCosmeticSlot(j, _reader.ReadInt32());
			}
			int num2 = _reader.ReadInt32();
			for (int k = 0; k < num2; k++)
			{
				equipment.m_unlockedCosmetics.Add(_reader.ReadInt32());
			}
		}
		if (_reader.ReadBoolean())
		{
			ItemStack[] array2 = GameUtils.ReadItemStack(_reader);
			if (array2 != null && array2.Length != 0)
			{
				dragAndDropItem = array2[0];
			}
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(toolbelt != null);
		if (toolbelt != null)
		{
			GameUtils.WriteItemStack(_writer, toolbelt);
		}
		_writer.Write(bag != null);
		if (bag != null)
		{
			GameUtils.WriteItemStack(_writer, bag);
		}
		_writer.Write(equipment != null);
		if (equipment != null)
		{
			GameUtils.WriteItemValueArray(_writer, equipment.GetItems());
			int[] cosmeticIDs = equipment.GetCosmeticIDs();
			for (int i = 0; i < cosmeticIDs.Length; i++)
			{
				_writer.Write(cosmeticIDs[i]);
			}
			List<int> unlockedCosmetics = equipment.m_unlockedCosmetics;
			_writer.Write(unlockedCosmetics.Count);
			for (int j = 0; j < unlockedCosmetics.Count; j++)
			{
				_writer.Write(unlockedCosmetics[j]);
			}
		}
		_writer.Write(dragAndDropItem != null);
		if (dragAndDropItem != null)
		{
			GameUtils.WriteItemStack(_writer, new ItemStack[1] { dragAndDropItem });
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		PlayerDataFile latestPlayerData = base.Sender.latestPlayerData;
		if (toolbelt != null)
		{
			latestPlayerData.inventory = toolbelt;
		}
		if (bag != null)
		{
			latestPlayerData.bag = bag;
		}
		if (equipment != null)
		{
			latestPlayerData.equipment = equipment;
		}
		if (dragAndDropItem != null)
		{
			latestPlayerData.dragAndDropItem = dragAndDropItem;
		}
		latestPlayerData.bModifiedSinceLastSave = true;
	}

	public override int GetLength()
	{
		return 0;
	}
}
