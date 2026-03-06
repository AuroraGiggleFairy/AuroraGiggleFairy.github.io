using UnityEngine.Scripting;

[Preserve]
public class BlockStairs : Block
{
	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFace _face)
	{
		if (_blockValue.ischild)
		{
			return false;
		}
		return true;
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		if (_blockValue.ischild)
		{
			return false;
		}
		return true;
	}
}
