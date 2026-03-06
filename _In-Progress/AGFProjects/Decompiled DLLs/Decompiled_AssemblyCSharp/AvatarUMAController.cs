using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarUMAController : LegacyAvatarController
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

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bNewModel;

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
		if (!_bFPV)
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
		}
		else
		{
			bNewModel = bipedTransform.FindInChilds("Origin") != null;
			if (!bNewModel)
			{
				pelvis = bipedTransform.Find("Bip001/Bip001 Pelvis");
				spine = pelvis.Find("Bip001 Spine");
				spine1 = spine.Find("Bip001 Spine1");
				spine2 = spine1.Find("Bip001 Spine2");
				spine3 = spine2.Find("Bip001 Spine3");
				head = spine3.Find("Bip001 Neck/Bip001 Head");
				cameraNode = head.Find("CameraNode");
				cameraNode = spine3;
				rightHand = bipedTransform.FindInChilds(entity.GetRightHandTransformName());
			}
			else
			{
				pelvis = null;
				spine = null;
				spine1 = null;
				spine2 = null;
				spine3 = null;
				head = null;
				cameraNode = null;
				cameraRootTransform = spine;
				rightHand = bipedTransform.FindInChilds("Gunjoint");
			}
		}
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
		else if (!anim.IsInTransition(1) && AnimationDelayData.AnimationDelay[entity.inventory.holdingItem.HoldType.Value].TwoHanded)
		{
			anim.SetLayerWeight(1, 0f);
			anim.SetLayerWeight(2, 1f);
		}
		else if (!anim.IsInTransition(2))
		{
			anim.SetLayerWeight(1, 1f);
			anim.SetLayerWeight(2, 0f);
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
		string n = ((!_bFPV) ? _modelName : (_bMale ? "maleArms_fp" : "femaleArms_fp"));
		Transform transform = modelTransform.Find(n);
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
		anim = GetComponentInChildren<Animator>();
		_setBool("IsMale", _bMale);
		if (anim != null)
		{
			anim.logWarnings = false;
			anim.GetBool(AvatarController.isDeadHash);
			anim.GetInteger(AvatarController.weaponHoldTypeHash);
		}
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
		if (anim.GetBool(AvatarController.isDeadHash) && !anim.IsInTransition(0))
		{
			_setBool(AvatarController.isDeadHash, _value: false);
		}
		if (!anim.IsInTransition(2) && anim.GetBool("Reload") && reloadStates.Contains(currentWeaponHoldLayer.fullPathHash) && rightHandAnimator != null)
		{
			rightHandAnimator.SetBool("Reload", value: false);
		}
		bool num = anim.IsInTransition(4);
		int integer = anim.GetInteger(AvatarController.hitBodyPartHash);
		bool flag = IsAnimationHitRunning();
		if (!num && integer != 0 && flag)
		{
			_setInt(AvatarController.hitBodyPartHash, 0);
			_setBool("isCritical", _value: false);
		}
		if (anim != null && anim.GetBool(AvatarController.itemUseHash) && --itemUseTicks <= 0)
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
