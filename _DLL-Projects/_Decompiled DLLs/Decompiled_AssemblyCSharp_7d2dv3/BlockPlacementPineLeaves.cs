using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlacementPineLeaves : BlockPlacement
{
	public override Result OnPlaceBlock(EnumPlacement _placement, EnumRotationMode _mode, int _localRot, WorldBase _world, BlockValue _blockValue, PropTransform _propTransform, HitInfoDetails _hitInfo, Vector3 _entityPos)
	{
		if (_mode != EnumRotationMode.Auto)
		{
			return base.OnPlaceBlock(_placement, _mode, _localRot, _world, _blockValue, _propTransform, _hitInfo, _entityPos);
		}
		Result result = new Result(_placement, _blockValue, _propTransform, _hitInfo);
		result.blockValue.rotation = 0;
		return result;
	}
}
