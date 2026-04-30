using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarAnimalController : AvatarController
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool visInit;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_bVisible;

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
	public Transform head;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform leftUpperArm;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform rightUpperArm;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform leftUpperLeg;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform rightUpperLeg;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float limbScale;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimatorStateInfo baseStateInfo;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float idleTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float actionTimeActive;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackPlayingTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool headDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool leftUpperArmDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool rightUpperArmDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool leftUpperLegDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool rightUpperLegDismembered;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isInDeathAnim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int missingMotorLimbs;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEating;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		modelT = EModelBase.FindModel(base.transform);
		if (EntityClass.list[entity.entityClass].PainResistPerHit >= 0f)
		{
			hitLayerIndex = 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void assignBodyParts()
	{
		Transform transform = bipedT.FindInChilds("Hips");
		if ((bool)transform)
		{
			head = bipedT.FindInChilds("Head");
			leftUpperLeg = bipedT.FindInChilds("LeftUpLeg");
			rightUpperLeg = bipedT.FindInChilds("RightUpLeg");
			leftUpperArm = bipedT.FindInChilds("LeftArm");
			rightUpperArm = bipedT.FindInChilds("RightArm");
			limbScale = (head.position - transform.position).magnitude * 1.32f / head.lossyScale.x;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform SpawnLimbGore(Transform limbT, bool isLeft, string path, bool restoreState)
	{
		if (limbT == null)
		{
			return null;
		}
		CharacterJoint[] componentsInChildren = limbT.GetComponentsInChildren<CharacterJoint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		Rigidbody[] componentsInChildren2 = limbT.GetComponentsInChildren<Rigidbody>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[j]);
		}
		Collider[] componentsInChildren3 = limbT.GetComponentsInChildren<Collider>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			UnityEngine.Object.Destroy(componentsInChildren3[k]);
		}
		Transform parent = limbT.parent;
		Transform transform = null;
		if (parent != null)
		{
			transform = SpawnLimbGore(parent, path, restoreState: false);
			transform.localPosition = new Vector3(limbT.localPosition.x * 0.5f, limbT.localPosition.y, limbT.localPosition.z);
			transform.localRotation = limbT.localRotation;
			float num = limbScale;
			if (limbT != head)
			{
				num *= 0.7f;
				Vector3 vector = new Vector3(0f, 0f, 90f);
				if (isLeft)
				{
					vector *= -1f;
				}
				vector += new Vector3(175f, 270f, 45f);
				transform.localEulerAngles += vector;
			}
			else
			{
				transform.localPosition = limbT.localPosition * 0.63f;
				Transform transform2 = new GameObject("scaleTarget").transform;
				transform2.position = limbT.position;
				for (int l = 0; l < limbT.childCount; l++)
				{
					limbT.GetChild(l).SetParent(transform2);
				}
				transform2.SetParent(limbT.parent);
				limbT.SetParent(transform2);
				transform2.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			}
			transform.localScale = limbT.localScale * num;
		}
		limbT.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform SpawnLimbGore(Transform parent, string path, bool restoreState)
	{
		if ((bool)parent)
		{
			GameObject gameObject = (GameObject)Resources.Load(path);
			if (!gameObject)
			{
				Log.Out(entity.EntityName + " SpawnLimbGore prefab not found in resource. path: {0}", path);
				string assetBundlePath = DismembermentManager.GetAssetBundlePath(path);
				GameObject gameObject2 = DataLoader.LoadAsset<GameObject>(assetBundlePath);
				if (!gameObject2)
				{
					Log.Warning(entity.EntityName + " SpawnLimbGore prefab not found in asset bundle. path: {0}", assetBundlePath);
					return null;
				}
				gameObject = gameObject2;
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

	public override void RemoveLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		EnumBodyPartHit bodyPartHit = _bodyDamage.bodyPartHit;
		EnumDamageTypes damageType = _bodyDamage.damageType;
		if (!headDismembered && (bodyPartHit & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
		{
			MakeDismemberedPart(1u, damageType, head, isLeft: false, restoreState);
			headDismembered = true;
		}
		if ((!leftUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None) || (bodyPartHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
		{
			MakeDismemberedPart(32u, damageType, leftUpperLeg, isLeft: true, restoreState);
			leftUpperLegDismembered = true;
		}
		if ((!rightUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None) || (bodyPartHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
		{
			MakeDismemberedPart(128u, damageType, rightUpperLeg, isLeft: false, restoreState);
			rightUpperLegDismembered = true;
		}
		if ((!leftUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None) || (bodyPartHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None)
		{
			MakeDismemberedPart(2u, damageType, leftUpperArm, isLeft: true, restoreState);
			leftUpperArmDismembered = true;
		}
		if ((!rightUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None) || (bodyPartHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None)
		{
			MakeDismemberedPart(8u, damageType, rightUpperArm, isLeft: false, restoreState);
			rightUpperArmDismembered = true;
		}
		if (entity.IsAlive())
		{
			int num = 0;
			if (leftUpperLegDismembered)
			{
				num++;
			}
			if (rightUpperLegDismembered)
			{
				num++;
			}
			if (leftUpperArmDismembered)
			{
				num++;
			}
			if (rightUpperArmDismembered)
			{
				num++;
			}
			if (missingMotorLimbs != num)
			{
				missingMotorLimbs = num;
			}
			float num2 = Mathf.Max(entity.moveSpeedAggroMax, entity.moveSpeedPanicMax) * (1f - (float)num / 5f);
			if (missingMotorLimbs >= 3)
			{
				num2 = 0f;
				entity.lastDamageResponse.Fatal = true;
				entity.Kill(entity.lastDamageResponse);
			}
			entity.moveSpeed = num2;
			entity.moveSpeedAggro = num2;
			entity.moveSpeedPanic = num2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeDismemberedPart(uint bodyDamageFlag, EnumDamageTypes damageType, Transform partT, bool isLeft, bool restoreState)
	{
		DismemberedPartData dismemberedPartData = DismembermentManager.DismemberPart(bodyDamageFlag, damageType, entity, isBiped: true);
		if (dismemberedPartData != null && (bool)partT)
		{
			ProcDismemberedPart(partT, partT, dismemberedPartData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcDismemberedPart(Transform t, Transform partT, DismemberedPartData part)
	{
		if (part.particlePaths != null)
		{
			for (int i = 0; i < part.particlePaths.Length; i++)
			{
				string value = part.particlePaths[i];
				if (!string.IsNullOrEmpty(value))
				{
					DismembermentManager.SpawnParticleEffect(new ParticleEffect(value, t.position + Origin.position, Quaternion.identity, 1f, Color.white));
				}
			}
		}
		else
		{
			DismembermentManager.SpawnParticleEffect(new ParticleEffect("blood_impact", t.position + Origin.position, Quaternion.identity, 1f, Color.white));
		}
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (!bipedT)
		{
			bipedT = entity.emodel.GetModelTransform();
			rightHandT = FindTransform(entity.GetRightHandTransformName());
			assignBodyParts();
			assignStates();
			SetAnimator(bipedT);
			if (entity.RootMotion)
			{
				AvatarRootMotion avatarRootMotion = bipedT.GetComponent<AvatarRootMotion>();
				if (avatarRootMotion == null)
				{
					avatarRootMotion = bipedT.gameObject.AddComponent<AvatarRootMotion>();
				}
				avatarRootMotion.Init(this, anim);
			}
		}
		SetWalkType(entity.GetWalkType());
		_setBool(AvatarController.isDeadHash, entity.IsDead());
		_setBool(AvatarController.isAliveHash, entity.IsAlive());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform FindTransform(string _name)
	{
		return bipedT.FindInChildren(_name);
	}

	public override Transform GetRightHandTransform()
	{
		return rightHandT;
	}

	public override ActionState GetActionState()
	{
		if (attackPlayingTime > 0f)
		{
			return ActionState.Active;
		}
		int tagHash = baseStateInfo.tagHash;
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
		if (!(attackPlayingTime > 0f))
		{
			if (!anim.IsInTransition(0))
			{
				return baseStateInfo.tagHash == AvatarController.attackHash;
			}
			return false;
		}
		return true;
	}

	public override void StartAnimationAttack()
	{
		if (!entity.isEntityRemote)
		{
			attackPlayingTime = 0.5f;
		}
		idleTime = 0f;
		_setTrigger(AvatarController.attackTriggerHash);
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return IsAnimationAttackPlaying();
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return IsAnimationAttackPlaying();
	}

	public override bool IsAnimationRagingPlaying()
	{
		return IsAnimationAttackPlaying();
	}

	public override void SetAlive()
	{
		_setBool(AvatarController.isAliveHash, _value: true);
		_setBool(AvatarController.isDeadHash, _value: false);
		_setTrigger(AvatarController.triggerAliveHash);
	}

	public override void SetVisible(bool _b)
	{
		if (m_bVisible == _b && visInit)
		{
			return;
		}
		m_bVisible = _b;
		visInit = true;
		Transform transform = bipedT;
		if (transform != null)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = _b;
			}
		}
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		if (!CheckHit(_duration))
		{
			SetDataFloat(DataTypes.HitDuration, _duration);
			return;
		}
		idleTime = 0f;
		_setInt(AvatarController.bodyPartHitHash, (int)_bodyPart);
		_setInt(AvatarController.hitDirectionHash, _dir);
		_setInt(AvatarController.hitDamageHash, _hitDamage);
		_setBool(AvatarController.criticalHitHash, _criticalHit);
		_setInt(AvatarController.movementStateHash, _movementState);
		_setInt(AvatarController.randomHash, Mathf.FloorToInt(_random * 100f));
		SetDataFloat(DataTypes.HitDuration, _duration);
		_setTrigger(AvatarController.painTriggerHash);
	}

	public override bool IsAnimationHitRunning()
	{
		if (hitLayerIndex < 0)
		{
			return false;
		}
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(hitLayerIndex);
		if (currentAnimatorStateInfo.tagHash != AvatarController.hitHash || !(currentAnimatorStateInfo.normalizedTime < 0.55f))
		{
			return anim.IsInTransition(hitLayerIndex);
		}
		return true;
	}

	public override void StartAnimationJump(AnimJumpMode jumpMode)
	{
		idleTime = 0f;
		if (!(bipedT == null) && bipedT.gameObject.activeInHierarchy && anim != null)
		{
			if (jumpMode == AnimJumpMode.Start)
			{
				_setTrigger(AvatarController.jumpStartHash);
			}
			else
			{
				_setTrigger(AvatarController.jumpLandHash);
			}
		}
	}

	public override void StartEating()
	{
		if (!isEating)
		{
			_setTrigger(AvatarController.beginCorpseEatHash);
			isEating = true;
		}
	}

	public override void StopEating()
	{
		if (isEating)
		{
			_setTrigger(AvatarController.endCorpseEatHash);
			isEating = false;
		}
	}

	public override void SetSwim(bool _enable)
	{
		_setBool(AvatarController.isSwimHash, _enable);
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float _random)
	{
		isInDeathAnim = true;
		_setBool(AvatarController.isAliveHash, _value: false);
		_setBool(AvatarController.isDeadHash, _value: true);
		_setInt(AvatarController.randomHash, Mathf.FloorToInt(_random * 100f));
		idleTime = 0f;
		_resetTrigger(AvatarController.attackTriggerHash);
		_resetTrigger(AvatarController.painTriggerHash);
		_setTrigger(AvatarController.deathTriggerHash);
	}

	public override Transform GetActiveModelRoot()
	{
		if (!modelT)
		{
			return bipedT;
		}
		return modelT;
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
		if (electrocuteTime > 0.3f && !entity.emodel.IsRagdollActive)
		{
			_setTrigger(AvatarController.electrocuteTriggerHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if ((!m_bVisible && (!entity || !entity.RootMotion || entity.isEntityRemote)) || !bipedT || !bipedT.gameObject.activeInHierarchy || !anim || !anim.enabled)
		{
			return;
		}
		attackPlayingTime -= Time.deltaTime;
		UpdateLayerStateInfo();
		float num = 0f;
		float num2 = 0f;
		if (!entity.IsFlyMode.Value)
		{
			num = entity.speedForward;
			num2 = entity.speedStrafe;
		}
		float num3 = num2;
		if (num3 >= 1234f)
		{
			num3 = 0f;
		}
		_setFloat(AvatarController.forwardHash, num, _netSync: false);
		_setFloat(AvatarController.strafeHash, num3, _netSync: false);
		if (!entity.IsDead())
		{
			float num4 = num * num + num3 * num3;
			_setInt(AvatarController.movementStateHash, (num4 > entity.moveSpeedAggro * entity.moveSpeedAggro) ? 3 : ((num4 > entity.moveSpeed * entity.moveSpeed) ? 2 : ((num4 > 0.001f) ? 1 : 0)), _netsync: false);
		}
		if (IsMoving(num, num2))
		{
			idleTime = 0f;
			_setBool(AvatarController.isMovingHash, _value: true, _netsync: false);
		}
		else
		{
			_setBool(AvatarController.isMovingHash, _value: false, _netsync: false);
		}
		if (isInDeathAnim && baseStateInfo.tagHash == AvatarController.deathHash && baseStateInfo.normalizedTime >= 1f && !anim.IsInTransition(0))
		{
			isInDeathAnim = false;
			if (entity.HasDeathAnim)
			{
				entity.emodel.DoRagdoll(DamageResponse.New(_fatal: true));
			}
		}
		_setFloat(AvatarController.idleTimeHash, idleTime, _netSync: false);
		idleTime += Time.deltaTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLayerStateInfo()
	{
		baseStateInfo = anim.GetCurrentAnimatorStateInfo(0);
	}
}
