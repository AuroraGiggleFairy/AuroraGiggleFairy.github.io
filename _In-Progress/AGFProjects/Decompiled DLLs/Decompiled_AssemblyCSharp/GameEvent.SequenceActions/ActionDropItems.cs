using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionDropItems : ActionBaseItemAction
{
	public string ReplacedByItem = "";

	public string DropSound = "";

	public static string PropReplacedByItem = "replaced_by_item";

	public static string PropDropSound = "drop_sound";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> droppedItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> replaceItemTag = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClientActionStarted(EntityPlayer player)
	{
		droppedItems = new List<ItemStack>();
		replaceItemTag = ((itemTags == "") ? FastTags<TagGroup.Global>.none : FastTags<TagGroup.Global>.Parse(itemTags));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClientActionEnded(EntityPlayer player)
	{
		if (droppedItems.Count > 0)
		{
			Vector3 dropPosition = player.GetDropPosition();
			GameManager.Instance.DropContentInLootContainerServer(player.entityId, "DroppedLootContainerTwitch", dropPosition, droppedItems.ToArray());
			if (DropSound != "")
			{
				Manager.BroadcastPlay(player, DropSound);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
	{
		if (!stack.IsEmpty() && (replaceItemTag.IsEmpty || stack.itemValue.ItemClass.HasAnyTags(replaceItemTag)) && stack.itemValue.ItemClass.GetItemName() != ReplacedByItem)
		{
			if (count != -1)
			{
				if (countType == CountTypes.Slots)
				{
					droppedItems.Add(stack.Clone());
					if (ReplacedByItem == "")
					{
						stack = ItemStack.Empty.Clone();
					}
					else
					{
						stack = new ItemStack(ItemClass.GetItem(ReplacedByItem), stack.count);
					}
					count--;
					if (count == 0)
					{
						isFinished = true;
					}
					return true;
				}
				if (stack.count > count)
				{
					ItemStack itemStack = stack.Clone();
					itemStack.count = count;
					droppedItems.Add(itemStack);
					stack.count -= count;
					if (ReplacedByItem != "")
					{
						ItemStack stack2 = new ItemStack(ItemClass.GetItem(ReplacedByItem), count);
						AddStack(player as EntityPlayerLocal, stack2);
					}
					count = 0;
					isFinished = true;
				}
				else
				{
					count -= stack.count;
					droppedItems.Add(stack.Clone());
					if (ReplacedByItem == "")
					{
						stack = ItemStack.Empty.Clone();
					}
					else
					{
						stack = new ItemStack(ItemClass.GetItem(ReplacedByItem), stack.count);
					}
				}
			}
			else
			{
				droppedItems.Add(stack.Clone());
				if (ReplacedByItem == "")
				{
					stack = ItemStack.Empty.Clone();
				}
				else
				{
					stack = new ItemStack(ItemClass.GetItem(ReplacedByItem), stack.count);
				}
			}
			return true;
		}
		return false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropReplacedByItem, ref ReplacedByItem);
		properties.ParseString(PropDropSound, ref DropSound);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionDropItems
		{
			ReplacedByItem = ReplacedByItem,
			DropSound = DropSound
		};
	}
}
