using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionLauncher : ItemActionRanged
{
	public class ItemActionDataLauncher : ItemActionDataRanged
	{
		public Transform projectileJointT;

		public List<Transform> projectileTs;

		public float strainPercent = 1f;

		public float lastAttackStrainPercent;

		public ItemActionDataLauncher(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
			projectileJointT = ((_invData.model != null) ? _invData.model.FindInChilds("ProjectileJoint") : null);
			projectileTs = new List<Transform>();
		}
	}

	public ItemValue LastProjectileType;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDataLauncher(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		base.StartHolding(_actionData);
		DeleteProjectiles(_actionData);
		ItemActionDataLauncher itemActionDataLauncher = (ItemActionDataLauncher)_actionData;
		if (_actionData.invData.itemValue.Meta != 0 && GetMaxAmmoCount(itemActionDataLauncher) != 0)
		{
			for (int i = 0; i < _actionData.invData.itemValue.Meta; i++)
			{
				itemActionDataLauncher.projectileTs.Add(instantiateProjectile(_actionData));
			}
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		DeleteProjectiles(_data);
	}

	public void DeleteProjectiles(ItemActionData _actionData)
	{
		ItemActionDataLauncher itemActionDataLauncher = (ItemActionDataLauncher)_actionData;
		for (int i = 0; i < itemActionDataLauncher.projectileTs.Count; i++)
		{
			Transform transform = itemActionDataLauncher.projectileTs[i];
			if ((bool)transform)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		itemActionDataLauncher.projectileTs.Clear();
	}

	public override void ReloadGun(ItemActionData _actionData)
	{
		ItemActionDataLauncher itemActionDataLauncher = (ItemActionDataLauncher)_actionData;
		if (itemActionDataLauncher != null)
		{
			itemActionDataLauncher.isReloadRequested = false;
			Manager.StopSequence(_actionData.invData.holdingEntity, ((ItemActionDataRanged)_actionData).SoundStart);
			if (!_actionData.invData.holdingEntity.isEntityRemote)
			{
				_actionData.invData.holdingEntity.OnReloadStart();
			}
		}
	}

	public override void CancelReload(ItemActionData _actionData, bool holsterWeapon)
	{
		base.CancelReload(_actionData, holsterWeapon);
		ItemActionDataLauncher actionData = (ItemActionDataLauncher)_actionData;
		ClampAmmoCount(actionData);
	}

	public override void SwapAmmoType(EntityAlive _entity, int _selectedIndex = -1)
	{
		ItemActionDataLauncher actionData = (ItemActionDataLauncher)_entity.inventory.holdingItemData.actionData[0];
		ClampAmmoCount(actionData);
		base.SwapAmmoType(_entity, _selectedIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClampAmmoCount(ItemActionDataLauncher actionData)
	{
		int maxAmmoCount = GetMaxAmmoCount(actionData);
		int num = actionData.projectileTs.Count - maxAmmoCount;
		if (num <= 0)
		{
			return;
		}
		for (int i = maxAmmoCount; i < actionData.projectileTs.Count; i++)
		{
			if (actionData.projectileTs[i] != null)
			{
				Object.Destroy(actionData.projectileTs[i].gameObject);
			}
		}
		actionData.projectileTs.RemoveRange(maxAmmoCount, num);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 fireShot(int _shotIdx, ItemActionDataRanged _actionData, ref bool hitEntity)
	{
		hitEntity = true;
		return Vector3.zero;
	}

	public Transform instantiateProjectile(ItemActionData _actionData, Vector3 _positionOffset = default(Vector3))
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue holdingItemItemValue = holdingEntity.inventory.holdingItemItemValue;
		ItemClass forId = ItemClass.GetForId((LastProjectileType = ItemClass.GetItem(MagazineItemNames[holdingItemItemValue.SelectedAmmoTypeIndex])).type);
		if (forId == null)
		{
			return null;
		}
		ItemActionDataLauncher itemActionDataLauncher = (ItemActionDataLauncher)_actionData;
		itemActionDataLauncher.lastAttackStrainPercent = itemActionDataLauncher.strainPercent;
		ItemValue itemValue = new ItemValue(forId.Id);
		Transform transform = forId.CloneModel(_actionData.invData.world, itemValue, Vector3.zero, null);
		Transform transform2 = itemActionDataLauncher.projectileJointT;
		if (!transform2)
		{
			transform2 = holdingEntity.emodel.GetRightHandTransform();
		}
		if ((bool)transform2)
		{
			transform.SetParent(transform2, worldPositionStays: false);
			transform.SetLocalPositionAndRotation(_positionOffset, Quaternion.identity);
			transform.localScale = Vector3.one;
		}
		else
		{
			transform.parent = null;
			transform.localScale = Vector3.one;
		}
		Utils.SetLayerRecursively(transform.gameObject, transform2 ? transform2.gameObject.layer : 0);
		ProjectileMoveScript projectileMoveScript = transform.gameObject.AddComponent<ProjectileMoveScript>();
		projectileMoveScript.itemProjectile = forId;
		projectileMoveScript.itemValueProjectile = itemValue;
		projectileMoveScript.itemValueLauncher = holdingItemItemValue;
		projectileMoveScript.itemActionProjectile = (ItemActionProjectile)((forId.Actions[0] is ItemActionProjectile) ? forId.Actions[0] : forId.Actions[1]);
		projectileMoveScript.ProjectileOwnerID = holdingEntity.entityId;
		projectileMoveScript.actionData = itemActionDataLauncher;
		transform.gameObject.SetActive(value: true);
		return transform;
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
		if (_firingState == 0)
		{
			return;
		}
		ItemActionDataLauncher itemActionDataLauncher = (ItemActionDataLauncher)_actionData;
		int num = GetBurstCount(_actionData);
		if (num <= 0)
		{
			return;
		}
		int num2 = itemActionDataLauncher.projectileTs.Count - 1;
		while (num2 >= 0)
		{
			Transform transform = itemActionDataLauncher.projectileTs[num2];
			if ((bool)transform)
			{
				transform.GetComponent<ProjectileMoveScript>().Fire(_startPos, getDirectionOffset(itemActionDataLauncher, _direction, num2), _actionData.invData.holdingEntity, hitmaskOverride);
			}
			itemActionDataLauncher.projectileTs.RemoveAt(num2);
			if (--num > 0)
			{
				num2--;
				continue;
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ConsumeAmmo(ItemActionData _actionData)
	{
		_actionData.invData.itemValue.Meta -= GetBurstCount(_actionData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int GetActionEffectsValues(ItemActionData _actionData, out Vector3 _startPos, out Vector3 _direction)
	{
		ItemActionDataLauncher itemActionDataLauncher = (ItemActionDataLauncher)_actionData;
		Ray lookRay = itemActionDataLauncher.invData.holdingEntity.GetLookRay();
		_startPos = lookRay.origin;
		_direction = lookRay.direction;
		_direction = getDirectionOffset(itemActionDataLauncher, _direction);
		return 0;
	}
}
