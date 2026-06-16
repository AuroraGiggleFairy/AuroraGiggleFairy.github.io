using UnityEngine;

public class FirstPersonAnimator : BodyAnimator
{
	public FirstPersonAnimator(EntityAlive _entity, AvatarCharacterController.AnimationStates _animStates, Transform _bodyTransform, EnumState _defaultState)
	{
		initBodyAnimator(_entity, new BodyParts(_bodyTransform, _bodyTransform.FindInChilds((_entity.emodel is EModelSDCS) ? "RightWeapon" : "Gunjoint")), _defaultState);
	}

	public override void SetDrunk(float _numBeers)
	{
		Animator animator = base.Animator;
		if ((bool)animator)
		{
			animator.SetFloat("drunk", _numBeers);
		}
	}
}
