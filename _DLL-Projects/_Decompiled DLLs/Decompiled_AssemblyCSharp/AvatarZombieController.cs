using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.DuckType.Jiggle;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarZombieController : AvatarController
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cOverrideLayerIndex = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cFullBodyLayerIndex = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cHitLayerIndex = 3;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AvatarRootMotion rootMotion;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform modelT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform bipedT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform rightHandT;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo baseStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo overrideStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo fullBodyStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo hitStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isSuppressPain;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isCrippled;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isCrawler;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isVisibleInit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isVisible;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float idleTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float crawlerTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float actionTimeActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackPlayingTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isAttackImpact;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeSpecialAttack2Playing;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeRagePlaying;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int jumpState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isJumpStarted;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEating;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int movementStateOverride = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool headDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool leftUpperArmDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool leftLowerArmDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool rightUpperArmDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool rightLowerArmDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool leftUpperLegDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool leftLowerLegDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool rightUpperLegDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool rightLowerLegDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isInDeathAnim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool didDeathTransition;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material mainZombieMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material mainZombieMaterialCopy;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material gibCapMaterial;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material gibCapMaterialCopy;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material dismemberMat;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer skinnedMeshRenderer;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer smrLODOne;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer smrLODTwo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string rootDismmemberDir;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string subFolderDismemberEntityName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int altEntityMatId = -1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public string altMatName;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<DismemberedPart> dismemberedParts = new List<DismemberedPart>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCensored;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cEmissiveColor = "_EmissiveColor";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 defaultHeadPos;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const string cElectrocuteKeyword = "_ELECTRIC_SHOCK_ON";

	public bool IsCrippled => isCrippled;

	public bool rightArmDismembered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!rightUpperArmDismembered)
			{
				return rightLowerArmDismembered;
			}
			return true;
		}
	}

	public bool leftArmDismembered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!leftUpperArmDismembered)
			{
				return leftLowerArmDismembered;
			}
			return true;
		}
	}

	public bool IsCrawler => isCrawler;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		modelT = EModelBase.FindModel(base.transform);
		assignStates();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		hitLayerIndex = 3;
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (!bipedT)
		{
			bipedT = entity.emodel.GetModelTransform();
			rightHandT = FindTransform(entity.GetRightHandTransformName());
			SetAnimator(bipedT);
			if (entity.RootMotion)
			{
				rootMotion = bipedT.gameObject.AddComponent<AvatarRootMotion>();
				rootMotion.Init(this, anim);
			}
		}
		SetWalkType(entity.GetWalkType());
		_setBool(AvatarController.isDeadHash, entity.IsDead());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform FindTransform(string _name)
	{
		return bipedT.FindInChildren(_name);
	}

	public override void SetVisible(bool _b)
	{
		if (isVisible == _b && isVisibleInit)
		{
			return;
		}
		isVisible = _b;
		isVisibleInit = true;
		Transform transform = bipedT;
		if ((bool)transform)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = _b;
			}
		}
	}

	public override Transform GetActiveModelRoot()
	{
		return modelT;
	}

	public override Transform GetRightHandTransform()
	{
		return rightHandT;
	}

	public override void SetInRightHand(Transform _transform)
	{
		idleTime = 0f;
		if ((bool)_transform)
		{
			Quaternion identity = Quaternion.identity;
			_transform.SetParent(GetRightHandTransform(), worldPositionStays: false);
			if (entity.inventory != null && entity.inventory.holdingItem != null)
			{
				AnimationGunjointOffsetData.AnimationGunjointOffsets animationGunjointOffsets = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value];
				_transform.localPosition = animationGunjointOffsets.position;
				_transform.localRotation = Quaternion.Euler(animationGunjointOffsets.rotation);
			}
			else
			{
				_transform.localPosition = Vector3.zero;
				_transform.localRotation = identity;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		float deltaTime = Time.deltaTime;
		if (actionTimeActive > 0f)
		{
			actionTimeActive -= deltaTime;
		}
		if (attackPlayingTime > 0f)
		{
			attackPlayingTime -= deltaTime;
			if (attackPlayingTime <= 0f)
			{
				isAttackImpact = true;
			}
		}
		if (timeSpecialAttack2Playing > 0f)
		{
			timeSpecialAttack2Playing -= deltaTime;
		}
		if (timeRagePlaying > 0f)
		{
			timeRagePlaying -= deltaTime;
		}
		if ((!isVisible && (!entity || !entity.RootMotion || entity.isEntityRemote)) || !bipedT || !bipedT.gameObject.activeInHierarchy || !anim || !anim.avatar.isValid || !anim.enabled)
		{
			return;
		}
		UpdateLayerStateInfo();
		SetLayerWeights();
		float speedForward = entity.speedForward;
		_setFloat(AvatarController.forwardHash, speedForward, _netSync: false);
		if (!entity.IsDead())
		{
			if (movementStateOverride != -1)
			{
				_setInt(AvatarController.movementStateHash, movementStateOverride);
				movementStateOverride = -1;
			}
			else
			{
				float num = speedForward * speedForward;
				_setInt(AvatarController.movementStateHash, (num > entity.moveSpeedAggro * entity.moveSpeedAggro) ? 3 : ((num > entity.moveSpeed * entity.moveSpeed) ? 2 : ((num > 0.001f) ? 1 : 0)), _netsync: false);
			}
		}
		if (electrocuteTime > 0.3f && !entity.emodel.IsRagdollActive)
		{
			_setTrigger(AvatarController.isElectrocutedHash);
		}
		if (bipedT.gameObject.activeInHierarchy)
		{
			if (entity.IsInElevator() || entity.Climbing)
			{
				_setBool(AvatarController.isClimbingHash, _value: true);
			}
			else
			{
				_setBool(AvatarController.isClimbingHash, _value: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LateUpdate()
	{
		if (!entity || !bipedT || !bipedT.gameObject.activeInHierarchy || !anim || !anim.enabled)
		{
			return;
		}
		UpdateLayerStateInfo();
		ItemClass holdingItem = entity.inventory.holdingItem;
		if (holdingItem.Actions[0] != null)
		{
			holdingItem.Actions[0].UpdateNozzleParticlesPosAndRot(entity.inventory.holdingItemData.actionData[0]);
		}
		if (holdingItem.Actions[1] != null)
		{
			holdingItem.Actions[1].UpdateNozzleParticlesPosAndRot(entity.inventory.holdingItemData.actionData[1]);
		}
		int fullPathHash = baseStateInfo.fullPathHash;
		bool flag = anim.IsInTransition(0);
		if (!flag)
		{
			isJumpStarted = false;
			if (fullPathHash == jumpState)
			{
				_setBool(AvatarController.jumpHash, _value: false);
			}
		}
		if (isInDeathAnim)
		{
			if (baseStateInfo.tagHash == AvatarController.deathHash && baseStateInfo.normalizedTime >= 1f && !flag)
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
		if (isCrawler && Time.time - crawlerTime > 2f)
		{
			isSuppressPain = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLayerStateInfo()
	{
		baseStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		overrideStateInfo = anim.GetCurrentAnimatorStateInfo(1);
		fullBodyStateInfo = anim.GetCurrentAnimatorStateInfo(2);
		if (anim.layerCount > 3)
		{
			hitStateInfo = anim.GetCurrentAnimatorStateInfo(3);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLayerWeights()
	{
		isSuppressPain = isSuppressPain && (anim.IsInTransition(2) || fullBodyStateInfo.fullPathHash != 0);
		anim.SetLayerWeight(1, 1f);
		anim.SetLayerWeight(2, (!isSuppressPain && entity.bodyDamage.CurrentStun == EnumEntityStunType.None) ? 1 : 0);
	}

	public override void ResetAnimations()
	{
		base.ResetAnimations();
		anim.Play("None", 1, 0f);
		anim.Play("None", 2, 0f);
	}

	public override void SetMeleeAttackSpeed(float _speed)
	{
	}

	public override ActionState GetActionState()
	{
		if (attackPlayingTime > 0f || overrideStateInfo.tagHash == AvatarController.attackHash)
		{
			return ActionState.Active;
		}
		int tagHash = fullBodyStateInfo.tagHash;
		if (tagHash == AvatarController.attackStartHash || actionTimeActive > 0f)
		{
			return ActionState.Start;
		}
		if (tagHash == AvatarController.attackReadyHash)
		{
			return ActionState.Ready;
		}
		if (tagHash == AvatarController.attackHash)
		{
			return ActionState.Active;
		}
		return ActionState.None;
	}

	public override bool IsActionActive()
	{
		return GetActionState() != ActionState.None;
	}

	public override void StartAction(int _animType)
	{
		if (_animType < 3000)
		{
			StartAnimationAttack();
			return;
		}
		idleTime = 0f;
		_setInt(AvatarController.attackHash, _animType);
		_setTrigger(AvatarController.attackTriggerHash);
		actionTimeActive = 0.2f;
	}

	public override bool IsAnimationAttackPlaying()
	{
		if (!(attackPlayingTime > 0f) && overrideStateInfo.tagHash != AvatarController.attackHash)
		{
			return fullBodyStateInfo.tagHash == AvatarController.attackHash;
		}
		return true;
	}

	public override void StartAnimationAttack()
	{
		if (!bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		idleTime = 0f;
		isAttackImpact = false;
		attackPlayingTime = 2f;
		float randomFloat = entity.rand.RandomFloat;
		int num = -1;
		if (!rightArmDismembered)
		{
			num = 0;
			if (!leftArmDismembered)
			{
				num = entity.rand.RandomInt & 1;
			}
		}
		else if (!leftArmDismembered)
		{
			num = 1;
		}
		int num2 = 8;
		if (num >= 0)
		{
			num2 = num;
		}
		int walkType = entity.GetWalkType();
		if (walkType >= 20)
		{
			num2 += walkType * 100;
		}
		if (entity.IsBreakingDoors && num >= 0)
		{
			num2 += 10;
		}
		if (num2 <= 1)
		{
			if (walkType == 1)
			{
				num2 += 100;
			}
			else if (entity.rand.RandomFloat < 0.25f)
			{
				num2 += 4;
			}
		}
		_setInt(AvatarController.attackHash, num2);
		_setFloat(AvatarController.attackBlendHash, randomFloat);
		_setTrigger(AvatarController.attackTriggerHash);
	}

	public override void SetAttackImpact()
	{
		if (!isAttackImpact)
		{
			isAttackImpact = true;
			attackPlayingTime = 0.1f;
		}
	}

	public override bool IsAttackImpact()
	{
		return isAttackImpact;
	}

	public override bool IsAnimationHitRunning()
	{
		if (hitWeight == 0f)
		{
			return false;
		}
		int tagHash = hitStateInfo.tagHash;
		if (tagHash != AvatarController.hitStartHash && (tagHash != AvatarController.hitHash || !(hitStateInfo.normalizedTime < 0.55f)))
		{
			return anim.IsInTransition(3);
		}
		return true;
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return false;
	}

	public override void StartAnimationSpecialAttack(bool _b, int _animType)
	{
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return timeSpecialAttack2Playing > 0f;
	}

	public override void StartAnimationSpecialAttack2()
	{
		idleTime = 0f;
		timeSpecialAttack2Playing = 0.3f;
		_setTrigger(AvatarController.specialAttack2Hash);
	}

	public override bool IsAnimationRagingPlaying()
	{
		return timeRagePlaying > 0f;
	}

	public override void StartAnimationRaging()
	{
		idleTime = 0f;
		_setTrigger(AvatarController.rageHash);
		timeRagePlaying = 0.3f;
	}

	public override void StartAnimationElectrocute(float _duration)
	{
		base.StartAnimationElectrocute(_duration);
		idleTime = 0f;
	}

	public override bool IsAnimationDigRunning()
	{
		return AvatarController.digHash == baseStateInfo.tagHash;
	}

	public override void StartAnimationDodge(float _blend)
	{
		_setFloat(AvatarController.dodgeBlendHash, _blend);
		_setBool(AvatarController.dodgeTriggerHash, _value: true);
	}

	public override void StartAnimationJumping()
	{
		idleTime = 0f;
		if (!(bipedT == null) && bipedT.gameObject.activeInHierarchy && anim != null)
		{
			_setBool(AvatarController.jumpHash, _value: true);
		}
	}

	public override void StartAnimationJump(AnimJumpMode jumpMode)
	{
		idleTime = 0f;
		if (bipedT == null || !bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		isJumpStarted = true;
		if (anim != null)
		{
			if (jumpMode == AnimJumpMode.Start)
			{
				_setTrigger(AvatarController.jumpStartHash);
				return;
			}
			_setTrigger(AvatarController.jumpLandHash);
			_setInt(AvatarController.jumpLandResponseHash, 0);
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

	public override void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, Utils.EnumHitDirection _hitDirection, bool _criticalHit, float random)
	{
		_setInt(AvatarController.stunTypeHash, (int)stun);
		_setInt(AvatarController.stunBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb());
		_setInt(AvatarController.hitDirectionHash, (int)_hitDirection);
		_setFloat(AvatarController.HitRandomValueHash, random);
		_setTrigger(AvatarController.beginStunTriggerHash);
		_resetTrigger(AvatarController.endStunTriggerHash);
	}

	public override void EndStun()
	{
		_setTrigger(AvatarController.endStunTriggerHash);
	}

	public override bool IsAnimationStunRunning()
	{
		if (baseStateInfo.tagHash == AvatarController.stunHash)
		{
			return true;
		}
		return false;
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		idleTime = 0f;
		isInDeathAnim = true;
		didDeathTransition = false;
		if (!(bipedT == null) && bipedT.gameObject.activeInHierarchy)
		{
			if (anim != null)
			{
				movementStateOverride = _movementState;
				_setInt(AvatarController.movementStateHash, _movementState);
				_setBool(AvatarController.isAliveHash, _value: false);
				_setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb());
				_setFloat(AvatarController.HitRandomValueHash, random);
				SetFallAndGround(_canFall: false, entity.onGround);
			}
			if (!(bipedT == null) && bipedT.gameObject.activeInHierarchy && anim != null)
			{
				_setTrigger(AvatarController.deathTriggerHash);
			}
		}
	}

	public override void StartEating()
	{
		if (!isEating)
		{
			_setInt(AvatarController.attackHash, 0);
			_setTrigger(AvatarController.beginCorpseEatHash);
			isEating = true;
		}
	}

	public override void StopEating()
	{
		if (isEating)
		{
			_setInt(AvatarController.attackHash, 0);
			_setTrigger(AvatarController.endCorpseEatHash);
			isEating = false;
		}
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		if (!isCrawler || Time.time - crawlerTime > 2f)
		{
			InternalStartAnimationHit(_bodyPart, _dir, _hitDamage, _criticalHit, _movementState, _random, _duration);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InternalStartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random, float _duration)
	{
		if (bipedT == null || !bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!CheckHit(_duration))
		{
			SetDataFloat(DataTypes.HitDuration, _duration);
			return;
		}
		idleTime = 0f;
		if ((bool)anim)
		{
			movementStateOverride = _movementState;
			_setInt(AvatarController.movementStateHash, _movementState);
			_setInt(AvatarController.hitDirectionHash, _dir);
			_setInt(AvatarController.hitDamageHash, _hitDamage);
			_setFloat(AvatarController.HitRandomValueHash, random);
			_setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb());
			SetDataFloat(DataTypes.HitDuration, _duration);
			_setTrigger(AvatarController.hitTriggerHash);
		}
	}

	public override void CrippleLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		if (!isCrippled && _bodyDamage.bodyPartHit.IsLeg())
		{
			int walkType = entity.GetWalkType();
			if (walkType != 5 && walkType < 20)
			{
				isCrippled = true;
				SetWalkType(5);
				_setTrigger(AvatarController.movementTriggerHash);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCensoredContent()
	{
		EntityClass entityClass = entity.EntityClass;
		if ((entityClass == null || entityClass.censorMode != 0) && GameManager.Instance.IsGoreCensored())
		{
			EntityClass entityClass2 = entity.EntityClass;
			if (entityClass2 == null || entityClass2.censorType != 2)
			{
				EntityClass entityClass3 = entity.EntityClass;
				if (entityClass3 == null)
				{
					return false;
				}
				return entityClass3.censorType == 3;
			}
			return true;
		}
		return false;
	}

	public void CleanupDismemberedLimbs()
	{
		for (int i = 0; i < dismemberedParts.Count; i++)
		{
			dismemberedParts[i].ReadyForCleanup = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _InitDismembermentMaterials()
	{
		if ((bool)mainZombieMaterial)
		{
			return;
		}
		EModelBase emodel = entity.emodel;
		if ((bool)emodel)
		{
			Transform meshTransform = emodel.meshTransform;
			if ((bool)meshTransform)
			{
				Renderer component = meshTransform.GetComponent<Renderer>();
				if ((bool)component)
				{
					mainZombieMaterial = component.sharedMaterial;
					bool flag = entity.HasAnyTags(DismembermentManager.radiatedTag) && (mainZombieMaterial.HasProperty("_IsRadiated") || mainZombieMaterial.HasProperty("_Irradiated"));
					DismembermentManager instance = DismembermentManager.Instance;
					gibCapMaterial = ((!flag) ? instance.GibCapsMaterial : instance.GibCapsRadMaterial);
				}
			}
		}
		isCensored = isCensoredContent();
	}

	public override void RemoveLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		if (DismembermentManager.DebugDontCreateParts)
		{
			return;
		}
		int num = DismembermentManager.Instance?.parts.Count ?? 0;
		if (entity.isDisintegrated && num >= 25)
		{
			return;
		}
		_InitDismembermentMaterials();
		EnumBodyPartHit bodyPartHit = _bodyDamage.bodyPartHit;
		EnumDamageTypes enumDamageTypes = _bodyDamage.damageType;
		bool flag = enumDamageTypes == EnumDamageTypes.Heat;
		if (isCensored)
		{
			enumDamageTypes = ((!DismembermentManager.BluntCensors.Contains(entity.EntityClass?.entityClassName)) ? EnumDamageTypes.Piercing : EnumDamageTypes.Bashing);
		}
		int num2 = 0;
		if (!headDismembered && (bodyPartHit & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
		{
			headDismembered = true;
			num2++;
			if (flag || entity.OverrideHeadSize != 1f)
			{
				enumDamageTypes = EnumDamageTypes.Piercing;
			}
			Transform partT = FindTransform("Neck");
			MakeDismemberedPart(1u, enumDamageTypes, partT, restoreState);
			Transform transform = bipedT.Find("HeadAccessories");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(value: false);
			}
		}
		if (!leftUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
		{
			leftUpperLegDismembered = true;
			num2++;
			Transform partT2 = FindTransform("LeftUpLeg");
			MakeDismemberedPart(32u, enumDamageTypes, partT2, restoreState);
		}
		if (!leftLowerLegDismembered && !leftUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
		{
			leftLowerLegDismembered = true;
			num2++;
			Transform partT3 = FindTransform("LeftLeg");
			MakeDismemberedPart(64u, enumDamageTypes, partT3, restoreState);
		}
		if (!rightUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
		{
			rightUpperLegDismembered = true;
			num2++;
			Transform partT4 = FindTransform("RightUpLeg");
			MakeDismemberedPart(128u, enumDamageTypes, partT4, restoreState);
		}
		if (!rightLowerLegDismembered && !rightUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
		{
			rightLowerLegDismembered = true;
			num2++;
			Transform partT5 = FindTransform("RightLeg");
			MakeDismemberedPart(256u, enumDamageTypes, partT5, restoreState);
		}
		if (!leftUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
		{
			leftUpperArmDismembered = true;
			num2++;
			Transform partT6 = FindTransform("LeftArm");
			MakeDismemberedPart(2u, enumDamageTypes, partT6, restoreState);
		}
		if (!leftLowerArmDismembered && !leftUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None)
		{
			leftLowerArmDismembered = true;
			num2++;
			Transform partT7 = FindTransform("LeftForeArm");
			MakeDismemberedPart(4u, enumDamageTypes, partT7, restoreState);
		}
		if (!rightUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
		{
			rightUpperArmDismembered = true;
			num2++;
			Transform partT8 = FindTransform("RightArm");
			MakeDismemberedPart(8u, enumDamageTypes, partT8, restoreState);
		}
		if (!rightLowerArmDismembered && !rightUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None)
		{
			rightLowerArmDismembered = true;
			num2++;
			Transform partT9 = FindTransform("RightForeArm");
			MakeDismemberedPart(16u, enumDamageTypes, partT9, restoreState);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform SpawnLimbGore(Transform parent, string path, bool restoreState)
	{
		if ((bool)parent && !string.IsNullOrEmpty(path))
		{
			string text = DismembermentManager.GetAssetBundlePath(path);
			GameObject gameObject = null;
			if (isCensored)
			{
				string text2 = text.Replace(".", "_CGore.");
				GameObject gameObject2 = DataLoader.LoadAsset<GameObject>(text2);
				if ((bool)gameObject2)
				{
					text = text2;
					gameObject = gameObject2;
				}
			}
			if (!gameObject)
			{
				gameObject = DataLoader.LoadAsset<GameObject>(text);
			}
			if (!gameObject)
			{
				return null;
			}
			GameObject obj = UnityEngine.Object.Instantiate(gameObject, parent);
			GorePrefab component = obj.GetComponent<GorePrefab>();
			if ((bool)component)
			{
				component.restoreState = restoreState;
			}
			return obj.transform;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcDismemberedPart(Transform t, Transform partT, DismemberedPartData part, uint bodyDamageFlag)
	{
		Transform transform = partT.FindRecursive(part.targetBone);
		if ((bool)transform)
		{
			if (!part.attachToParent)
			{
				Vector3 localScale = t.localScale;
				localScale.x /= Utils.FastMax(0.01f, transform.localScale.x);
				localScale.y /= Utils.FastMax(0.01f, transform.localScale.y);
				localScale.z /= Utils.FastMax(0.01f, transform.localScale.z);
				t.localScale = localScale;
			}
			if (!string.IsNullOrEmpty(part.childTargetObj))
			{
				Transform transform2 = new GameObject("scaleTarget").transform;
				transform2.position = transform.position;
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).SetParent(transform2);
				}
				transform2.SetParent(transform.parent);
				transform.SetParent(transform2);
				transform2.localScale = Vector3.zero;
			}
			if (!string.IsNullOrEmpty(part.insertBoneObj))
			{
				Transform transform3 = new GameObject("scaleTarget").transform;
				transform3.position = transform.position;
				for (int j = 0; j < transform.childCount; j++)
				{
					transform.GetChild(j).SetParent(transform3);
				}
				transform3.SetParent(transform);
				if (defaultHeadPos != Vector3.zero)
				{
					transform3.position = defaultHeadPos;
					Vector3 localPosition = transform3.localPosition;
					localPosition.z = (0f - localPosition.y) * 0.5f;
					transform3.localPosition = localPosition;
				}
				transform3.localScale = Vector3.zero;
			}
			if (!string.IsNullOrEmpty(part.addScalePoint))
			{
				addScalePoint(transform);
			}
			if (!string.IsNullOrEmpty(part.maskScaleBlend))
			{
				partT.FindRecursive(part.maskScaleBlend).localScale = part.scale;
			}
			if (!string.IsNullOrEmpty(part.setFixedValues))
			{
				Transform boneT = partT.FindRecursive(part.setFixedValues);
				Transform obj = addScalePoint(boneT).transform;
				obj.localPosition = part.pos;
				obj.localScale = part.scale;
			}
		}
		if (part.hasRotOffset)
		{
			t.localEulerAngles = part.rot;
		}
		if (DismembermentManager.DebugShowArmRotations)
		{
			DismembermentManager.AddDebugArmObjects(partT, t);
		}
		if (part.offset != Vector3.zero)
		{
			Transform transform4 = t.FindRecursive("pos");
			if ((bool)transform4)
			{
				transform4.localPosition += part.offset;
			}
		}
		if (part.particlePaths != null)
		{
			for (int k = 0; k < part.particlePaths.Length; k++)
			{
				string value = part.particlePaths[k];
				if (!string.IsNullOrEmpty(value))
				{
					DismembermentManager.SpawnParticleEffect(new ParticleEffect(value, t.position + Origin.position, Quaternion.identity, 1f, Color.white));
				}
			}
		}
		Transform transform5 = t.FindRecursive("pos");
		if ((bool)transform5)
		{
			Renderer[] componentsInChildren = transform5.GetComponentsInChildren<Renderer>(includeInactive: true);
			Material altMaterial = entity.emodel.AltMaterial;
			if ((bool)altMaterial)
			{
				altMatName = altMaterial.name;
				for (int l = 0; l < altMatName.Length; l++)
				{
					char c = altMatName[l];
					if (char.IsDigit(c))
					{
						altEntityMatId = int.Parse(c.ToString());
						break;
					}
				}
			}
			else
			{
				string text = mainZombieMaterial.name;
				for (int m = 0; m < text.Length; m++)
				{
					char c2 = text[m];
					if (char.IsDigit(c2))
					{
						altEntityMatId = int.Parse(c2.ToString());
						break;
					}
				}
			}
			foreach (Renderer renderer in componentsInChildren)
			{
				if ((bool)renderer.GetComponent<ParticleSystem>())
				{
					continue;
				}
				Material[] sharedMaterials = renderer.sharedMaterials;
				for (int num = 0; num < sharedMaterials.Length; num++)
				{
					Material material = sharedMaterials[num];
					string a = material.name;
					if ((part.prefabPath.ContainsCaseInsensitive("head") && a.ContainsCaseInsensitive("hair")) || (renderer.name.ContainsCaseInsensitive("eye") && !material.HasProperty("_IsRadiated") && !material.HasProperty("_Irradiated")))
					{
						continue;
					}
					bool flag = false;
					for (int num2 = 0; num2 < DismembermentManager.DefaultBundleGibs.Length; num2++)
					{
						flag = a.ContainsCaseInsensitive(DismembermentManager.DefaultBundleGibs[num2]);
						if (!flag)
						{
							continue;
						}
						if (a.ContainsCaseInsensitive("ZombieGibs_caps"))
						{
							if (!gibCapMaterialCopy)
							{
								gibCapMaterialCopy = UnityEngine.Object.Instantiate(gibCapMaterial);
								gibCapMaterialCopy.name = gibCapMaterial.name.Replace("(global)", "(local)");
							}
							sharedMaterials[num] = gibCapMaterialCopy;
						}
						break;
					}
					if (!flag && material.name.Contains("HD_"))
					{
						if (!mainZombieMaterialCopy)
						{
							mainZombieMaterialCopy = UnityEngine.Object.Instantiate(mainZombieMaterial);
						}
						sharedMaterials[num] = mainZombieMaterialCopy;
					}
				}
				renderer.materials = sharedMaterials;
			}
		}
		if (entity.IsFeral && bodyDamageFlag == 1)
		{
			setUpEyeMats(t);
			if (part.isDetachable)
			{
				Transform transform6 = t.FindRecursive("Detachable");
				if ((bool)transform6)
				{
					setUpEyeMats(transform6);
				}
			}
			Transform transform7 = t.FindRecursive("FeralFlame");
			if ((bool)transform7 && !entity.HasAnyTags(DismembermentManager.radiatedTag))
			{
				transform7.gameObject.SetActive(value: true);
				string text2 = "large_flames_LOD (3)";
				Transform transform8 = entity.transform.FindRecursive(text2);
				if ((bool)transform8)
				{
					transform8.gameObject.SetActive(value: false);
				}
				else
				{
					Log.Warning("entity {0} no longer has a child named {1}", entity.name, text2);
				}
			}
		}
		if ((bool)dismemberMat || string.IsNullOrEmpty(subFolderDismemberEntityName))
		{
			return;
		}
		string text3 = rootDismmemberDir + $"/gibs_{subFolderDismemberEntityName.ToLower()}";
		Material sharedMaterial = skinnedMeshRenderer.sharedMaterial;
		if (entity.HasAnyTags(DismembermentManager.radiatedTag) && (sharedMaterial.HasProperty("_IsRadiated") || sharedMaterial.HasProperty("_Irradiated")))
		{
			text3 += "_IsRadiated";
		}
		Material material2 = null;
		if (!string.IsNullOrEmpty(part.dismemberMatPath))
		{
			string text4 = rootDismmemberDir + "/" + part.dismemberMatPath + ".mat";
			material2 = DataLoader.LoadAsset<Material>(text4);
			if ((bool)material2)
			{
				text3 = text4;
			}
		}
		text3 += ((altEntityMatId != -1) ? ((object)altEntityMatId) : "");
		if (isCensored)
		{
			string text5 = text3;
			text3 += "_CGore.mat";
			material2 = DataLoader.LoadAsset<Material>(text3);
			if (!material2)
			{
				text3 = text5;
			}
		}
		if (!material2)
		{
			text3 += ".mat";
		}
		material2 = DataLoader.LoadAsset<Material>(text3);
		if (!material2)
		{
			_ = part.useMask;
			return;
		}
		dismemberMat = UnityEngine.Object.Instantiate(material2);
		if (dismemberMat.HasColor("_EmissiveColor") && sharedMaterial.HasColor("_EmissiveColor"))
		{
			dismemberMat.SetColor("_EmissiveColor", sharedMaterial.GetColor("_EmissiveColor"));
		}
		skinnedMeshRenderer.material = dismemberMat;
		if ((bool)smrLODOne)
		{
			smrLODOne.material = dismemberMat;
		}
		if ((bool)smrLODTwo)
		{
			smrLODTwo.material = dismemberMat;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject addScalePoint(Transform boneT)
	{
		GameObject gameObject = new GameObject("scaleTarget");
		Transform transform = gameObject.transform;
		for (int i = 0; i < boneT.childCount; i++)
		{
			boneT.GetChild(i).SetParent(transform);
		}
		transform.SetParent(boneT);
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.zero;
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setUpEyeMats(Transform t)
	{
		Transform transform = t.FindRecursive("NormalEye");
		Transform transform2 = t.FindRecursive("FeralEye");
		if ((bool)transform2 && !entity.HasAnyTags(DismembermentManager.radOrChargedTag))
		{
			if ((bool)transform)
			{
				transform.gameObject.SetActive(value: false);
			}
			transform2.gameObject.SetActive(value: true);
		}
		else
		{
			if (!transform)
			{
				return;
			}
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			if ((bool)component)
			{
				Material material = UnityEngine.Object.Instantiate(component.material);
				if (material.HasProperty("_IsRadiated"))
				{
					material.SetFloat("_IsRadiated", 1f);
				}
				if (material.HasProperty("_Irradiated"))
				{
					material.SetFloat("_Irradiated", 1f);
				}
				material.DisableKeyword("_ELECTRIC_SHOCK_ON");
				component.material = material;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeDismemberedPart(uint bodyDamageFlag, EnumDamageTypes damageType, Transform partT, bool restoreState)
	{
		DismemberedPartData dismemberedPartData = DismembermentManager.DismemberPart(bodyDamageFlag, damageType, entity, isBiped: true, DismembermentManager.DebugUseLegacy);
		if (dismemberedPartData == null)
		{
			return;
		}
		DismemberedPart dismemberedPart = new DismemberedPart(dismemberedPartData, bodyDamageFlag, damageType);
		dismemberedParts.Add(dismemberedPart);
		if (!partT)
		{
			return;
		}
		Transform transform = null;
		if (!string.IsNullOrEmpty(dismemberedPartData.targetBone))
		{
			Transform transform2 = partT.FindRecursive(dismemberedPartData.targetBone);
			if (!transform2)
			{
				transform2 = partT.FindParent(dismemberedPartData.targetBone);
			}
			Transform transform3 = new GameObject("DynamicGore").transform;
			if (!dismemberedPartData.attachToParent)
			{
				transform3.SetParent(transform2);
			}
			else
			{
				transform3.SetParent(transform2.parent);
			}
			transform3.localPosition = Vector3.zero;
			transform3.localRotation = Quaternion.identity;
			transform3.localScale = Vector3.one;
			transform = transform3;
			defaultHeadPos = Vector3.zero;
			if (!dismemberedPartData.useMask)
			{
				if (!string.IsNullOrEmpty(dismemberedPartData.insertBoneObj))
				{
					Transform transform4 = transform2.FindRecursive(dismemberedPartData.insertBoneObj);
					defaultHeadPos = transform4.position;
				}
				transform2.localScale = dismemberedPartData.scale;
				scaleOutChildBones(transform2);
			}
			else
			{
				Collider component = transform2.GetComponent<Collider>();
				if ((bool)component)
				{
					component.enabled = false;
				}
				disableChildColliders(transform2);
			}
		}
		else
		{
			partT.localScale = dismemberedPartData.scale;
		}
		if (string.IsNullOrEmpty(dismemberedPartData.prefabPath))
		{
			return;
		}
		if (string.IsNullOrEmpty(rootDismmemberDir) && dismemberedPartData.prefabPath.Contains("/"))
		{
			subFolderDismemberEntityName = dismemberedPartData.prefabPath.Remove(dismemberedPartData.prefabPath.IndexOf("/"));
			rootDismmemberDir = "@:Entities/Zombies/" + subFolderDismemberEntityName + "/Dismemberment";
		}
		if (!transform)
		{
			string text = dismemberedPartData.propertyKey.Replace("DismemberTag_", "");
			transform = GameUtils.FindTagInChilds(bipedT, text);
		}
		Transform transform5 = SpawnLimbGore(transform, dismemberedPartData.prefabPath, restoreState);
		if (!transform5 || string.IsNullOrEmpty(dismemberedPartData.targetBone))
		{
			return;
		}
		if (DismembermentManager.DebugLogEnabled)
		{
			MeshFilter[] componentsInChildren = transform5.GetComponentsInChildren<MeshFilter>();
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				Mesh sharedMesh = meshFilter.sharedMesh;
				if (!sharedMesh || ((bool)sharedMesh && sharedMesh.vertexCount == 0))
				{
					Log.Warning($"{GetType()} prefabPath {dismemberedPartData.prefabPath} partName {meshFilter.transform.name} is missing a mesh.");
				}
			}
		}
		if (!skinnedMeshRenderer)
		{
			skinnedMeshRenderer = entity.emodel.meshTransform.GetComponent<SkinnedMeshRenderer>();
			Transform parent = skinnedMeshRenderer.transform.parent;
			for (int j = 0; j < parent.childCount; j++)
			{
				Transform child = parent.GetChild(j);
				if (child.name.ContainsCaseInsensitive("LOD1"))
				{
					smrLODOne = child.GetComponent<SkinnedMeshRenderer>();
				}
				if (child.name.ContainsCaseInsensitive("LOD2"))
				{
					smrLODTwo = child.GetComponent<SkinnedMeshRenderer>();
				}
			}
		}
		Renderer[] componentsInChildren2 = transform5.GetComponentsInChildren<Renderer>();
		bool flag = false;
		foreach (Renderer renderer in componentsInChildren2)
		{
			if ((bool)renderer.GetComponent<ParticleSystem>())
			{
				continue;
			}
			Material[] sharedMaterials = renderer.sharedMaterials;
			for (int l = 0; l < sharedMaterials.Length; l++)
			{
				Material material = sharedMaterials[l];
				string text2 = material.name;
				if ((!dismemberedPart.prefabPath.ContainsCaseInsensitive("head") || !text2.ContainsCaseInsensitive("hair")) && material.shader.name == "Game/Character" && !DismembermentManager.IsDefaultGib(text2))
				{
					sharedMaterials[l] = mainZombieMaterial;
					flag = true;
				}
			}
			if (flag)
			{
				renderer.sharedMaterials = sharedMaterials;
			}
		}
		ProcDismemberedPart(transform5, partT, dismemberedPartData, bodyDamageFlag);
		dismemberedPart.prefabT = transform5;
		Transform transform6 = partT.FindRecursive(dismemberedPartData.targetBone);
		if (!transform6)
		{
			transform6 = partT.FindParent(dismemberedPartData.targetBone);
		}
		dismemberedPart.targetT = transform6;
		if (dismemberedPartData.useMask)
		{
			if (dismemberedPartData.scaleOutLimb)
			{
				Transform transform7 = partT.FindRecursive(dismemberedPartData.targetBone);
				if (!transform7)
				{
					transform7 = partT.FindParent(dismemberedPartData.targetBone);
				}
				if (!string.IsNullOrEmpty(dismemberedPartData.solTarget))
				{
					transform7 = partT.FindRecursive(dismemberedPartData.solTarget);
					if (!transform7)
					{
						transform7 = partT.FindParent(dismemberedPartData.solTarget);
					}
				}
				scaleOutChildBones(transform7);
				if (dismemberedPartData.hasSolScale)
				{
					transform7.localScale = dismemberedPartData.solScale;
				}
			}
			else
			{
				scaleOutChildBones(transform6);
			}
			EnumBodyPartHit bodyPartHit = DismembermentManager.GetBodyPartHit(dismemberedPart.bodyDamageFlag);
			Transform transform8 = modelT.FindTagInChildren("D_Accessory");
			if ((bool)transform8)
			{
				DismembermentAccessoryMan component2 = transform8.GetComponent<DismembermentAccessoryMan>();
				if ((bool)component2)
				{
					component2.HidePart(bodyPartHit);
				}
			}
			if (!dismemberedPartData.scaleOutLimb || !string.IsNullOrEmpty(dismemberedPartData.solTarget) || !string.IsNullOrEmpty(dismemberedPartData.maskScaleBlend))
			{
				setLimbShaderProps(bodyPartHit, dismemberedPart);
			}
		}
		if (dismemberedPartData.isDetachable)
		{
			ActivateDetachableLimbs(bodyDamageFlag, damageType, transform5, dismemberedPart);
		}
	}

	[Conditional("DEBUG_DISMEMBERMENT")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void logDismemberment(string _log)
	{
		if (DismembermentManager.DebugLogEnabled)
		{
			Log.Out(GetType()?.ToString() + " " + _log);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void scaleOutChildBones(Transform _boneT)
	{
		if (_boneT.childCount <= 0)
		{
			return;
		}
		for (int i = 0; i < _boneT.childCount; i++)
		{
			Transform child = _boneT.GetChild(i);
			if ((bool)child && !child.name.Equals("DynamicGore"))
			{
				child.localScale = Vector3.zero;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void disableChildColliders(Transform _boneT)
	{
		Collider[] componentsInChildren = _boneT.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if ((bool)collider)
			{
				collider.enabled = false;
			}
		}
		CharacterJoint[] componentsInChildren2 = _boneT.GetComponentsInChildren<CharacterJoint>();
		foreach (CharacterJoint characterJoint in componentsInChildren2)
		{
			if ((bool)characterJoint)
			{
				Rigidbody component = characterJoint.GetComponent<Rigidbody>();
				UnityEngine.Object.Destroy(characterJoint);
				UnityEngine.Object.Destroy(component);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ActivateDetachableLimbs(uint bodyDamageFlag, EnumDamageTypes damageType, Transform partT, DismemberedPart part)
	{
		Transform entitiesTransform = GameManager.Instance.World.EntitiesTransform;
		if (!entitiesTransform)
		{
			return;
		}
		Transform transform = entitiesTransform.Find("DismemberedLimbs");
		if (!transform)
		{
			transform = new GameObject("DismemberedLimbs").transform;
			transform.SetParent(entitiesTransform);
			transform.localPosition = Vector3.zero;
		}
		Transform transform2 = partT.FindRecursive("Detachable");
		if (!transform2)
		{
			return;
		}
		EnumBodyPartHit bodyPartHit = DismembermentManager.GetBodyPartHit(bodyDamageFlag);
		Transform transform3 = new GameObject($"{entity.entityId}_{entity.EntityName}_{bodyPartHit}").transform;
		transform3.SetParent(transform);
		part.SetDetachedTransform(transform3, transform2);
		if (entity.IsBloodMoon)
		{
			part.lifeTime /= 3f;
		}
		if (leftLowerArmDismembered && bodyDamageFlag == 2)
		{
			DismembermentManager.ActivateDetachable(transform2, "HalfArm");
			hideDismemberedPart(bodyDamageFlag);
		}
		if (leftLowerLegDismembered && bodyDamageFlag == 32)
		{
			DismembermentManager.ActivateDetachable(transform2, "HalfLeg");
			hideDismemberedPart(bodyDamageFlag);
		}
		if (rightLowerArmDismembered && bodyDamageFlag == 8)
		{
			DismembermentManager.ActivateDetachable(transform2, "HalfArm");
			hideDismemberedPart(bodyDamageFlag);
		}
		if (rightLowerLegDismembered && bodyDamageFlag == 128)
		{
			DismembermentManager.ActivateDetachable(transform2, "HalfLeg");
			hideDismemberedPart(bodyDamageFlag);
		}
		if (!transform2.gameObject.activeSelf)
		{
			transform2.gameObject.SetActive(value: true);
		}
		if (entity.OverrideHeadSize != 1f && headDismembered && bodyDamageFlag == 1)
		{
			float num = (part.overrideHeadSize = entity.emodel.HeadBigSize);
			part.overrideHeadDismemberScaleTime = entity.OverrideHeadDismemberScaleTime;
			Transform transform4 = transform2.Find("Physics");
			Transform transform5 = new GameObject("pivot").transform;
			transform5.SetParent(transform4);
			transform5.localScale = Vector3.one;
			for (int i = 0; i < part.targetT.childCount; i++)
			{
				Transform child = part.targetT.GetChild(i);
				if (child.CompareTag("E_BP_Head"))
				{
					Transform transform6 = transform2.FindRecursive(bodyPartHit.ToString());
					if ((bool)transform6)
					{
						Renderer component = transform6.GetComponent<Renderer>();
						transform5.position = child.position + (component.bounds.center - child.position);
						part.pivotT = transform5;
					}
					else
					{
						transform5.position = child.position;
						part.pivotT = transform5;
					}
					break;
				}
			}
			List<Transform> list = new List<Transform>();
			for (int j = 0; j < transform4.childCount; j++)
			{
				Transform child2 = transform4.GetChild(j);
				if (child2 != transform4)
				{
					list.Add(child2);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				list[k].SetParent(transform5);
			}
			transform5.localScale = new Vector3(num, num, num);
		}
		transform2.SetParent(transform3);
		DismembermentManager.Instance?.AddPart(part);
		string empty = string.Empty;
		Renderer[] componentsInChildren = transform2.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!(renderer != null))
			{
				continue;
			}
			Material[] sharedMaterials = renderer.sharedMaterials;
			for (int m = 0; m < sharedMaterials.Length; m++)
			{
				Material material = sharedMaterials[m];
				if (!(material != null))
				{
					continue;
				}
				empty = material.name;
				if ((!part.prefabPath.ContainsCaseInsensitive("head") || !empty.ContainsCaseInsensitive("hair")) && (!renderer.name.ContainsCaseInsensitive("eye") || material.HasProperty("_IsRadiated") || material.HasProperty("_Irradiated")))
				{
					if (empty.ContainsCaseInsensitive("ZombieGibs_caps"))
					{
						sharedMaterials[m] = gibCapMaterial;
					}
					if (empty.Contains("HD_"))
					{
						sharedMaterials[m] = mainZombieMaterial;
					}
					sharedMaterials[m].DisableKeyword("_ELECTRIC_SHOCK_ON");
				}
			}
			renderer.sharedMaterials = sharedMaterials;
		}
		Jiggle[] componentsInChildren2 = transform2.GetComponentsInChildren<Jiggle>(includeInactive: true);
		for (int n = 0; n < componentsInChildren2.Length; n++)
		{
			componentsInChildren2[n].enabled = true;
		}
		Rigidbody componentInChildren = transform2.GetComponentInChildren<Rigidbody>();
		if (!componentInChildren)
		{
			return;
		}
		Vector3 vector = Vector3.up * entity.lastHitForce;
		float num2 = Vector3.Angle(entity.GetForwardVector(), entity.lastHitImpactDir);
		componentInChildren.AddTorque(Quaternion.FromToRotation(entity.GetForwardVector(), entity.lastHitImpactDir).eulerAngles * (1f + num2 / 90f), ForceMode.Impulse);
		componentInChildren.AddForce((entity.lastHitImpactDir + vector) * entity.lastHitForce, ForceMode.Impulse);
		string damageTag = DismembermentManager.GetDamageTag(damageType, entity.lastHitRanged);
		if (damageTag == "blunt")
		{
			if (damageType == EnumDamageTypes.Piercing)
			{
				componentInChildren.AddForce(entity.lastHitImpactDir + vector, ForceMode.Impulse);
			}
			else
			{
				componentInChildren.AddForce(entity.lastHitImpactDir * entity.lastHitForce * 1.5f + vector * 1.25f, ForceMode.Impulse);
			}
		}
		if (damageTag == "blade")
		{
			float num3 = Vector3.Dot(entity.GetForwardVector(), entity.lastHitImpactDir);
			float num4 = Vector3.Dot(entity.GetForwardVector(), entity.lastHitEntityFwd);
			componentInChildren.AddForce((num3 < num4) ? (-entity.transform.right * entity.lastHitForce + vector) : (entity.transform.right * entity.lastHitForce + vector), ForceMode.Impulse);
			componentInChildren.AddTorque(Quaternion.FromToRotation(entity.GetForwardVector(), entity.lastHitImpactDir).eulerAngles * (1f + num2 / 90f) * entity.lastHitForce, ForceMode.Impulse);
		}
		if (damageType == EnumDamageTypes.Heat)
		{
			float num5 = 2.67f;
			componentInChildren.AddForce(entity.lastHitImpactDir * num5 + Vector3.up * num5 * 0.67f, ForceMode.Impulse);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Material GetMainZombieBodyMaterial()
	{
		EModelBase emodel = entity.emodel;
		if ((bool)emodel)
		{
			Transform meshTransform = emodel.meshTransform;
			if ((bool)meshTransform)
			{
				return meshTransform.GetComponent<Renderer>().sharedMaterial;
			}
		}
		return null;
	}

	public override void Electrocute(bool enabled)
	{
		base.Electrocute(enabled);
		if (enabled)
		{
			Material mainZombieBodyMaterial = GetMainZombieBodyMaterial();
			if ((bool)mainZombieBodyMaterial)
			{
				mainZombieBodyMaterial.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if ((bool)dismemberMat)
			{
				dismemberMat.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if ((bool)mainZombieMaterialCopy)
			{
				mainZombieMaterialCopy.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if ((bool)gibCapMaterialCopy)
			{
				gibCapMaterialCopy.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			StartAnimationElectrocute(0.6f);
		}
		else
		{
			Material mainZombieBodyMaterial2 = GetMainZombieBodyMaterial();
			if ((bool)mainZombieBodyMaterial2)
			{
				mainZombieBodyMaterial2.DisableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if ((bool)dismemberMat)
			{
				dismemberMat.DisableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if ((bool)mainZombieMaterialCopy)
			{
				mainZombieMaterialCopy.DisableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if ((bool)gibCapMaterialCopy)
			{
				gibCapMaterialCopy.DisableKeyword("_ELECTRIC_SHOCK_ON");
			}
			StartAnimationElectrocute(0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLimbShaderProps(EnumBodyPartHit partHit, DismemberedPart part)
	{
		DismemberedPartData data = part.Data;
		if (!dismemberMat)
		{
			return;
		}
		bool scaleOutLimb = data.scaleOutLimb;
		bool isLinked = data.isLinked;
		if (dismemberMat.HasProperty("_LeftLowerLeg") && (partHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
		{
			dismemberMat.SetFloat("_LeftLowerLeg", 1f);
			if (isLinked)
			{
				dismemberMat.SetFloat("_LeftUpperLeg", 1f);
			}
		}
		if (dismemberMat.HasProperty("_LeftUpperLeg") && (partHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
		{
			if (!isLinked)
			{
				dismemberMat.SetFloat("_LeftUpperLeg", 1f);
			}
			if (!scaleOutLimb)
			{
				dismemberMat.SetFloat("_LeftLowerLeg", 1f);
			}
		}
		if (dismemberMat.HasProperty("_RightLowerLeg") && (partHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
		{
			dismemberMat.SetFloat("_RightLowerLeg", 1f);
			if (isLinked)
			{
				dismemberMat.SetFloat("_RightUpperLeg", 1f);
			}
		}
		if (dismemberMat.HasProperty("_RightUpperLeg") && (partHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
		{
			if (!isLinked)
			{
				dismemberMat.SetFloat("_RightUpperLeg", 1f);
			}
			if (!scaleOutLimb)
			{
				dismemberMat.SetFloat("_RightLowerLeg", 1f);
			}
		}
		if (dismemberMat.HasProperty("_LeftLowerArm") && (partHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None && !scaleOutLimb)
		{
			dismemberMat.SetFloat("_LeftLowerArm", 1f);
		}
		if (dismemberMat.HasProperty("_LeftUpperArm") && (partHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
		{
			if (!isLinked)
			{
				dismemberMat.SetFloat("_LeftUpperArm", 1f);
			}
			if (!scaleOutLimb)
			{
				dismemberMat.SetFloat("_LeftLowerArm", 1f);
			}
		}
		if (dismemberMat.HasProperty("_RightLowerArm") && (partHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None && !scaleOutLimb)
		{
			dismemberMat.SetFloat("_RightLowerArm", 1f);
		}
		if (dismemberMat.HasProperty("_RightUpperArm") && (partHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
		{
			if (!isLinked)
			{
				dismemberMat.SetFloat("_RightUpperArm", 1f);
			}
			if (!scaleOutLimb)
			{
				dismemberMat.SetFloat("_RightLowerArm", 1f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hideDismemberedPart(uint bodyDamageFlag)
	{
		uint lowerBodyPart = 0u;
		if (bodyDamageFlag == 2)
		{
			lowerBodyPart = 4u;
		}
		if (bodyDamageFlag == 8)
		{
			lowerBodyPart = 16u;
		}
		if (bodyDamageFlag == 32)
		{
			lowerBodyPart = 64u;
		}
		if (bodyDamageFlag == 128)
		{
			lowerBodyPart = 256u;
		}
		if (lowerBodyPart != 0)
		{
			dismemberedParts.Find([PublicizedFrom(EAccessModifier.Internal)] (DismemberedPart p) => p.bodyDamageFlag == lowerBodyPart)?.Hide();
		}
	}

	public override void TurnIntoCrawler(bool restoreState)
	{
		if (!isCrawler && entity.GetWalkType() != 21)
		{
			isCrawler = true;
			crawlerTime = Time.time;
			isSuppressPain = true;
			_setInt(AvatarController.hitBodyPartHash, 0);
			SetWalkType(21);
			_setTrigger(AvatarController.toCrawlerTriggerHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		if (!IsCrawler || _propertyHash != AvatarController.walkTypeHash || _value != 5)
		{
			base._setInt(_propertyHash, _value, _netsync);
		}
	}

	public override void TriggerSleeperPose(int pose, bool returningToSleep = false)
	{
		if (returningToSleep)
		{
			base.TriggerSleeperPose(pose, returningToSleep);
		}
		else if ((bool)anim)
		{
			_setInt(AvatarController.sleeperPoseHash, pose);
			switch (pose)
			{
			case 0:
				anim.Play(AvatarController.sleeperIdleSitHash);
				break;
			case 1:
				anim.Play(AvatarController.sleeperIdleSideRightHash);
				break;
			case 2:
				anim.Play(AvatarController.sleeperIdleSideLeftHash);
				break;
			case 3:
				anim.Play(AvatarController.sleeperIdleBackHash);
				break;
			case 4:
				anim.Play(AvatarController.sleeperIdleStomachHash);
				break;
			case 5:
				anim.Play(AvatarController.sleeperIdleStandHash);
				break;
			case -2:
				anim.CrossFadeInFixedTime("Crouch Walk 8", 0.25f);
				break;
			case -1:
				_setTrigger(AvatarController.sleeperTriggerHash);
				break;
			}
		}
	}
}
