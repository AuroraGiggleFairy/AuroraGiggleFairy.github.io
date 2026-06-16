using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionMelee : ItemActionAttack
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class InventoryDataMelee : ItemActionAttackData
	{
		public bool bAttackStarted;

		public Ray ray;

		public bool bHarvesting;

		public bool bFirstHitInARow;

		public InventoryDataMelee(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float rayCastDelay;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new InventoryDataMelee(_invData, _indexInEntityOfAction);
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (isShowOverlay((ItemActionAttackData)_actionData))
		{
			return ItemClass.EnumCrosshairType.Damage;
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		InventoryDataMelee inventoryDataMelee = (InventoryDataMelee)_actionData;
		EntityAlive holdingEntity = inventoryDataMelee.invData.holdingEntity;
		inventoryDataMelee.ray = holdingEntity.GetMeleeRay();
		if (holdingEntity.IsBreakingBlocks)
		{
			if (inventoryDataMelee.ray.direction.y < 0f)
			{
				inventoryDataMelee.ray.direction = new Vector3(inventoryDataMelee.ray.direction.x, 0f, inventoryDataMelee.ray.direction.z);
				inventoryDataMelee.ray.origin += new Vector3(0f, -0.7f, 0f);
			}
		}
		else if (holdingEntity.GetAttackTarget() != null)
		{
			Vector3 direction = holdingEntity.GetAttackTargetHitPosition() - inventoryDataMelee.ray.origin;
			inventoryDataMelee.ray = new Ray(inventoryDataMelee.ray.origin, direction);
		}
		inventoryDataMelee.ray.origin -= 0.15f * inventoryDataMelee.ray.direction;
		int modelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2);
		float distance = Utils.FastMax(Range, BlockRange) + 0.15f;
		if (holdingEntity is EntityEnemy && holdingEntity.IsBreakingBlocks)
		{
			Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, 1073807360, 128, 0.4f);
		}
		else
		{
			EntityAlive entityAlive = null;
			int layerMask = -538767389;
			if (Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, layerMask, 128, SphereRadius))
			{
				entityAlive = ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive;
			}
			if (entityAlive == null)
			{
				Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, -538488845, 128, SphereRadius);
			}
		}
		holdingEntity.SetModelLayer(modelLayer);
		return _actionData.GetUpdatedHitInfo();
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		InventoryDataMelee inventoryDataMelee = (InventoryDataMelee)_actionData;
		if (_bReleased)
		{
			inventoryDataMelee.bFirstHitInARow = true;
		}
		else
		{
			if (Time.time - inventoryDataMelee.lastUseTime < Delay)
			{
				return;
			}
			inventoryDataMelee.lastUseTime = Time.time;
			if (inventoryDataMelee.invData.itemValue.MaxUseTimes > 0 && inventoryDataMelee.invData.itemValue.UseTimes >= (float)inventoryDataMelee.invData.itemValue.MaxUseTimes)
			{
				EntityPlayerLocal localPlayer = _actionData.invData.holdingEntity as EntityPlayerLocal;
				HandleJamSound(_actionData.invData.itemValue, item, localPlayer);
				return;
			}
			_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
			inventoryDataMelee.bHarvesting = checkHarvesting(_actionData, out var _);
			if (inventoryDataMelee.bHarvesting)
			{
				_actionData.invData.holdingEntity.HarvestingAnimation = true;
			}
			string text = soundStart;
			if (text != null)
			{
				_actionData.invData.holdingEntity.PlayOneShot(text);
			}
			inventoryDataMelee.bAttackStarted = true;
			if ((double)inventoryDataMelee.invData.holdingEntity.speedForward > 0.009)
			{
				rayCastDelay = AnimationDelayData.AnimationDelay[inventoryDataMelee.invData.item.HoldType.Value].RayCastMoving;
			}
			else
			{
				rayCastDelay = AnimationDelayData.AnimationDelay[inventoryDataMelee.invData.item.HoldType.Value].RayCast;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isShowOverlay(ItemActionData actionData)
	{
		if (!base.isShowOverlay(actionData))
		{
			return false;
		}
		if (((InventoryDataMelee)actionData).bFirstHitInARow && !(Time.time - actionData.lastUseTime > rayCastDelay))
		{
			return false;
		}
		WorldRayHitInfo executeActionTarget = GetExecuteActionTarget(actionData);
		if (!executeActionTarget.bHitValid)
		{
			return false;
		}
		if (executeActionTarget.tag != null && GameUtils.IsBlockOrTerrain(executeActionTarget.tag) && executeActionTarget.hit.distanceSq > BlockRange * BlockRange)
		{
			return false;
		}
		if (executeActionTarget.tag != null && executeActionTarget.tag.StartsWith("E_") && executeActionTarget.hit.distanceSq > Range * Range)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkHarvesting(ItemActionData _actionData, out AttackHitInfo myAttackHitInfo)
	{
		WorldRayHitInfo executeActionTarget = GetExecuteActionTarget(_actionData);
		ItemValue itemValue = _actionData.invData.itemValue;
		bool flag = false;
		float num = GetDamageEntity(itemValue, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction);
		if (_actionData.invData.holdingEntity is EntityPlayer)
		{
			num *= ItemActionAttack.MeleeDamagePercent;
			flag = ItemActionAttack.MeleeDamagePercent == 0f;
		}
		else
		{
			num *= ItemActionAttack.EntityPlayerDamagePercent;
			flag = ItemActionAttack.EntityPlayerDamagePercent == 0f;
		}
		myAttackHitInfo = new AttackHitInfo
		{
			WeaponTypeTag = ItemActionAttack.MeleeTag
		};
		ItemActionAttack.Hit(executeActionTarget, _actionData.invData.holdingEntity.entityId, (DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Bashing : DamageType, GetDamageBlock(itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, executeActionTarget), _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction), num, 1f, 1f, 0f, ItemAction.GetDismemberChance(_actionData, executeActionTarget), item.MadeOfMaterial.id, damageMultiplier, getBuffActions(_actionData), myAttackHitInfo, 1, ActionExp, ActionExpBonusMultiplier, this, ToolBonuses, EnumAttackMode.Simulate, null, -1, null, _isGrazingHit: false, flag);
		if (myAttackHitInfo.bKilled)
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

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		InventoryDataMelee inventoryDataMelee = (InventoryDataMelee)_actionData;
		if (Time.time - inventoryDataMelee.lastUseTime < Delay + 0.1f)
		{
			return true;
		}
		return false;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		InventoryDataMelee inventoryDataMelee = (InventoryDataMelee)_actionData;
		if (!inventoryDataMelee.bAttackStarted || Time.time - inventoryDataMelee.lastUseTime < rayCastDelay)
		{
			return;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (rayCastDelay <= 0f && !holdingEntity.IsAttackImpact())
		{
			return;
		}
		inventoryDataMelee.bAttackStarted = false;
		ItemActionAttackData.HitDelegate hitDelegate = inventoryDataMelee.hitDelegate;
		inventoryDataMelee.hitDelegate = null;
		if (!holdingEntity.IsAttackValid())
		{
			return;
		}
		float num = EffectManager.GetValue(PassiveEffects.StaminaLoss, inventoryDataMelee.invData.itemValue, 0f, holdingEntity, null, (_actionData.indexInEntityOfAction == 0) ? FastTags<TagGroup.Global>.Parse("primary") : FastTags<TagGroup.Global>.Parse("secondary")) * ItemActionAttack.StaminaUsageMultiplier;
		holdingEntity.AddStamina(0f - num);
		float damageScale = 1f;
		WorldRayHitInfo worldRayHitInfo = ((hitDelegate == null) ? GetExecuteActionTarget(_actionData) : hitDelegate(out damageScale));
		if (worldRayHitInfo != null && worldRayHitInfo.bHitValid && (worldRayHitInfo.tag == null || !GameUtils.IsBlockOrTerrain(worldRayHitInfo.tag) || !(worldRayHitInfo.hit.distanceSq > BlockRange * BlockRange)) && (worldRayHitInfo.tag == null || !worldRayHitInfo.tag.StartsWith("E_") || !(worldRayHitInfo.hit.distanceSq > Range * Range)))
		{
			if (inventoryDataMelee.invData.itemValue.MaxUseTimes > 0)
			{
				_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, inventoryDataMelee.invData.itemValue, 1f, holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags) * ItemAction.ItemDegradationModifier;
				HandleItemBreak(_actionData);
			}
			if (ItemAction.ShowDebugDisplayHit)
			{
				DebugLines.Create("MeleeHit", holdingEntity.RootTransform, holdingEntity.position, worldRayHitInfo.hit.pos, new Color(0.7f, 0f, 0f), new Color(1f, 1f, 0f), 0.05f, 0.02f, 1f);
			}
			hitTheTarget(inventoryDataMelee, worldRayHitInfo, damageScale);
			if (inventoryDataMelee.bFirstHitInARow)
			{
				inventoryDataMelee.bFirstHitInARow = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void hitTheTarget(InventoryDataMelee _actionData, WorldRayHitInfo hitInfo, float damageScale)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = _actionData.invData.itemValue;
		bool flag = false;
		float weaponCondition = 1f;
		if (itemValue.MaxUseTimes > 0)
		{
			weaponCondition = ((float)itemValue.MaxUseTimes - itemValue.UseTimes) / (float)itemValue.MaxUseTimes;
		}
		_actionData.attackDetails.WeaponTypeTag = ItemActionAttack.MeleeTag;
		int num = 1;
		if (bUseParticleHarvesting && (particleHarvestingCategory == null || particleHarvestingCategory == item.MadeOfMaterial.id))
		{
			num |= 4;
		}
		float blockDamage = GetDamageBlock(itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, hitInfo), holdingEntity, _actionData.indexInEntityOfAction) * damageScale;
		float num2 = GetDamageEntity(itemValue, holdingEntity, _actionData.indexInEntityOfAction);
		if (holdingEntity is EntityPlayer)
		{
			num2 *= ItemActionAttack.MeleeDamagePercent;
			flag = ItemActionAttack.MeleeDamagePercent == 0f;
		}
		else
		{
			num2 *= ItemActionAttack.EntityPlayerDamagePercent;
			flag = ItemActionAttack.EntityPlayerDamagePercent == 0f;
		}
		ItemActionAttack.Hit(hitInfo, holdingEntity.entityId, (DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Bashing : DamageType, blockDamage, num2, holdingEntity.Stats.Stamina?.ValuePercent ?? 1f, weaponCondition, 0f, ItemAction.GetDismemberChance(_actionData, hitInfo), item.MadeOfMaterial.SurfaceCategory, damageMultiplier, getBuffActions(_actionData), _actionData.attackDetails, num, ActionExp, ActionExpBonusMultiplier, this, ToolBonuses, _actionData.bHarvesting ? EnumAttackMode.RealAndHarvesting : EnumAttackMode.RealNoHarvesting, null, -1, null, _isGrazingHit: false, flag);
		GameUtils.HarvestOnAttack(_actionData, ToolBonuses);
	}
}
