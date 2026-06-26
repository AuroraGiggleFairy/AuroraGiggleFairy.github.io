using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionLauncher : ItemActionRanged
{
	public class ItemActionDataLauncher : ItemActionDataRanged
	{
		public Transform projectileJoint;

		public List<Transform> projectileInstance;

		public float strainPercent = 1f;

		public ItemActionDataLauncher(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
			projectileJoint = ((_invData.model != null) ? _invData.model.FindInChilds("ProjectileJoint") : null);
			projectileInstance = new List<Transform>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new bool InstantiateOnLoad = true;

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
				itemActionDataLauncher.projectileInstance.Add(instantiateProjectile(_actionData));
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
		for (int i = 0; i < itemActionDataLauncher.projectileInstance.Count; i++)
		{
			Transform transform = itemActionDataLauncher.projectileInstance[i];
			if (transform != null)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		itemActionDataLauncher.projectileInstance.Clear();
	}

	public override void ReloadGun(ItemActionData _actionData)
	{
		Manager.StopSequence(_actionData.invData.holdingEntity, ((ItemActionDataRanged)_actionData).SoundStart);
		if (!_actionData.invData.holdingEntity.isEntityRemote)
		{
			_actionData.invData.holdingEntity.OnReloadStart();
		}
	}

	public override void CancelReload(ItemActionData _actionData)
	{
		base.CancelReload(_actionData);
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
		int num = actionData.projectileInstance.Count - maxAmmoCount;
		if (num <= 0)
		{
			return;
		}
		for (int i = maxAmmoCount; i < actionData.projectileInstance.Count; i++)
		{
			if (actionData.projectileInstance[i] != null)
			{
				Object.Destroy(actionData.projectileInstance[i].gameObject);
			}
		}
		actionData.projectileInstance.RemoveRange(maxAmmoCount, num);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 fireShot(int _shotIdx, ItemActionDataRanged _actionData, ref bool hitEntity)
	{
		hitEntity = true;
		return Vector3.zero;
	}

	public Transform instantiateProjectile(ItemActionData _actionData, Vector3 _positionOffset = default(Vector3))
	{
		ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		ItemClass forId = ItemClass.GetForId((LastProjectileType = ItemClass.GetItem(MagazineItemNames[holdingItemItemValue.SelectedAmmoTypeIndex])).type);
		if (forId == null)
		{
			return null;
		}
		ItemActionDataLauncher itemActionDataLauncher = (ItemActionDataLauncher)_actionData;
		int entityId = _actionData.invData.holdingEntity.entityId;
		ItemValue itemValue = new ItemValue(forId.Id);
		Transform transform = forId.CloneModel(_actionData.invData.world, itemValue, Vector3.zero, null, BlockShape.MeshPurpose.World, 0L);
		Transform transform2 = itemActionDataLauncher.projectileJoint;
		if (transform2 == null)
		{
			transform2 = ((itemActionDataLauncher.invData.holdingEntity.emodel.avatarController != null) ? itemActionDataLauncher.invData.holdingEntity.emodel.GetRightHandTransform() : null);
		}
		if (transform2 != null)
		{
			transform.parent = transform2;
			transform.localPosition = _positionOffset;
			transform.localRotation = Quaternion.identity;
		}
		else
		{
			transform.parent = null;
		}
		Utils.SetLayerRecursively(transform.gameObject, (transform2 != null) ? transform2.gameObject.layer : 0);
		ProjectileMoveScript projectileMoveScript = transform.gameObject.AddComponent<ProjectileMoveScript>();
		projectileMoveScript.itemProjectile = forId;
		projectileMoveScript.itemValueProjectile = itemValue;
		projectileMoveScript.itemValueLauncher = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		projectileMoveScript.itemActionProjectile = (ItemActionProjectile)((forId.Actions[0] is ItemActionProjectile) ? forId.Actions[0] : forId.Actions[1]);
		projectileMoveScript.ProjectileOwnerID = entityId;
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
		int num2 = itemActionDataLauncher.projectileInstance.Count - 1;
		while (num2 >= 0)
		{
			Transform transform = itemActionDataLauncher.projectileInstance[num2];
			if (transform != null)
			{
				transform.GetComponent<ProjectileMoveScript>().Fire(_startPos, getDirectionOffset(itemActionDataLauncher, _direction, num2), _actionData.invData.holdingEntity, hitmaskOverride);
			}
			itemActionDataLauncher.projectileInstance.RemoveAt(num2);
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
	public override void getImageActionEffectsStartPosAndDirection(ItemActionData _actionData, out Vector3 _startPos, out Vector3 _direction)
	{
		ItemActionDataLauncher itemActionDataLauncher = (ItemActionDataLauncher)_actionData;
		Ray lookRay = itemActionDataLauncher.invData.holdingEntity.GetLookRay();
		_startPos = lookRay.origin;
		_direction = lookRay.direction;
		_direction = getDirectionOffset(itemActionDataLauncher, _direction);
	}
}
