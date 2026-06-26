using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlacementPineLeaves : BlockPlacement
{
	public override Result OnPlaceBlock(EnumRotationMode _mode, int _localRot, WorldBase _world, BlockValue _blockValue, HitInfoDetails _hitInfo, Vector3 _entityPos)
	{
		if (_mode != EnumRotationMode.Auto)
		{
			return base.OnPlaceBlock(_mode, _localRot, _world, _blockValue, _hitInfo, _entityPos);
		}
		Result result = new Result(_blockValue, _hitInfo);
		result.blockValue.rotation = 0;
		return result;
	}
}
