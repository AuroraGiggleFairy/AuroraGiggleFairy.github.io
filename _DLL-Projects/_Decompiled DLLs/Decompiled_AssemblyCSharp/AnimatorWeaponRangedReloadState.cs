using System;
using Audio;
using UnityEngine;

public class AnimatorWeaponRangedReloadState : StateMachineBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionRanged.ItemActionDataRanged actionData;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool("Reload", value: false);
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		actionData = ((componentInParent != null) ? (componentInParent.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged) : null);
		actionData.isWeaponReloading = true;
		actionData.wasWeaponReloadCancelled = false;
		if (actionData.invData.item.Properties.Values[ItemClass.PropSoundIdle] != null)
		{
			Manager.Stop(actionData.invData.holdingEntity.entityId, actionData.invData.item.Properties.Values[ItemClass.PropSoundIdle]);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (actionData != null && actionData.isWeaponReloadCancelled && !actionData.wasWeaponReloadCancelled)
		{
			actionData.wasWeaponReloadCancelled = true;
			animator.Play(0, -1, 1f);
			animator.Update(0f);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		actionData.isWeaponReloading = false;
		actionData.isWeaponReloadCancelled = false;
		animator.speed = 1f;
	}

	public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (actionData != null)
		{
			actionData.isWeaponReloading = false;
			actionData.isWeaponReloadCancelled = false;
		}
	}
}
