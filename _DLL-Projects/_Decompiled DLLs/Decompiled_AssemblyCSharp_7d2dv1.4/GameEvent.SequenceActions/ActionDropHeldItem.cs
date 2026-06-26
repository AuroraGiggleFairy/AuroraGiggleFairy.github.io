using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionDropHeldItem : ActionBaseItemAction
{
	public string DropSound = "";

	public static string PropDropSound = "drop_sound";

	public override bool CanPerform(Entity target)
	{
		if (target is EntityPlayer entityPlayer)
		{
			if (((entityPlayer.saveInventory != null) ? entityPlayer.saveInventory : entityPlayer.inventory).holdingItemStack == ItemStack.Empty)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override void OnClientPerform(Entity target)
	{
		EntityAlive entityAlive = target as EntityAlive;
		if (entityAlive != null)
		{
			Inventory inventory = ((entityAlive.saveInventory != null) ? entityAlive.saveInventory : entityAlive.inventory);
			if (inventory.holdingItem != entityAlive.inventory.GetBareHandItem())
			{
				Vector3 dropPosition = entityAlive.GetDropPosition();
				ItemValue holdingItemItemValue = inventory.holdingItemItemValue;
				int num = inventory.holdingItemStack.count;
				GameManager.Instance.DropContentInLootContainerServer(entityAlive.entityId, "DroppedLootContainerTwitch", dropPosition, new ItemStack[1] { inventory.holdingItemStack.Clone() });
				entityAlive.AddUIHarvestingItem(new ItemStack(holdingItemItemValue, -num));
				Manager.BroadcastPlay(entityAlive, DropSound);
				inventory.DecHoldingItem(num);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropDropSound, ref DropSound);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionDropHeldItem
		{
			targetGroup = targetGroup,
			DropSound = DropSound
		};
	}
}
