using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionCollectWater : ItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class CollectWaterActionData : ItemActionAttackData
	{
		public Vector3i targetPosition;

		public int targetMass;

		public CollectWaterActionData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string changeItemToItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isReduceWater;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<CollectWaterUtils.WaterPoint> waterPoints = new List<CollectWaterUtils.WaterPoint>();

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		changeItemToItem = _props.GetString("Change_item_to");
		isReduceWater = _props.GetBool("ReduceWater");
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new CollectWaterActionData(_invData, _indexInEntityOfAction);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		int num = 19500;
		if (_actionData.invData.item is ItemClassWaterContainer itemClassWaterContainer)
		{
			int meta = _actionData.invData.itemValue.Meta;
			num = Mathf.Max(0, itemClassWaterContainer.MaxMass - meta);
		}
		if (num < 195 || _actionData.lastUseTime > 0f)
		{
			return;
		}
		ItemInventoryData invData = _actionData.invData;
		if (Voxel.Raycast(invData.world, invData.hitInfo.ray, Constants.cDigAndBuildDistance, 16, 4095, 0f) && Voxel.voxelRayHitInfo.bHitValid && Voxel.voxelRayHitInfo.hit.voxelData.WaterValue.HasMass())
		{
			_actionData.lastUseTime = Time.time;
			CollectWaterActionData obj = (CollectWaterActionData)_actionData;
			obj.targetPosition = Voxel.voxelRayHitInfo.hit.blockPos;
			obj.targetMass = num;
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

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		if (_actionData.lastUseTime == 0f || IsActionRunning(_actionData))
		{
			return;
		}
		_actionData.lastUseTime = 0f;
		ChunkCluster chunkCluster = GameManager.Instance.World?.ChunkCache;
		if (chunkCluster == null)
		{
			return;
		}
		CollectWaterActionData collectWaterActionData = (CollectWaterActionData)_actionData;
		int num = CollectWaterUtils.CollectInCube(chunkCluster, collectWaterActionData.targetMass, collectWaterActionData.targetPosition, 1, waterPoints);
		if (num > 195)
		{
			if (isReduceWater)
			{
				NetPackageWaterSet package = NetPackageManager.GetPackage<NetPackageWaterSet>();
				foreach (CollectWaterUtils.WaterPoint waterPoint in waterPoints)
				{
					if (waterPoint.massToTake > 0)
					{
						package.AddChange(waterPoint.worldPos, new WaterValue(waterPoint.finalMass));
					}
				}
				GameManager.Instance.SetWaterRPC(package);
			}
			if (!string.IsNullOrEmpty(changeItemToItem))
			{
				ItemStack itemStack = new ItemStack(ItemClass.GetItem(changeItemToItem), _actionData.invData.holdingEntity.inventory.holdingCount);
				itemStack.itemValue.Meta = _actionData.invData.itemValue.Meta + num;
				_actionData.invData.holdingEntity.inventory.SetItem(_actionData.invData.slotIdx, itemStack);
			}
		}
		waterPoints.Clear();
	}
}
