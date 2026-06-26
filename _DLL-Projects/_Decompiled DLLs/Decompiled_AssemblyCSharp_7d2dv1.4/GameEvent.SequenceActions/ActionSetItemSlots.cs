using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionSetItemSlots : ActionBaseClientAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum ItemLocations
	{
		Toolbelt,
		Backpack,
		Equipment
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemLocations ItemLocation;

	public string[] Items;

	public string[] ItemCounts;

	public string[] SlotNumbers;

	public static string PropItemLocation = "items_location";

	public static string PropItems = "items";

	public static string PropItemCounts = "item_counts";

	public static string PropSlotNumbers = "slot_numbers";

	public override void OnClientPerform(Entity target)
	{
		if (Items == null || (ItemLocation != ItemLocations.Equipment && SlotNumbers == null))
		{
			return;
		}
		XUiM_PlayerEquipment xUiM_PlayerEquipment = null;
		if (!(target is EntityPlayerLocal entityPlayerLocal))
		{
			return;
		}
		for (int i = 0; i < Items.Length; i++)
		{
			string value = ((ItemCounts != null && ItemCounts.Length > i) ? ItemCounts[i] : "1");
			int num = ((SlotNumbers != null && SlotNumbers.Length > i) ? StringParsers.ParseSInt32(SlotNumbers[i]) : (-1));
			if (num == -1 && ItemLocation != ItemLocations.Equipment)
			{
				break;
			}
			int num2 = 1;
			ItemClass itemClass = ItemClass.GetItemClass(Items[i]);
			ItemValue itemValue = null;
			num2 = GameEventManager.GetIntValue(entityPlayerLocal, value, 1);
			if (itemClass.HasQuality)
			{
				itemValue = new ItemValue(itemClass.Id, num2, num2);
				num2 = 1;
			}
			else
			{
				itemValue = new ItemValue(itemClass.Id);
			}
			if (itemValue.ItemClass.Actions[0] is ItemActionRanged itemActionRanged)
			{
				itemValue.Meta = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, itemActionRanged.BulletsPerMagazine, entityPlayerLocal);
			}
			ItemStack itemStack = new ItemStack(itemValue, num2);
			switch (ItemLocation)
			{
			case ItemLocations.Toolbelt:
				entityPlayerLocal.inventory.SetItem(num, itemStack);
				break;
			case ItemLocations.Backpack:
				entityPlayerLocal.bag.SetSlot(num, itemStack);
				break;
			case ItemLocations.Equipment:
				if (xUiM_PlayerEquipment == null)
				{
					xUiM_PlayerEquipment = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal).xui.PlayerEquipment;
				}
				xUiM_PlayerEquipment.EquipItem(itemStack);
				break;
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropItemLocation, ref ItemLocation);
		if (properties.Values.ContainsKey(PropItems))
		{
			Items = properties.Values[PropItems].Replace(" ", "").Split(',');
			if (properties.Values.ContainsKey(PropItemCounts))
			{
				ItemCounts = properties.Values[PropItemCounts].Replace(" ", "").Split(',');
			}
			else
			{
				ItemCounts = null;
			}
			if (properties.Values.ContainsKey(PropSlotNumbers))
			{
				SlotNumbers = properties.Values[PropSlotNumbers].Replace(" ", "").Split(',');
				return;
			}
			SlotNumbers = null;
			if (ItemLocation != ItemLocations.Equipment)
			{
				Items = null;
				ItemCounts = null;
			}
		}
		else
		{
			Items = null;
			SlotNumbers = null;
			ItemCounts = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionSetItemSlots
		{
			ItemLocation = ItemLocation,
			Items = Items,
			ItemCounts = ItemCounts,
			SlotNumbers = SlotNumbers,
			targetGroup = targetGroup
		};
	}
}
