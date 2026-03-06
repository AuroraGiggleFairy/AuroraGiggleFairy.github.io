using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionThrownWeapon : ItemActionThrowAway
{
	public float damageEntity;

	public float damageBlock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int hitmaskOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> usePassedInTransformTag = FastTags<TagGroup.Global>.Parse("usePassedInTransform");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tmpTag;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public new ExplosionData Explosion { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public new int Velocity { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public new float FlyTime { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public new float LifeTime { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public new float CollisionRadius { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public float Gravity { get; set; }

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		Explosion = new ExplosionData(Properties, item.Effects);
		if (Properties.Values.ContainsKey("Velocity"))
		{
			Velocity = (int)StringParsers.ParseFloat(Properties.Values["Velocity"]);
		}
		else
		{
			Velocity = 1;
		}
		if (Properties.Values.ContainsKey("FlyTime"))
		{
			FlyTime = StringParsers.ParseFloat(Properties.Values["FlyTime"]);
		}
		else
		{
			FlyTime = 20f;
		}
		if (Properties.Values.ContainsKey("LifeTime"))
		{
			LifeTime = StringParsers.ParseFloat(Properties.Values["LifeTime"]);
		}
		else
		{
			LifeTime = 100f;
		}
		if (Properties.Values.ContainsKey("CollisionRadius"))
		{
			CollisionRadius = StringParsers.ParseFloat(Properties.Values["CollisionRadius"]);
		}
		else
		{
			CollisionRadius = 0.05f;
		}
		if (Properties.Values.ContainsKey("Gravity"))
		{
			Gravity = StringParsers.ParseFloat(Properties.Values["Gravity"]);
		}
		else
		{
			Gravity = -9.81f;
		}
		if (_props.Values.ContainsKey("DamageEntity"))
		{
			damageEntity = StringParsers.ParseFloat(_props.Values["DamageEntity"]);
		}
		else
		{
			damageEntity = 0f;
		}
		if (_props.Values.ContainsKey("DamageBlock"))
		{
			damageBlock = StringParsers.ParseFloat(_props.Values["DamageBlock"]);
		}
		else
		{
			damageBlock = 0f;
		}
		hitmaskOverride = Voxel.ToHitMask(_props.GetString("Hitmask_override"));
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return ((MyInventoryData)_actionData).m_bActivated;
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		base.StartHolding(_actionData);
		MyInventoryData obj = (MyInventoryData)_actionData;
		obj.m_bActivated = false;
		obj.m_ActivateTime = 0f;
		obj.m_LastThrowTime = 0f;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		float num = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? 0.1f : AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast);
		if (!(myInventoryData.m_LastThrowTime <= 0f) && !(Time.time - myInventoryData.m_LastThrowTime < num))
		{
			GameManager.Instance.ItemActionEffectsServer(myInventoryData.invData.holdingEntity.entityId, myInventoryData.invData.slotIdx, myInventoryData.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero);
		}
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		_actionData.invData.holdingEntity.emodel.avatarController.TriggerEvent("WeaponFire");
		(_actionData as MyInventoryData).m_LastThrowTime = 0f;
		if (!_actionData.invData.holdingEntity.isEntityRemote)
		{
			throwAway(_actionData as MyInventoryData);
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (!myInventoryData.m_bActivated && Time.time - myInventoryData.m_LastThrowTime < Delay)
		{
			return;
		}
		if (_actionData.invData.itemValue.PercentUsesLeft == 0f)
		{
			if (_bReleased)
			{
				EntityPlayerLocal player = _actionData.invData.holdingEntity as EntityPlayerLocal;
				if (item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
				{
					Manager.PlayInsidePlayerHead(item.Properties.Values[ItemClass.PropSoundJammed]);
				}
				GameManager.ShowTooltip(player, "ttItemNeedsRepair");
			}
		}
		else if (!_bReleased)
		{
			if (!myInventoryData.m_bActivated && !myInventoryData.m_bCanceled)
			{
				myInventoryData.m_bActivated = true;
				myInventoryData.m_ActivateTime = Time.time;
				myInventoryData.invData.holdingEntity.emodel.avatarController.TriggerEvent("WeaponPreFire");
			}
		}
		else if (myInventoryData.m_bCanceled)
		{
			myInventoryData.m_bCanceled = false;
		}
		else if (myInventoryData.m_bActivated)
		{
			myInventoryData.m_ThrowStrength = Mathf.Min(maxStrainTime, Time.time - myInventoryData.m_ActivateTime) / maxStrainTime * maxThrowStrength;
			myInventoryData.m_LastThrowTime = Time.time;
			myInventoryData.m_bActivated = false;
			if (!myInventoryData.invData.holdingEntity.isEntityRemote)
			{
				myInventoryData.invData.holdingEntity.emodel.avatarController.TriggerEvent("WeaponFire");
			}
			if (soundStart != null)
			{
				myInventoryData.invData.holdingEntity.PlayOneShot(soundStart);
			}
		}
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		MyInventoryData obj = (MyInventoryData)_actionData;
		obj.invData.holdingEntity.emodel.avatarController.TriggerEvent("WeaponPreFireCancel");
		obj.m_bActivated = false;
		obj.m_bCanceled = true;
		obj.m_ActivateTime = 0f;
		obj.m_LastThrowTime = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void throwAway(MyInventoryData _actionData)
	{
		ThrownWeaponMoveScript thrownWeaponMoveScript = instantiateProjectile(_actionData);
		_ = _actionData.invData;
		Vector3 lookVector = _actionData.invData.holdingEntity.GetLookVector();
		_actionData.invData.holdingEntity.getHeadPosition();
		Vector3 origin = _actionData.invData.holdingEntity.GetLookRay().origin;
		if (_actionData.invData.holdingEntity as EntityPlayerLocal != null)
		{
			float value = EffectManager.GetValue(PassiveEffects.StaminaLoss, _actionData.invData.holdingEntity.inventory.holdingItemItemValue, 2f, _actionData.invData.holdingEntity, null, _actionData.invData.holdingEntity.inventory.holdingItem.ItemTags | FastTags<TagGroup.Global>.Parse((_actionData.indexInEntityOfAction == 0) ? "primary" : "secondary"));
			_actionData.invData.holdingEntity.Stats.Stamina.Value -= value;
		}
		thrownWeaponMoveScript.Fire(origin, lookVector, _actionData.invData.holdingEntity, hitmaskOverride, _actionData.m_ThrowStrength);
		_actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
	}

	public ThrownWeaponMoveScript instantiateProjectile(ItemActionData _actionData)
	{
		_ = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		_ = _actionData.invData.holdingEntity.entityId;
		new ItemValue(_actionData.invData.item.Id);
		Transform transform = Object.Instantiate(_actionData.invData.holdingEntity.inventory.models[_actionData.invData.holdingEntity.inventory.holdingItemIdx].gameObject).transform;
		Utils.ForceMaterialsInstance(transform.gameObject);
		transform.parent = null;
		transform.position = _actionData.invData.model.transform.position;
		transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		Utils.SetLayerRecursively(transform.gameObject, 0);
		ThrownWeaponMoveScript thrownWeaponMoveScript = transform.gameObject.AddMissingComponent<ThrownWeaponMoveScript>();
		thrownWeaponMoveScript.itemActionThrownWeapon = this;
		thrownWeaponMoveScript.itemWeapon = _actionData.invData.item;
		thrownWeaponMoveScript.itemValueWeapon = _actionData.invData.itemValue;
		thrownWeaponMoveScript.actionData = _actionData as MyInventoryData;
		thrownWeaponMoveScript.ProjectileOwnerID = _actionData.invData.holdingEntity.entityId;
		transform.gameObject.SetActive(value: true);
		_actionData.invData.model.gameObject.SetActive(value: false);
		_actionData.invData.holdingEntity.MinEventContext.Self = _actionData.invData.holdingEntity;
		_actionData.invData.holdingEntity.MinEventContext.Transform = transform;
		_actionData.invData.holdingEntity.MinEventContext.Tags = usePassedInTransformTag;
		thrownWeaponMoveScript.itemValueWeapon.FireEvent(MinEventTypes.onSelfHoldingItemThrown, _actionData.invData.holdingEntity.MinEventContext);
		return thrownWeaponMoveScript;
	}

	public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		tmpTag = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		tmpTag |= ((_itemValue.ItemClass == null) ? ItemActionAttack.MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			tmpTag |= _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag;
		}
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, damageEntity, _holdingEntity, null, tmpTag);
	}

	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		tmpTag = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		tmpTag |= ((_itemValue.ItemClass == null) ? ItemActionAttack.MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			tmpTag |= _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag;
		}
		tmpTag |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, damageBlock, _holdingEntity, null, tmpTag);
		return Utils.FastMin(_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}
}
