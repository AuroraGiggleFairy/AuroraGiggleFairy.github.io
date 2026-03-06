using UnityEngine;

public class AnimationStateRagdoll : StateMachineBehaviour
{
	public DynamicRagdollFlags RagdollFlags = DynamicRagdollFlags.Active | DynamicRagdollFlags.RagdollOnFall | DynamicRagdollFlags.UseBoneVelocities;

	[Tooltip("Time period to stun")]
	public FloatRange StunTime = new FloatRange(1f, 1f);

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent != null)
		{
			componentInParent.BeginDynamicRagdoll(RagdollFlags, StunTime);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent != null)
		{
			componentInParent.ActivateDynamicRagdoll();
		}
	}
}
