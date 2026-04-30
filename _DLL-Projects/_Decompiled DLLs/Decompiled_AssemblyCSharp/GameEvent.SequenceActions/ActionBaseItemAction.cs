using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBaseItemAction : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum ItemLocations
	{
		Toolbelt,
		Backpack,
		Equipment,
		BiomeBadge,
		Held
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum CountTypes
	{
		Items,
		Slots
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<ItemLocations> itemLocations = new List<ItemLocations>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public string itemTags = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public FastTags<TagGroup.Global> fastItemTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Protected)]
	public CountTypes countType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isFinished;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int count = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string countText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemLocation = "items_location";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemTag = "items_tags";

	public static string PropFullCount = "count";

	public static string PropCountType = "count_type";

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityPlayer entityPlayer))
		{
			return;
		}
		OnClientActionStarted(entityPlayer);
		count = GameEventManager.GetIntValue(entityPlayer, countText, -1);
		bool flag = false;
		FastTags<TagGroup.Global>.Parse(itemTags);
		if (itemLocations.Contains(ItemLocations.Toolbelt) && !isFinished)
		{
			ItemStack[] array = ((entityPlayer.AttachedToEntity != null && entityPlayer.saveInventory != null) ? entityPlayer.saveInventory.GetSlots() : entityPlayer.inventory.GetSlots());
			for (int i = 0; i < array.Length; i++)
			{
				if (HandleItemStackChange(ref array[i], entityPlayer))
				{
					flag = true;
				}
				if (isFinished)
				{
					break;
				}
			}
			if (flag)
			{
				entityPlayer.inventory.SetSlots(array);
				entityPlayer.bPlayerStatsChanged = true;
			}
		}
		flag = false;
		if (itemLocations.Contains(ItemLocations.Equipment) || (itemLocations.Contains(ItemLocations.BiomeBadge) && !isFinished))
		{
			int slotCount = entityPlayer.equipment.GetSlotCount();
			int num = 4;
			for (int j = 0; j < slotCount; j++)
			{
				if ((j >= num && !itemLocations.Contains(ItemLocations.BiomeBadge)) || (j < num && !itemLocations.Contains(ItemLocations.Equipment)))
				{
					continue;
				}
				if (CheckEquipmentReplace(entityPlayer.equipment, j))
				{
					ItemValue itemValue = entityPlayer.equipment.GetSlotItemOrNone(j);
					if (HandleItemValueChange(ref itemValue, entityPlayer))
					{
						entityPlayer.equipment.SetSlotItem(j, itemValue);
						flag = true;
					}
				}
				if (isFinished)
				{
					break;
				}
			}
			if (flag)
			{
				entityPlayer.bPlayerStatsChanged = true;
			}
		}
		flag = false;
		if (itemLocations.Contains(ItemLocations.Backpack) && !isFinished)
		{
			ItemStack[] slots = entityPlayer.bag.GetSlots();
			for (int k = 0; k < slots.Length; k++)
			{
				if (HandleItemStackChange(ref slots[k], entityPlayer))
				{
					flag = true;
				}
				if (isFinished)
				{
					break;
				}
			}
			if (flag)
			{
				entityPlayer.bag.SetSlots(slots);
				entityPlayer.bPlayerStatsChanged = true;
			}
		}
		if (itemLocations.Contains(ItemLocations.Backpack) && !isFinished)
		{
			XUiC_DragAndDropWindow dragAndDrop = LocalPlayerUI.GetUIForPrimaryPlayer().xui.dragAndDrop;
			if (!dragAndDrop.CurrentStack.IsEmpty())
			{
				ItemStack stack = dragAndDrop.CurrentStack;
				if (HandleItemStackChange(ref stack, entityPlayer))
				{
					entityPlayer.bPlayerStatsChanged = true;
				}
			}
		}
		if (!itemLocations.Contains(ItemLocations.Toolbelt) && itemLocations.Contains(ItemLocations.Held) && !isFinished)
		{
			Inventory inventory = ((entityPlayer.saveInventory != null) ? entityPlayer.saveInventory : entityPlayer.inventory);
			if (inventory.holdingItem != entityPlayer.inventory.GetBareHandItem())
			{
				ItemStack stack2 = inventory.holdingItemStack;
				if (HandleItemStackChange(ref stack2, entityPlayer))
				{
					inventory.SetItem(inventory.holdingItemIdx, stack2);
					entityPlayer.bPlayerStatsChanged = true;
				}
			}
		}
		OnClientActionEnded(entityPlayer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CheckEquipmentReplace(Equipment equipment, int slot)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnClientActionStarted(EntityPlayer player)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnClientActionEnded(EntityPlayer player)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool HandleItemValueChange(ref ItemValue itemValue, EntityPlayer player)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AddStack(EntityPlayerLocal player, ItemStack stack)
	{
		if (!LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory.AddItem(stack))
		{
			GameManager.Instance.ItemDropServer(stack, player.GetPosition(), Vector3.zero);
		}
	}

	public override BaseAction Clone()
	{
		ActionBaseItemAction obj = (ActionBaseItemAction)base.Clone();
		obj.countText = countText;
		obj.countType = countType;
		obj.itemTags = itemTags;
		obj.fastItemTags = fastItemTags;
		obj.itemLocations = new List<ItemLocations>(itemLocations);
		return obj;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		ItemLocations result = ItemLocations.Toolbelt;
		if (properties.Values.ContainsKey(PropItemLocation))
		{
			string[] array = properties.Values[PropItemLocation].Split(',');
			itemLocations.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				if (Enum.TryParse<ItemLocations>(array[i], ignoreCase: true, out result))
				{
					itemLocations.Add(result);
				}
			}
		}
		properties.ParseString(PropItemTag, ref itemTags);
		fastItemTags = FastTags<TagGroup.Global>.Parse(itemTags);
		properties.ParseString(PropFullCount, ref countText);
		properties.ParseEnum(PropCountType, ref countType);
	}
}
