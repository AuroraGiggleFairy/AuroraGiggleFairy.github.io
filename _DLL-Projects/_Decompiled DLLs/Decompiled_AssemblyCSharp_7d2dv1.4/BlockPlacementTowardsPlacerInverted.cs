using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlacementTowardsPlacerInverted : BlockPlacement
{
	public override Result OnPlaceBlock(EnumRotationMode _mode, int _localRot, WorldBase _world, BlockValue _blockValue, HitInfoDetails _hitInfo, Vector3 _entityPos)
	{
		if (_mode != EnumRotationMode.Auto)
		{
			return base.OnPlaceBlock(_mode, _localRot, _world, _blockValue, _hitInfo, _entityPos);
		}
		Result result = new Result(_blockValue, _hitInfo);
		result.blockValue.rotation = 0;
		float num = _entityPos.x - _hitInfo.pos.x;
		float num2 = _entityPos.z - _hitInfo.pos.z;
		if (Mathf.Abs(num) > Mathf.Abs(num2) && num > 0f)
		{
			result.blockValue.rotation = 1;
		}
		else if (Mathf.Abs(num) > Mathf.Abs(num2) && num <= 0f)
		{
			result.blockValue.rotation = 3;
		}
		else if (Mathf.Abs(num2) > Mathf.Abs(num) && num2 > 0f)
		{
			result.blockValue.rotation = 0;
		}
		else if (Mathf.Abs(num2) > Mathf.Abs(num) && num2 <= 0f)
		{
			result.blockValue.rotation = 2;
		}
		return result;
	}
}
