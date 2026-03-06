using System;
using Audio;
using UnityEngine;

public class AnimatorRangedReloadState : StateMachineBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionRanged.ItemActionDataRanged actionData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionRanged actionRanged;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float MultiProjectileOffset = 0.005f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.emodel.avatarController.UpdateInt("CurrentAnim", 3);
		actionData = componentInParent.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged;
		if (actionData == null)
		{
			return;
		}
		actionRanged = (ItemActionRanged)componentInParent.inventory.holdingItem.Actions[0];
		if (actionData.invData.item.Properties.Values[ItemClass.PropSoundIdle] != null)
		{
			Manager.Stop(actionData.invData.holdingEntity.entityId, actionData.invData.item.Properties.Values[ItemClass.PropSoundIdle]);
		}
		actionData.wasAiming = actionData.invData.holdingEntity.AimingGun;
		if (actionData.invData.holdingEntity.AimingGun && actionData.invData.item.Actions[1] is ItemActionZoom)
		{
			actionData.invData.holdingEntity.inventory.Execute(1, _bReleased: false);
			actionData.invData.holdingEntity.inventory.Execute(1, _bReleased: true);
		}
		if (animator.GetCurrentAnimatorClipInfo(0).Length != 0 && animator.GetCurrentAnimatorClipInfo(0)[0].clip.events.Length == 0)
		{
			if (actionRanged.SoundReload != null)
			{
				componentInParent.PlayOneShot(actionRanged.SoundReload.Value);
			}
		}
		else if (animator.GetNextAnimatorClipInfo(0).Length != 0 && animator.GetNextAnimatorClipInfo(0)[0].clip.events.Length == 0 && actionRanged.SoundReload != null)
		{
			componentInParent.PlayOneShot(actionRanged.SoundReload.Value);
		}
		int num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, actionData.invData.itemValue, actionRanged.BulletsPerMagazine, actionData.invData.holdingEntity);
		if (actionRanged is ItemActionLauncher itemActionLauncher)
		{
			ItemValue itemValue = actionData.invData.itemValue;
			ItemValue item = ItemClass.GetItem(actionRanged.MagazineItemNames[itemValue.SelectedAmmoTypeIndex]);
			ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = actionData as ItemActionLauncher.ItemActionDataLauncher;
			if (itemActionDataLauncher.isChangingAmmoType)
			{
				itemActionLauncher.DeleteProjectiles(actionData);
				itemActionDataLauncher.isChangingAmmoType = false;
			}
			int num2 = 1;
			if (!actionData.invData.holdingEntity.isEntityRemote)
			{
				num2 = (itemActionLauncher.HasInfiniteAmmo(actionData) ? num : GetAmmoCount(actionData.invData.holdingEntity, item, num));
			}
			for (int i = itemActionDataLauncher.projectileTs.Count; i < num2; i++)
			{
				itemActionDataLauncher.projectileTs.Add(itemActionLauncher.instantiateProjectile(actionData, new Vector3(0f, (float)i * 0.005f, 0f)));
			}
		}
		actionData.wasReloadCancelled = false;
		actionData.isReloading = true;
		actionData.invData.holdingEntity.MinEventContext.ItemActionData = actionData;
		actionData.invData.holdingEntity.FireEvent(MinEventTypes.onReloadStart);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.emodel.avatarController.UpdateBool("Reload", _value: false);
		if (actionData == null)
		{
			return;
		}
		animator.speed = 1f;
		if (actionData.isReloadCancelled)
		{
			if (actionRanged is ItemActionLauncher itemActionLauncher)
			{
				itemActionLauncher.DeleteProjectiles(actionData);
			}
		}
		else
		{
			EntityAlive holdingEntity = actionData.invData.holdingEntity;
			ItemValue item = ItemClass.GetItem(actionRanged.MagazineItemNames[actionData.invData.itemValue.SelectedAmmoTypeIndex]);
			int num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, actionData.invData.itemValue, actionRanged.BulletsPerMagazine, holdingEntity);
			actionData.reloadAmount = GetAmmoCountToReload(holdingEntity, item, num);
			if (actionData.reloadAmount > 0)
			{
				actionData.invData.itemValue.Meta = Utils.FastMin(actionData.invData.itemValue.Meta + actionData.reloadAmount, num);
				if (actionData.invData.item.Properties.Values[ItemClass.PropSoundIdle] != null)
				{
					actionData.invData.holdingEntitySoundID = -1;
				}
			}
			actionRanged.ReloadSuccess(actionData);
		}
		actionData.isReloading = false;
		actionData.isWeaponReloading = false;
		actionData.invData.holdingEntity.MinEventContext.ItemActionData = actionData;
		actionData.invData.holdingEntity.FireEvent(MinEventTypes.onReloadStop);
		actionData.invData.holdingEntity.OnReloadEnd();
		actionData.invData.holdingEntity.inventory.CallOnToolbeltChangedInternal();
		actionData.isReloadCancelled = false;
		animator.SetBool("Reload", value: false);
		actionData.invData.holdingEntity.StopAnimatorAudio(Entity.StopAnimatorAudioType.StopOnReloadCancel);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (actionData == null)
		{
			return;
		}
		if (actionData.isReloadCancelled)
		{
			if (!actionData.wasReloadCancelled)
			{
				actionData.wasReloadCancelled = true;
				animator.Play(0, -1, 1f);
				animator.Update(0f);
			}
		}
		else
		{
			actionData.invData.holdingEntity.MinEventContext.ItemActionData = actionData;
			actionData.invData.holdingEntity.FireEvent(MinEventTypes.onReloadUpdate);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetAmmoCountToReload(EntityAlive ea, ItemValue ammo, int modifiedMagazineSize)
	{
		if (actionRanged.HasInfiniteAmmo(actionData))
		{
			if (actionRanged.AmmoIsPerMagazine)
			{
				return modifiedMagazineSize;
			}
			return modifiedMagazineSize - actionData.invData.itemValue.Meta;
		}
		if (ea.bag.GetItemCount(ammo) > 0)
		{
			if (actionRanged.AmmoIsPerMagazine)
			{
				return modifiedMagazineSize * ea.bag.DecItem(ammo, 1);
			}
			return ea.bag.DecItem(ammo, modifiedMagazineSize - actionData.invData.itemValue.Meta);
		}
		if (ea.inventory.GetItemCount(ammo) > 0)
		{
			if (actionRanged.AmmoIsPerMagazine)
			{
				return modifiedMagazineSize * ea.inventory.DecItem(ammo, 1);
			}
			return actionData.invData.holdingEntity.inventory.DecItem(ammo, modifiedMagazineSize - actionData.invData.itemValue.Meta);
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetAmmoCount(EntityAlive ea, ItemValue ammo, int modifiedMagazineSize)
	{
		return Mathf.Min(ea.bag.GetItemCount(ammo) + ea.inventory.GetItemCount(ammo), modifiedMagazineSize);
	}
}
