using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlacementDoor : BlockPlacementTowardsPlacerInverted
{
	public override Result OnPlaceBlock(EnumRotationMode _mode, int _localRot, WorldBase _world, BlockValue _blockValue, HitInfoDetails _hitInfo, Vector3 _entityPos)
	{
		if (_mode != EnumRotationMode.Auto)
		{
			return base.OnPlaceBlock(_mode, _localRot, _world, _blockValue, _hitInfo, _entityPos);
		}
		Result result = new Result(_blockValue, _hitInfo);
		float num = _entityPos.x - _hitInfo.pos.x;
		float num2 = _entityPos.z - _hitInfo.pos.z;
		if (!_world.GetBlock(_hitInfo.clrIdx, _hitInfo.blockPos + Vector3i.right).isair && !_world.GetBlock(_hitInfo.clrIdx, _hitInfo.blockPos - Vector3i.right).isair)
		{
			if (Mathf.Abs(num2) > Mathf.Abs(num) && num2 > 0f)
			{
				result.blockValue.rotation = 0;
			}
			else
			{
				result.blockValue.rotation = 2;
			}
		}
		else
		{
			if (_world.GetBlock(_hitInfo.clrIdx, _hitInfo.blockPos + Vector3i.forward).isair || _world.GetBlock(_hitInfo.clrIdx, _hitInfo.blockPos - Vector3i.forward).isair)
			{
				return base.OnPlaceBlock(_mode, _localRot, _world, _blockValue, _hitInfo, _entityPos);
			}
			if (Mathf.Abs(num) > Mathf.Abs(num2) && num > 0f)
			{
				result.blockValue.rotation = 1;
			}
			else
			{
				result.blockValue.rotation = 3;
			}
		}
		return result;
	}

	public override byte LimitRotation(EnumRotationMode _mode, ref int _localRot, HitInfoDetails _hitInfo, bool _bAdd, BlockValue _bv, byte _rotation)
	{
		if (_mode != EnumRotationMode.Auto)
		{
			return base.LimitRotation(_mode, ref _localRot, _hitInfo, _bAdd, _bv, _rotation);
		}
		int num = (_bAdd ? (_rotation + 1) : (_rotation - 1));
		if (num > 3)
		{
			return 0;
		}
		if (num < 0)
		{
			return 3;
		}
		return (byte)num;
	}
}
