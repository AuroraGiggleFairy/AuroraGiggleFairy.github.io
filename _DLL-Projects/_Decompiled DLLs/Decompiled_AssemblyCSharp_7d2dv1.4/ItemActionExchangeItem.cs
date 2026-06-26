using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionExchangeItem : ItemAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string changeItemToItem;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string changeBlockTo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string doBlockAction;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue hitLiquidBlock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i hitLiquidPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<BlockValue> focusedBlocks = new List<BlockValue>();

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("Change_item_to"))
		{
			throw new Exception("Missing attribute 'change_item_to' in use_action 'ExchangeItem'");
		}
		changeItemToItem = _props.Values["Change_item_to"];
		if (_props.Values.ContainsKey("Change_block_to"))
		{
			changeBlockTo = _props.Values["Change_block_to"];
		}
		if (_props.Values.ContainsKey("Do_block_action"))
		{
			doBlockAction = _props.Values["Do_block_action"];
		}
		int num = 1;
		while (_props.Values.ContainsKey("Focused_blockname_" + num))
		{
			string text = _props.Values["Focused_blockname_" + num];
			BlockValue blockValue = ItemClass.GetItem(text).ToBlockValue();
			if (blockValue.Equals(BlockValue.Air))
			{
				throw new Exception("Unknown block name '" + text + "' in use_action!");
			}
			focusedBlocks.Add(blockValue);
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isFocusingBlock(WorldRayHitInfo _hitInfo)
	{
		for (int i = 0; i < focusedBlocks.Count; i++)
		{
			BlockValue other = focusedBlocks[i];
			if (_hitInfo.hit.blockValue.Equals(other))
			{
				return true;
			}
		}
		return false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased || _actionData.lastUseTime > 0f)
		{
			return;
		}
		ItemInventoryData invData = _actionData.invData;
		Ray lookRay = invData.holdingEntity.GetLookRay();
		lookRay.origin += lookRay.direction.normalized * 0.5f;
		if (Voxel.Raycast(invData.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f) && Voxel.voxelRayHitInfo.bHitValid && isFocusingBlock(Voxel.voxelRayHitInfo))
		{
			hitLiquidBlock = Voxel.voxelRayHitInfo.hit.blockValue;
			hitLiquidPos = Voxel.voxelRayHitInfo.hit.blockPos;
			_actionData.lastUseTime = Time.time;
			invData.holdingEntity.RightArmAnimationUse = true;
			if (soundStart != null)
			{
				invData.holdingEntity.PlayOneShot(soundStart);
			}
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		if (_actionData.lastUseTime != 0f && Time.time - _actionData.lastUseTime < Delay)
		{
			return true;
		}
		return false;
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		_data.lastUseTime = 0f;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		if (_actionData.lastUseTime != 0f && !IsActionRunning(_actionData))
		{
			QuestEventManager.Current.ExchangedFromItem(_actionData.invData.itemStack);
			ItemValue itemValue = ItemClass.GetItem(changeItemToItem);
			_actionData.invData.holdingEntity.inventory.SetItem(_actionData.invData.slotIdx, new ItemStack(itemValue, _actionData.invData.holdingEntity.inventory.holdingCount));
			if (doBlockAction != null && GameManager.Instance.World.IsWater(hitLiquidPos))
			{
				hitLiquidBlock.Block.DoExchangeAction(_actionData.invData.world, 0, hitLiquidPos, hitLiquidBlock, doBlockAction, _actionData.invData.holdingEntity.inventory.holdingCount);
			}
			if (changeBlockTo != null)
			{
				Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
				_actionData.invData.world.GetBlock(blockPos);
				BlockValue blockValue = ItemClass.GetItem(changeBlockTo).ToBlockValue();
				_actionData.invData.world.SetBlockRPC(blockPos, blockValue);
			}
		}
	}
}
