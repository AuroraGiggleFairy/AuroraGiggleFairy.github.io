using UnityEngine;

public class BodyAnimator
{
	public enum EnumState
	{
		Visible,
		OnlyColliders,
		Disabled
	}

	public class BodyParts
	{
		public GameObject BodyObj;

		public Transform RightHandT;

		public BodyParts(Transform _bodyTransform, Transform _rightHand)
		{
			BodyObj = _bodyTransform.gameObject;
			RightHandT = _rightHand;
		}
	}

	public EntityAlive Entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public Animator animator;

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo currentBaseState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarController avatarController;

	[PublicizedFrom(EAccessModifier.Private)]
	public BodyParts bodyParts;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumState state;

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorCullingMode defaultCullingMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRagdoll;

	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer[] skinnedMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer[] meshes;

	public Animator Animator
	{
		get
		{
			if (!bodyParts.BodyObj.activeInHierarchy)
			{
				return null;
			}
			return animator;
		}
		set
		{
			if (bodyParts.BodyObj.activeInHierarchy)
			{
				animator = value;
			}
		}
	}

	public EnumState State
	{
		set
		{
			if (state != value)
			{
				state = value;
				bodyParts.BodyObj.SetActive(state != EnumState.Disabled);
				updateVisibility();
				if (state != EnumState.Disabled)
				{
					animator = bodyParts.BodyObj.GetComponentInChildren<Animator>();
				}
			}
		}
	}

	public BodyParts Parts => bodyParts;

	public bool RagdollActive
	{
		get
		{
			return isRagdoll;
		}
		set
		{
			if (isRagdoll != value)
			{
				isRagdoll = value;
				if ((bool)animator)
				{
					animator.cullingMode = ((!isRagdoll) ? defaultCullingMode : AnimatorCullingMode.AlwaysAnimate);
					animator.enabled = !isRagdoll;
				}
			}
		}
	}

	public virtual void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		if (avatarController != null)
		{
			avatarController.UpdateInt(AvatarController.movementStateHash, _movementState);
			avatarController.UpdateBool(AvatarController.isAliveHash, _value: false);
			avatarController.UpdateBool(AvatarController.isDeadHash, _value: true);
			avatarController.UpdateInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb());
			avatarController.UpdateFloat("HitRandomValue", random);
			avatarController.TriggerEvent("DeathTrigger");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void initBodyAnimator(EntityAlive _entity, BodyParts _bodyParts, EnumState _defaultState)
	{
		Entity = _entity;
		bodyParts = _bodyParts;
		state = _defaultState;
		animator = bodyParts.BodyObj.GetComponentInChildren<Animator>();
		defaultCullingMode = AnimatorCullingMode.AlwaysAnimate;
		meshes = bodyParts.BodyObj.GetComponentsInChildren<MeshRenderer>();
		skinnedMeshes = bodyParts.BodyObj.GetComponentsInChildren<SkinnedMeshRenderer>();
		if (Entity.emodel != null)
		{
			avatarController = Entity.emodel.avatarController;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void cacheLayerStateInfo()
	{
		if ((bool)animator && animator.gameObject.activeInHierarchy)
		{
			currentBaseState = animator.GetCurrentAnimatorStateInfo(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual AnimatorStateInfo getCachedLayerStateInfo(int _layer)
	{
		return currentBaseState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVisibility()
	{
		if (state == EnumState.Disabled)
		{
			return;
		}
		bool enabled = state == EnumState.Visible;
		if (meshes != null)
		{
			MeshRenderer[] array = meshes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = enabled;
			}
		}
		if (skinnedMeshes == null)
		{
			return;
		}
		SkinnedMeshRenderer[] array2 = skinnedMeshes;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array2)
		{
			if ((bool)skinnedMeshRenderer)
			{
				skinnedMeshRenderer.enabled = enabled;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void assignLayerWeights()
	{
	}

	public virtual void SetDrunk(float _numBeers)
	{
	}

	public virtual void Update()
	{
		assignLayerWeights();
		updateVisibility();
	}

	public virtual void LateUpdate()
	{
		cacheLayerStateInfo();
	}
}
