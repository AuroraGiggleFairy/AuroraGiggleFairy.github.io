using System;
using Audio;
using UnityEngine;

public class Animator3PRangedReloadState : StateMachineBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public const float MultiProjectileOffset = 0.005f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionRanged.ItemActionDataRanged actionData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionRanged actionRanged;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent == null)
		{
			return;
		}
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
		if (actionRanged is ItemActionLauncher itemActionLauncher && actionData.invData.itemValue.Meta < num)
		{
			ItemValue itemValue = actionData.invData.itemValue;
			ItemValue item = ItemClass.GetItem(actionRanged.MagazineItemNames[itemValue.SelectedAmmoTypeIndex]);
			ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = actionData as ItemActionLauncher.ItemActionDataLauncher;
			int num2 = itemActionDataLauncher.projectileTs.Count;
			if (item != itemActionLauncher.LastProjectileType)
			{
				itemActionLauncher.DeleteProjectiles(actionData);
				itemActionDataLauncher.isChangingAmmoType = false;
				num2 = 0;
			}
			int num3 = 1;
			if (!actionData.invData.holdingEntity.isEntityRemote)
			{
				num3 = (itemActionLauncher.HasInfiniteAmmo(actionData) ? num : GetAmmoCount(actionData.invData.holdingEntity, item, num));
			}
			for (int i = num2; i < num3; i++)
			{
				itemActionDataLauncher.projectileTs.Add(itemActionLauncher.instantiateProjectile(actionData, new Vector3(0f, (float)i * 0.005f, 0f)));
			}
		}
		actionData.isReloading = true;
		actionData.isWeaponReloading = true;
		actionData.invData.holdingEntity.MinEventContext.ItemActionData = actionData;
		actionData.invData.holdingEntity.FireEvent(MinEventTypes.onReloadStart);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
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
