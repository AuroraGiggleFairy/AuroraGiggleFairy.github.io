using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionDynamicMelee : ItemActionDynamic
{
	public class ItemActionDynamicMeleeData(ItemInventoryData _invData, int _indexInEntityOfAction) : ItemActionDynamicData(_invData, _indexInEntityOfAction)
	{
		public float StaminaUsage;

		public bool Attacking;

		public bool HasReleased = true;

		public bool HasFinished = true;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionDynamicMeleeData itemActionDynamicMeleeData = _actionData as ItemActionDynamicMeleeData;
		if (!_bReleased)
		{
			if (!canStartAttack(itemActionDynamicMeleeData))
			{
				itemActionDynamicMeleeData.HasReleased = false;
				return;
			}
			if (itemActionDynamicMeleeData.HasExecuted)
			{
				SetAttackFinished(itemActionDynamicMeleeData);
				itemActionDynamicMeleeData.HasExecuted = false;
			}
			itemActionDynamicMeleeData.lastUseTime = Time.time;
			itemActionDynamicMeleeData.lastWeaponHeadPosition = Vector3.zero;
			itemActionDynamicMeleeData.lastWeaponHeadPositionDebug = Vector3.zero;
			itemActionDynamicMeleeData.lastClipPercentage = -1f;
			itemActionDynamicMeleeData.alreadyHitEnts.Clear();
			itemActionDynamicMeleeData.alreadyHitBlocks.Clear();
			itemActionDynamicMeleeData.EventParms.Self = itemActionDynamicMeleeData.invData.holdingEntity;
			itemActionDynamicMeleeData.EventParms.Other = null;
			itemActionDynamicMeleeData.EventParms.ItemActionData = itemActionDynamicMeleeData;
			itemActionDynamicMeleeData.EventParms.ItemValue = itemActionDynamicMeleeData.invData.itemValue;
			_actionData.invData.holdingEntity.MinEventContext.Other = null;
			for (int i = 0; i < ItemActionDynamic.DebugDisplayHits.Count; i++)
			{
				Object.DestroyImmediate(ItemActionDynamic.DebugDisplayHits[i]);
			}
			ItemActionDynamic.DebugDisplayHits.Clear();
			itemActionDynamicMeleeData.IsHarvesting = checkHarvesting(_actionData, out var _);
			EntityAlive holdingEntity = _actionData.invData.holdingEntity;
			AvatarController avatarController = holdingEntity.emodel.avatarController;
			avatarController.UpdateBool(AvatarController.harvestingHash, itemActionDynamicMeleeData.IsHarvesting);
			avatarController.UpdateBool("IsHarvesting", itemActionDynamicMeleeData.IsHarvesting);
			avatarController.UpdateInt(AvatarController.itemActionIndexHash, itemActionDynamicMeleeData.indexInEntityOfAction);
			string text = soundStart;
			if (text != null && !itemActionDynamicMeleeData.IsHarvesting)
			{
				holdingEntity.PlayOneShot(text);
			}
			if (UsePowerAttackAnimation)
			{
				avatarController.TriggerEvent("PowerAttack");
			}
			else if (!itemActionDynamicMeleeData.IsHarvesting)
			{
				holdingEntity.RightArmAnimationAttack = true;
			}
			if (itemActionDynamicMeleeData.IsHarvesting)
			{
				holdingEntity.StartHarvestingAnim(HarvestLength, !UsePowerAttackAnimation);
			}
			if (!UsePowerAttackTriggers)
			{
				holdingEntity.FireEvent(MinEventTypes.onSelfPrimaryActionStart);
			}
			else
			{
				holdingEntity.FireEvent(MinEventTypes.onSelfSecondaryActionStart);
			}
			if (holdingEntity is EntityPlayerLocal entityPlayerLocal && entityPlayerLocal.movementInput.lastInputController)
			{
				entityPlayerLocal.MoveController.FindCameraSnapTarget(eCameraSnapMode.MeleeAttack, Range + 1f);
			}
			itemActionDynamicMeleeData.Attacking = true;
			itemActionDynamicMeleeData.HasExecuted = true;
		}
		else
		{
			itemActionDynamicMeleeData.HasReleased = true;
			SetAttackFinished(itemActionDynamicMeleeData);
			itemActionDynamicMeleeData.HasExecuted = false;
			itemActionDynamicMeleeData.HasFinished = true;
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		bool result = false;
		if (_actionData is ItemActionDynamicMeleeData itemActionDynamicMeleeData)
		{
			result = itemActionDynamicMeleeData.IsHarvesting || (itemActionDynamicMeleeData.invData.holdingEntity.emodel.avatarController != null && itemActionDynamicMeleeData.invData.holdingEntity.emodel.avatarController.IsAnimationHarvestingPlaying()) || itemActionDynamicMeleeData.Attacking || !itemActionDynamicMeleeData.HasFinished;
		}
		return result;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionDynamicMeleeData itemActionDynamicMeleeData = _actionData as ItemActionDynamicMeleeData;
		if (!itemActionDynamicMeleeData.Attacking)
		{
			return;
		}
		FastTags<TagGroup.Global> tags = ((_actionData.indexInEntityOfAction == 0) ? FastTags<TagGroup.Global>.Parse("primary") : FastTags<TagGroup.Global>.Parse("secondary"));
		ItemValue itemValue = _actionData.invData.itemValue;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			tags |= itemClass.ItemTags;
		}
		float num = EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, 60f, _actionData.invData.holdingEntity, null, tags);
		if (num == 0f)
		{
			num = 0.0001f;
		}
		float num2 = (itemActionDynamicMeleeData.attackTime = 60f / num);
		if (Time.time - itemActionDynamicMeleeData.lastUseTime > num2 + 0.1f)
		{
			SetAttackFinished(_actionData);
		}
		else if (itemActionDynamicMeleeData.invData == null || itemActionDynamicMeleeData.invData.holdingEntity == null || itemActionDynamicMeleeData.invData.holdingEntity.emodel == null)
		{
			SetAttackFinished(_actionData);
		}
		else if (itemActionDynamicMeleeData.invData.holdingEntity.emodel.avatarController == null || itemActionDynamicMeleeData.invData.holdingEntity.IsDead())
		{
			SetAttackFinished(_actionData);
		}
		else if (UseGrazingHits && itemActionDynamicMeleeData.invData.holdingEntity as EntityPlayerLocal != null)
		{
			float num3 = (Time.time - itemActionDynamicMeleeData.lastUseTime) / num2;
			if (num3 > GrazeStart - 0.1f && num3 < GrazeEnd + 0.1f)
			{
				GrazeCast(itemActionDynamicMeleeData, num3);
			}
		}
	}

	public void SetAttackFinished(ItemActionData _actionData)
	{
		if (!(_actionData is ItemActionDynamicMeleeData itemActionDynamicMeleeData))
		{
			return;
		}
		if (itemActionDynamicMeleeData.Attacking)
		{
			if (!UsePowerAttackTriggers)
			{
				itemActionDynamicMeleeData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);
			}
			else
			{
				itemActionDynamicMeleeData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfSecondaryActionEnd);
			}
			if (_actionData.invData.holdingEntity.MinEventContext.Other == null)
			{
				_actionData.invData.holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionMissEntity : MinEventTypes.onSelfSecondaryActionMissEntity);
			}
		}
		itemActionDynamicMeleeData.Attacking = false;
		itemActionDynamicMeleeData.IsHarvesting = false;
		itemActionDynamicMeleeData.HasFinished = true;
	}

	public override bool GrazeCast(ItemActionDynamicData _actionData, float normalizedClipTime = -1f)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (holdingEntity is EntityVehicle)
		{
			return false;
		}
		WorldRayHitInfo[] executeActionGrazeTarget = GetExecuteActionGrazeTarget(_actionData, normalizedClipTime);
		bool result = false;
		for (int i = 0; i < executeActionGrazeTarget.Length; i++)
		{
			float num = (_actionData as ItemActionDynamicMeleeData).StaminaUsage * EffectManager.GetValue(PassiveEffects.GrazeStaminaMultiplier, _actionData.invData.holdingEntity.inventory.holdingItemItemValue, GrazeStaminaPercentage, _actionData.invData.holdingEntity, null, _actionData.invData.holdingEntity.inventory.holdingItem.ItemTags);
			if (!(holdingEntity.Stats.Stamina.Value < num))
			{
				if (holdingEntity as EntityPlayerLocal != null)
				{
					holdingEntity.Stats.Stamina.Value -= num;
				}
				hitTarget(_actionData, executeActionGrazeTarget[i], _isGrazingHit: true);
				result = true;
			}
		}
		return result;
	}

	public override bool Raycast(ItemActionDynamicData _actionData)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (holdingEntity is EntityVehicle)
		{
			return false;
		}
		if (holdingEntity as EntityPlayerLocal != null)
		{
			holdingEntity.Stats.Stamina.Value -= (_actionData as ItemActionDynamicMeleeData).StaminaUsage;
		}
		_actionData.waterCollisionParticles.Reset();
		int num = 1;
		ItemValue itemValue = _actionData.invData.itemValue;
		FastTags<TagGroup.Global> tags = ((itemValue.ItemClass != null) ? itemValue.ItemClass.ItemTags : FastTags<TagGroup.Global>.none);
		num += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, itemValue, EntityPenetrationCount, holdingEntity, null, tags));
		int num2 = 0;
		int num3 = 0;
		EntityAlive entityAlive = null;
		do
		{
			WorldRayHitInfo executeActionTarget = GetExecuteActionTarget(_actionData);
			_actionData.waterCollisionParticles.CheckCollision(_actionData.ray.origin, _actionData.ray.direction, Utils.FastMax(Range, BlockRange) + SphereRadius, holdingEntity.entityId);
			if (!isHitValid(executeActionTarget, _actionData, out var _hitEntity))
			{
				break;
			}
			if (!_hitEntity || _hitEntity != entityAlive)
			{
				entityAlive = _hitEntity;
				if (ItemAction.ShowDebugDisplayHit)
				{
					DebugLines.Create(null, holdingEntity.RootTransform, holdingEntity.position, executeActionTarget.hit.pos, new Color(0.7f, 0f, 0f), new Color(1f, 1f, 0f), 0.05f, 0.02f, 2f);
				}
				hitTarget(_actionData, executeActionTarget);
				num2++;
			}
			if (++num3 >= 20 || !_hitEntity)
			{
				break;
			}
			_actionData.ray.origin = _actionData.hitInfo.hit.pos + _actionData.ray.direction * 0.1f;
			_actionData.useExistingRay = true;
		}
		while (num2 < num);
		AvatarController avatarController = holdingEntity.emodel.avatarController;
		if (num2 == 0)
		{
			if ((bool)avatarController)
			{
				avatarController.UpdateBool("RayHit", _value: false);
				holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionRayMiss : MinEventTypes.onSelfSecondaryActionRayMiss);
			}
			return false;
		}
		if ((bool)avatarController)
		{
			avatarController.UpdateBool("RayHit", _value: true);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkHarvesting(ItemActionData _actionData, out ItemActionAttack.AttackHitInfo myAttackHitInfo)
	{
		WorldRayHitInfo executeActionTarget = GetExecuteActionTarget(_actionData);
		_ = _actionData.invData.itemValue;
		myAttackHitInfo = new ItemActionAttack.AttackHitInfo
		{
			WeaponTypeTag = ItemActionAttack.MeleeTag
		};
		ItemActionAttack.Hit(executeActionTarget, _actionData.invData.holdingEntity.entityId, (DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Bashing : DamageType, GetDamageBlock(_actionData.invData.itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, executeActionTarget), _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction), GetDamageEntity(_actionData.invData.itemValue, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction), 1f, 1f, 0f, ItemAction.GetDismemberChance(_actionData, executeActionTarget), item.MadeOfMaterial.id, new DamageMultiplier(), new List<string>(), myAttackHitInfo, 1, ActionExp, ActionExpBonusMultiplier, null, ToolBonuses, ItemActionAttack.EnumAttackMode.Simulate);
		if (myAttackHitInfo.bKilled)
		{
			return false;
		}
		if (Voxel.voxelRayHitInfo.tag.StartsWith("E_") && executeActionTarget.hit.distanceSq > Range * Range)
		{
			return false;
		}
		if (executeActionTarget.hit.distanceSq > BlockRange * BlockRange)
		{
			return false;
		}
		if (myAttackHitInfo.itemsToDrop != null && myAttackHitInfo.itemsToDrop.ContainsKey(EnumDropEvent.Harvest))
		{
			List<Block.SItemDropProb> list = myAttackHitInfo.itemsToDrop[EnumDropEvent.Harvest];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].toolCategory != null && ToolBonuses != null && ToolBonuses.ContainsKey(list[i].toolCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canStartAttack(ItemActionDynamicMeleeData _actionData)
	{
		if (_actionData.Attacking)
		{
			return false;
		}
		if (_actionData.invData.holdingEntity is EntityPlayerLocal { bFirstPersonView: false } entityPlayerLocal)
		{
			entityPlayerLocal.StartTPCameraLockTimer();
			if (!CharacterCameraAngleValid(entityPlayerLocal, out var _result) && _result != eTPCameraCheckResult.LineOfSightCheckFailed)
			{
				return false;
			}
		}
		FastTags<TagGroup.Global> tags = ((_actionData.indexInEntityOfAction == 0) ? FastTags<TagGroup.Global>.Parse("primary") : FastTags<TagGroup.Global>.Parse("secondary"));
		ItemClass itemClass = _actionData.invData.itemValue.ItemClass;
		if (itemClass != null)
		{
			tags |= itemClass.ItemTags;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		float num = EffectManager.GetValue(PassiveEffects.AttacksPerMinute, _actionData.invData.itemValue, 60f, holdingEntity, null, tags);
		if (num == 0f)
		{
			num = 0.0001f;
		}
		num = 60f / num;
		if (Time.time - _actionData.lastUseTime < num + 0.1f)
		{
			return false;
		}
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags) > 0f)
		{
			_actionData.lastUseTime = Time.time;
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			return false;
		}
		if (_actionData.invData.itemValue.PercentUsesLeft == 0f)
		{
			if (_actionData.HasReleased)
			{
				EntityPlayerLocal player = holdingEntity as EntityPlayerLocal;
				if (item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
				{
					Manager.PlayInsidePlayerHead(item.Properties.Values[ItemClass.PropSoundJammed]);
				}
				GameManager.ShowTooltip(player, "ttItemNeedsRepair");
			}
			return false;
		}
		if (holdingEntity.Stats.Stamina != null)
		{
			_actionData.StaminaUsage = EffectManager.GetValue(PassiveEffects.StaminaLoss, _actionData.invData.itemValue, 2f, holdingEntity, null, _actionData.ActionTags);
			if (holdingEntity.Stats.Stamina.Value < _actionData.StaminaUsage)
			{
				if (_actionData.HasReleased)
				{
					Manager.Play(holdingEntity, holdingEntity.IsMale ? "player1stamina" : "player2stamina");
					GameManager.ShowTooltip(holdingEntity as EntityPlayerLocal, "ttOutOfStamina");
				}
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canContinueAttack(ItemActionDynamicMeleeData _actionData)
	{
		return _actionData.invData.holdingEntity.IsAttackValid();
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDynamicMeleeData(_invData, _indexInEntityOfAction);
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (!CharacterCameraAngleValid(_actionData, out var _))
		{
			return ItemClass.EnumCrosshairType.None;
		}
		if (isShowOverlay(_actionData))
		{
			return ItemClass.EnumCrosshairType.Damage;
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		SetAttackFinished(_data);
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(_data.invData.holdingEntity as EntityPlayerLocal);
		if (uIForPlayer != null && XUiC_FocusedBlockHealth.IsWindowOpen(uIForPlayer))
		{
			XUiC_FocusedBlockHealth.SetData(uIForPlayer, null, 0f);
			((ItemActionDynamicMeleeData)_data).uiOpenedByMe = false;
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		SetAttackFinished(_data);
	}
}
