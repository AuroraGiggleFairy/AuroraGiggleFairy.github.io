using System;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class RewardLootItem : BaseReward
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int LootGameStage = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack item;

	public static string PropLootTier = "loot_tier";

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
		LootGameStage = Convert.ToInt32(base.Value);
		ItemClass itemClass = Item.itemValue.ItemClass;
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
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (!playerInventory.AddItem(Item))
		{
			playerInventory.DropItem(Item);
		}
	}

	public override ItemStack GetRewardItem()
	{
		return item.Clone();
	}

	public void SetupItem()
	{
		string[] array = base.ID.Split(',');
		int num = 10;
		if (!string.IsNullOrEmpty(base.Value))
		{
			LootGameStage = StringParsers.ParseSInt32(base.Value);
		}
		while (num > 0)
		{
			if (array.Length > 1)
			{
				World world = GameManager.Instance.World;
				item = LootContainer.GetRewardItem(array[world.GetGameRandom().RandomRange(array.Length)], LootGameStage);
			}
			else if (array.Length == 1)
			{
				item = LootContainer.GetRewardItem(base.ID, LootGameStage);
			}
			bool flag = false;
			for (int i = 0; i < base.OwnerQuest.Rewards.Count; i++)
			{
				if (base.OwnerQuest.Rewards[i] is RewardLootItem rewardLootItem)
				{
					if (rewardLootItem == this)
					{
						flag = true;
						break;
					}
					if (rewardLootItem.Item.itemValue.type == item.itemValue.type)
					{
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
			num--;
		}
		item.itemValue.UseTimes = 0f;
	}

	public override BaseReward Clone()
	{
		RewardLootItem rewardLootItem = new RewardLootItem();
		CopyValues(rewardLootItem);
		rewardLootItem.LootGameStage = LootGameStage;
		if (item != null)
		{
			rewardLootItem.item = item.Clone();
		}
		return rewardLootItem;
	}

	public override string GetRewardText()
	{
		string localizedItemName = Item.itemValue.ItemClass.GetLocalizedItemName();
		if (Item.itemValue.HasQuality)
		{
			return localizedItemName;
		}
		if (Item.count <= 1)
		{
			return localizedItemName;
		}
		return $"{localizedItemName} ({Item.count})";
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropLootTier))
		{
			base.Value = properties.Values[PropLootTier];
		}
	}
}
