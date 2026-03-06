using System.Collections.Generic;
using UnityEngine;

public class UMACharacterBodyAnimator : BodyAnimator
{
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo currentOverrideLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo twoHandedLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo painLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> deathStates;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> hitStates;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInDeathAnim;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool didDeathTransition;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cYLookUpdateSpeed = 12f;

	public UMACharacterBodyAnimator(EntityAlive _entity, AvatarCharacterController.AnimationStates _animStates, Transform _bodyTransform, EnumState _defaultState)
	{
		initBodyAnimator(_entity, new BodyParts(_bodyTransform, _bodyTransform.FindInChilds((_entity.emodel is EModelSDCS) ? "RightWeapon" : "Gunjoint")), _defaultState);
		deathStates = _animStates.DeathStates;
		hitStates = _animStates.HitStates;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void assignLayerWeights()
	{
		base.assignLayerWeights();
		Animator animator = base.Animator;
		if (!animator)
		{
			return;
		}
		if (Entity.IsDead())
		{
			animator.SetLayerWeight(1, 0f);
			animator.SetLayerWeight(2, 0f);
			animator.SetLayerWeight(3, 0f);
			return;
		}
		animator.SetLayerWeight(3, 1f);
		if (animator.GetInteger(AvatarController.vehiclePoseHash) >= 0)
		{
			animator.SetLayerWeight(1, 0f);
			animator.SetLayerWeight(2, 0f);
		}
		else if (!animator.IsInTransition(1) && AnimationDelayData.AnimationDelay[Entity.inventory.holdingItem.HoldType.Value].TwoHanded)
		{
			animator.SetLayerWeight(1, 0f);
			animator.SetLayerWeight(2, 1f);
		}
		else if (!animator.IsInTransition(2))
		{
			animator.SetLayerWeight(1, 1f);
			animator.SetLayerWeight(2, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void cacheLayerStateInfo()
	{
		base.cacheLayerStateInfo();
		Animator animator = base.Animator;
		if ((bool)animator)
		{
			currentOverrideLayer = animator.GetCurrentAnimatorStateInfo(1);
			twoHandedLayer = animator.GetCurrentAnimatorStateInfo(2);
			painLayer = animator.GetCurrentAnimatorStateInfo(4);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override AnimatorStateInfo getCachedLayerStateInfo(int _layer)
	{
		return _layer switch
		{
			1 => currentOverrideLayer, 
			2 => twoHandedLayer, 
			_ => base.getCachedLayerStateInfo(_layer), 
		};
	}

	public override void SetDrunk(float _numBeers)
	{
		if ((bool)base.Animator && Entity.AttachedToEntity == null && avatarController != null)
		{
			avatarController.UpdateFloat("drunk", _numBeers);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateSpineRotation()
	{
		if (!base.RagdollActive && !Entity.IsDead() && (bool)base.Animator && Entity.AttachedToEntity == null && avatarController != null)
		{
			avatarController.TryGetFloat(AvatarController.yLookHash, out var _value);
			if (Entity is EntityPlayerLocal entityPlayerLocal && !entityPlayerLocal.vp_FPCamera.Locked3rdPerson && entityPlayerLocal.IsCameraFacingCharacter())
			{
				avatarController.UpdateFloat(AvatarController.yLookHash, Mathf.Lerp(_value, 0f, Time.deltaTime * 12f), _netsync: false);
			}
			else
			{
				avatarController.UpdateFloat(AvatarController.yLookHash, Mathf.Lerp(_value, (0f - Entity.rotation.x) / 90f, Time.deltaTime * 12f), _netsync: false);
			}
		}
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		base.StartDeathAnimation(_bodyPart, _movementState, random);
		isInDeathAnim = true;
		didDeathTransition = false;
	}

	public override void Update()
	{
		base.Update();
		updateSpineRotation();
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		Animator animator = base.Animator;
		if (!animator)
		{
			return;
		}
		AnimatorStateInfo cachedLayerStateInfo = getCachedLayerStateInfo(0);
		if (isInDeathAnim)
		{
			if (deathStates.Contains(cachedLayerStateInfo.fullPathHash) && !animator.IsInTransition(0))
			{
				didDeathTransition = true;
			}
			if (didDeathTransition && (cachedLayerStateInfo.normalizedTime >= 1f || animator.IsInTransition(0)))
			{
				isInDeathAnim = false;
				if (Entity.HasDeathAnim)
				{
					Entity.emodel.DoRagdoll(DamageResponse.New(_fatal: true));
				}
			}
		}
		if (animator.GetInteger(AvatarController.hitBodyPartHash) != 0 && avatarController != null && !animator.IsInTransition(4) && hitStates.Contains(painLayer.fullPathHash))
		{
			avatarController.UpdateInt(AvatarController.hitBodyPartHash, 0);
		}
	}
}
