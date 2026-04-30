using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarMultiBodyController : AvatarController
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<BodyAnimator> bodyAnimators = new List<BodyAnimator>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BodyAnimator primaryBody;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform heldItemTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator heldItemAnimator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool visible = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float animationToDodgeTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeUseAnimationPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeHarestingAnimationPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSpecialAttackPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float timeSpecialAttack2Playing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float idleTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float idleTimeSent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float reviveTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cReviveAnimLength = 4.5f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<int, AnimParamData> FullSyncAnimationParameters = new Dictionary<int, AnimParamData>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool changed;

	public BodyAnimator PrimaryBody
	{
		get
		{
			return primaryBody;
		}
		set
		{
			primaryBody = value;
			SetInRightHand(heldItemTransform);
		}
	}

	public List<BodyAnimator> BodyAnimators => bodyAnimators;

	public Animator HeldItemAnimator => heldItemAnimator;

	public Transform HeldItemTransform => heldItemTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BodyAnimator addBodyAnimator(BodyAnimator _body)
	{
		Animator animator = _body.Animator;
		if ((bool)animator)
		{
			animator.logWarnings = false;
		}
		bodyAnimators.Add(_body);
		SetAnimator(bodyAnimators[0].Animator);
		return _body;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void removeBodyAnimator(BodyAnimator _body)
	{
		bodyAnimators.Remove(_body);
	}

	public override void PlayPlayerFPRevive()
	{
		int count = bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator)
			{
				animator.SetTrigger(AvatarController.reviveHash);
			}
		}
		reviveTime = Time.time;
	}

	public override bool IsAnimationPlayerFPRevivePlaying()
	{
		return Time.time - reviveTime < 4.5f;
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (!(heldItemTransform == null) && !(entity == null) && entity.inventory != null && entity.inventory.holdingItem != null)
		{
			if (_bFPV)
			{
				heldItemTransform.localPosition = Vector3.zero;
				heldItemTransform.localEulerAngles = Vector3.zero;
			}
			else
			{
				heldItemTransform.localPosition = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].position;
				heldItemTransform.localEulerAngles = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].rotation;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTrigger(int _id)
	{
		if (_id == AvatarController.weaponFireHash)
		{
			animationToDodgeTime = 1f;
		}
	}

	public override bool IsAnimationToDodge()
	{
		return animationToDodgeTime > 0f;
	}

	public override bool IsAnimationAttackPlaying()
	{
		return false;
	}

	public override void SetInAir(bool inAir)
	{
		_setBool(AvatarController.inAirHash, inAir);
	}

	public override void StartAnimationAttack()
	{
		_setBool(AvatarController.harvestingHash, _value: false);
		int meta = entity.inventory.holdingItemItemValue.Meta;
		_setInt(AvatarController.weaponAmmoRemaining, meta);
		_setTrigger(AvatarController.weaponFireHash);
	}

	public override bool IsAnimationUsePlaying()
	{
		return timeUseAnimationPlaying > 0f;
	}

	public override void StartAnimationUse()
	{
		_setTrigger(AvatarController.useItemHash);
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return bSpecialAttackPlaying;
	}

	public override void StartAnimationSpecialAttack(bool _b, int _animType)
	{
		idleTime = 0f;
		bSpecialAttackPlaying = _b;
		if (_b)
		{
			_resetTrigger(AvatarController.weaponFireHash);
			_resetTrigger(AvatarController.weaponPreFireCancelHash);
			_setTrigger(AvatarController.weaponPreFireHash);
		}
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return timeSpecialAttack2Playing > 0f;
	}

	public override void StartAnimationSpecialAttack2()
	{
		idleTime = 0f;
		timeSpecialAttack2Playing = 0.3f;
		_resetTrigger(AvatarController.weaponFireHash);
		_resetTrigger(AvatarController.weaponPreFireHash);
		_setTrigger(AvatarController.weaponPreFireCancelHash);
	}

	public override bool IsAnimationHarvestingPlaying()
	{
		return timeHarestingAnimationPlaying > 0f;
	}

	public override void StartAnimationHarvesting(float _length, bool _weaponFireTrigger)
	{
		timeHarestingAnimationPlaying = _length;
		_setBool(AvatarController.harvestingHash, _value: true);
		if (_weaponFireTrigger)
		{
			_setTrigger(AvatarController.weaponFireHash);
		}
	}

	public override void SetDrunk(float _numBeers)
	{
		int count = bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			BodyAnimator bodyAnimator = bodyAnimators[i];
			if ((bool)bodyAnimator.Animator)
			{
				bodyAnimator.SetDrunk(_numBeers);
			}
		}
	}

	public override void SetVehicleAnimation(int _animHash, int _pose)
	{
		int count = bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator)
			{
				animator.SetInteger(_animHash, _pose);
			}
		}
	}

	public override void SetAiming(bool _bEnable)
	{
		idleTime = 0f;
		_setBool(AvatarController.isAimingHash, _bEnable);
	}

	public override void SetCrouching(bool _bEnable)
	{
		idleTime = 0f;
		_setBool(AvatarController.isCrouchingHash, _bEnable);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void avatarVisibilityChanged(BodyAnimator _body, bool _bVisible)
	{
		_body.State = ((!_bVisible) ? BodyAnimator.EnumState.Disabled : BodyAnimator.EnumState.Visible);
	}

	public override void SetVisible(bool _b)
	{
		if (visible == _b)
		{
			return;
		}
		int count = bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			BodyAnimator body = bodyAnimators[i];
			avatarVisibilityChanged(body, _b);
		}
		Transform holdingItemTransform = entity.inventory.GetHoldingItemTransform();
		if (holdingItemTransform != null)
		{
			MeshRenderer[] componentsInChildren = holdingItemTransform.GetComponentsInChildren<MeshRenderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = true;
			}
		}
		visible = _b;
	}

	public override void SetRagdollEnabled(bool _b)
	{
		int count = bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			bodyAnimators[i].RagdollActive = _b;
		}
	}

	public override void StartAnimationReloading()
	{
		idleTime = 0f;
		float value = EffectManager.GetValue(PassiveEffects.ReloadSpeedMultiplier, entity.inventory.holdingItemItemValue, 1f, entity);
		_ = bodyAnimators.Count;
		bool value2 = entity as EntityPlayerLocal != null && (entity as EntityPlayerLocal).emodel.IsFPV;
		_setBool(AvatarController.isFPVHash, value2);
		_setBool(AvatarController.reloadHash, _value: true);
		_setFloat(AvatarController.reloadSpeedHash, value);
	}

	public override void StartAnimationJump(AnimJumpMode jumpMode)
	{
		idleTime = 0f;
		switch (jumpMode)
		{
		case AnimJumpMode.Start:
			_setTrigger(AvatarController.jumpTriggerHash);
			_setBool(AvatarController.inAirHash, _value: true);
			break;
		case AnimJumpMode.Land:
			_setTrigger(AvatarController.jumpLandHash);
			_setInt(AvatarController.jumpLandResponseHash, 0);
			_setBool(AvatarController.inAirHash, _value: false);
			break;
		}
	}

	public override void SetSwim(bool _enable)
	{
		int walkType = -1;
		if (!_enable)
		{
			walkType = entity.GetWalkType();
		}
		SetWalkType(walkType, _trigger: true);
	}

	public override void StartAnimationFiring()
	{
		StartAnimationAttack();
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		idleTime = 0f;
		_setInt(AvatarController.movementStateHash, _movementState);
		_setInt(AvatarController.hitDirectionHash, _dir);
		_setInt(AvatarController.hitDamageHash, _hitDamage);
		_setInt(AvatarController.hitBodyPartHash, (int)_bodyPart);
		_setFloat(AvatarController.hitRandomValueHash, _random);
		_setBool(AvatarController.isCriticalHash, _criticalHit);
		_setTrigger(AvatarController.hitTriggerHash);
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		idleTime = 0f;
		int count = bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			bodyAnimators[i].StartDeathAnimation(_bodyPart, _movementState, random);
		}
	}

	public override void SetInRightHand(Transform _transform)
	{
		idleTime = 0f;
		if (_transform != null)
		{
			Quaternion localRotation = ((heldItemTransform != null) ? heldItemTransform.localRotation : Quaternion.identity);
			_transform.SetParent(GetRightHandTransform(), worldPositionStays: false);
			if ((!entity.emodel.IsFPV || entity.isEntityRemote) && entity.inventory != null && entity.inventory.holdingItem != null)
			{
				AnimationGunjointOffsetData.AnimationGunjointOffsets animationGunjointOffsets = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value];
				_transform.localPosition = animationGunjointOffsets.position;
				_transform.localRotation = Quaternion.Euler(animationGunjointOffsets.rotation);
			}
			else
			{
				_transform.localPosition = Vector3.zero;
				_transform.localRotation = localRotation;
			}
		}
		heldItemTransform = _transform;
		heldItemAnimator = ((_transform != null) ? _transform.GetComponent<Animator>() : null);
		if (heldItemAnimator != null)
		{
			heldItemAnimator.logWarnings = false;
			heldItemAnimator.runtimeAnimatorController = (entity.emodel.IsFPV ? GameManager.Instance.FirstPersonWeaponAnimatorController : GameManager.Instance.ThirdPersonWeaponAnimatorController);
		}
	}

	public override Transform GetRightHandTransform()
	{
		if (primaryBody == null)
		{
			return null;
		}
		return primaryBody.Parts.RightHandT;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		float deltaTime = Time.deltaTime;
		if (animationToDodgeTime > 0f)
		{
			animationToDodgeTime -= deltaTime;
		}
		if (timeUseAnimationPlaying > 0f)
		{
			timeUseAnimationPlaying -= deltaTime;
		}
		if (timeHarestingAnimationPlaying > 0f)
		{
			timeHarestingAnimationPlaying -= deltaTime;
		}
		if (timeSpecialAttack2Playing > 0f)
		{
			timeSpecialAttack2Playing -= deltaTime;
		}
		if (!IsAnimationUsePlaying())
		{
			int value = entity.inventory.holdingItem.HoldType.Value;
			_setInt(AvatarController.weaponHoldTypeHash, value);
		}
		float speedForward = entity.speedForward;
		float speedStrafe = entity.speedStrafe;
		float x = entity.rotation.x;
		bool flag = entity.IsDead();
		bool flag2 = IsMoving(speedForward, speedStrafe);
		if (flag2)
		{
			idleTime = 0f;
		}
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			bodyAnimators[i].Update();
		}
		float num = speedStrafe;
		if (num >= 1234f)
		{
			num = 0f;
		}
		_setFloat(AvatarController.forwardHash, speedForward, false);
		_setFloat(AvatarController.strafeHash, num, false);
		_setBool(AvatarController.isMovingHash, flag2, _netsync: false);
		_setFloat(AvatarController.rotationPitchHash, x, false);
		if (!flag)
		{
			if (speedStrafe >= 1234f)
			{
				_setInt(AvatarController.movementStateHash, 4);
			}
			else
			{
				float num2 = speedForward * speedForward + speedStrafe * speedStrafe;
				_setInt(AvatarController.movementStateHash, (num2 > base.Entity.moveSpeedAggro * base.Entity.moveSpeedAggro) ? 3 : ((num2 > base.Entity.moveSpeed * base.Entity.moveSpeed) ? 2 : ((num2 > 0.001f) ? 1 : 0)));
			}
		}
		float num3 = idleTime - idleTimeSent;
		if (num3 * num3 > 0.25f)
		{
			idleTimeSent = idleTime;
			_setFloat(AvatarController.idleTimeHash, idleTime, false);
		}
		idleTime += deltaTime;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (base.Entity.inventory.holdingItem.Actions[0] != null)
		{
			base.Entity.inventory.holdingItem.Actions[0].UpdateNozzleParticlesPosAndRot(base.Entity.inventory.holdingItemData.actionData[0]);
		}
		if (base.Entity.inventory.holdingItem.Actions[1] != null)
		{
			base.Entity.inventory.holdingItem.Actions[1].UpdateNozzleParticlesPosAndRot(base.Entity.inventory.holdingItemData.actionData[1]);
		}
	}

	public override void NotifyAnimatorMove(Animator anim)
	{
	}

	public override Animator GetAnimator()
	{
		return primaryBody.Animator;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool animatorIsValid(Animator animator)
	{
		if ((bool)animator && animator.enabled)
		{
			return animator.gameObject.activeInHierarchy;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setTrigger(int _propertyHash, bool _netsync = true)
	{
		changed = false;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if (animatorIsValid(animator) && !animator.GetBool(_propertyHash))
			{
				animator.SetTrigger(_propertyHash);
				changed = true;
			}
		}
		if (animatorIsValid(heldItemAnimator) && !heldItemAnimator.GetBool(_propertyHash))
		{
			heldItemAnimator.SetTrigger(_propertyHash);
			changed = true;
		}
		if (!entity.isEntityRemote && changed && _netsync)
		{
			changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Trigger, _value: true));
		}
		if (changed)
		{
			OnTrigger(_propertyHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _resetTrigger(int _propertyHash, bool _netsync = true)
	{
		changed = false;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator && animator.GetBool(_propertyHash))
			{
				animator.ResetTrigger(_propertyHash);
				changed = true;
			}
		}
		if ((bool)heldItemAnimator && heldItemAnimator.gameObject.activeInHierarchy && heldItemAnimator.GetBool(_propertyHash))
		{
			heldItemAnimator.ResetTrigger(_propertyHash);
			changed = true;
		}
		if (!entity.isEntityRemote && changed && _netsync)
		{
			changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Trigger, _value: false));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setFloat(int _propertyHash, float _value, bool _netsync = true)
	{
		changed = false;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator)
			{
				float num = animator.GetFloat(_propertyHash) - _value;
				if (num * num > 1.0000001E-06f)
				{
					animator.SetFloat(_propertyHash, _value);
					changed = true;
				}
			}
		}
		if ((bool)heldItemAnimator && heldItemAnimator.gameObject.activeInHierarchy && heldItemAnimator.GetFloat(_propertyHash) != _value)
		{
			heldItemAnimator.SetFloat(_propertyHash, _value);
			changed = true;
		}
		if (!entity.isEntityRemote && changed && _netsync)
		{
			changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Float, _value));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setBool(int _propertyHash, bool _value, bool _netsync = true)
	{
		changed = false;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator && animator.GetBool(_propertyHash) != _value)
			{
				animator.SetBool(_propertyHash, _value);
				changed = true;
			}
		}
		if ((bool)heldItemAnimator && heldItemAnimator.gameObject.activeInHierarchy && heldItemAnimator.GetBool(_propertyHash) != _value)
		{
			heldItemAnimator.SetBool(_propertyHash, _value);
			changed = true;
		}
		if (!entity.isEntityRemote && changed && _propertyHash != AvatarController.isFPVHash && _netsync)
		{
			changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Bool, _value));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		changed = false;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator && animator.GetInteger(_propertyHash) != _value)
			{
				animator.SetInteger(_propertyHash, _value);
				changed = true;
			}
		}
		if ((bool)heldItemAnimator && heldItemAnimator.gameObject.activeInHierarchy && heldItemAnimator.GetInteger(_propertyHash) != _value)
		{
			heldItemAnimator.SetInteger(_propertyHash, _value);
			changed = true;
		}
		if (!entity.isEntityRemote && changed && _netsync)
		{
			changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Int, _value));
		}
	}

	public override bool TryGetTrigger(int _propertyHash, out bool _value)
	{
		_value = false;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator)
			{
				_value |= animator.GetBool(_propertyHash);
				if (_value)
				{
					return true;
				}
			}
		}
		if ((bool)heldItemAnimator && heldItemAnimator.gameObject.activeInHierarchy)
		{
			_value |= heldItemAnimator.GetBool(_propertyHash);
		}
		return true;
	}

	public override bool TryGetFloat(int _propertyHash, out float _value)
	{
		_value = float.NaN;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator)
			{
				_value = animator.GetFloat(_propertyHash);
				if (_value != float.NaN)
				{
					return true;
				}
			}
		}
		if ((bool)heldItemAnimator && heldItemAnimator.gameObject.activeInHierarchy)
		{
			_value = heldItemAnimator.GetFloat(_propertyHash);
		}
		return _value != float.NaN;
	}

	public override bool TryGetBool(int _propertyHash, out bool _value)
	{
		_value = false;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator)
			{
				_value |= animator.GetBool(_propertyHash);
				if (_value)
				{
					return true;
				}
			}
		}
		if ((bool)heldItemAnimator && heldItemAnimator.gameObject.activeInHierarchy)
		{
			_value |= heldItemAnimator.GetBool(_propertyHash);
		}
		return true;
	}

	public override bool TryGetInt(int _propertyHash, out int _value)
	{
		_value = int.MinValue;
		for (int i = 0; i < bodyAnimators.Count; i++)
		{
			Animator animator = bodyAnimators[i].Animator;
			if ((bool)animator)
			{
				_value = animator.GetInteger(_propertyHash);
				if (_value != int.MinValue)
				{
					return true;
				}
			}
		}
		if ((bool)heldItemAnimator && heldItemAnimator.gameObject.activeInHierarchy)
		{
			_value = heldItemAnimator.GetInteger(_propertyHash);
		}
		return _value != int.MinValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarMultiBodyController()
	{
	}
}
