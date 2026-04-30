using System;
using Audio;
using UnityEngine;

public class AnimatorRangedHoldState : StateMachineBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionRanged.ItemActionDataRanged actionData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.emodel.avatarController.UpdateInt("CurrentAnim", 0);
		actionData = componentInParent.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged;
		if (actionData == null)
		{
			return;
		}
		itemClass = actionData.invData.itemValue.ItemClass;
		if (itemClass != null && itemClass.Properties.Values.ContainsKey(ItemClass.PropSoundIdle))
		{
			if (actionData.invData.itemValue.Meta > 0)
			{
				Manager.Play(actionData.invData.holdingEntity, itemClass.Properties.Values[ItemClass.PropSoundIdle]);
				actionData.invData.holdingEntitySoundID = 0;
			}
			else
			{
				Manager.Stop(actionData.invData.holdingEntity.entityId, itemClass.Properties.Values[ItemClass.PropSoundIdle]);
				actionData.invData.holdingEntitySoundID = -1;
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (itemClass != null && itemClass.Properties.Values.ContainsKey(ItemClass.PropSoundIdle))
		{
			if (actionData.invData.holdingEntitySoundID != -1 && actionData.invData.itemValue.Meta == 0)
			{
				Manager.Stop(actionData.invData.holdingEntity.entityId, itemClass.Properties.Values[ItemClass.PropSoundIdle]);
				actionData.invData.holdingEntitySoundID = -1;
			}
			else if (actionData.invData.holdingEntitySoundID == -1 && actionData.invData.itemValue.Meta > 0)
			{
				Manager.Play(actionData.invData.holdingEntity, itemClass.Properties.Values[ItemClass.PropSoundIdle]);
				actionData.invData.holdingEntitySoundID = 0;
			}
		}
	}
}
