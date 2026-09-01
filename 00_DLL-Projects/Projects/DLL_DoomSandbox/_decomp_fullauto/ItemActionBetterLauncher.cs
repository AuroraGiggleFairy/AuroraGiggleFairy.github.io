using FullautoLauncher.Scripts.ProjectileManager;
using UnityEngine;
using UnityEngine.Scripting;
using XMLData.Item;

[Preserve]
public class ItemActionBetterLauncher : ItemActionRanged
{
	public class ItemActionDataBetterLauncher : ItemActionDataRanged
	{
		public Transform projectileJoint;

		public ProjectileParams.ItemInfo info;

		public ItemActionDataBetterLauncher(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
			Transform model = _invData.model;
			projectileJoint = ((model != null) ? Extensions.FindInChilds(model, "ProjectileJoint", false) : null);
		}
	}

	private IProjectileItemGroup group;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return (ItemActionData)(object)new ItemActionDataBetterLauncher(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		((ItemActionRanged)this).ReadFrom(_props);
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		((ItemActionRanged)this).StartHolding(_actionData);
		ItemActionDataBetterLauncher itemActionDataBetterLauncher = (ItemActionDataBetterLauncher)(object)_actionData;
		ItemValue itemValue = ((ItemActionData)itemActionDataBetterLauncher).invData.itemValue;
		ItemClass forId = ItemClass.GetForId(ItemClass.GetItem(((ItemActionAttack)this).MagazineItemNames[itemValue.SelectedAmmoTypeIndex], false).type);
		group = CustomProjectileManager.Get(((ItemData)forId).Name);
		if (itemValue.Meta != 0 && ((ItemActionRanged)this).GetMaxAmmoCount((ItemActionData)(object)itemActionDataBetterLauncher) != 0)
		{
			group.Pool(itemValue.Meta * (int)EffectManager.GetValue((PassiveEffects)16, itemValue, 1f, ((ItemActionData)itemActionDataBetterLauncher).invData.holdingEntity, (Recipe)null, default(FastTags<Global>), true, true, true, true, true, 1, true, false));
		}
		itemActionDataBetterLauncher.info = new ProjectileParams.ItemInfo
		{
			actionData = (ItemActionData)(object)itemActionDataBetterLauncher,
			itemProjectile = forId,
			itemActionProjectile = (ItemActionProjectile)((forId.Actions[0] is ItemActionProjectile) ? forId.Actions[0] : forId.Actions[1]),
			itemValueLauncher = itemValue,
			itemValueProjectile = new ItemValue(((ItemData)forId).Id, false),
			group = group
		};
	}

	public override void OnModificationsChanged(ItemActionData _data)
	{
		((ItemActionRanged)this).OnModificationsChanged(_data);
	}

	public override void StopHolding(ItemActionData _data)
	{
		((ItemActionRanged)this).StopHolding(_data);
		ItemActionDataBetterLauncher itemActionDataBetterLauncher = (ItemActionDataBetterLauncher)(object)_data;
		itemActionDataBetterLauncher.info = null;
	}

	public override void SwapAmmoType(EntityAlive _entity, int _ammoItemId = -1)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		((ItemActionRanged)this).SwapAmmoType(_entity, _ammoItemId);
		ItemActionDataBetterLauncher itemActionDataBetterLauncher = (ItemActionDataBetterLauncher)(object)_entity.inventory.holdingItemData.actionData[((ItemAction)this).ActionIndex];
		ItemValue itemValue = ((ItemActionData)itemActionDataBetterLauncher).invData.itemValue;
		ItemClass forId = ItemClass.GetForId(ItemClass.GetItem(((ItemActionAttack)this).MagazineItemNames[itemValue.SelectedAmmoTypeIndex], false).type);
		group = CustomProjectileManager.Get(((ItemData)forId).Name);
		itemActionDataBetterLauncher.info = new ProjectileParams.ItemInfo
		{
			actionData = (ItemActionData)(object)itemActionDataBetterLauncher,
			itemProjectile = forId,
			itemActionProjectile = (ItemActionProjectile)((forId.Actions[0] is ItemActionProjectile) ? forId.Actions[0] : forId.Actions[1]),
			itemValueLauncher = itemValue,
			itemValueProjectile = new ItemValue(((ItemData)forId).Id, false),
			group = group
		};
	}

	public override Vector3 fireShot(int _shotIdx, ItemActionDataRanged _actionData, ref bool hitEntity)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		hitEntity = true;
		return Vector3.zero;
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		((ItemActionRanged)this).ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
		if (_firingState == 0)
		{
			return;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (((Entity)holdingEntity).isEntityRemote && GameManager.IsDedicatedServer)
		{
			return;
		}
		ItemActionDataBetterLauncher itemActionDataBetterLauncher = (ItemActionDataBetterLauncher)(object)_actionData;
		ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		ItemClass forId = ItemClass.GetForId(ItemClass.GetItem(((ItemActionAttack)this).MagazineItemNames[holdingItemItemValue.SelectedAmmoTypeIndex], false).type);
		int num = (int)EffectManager.GetValue((PassiveEffects)16, ((ItemActionData)itemActionDataBetterLauncher).invData.itemValue, 1f, ((ItemActionData)itemActionDataBetterLauncher).invData.holdingEntity, (Recipe)null, default(FastTags<Global>), true, true, true, true, true, 1, true, false);
		if (num <= 0)
		{
			return;
		}
		if (itemActionDataBetterLauncher.info == null)
		{
			Log.Error("null info!");
			return;
		}
		Vector3 realStartPosition = ((!Object.op_Implicit((Object)(object)itemActionDataBetterLauncher.projectileJoint)) ? (((Component)_actionData.invData.model).transform.position + Origin.position) : (itemActionDataBetterLauncher.projectileJoint.position + Origin.position));
		for (int i = 0; i < num; i++)
		{
			ProjectileParams projectileParams = group.Fire(((Entity)holdingEntity).entityId, itemActionDataBetterLauncher.info, _startPos, realStartPosition, ((ItemActionRanged)this).getDirectionOffset((ItemActionDataRanged)(object)itemActionDataBetterLauncher, _direction, i), (Entity)(object)holdingEntity, ((ItemActionAttack)this).hitmaskOverride);
		}
	}

	public override int GetActionEffectsValues(ItemActionData _actionData, out Vector3 _startPos, out Vector3 _direction)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Ray lookRay = _actionData.invData.holdingEntity.GetLookRay();
		_startPos = ((Ray)(ref lookRay)).origin;
		_direction = ((Ray)(ref lookRay)).direction;
		return 0;
	}
}
