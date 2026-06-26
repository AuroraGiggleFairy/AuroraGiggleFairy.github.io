using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionReplaceItems : ActionBaseItemAction
{
	public string ReplacedByItem = "";

	public static string PropReplacedByItem = "replaced_by_item";

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> replaceItemTag = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClientActionStarted(EntityPlayer player)
	{
		replaceItemTag = FastTags<TagGroup.Global>.Parse(itemTags);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CheckEquipmentReplace(Equipment equipment, int slot)
	{
		ItemValue item = ItemClass.GetItem(ReplacedByItem);
		return equipment.PreferredItemSlot(item) == slot;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
	{
		if (!stack.IsEmpty() && stack.itemValue.ItemClass.HasAnyTags(replaceItemTag) && stack.itemValue.ItemClass.GetItemName() != ReplacedByItem)
		{
			if (count != -1)
			{
				if (countType == CountTypes.Items)
				{
					if (stack.count <= count)
					{
						count -= stack.count;
						stack = new ItemStack(ItemClass.GetItem(ReplacedByItem), stack.count);
					}
					else
					{
						stack.count -= count;
						ItemStack stack2 = new ItemStack(ItemClass.GetItem(ReplacedByItem), count);
						AddStack(player as EntityPlayerLocal, stack2);
						count = 0;
						isFinished = true;
					}
				}
				else
				{
					stack = new ItemStack(ItemClass.GetItem(ReplacedByItem), stack.count);
					count--;
					if (count == 0)
					{
						isFinished = true;
					}
				}
				return true;
			}
			stack = new ItemStack(ItemClass.GetItem(ReplacedByItem), stack.count);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleItemValueChange(ref ItemValue itemValue, EntityPlayer player)
	{
		if (!itemValue.IsEmpty() && itemValue.ItemClass.HasAnyTags(replaceItemTag) && itemValue.ItemClass.GetItemName() != ReplacedByItem)
		{
			itemValue = ItemClass.GetItem(ReplacedByItem).Clone();
			if (count != -1)
			{
				count--;
				if (count == 0)
				{
					isFinished = true;
				}
			}
			return true;
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropReplacedByItem))
		{
			ReplacedByItem = properties.Values[PropReplacedByItem];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionReplaceItems
		{
			ReplacedByItem = ReplacedByItem
		};
	}
}
