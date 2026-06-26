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

	public void PlaySound(string name)
	{
		if (entity != null)
		{
			EntityPlayer entityPlayer = entity as EntityPlayer;
			if (!(entityPlayer != null) || !entityPlayer.IsReloadCancelled())
			{
				entity.PlayOneShot(name, sound_in_head: false, serverSignalOnly: true);
			}
		}
	}

	public void PlayLocalSound(string name)
	{
		PlaySound(name);
	}

	public void PlayStepSound()
	{
		if (entity != null && (double)(Time.time - lastTimeStepPlayed) > 0.1)
		{
			entity.PlayStepSound();
			lastTimeStepPlayed = Time.time;
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
		if (entity != null && entity.emodel != null && entity.inventory.models[entity.inventory.holdingItemIdx] != null)
		{
			entity.inventory.models[entity.inventory.holdingItemIdx].gameObject.SetActive(value: false);
		}
	}

	public void ShowHoldingItem()
	{
		if (entity != null && entity.emodel != null && entity.inventory.models[entity.inventory.holdingItemIdx] != null)
		{
			entity.inventory.models[entity.inventory.holdingItemIdx].gameObject.SetActive(value: true);
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
