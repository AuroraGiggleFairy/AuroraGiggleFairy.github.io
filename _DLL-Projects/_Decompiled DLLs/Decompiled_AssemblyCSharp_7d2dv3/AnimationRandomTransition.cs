using UnityEngine;

public class AnimationRandomTransition : StateMachineBehaviour
{
	public string animationParameter = "RandomIndex";

	public int numberOfAnimations;

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (numberOfAnimations > 0)
		{
			int value = Random.Range(0, numberOfAnimations);
			animator.SetInteger(animationParameter, value);
		}
	}
}
