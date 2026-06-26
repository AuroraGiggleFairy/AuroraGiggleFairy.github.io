using UnityEngine;

public class AnimationStateRandomBlend : StateMachineBehaviour
{
	[Tooltip("The number of options to randomly select from")]
	public int ChoiceCount = 1;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int integer = animator.GetInteger("RandomSelector");
		if (ChoiceCount > 0)
		{
			animator.SetFloat("RandomVariationQuantized", integer % ChoiceCount);
		}
		else
		{
			animator.SetFloat("RandomVariationQuantized", 0f);
		}
	}
}
