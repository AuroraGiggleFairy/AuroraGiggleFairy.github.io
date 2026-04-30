using UnityEngine;

public class AnimatorRangedSpecialAttackState : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (!(componentInParent == null))
		{
			componentInParent.emodel.avatarController.UpdateInt("CurrentAnim", 1);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}
}
