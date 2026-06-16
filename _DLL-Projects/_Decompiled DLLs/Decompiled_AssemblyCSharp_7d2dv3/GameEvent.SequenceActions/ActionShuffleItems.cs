using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionShuffleItems : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum ItemLocations
	{
		Toolbelt,
		Backpack
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<ItemLocations> itemLocations = new List<ItemLocations>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemLocation = "items_location";

	public override void OnClientPerform(Entity target)
	{
		if (target is EntityPlayer player)
		{
			GameManager.Instance.StartCoroutine(handleShuffle(player));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator handleShuffle(EntityPlayer player)
	{
		while (player.inventory.IsHoldingItemActionRunning() || !player.inventory.GetItem(player.inventory.DUMMY_SLOT_IDX).IsEmpty())
		{
			yield return new WaitForSeconds(0.25f);
		}
		List<ItemStack> list = new List<ItemStack>();
		if (itemLocations.Contains(ItemLocations.Toolbelt))
		{
			ItemStack[] collection = ((player.AttachedToEntity != null && player.saveInventory != null) ? player.saveInventory.GetSlots() : player.inventory.GetSlots());
			list.AddRange(collection);
			list.RemoveAt(list.Count - 1);
		}
		if (itemLocations.Contains(ItemLocations.Backpack))
		{
			ItemStack[] slots = player.bag.GetSlots();
			list.AddRange(slots);
		}
		GameRandom random = GameEventManager.Current.Random;
		for (int i = 0; i < list.Count * 2; i++)
		{
			int num = random.RandomRange(list.Count);
			int num2 = random.RandomRange(list.Count);
			ItemStack value = list[num];
			list[num] = list[num2];
			list[num2] = value;
		}
		int num3 = 0;
		if (itemLocations.Contains(ItemLocations.Toolbelt))
		{
			Inventory inventory = ((player.saveInventory != null) ? player.saveInventory : player.inventory);
			ItemStack[] slots2 = inventory.GetSlots();
			for (int j = 0; j < slots2.Length - 1; j++)
			{
				slots2[j] = list[num3++];
			}
			inventory.SetSlots(slots2);
			player.bPlayerStatsChanged = true;
		}
		if (itemLocations.Contains(ItemLocations.Backpack))
		{
			ItemStack[] slots3 = player.bag.GetSlots();
			for (int k = 0; k < slots3.Length; k++)
			{
				slots3[k] = list[num3++];
			}
			player.bag.SetSlots(slots3);
			player.bPlayerStatsChanged = true;
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		ItemLocations result = ItemLocations.Toolbelt;
		if (!properties.Values.ContainsKey(PropItemLocation))
		{
			return;
		}
		string[] array = properties.Values[PropItemLocation].Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (Enum.TryParse<ItemLocations>(array[i], ignoreCase: true, out result))
			{
				itemLocations.Add(result);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionShuffleItems
		{
			itemLocations = itemLocations,
			targetGroup = targetGroup
		};
	}
}
