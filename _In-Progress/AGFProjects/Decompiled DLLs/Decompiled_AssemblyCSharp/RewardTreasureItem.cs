using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardTreasureItem : BaseReward
{
	public override void SetupReward()
	{
		base.Description = ItemClass.GetItemClass(base.ID).Name;
		base.ValueText = base.Value;
		base.Icon = "ui_game_symbol_hand";
		base.IconAtlas = "ItemIconAtlas";
	}

	public override void GiveReward(EntityPlayer player)
	{
		if (base.OwnerQuest == null)
		{
			return;
		}
		ItemValue item = ItemClass.GetItem(base.ID);
		ItemStack.Empty.Clone();
		ItemValue itemValue = new ItemValue(item.type, _bCreateDefaultParts: true);
		int result = 1;
		if (base.Value != null && base.Value != "")
		{
			if (int.TryParse(base.Value, out result))
			{
				if (itemValue.HasQuality)
				{
					itemValue = new ItemValue(item.type, result, result, _bCreateDefaultModItems: true);
					result = 1;
				}
				else
				{
					itemValue = new ItemValue(item.type, _bCreateDefaultParts: true);
				}
			}
			else if (base.Value.Contains("-"))
			{
				string[] array = base.Value.Split('-');
				int num = Convert.ToInt32(array[0]);
				int num2 = Convert.ToInt32(array[1]);
				if (itemValue.HasQuality)
				{
					itemValue = new ItemValue(item.type, num, num2, _bCreateDefaultModItems: true);
					result = 1;
				}
				else
				{
					World world = GameManager.Instance.World;
					itemValue = new ItemValue(item.type, _bCreateDefaultParts: true);
					result = world.GetGameRandom().RandomRange(num, num2);
				}
			}
		}
		string[] array2 = base.OwnerQuest.DataVariables["treasurecontainer"].Split(',');
		Vector3i zero = Vector3i.zero;
		if (array2.Length == 3)
		{
			zero = new Vector3i(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]), Convert.ToInt32(array2[2]));
			((TileEntityLootContainer)GameManager.Instance.World.GetTileEntity(0, zero)).AddItem(new ItemStack(itemValue, result));
		}
	}

	public override BaseReward Clone()
	{
		RewardTreasureItem rewardTreasureItem = new RewardTreasureItem();
		CopyValues(rewardTreasureItem);
		return rewardTreasureItem;
	}
}
