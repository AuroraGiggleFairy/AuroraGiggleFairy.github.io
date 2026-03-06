using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionOpenLootBundle : ItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public bool bEatingStarted;

		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	public new bool Consume;

	public HashSet<int> ConditionBlockTypes;

	public string lootListName = "";

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Consume"))
		{
			Consume = StringParsers.ParseBool(_props.Values["Consume"]);
		}
		else
		{
			Consume = true;
		}
		_props.ParseString("LootList", ref lootListName);
		UseAnimation = false;
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		((MyInventoryData)_data).bEatingStarted = false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		MyInventoryData myInventoryData = (MyInventoryData)_actionData;
		if (!_bReleased || Time.time - _actionData.lastUseTime < Delay || IsActionRunning(_actionData))
		{
			return;
		}
		EntityAlive holdingEntity = myInventoryData.invData.holdingEntity;
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags) > 0f)
		{
			_actionData.lastUseTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack");
			return;
		}
		BlockValue air = BlockValue.Air;
		if (ConditionBlockTypes != null)
		{
			Ray lookRay = holdingEntity.GetLookRay();
			int modelLayer = holdingEntity.GetModelLayer();
			holdingEntity.SetModelLayer(2);
			Voxel.Raycast(myInventoryData.invData.world, lookRay, 2.5f, 131, (holdingEntity is EntityPlayer) ? 0.2f : 0.4f);
			holdingEntity.SetModelLayer(modelLayer);
			WorldRayHitInfo voxelRayHitInfo = Voxel.voxelRayHitInfo;
			if (!GameUtils.IsBlockOrTerrain(voxelRayHitInfo.tag))
			{
				return;
			}
			_ = voxelRayHitInfo.hit;
			air = voxelRayHitInfo.hit.blockValue;
			if (air.isair || !ConditionBlockTypes.Contains(air.type))
			{
				lookRay = myInventoryData.invData.holdingEntity.GetLookRay();
				lookRay.origin += lookRay.direction.normalized * 0.5f;
				if (!Voxel.Raycast(myInventoryData.invData.world, lookRay, 2.5f, -538480645, 4095, 0f))
				{
					return;
				}
				_ = voxelRayHitInfo.hit;
				air = voxelRayHitInfo.hit.blockValue;
				if (air.isair || !ConditionBlockTypes.Contains(air.type))
				{
					return;
				}
			}
		}
		_actionData.lastUseTime = Time.time;
		ExecuteInstantAction(myInventoryData.invData.holdingEntity, myInventoryData.invData.itemStack, isHeldItem: true, null);
	}

	public override bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
	{
		ent.MinEventContext.ItemValue = stack.itemValue;
		ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionStart, ent.MinEventContext);
		ent.FireEvent(MinEventTypes.onSelfPrimaryActionStart, useInventory: false);
		if (soundStart != null)
		{
			ent.PlayOneShot(soundStart);
		}
		LootContainer lootContainer = LootContainer.GetLootContainer(lootListName);
		if (lootContainer == null)
		{
			return false;
		}
		if (Consume)
		{
			if (stack.itemValue.MaxUseTimes > 0 && stack.itemValue.UseTimes + 1f < (float)stack.itemValue.MaxUseTimes)
			{
				stack.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, stack.itemValue, 1f, ent, null, stack.itemValue.ItemClass.ItemTags);
				return true;
			}
			if (isHeldItem)
			{
				ent.inventory.DecHoldingItem(1);
			}
			else
			{
				stack.count--;
			}
		}
		ent.MinEventContext.ItemValue = stack.itemValue;
		ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, ent.MinEventContext);
		ent.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, useInventory: false);
		new List<ItemStack>();
		if (ent is EntityPlayer entityPlayer)
		{
			IList<ItemStack> list = lootContainer.Spawn(ent.rand, 100, entityPlayer.GetHighestPartyLootStage(0f, 0f), 0f, entityPlayer, FastTags<TagGroup.Global>.none, uniqueItems: true, ignoreLootProb: false);
			for (int i = 0; i < list.Count; i++)
			{
				ItemStack itemStack = list[i].Clone();
				if (!LocalPlayerUI.GetUIForPlayer(ent as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
				{
					ent.world.gameManager.ItemDropServer(itemStack, ent.GetPosition(), Vector3.zero);
				}
			}
		}
		return true;
	}
}
