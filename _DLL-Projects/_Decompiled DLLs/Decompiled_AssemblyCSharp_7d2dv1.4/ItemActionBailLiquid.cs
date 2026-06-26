using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionBailLiquid : ItemAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public Vector3i targetPosition;

		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased || _actionData.lastUseTime > 0f)
		{
			return;
		}
		ItemInventoryData invData = _actionData.invData;
		if (Voxel.Raycast(invData.world, invData.hitInfo.ray, Constants.cDigAndBuildDistance, 16, 4095, 0f) && Voxel.voxelRayHitInfo.bHitValid && Voxel.voxelRayHitInfo.hit.voxelData.WaterValue.HasMass())
		{
			_actionData.lastUseTime = Time.time;
			((MyInventoryData)_actionData).targetPosition = Voxel.voxelRayHitInfo.hit.blockPos;
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
		if (_actionData.lastUseTime != 0f && !IsActionRunning(_actionData))
		{
			_actionData.lastUseTime = 0f;
			Vector3i targetPosition = ((MyInventoryData)_actionData).targetPosition;
			NetPackageWaterSet package = NetPackageManager.GetPackage<NetPackageWaterSet>();
			package.AddChange(targetPosition, WaterValue.Empty);
			GameManager.Instance.SetWaterRPC(package);
		}
	}
}
