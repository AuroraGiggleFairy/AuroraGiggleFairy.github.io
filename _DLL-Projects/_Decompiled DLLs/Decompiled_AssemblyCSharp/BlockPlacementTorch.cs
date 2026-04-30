using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlacementTorch : BlockPlacement
{
	public override Result OnPlaceBlock(EnumRotationMode _mode, int _localRot, WorldBase _world, BlockValue _blockValue, HitInfoDetails _hitInfo, Vector3 _entityPos)
	{
		if (_mode != EnumRotationMode.Auto)
		{
			return base.OnPlaceBlock(_mode, _localRot, _world, _blockValue, _hitInfo, _entityPos);
		}
		Result result = new Result(_blockValue, _hitInfo);
		result.blockValue.meta = 0;
		switch (_hitInfo.blockFace)
		{
		case BlockFace.East:
			result.blockValue.rotation = 1;
			break;
		case BlockFace.West:
			result.blockValue.rotation = 3;
			break;
		case BlockFace.North:
			result.blockValue.rotation = 0;
			break;
		case BlockFace.South:
			result.blockValue.rotation = 2;
			break;
		}
		return result;
	}
}
