using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarNpcController : LegacyAvatarController
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform meshTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform cameraRootTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> hitStates;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo painLayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void assignStates()
	{
		jumpState = Animator.StringToHash("Base Layer.Jump");
		fpvJumpState = Animator.StringToHash("Base Layer.FPVFemaleJump");
		deathStates = new HashSet<int>();
		AvatarCharacterController.GetThirdPersonDeathStates(deathStates);
		reloadStates = new HashSet<int>();
		AvatarCharacterController.GetThirdPersonReloadStates(reloadStates);
		hitStates = new HashSet<int>();
		AvatarCharacterController.GetThirdPersonHitStates(hitStates);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void assignParts(bool _bFPV)
	{
		pelvis = bipedTransform.FindInChilds("Hips");
		spine = pelvis.Find("LowerBack");
		spine1 = spine.Find("Spine");
		spine2 = spine1.Find("Spine1");
		spine3 = spine2.Find("Neck");
		head = spine3.Find("Head");
		cameraNode = head.Find("CameraNode");
		cameraRootTransform = spine3;
		rightHand = bipedTransform.FindInChilds(entity.GetRightHandTransformName());
		meshTransform = bipedTransform.FindInChilds("body");
		if (meshTransform == null)
		{
			meshTransform = bipedTransform.FindInChilds("TraderBob");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		if (anim == null && m_bVisible)
		{
			SetAnimator(GetComponentInChildren<Animator>());
		}
		base.Update();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setLayerWeights()
	{
		if (!(anim != null))
		{
			return;
		}
		if (entity.IsDead())
		{
			anim.SetLayerWeight(1, 0f);
			anim.SetLayerWeight(2, 0f);
			anim.SetLayerWeight(3, 0f);
			return;
		}
		anim.SetLayerWeight(3, 1f);
		if (anim.GetInteger(AvatarController.vehiclePoseHash) >= 0)
		{
			anim.SetLayerWeight(1, 0f);
			anim.SetLayerWeight(2, 0f);
		}
		else
		{
			anim.SetLayerWeight(1, (!(entity.inventory.holdingItem.HoldType == 0) && !AnimationDelayData.AnimationDelay[entity.inventory.holdingItem.HoldType.Value].TwoHanded) ? 1 : 0);
			anim.SetLayerWeight(2, (!(entity.inventory.holdingItem.HoldType == 0) && AnimationDelayData.AnimationDelay[entity.inventory.holdingItem.HoldType.Value].TwoHanded) ? 1 : 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateLayerStateInfo()
	{
		if (anim != null)
		{
			baseStateInfo = anim.GetCurrentAnimatorStateInfo(0);
			currentWeaponHoldLayer = anim.GetCurrentAnimatorStateInfo((!(entity.inventory.holdingItem.HoldType != 0) || !AnimationDelayData.AnimationDelay[entity.inventory.holdingItem.HoldType.Value].TwoHanded) ? 1 : 2);
			painLayer = anim.GetCurrentAnimatorStateInfo(4);
		}
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		Transform transform = modelTransform.Find(_modelName);
		if (transform == null && _bFPV)
		{
			transform = modelTransform.Find(_modelName);
		}
		if (bipedTransform != null && bipedTransform != transform)
		{
			bipedTransform.gameObject.SetActive(value: false);
		}
		bipedTransform = transform;
		bipedTransform.gameObject.SetActive(value: true);
		modelName = _modelName;
		bMale = _bMale;
		bFPV = _bFPV;
		assignParts(bFPV);
		SetAnimator(GetComponentInChildren<Animator>());
		if (!(anim == null))
		{
			_setBool("IsMale", _bMale);
			if (rightHandItemTransform != null)
			{
				rightHandItemTransform.parent = rightHand;
				Vector3 position = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].position;
				Vector3 rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].rotation;
				rightHandItemTransform.localPosition = position;
				rightHandItemTransform.localEulerAngles = rotation;
			}
			SetWalkType(entity.GetWalkType());
			_setBool(AvatarController.isDeadHash, entity.IsDead());
			_setBool(AvatarController.isFPVHash, bFPV);
			_setBool(AvatarController.isAliveHash, entity.IsAlive());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpineRotation()
	{
	}

	public override Transform GetRightHandTransform()
	{
		return rightHand;
	}

	public override Transform GetMeshTransform()
	{
		if (!(meshTransform != null))
		{
			return bipedTransform;
		}
		return meshTransform;
	}

	public override bool IsAnimationHitRunning()
	{
		if (!base.IsAnimationHitRunning())
		{
			return hitStates.Contains(painLayer.fullPathHash);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
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
		if ((baseStateInfo.fullPathHash == jumpState || baseStateInfo.fullPathHash == fpvJumpState) && !anim.IsInTransition(0))
		{
			_setBool("Jump", _value: false);
		}
		if (deathStates.Contains(baseStateInfo.fullPathHash) && !anim.IsInTransition(0))
		{
			_setBool("IsDead", _value: false);
		}
		bool num = anim.IsInTransition(4);
		int integer = anim.GetInteger(AvatarController.hitBodyPartHash);
		bool flag = IsAnimationHitRunning();
		if (!num && integer != 0 && flag)
		{
			_setInt(AvatarController.hitBodyPartHash, 0);
			_setBool("isCritical", _value: false);
		}
		if (anim.GetBool(AvatarController.itemUseHash) && --itemUseTicks <= 0)
		{
			_setBool(AvatarController.itemUseHash, _value: false);
			if (rightHandAnimator != null)
			{
				rightHandAnimator.SetBool(AvatarController.itemUseHash, value: false);
			}
		}
		if (isInDeathAnim && deathStates.Contains(baseStateInfo.fullPathHash) && !anim.IsInTransition(0))
		{
			didDeathTransition = true;
		}
		if (isInDeathAnim && didDeathTransition && (baseStateInfo.normalizedTime >= 1f || anim.IsInTransition(0)))
		{
			isInDeathAnim = false;
			if (entity.HasDeathAnim)
			{
				entity.emodel.DoRagdoll(DamageResponse.New(_fatal: true));
			}
		}
		if (isInDeathAnim && entity.HasDeathAnim && entity.RootMotion && entity.isCollidedHorizontally)
		{
			isInDeathAnim = false;
			entity.emodel.DoRagdoll(DamageResponse.New(_fatal: true));
		}
	}
}
