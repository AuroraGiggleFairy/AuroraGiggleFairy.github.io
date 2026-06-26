using UnityEngine.Scripting;

[Preserve]
public class BlockPlacementDrawBridge : BlockPlacementTowardsPlacer
{
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
