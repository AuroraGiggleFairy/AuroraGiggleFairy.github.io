using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionDynamic : ItemAction
{
	public class ItemActionDynamicData : ItemActionAttackData
	{
		public Ray ray;

		public Vector3 rayStartPos;

		public bool useExistingRay;

		public bool IsHarvesting;

		public List<int> alreadyHitEnts;

		public List<Vector3i> alreadyHitBlocks;

		public Vector3 lastWeaponHeadPosition = Vector3.zero;

		public Vector3 lastWeaponHeadPositionDebug = Vector3.zero;

		public float lastClipPercentage = -1f;

		public float attackTime;

		public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

		public ItemActionDynamicData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
			alreadyHitEnts = new List<int>();
			alreadyHitBlocks = new List<Vector3i>();
			waterCollisionParticles.Init(_invData.holdingEntity.entityId, _invData.item.MadeOfMaterial.SurfaceCategory, "water", 16);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumDamageTypes DamageType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, ItemActionAttack.Bonuses> ToolBonuses = new Dictionary<string, ItemActionAttack.Bonuses>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, string> HitSoundOverrides;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, string> GrazeSoundOverrides;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int lastModelLayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float RangeDefault;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new float Range;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new float BlockRange;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool UsePowerAttackAnimation;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool UsePowerAttackTriggers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int hitmaskOverride;

	public bool UseGrazingHits;

	public float GrazeStart;

	public float GrazeEnd;

	public float GrazeDamagePercentage;

	public float GrazeStaminaPercentage;

	public bool IsVerticalSwing;

	public bool IsHorizontalSwing;

	public bool InvertSwing;

	public float SwingDegrees;

	public float SwingAngle;

	public int EntityPenetrationCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool harvestHitEffectOn;

	public float HarvestLength = 0.3f;

	public static bool ShowDebugSwing = false;

	public static List<GameObject> DebugDisplayHits = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tmpTag;

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		foreach (KeyValuePair<string, string> item in _props.Values.Dict)
		{
			if (item.Key.StartsWith("ToolCategory."))
			{
				ToolBonuses[item.Key.Substring("ToolCategory.".Length)] = new ItemActionAttack.Bonuses(StringParsers.ParseFloat(_props.Values[item.Key]), _props.Params1.ContainsKey(item.Key) ? StringParsers.ParseFloat(_props.Params1[item.Key]) : 2f);
			}
		}
		if (_props.Values.ContainsKey("Damage_type"))
		{
			DamageType = EnumUtils.Parse<EnumDamageTypes>(_props.Values["Damage_type"]);
		}
		else if (_props.Values.ContainsKey("DamageType"))
		{
			DamageType = EnumUtils.Parse<EnumDamageTypes>(_props.Values["DamageType"]);
		}
		else
		{
			DamageType = EnumDamageTypes.Bashing;
		}
		RangeDefault = 2f;
		_props.ParseFloat("Range", ref RangeDefault);
		UsePowerAttackAnimation = ActionIndex == 1;
		UsePowerAttackTriggers = ActionIndex == 1;
		if (_props.Values.ContainsKey("UsePowerAttackAnimation"))
		{
			UsePowerAttackAnimation = StringParsers.ParseBool(_props.Values["UsePowerAttackAnimation"]);
		}
		if (_props.Values.ContainsKey("UsePowerAttackTriggers"))
		{
			UsePowerAttackTriggers = StringParsers.ParseBool(_props.Values["UsePowerAttackTriggers"]);
		}
		if (_props.Values.ContainsKey("UseGrazingHits"))
		{
			UseGrazingHits = StringParsers.ParseBool(_props.Values["UseGrazingHits"]);
		}
		else
		{
			UseGrazingHits = false;
		}
		if (_props.Values.ContainsKey("GrazeDamagePercentage"))
		{
			GrazeDamagePercentage = StringParsers.ParseFloat(_props.Values["GrazeDamagePercentage"]);
		}
		else
		{
			GrazeDamagePercentage = 0.1f;
		}
		if (_props.Values.ContainsKey("GrazeStaminaPercentage"))
		{
			GrazeStaminaPercentage = StringParsers.ParseFloat(_props.Values["GrazeStaminaPercentage"]);
		}
		else
		{
			GrazeStaminaPercentage = 0.01f;
		}
		if (_props.Values.ContainsKey("Sphere"))
		{
			SphereRadius = StringParsers.ParseFloat(_props.Values["Sphere"]);
		}
		else
		{
			SphereRadius = 0f;
		}
		if (_props.Values.ContainsKey("GrazeStart"))
		{
			GrazeStart = StringParsers.ParseFloat(_props.Values["GrazeStart"]);
		}
		else
		{
			GrazeStart = -0.15f;
		}
		if (_props.Values.ContainsKey("GrazeEnd"))
		{
			GrazeEnd = StringParsers.ParseFloat(_props.Values["GrazeEnd"]);
		}
		else
		{
			GrazeEnd = 0.15f;
		}
		if (_props.Values.ContainsKey("IsVerticalSwing"))
		{
			IsVerticalSwing = StringParsers.ParseBool(_props.Values["IsVerticalSwing"]);
		}
		else
		{
			IsVerticalSwing = false;
		}
		if (_props.Values.ContainsKey("IsHorizontalSwing"))
		{
			IsHorizontalSwing = StringParsers.ParseBool(_props.Values["IsHorizontalSwing"]);
		}
		else
		{
			IsHorizontalSwing = false;
		}
		if (_props.Values.ContainsKey("InvertSwing"))
		{
			InvertSwing = StringParsers.ParseBool(_props.Values["InvertSwing"]);
		}
		else
		{
			InvertSwing = false;
		}
		if (_props.Values.ContainsKey("SwingDegrees"))
		{
			SwingDegrees = StringParsers.ParseFloat(_props.Values["SwingDegrees"]);
		}
		else
		{
			SwingDegrees = 65f;
		}
		if (_props.Values.ContainsKey("SwingAngle"))
		{
			SwingAngle = StringParsers.ParseFloat(_props.Values["SwingAngle"]);
		}
		else
		{
			SwingAngle = 0f;
		}
		_props.ParseInt("EntityPenetrationCount", ref EntityPenetrationCount);
		harvestHitEffectOn = true;
		_props.ParseBool("HarvestHitEffectOn", ref harvestHitEffectOn);
		if (_props.Classes.ContainsKey("HitSounds"))
		{
			HitSoundOverrides = new Dictionary<string, string>();
			for (int i = 0; i < 10; i++)
			{
				string text = "Override" + i;
				if (_props.Classes["HitSounds"].Values.ContainsKey(text) && _props.Classes["HitSounds"].Params1.ContainsKey(text))
				{
					HitSoundOverrides[_props.Classes["HitSounds"].Values[text]] = _props.Classes["HitSounds"].Params1[text];
				}
			}
		}
		if (_props.Classes.ContainsKey("GrazeSounds"))
		{
			GrazeSoundOverrides = new Dictionary<string, string>();
			for (int j = 0; j < 10; j++)
			{
				string text2 = "Override" + j;
				if (_props.Classes["GrazeSounds"].Values.ContainsKey(text2) && _props.Classes["GrazeSounds"].Params1.ContainsKey(text2))
				{
					GrazeSoundOverrides[_props.Classes["GrazeSounds"].Values[text2]] = _props.Classes["GrazeSounds"].Params1[text2];
				}
			}
		}
		if (_props.Values.ContainsKey("HarvestLength"))
		{
			HarvestLength = StringParsers.ParseFloat(_props.Values["HarvestLength"]);
		}
		hitmaskOverride = Voxel.ToHitMask(_props.GetString("Hitmask_override"));
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator harvestOnCompletion(ItemActionDynamicMelee.ItemActionDynamicMeleeData dmd, Action callback)
	{
		if (dmd != null)
		{
			while (!dmd.HasFinished)
			{
				yield return null;
			}
		}
		callback();
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return false;
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDynamicData(_invData, _indexInEntityOfAction);
	}

	public virtual bool GrazeCast(ItemActionDynamicData _actionData, float normalizedClipTime = -1f)
	{
		return false;
	}

	public virtual bool Raycast(ItemActionDynamicData _actionData)
	{
		return false;
	}

	public override WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		float sphereRadius = SphereRadius;
		ItemActionDynamicData itemActionDynamicData = (ItemActionDynamicData)_actionData;
		EntityAlive holdingEntity = itemActionDynamicData.invData.holdingEntity;
		if (!itemActionDynamicData.useExistingRay)
		{
			itemActionDynamicData.ray = holdingEntity.GetMeleeRay();
			if (holdingEntity.IsBreakingBlocks)
			{
				if (itemActionDynamicData.ray.direction.y < 0f)
				{
					itemActionDynamicData.ray.direction = new Vector3(itemActionDynamicData.ray.direction.x, 0f, itemActionDynamicData.ray.direction.z);
					itemActionDynamicData.ray.origin += new Vector3(0f, -0.7f, 0f);
				}
			}
			else if (holdingEntity.GetAttackTarget() != null)
			{
				Vector3 direction = holdingEntity.GetAttackTargetHitPosition() - itemActionDynamicData.ray.origin;
				itemActionDynamicData.ray = new Ray(itemActionDynamicData.ray.origin, direction);
			}
			itemActionDynamicData.ray.origin -= sphereRadius * itemActionDynamicData.ray.direction;
			itemActionDynamicData.rayStartPos = itemActionDynamicData.ray.origin;
		}
		itemActionDynamicData.useExistingRay = false;
		lastModelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2);
		ItemValue itemValue = itemActionDynamicData.invData.itemValue;
		ItemClass holdingItem = holdingEntity.inventory.holdingItem;
		FastTags<TagGroup.Global> actionTags = _actionData.ActionTags;
		actionTags |= holdingItem?.ItemTags ?? ItemActionAttack.MeleeTag;
		Range = EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, RangeDefault, holdingEntity, null, actionTags);
		BlockRange = EffectManager.GetValue(PassiveEffects.BlockRange, itemValue, Range, holdingEntity, null, actionTags);
		float num = Utils.FastMax(Range, BlockRange) + sphereRadius;
		num -= (itemActionDynamicData.ray.origin - itemActionDynamicData.rayStartPos).magnitude;
		if (holdingEntity is EntityEnemy && holdingEntity.IsBreakingBlocks)
		{
			Voxel.Raycast(itemActionDynamicData.invData.world, itemActionDynamicData.ray, num, 1073807360, (hitmaskOverride == 0) ? 128 : hitmaskOverride, 0.4f);
		}
		else
		{
			EntityAlive entityAlive = null;
			int layerMask = -538767381;
			if (Voxel.Raycast(itemActionDynamicData.invData.world, itemActionDynamicData.ray, num, layerMask, (hitmaskOverride == 0) ? 128 : hitmaskOverride, SphereRadius))
			{
				entityAlive = ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive;
			}
			if (entityAlive == null)
			{
				Voxel.Raycast(itemActionDynamicData.invData.world, itemActionDynamicData.ray, num, -538488837, (hitmaskOverride == 0) ? 128 : hitmaskOverride, SphereRadius);
			}
		}
		holdingEntity.SetModelLayer(lastModelLayer);
		return _actionData.GetUpdatedHitInfo();
	}

	public WorldRayHitInfo[] GetExecuteActionGrazeTarget(ItemActionData _actionData, float normalizedClipTime = -1f)
	{
		List<WorldRayHitInfo> list = new List<WorldRayHitInfo>();
		normalizedClipTime = Mathf.Clamp((normalizedClipTime - GrazeStart) / (GrazeEnd - GrazeStart), 0f, 1f);
		float num = SphereRadius * 1.25f;
		if (num == 0f)
		{
			num = 0.15f;
		}
		float num2 = num;
		float b = 0f - SwingDegrees * 0.5f;
		float a = SwingDegrees * 0.5f;
		ItemActionDynamicData itemActionDynamicData = (ItemActionDynamicData)_actionData;
		EntityAlive holdingEntity = itemActionDynamicData.invData.holdingEntity;
		EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
		Ray meleeRay = itemActionDynamicData.invData.holdingEntity.GetMeleeRay();
		meleeRay.direction = Quaternion.AngleAxis(Mathf.Lerp(a, b, normalizedClipTime), (holdingEntity as EntityPlayerLocal).cameraTransform.right) * meleeRay.direction;
		if (SwingAngle != 0f)
		{
			meleeRay.direction = Quaternion.AngleAxis(SwingAngle, (holdingEntity as EntityPlayerLocal).cameraTransform.forward) * meleeRay.direction;
		}
		float num3 = EffectManager.GetValue(PassiveEffects.MaxRange, itemActionDynamicData.invData.itemValue, 2f, holdingEntity, null, itemActionDynamicData.ActionTags) + num2;
		meleeRay.origin -= meleeRay.direction * num2;
		Vector3 vector = meleeRay.origin + meleeRay.direction * num3;
		if (itemActionDynamicData.lastWeaponHeadPosition == Vector3.zero)
		{
			itemActionDynamicData.lastWeaponHeadPosition = vector;
			return list.ToArray();
		}
		float num4 = Vector3.Distance(vector, itemActionDynamicData.lastWeaponHeadPosition);
		Vector3 normalized = (vector - itemActionDynamicData.lastWeaponHeadPosition).normalized;
		_ = itemActionDynamicData.invData.holdingEntity.GetMeleeRay().direction;
		lastModelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2);
		Entity entity = null;
		Ray meleeRay2 = itemActionDynamicData.invData.holdingEntity.GetMeleeRay();
		meleeRay2.origin -= meleeRay2.direction * SphereRadius;
		if (Voxel.Raycast(itemActionDynamicData.invData.world, meleeRay2, num3, -538750981, (hitmaskOverride == 0) ? 128 : hitmaskOverride, SphereRadius))
		{
			WorldRayHitInfo updatedHitInfo = _actionData.GetUpdatedHitInfo();
			if (updatedHitInfo.tag != null && updatedHitInfo.tag.StartsWith("E_"))
			{
				entity = ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive;
				if (entity != null && !shouldIgnoreTarget(entity, _actionData.invData.holdingEntity))
				{
					itemActionDynamicData.alreadyHitEnts.Add(entity.entityId);
				}
			}
		}
		Debug.DrawLine(meleeRay2.origin, meleeRay2.origin + meleeRay2.direction * num3, Color.green, Time.deltaTime);
		float num5 = 0f - num;
		while (num5 < num4)
		{
			num5 += num;
			num5 = Mathf.Clamp(num5, 0f, num4);
			meleeRay.direction = (itemActionDynamicData.lastWeaponHeadPosition + normalized * num5 - meleeRay.origin).normalized;
			meleeRay.origin = itemActionDynamicData.invData.holdingEntity.GetMeleeRay().origin - num2 * meleeRay.direction;
			_ = meleeRay.origin + meleeRay.direction * (num3 + num2);
			EntityAlive entityAlive = null;
			Color color = Color.red;
			if (Voxel.Raycast(itemActionDynamicData.invData.world, meleeRay, num3, -538750981, (hitmaskOverride == 0) ? 128 : hitmaskOverride, num))
			{
				Vector3.Distance(Voxel.voxelRayHitInfo.hit.pos, meleeRay.origin);
				entityAlive = ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive;
				if (entityAlive == null)
				{
					entityAlive = Voxel.voxelRayHitInfo.hitCollider.GetComponentInParent<EntityAlive>();
				}
				if (shouldIgnoreTarget(entityAlive, _actionData.invData.holdingEntity))
				{
					entityAlive = null;
				}
				if (entityAlive != null && entityAlive.IsAlive() && !itemActionDynamicData.alreadyHitEnts.Contains(entityAlive.entityId))
				{
					color = Color.green;
				}
				else
				{
					_actionData.invData.holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionGrazeMiss : MinEventTypes.onSelfSecondaryActionGrazeMiss);
				}
				if (entityAlive != null && entityAlive.IsAlive() && !(entityAlive is EntityVehicle) && entityPlayerLocal != null && (entityPlayerLocal.HitInfo.transform == null || !entityPlayerLocal.HitInfo.transform.IsChildOf(entityAlive.ModelTransform)) && Vector3.Angle(holdingEntity.transform.forward, entityAlive.transform.position - holdingEntity.transform.position) <= 30f)
				{
					entityPlayerLocal.MoveController.SetCameraSnapEntity(entityAlive, eCameraSnapMode.MeleeAttack);
				}
			}
			Debug.DrawLine(meleeRay.origin, meleeRay.origin + meleeRay.direction * num3, color, 2f);
			if (ShowDebugSwing)
			{
				DebugDisplayHits.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
				DebugDisplayHits[DebugDisplayHits.Count - 1].transform.position = meleeRay.origin + meleeRay.direction * (num2 + 0.2f) + meleeRay.direction * ((num3 - (num2 + 0.2f)) * 0.5f);
				DebugDisplayHits[DebugDisplayHits.Count - 1].transform.LookAt(meleeRay.origin);
				DebugDisplayHits[DebugDisplayHits.Count - 1].transform.localScale = Vector3.right * num + Vector3.up * num + Vector3.forward * (num3 - (num2 + 0.2f));
				DebugDisplayHits[DebugDisplayHits.Count - 1].layer = 2;
				DebugDisplayHits[DebugDisplayHits.Count - 1].transform.position -= Origin.position;
				DebugDisplayHits[DebugDisplayHits.Count - 1].GetComponent<MeshRenderer>().material.SetColor("_Color", color);
			}
			if (isValidTarget(entityAlive, _actionData.invData.holdingEntity) && (entity == null || entityAlive.entityId != entity.entityId) && !itemActionDynamicData.alreadyHitEnts.Contains(entityAlive.entityId))
			{
				list.Add(Voxel.voxelRayHitInfo.Clone());
				itemActionDynamicData.alreadyHitEnts.Add(entityAlive.entityId);
			}
		}
		holdingEntity.SetModelLayer(lastModelLayer);
		itemActionDynamicData.lastWeaponHeadPosition = vector;
		return list.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void hitTarget(ItemActionData _actionData, WorldRayHitInfo hitInfo, bool _isGrazingHit = false)
	{
		ItemActionDynamicData itemActionDynamicData = (ItemActionDynamicData)_actionData;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		tmpTag = _actionData.ActionTags;
		tmpTag |= ((holdingEntity.inventory.holdingItem == null) ? ItemActionAttack.MeleeTag : holdingEntity.inventory.holdingItem.ItemTags);
		tmpTag = tmpTag | holdingEntity.CurrentStanceTag | holdingEntity.CurrentMovementTag;
		if (!_isGrazingHit && _actionData.invData.itemValue.MaxUseTimes > 0)
		{
			_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, _actionData.invData.itemValue, 1f, holdingEntity, null, tmpTag);
			HandleItemBreak(_actionData);
		}
		_actionData.attackDetails.isCriticalHit = false;
		MinEventParams minEventContext = holdingEntity.MinEventContext;
		minEventContext.StartPosition = hitInfo.ray.origin;
		if (hitInfo.tag != null)
		{
			if (GameUtils.IsBlockOrTerrain(hitInfo.tag))
			{
				minEventContext.Other = null;
				minEventContext.BlockValue = hitInfo.fmcHit.blockValue;
				minEventContext.Position = hitInfo.hit.pos;
				itemActionDynamicData.alreadyHitBlocks.Add(hitInfo.fmcHit.blockPos);
				if (_isGrazingHit)
				{
					return;
				}
			}
			else if (hitInfo.tag.StartsWith("E_"))
			{
				minEventContext.Other = hitInfo.hitCollider.transform.GetComponentInParent<EntityAlive>();
				minEventContext.BlockValue = BlockValue.Air;
				minEventContext.Position = hitInfo.hit.pos;
			}
		}
		if (!_isGrazingHit)
		{
			holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionRayHit : MinEventTypes.onSelfSecondaryActionRayHit);
			_actionData.attackDetails.isCriticalHit = _actionData.indexInEntityOfAction == 1;
		}
		else
		{
			holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionGrazeHit : MinEventTypes.onSelfSecondaryActionGrazeHit);
			_actionData.attackDetails.isCriticalHit = false;
		}
		_actionData.attackDetails.WeaponTypeTag = ItemActionAttack.MeleeTag;
		float blockDamage = GetDamageBlock(_actionData.invData.itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, hitInfo), holdingEntity, _actionData.indexInEntityOfAction);
		float num = GetDamageEntity(_actionData.invData.itemValue, holdingEntity, _actionData.indexInEntityOfAction);
		if (_isGrazingHit)
		{
			blockDamage = 0f;
			num *= EffectManager.GetValue(PassiveEffects.GrazeDamageMultiplier, _actionData.invData.itemValue, GrazeDamagePercentage, holdingEntity, null, tmpTag);
		}
		int num2 = 1;
		if (bUseParticleHarvesting && (particleHarvestingCategory == null || particleHarvestingCategory == item.MadeOfMaterial.id))
		{
			num2 |= 4;
		}
		if (!harvestHitEffectOn && itemActionDynamicData.IsHarvesting)
		{
			num2 |= 8;
		}
		ItemActionAttack.Hit(hitInfo, holdingEntity.entityId, DamageType, blockDamage, num, 1f, 1f, getCriticalChance(_actionData), ItemAction.GetDismemberChance(_actionData, hitInfo), item.MadeOfMaterial.SurfaceCategory, new DamageMultiplier(), new List<string>(), _actionData.attackDetails, num2, ActionExp, ActionExpBonusMultiplier, null, ToolBonuses, itemActionDynamicData.IsHarvesting ? ItemActionAttack.EnumAttackMode.RealAndHarvesting : ItemActionAttack.EnumAttackMode.RealNoHarvesting, _isGrazingHit ? GrazeSoundOverrides : HitSoundOverrides);
		if (_isGrazingHit)
		{
			return;
		}
		ItemActionDynamicMelee.ItemActionDynamicMeleeData dmd = _actionData as ItemActionDynamicMelee.ItemActionDynamicMeleeData;
		if (_actionData.invData.itemValue.ItemClass == ItemValue.None.ItemClass)
		{
			GameManager.Instance.StartCoroutine(harvestOnCompletion(dmd, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameUtils.HarvestOnAttack(_actionData, ToolBonuses);
			}));
		}
		else
		{
			GameUtils.HarvestOnAttack(_actionData, ToolBonuses);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getCriticalChance(ItemActionData _actionData)
	{
		return EffectManager.GetValue(PassiveEffects.CriticalChance, _actionData.invData.itemValue, 0f, _actionData.invData.holdingEntity, null, _actionData.ActionTags);
	}

	public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		tmpTag = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		tmpTag |= ((_itemValue.ItemClass == null) ? ItemActionAttack.MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			tmpTag = tmpTag | _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag;
		}
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, 0f, _holdingEntity, null, tmpTag);
	}

	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		tmpTag = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		tmpTag |= ((_itemValue.ItemClass == null) ? ItemActionAttack.MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			tmpTag = tmpTag | _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag;
		}
		tmpTag |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, 0f, _holdingEntity, null, tmpTag);
		return Utils.FastMin(_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isHitValid(WorldRayHitInfo _hitInfo, ItemActionData _actionData, out EntityAlive _hitEntity)
	{
		_hitEntity = null;
		if (_hitInfo == null)
		{
			return false;
		}
		if (!_hitInfo.bHitValid)
		{
			return false;
		}
		ItemActionDynamicData itemActionDynamicData = (ItemActionDynamicData)_actionData;
		float sqrMagnitude = (_hitInfo.hit.pos - itemActionDynamicData.rayStartPos).sqrMagnitude;
		if (_hitInfo.tag != null && GameUtils.IsBlockOrTerrain(_hitInfo.tag) && sqrMagnitude > BlockRange * BlockRange)
		{
			return false;
		}
		if (_hitInfo.tag != null && _hitInfo.tag.StartsWith("E_"))
		{
			EntityAlive entityAlive = GameUtils.GetHitRootEntity(_hitInfo.tag, _hitInfo.transform) as EntityAlive;
			if (entityAlive == null)
			{
				return false;
			}
			if (shouldIgnoreTarget(entityAlive, _actionData.invData.holdingEntity, _ignoreDead: false))
			{
				return false;
			}
			if (sqrMagnitude > Range * Range)
			{
				return false;
			}
			_hitEntity = entityAlive;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isValidTarget(Entity _target, Entity _self, bool _deadInvalid = true)
	{
		return !shouldIgnoreTarget(_target, _self, _deadInvalid);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldIgnoreTarget(Entity _target, Entity _self, bool _ignoreDead = true)
	{
		if (_target == null)
		{
			return true;
		}
		if (_ignoreDead && !_target.IsAlive())
		{
			return true;
		}
		if (_target.entityId == _self.entityId)
		{
			return true;
		}
		if (_target is EntityDrone)
		{
			return (_target as EntityDrone).isAlly(_self as EntityPlayer);
		}
		EntityPlayer entityPlayer = _self as EntityPlayer;
		EntityPlayer entityPlayer2 = _target as EntityPlayer;
		if (entityPlayer != null && entityPlayer2 != null)
		{
			return !entityPlayer.FriendlyFireCheck(entityPlayer2);
		}
		return false;
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isShowOverlay(ItemActionData _actionData)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal == null)
		{
			return false;
		}
		EntityAlive entityAlive = _actionData.attackDetails.entityHit as EntityAlive;
		if (entityAlive is EntityDrone && (entityPlayerLocal.PlayerUI.xui.Dialog.Respondent != null || (float)entityAlive.Health == entityAlive.Stats.Health.Max))
		{
			if (_actionData.uiOpenedByMe && XUiC_FocusedBlockHealth.IsWindowOpen(entityPlayerLocal.PlayerUI))
			{
				XUiC_FocusedBlockHealth.SetData(entityPlayerLocal.PlayerUI, null, 0f);
				_actionData.uiOpenedByMe = false;
			}
			return false;
		}
		if (!isShowOverlayInternal(_actionData))
		{
			return false;
		}
		WorldRayHitInfo hitInfo = _actionData.hitInfo;
		if (!hitInfo.bHitValid)
		{
			return false;
		}
		if (hitInfo.tag != null)
		{
			if (GameUtils.IsBlockOrTerrain(hitInfo.tag))
			{
				if (hitInfo.hit.distanceSq > BlockRange * BlockRange)
				{
					return false;
				}
				if (!hitInfo.hit.blockValue.Block.IsHealthShownInUI(hitInfo.hit, hitInfo.hit.blockValue))
				{
					return false;
				}
			}
			if (hitInfo.tag.StartsWith("E_") && hitInfo.hit.distanceSq > Range * Range)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isShowOverlayInternal(ItemActionData actionData)
	{
		WorldRayHitInfo executeActionTarget = GetExecuteActionTarget(actionData);
		if (executeActionTarget == null)
		{
			return false;
		}
		if (!executeActionTarget.bHitValid)
		{
			return false;
		}
		bool flag = actionData.attackDetails.entityHit is EntityDrone;
		if (actionData.attackDetails.itemsToDrop == null && !flag)
		{
			return false;
		}
		if (actionData.attackDetails.bBlockHit)
		{
			return actionData.attackDetails.raycastHitPosition == executeActionTarget.hit.blockPos;
		}
		Entity hitRootEntity = GameUtils.GetHitRootEntity(executeActionTarget.tag, executeActionTarget.transform);
		if ((bool)hitRootEntity)
		{
			return hitRootEntity == actionData.attackDetails.entityHit;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void getOverlayData(ItemActionData actionData, out float _perc, out string _text)
	{
		float num = actionData.attackDetails.damageTotalOfTarget;
		float num2 = actionData.attackDetails.damageMax;
		if (actionData.attackDetails.bBlockHit)
		{
			BlockValue block = actionData.invData.world.GetBlock(actionData.attackDetails.hitPosition);
			num = block.damage;
			num2 = block.Block.GetShownMaxDamage();
		}
		else
		{
			EntityAlive entityAlive = actionData.attackDetails.entityHit as EntityAlive;
			if (entityAlive != null)
			{
				if (entityAlive is EntityDrone)
				{
					float num3 = entityAlive.Health;
					float max = entityAlive.Stats.Health.Max;
					_perc = num3 / max;
					_text = string.Format("{0}/{1}", Utils.FastMax(0f, num3).ToCultureInvariantString("0"), max.ToCultureInvariantString());
					return;
				}
				num = -entityAlive.DeathHealth;
				num2 = EntityClass.list[entityAlive.entityClass].DeadBodyHitPoints;
			}
		}
		_perc = (num2 - num) / num2;
		_text = string.Format("{0}/{1}", Utils.FastMax(0f, num2 - num).ToCultureInvariantString("0"), num2.ToCultureInvariantString());
	}

	public override void OnHUD(ItemActionData actionData, int _x, int _y)
	{
		if (actionData == null || !canShowOverlay(actionData))
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = actionData.invData.holdingEntity as EntityPlayerLocal;
		if (!entityPlayerLocal)
		{
			return;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (!isShowOverlay(actionData))
		{
			if (actionData.uiOpenedByMe && XUiC_FocusedBlockHealth.IsWindowOpen(uIForPlayer))
			{
				XUiC_FocusedBlockHealth.SetData(uIForPlayer, null, 0f);
				actionData.uiOpenedByMe = false;
			}
			return;
		}
		if (!XUiC_FocusedBlockHealth.IsWindowOpen(uIForPlayer))
		{
			actionData.uiOpenedByMe = true;
		}
		getOverlayData(actionData, out var _perc, out var _text);
		if (_perc < 1f)
		{
			XUiC_FocusedBlockHealth.SetData(uIForPlayer, _text, _perc);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canShowOverlay(ItemActionData actionData)
	{
		return true;
	}
}
