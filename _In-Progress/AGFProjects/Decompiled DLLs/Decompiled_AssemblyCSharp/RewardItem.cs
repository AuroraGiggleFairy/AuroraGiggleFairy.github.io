using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class RewardItem : BaseReward
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack item;

	public ItemStack Item
	{
		get
		{
			if (item == null || item.IsEmpty())
			{
				SetupItem();
			}
			return item;
		}
	}

	public override void SetupReward()
	{
		ItemClass itemClass = ItemClass.GetItemClass(base.ID);
		base.Description = itemClass.GetLocalizedItemName();
		base.ValueText = base.Value;
		switch (itemClass.Groups[0].ToLower())
		{
		case "basics":
			base.Icon = "ui_game_symbol_shopping_cart";
			break;
		case "books":
			base.Icon = "ui_game_symbol_book";
			break;
		case "building":
			base.Icon = "ui_game_symbol_map_house";
			break;
		case "resources":
			base.Icon = "ui_game_symbol_resource";
			break;
		case "ammo/weapons":
			base.Icon = "ui_game_symbol_knife";
			break;
		case "decor/miscellaneous":
			base.Icon = "ui_game_symbol_chair";
			break;
		case "tools/traps":
			base.Icon = "ui_game_symbol_tool";
			break;
		case "food/cooking":
			base.Icon = "ui_game_symbol_fork";
			break;
		case "science":
			base.Icon = "ui_game_symbol_science";
			break;
		case "clothing":
			base.Icon = "ui_game_symbol_shirt";
			break;
		case "chemicals":
			base.Icon = "ui_game_symbol_water";
			break;
		case "mods":
			base.Icon = "ui_game_symbol_assemble";
			break;
		case "special items":
			base.Icon = "ui_game_symbol_book";
			break;
		}
		base.IconAtlas = "ItemIconAtlas";
	}

	public override void Read(BinaryReader _br)
	{
		base.Read(_br);
		item = new ItemStack();
		item.Read(_br);
	}

	public override void Write(BinaryWriter _bw)
	{
		base.Write(_bw);
		if (item == null)
		{
			item = ItemStack.Empty.Clone();
		}
		item.Write(_bw);
	}

	public override void GiveReward(EntityPlayer player)
	{
		item = GetRewardItem();
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (!playerInventory.AddItem(Item))
		{
			playerInventory.DropItem(Item);
		}
	}

	public override ItemStack GetRewardItem()
	{
		ItemStack itemStack = Item.Clone();
		if (!itemStack.itemValue.ItemClass.HasAllTags(FastTags<TagGroup.Global>.Parse("dukes")))
		{
			return itemStack;
		}
		int count = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.QuestBonusItemReward, null, itemStack.count, base.OwnerQuest.OwnerJournal.OwnerPlayer));
		itemStack.count = count;
		return itemStack;
	}

	public void SetupItem()
	{
		ItemValue itemValue = ItemClass.GetItem(base.ID);
		ItemValue itemValue2 = new ItemValue(ItemClass.GetItem(base.ID).type, _bCreateDefaultParts: true);
		int result = 1;
		if (base.Value != null && base.Value != "")
		{
			if (int.TryParse(base.Value, out result))
			{
				if (itemValue2.HasQuality)
				{
					itemValue2 = new ItemValue(itemValue.type, result, result, _bCreateDefaultModItems: true);
					result = 1;
				}
				else
				{
					itemValue2 = new ItemValue(itemValue.type, _bCreateDefaultParts: true);
				}
			}
			else if (base.Value.Contains("-"))
			{
				string[] array = base.Value.Split('-');
				int num = Convert.ToInt32(array[0]);
				int num2 = Convert.ToInt32(array[1]);
				if (itemValue2.HasQuality)
				{
					itemValue2 = new ItemValue(itemValue.type, num, num2, _bCreateDefaultModItems: true);
					result = 1;
				}
				else
				{
					itemValue2 = new ItemValue(itemValue.type, _bCreateDefaultParts: true);
					result = GameManager.Instance.World.GetGameRandom().RandomRange(num, num2);
				}
			}
		}
		item = new ItemStack(itemValue2, result);
	}

	public override BaseReward Clone()
	{
		RewardItem rewardItem = new RewardItem();
		CopyValues(rewardItem);
		if (item != null)
		{
			rewardItem.item = item.Clone();
		}
		return rewardItem;
	}

	public override string GetRewardText()
	{
		return Item.count + " x " + Item.itemValue.ItemClass.GetLocalizedItemName();
	}
}
