using UnityEngine;

public class AnimatorStateHolstering : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent != null && componentInParent.emodel != null && componentInParent.emodel.avatarController != null)
		{
			componentInParent.emodel.avatarController.CancelEvent("WeaponFire");
			componentInParent.emodel.avatarController.CancelEvent("PowerAttack");
			componentInParent.emodel.avatarController.CancelEvent("UseItem");
			componentInParent.emodel.avatarController.UpdateBool("ItemUse", _value: false);
			componentInParent.emodel.avatarController.CancelEvent("Reload");
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}
}
