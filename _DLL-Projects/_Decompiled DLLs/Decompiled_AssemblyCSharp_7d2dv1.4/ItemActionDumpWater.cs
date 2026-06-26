using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionDumpWater : ItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class DumpWaterActionData : ItemActionAttackData
	{
		public Vector3i targetPosition;

		public DumpWaterActionData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropChangeItemTo = "Change_item_to";

	[PublicizedFrom(EAccessModifier.Private)]
	public string changeItemToItem;

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Change_item_to"))
		{
			changeItemToItem = _props.Values["Change_item_to"];
		}
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new DumpWaterActionData(_invData, _indexInEntityOfAction);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		if (!(_actionData.invData.item is ItemClassWaterContainer))
		{
			Debug.LogError("Cannot dump water as item is not a WaterContainer");
		}
		else
		{
			if (_actionData.invData.itemValue.Meta < 195 || _actionData.lastUseTime > 0f)
			{
				return;
			}
			ItemInventoryData invData = _actionData.invData;
			if (!Voxel.Raycast(invData.world, invData.hitInfo.ray, Constants.cDigAndBuildDistance, -555266053, 4095, 0f))
			{
				return;
			}
			WorldRayHitInfo voxelRayHitInfo = Voxel.voxelRayHitInfo;
			if (!voxelRayHitInfo.bHitValid)
			{
				return;
			}
			DumpWaterActionData dumpWaterActionData = (DumpWaterActionData)_actionData;
			if (!TryFindDumpPosition(voxelRayHitInfo, out dumpWaterActionData.targetPosition))
			{
				return;
			}
			if (GameManager.Instance.World.IsWithinTraderArea(dumpWaterActionData.targetPosition))
			{
				GameManager.ShowTooltip(_actionData.invData.holdingEntity as EntityPlayerLocal, "ttCannotUseAtThisTime");
				return;
			}
			if (GameManager.Instance.World.GetLandClaimOwner(dumpWaterActionData.targetPosition, GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_actionData.invData.holdingEntity.entityId)) == EnumLandClaimOwner.Other)
			{
				GameManager.ShowTooltip(_actionData.invData.holdingEntity as EntityPlayerLocal, "ttCannotUseAtThisTime");
				return;
			}
			_actionData.lastUseTime = Time.time;
			invData.holdingEntity.RightArmAnimationUse = true;
			if (soundStart != null)
			{
				invData.holdingEntity.PlayOneShot(soundStart);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryFindDumpPosition(WorldRayHitInfo hitInfo, out Vector3i blockPos)
	{
		blockPos = World.worldToBlockPos(hitInfo.hit.pos);
		if (WaterUtils.CanWaterFlowThrough(GameManager.Instance.World.GetBlock(blockPos)))
		{
			return true;
		}
		if (!hitInfo.hit.blockValue.isair)
		{
			switch (hitInfo.hit.blockFace)
			{
			case BlockFace.Top:
				blockPos += Vector3i.up;
				break;
			case BlockFace.Bottom:
				blockPos += Vector3i.down;
				break;
			case BlockFace.North:
				blockPos += Vector3i.forward;
				break;
			case BlockFace.West:
				blockPos += Vector3i.right;
				break;
			case BlockFace.South:
				blockPos += Vector3i.back;
				break;
			case BlockFace.East:
				blockPos += Vector3i.left;
				break;
			case BlockFace.Middle:
			case BlockFace.None:
				return false;
			}
			if (WaterUtils.CanWaterFlowThrough(GameManager.Instance.World.GetBlock(blockPos)))
			{
				return true;
			}
		}
		blockPos = hitInfo.lastBlockPos;
		if (WaterUtils.CanWaterFlowThrough(GameManager.Instance.World.GetBlock(blockPos)))
		{
			return true;
		}
		return false;
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
		if (_actionData.lastUseTime != 0f && !IsActionRunning(_actionData))
		{
			_actionData.lastUseTime = 0f;
			DumpWaterActionData dumpWaterActionData = (DumpWaterActionData)_actionData;
			int meta = _actionData.invData.itemValue.Meta;
			WaterValue water = GameManager.Instance.World.GetWater(dumpWaterActionData.targetPosition);
			int num = water.GetMass() + meta;
			if (num > 65535)
			{
				Debug.LogError($"Trying to dump {meta} into {water.GetMass()} which more than the maximum mass. Mass will be clamped to {65535}");
				num = 65535;
			}
			NetPackageWaterSet package = NetPackageManager.GetPackage<NetPackageWaterSet>();
			package.AddChange(dumpWaterActionData.targetPosition, new WaterValue(num));
			GameManager.Instance.SetWaterRPC(package);
			if (!string.IsNullOrEmpty(changeItemToItem))
			{
				ItemValue itemValue = ItemClass.GetItem(changeItemToItem);
				_actionData.invData.holdingEntity.inventory.SetItem(_actionData.invData.slotIdx, new ItemStack(itemValue, _actionData.invData.holdingEntity.inventory.holdingCount));
			}
		}
	}
}
