using System;
using Audio;
using UnityEngine;

public class AnimationEventBridge : RootTransformRefEntity
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive _entity;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeStepPlayed;

	public EntityAlive entity
	{
		get
		{
			if (!_entity && (bool)RootTransform)
			{
				_entity = RootTransform.GetComponent<EntityAlive>();
			}
			return _entity;
		}
	}

	public void PlaySound(AnimationEvent ae)
	{
		playSound(ae.stringParameter, ae);
	}

	public void playSound(string name, AnimationEvent ae = null)
	{
		if (entity != null)
		{
			EntityPlayer entityPlayer = entity as EntityPlayer;
			if (!(entityPlayer != null) || !entityPlayer.IsReloadCancelled())
			{
				entity.PlayOneShot(name, sound_in_head: false, serverSignalOnly: true, isUnique: false, ae);
			}
		}
	}

	public void PlayLocalSound(string name)
	{
		playSound(name);
	}

	public void PlayStepSound(AnimationEvent _animEvent)
	{
		if (!entity || !(_animEvent.animatorClipInfo.weight >= 0.3f))
		{
			return;
		}
		float time = Time.time;
		if (time - lastTimeStepPlayed >= 0.1f)
		{
			lastTimeStepPlayed = time;
			float num = _animEvent.floatParameter;
			if (num == 0f)
			{
				num = 1f;
			}
			entity.PlayStepSound(num);
		}
	}

	public void DeathImpactLight()
	{
		if (entity != null && entity.IsDead())
		{
			Manager.Play(entity, "impactbodylight");
		}
	}

	public void DeathImpactHeavy()
	{
		if (entity != null && entity.IsDead())
		{
			Manager.Play(entity, "impactbodyheavy");
		}
	}

	public void HideHoldingItem()
	{
		EntityAlive entityAlive = entity;
		if ((bool)entityAlive && (bool)entityAlive.emodel)
		{
			Transform holdingItemTransform = entityAlive.inventory.GetHoldingItemTransform();
			if ((bool)holdingItemTransform)
			{
				holdingItemTransform.gameObject.SetActive(value: false);
			}
		}
	}

	public void ShowHoldingItem()
	{
		EntityAlive entityAlive = entity;
		if ((bool)entityAlive && (bool)entityAlive.emodel)
		{
			Transform holdingItemTransform = entityAlive.inventory.GetHoldingItemTransform();
			if ((bool)holdingItemTransform)
			{
				holdingItemTransform.gameObject.SetActive(value: true);
			}
		}
	}

	public void Hit()
	{
		Entity entity = this.entity;
		if ((bool)entity && (bool)entity.emodel && (bool)entity.emodel.avatarController)
		{
			entity.emodel.avatarController.SetAttackImpact();
		}
	}

	public void ConsumeComplete()
	{
		EntityPlayerLocal entityPlayerLocal = entity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			ItemClass holdingItem = entityPlayerLocal.inventory.holdingItem;
			int num = 0;
			if (num < holdingItem.Actions.Length && holdingItem.Actions[num] is ItemActionEat itemActionEat)
			{
				itemActionEat.Completed(entityPlayerLocal.inventory.holdingItemData.actionData[num]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		EntityAlive entityAlive = entity;
		if ((object)entityAlive != null && entityAlive.emodel != null)
		{
			entityAlive.emodel.avatarController.SetCrouching(entityAlive.IsCrouching);
			entityAlive.SetVehiclePoseMode(entityAlive.vehiclePoseMode);
		}
	}
}
