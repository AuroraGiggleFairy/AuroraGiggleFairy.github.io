using UnityEngine.Scripting;

[Preserve]
public class BlockTrunkTip : BlockDamage
{
	public override bool RotateVerticesOnCollisionCheck(BlockValue _blockValue)
	{
		return false;
	}
}
