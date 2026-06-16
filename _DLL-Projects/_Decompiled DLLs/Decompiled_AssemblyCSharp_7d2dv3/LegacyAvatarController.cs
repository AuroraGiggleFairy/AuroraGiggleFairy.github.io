using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class LegacyAvatarController : AvatarController
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPitchUpdateSpeed = 12f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int jumpState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int fpvJumpState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isJumpStarted;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<int> reloadStates = new HashSet<int>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<int> deathStates = new HashSet<int>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_bVisible;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bFPV;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bMale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo baseStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo currentWeaponHoldLayer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo currentUpperBodyState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float lastAbsMotionX;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float lastAbsMotionZ;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float lastAbsMotion;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform bipedTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform modelTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform rightHand;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform pelvis;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform spine;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform spine1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform spine2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform spine3;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform head;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform cameraNode;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeAttackAnimationPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isAttackImpact;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeUseAnimationPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeHarestingAnimationPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform rightHandItemTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Animator rightHandAnimator;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bIsRagdoll;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool useIdle = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float idleTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float idleTimeSent;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int itemUseTicks;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int movementStateOverride = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string modelName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isInDeathAnim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool didDeathTransition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSpecialAttackPlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeSpecialAttack2Playing;

	public Transform HeldItemTransform => rightHandItemTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if (entity is EntityPlayerLocal)
		{
			modelTransform = base.transform.Find("Camera");
		}
		else
		{
			modelTransform = EModelBase.FindModel(base.transform);
		}
		assignStates();
	}

	public override Transform GetActiveModelRoot()
	{
		return modelTransform;
	}

	public override void SetInRightHand(Transform _transform)
	{
		idleTime = 0f;
		if (_transform != null)
		{
			_transform.SetParent(rightHand, worldPositionStays: false);
		}
		rightHandItemTransform = _transform;
		rightHandAnimator = ((_transform != null) ? _transform.GetComponent<Animator>() : null);
		if (rightHandAnimator != null)
		{
			rightHandAnimator.logWarnings = false;
			rightHandAnimator.runtimeAnimatorController = GameManager.Instance.ThirdPersonWeaponAnimatorController;
		}
		if (rightHandItemTransform != null)
		{
			Utils.SetLayerRecursively(rightHandItemTransform.gameObject, 0);
		}
	}

	public override Transform GetRightHandTransform()
	{
		return rightHandItemTransform;
	}

	public override bool IsAnimationAttackPlaying()
	{
		return timeAttackAnimationPlaying > 0f;
	}

	public override void StartAnimationAttack()
	{
		idleTime = 0f;
		isAttackImpact = false;
		timeAttackAnimationPlaying = 0.3f;
		if (!(bipedTransform == null) && bipedTransform.gameObject.activeInHierarchy)
		{
			_setTrigger(AvatarController.weaponFireHash);
		}
	}

	public override void SetAttackImpact()
	{
		if (!isAttackImpact)
		{
			isAttackImpact = true;
			timeAttackAnimationPlaying = 0.1f;
		}
	}

	public override bool IsAttackImpact()
	{
		return isAttackImpact;
	}

	public override bool IsAnimationUsePlaying()
	{
		return timeUseAnimationPlaying > 0f;
	}

	public override void StartAnimationUse()
	{
		idleTime = 0f;
		itemUseTicks = 3;
		timeUseAnimationPlaying = 0.3f;
		if (!(bipedTransform == null) && bipedTransform.gameObject.activeInHierarchy)
		{
			_setBool(AvatarController.itemUseHash, _value: true);
		}
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
		return timeSpecialAttack2Playing > 0.3f;
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

	public override void StartAnimationFiring()
	{
		idleTime = 0f;
		timeAttackAnimationPlaying = 0.3f;
		if (!(bipedTransform == null) && bipedTransform.gameObject.activeInHierarchy)
		{
			_setTrigger(AvatarController.weaponFireHash);
		}
	}

	public override void StartAnimationReloading()
	{
		idleTime = 0f;
		if (!(bipedTransform == null) && bipedTransform.gameObject.activeInHierarchy)
		{
			float value = EffectManager.GetValue(PassiveEffects.ReloadSpeedMultiplier, entity.inventory.holdingItemItemValue, 1f, entity);
			_setBool(AvatarController.reloadHash, _value: true);
			_setFloat(AvatarController.reloadSpeedHash, value);
		}
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random, float _duration)
	{
		InternalStartAnimationHit(_bodyPart, _dir, _hitDamage, _criticalHit, _movementState, random, _duration);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void InternalStartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random, float _duration)
	{
		if (bipedTransform == null || !bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!CheckHit(_duration))
		{
			SetDataFloat(DataTypes.HitDuration, _duration);
			return;
		}
		idleTime = 0f;
		if (anim != null)
		{
			movementStateOverride = _movementState;
			_setInt(AvatarController.movementStateHash, _movementState);
			_setBool(AvatarController.isCriticalHash, _criticalHit);
			_setInt(AvatarController.hitDirectionHash, _dir);
			_setInt(AvatarController.hitDamageHash, _hitDamage);
			_setFloat(AvatarController.hitRandomValueHash, random);
			_setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb());
			_setTrigger(AvatarController.hitTriggerHash);
			SetDataFloat(DataTypes.HitDuration, _duration);
		}
	}

	public override void SetVisible(bool _b)
	{
		m_bVisible = _b;
		Transform meshTransform = GetMeshTransform();
		if (meshTransform != null && meshTransform.gameObject.activeSelf != _b)
		{
			meshTransform.gameObject.SetActive(_b);
			if (_b)
			{
				SwitchModelAndView(modelName, bFPV, bMale);
			}
		}
	}

	public override void SetVehicleAnimation(int _animHash, int _pose)
	{
		if ((bool)anim)
		{
			_setInt(_animHash, _pose);
		}
	}

	public override void SetAiming(bool _bEnable)
	{
		idleTime = 0f;
		if (!(bipedTransform == null) && bipedTransform.gameObject.activeInHierarchy && anim != null)
		{
			_setBool(AvatarController.isAimingHash, _bEnable);
		}
	}

	public override void SetCrouching(bool _bEnable)
	{
		idleTime = 0f;
		if (!(bipedTransform == null) && bipedTransform.gameObject.activeInHierarchy && anim != null)
		{
			_setBool(AvatarController.isCrouchingHash, _bEnable);
		}
	}

	public override bool IsAnimationDigRunning()
	{
		return AvatarController.digHash == baseStateInfo.tagHash;
	}

	public override void StartAnimationJumping()
	{
		idleTime = 0f;
		if (!(bipedTransform == null) && bipedTransform.gameObject.activeInHierarchy && anim != null)
		{
			_setBool(AvatarController.jumpHash, _value: true);
		}
	}

	public override void StartAnimationJump(AnimJumpMode jumpMode)
	{
		idleTime = 0f;
		if (bipedTransform == null || !bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		isJumpStarted = true;
		if (anim != null)
		{
			if (jumpMode == AnimJumpMode.Start)
			{
				_setTrigger(AvatarController.jumpStartHash);
				_setBool(AvatarController.inAirHash, _value: true);
			}
			else
			{
				_setTrigger(AvatarController.jumpLandHash);
				_setInt(AvatarController.jumpLandResponseHash, 0);
				_setBool(AvatarController.inAirHash, _value: false);
			}
		}
	}

	public override bool IsAnimationJumpRunning()
	{
		if (isJumpStarted)
		{
			return true;
		}
		if (AvatarController.jumpHash == baseStateInfo.tagHash)
		{
			return true;
		}
		return false;
	}

	public override bool IsAnimationWithMotionRunning()
	{
		int tagHash = baseStateInfo.tagHash;
		if (tagHash == AvatarController.jumpHash || tagHash == AvatarController.moveHash)
		{
			return true;
		}
		return false;
	}

	public override void SetSwim(bool _enable)
	{
		int walkType = -1;
		if (!_enable)
		{
			walkType = entity.GetWalkType();
		}
		else
		{
			_setFloat(AvatarController.swimSelectHash, entity.rand.RandomFloat);
		}
		SetWalkType(walkType, _trigger: true);
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		idleTime = 0f;
		isInDeathAnim = true;
		didDeathTransition = false;
		if (!(bipedTransform == null) && bipedTransform.gameObject.activeInHierarchy && anim != null)
		{
			movementStateOverride = _movementState;
			_setInt(AvatarController.movementStateHash, _movementState);
			_setBool(AvatarController.isAliveHash, _value: false);
			_setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb());
			_setFloat(AvatarController.hitRandomValueHash, random);
			SetFallAndGround(_canFall: false, entity.onGround);
		}
	}

	public override void SetRagdollEnabled(bool _b)
	{
		bIsRagdoll = _b;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateLayerStateInfo()
	{
		if (anim != null)
		{
			baseStateInfo = anim.GetCurrentAnimatorStateInfo(0);
			if (anim.layerCount > 1)
			{
				currentWeaponHoldLayer = anim.GetCurrentAnimatorStateInfo(1);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateSpineRotation()
	{
		if (!(modelTransform.parent != null) || bIsRagdoll || entity.IsDead())
		{
			return;
		}
		if (bFPV)
		{
			spine3.transform.localEulerAngles = new Vector3(spine1.transform.localEulerAngles.x, spine1.transform.localEulerAngles.y, spine1.transform.localEulerAngles.z + 1f * entity.rotation.x);
			return;
		}
		float num = 1f * entity.rotation.x / 3f;
		if (Time.timeScale > 0.001f)
		{
			spine1.transform.localEulerAngles = new Vector3(spine1.transform.localEulerAngles.x, spine1.transform.localEulerAngles.y, spine1.transform.localEulerAngles.z + num);
			spine2.transform.localEulerAngles = new Vector3(spine2.transform.localEulerAngles.x, spine2.transform.localEulerAngles.y, spine2.transform.localEulerAngles.z + num);
			spine3.transform.localEulerAngles = new Vector3(spine3.transform.localEulerAngles.x, spine3.transform.localEulerAngles.y, spine3.transform.localEulerAngles.z + num);
		}
		else
		{
			spine1.transform.localEulerAngles = new Vector3(spine1.transform.localEulerAngles.x, spine1.transform.localEulerAngles.y, num);
			spine2.transform.localEulerAngles = new Vector3(spine2.transform.localEulerAngles.x, spine2.transform.localEulerAngles.y, num);
			spine3.transform.localEulerAngles = new Vector3(spine3.transform.localEulerAngles.x, spine3.transform.localEulerAngles.y, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (entity == null || bipedTransform == null || !bipedTransform.gameObject.activeInHierarchy || anim == null || !anim.enabled)
		{
			return;
		}
		updateLayerStateInfo();
		updateSpineRotation();
		if (entity.inventory.holdingItem.Actions[0] != null)
		{
			entity.inventory.holdingItem.Actions[0].UpdateNozzleParticlesPosAndRot(entity.inventory.holdingItemData.actionData[0]);
		}
		if (entity.inventory.holdingItem.Actions[1] != null)
		{
			entity.inventory.holdingItem.Actions[1].UpdateNozzleParticlesPosAndRot(entity.inventory.holdingItemData.actionData[1]);
		}
		int fullPathHash = baseStateInfo.fullPathHash;
		bool flag = anim.IsInTransition(0);
		if (!flag)
		{
			isJumpStarted = false;
			if (fullPathHash == jumpState || fullPathHash == fpvJumpState)
			{
				_setBool(AvatarController.jumpHash, _value: false);
			}
			if (anim.GetBool(AvatarController.reloadHash) && reloadStates.Contains(currentWeaponHoldLayer.fullPathHash))
			{
				_setBool(AvatarController.reloadHash, _value: false);
			}
		}
		if (anim.GetBool(AvatarController.itemUseHash) && --itemUseTicks <= 0)
		{
			_setBool(AvatarController.itemUseHash, _value: false);
		}
		if (!isInDeathAnim)
		{
			return;
		}
		if ((baseStateInfo.tagHash == AvatarController.deathHash || deathStates.Contains(fullPathHash)) && baseStateInfo.normalizedTime >= 1f && !flag)
		{
			isInDeathAnim = false;
			if (entity.HasDeathAnim)
			{
				entity.emodel.DoRagdoll(DamageResponse.New(_fatal: true));
			}
		}
		if (entity.HasDeathAnim && entity.RootMotion && entity.isCollidedHorizontally)
		{
			isInDeathAnim = false;
			entity.emodel.DoRagdoll(DamageResponse.New(_fatal: true));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setLayerWeights()
	{
		if (anim == null || anim.layerCount <= 1)
		{
			return;
		}
		if (entity.IsDead())
		{
			anim.SetLayerWeight(1, 1f);
			if (!bFPV)
			{
				anim.SetLayerWeight(2, 1f);
			}
			return;
		}
		if (entity.inventory.holdingItem.HoldType == 0)
		{
			anim.SetLayerWeight(1, 0f);
		}
		else
		{
			anim.SetLayerWeight(1, 1f);
		}
		if (!bFPV)
		{
			anim.SetLayerWeight(2, 1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (timeAttackAnimationPlaying > 0f)
		{
			timeAttackAnimationPlaying -= Time.deltaTime;
			if (timeAttackAnimationPlaying <= 0f)
			{
				isAttackImpact = true;
			}
		}
		if (timeUseAnimationPlaying > 0f)
		{
			timeUseAnimationPlaying -= Time.deltaTime;
		}
		if (timeHarestingAnimationPlaying > 0f)
		{
			timeHarestingAnimationPlaying -= Time.deltaTime;
			if (timeHarestingAnimationPlaying <= 0f && anim != null)
			{
				_setBool(AvatarController.harvestingHash, _value: false);
			}
		}
		if (timeSpecialAttack2Playing > 0f)
		{
			timeSpecialAttack2Playing -= Time.deltaTime;
		}
		if ((!m_bVisible && (!entity || !entity.RootMotion || entity.isEntityRemote)) || bipedTransform == null || !bipedTransform.gameObject.activeInHierarchy || anim == null || !anim.avatar.isValid || !anim.enabled)
		{
			return;
		}
		updateLayerStateInfo();
		setLayerWeights();
		int value = entity.inventory.holdingItem.HoldType.Value;
		_setInt(AvatarController.weaponHoldTypeHash, value);
		float speedForward = entity.speedForward;
		float speedStrafe = entity.speedStrafe;
		float num = speedStrafe;
		if (num >= 1234f)
		{
			num = 0f;
		}
		_setFloat(AvatarController.forwardHash, speedForward, false);
		_setFloat(AvatarController.strafeHash, num, false);
		if (!entity.IsDead())
		{
			if (movementStateOverride != -1)
			{
				_setInt(AvatarController.movementStateHash, movementStateOverride);
				movementStateOverride = -1;
			}
			else if (speedStrafe >= 1234f)
			{
				_setInt(AvatarController.movementStateHash, 4);
			}
			else
			{
				float num2 = speedForward * speedForward + num * num;
				_setInt(AvatarController.movementStateHash, (num2 > entity.moveSpeedAggro * entity.moveSpeedAggro) ? 3 : ((num2 > entity.moveSpeed * entity.moveSpeed) ? 2 : ((num2 > 0.001f) ? 1 : 0)));
			}
		}
		if (IsMoving(speedForward, speedStrafe))
		{
			idleTime = 0f;
			_setBool(AvatarController.isMovingHash, _value: true);
		}
		else
		{
			_setBool(AvatarController.isMovingHash, _value: false);
		}
		if (useIdle)
		{
			float num3 = idleTime - idleTimeSent;
			if (num3 * num3 > 0.25f)
			{
				idleTimeSent = idleTime;
				_setFloat(AvatarController.idleTimeHash, idleTime, false);
			}
			idleTime += Time.deltaTime;
		}
		TryGetFloat(AvatarController.rotationPitchHash, out var _value);
		float value2 = Mathf.Lerp(_value, entity.rotation.x, Time.deltaTime * 12f);
		_setFloat(AvatarController.rotationPitchHash, value2, false);
		TryGetFloat(AvatarController.yLookHash, out var _value2);
		UpdateFloat(AvatarController.yLookHash, Mathf.Lerp(_value2, (0f - base.Entity.rotation.x) / 90f, Time.deltaTime * 12f), _netsync: false);
	}

	public virtual Transform GetMeshTransform()
	{
		return bipedTransform;
	}

	public override void SetArchetypeStance(NPCInfo.StanceTypes stance)
	{
		if (anim != null && anim.avatar.isValid && anim.enabled)
		{
			_setInt(AvatarController.archetypeStanceHash, (int)stance);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setTrigger(int _propertyHash, bool _netsync = true)
	{
		base._setTrigger(_propertyHash, _netsync);
		if (rightHandAnimator != null && rightHandAnimator.runtimeAnimatorController != null)
		{
			rightHandAnimator.SetTrigger(_propertyHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _resetTrigger(int _propertyHash, bool _netsync = true)
	{
		base._resetTrigger(_propertyHash, _netsync);
		if (rightHandAnimator != null && rightHandAnimator.runtimeAnimatorController != null && rightHandAnimator.GetBool(_propertyHash))
		{
			rightHandAnimator.ResetTrigger(_propertyHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setFloat(int _propertyHash, float _value, bool _netsync = true)
	{
		base._setFloat(_propertyHash, _value, _netsync);
		if (rightHandAnimator != null && rightHandAnimator.runtimeAnimatorController != null)
		{
			rightHandAnimator.SetFloat(_propertyHash, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setBool(int _propertyHash, bool _value, bool _netsync = true)
	{
		base._setBool(_propertyHash, _value, _netsync);
		if (rightHandAnimator != null && rightHandAnimator.runtimeAnimatorController != null)
		{
			rightHandAnimator.SetBool(_propertyHash, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		base._setInt(_propertyHash, _value, _netsync);
		if (rightHandAnimator != null && rightHandAnimator.runtimeAnimatorController != null)
		{
			rightHandAnimator.SetInteger(_propertyHash, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public LegacyAvatarController()
	{
	}
}
