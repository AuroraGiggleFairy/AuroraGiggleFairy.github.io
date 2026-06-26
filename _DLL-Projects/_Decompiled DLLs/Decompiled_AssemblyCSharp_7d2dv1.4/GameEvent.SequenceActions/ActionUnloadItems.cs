using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionUnloadItems : ActionBaseItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> ItemStacks = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
	{
		if (!stack.IsEmpty() && (itemTags == "" || stack.itemValue.ItemClass.HasAnyTags(fastItemTags)))
		{
			ItemClass itemClass = stack.itemValue.ItemClass;
			if (itemClass != null && itemClass.Actions[0] is ItemActionAttack itemActionAttack && !itemActionAttack.IsEditingTool())
			{
				int meta = stack.itemValue.Meta;
				string itemName = itemActionAttack.MagazineItemNames[stack.itemValue.SelectedAmmoTypeIndex];
				stack.itemValue.Meta = 0;
				ItemStacks.Add(new ItemStack(ItemClass.GetItem(itemName), meta));
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClientActionEnded(EntityPlayer player)
	{
		base.OnClientActionEnded(player);
		for (int num = ItemStacks.Count - 1; num >= 0; num--)
		{
			ItemStack itemStack = ItemStacks[num];
			if (LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
			{
				ItemStacks.RemoveAt(num);
			}
		}
		if (ItemStacks.Count > 0)
		{
			string text = "DroppedLootContainerTwitch";
			EntityLootContainer entityLootContainer = EntityFactory.CreateEntity(player.entityId, text.GetHashCode(), player.position, Vector3.zero) as EntityLootContainer;
			if (entityLootContainer != null)
			{
				entityLootContainer.SetContent(ItemStack.Clone(ItemStacks));
			}
			GameManager.Instance.World.SpawnEntityInWorld(entityLootContainer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionUnloadItems();
	}
}
