using UnityEngine;

[SharedBetweenAnimators]
public class AnimatorStateRaycast : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent != null && !componentInParent.isEntityRemote)
		{
			int integer = animator.GetInteger(AvatarController.itemActionIndexHash);
			if (integer >= 0 && integer < componentInParent.inventory.holdingItem.Actions.Length && componentInParent.inventory.holdingItemData.actionData[integer] is ItemActionDynamicMelee.ItemActionDynamicMeleeData)
			{
				(componentInParent.inventory.holdingItem.Actions[integer] as ItemActionDynamicMelee).Raycast(componentInParent.inventory.holdingItemData.actionData[integer] as ItemActionDynamic.ItemActionDynamicData);
			}
		}
	}
}
