using UnityEngine;

public class AnimatorThrownWeaponHolsterState : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityPlayerLocal componentInParent = animator.GetComponentInParent<EntityPlayerLocal>();
		if (componentInParent != null)
		{
			componentInParent.HolsterWeapon(holster: true);
		}
	}

	public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}
}
