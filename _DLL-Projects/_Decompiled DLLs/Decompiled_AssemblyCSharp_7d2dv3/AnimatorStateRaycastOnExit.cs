using UnityEngine;

[SharedBetweenAnimators]
public class AnimatorStateRaycastOnExit : StateMachineBehaviour
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.ResetTrigger("WeaponFire");
		animator.ResetTrigger("PowerAttack");
	}
}
