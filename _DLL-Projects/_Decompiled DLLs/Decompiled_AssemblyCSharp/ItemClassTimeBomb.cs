using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassTimeBomb : ItemClass
{
	public bool bExplodeOnHitGround;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPrimed = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosion;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dropStarts;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mustPrime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int explodeAfterTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tickSoundDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] activationTransformToHide;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activationEmissive;

	public override void Init()
	{
		base.Init();
		explosion = new ExplosionData(Properties, Effects);
		Properties.ParseBool("ExplodeOnHit", ref bExplodeOnHitGround);
		Properties.ParseBool("FuseStartOnDrop", ref dropStarts);
		Properties.ParseBool("FusePrimeOnActivate", ref mustPrime);
		string text = Properties.GetString("ActivationTransformToHide");
		if (text.Length > 0)
		{
			activationTransformToHide = text.Split(';');
		}
		activationEmissive = Properties.GetString("ActivationEmissive");
		float optionalValue = 2f;
		Properties.ParseFloat("FuseTime", ref optionalValue);
		explodeAfterTicks = (int)(optionalValue * 20f);
	}

	public override void StartHolding(ItemInventoryData _data, Transform _modelTransform)
	{
		base.StartHolding(_data, _modelTransform);
		OnHoldingReset(_data);
		activateHolding(_data.itemValue, 0, _modelTransform);
		_data.Changed();
	}

	public override void OnHoldingItemActivated(ItemInventoryData _data)
	{
		ItemValue itemValue = _data.itemValue;
		if (itemValue.Meta == 0)
		{
			if (_data.holdingEntity.emodel.avatarController != null)
			{
				_data.holdingEntity.emodel.avatarController.CancelEvent("WeaponPreFireCancel");
			}
			setActivationTransformsActive(_data.holdingEntity.inventory.models[_data.holdingEntity.inventory.holdingItemIdx], isActive: true);
			_data.holdingEntity.RightArmAnimationUse = true;
			int ticks = explodeAfterTicks;
			if (mustPrime)
			{
				ticks = -1;
			}
			activateHolding(itemValue, ticks, _data.model);
			_data.Changed();
			AudioSource audioSource = ((_data.model != null) ? _data.model.GetComponentInChildren<AudioSource>() : null);
			if ((bool)audioSource)
			{
				audioSource.Play();
			}
			if (!_data.holdingEntity.isEntityRemote)
			{
				_data.gameManager.SimpleRPC(_data.holdingEntity.entityId, SimpleRPCType.OnActivateItem, _bExeLocal: false, _bOnlyLocal: false);
			}
		}
	}

	public override void OnHoldingUpdate(ItemInventoryData _data)
	{
		base.OnHoldingUpdate(_data);
		if (_data.holdingEntity.isEntityRemote)
		{
			return;
		}
		ItemValue itemValue = _data.itemValue;
		if (itemValue.Meta > 0)
		{
			itemValue.Meta--;
			if (itemValue.Meta == 0)
			{
				Vector3 vector = ((_data.model != null) ? (_data.model.position + Origin.position) : _data.holdingEntity.GetPosition());
				MinEventParams.CachedEventParam.Self = _data.holdingEntity;
				MinEventParams.CachedEventParam.Position = vector;
				MinEventParams.CachedEventParam.ItemValue = itemValue;
				itemValue.FireEvent(MinEventTypes.onProjectileImpact, MinEventParams.CachedEventParam);
				_data.gameManager.ExplosionServer(0, vector, World.worldToBlockPos(vector), Quaternion.identity, explosion, _data.holdingEntity.entityId, 0.1f, _bRemoveBlockAtExplPosition: false, itemValue.Clone());
				activateHolding(itemValue, 0, _data.model);
				_data.holdingEntity.inventory.DecHoldingItem(1);
			}
			else
			{
				_data.Changed();
			}
		}
	}

	public override void OnHoldingReset(ItemInventoryData _data)
	{
		_data.itemValue.Meta = 0;
		setActivationTransformsActive(_data.holdingEntity.inventory.models[_data.holdingEntity.inventory.holdingItemIdx], isActive: false);
		if (!_data.holdingEntity.isEntityRemote)
		{
			_data.gameManager.SimpleRPC(_data.holdingEntity.entityId, SimpleRPCType.OnResetItem, _bExeLocal: false, _bOnlyLocal: false);
		}
		if (_data.holdingEntity.emodel.avatarController != null)
		{
			_data.holdingEntity.emodel.avatarController.TriggerEvent("WeaponPreFireCancel");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void activateHolding(ItemValue iv, int ticks, Transform _t)
	{
		iv.Meta = ticks;
		if (!(_t != null))
		{
			return;
		}
		OnActivateItemGameObjectReference component = _t.GetComponent<OnActivateItemGameObjectReference>();
		if (component != null)
		{
			bool flag = ticks != 0;
			if (component.IsActivated() != flag)
			{
				component.ActivateItem(flag);
			}
		}
	}

	public override void OnMeshCreated(ItemWorldData _data)
	{
		EntityItem entityItem = _data.entityItem;
		ItemValue itemValue = entityItem.itemStack.itemValue;
		setActivationTransformsActive(entityItem.GetModelTransform(), itemValue.Meta != 0);
	}

	public override bool CanDrop(ItemValue _iv = null)
	{
		if (bCanDrop)
		{
			if (_iv != null)
			{
				return _iv.Meta == 0;
			}
			return true;
		}
		return false;
	}

	public override void Deactivate(ItemValue _iv)
	{
		_iv.Meta = 0;
	}

	public override void OnDroppedUpdate(ItemWorldData _data)
	{
		EntityItem entityItem = _data.entityItem;
		bool flag = _data.world.IsRemote();
		ItemValue itemValue = entityItem.itemStack.itemValue;
		if (itemValue.Meta == 65535)
		{
			itemValue.Meta = -1;
		}
		Vector3 vector = entityItem.PhysicsMasterGetFinalPosition();
		if (!flag && bExplodeOnHitGround && (!mustPrime || itemValue.Meta > 0) && (entityItem.isCollided || _data.world.IsWater(vector)))
		{
			MinEventParams minEventParams = new MinEventParams();
			minEventParams.Self = _data.world.GetEntity(_data.belongsEntityId) as EntityAlive;
			minEventParams.IsLocal = true;
			minEventParams.Position = vector;
			minEventParams.ItemValue = itemValue;
			itemValue.FireEvent(MinEventTypes.onProjectileImpact, minEventParams);
			itemValue.Meta = 1;
		}
		if (!flag && ((dropStarts && itemValue.Meta == 0) || (mustPrime && itemValue.Meta <= -1)))
		{
			Animator componentInChildren = entityItem.gameObject.GetComponentInChildren<Animator>();
			if (componentInChildren != null)
			{
				componentInChildren.SetBool("PinPulled", value: true);
			}
			itemValue.Meta = explodeAfterTicks;
		}
		if (itemValue.Meta <= 0)
		{
			return;
		}
		OnActivateItemGameObjectReference onActivateItemGameObjectReference = ((entityItem.GetModelTransform() != null) ? entityItem.GetModelTransform().GetComponent<OnActivateItemGameObjectReference>() : null);
		if (onActivateItemGameObjectReference != null && !onActivateItemGameObjectReference.IsActivated())
		{
			onActivateItemGameObjectReference.ActivateItem(_activate: true);
			setActivationTransformsActive(entityItem.GetModelTransform(), isActive: true);
		}
		if (!flag)
		{
			tickSoundDelay -= 0.05f;
			if (tickSoundDelay <= 0f)
			{
				tickSoundDelay = entityItem.itemClass.SoundTickDelay;
				entityItem.PlayOneShot(entityItem.itemClass.SoundTick);
			}
			itemValue.Meta--;
			if (itemValue.Meta == 0)
			{
				entityItem.SetDead();
				_data.gameManager.ExplosionServer(0, vector, World.worldToBlockPos(vector), Quaternion.identity, explosion, _data.belongsEntityId, 0f, _bRemoveBlockAtExplPosition: false, itemValue.Clone());
			}
			else
			{
				entityItem.itemStack.itemValue = itemValue;
			}
		}
	}

	public override void OnDamagedByExplosion(ItemWorldData _data)
	{
		_data.gameManager.ExplosionServer(0, _data.entityItem.GetPosition(), World.worldToBlockPos(_data.entityItem.GetPosition()), Quaternion.identity, explosion, _data.belongsEntityId, _data.entityItem.rand.RandomRange(0.1f, 0.3f), _bRemoveBlockAtExplPosition: false, _data.entityItem.itemStack.itemValue.Clone());
	}

	public override bool CanCollect(ItemValue _iv)
	{
		return _iv.Meta == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setActivationTransformsActive(Transform item, bool isActive)
	{
		if (activationTransformToHide != null)
		{
			string[] array = activationTransformToHide;
			foreach (string name in array)
			{
				Transform transform = item.FindInChilds(name);
				if ((bool)transform)
				{
					transform.gameObject.SetActive(isActive);
				}
			}
		}
		if (string.IsNullOrEmpty(activationEmissive))
		{
			return;
		}
		float value = (isActive ? 1 : 0);
		Renderer[] componentsInChildren = item.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.CompareTag(activationEmissive))
			{
				renderer.material.SetFloat("_EmissionMultiply", value);
			}
		}
	}
}
